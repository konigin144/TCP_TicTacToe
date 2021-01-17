using System;
using System.Windows.Forms;

using ClassLibrary;

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
            Packet.Send(mainForm.client.networkStream, "single");

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
            Packet.Send(mainForm.client.networkStream, "multi");

            GameMultiForm gameForm = new GameMultiForm(mainForm);
            gameForm.TopLevel = false;
            mainForm.panel1.Controls.Clear();
            mainForm.panel1.Controls.Add(gameForm);
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Packet.Send(mainForm.client.networkStream, "rank");
            string response = Packet.Read(mainForm.client.networkStream);
            MessageBox.Show(response, "Ranking");
        }
    }
}
