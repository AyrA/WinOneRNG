using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace rndTest
{
    class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            args = new string[]
            {
                @"C:\Temp\RNG\ent.rnd"
            };
#endif
            if (args.Length > 0 && args[0] != "/?" && args[0] != "-?" && args[0] != "--help")
            {
                if (args[0].Contains("?") || args[0].Contains("*"))
                {
                    Console.WriteLine("File masks are not supported");
                    return 1;
                }
                if (File.Exists(args[0]))
                {
                    using(FileStream FS=File.OpenRead(args[0]))
                    {
                        Test T = new Test(FS);
                        Test.TestResult R = T.Start();
                        Console.WriteLine(@"Test results:
Entropy : {0} (8 = best)
Mean    : {1} (127.5 = best)

PI value: {2} (real PI = best)
real PI : {3}", R.entropy, R.mean, R.montepicalc, Math.PI);
                        flush();
                        Console.ReadKey(true);
                    }
                }
                else
                {
                    Console.WriteLine("File not found: {0}", args[0]);
                    return 2;
                }
            }
            else
            {
                help();
                return 255;
            }
            return 0;
        }

        private static void flush()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }

        private static void help()
        {
            Console.WriteLine("rndTest <Filename>");
        }
    }
}
