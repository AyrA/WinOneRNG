using System;
using System.Collections.Generic;
using libOneRNG;
using System.IO;
using System.Threading;
using AyrA.IO;
using System.Diagnostics;

namespace rnd
{
    class Program
    {
        private static RNG R;
        private static bool KeepLoop;

        /// <summary>
        /// Represents INI file settings
        /// </summary>
        private class Settings
        {
            /// <summary>
            /// IP address, the TCP server listens on
            /// </summary>
            public string IP
            { get; set; }
            /// <summary>
            /// Port the TCP server listens on
            /// </summary>
            public int Port
            { get; set; }
            /// <summary>
            /// the name of the serial Port
            /// </summary>
            public string SerialPort
            { get; set; }
            /// <summary>
            /// The block size for each client
            /// </summary>
            public int BlockSize
            { get; set; }

            public Settings()
            {
                ApplyDefaults();
            }

            public void ApplyDefaults()
            {
                if (string.IsNullOrEmpty(IP))
                {
                    IP = "127.0.0.1";
                }
                if (string.IsNullOrEmpty(SerialPort))
                {
                    SerialPort = "COM1";
                }
                if (Port < 1)
                {
                    Port = 0xBEEF;
                }
                if (BlockSize < 1)
                {
                    BlockSize = 1024;
                }
            }
        }

        /// <summary>
        /// Gets the configuration file
        /// </summary>
        private static string Config
        {
            get
            {
                return Path.Combine(new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Directory.FullName, "config.ini");
            }
        }

        private static Settings S;

        public static void Main(string[] args)
        {
            //get settings
            S = new Settings();
            //The IP address can either be IPv4 or IPv6
            S.IP = INI.getSetting(Config, "NET", "IP");
            //The port must be in the range of ushort
            S.Port = INI.getInt(Config, "NET", "Port", 0);
            //The block size for clients. If you expect a high number of clients
            //reduce this number.
            S.BlockSize = INI.getInt(Config, "NET", "BlockSize", 0);
            //The serial port of OneRNG
            S.SerialPort = INI.getSetting(Config, "SerialPort", "PortName");
            S.ApplyDefaults();

            //init RNG
            Console.Write("Starting RNG...");
            R = new RNG(S.SerialPort);
            R.SetMode(RNG.RngModes.AvalancheWithWhitener);
            R.Start();
            Console.WriteLine("OK");
            
            //init server
            Console.Write("Starting TCP Server...");
            KeepLoop = true;
            TcpServer Srv = new TcpServer(S.IP, S.Port);
            //Bind to the Srv.NewConnection event here if you wish.
            Srv.NewUser += new NewUserHandler(Srv_NewUser);
            Srv.Start();
            Console.WriteLine("OK, Server started on {0}:{1}", S.IP, S.Port);

            //Waits for quit command (you can also press Ctrl+C for a less graceful shutdown)
            while (Console.ReadLine().ToLower() != "quit") ;

            //end all user threads gracefully
            KeepLoop = false;
            
            //exit server and RNG
            R.Dispose();
            Srv.Dispose();

            //save current settings, if we implement a more advanced CLI it might become handy
            INI.Save(Config, "NET", "IP", S.IP, false);
            INI.Save(Config, "NET", "Port", S.Port.ToString(), false);
            INI.Save(Config, "NET", "BlockSize", S.BlockSize.ToString(), false);
            INI.Save(Config, "SerialPort", "PortName", S.SerialPort, false);

            Console.WriteLine("Server closed");
        }

        private static void Loop(object o)
        {
            bool connected = true;
            User u = (User)o;
            //Yes, C# has JavaScript like delegates.
            u.UserDisconnected += delegate(User sender) { connected = false; };
            while (KeepLoop && connected)
            {
                //see the RNG.Read, why this is a good idea
                connected &= u.Send(R.Read(S.BlockSize));
            }
            Console.WriteLine("User disconnected");
            u.Dispose();
        }

        private static void Srv_NewUser(TcpServer sender, User u)
        {
            //keep each user in it's own thread,
            //so if a socket get's locked up it will not affect others.
            Thread t = new Thread(Loop);
            t.IsBackground = true;
            t.Start(u);
        }
    }
}
