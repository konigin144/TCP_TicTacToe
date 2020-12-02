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
    public partial class WaitingForm : Form
    {
        Client client;
        MainForm mainForm;
        delegate bool WaitDelegate();
        public WaitingForm(Client client, MainForm mainForm)
        {
            InitializeComponent();

            this.client = client;
            this.mainForm = mainForm;

            WaitDelegate waitDelegate = new WaitDelegate(wait);
            var idWait = waitDelegate.BeginInvoke(null, null);
            bool response = waitDelegate.EndInvoke(idWait);

            Console.WriteLine("waited");


            GameMultiForm gameForm = new GameMultiForm(client, response, mainForm);
            gameForm.TopLevel = false;
            mainForm.panel1.Controls.Clear();
            mainForm.panel1.Controls.Add(gameForm);
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Show();
            //wait();
        }




        bool wait()
        {
            byte[] buffer = new byte[16];
            client.networkStream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
            bool ifBegins;
            if (response == "1") //if begins
            {
                ifBegins = true;
            }
            else
            {
                ifBegins = false;
            }
            return ifBegins;
            /*
            this.Hide();
            GameMultiForm gameForm = new GameMultiForm(tcpClient, ifBegins);
            gameForm.Show();*/
        }
    }
}
