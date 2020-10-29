using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClassLibrary
{
    /// <summary>
    /// Server and game handling
    /// </summary>
    public class ServerSync : Server
    {
        TicTacToe game;
        public ServerSync(IPAddress IP, int port) : base(IP, port)
        {
            game = new TicTacToe();
        }

        protected override void AcceptClient()
        {
            TcpClient tcpClient = TcpListener.AcceptTcpClient();
            byte[] buffer = new byte[Buffer_size];
            NetworkStream Stream = tcpClient.GetStream();
            Run(Stream);
        }

        public override void Start()
        {
            StartListening();
            //transmission starts within the accept function
            AcceptClient();
        }

        /// <summary>
        /// Writes grid
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
        /// Main function, handles server and game, implements communication with user
        /// </summary>
        protected override void Run(NetworkStream networkStream)
        {
            //Buffer initialization and basic information writing
            byte[] buffer = new byte[16];

            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Type column and row\r\n");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            networkStream.ReadTimeout = 10000;
            //App loop
            while (true)
            {
                try
                {

                    //Variables declaration
                    TicTacToe game = new TicTacToe();
                    bool gameContinue = true;
                    string userInput;
                    int x, y;

                    //Main game loop
                    while (gameContinue)
                    {
                        Print(networkStream, game);

                        //User input
                        while (true)
                        {
                            buffer = new byte[16];
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
    }
}