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
    public partial class GameForm : Form
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        public GameForm(TcpClient tcpClient)
        {
            InitializeComponent();
            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();
        }

        private void button_Click(object sender, EventArgs e)
        {
            (sender as Button).Text = "x";
            (sender as Button).Enabled = false;
            byte[] buffer = new byte[16];
            var buttonId = ((Button)sender).Name;
            
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(buttonId[6]+" "+buttonId[8]);
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            
            networkStream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
            if (response[0].Equals('1'))
            {
                if (response[2].Equals('1'))
                    label1.Text = "You won!";
                else if (response[2].Equals('2'))
                {
                    label1.Text = "AI won!";
                    string name = "button" + response[4] + "_" + response[6];
                    Control ctn = this.Controls["button" + response[4] + "_" + response[6]];
                    ctn.Text = "o";
                }
                else if (response[2].Equals('3'))
                {
                    label1.Text = "Draw!";
                    string name = "button" + response[4] + "_" + response[6];
                    Control ctn = this.Controls["button" + response[4] + "_" + response[6]];
                    ctn.Text = "o";
                }
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        this.Controls["button"+i+"_"+j].Enabled = false;
                    }
                label2.Visible = true;
                button10.Visible = true;
                button11.Visible = true;
            }
            else
            {
                string name = "button"+response[2]+"_"+response[4];
                Control ctn = this.Controls[name];
                ctn.Text = "o";
                ctn.Enabled = false;
            }           
        }

        private void button10_Click(object sender, EventArgs e)
        {
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("1");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            this.Hide();
            GameForm gameForm = new GameForm(tcpClient);
            gameForm.Show();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("0");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            Application.Exit();
        }
    }
}
