using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace concat
{
    /// <summary>
    /// A simple high performance file concat utility.
    /// Works best if source and destination are on the same disk.
    /// Uses 512 MB of ram, but you can customize it using the MAXLEN constant
    /// </summary>
    class Program
    {
        /// <summary>
        /// Use 512 MB RAM for copy.
        /// Feel free to change. Higher=faster but more RAM usage.
        /// Can be any arbitrary number.
        /// </summary>
        private const int MAXLEN=1024*1024*512;

        static void Main(string[] args)
        {
#if DEBUG
            args = new string[]
            {
                @"C:\Users\AyrA\Desktop\rnd\rnd_*.bin",
                @"C:\Users\AyrA\Desktop\rnd\_test.rnd"
            };
            long start = DateTime.Now.Ticks;
#endif
            if (args.Length > 1)
            {
                List<string> Source = new List<string>();
                string Dest=args[args.Length-1];
                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (args[i].Contains("?") || args[i].Contains("*"))
                    {
                        int len = Source.Count;
                        Source.AddRange(MaskMatch.Match(args[i], MatchType.File));
                        if (len == Source.Count)
                        {
                            Console.Error.WriteLine("Mask yielded 0 rresults: {0}", args[i]);
                            return;
                        }
                    }
                    else
                    {
                        if (File.Exists(args[i]))
                        {
                            Source.Add(args[i]);
                        }
                        else
                        {
                            Console.Error.WriteLine("Input file not found: {0}", args[i]);
                            return;
                        }
                    }
                }
                if (File.Exists(Dest))
                {
                    File.Delete(Dest);
                }
                foreach (string F in Source)
                {
                    if (!File.Exists(F))
                    {
                        Console.Error.WriteLine("Input file not found: {0}", F);
                    }
                }
                using (FileStream FS = File.Create(Dest))
                {
                    MemoryStream MS = new MemoryStream();
                    foreach (string F in Source)
                    {
                        Console.Error.WriteLine("{0} [READ]", F);
                        byte[] B = File.ReadAllBytes(F);

                        //if Position==0, do not copy but wait one more turn
                        if (MS.Position + B.Length > MAXLEN && MS.Position > 0)
                        {
                            Console.Error.WriteLine("Flushing buffer [WRITE]");
                            FS.Write(MS.ToArray(), 0, (int)MS.Position);
                            MS.Dispose();
                            MS = new MemoryStream();
                        }
                        MS.Write(B, 0, B.Length);
                    }
                    Console.Error.WriteLine("Flushing buffer [WRITE]");
                    FS.Write(MS.ToArray(), 0, (int)MS.Position);
                    MS.Dispose();
                    Console.Error.WriteLine("[DONE]");
                }
            }
#if DEBUG
            long end = DateTime.Now.Ticks;
            Console.Error.WriteLine("Execution time: {0:0.00} Seconds", new TimeSpan(end - start).TotalSeconds);
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
        }
    }
}
