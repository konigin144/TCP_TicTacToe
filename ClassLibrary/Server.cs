using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClassLibrary
{
    /// <summary>
    /// Server and game handling
    /// </summary>
    public class Server
    {
        TicTacToe game;
        public Server()
        {
            game = new TicTacToe();

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
            row = new[] { ' ', game.Grid[0, 0], ' ', '|', ' ', game.Grid[0,1], ' ', '|', ' ', game.Grid[0, 2], '\n', '\r' };
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
        public void Run()
        {
            //Sever setup
            TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 2048);
            tcpListener.Start();
            TcpClient client = tcpListener.AcceptTcpClient();

            //Buffers initialization and basic information writing
            byte[] buffer = new byte[16];
            NetworkStream networkStream = client.GetStream();
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Type column and row\r\n");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            //App loop
            while (true)
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
                        client.Close();
                        return;
                    }
                    else if (userInput == "yes")
                        break;
                    else
                        myWriteBuffer = Encoding.ASCII.GetBytes("Wrong answer. Try again:\r\n");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                }
            }

            //-----  teraz nej
            //TcpClient client2 = new TcpClient();
            //client.Connect(IPAddress.Parse("127.0.0.1"), 2048);
        }
    }
}
