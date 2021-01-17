using System.Net;
using System.Net.Sockets;

namespace ClientForms
{
    /// <summary>
    /// Class containing client's info
    /// </summary>
    public class Client
    {
        public TcpClient tcpClient;
        public NetworkStream networkStream;
        public string username;
        public string enemyUsername;
        public int playerID;

        public Client(TcpClient tcpClient, IPEndPoint remoteEP)
        {
            this.tcpClient = tcpClient;
            this.tcpClient.Connect(remoteEP);
            this.networkStream = tcpClient.GetStream();
        }
    }
}
