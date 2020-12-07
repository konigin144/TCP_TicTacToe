using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForms
{
    public partial class GameMultiForm : Form
    {
        Client client;
        MainForm mainForm;
        int playerID;
        string enemyUsername;
        string playerUsername;
        public GameMultiForm(Client client, bool ifBegins, MainForm mainForm)
        {
            InitializeComponent();
            this.client = client;
            this.mainForm = mainForm;
            Console.WriteLine("init");

            multiInit();
        }

        private void multiInit()
        {
            Console.WriteLine("multi init");
            byte[] buffer = new byte[16];
            client.networkStream.Read(buffer, 0, buffer.Length);
            string[] initResponse = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty).Split(' ');
            enemyUsername = initResponse[1];
            if (initResponse[0] == "0")
            {
                playerID = 0;
                client.networkStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
                string name = "button" + response[2] + "_" + response[4];
                Control ctn = this.Controls[name];
                ctn.Text = "x";
                ctn.Enabled = false;

                label1.Text = "Your turn";
            }
            else
            {
                label1.Text = "Your turn";
                playerID = 1;
            }
            Console.WriteLine("multi init end");
            GetMsg(this, new EventArgs());
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (playerID == 1)
                (sender as Button).Text = "x";
            else (sender as Button).Text = "o";
            (sender as Button).Enabled = false;
            label1.Text = "Enemy turn";
            byte[] buffer = new byte[16];
            var buttonId = ((Button)sender).Name;
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(buttonId[6] + " " + buttonId[8]);
            client.networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            //label1.Text = "Enemy turn";

            client.networkStream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
            if (response[0].Equals('2')) //dla zwycięzcy i draw   //response na swój ruch
            {
                if (response[2].Equals('1'))
                    label1.Text = "You won!";
                else if (response[2].Equals('2'))
                {
                    label1.Text = "Enemy won!";
                }
                else if (response[2].Equals('3'))
                {
                    label1.Text = "Draw!";
                }
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        this.Controls["button" + i + "_" + j].Enabled = false;
                    }
                label2.Visible = true;
                button10.Visible = true;
                button11.Visible = true;
                button12.Visible = true;
                button13.Visible = true;
            }
            else
            {
                string name = "button" + response[2] + "_" + response[4];
                Control ctn = this.Controls[name];
                if (playerID == 1)
                    ctn.Text = "o";
                else ctn.Text = "x";
                ctn.Enabled = false;

                label1.Text = "Your turn";
                GetMsg(this, new EventArgs());           
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("yes");
            client.networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            GameMultiForm gameForm = new GameMultiForm(client, true, mainForm);
            gameForm.TopLevel = false;
            mainForm.panel1.Controls.Clear();
            mainForm.panel1.Controls.Add(gameForm);
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Show();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("no");
            client.networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            Application.Exit();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("rev");
            client.networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            GameMultiForm gameForm = new GameMultiForm(client, true, mainForm);
            gameForm.TopLevel = false;
            mainForm.panel1.Controls.Clear();
            mainForm.panel1.Controls.Add(gameForm);
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Show();
        }

        public event EventHandler<EventArgs> MyEvent;

        protected virtual void GetMsg(object sender, EventArgs e)
        {
            Console.WriteLine(client.username + " event");
            byte[] buffer = new byte[16];

            client.networkStream.Read(buffer, 0, 3);
            string response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
            Console.WriteLine(response);

            if (response == "end")
            {
                client.networkStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
                if (response[0].Equals('2')) // dla losera i draw  //response na cuchy ruch

                {
                    Console.WriteLine(client.username + " event 2");
                    if (response[2].Equals('1'))
                        label1.Text = "You won!";
                    else if (response[2].Equals('2'))
                    {
                        label1.Text = "Enemy won!";
                    }
                    else if (response[2].Equals('3'))
                    {
                        label1.Text = "Draw!";
                    }
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            this.Controls["button" + i + "_" + j].Enabled = false;
                        }
                    label2.Visible = true;
                    button10.Visible = true;
                    button11.Visible = true;
                    button12.Visible = true;
                    button13.Visible = true;
                }
            }

            var handler = MyEvent;
            handler?.Invoke(sender, e);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string msg = "rank";
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(msg);
            client.networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            byte[] buffer = new byte[512];
            client.networkStream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);

            string message = response;
            string title = "Ranking";
            MessageBox.Show(message, title);
        }
    }
}
