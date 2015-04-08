using System;
using System.IO;
using System.Net.Sockets;
using libOneRNG;

namespace rnd
{
    public delegate void UserDisconnectedHandler(User sender);

    /// <summary>
    /// This represents a user (Connection)
    /// </summary>
	public class User : IDisposable
	{
        public event UserDisconnectedHandler UserDisconnected;

        private NetworkStream NS;

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="S">Open Socket</param>
        public User(Socket S)
        {
            UserDisconnected += new UserDisconnectedHandler(User_UserDisconnected);
            NS = new NetworkStream(S, true);
        }

        private void User_UserDisconnected(User sender)
        {
            //NOP
        }

        /// <summary>
        /// Closes and releases resources.
        /// </summary>
        public void Close()
        {
            if (NS != null)
            {
                lock (this)
                {
                    NS.Close();
                    NS.Dispose();
                    NS = null;
                }
            }
        }

        /// <summary>
        /// Sends data over the socket
        /// </summary>
        /// <param name="Data">Bytes to send</param>
        /// <returns>true, if successfully sent</returns>
        public bool Send(byte[] Data)
        {
            if (NS != null)
            {
                try
                {
                    //Write can lock up when the remote buffer is full
                    //or even throw exceptions when the remote end throws in a TCP RST packet.
                    NS.Write(Data, 0, Data.Length);
                }
                catch
                {
                    UserDisconnected(this);
                    return false;
                }
                return true;
            }
            return false;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
