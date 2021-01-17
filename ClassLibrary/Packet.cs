using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    /// <summary>
    /// Static class handling server-client communication
    /// </summary>
    static public class Packet
    {
        /// <summary>
        /// Sends message
        /// </summary>
        /// <param name="networkStream">Sender's network stream</param>
        /// <param name="message">Sessage</param>
        public static void Send(NetworkStream networkStream, string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            try
            {
                networkStream.Write(buffer, 0, buffer.Length);
            } catch (Exception e)
            {
                Console.WriteLine("Connection lost: Packet.Send");
            }
        }

        /// <summary>
        /// Reads message
        /// </summary>
        /// <param name="networkStream">Receiver's network stream</param>
        /// <param name="bufferSize">Size of buffer (default 512)</param>
        /// <returns></returns>
        public static string Read(NetworkStream networkStream, int bufferSize = 512)
        {
            byte[] buffer = new byte[bufferSize];
            string message = "";
            try
            {
                networkStream.Read(buffer, 0, buffer.Length);
            
                message = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection lost: Packet.Read");

            }
            /*if (message == "")
                throw new Exception();*/
            return message;
        }

        /// <summary>
        /// Reads message asynchronous
        /// </summary>
        /// <param name="networkStream">Receiver's network stream</param>
        /// <param name="bufferSize">Size of buffer (default 512)</param>
        /// <returns></returns>
        public static async Task<string> ReadAsync(NetworkStream networkStream, int bufferSize = 512)
        {
            byte[] buffer = new byte[bufferSize];
            try
            {
                await networkStream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection lost: Packet.ReadAsync");
            }
            string message = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
            return message;
        }
    }
}
