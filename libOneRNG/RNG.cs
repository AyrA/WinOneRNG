using System.IO.Ports;
using System;
using System.IO;

namespace libOneRNG
{
    /// <summary>
    /// Provides an Interface to the OneRNG
    /// </summary>
    public class RNG : IDisposable
    {
        private const int SPEED = 9600;

        /// <summary>
        /// Possible RNG Modes to activate
        /// </summary>
        public enum RngModes : byte
        {
            AvalancheWithWhitener = 0,
            RawAvalanche = 1,
            AvalancheWithRfWithWhitener = 2,
            AvalancheWithRf = 3,
            NoNoise1 = 4,
            NoNoise2 = 5,
            RfWithWhitener = 6,
            RawRf = 7
        }

        private SerialPort SP;

        /// <summary>
        /// Available Serial Ports
        /// </summary>
        public static string[] Ports
        {
            get
            {
                return SerialPort.GetPortNames();
            }
        }

        public static bool PortExists(string Port)
        {
            if (string.IsNullOrEmpty(Port))
            {
                return false;
            }
            foreach (string s in Ports)
            {
                if (s.ToUpper() == Port.ToUpper())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Initializes a new OneRNG
        /// </summary>
        /// <param name="Port">Serial Port</param>
        public RNG(string Port)
        {
            SP = new SerialPort(Port, SPEED , Parity.None, 8, StopBits.One);
            SP.Handshake = Handshake.RequestToSend;
            //this throws an exception if the device no longer accepts commands
            SP.WriteTimeout = 1000;
            //this allows the serial port up to 5 seconds to fulfill any Read() request
            //to its specified size. If this timeout is reached, only the already read data
            //is returned
            SP.ReadTimeout = 5000;
            SP.Open();
        }

        /// <summary>
        /// Flushes the Serial Buffer
        /// </summary>
        public void Flush()
        {
            SP.Write("cmdW");
        }

        /// <summary>
        /// Starts RNG Generation
        /// </summary>
        public void Start()
        {
            SP.DtrEnable = true;
            SP.Write("cmdO");
        }

        /// <summary>
        /// Stops RNG Generation
        /// </summary>
        public void Stop()
        {
            SP.Write("cmdo");
            SP.DtrEnable = false;
        }

        /// <summary>
        /// Sets a specific RNG Mode
        /// </summary>
        /// <param name="Mode">RNG Mode to activate</param>
        public void SetMode(RngModes Mode)
        {
            SP.Write(new char[] { 'c', 'm', 'd', Mode.ToString()[0] }, 0, 4);
        }

        /// <summary>
        /// Reads a specific number of bytes from the entropy pool
        /// </summary>
        /// <param name="Count">Number of bytes</param>
        /// <returns>Random Data</returns>
        public byte[] Read(int Count)
        {
            byte[] b=new byte[Count];
            int Read = 0;
            //Locking a common object is important here!
            //Streams are sometimes not thread safe.
            //If this call is made simultaneously we want to make sure,
            //that two calls will not reveice the same data.
            //By using lock we let C# handle the rest.
            //The reads will take a while, so if we have many threads,
            //they might start to accumulate here. This is actually not bad.
            //C# will process the calls in the order they appear here. It's
            //like a transparently handles Queue. This gives each thread an
            //equal chance to get random data but at the same time will not
            //hinder other threads if one has a long time to process data,
            //as the lock is only around the single problematic call and does
            //not wraps the whole function body
            lock (SP)
            {
                Read = SP.BaseStream.Read(b, 0, Count);
            }
            //If the serial buffer is too small and not all bytes could be read,
            //shrink the array, otherwise the end is still 0-filled and will mess up
            //your entropy. This should not happen due to a large ReadTimeout value
            if (Read < Count)
            {
                Array.Resize<byte>(ref b, Read);
            }
            return b;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
            SP.Close();
            SP.Dispose();
        }

        #endregion
    }
}
