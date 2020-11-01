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
        public ServerAsync(IPAddress IP, int port) : base(IP, port)
        {
        }
        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
                NetworkStream Stream = tcpClient.GetStream();
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(Run);
                //callback style
                var id = transmissionDelegate.BeginInvoke(Stream, TransmissionCallback, tcpClient);
                // async result style
                //IAsyncResult result = transmissionDelegate.BeginInvoke(Stream, null, null);
                ////operacje......
                //while (!result.IsCompleted) ;
                ////sprzątanie
            }
        }

        private void TransmissionCallback(IAsyncResult ar)
        {
            // sprzątanie
            TcpClient tcpClient = ar.AsyncState as TcpClient;
            tcpClient.Close();
        }

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
            //Array.Clear(buffer, 0, buffer.Length);
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

                    

                    // byte[] buffer = new byte[16];

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
                            //Array.Clear(buffer, 0, buffer.Length);
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

                    Dictionary<string, Ranking> rankDict = game.updateRanking(username, game.State);
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
                        //Array.Clear(buffer, 0, buffer.Length);
                        networkStream.Read(buffer, 0, buffer.Length);
                        if (userInput == "no")
                        {
                            //tcpClient.Close();
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
            //transmission starts within the accept function
            AcceptClient();
        }
    }
}

