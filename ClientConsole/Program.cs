using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientConsole
{
    class Program
    {
        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }

        public delegate void CommunicationDelegate(TcpClient tcpClient);
        public static void StartClient()
        {

            try
            {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2048);

                TcpClient tcpClient = new TcpClient();

                try
                {
                    tcpClient.Connect(remoteEP);
                    Console.WriteLine("Connected chyba");
                    //NetworkStream networkStream = tcpClient.GetStream();

                    CommunicationDelegate receiveDelegate = new CommunicationDelegate(Receive);
                    CommunicationDelegate sendDelegate = new CommunicationDelegate(Send);

                    Parallel.Invoke(
                        () => receiveDelegate.Invoke(tcpClient),
                        () => sendDelegate.Invoke(tcpClient)
                    );



                    // Receive the response from the remote device.    

                    // Release the socket.    
                    /*sender.Shutdown(SocketShutdown.Both);
                    sender.Close();*/

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Receive(TcpClient tcpClient)
        {
            while (true)
            {

                byte[] buffer = new byte[128];
                NetworkStream networkStream = tcpClient.GetStream();
                while (networkStream.DataAvailable)
                {
                    networkStream.Read(buffer, 0, buffer.Length);
                    string msg = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);

                    if (msg == "exit")
                    {
                        tcpClient.Close();
                        return;
                    }
                    else
                        Console.WriteLine(msg);
                }
            }

        }

        public static void Send(TcpClient tcpClient)
        {
            // TODO: disconnect
            while (true)
            {
                string input = Console.ReadLine();
                byte[] myWriteBuffer = Encoding.ASCII.GetBytes(input);
                tcpClient.GetStream().Write(myWriteBuffer, 0, myWriteBuffer.Length);
            }
        }
    }
}
