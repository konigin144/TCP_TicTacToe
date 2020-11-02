using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;

namespace ClassLibrary
{
    public class ServerAsync : Server
    {
        public delegate void TransmissionDataDelegate(NetworkStream stream);
        public ServerAsync(IPAddress IP, int port) : base(IP, port) {}

        /// <summary>
        /// Accept clients
        /// </summary>
        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
                NetworkStream Stream = tcpClient.GetStream();
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(Run);
                var id = transmissionDelegate.BeginInvoke(Stream, TransmissionCallback, tcpClient);
            }
        }

        /// <summary>
        /// Closes connection with client
        /// </summary>
        /// <param name="ar"></param>
        private void TransmissionCallback(IAsyncResult ar)
        {
            TcpClient tcpClient = ar.AsyncState as TcpClient;
            tcpClient.Close();
        }

        /// <summary>
        /// Prints grid
        /// </summary>
        /// <param name="networkStream">NetworkStream object</param>
        /// <param name="game">TicTacToe object</param>
        void Print(NetworkStream networkStream, TicTacToe game)
        {
            char[] row;
            byte[] myWriteBuffer;
            row = new[] { ' ', game.Grid[0, 0], ' ', '|', ' ', game.Grid[0, 1], ' ', '|', ' ', game.Grid[0, 2], '\n', '\r' };
            myWriteBuffer = Encoding.ASCII.GetBytes(row);
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            myWriteBuffer = Encoding.ASCII.GetBytes("-----------\n\r");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            row = new[] { ' ', game.Grid[1, 0], ' ', '|', ' ', game.Grid[1, 1], ' ', '|', ' ', game.Grid[1, 2], '\n', '\r' };
            myWriteBuffer = Encoding.ASCII.GetBytes(row);
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            myWriteBuffer = Encoding.ASCII.GetBytes("-----------\n\r");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            row = new[] { ' ', game.Grid[2, 0], ' ', '|', ' ', game.Grid[2, 1], ' ', '|', ' ', game.Grid[2, 2], '\n', '\r' };
            myWriteBuffer = Encoding.ASCII.GetBytes(row);
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
        }

        /// <summary>
        /// Prints ranking
        /// </summary>
        /// <param name="networkStream">NetworkStream object</param>
        /// <param name="dict">Dictionary including ranking</param>
        void PrintRanking(NetworkStream networkStream, Dictionary<string, Ranking> dict)
        {
            byte[] myWriteBuffer;
            foreach (var element in dict)
            {
                myWriteBuffer = Encoding.ASCII.GetBytes(element.Key + ":\n\r");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                myWriteBuffer = Encoding.ASCII.GetBytes("Wins: " + element.Value.wins.ToString() + "\n\r");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                myWriteBuffer = Encoding.ASCII.GetBytes("Loses: " + element.Value.loses.ToString() + "\n\r");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                myWriteBuffer = Encoding.ASCII.GetBytes("Draws: " + element.Value.draws.ToString() + "\n\r");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                myWriteBuffer = Encoding.ASCII.GetBytes("\n\r");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            }
        }

        /// <summary>
        /// Main function, handles game, implements communication with user
        /// </summary>
        protected override void Run(NetworkStream networkStream)
        {
            //networkStream.ReadTimeout = 10000;
            byte[] buffer = new byte[Buffer_size];
            string userInput;
            string username;

            //User name
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Your name: ");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            networkStream.Read(buffer, 0, buffer.Length);
            username = Encoding.ASCII.GetString(buffer).Trim(' ');
            username = username.Replace("\0", string.Empty);
            networkStream.Read(buffer, 0, buffer.Length);
            myWriteBuffer = Encoding.ASCII.GetBytes("Hello "+username+"!\r\n");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            while (true)
            {
                try
                {                    
                    //Variables declaration
                    TicTacToe game = new TicTacToe();
                    bool gameContinue = true;                    
                    int x, y;

                    myWriteBuffer = Encoding.ASCII.GetBytes("Type column and row\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                    //Main game loop
                    while (gameContinue)
                    {
                        Print(networkStream, game);

                        //User input
                        while (true)
                        {
                            buffer = new byte[Buffer_size];
                            networkStream.Read(buffer, 0, buffer.Length);
                            userInput = Encoding.ASCII.GetString(buffer).Replace(" ", "");
                            userInput = userInput.Replace("\0", string.Empty);
                            y = userInput[0] - 49;
                            x = userInput[1] - 49;
                            networkStream.Read(buffer, 0, buffer.Length);
                            if (userInput.Length == 2 && x > -1 && x < 3 && y > -1 && y < 3)
                            {
                                break;
                            }
                            else
                            {
                                myWriteBuffer = Encoding.ASCII.GetBytes("Wrong answer. Try again:\r\n");
                                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                            }
                        }

                        //Turn processing
                        gameContinue = game.play(x, y);
                        if (game.WrongSpace)
                        {
                            myWriteBuffer = Encoding.ASCII.GetBytes("Chosen space is unavailable. Try again:\r\n");
                            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        }
                    }

                    //Game is finished
                    Print(networkStream, game);
                    if (game.State == 1)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("Player won!\r\n");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                    else if (game.State == 2)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("AI won!\r\n");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                    else if (game.State == 3)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("Draw!\r\n");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }

                    //Updates and prints ranking
                    Dictionary<string, Ranking> rankDict = game.updateRanking(username);
                    PrintRanking(networkStream, rankDict);

                    //Play again question
                    while (true)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("Do you want to play again? yes/no\r\n");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        buffer = new byte[16];
                        networkStream.Read(buffer, 0, buffer.Length);
                        userInput = Encoding.ASCII.GetString(buffer).Replace(" ", "");
                        userInput = userInput.Replace("\0", string.Empty).ToLower();
                        networkStream.Read(buffer, 0, buffer.Length);
                        if (userInput == "no")
                        {
                            return;
                        }
                        else if (userInput == "yes")
                            break;
                        else
                            myWriteBuffer = Encoding.ASCII.GetBytes("Wrong answer. Try again:\r\n");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                }
                catch (IOException e)
                {
                    break;
                }
            }
        }

        public override void Start()
        {
            StartListening();
            AcceptClient();
        }
    }
}

