using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace rndTest
{
    public class Test
    {
        /**r_ent = ent;
            *r_chisq = chisq;
            *r_mean = datasum / totalc;
            *r_montepicalc = montepi;
            *r_scc = scc;*/

        public class TestResult
        {
            public double chisquare = 0.0;
            public double mean=0.0;
            public double montepicalc = 0.0;
            public double scc = 0.0;
            public double entropy = 0.0;
            public long[] CharCount=new long[256];
        }


        private const double log2of10 = 3.32192809488736234787;
        private const int MONTEN = 6;
        private const int BUFSIZE = 1024 * 1024 * 10;

        private Stream S;

        public Test(Stream Input)
        {
            S = Input;
        }

        private double log2(double x)
        {
            return Math.Log10(x) * log2of10;
        }

        public TestResult Start()
        {
            TestResult R = new TestResult();
            double[] Probability=new double[256];

            byte[] monte=new byte[MONTEN];

            //read 10 MB at a time
            byte[] buffer = new byte[BUFSIZE];

            //counters
            int i, j, mj;

            double datasum = 0.0;

            //number of chars 
            long totalc = 0;

            //monte pi calculation
            long mp = 0;
            long mcount = 0;
            long inmont = 0;
            double incirc = 65535.0 * 65535.0;
            
            double cexp = 0.0;

            bool sccfirst = true;

            double sccu0, scclast, scct1, scct2, scct3, montex, montey;
            sccu0 = scclast = scct1 = scct2 = scct3 = montex = montey = 0.0;

            incirc = Math.Pow(Math.Pow(256.0, (double)(MONTEN / 2.0)) - 1.0, 2.0);

            for (i = 0; i < R.CharCount.Length; R.CharCount[i++] = 0) ;
            totalc = 0;

            do
            {
                i = S.Read(buffer, 0, buffer.Length);
                Console.Error.Write("{0} MB readed", totalc / 1024 / 1024);
                Console.CursorLeft = 0;
                for (j = 0; j < i; ++j)
                {
                    ++R.CharCount[buffer[j]];
                    ++totalc;
                    monte[mp++] = buffer[j];
                    if (mp >= MONTEN)
                    {
                        mp = 0;
                        ++mcount;
                        montex = montey = 0;
                        for (mj = 0; mj < MONTEN / 2; ++mj)
                        {
                            montex = (montex * 256.0) + monte[mj];
                            montey = (montey * 256.0) + monte[(MONTEN / 2) + mj];
                        }
                        if ((montex * montex + montey * montey) <= incirc)
                        {
                            ++inmont;
                        }
                    }
                    if (sccfirst)
                    {
                        sccfirst = false;
                        sccu0 = buffer[j];
                    }
                    else
                    {
                        scct1 += scclast * (double)buffer[j];
                    }
                    scct2 += buffer[j];
                    scct3 += buffer[j] * buffer[j];
                    scclast = buffer[j];
                }
            } while (i > 0 && !Console.KeyAvailable);

            /* Complete calculation of serial correlation coefficient */

            scct1 = scct1 + scclast * sccu0;
            scct2 = scct2 * scct2;
            R.scc = totalc * scct3 - scct2;
            if (R.scc == 0.0)
            {
                R.scc = -100000;
            }
            else
            {
                R.scc = (totalc * scct1 - scct2) / R.scc;
            }

            /* Scan bins and calculate probability for each bin and
            Chi-Square distribution.  The probability will be reused
            in the entropy calculation below.  While we're at it,
            we sum of all the data which will be used to compute the
            mean. */

            cexp = (double)totalc / 256.0;  /* Expected count per bin */
            for (i = 0; i < 256; ++i)
            {
                double a = (double)R.CharCount[i] - cexp; ;
                Probability[i] = ((double)R.CharCount[i]) / totalc;
                R.chisquare += (a * a) / cexp;
                datasum += ((double)i) * R.CharCount[i];
            }

            /* Calculate entropy */

            for (i = 0; i < 256; ++i)
            {
                if (Probability[i] > 0.0)
                {
                    R.entropy += Probability[i] * log2(1.0/Probability[i]);
                }
            }

            /* Calculate Monte Carlo value for PI from percentage of hits within the circle */

            R.montepicalc = 4.0 * (((double)inmont) / (double)mcount);

            /* Return results */
            R.mean = datasum / (double)totalc;

            return R;
        }

        private void tMonte(object o)
        {
            object[] args = (object[])o;
            byte[] buffer = (byte[])args[0];
            int count = (int)args[1];
            TestResult R = (TestResult)args[2];
            for (int j = 0; j < i; ++j)
            {
                ++R.CharCount[buffer[j]];
                ++totalc;
                monte[mp++] = buffer[j];
                if (mp >= MONTEN)
                {
                    mp = 0;
                    ++mcount;
                    montex = montey = 0;
                    for (mj = 0; mj < MONTEN / 2; ++mj)
                    {
                        montex = (montex * 256.0) + monte[mj];
                        montey = (montey * 256.0) + monte[(MONTEN / 2) + mj];
                    }
                    if ((montex * montex + montey * montey) <= incirc)
                    {
                        ++inmont;
                    }
                }
                if (sccfirst)
                {
                    sccfirst = false;
                    sccu0 = buffer[j];
                }
                else
                {
                    scct1 += scclast * (double)buffer[j];
                }
                scct2 += buffer[j];
                scct3 += buffer[j] * buffer[j];
                scclast = buffer[j];
            }
        }

        private void tChi(object o)
        {
            object[] args = (object[])o;
            byte[] buffer = (byte[])args[0];
            int count = (int)args[1];
            TestResult R = (TestResult)args[2];
        }
    }
}
