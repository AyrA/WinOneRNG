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
                @"C:\Users\AyrA\Desktop\diehard\_test.rnd"
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
                        T.Start();
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

        private static void help()
        {
            Console.WriteLine("rndTest <Filename>");
        }
    }
}
