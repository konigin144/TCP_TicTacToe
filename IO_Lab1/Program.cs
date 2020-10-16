using ClassLibrary;
using System;

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
            Server server = new Server();
            server.Run();
        }
    }
}
