using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace rndTest
{
    public class Test
    {
        public class TestResult
        {
            public double chisquare = 0.0;
            public double mean=0.0;
            public double montepicalc = 0.0;
            public double scc = 0.0;
            public double entropy = 0.0;
            public long[] CharCount=new long[256];
        }

        private const int MONTEN = 6;
        private const int BUFSIZE = 1024 * 1024 * 5;

        private Stream S;

        //monte pi calculation
        private long mp;
        private long mcount;
        private long inmont;
        private double incirc;
        private byte[] monte;
        private double sccu0, scclast, scct1, scct2, scct3;

        private bool sccfirst = true;

        public Test(Stream Input)
        {
            S = Input;
        }

        public TestResult Start()
        {
            TestResult R = new TestResult();
            double[] Probability=new double[256];

            //read a big chunk
            byte[] buffer = new byte[BUFSIZE];
            byte[] send = new byte[BUFSIZE];

            double datasum = 0.0;

            int i;

            //number of chars 
            long totalc = 0;

            double cexp = 0.0;

            //initialize values again
            mp = mcount = inmont = 0;
            incirc = 65535.0 * 65535.0;
            monte = new byte[MONTEN];
            sccu0 = scclast = scct1 = scct2 = scct3 = 0.0;

            incirc = Math.Pow(Math.Pow(256.0, (double)(MONTEN / 2.0)) - 1.0, 2.0);

            for (i = 0; i < R.CharCount.Length; R.CharCount[i++] = 0) ;
            totalc = 0;
            i = S.Read(buffer, 0, BUFSIZE);
            Console.Error.WriteLine("Press [ESC] to cancel");
            do
            {
                Console.Error.Write("{0} MB readed of {1} MB ({2:0.00}%)", totalc / 1024 / 1024, S.Length / 1024 / 1024, S.Position / (double)S.Length * 100.0);
                Console.CursorLeft = 0;
                totalc += i;
                if (i > 0)
                {
                    Buffer.BlockCopy(buffer, 0, send, 0, i);
                    Thread T = new Thread(tMonte);
                    T.Start(new object[] { send, i, R });
                    for (int j = 0; j < i; ++j)
                    {
                        ++R.CharCount[buffer[j]];
                    }
                    i = S.Read(buffer, 0, BUFSIZE);
                    T.Join();

                }
            } while (i > 0 && !(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape));

            //Complete calculation of serial correlation coefficient
            scct1 = scclast * sccu0 + scct1;
            scct2 *= scct2;
            R.scc = totalc * scct3 - scct2;
            if (R.scc == 0.0)
            {
                R.scc = -100000;
            }
            else
            {
                R.scc = (totalc * scct1 - scct2) / R.scc;
            }

            //Scan bins and calculate probability for each bin and
            //Chi-Square distribution.  The probability will be reused
            //in the entropy calculation below.  While we're at it,
            //we sum of all the data which will be used to compute the mean.

            cexp = totalc / 256.0;  //Expected count per bin
            for (i = 0; i < 256; ++i)
            {
                double a = R.CharCount[i] - cexp;
                Probability[i] = R.CharCount[i] / (double)totalc;
                R.chisquare += (a * a) / cexp;
                datasum += i * (double)R.CharCount[i];
            }

            //Calculate entropy

            for (i = 0; i < 256; ++i)
            {
                if (Probability[i] > 0.0)
                {
                    R.entropy += Probability[i] * Math.Log(1.0 / Probability[i], 2.0);
                }
            }

            //Calculate Monte Carlo value for PI from percentage of hits within the circle

            R.montepicalc = 4.0 * (inmont / (double)mcount);

            //Return results
            R.mean = datasum / (double)totalc;
            return R;
        }

        private void tMonte(object o)
        {
            int montex, montey;
            object[] args = (object[])o;
            byte[] buffer = (byte[])args[0];
            int count = (int)args[1];
            TestResult R = (TestResult)args[2];

            for (int j = 0; j < count; ++j)
            {
                monte[mp++] = buffer[j];
                if (mp >= MONTEN)
                {
                    mp = 0;
                    ++mcount;
                    montex = montey = 0;
                    for (int mj = 0; mj < MONTEN / 2; ++mj)
                    {
                        montex = montex * 256 + monte[mj];
                        montey = montey * 256 + monte[MONTEN / 2 + mj];
                    }
                    if ((Math.Pow(montex, 2) + Math.Pow(montey, 2)) <= incirc)
                    {
                        ++inmont;
                    }
                }
                if (!sccfirst)
                {
                    scct1 += buffer[j] * scclast;
                }
                else
                {
                    sccfirst = false;
                    sccu0 = buffer[j];
                }
                scct2 += buffer[j];
                scct3 += Math.Pow(buffer[j], 2);
                scclast = buffer[j];
            }
        }
    }
}
