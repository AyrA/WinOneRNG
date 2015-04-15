using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace fileGen
{
    class Program
    {
        private class Arguments
        {
            public string Port = null;
            public string Directory = null;
            public string Mask = null;
            public int FileSize = 0;
            public int NumFiles = 0;
            public Exception ex = null;

            public Arguments()
            {
            }
        }

        static int Main(string[] args)
        {
            long total = 0;
#if DEBUG
            args = new string[]
            {
                "COM3","/S","1","/N","2", "/M","test_!_yay.bin","/D",@"R:\RNG"
            };
#endif
            if (args.Length > 0)
            {
                Arguments A = parseArgs(args);
                if (A.ex == null)
                {
                    Stream Output = null;
                    FileWriter FW = null;
                    if (A.Mask == "-")
                    {
                        Output = Console.OpenStandardOutput();
                    }
                    else
                    {
                        FW = new FileWriter(A.Directory, A.Mask, A.FileSize * 1024 * 1024, A.NumFiles);
                    }
                    Console.Clear();
                    libOneRNG.RNG R = new libOneRNG.RNG(A.Port);
                    R.Start();
                    while (!HasKey(ConsoleKey.Escape))
                    {
                        byte[] b = R.Read(1024);
                        total += b.Length;
                        Console.SetCursorPosition(0, 0);
                        Console.Error.WriteLine("Processed: {0}", nice(total));
                        if (Output != null)
                        {
                            Output.Write(b, 0, b.Length);
                        }
                        else
                        {
                            Console.Error.WriteLine("File: {0}/{1} ({2,6:0.00}%)", FW.CurrentNumber, FW.Count, perc(FW.Count, FW.CurrentNumber));
                            Console.Error.WriteLine("Size: {0} ({1,6:0.00}%)", nice(FW.CurrentPosition), perc(FW.Size, FW.CurrentPosition));
                            if (!FW.Write(b))
                            {
                                FW.Dispose();
                                return 0;
                            }
                        }
                        Console.Error.WriteLine("Press [ESC] to abort");
                    }
                    R.Stop();
                    R.Dispose();
                    Console.Error.WriteLine("Operation aborted by user input");
                    return 2;
                }
                else
                {
                    Console.Error.WriteLine("Error parsing arguments: {0}",A.ex.Message);
                    return 3;
                }
            }
            else
            {
                Help();
                return 1;
            }
        }

        private static double perc(double Max, double Current)
        {
            if (Max == 0.0)
            {
                return 0.0;
            }
            return Current / Max * 100.0;
        }

        private static string nice(long l)
        {
            string[] Sizes = new string[]
            {
                "B",
                "KB",
                "MB",
                "GB",
                "TB"
            };
            double d = l < 0 ? 0 : l;
            int i = 0;
            while (d >= 1024.0 && i<Sizes.Length)
            {
                d /= 1024.0;
                i++;
            }
            return string.Format("{0,7:0.00} {1,2}", d, Sizes[i]);
            
        }

        private static bool HasKey(ConsoleKey K)
        {
            while (Console.KeyAvailable)
            {
                if (Console.ReadKey(true).Key == K)
                {
                    return true;
                }
            }
            return false;
        }

        private static ConsoleKeyInfo WaitForKey()
        {
            EmptyBuffer();
            return Console.ReadKey(true);
        }

        private static void EmptyBuffer()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }

        private static Arguments parseArgs(string[] Args)
        {
            Arguments A = new Arguments();

            A.Directory = Environment.CurrentDirectory;
            A.FileSize = 10;
            A.Mask = "!.bin";
            A.NumFiles = 1;
            A.Port = string.Empty;
            A.ex = null;


            int current = 0;
            while (current < Args.Length && A.ex==null)
            {
                string arg = Args[current++];
                if (!arg.StartsWith("/"))
                {
                    if (string.IsNullOrEmpty(A.Port))
                    {
                        A.Port = arg;
                    }
                    else
                    {
                        A.ex = new Exception("Unknown argument: " + arg);
                    }
                }
                else
                {
                    int temp = -1;
                    if (current < Args.Length)
                    {
                        switch (arg.ToUpper())
                        {
                            case "/M":
                                A.Mask = Args[current++];
                                break;
                            case "/S":
                                if (int.TryParse(Args[current++], out temp))
                                {
                                    A.FileSize = temp;
                                }
                                else
                                {
                                    A.ex = new Exception("Invalid file size (/S) specified");
                                }
                                break;
                            case "/N":
                                if (int.TryParse(Args[current++], out temp) && temp >= 0)
                                {
                                    A.NumFiles = temp;
                                }
                                else
                                {
                                    A.ex = new Exception("Invalid file count (/N) specified");
                                }
                                break;
                            case "/D":
                                A.Directory = Args[current++];
                                break;
                            default:
                                A.ex = new Exception("Unknown argument: " + arg);
                                break;
                        }
                    }
                    else
                    {
                        A.ex = new Exception("Missing parameter for Argument " + arg);
                    }
                }
            }
            if (string.IsNullOrEmpty(A.Port))
            {
                A.ex = new Exception("No port specified");
            }
            else
            {
                if (!libOneRNG.RNG.PortExists(A.Port))
                {
                    A.ex = new Exception("Invalid port specified: " + A.Port);
                }
            }
            return A;
        }

        private static void Help()
        {
            Console.Error.WriteLine(@"
fileGen.exe <Port> [/D Directory] [/M Mask] [/S FileSize] [/N NumFiles]

Port   Serial Port to use. See below for the list of available ports.

/D     Directory to save files into. Defaults to current directory.

/M     File name mask. Use an exclamation mark to specify, where the number
       is put. Defaults to '!.bin'. If Mask is a single dash (-),
       then output is sent to stdout. This enforces /S to be non-zero.
       If the mask does not contains a '!', then /N is ignored

/S     File size in Megabytes. Defaults to 10. (0 = Infinite)

/N     Number of files. Defaults to 1. (0 = Infinite). Ignored if file size
       is also infinite.

List of ports:");
            foreach (string s in libOneRNG.RNG.Ports)
            {
                Console.WriteLine(s);
            }

        }
    }
}
