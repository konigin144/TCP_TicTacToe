﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ClientForms
{
    static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2048);
            Client client = new Client(new TcpClient(), remoteEP);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(client));
            Application.Exit();
        }
    }
}
