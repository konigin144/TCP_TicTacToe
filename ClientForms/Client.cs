using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientForms
{
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
