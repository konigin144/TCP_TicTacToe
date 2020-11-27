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
    public partial class LoginForm : Form
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        bool registerMode = false;
        public LoginForm(TcpClient tcpClient)
        {
            InitializeComponent();
            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string mode;
            if (registerMode) mode = "register";
            else mode = "login";
            string usernamePassword = mode + " " + usernameBox.Text + " " + passwordBox.Text;
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(usernamePassword);
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            byte[] buffer = new byte[16];

            networkStream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
            if (response == "1")
            {
                this.Hide();
                ChooseForm chooseForm = new ChooseForm(tcpClient);
                chooseForm.Show();
            }
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            if (registerMode)
            {
                registerMode = false;
                loginButton.Text = "Login";
                registerButton.Text = "Register";
            }
            else
            {
                registerMode = true;
                loginButton.Text = "Register";
                registerButton.Text = "Login";
            }
        }

    }
}
