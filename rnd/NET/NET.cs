using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace rnd
{
    public delegate void NewConnectionHandler(TcpServer sender, IPAddress RemoteAddr, ConnectionEventArgs e);
    public delegate void NewUserHandler(TcpServer sender, User u);

    /// <summary>
    /// A simple TCP server
    /// </summary>
    public class TcpServer : IDisposable
    {
        /// <summary>
        /// Incomming connection. You can cancel here to abort it
        /// </summary>
        public event NewConnectionHandler NewConnection;
        /// <summary>
        /// New user, fired after NewConnection has not been cancelled
        /// </summary>
        public event NewUserHandler NewUser;

        private TcpListener Srv;
        private IAsyncResult CurrentAsync;

        /// <summary>
        /// Creates a new TCP Server
        /// </summary>
        /// <param name="IP">IP to listen on (use 0.0.0.0 for all sockets)</param>
        /// <param name="Port">Port to listen on</param>
        public TcpServer(string IP, int Port)
        {
            Srv = new TcpListener(new IPEndPoint(IPAddress.Parse(IP), Port));
            NewConnection += new NewConnectionHandler(NET_NewConnection);
            NewUser += new NewUserHandler(NET_NewUser);
            CurrentAsync = null;
        }

        /// <summary>
        /// Starts the listener
        /// </summary>
        public void Start()
        {
            if (CurrentAsync == null)
            {
                Srv.Start();
                CurrentAsync = Srv.BeginAcceptSocket(conIn, null);
            }
        }

        /// <summary>
        /// Stops the listener
        /// </summary>
        public void Stop()
        {
            if (CurrentAsync != null)
            {
                Srv.Stop();
                //Srv.EndAcceptSocket(CurrentAsync);
                CurrentAsync = null;
            }
        }

        private void NET_NewUser(TcpServer sender, User u)
        {
            //NOOP
        }

        private void NET_NewConnection(TcpServer sender, IPAddress RemoteAddr, ConnectionEventArgs e)
        {
            //NOOP
        }

        private void conIn(IAsyncResult ar)
        {
            Socket S;
            try
            {
                S = Srv.EndAcceptSocket(ar);
            }
            catch
            {
                S = null;
            }
            //The trick is to listen for connections again,
            //before you process the Socket, so if it locks up,
            //it will not affect the server.
            if (CurrentAsync != null)
            {
                CurrentAsync = Srv.BeginAcceptSocket(conIn, null);
            }
            if (S != null)
            {
                ConnectionEventArgs CEA = new ConnectionEventArgs();
                NewConnection(this, ((IPEndPoint)S.RemoteEndPoint).Address, CEA);
                if (CEA.Cancel)
                {
                    //Calling shutdown will not throw exceptions when
                    //the other party has already disconnected.
                    //Calling Disconnect on the other hand might.
                    S.Shutdown(SocketShutdown.Both);
                    S.Close();
                }
                else
                {
                    NewUser(this, new User(S));
                }
            }
        }
    
        #region IDisposable Members

        public void  Dispose()
        {
            Stop();
        }

        #endregion
        }
}
