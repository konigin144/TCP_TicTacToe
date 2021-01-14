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
        MainForm mainForm;
        public ChooseForm(MainForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            this.mainForm.Text = this.mainForm.client.username;
            helloLabel.Text = "Hello " + mainForm.client.username + "!";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string gametype = "single";
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(gametype);
            mainForm.client.networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);



            GameForm gameForm = new GameForm(mainForm);
            gameForm.TopLevel = false;
            mainForm.panel1.Controls.Clear();
            mainForm.panel1.Controls.Add(gameForm);
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Show();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("multi");
            string gametype = "multi";
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(gametype);
            mainForm.client.networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);



            GameMultiForm gameForm = new GameMultiForm(mainForm);
            gameForm.TopLevel = false;
            mainForm.panel1.Controls.Clear();
            mainForm.panel1.Controls.Add(gameForm);
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Show();

            /*WaitingForm waitingForm = new WaitingForm(tcpClient, mainForm);
            waitingForm.TopLevel = false;
            mainForm.panel1.Controls.Clear();
            mainForm.panel1.Controls.Add(waitingForm);
            waitingForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            waitingForm.Dock = DockStyle.Fill;
            waitingForm.Show();*/
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string msg = "rank";
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(msg);
            mainForm.client.networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            byte[] buffer = new byte[512];
            mainForm.client.networkStream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);

            string message = response;
            string title = "Ranking";
            MessageBox.Show(message, title);
        }
    }
}
