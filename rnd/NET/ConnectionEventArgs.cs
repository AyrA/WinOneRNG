using System;
using System.Collections.Generic;
using System.Text;

namespace rnd
{
    /// <summary>
    /// allows cancellation of connections, before user object is created
    /// </summary>
    public class ConnectionEventArgs
    {
        /// <summary>
        /// Gets or sets, if the connection should be aborted.
        /// </summary>
        public bool Cancel
        { get; set; }

        public ConnectionEventArgs()
        {
            Cancel = false;
        }
    }
}
