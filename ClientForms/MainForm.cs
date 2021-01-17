using System.Windows.Forms;

namespace ClientForms
{
    public partial class MainForm : Form
    {
        public Client client;

        public MainForm(Client client)
        {
            this.client = client;
            InitializeComponent();
            LoginForm loginForm = new LoginForm(this);
            loginForm.TopLevel = false;
            this.panel1.Controls.Add(loginForm);
            loginForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            loginForm.Dock = DockStyle.Fill;
            loginForm.Show();
        }
    }
}
