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
        Client client;
        MainForm mainForm;
        bool registerMode = false;
        public LoginForm(Client client, MainForm mainForm)
        {
            InitializeComponent();
            this.client = client;
            this.mainForm = mainForm;
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string mode;
            if (registerMode) mode = "register";
            else mode = "login";
            string usernamePassword = mode + " " + usernameBox.Text + " " + passwordBox.Text;
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(usernamePassword);
            client.networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            byte[] buffer = new byte[16];

            client.networkStream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
            if (response == "1")
            {
                
                ChooseForm chooseForm = new ChooseForm(client, mainForm);
                chooseForm.TopLevel = false;
                mainForm.panel1.Controls.Clear();
                mainForm.panel1.Controls.Add(chooseForm);
                chooseForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                chooseForm.Dock = DockStyle.Fill;
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
