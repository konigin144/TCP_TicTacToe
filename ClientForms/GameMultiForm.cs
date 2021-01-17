using System;
using System.Windows.Forms;

using ClassLibrary;

namespace ClientForms
{
    public partial class GameMultiForm : Form
    {
        MainForm mainForm;
        int playerID;

        bool[,] buttonsEnabled = new bool[3,3];

        public GameMultiForm(MainForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    buttonsEnabled[i, j] = true;
            label1.Text = "Waiting for another player...";
            lockButtons();
            multiInit();
        }

        private async void multiInit()
        {
            string elo = await Packet.ReadAsync(mainForm.client.networkStream);
            string[] initResponse = elo.Replace("\0", string.Empty).Split(' ');
            mainForm.client.enemyUsername = initResponse[1];
            this.mainForm.Text = this.mainForm.client.username + " vs " + this.mainForm.client.enemyUsername;
            label1.Text = "You're playing with " + mainForm.client.enemyUsername;
           
            Packet.Send(mainForm.client.networkStream, "sync");

            if (initResponse[0] == "0")
            {
                label3.Text = "Enemy turn";
                playerID = 0;

                string response = await Packet.ReadAsync(mainForm.client.networkStream);

                string name = "button" + response[2] + "_" + response[4];
                Control ctn = this.Controls[name];
                ctn.Text = "x";
                buttonsEnabled[chToInt(response[2]), chToInt(response[4])] = false;
                unlockButtons();

                label3.Text = "Your turn";
            }
            else
            {
                unlockButtons();
                label3.Text = "Your turn";
                playerID = 1;
            }
            GetMsg(this, new EventArgs());
        }

        private async void button_Click(object sender, EventArgs e)
        {
            if (playerID == 1)
                (sender as Button).Text = "x";
            else (sender as Button).Text = "o";
            (sender as Button).Enabled = false;
            label3.Text = "Enemy turn";
            byte[] buffer = new byte[16];
            var buttonId = ((Button)sender).Name;
            buttonsEnabled[chToInt(buttonId[6]), chToInt(buttonId[8])] = false;
            lockButtons();
            Packet.Send(mainForm.client.networkStream, buttonId[6] + " " + buttonId[8]);

            string response = await Packet.ReadAsync(mainForm.client.networkStream);

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
                lockButtons();

                label3.Visible = false;
                label2.Visible = true;
                button10.Visible = true;
                button11.Visible = true;
                button12.Visible = true;
                button13.Visible = true;
                button14.Visible = true;
            }
            else
            {
                string name = "button" + response[2] + "_" + response[4];
                Control ctn = this.Controls[name];
                if (playerID == 1)
                    ctn.Text = "o";
                else ctn.Text = "x";
                ctn.Enabled = false;
                buttonsEnabled[chToInt(response[2]), chToInt(response[4])] = false;
                unlockButtons();

                label3.Text = "Your turn";
                GetMsg(this, new EventArgs());           
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Packet.Send(mainForm.client.networkStream, "yes");

            GameMultiForm gameForm = new GameMultiForm(mainForm);
            gameForm.TopLevel = false;
            mainForm.panel1.Controls.Clear();
            mainForm.panel1.Controls.Add(gameForm);
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Show();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Packet.Send(mainForm.client.networkStream, "no");

            Application.Exit();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Packet.Send(mainForm.client.networkStream, "rev");

            GameMultiForm gameForm = new GameMultiForm(mainForm);
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
            string response = Packet.Read(mainForm.client.networkStream, 3);

            if (response == "end")
            {
                response = Packet.Read(mainForm.client.networkStream);
                label1.Text = "";
                if (response[0].Equals('2')) // dla losera i draw  //response na cuchy ruch

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

                    label3.Visible = false;
                    label2.Visible = true;
                    button10.Visible = true;
                    button11.Visible = true;
                    button12.Visible = true;
                    button13.Visible = true;
                    button14.Visible = true;
                }
            }

            var handler = MyEvent;
            handler?.Invoke(sender, e);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Packet.Send(mainForm.client.networkStream, "rank");

            string response = Packet.Read(mainForm.client.networkStream);

            string message = response;
            string title = "Ranking";
            MessageBox.Show(message, title);
        }

        private int chToInt(char ch) {
            return (int)Char.GetNumericValue(ch);
        }

        private void lockButtons()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    this.Controls["button" + i + "_" + j].Enabled = false;
        }
        private void unlockButtons()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (buttonsEnabled[i, j])
                        this.Controls["button" + i + "_" + j].Enabled = true;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Packet.Send(mainForm.client.networkStream, "0");

            Application.Restart();
        }
    }
}
