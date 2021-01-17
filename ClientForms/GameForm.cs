using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using ClassLibrary;

namespace ClientForms
{
    public partial class GameForm : Form
    {
        MainForm mainForm;

        public GameForm(MainForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            this.mainForm.Text = this.mainForm.client.username + " vs bot";
        }

        private void button_Click(object sender, EventArgs e)
        {
            (sender as Button).Text = "x";
            (sender as Button).Enabled = false;

            var buttonId = ((Button)sender).Name;

            Packet.Send(mainForm.client.networkStream, buttonId[6] + " " + buttonId[8]);

            string response = Packet.Read(mainForm.client.networkStream);
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
                button12.Visible = true;
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
            Packet.Send(mainForm.client.networkStream, "1");

            GameForm gameForm = new GameForm(mainForm);
            gameForm.TopLevel = false;
            mainForm.panel1.Controls.Clear();
            mainForm.panel1.Controls.Add(gameForm);
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Show();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Packet.Send(mainForm.client.networkStream, "0");
            Application.Exit();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Packet.Send(mainForm.client.networkStream, "0");

            Application.Restart();
        }
    }
}
