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
    public partial class MainForm : Form
    {
        public MainForm(Client client)
        {
            InitializeComponent();
            LoginForm loginForm = new LoginForm(client, this);
            loginForm.TopLevel = false;
            this.panel1.Controls.Add(loginForm);
            loginForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            loginForm.Dock = DockStyle.Fill;
            loginForm.Show();
        }
    }
}
