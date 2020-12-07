using ClassLibrary;
using System;
using System.Net;

namespace TCPServer
{
    /// <summary>
    /// Class Program containing TCP server's Main function
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main function launching server
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Server server = new ServerAsync("127.0.0.1", 2048);
            server.Start();
        }
    }
}
