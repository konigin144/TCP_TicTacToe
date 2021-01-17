using System;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;

using ClassLibrary;

namespace ClientForms
{
    public partial class LoginForm : Form
    {
        MainForm mainForm;
        public LoginForm(MainForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            this.mainForm.Text = "Login";
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            login("login");
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            login("register");
        }

        private void login(string mode)
        {
            HashAlgorithm algorithm = SHA256.Create();
            var passHash = Encoding.ASCII.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(passwordBox.Text)));
            string usernamePassword = mode+" " + usernameBox.Text + " " + passHash;

            Packet.Send(mainForm.client.networkStream, usernamePassword);

            string response = Packet.Read(mainForm.client.networkStream);
            if (response == "1")
            {
                mainForm.client.username = usernameBox.Text;
                ChooseForm chooseForm = new ChooseForm( mainForm);
                chooseForm.TopLevel = false;
                mainForm.panel1.Controls.Clear();
                mainForm.panel1.Controls.Add(chooseForm);
                chooseForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                chooseForm.Dock = DockStyle.Fill;
                chooseForm.Show();
            }
            else
            {
                if (mode == "login")
                {
                    if (response[1] == '1')
                        statusLabel.Text = "Ths user is already logged in";
                    else
                        statusLabel.Text = "Invalid username or password";
                }
                else
                    statusLabel.Text = "Username is already in use";
            }
        }
    }
}
