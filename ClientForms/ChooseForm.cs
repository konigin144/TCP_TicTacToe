using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForms
{
    public partial class ChooseForm : Form
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        public ChooseForm(TcpClient tcpClient)
        {
            InitializeComponent();
            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string usernamePassword = "single";
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(usernamePassword);
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            this.Hide();
            GameForm gameForm = new GameForm(tcpClient);
            gameForm.Show();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string usernamePassword = "multi";
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(usernamePassword);
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            this.Hide();
            GameForm gameForm = new GameForm(tcpClient);
            gameForm.Show();
        }
    }
}
