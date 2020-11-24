using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class ServerAsync : Server
    {
        public delegate void LoginDelegate(TcpClient tcpClient, ref string username, ref bool gameType);
        public delegate void StartDelegate();

        public ServerAsync(IPAddress IP, int port) : base(IP, port) { }

        GameManager gameManager = new GameManager();

        /// <summary>
        /// Accepts clients
        /// </summary>
        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
                string username = "";
                bool gameType = false;
                LoginDelegate LoginDelegate = new LoginDelegate(Login);
                LoginDelegate.BeginInvoke(tcpClient, ref username, ref gameType, null, null);
            }
        }

        /// <summary>
        /// Signs in/up new users
        /// </summary>
        /// <param name="tcpClient">New user object</param>
        /// <param name="username">New user's name</param>
        /// <param name="gameType">Game type: 0 if multi, 1 if single</param>
        void Login(TcpClient tcpClient, ref string username, ref bool gameType)
        {
            byte[] buffer = new byte[Buffer_size];
            //string username;
            byte[] myWriteBuffer;

            NetworkStream networkStream = tcpClient.GetStream();

            while (true)
            {
                myWriteBuffer = Encoding.ASCII.GetBytes("Your name: ");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                networkStream.Read(buffer, 0, buffer.Length);
                username = Encoding.ASCII.GetString(buffer).Trim(' ');
                username = username.Replace("\0", string.Empty);
                //networkStream.Read(buffer, 0, buffer.Length);

                Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();
                string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\ranking.json";
                using (StreamReader r = File.OpenText(path))
                {
                    string json = r.ReadToEnd();
                    dict = JsonConvert.DeserializeObject<Dictionary<string, Ranking>>(json);
                }
                Ranking temp;
                if (!dict.TryGetValue(username, out temp))
                {
                    buffer = new byte[Buffer_size];
                    myWriteBuffer = Encoding.ASCII.GetBytes("SIGN UP\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    myWriteBuffer = Encoding.ASCII.GetBytes("Set password: ");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    networkStream.Read(buffer, 0, buffer.Length);
                    string password = Encoding.ASCII.GetString(buffer);
                    password = password.Replace("\0", string.Empty);
                    temp = new Ranking(password);

                    //networkStream.Read(buffer, 0, buffer.Length);

                    dict[username] = temp;
                    File.WriteAllText(@path, JsonConvert.SerializeObject(dict));

                    myWriteBuffer = Encoding.ASCII.GetBytes("You've signed up!\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                    AskGameType(tcpClient, ref gameType);
                    if (!gameType)
                        gameManager.AddClient(tcpClient, username);
                    else
                    {
                        Run(tcpClient);
                    }
                    return;
                }
                else
                {
                    buffer = new byte[Buffer_size];
                    myWriteBuffer = Encoding.ASCII.GetBytes("SIGN IN\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    myWriteBuffer = Encoding.ASCII.GetBytes("Password: ");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    networkStream.Read(buffer, 0, buffer.Length);
                    string input = Encoding.ASCII.GetString(buffer);
                    input = input.Replace("\0", string.Empty);
                    //networkStream.Read(buffer, 0, buffer.Length);

                    if (gameManager.IsLogged(username))
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("This user is already logged in!\r\n");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                    else
                    {
                        if (temp.password == input)
                        {
                            myWriteBuffer = Encoding.ASCII.GetBytes("You've logged in!\r\n");
                            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                            AskGameType(tcpClient, ref gameType);
                            if (!gameType)
                                gameManager.AddClient(tcpClient, username);
                            else
                            {
                                Run(tcpClient);
                            }
                            return;
                        }
                        else
                        {
                            myWriteBuffer = Encoding.ASCII.GetBytes("Wrong password!\r\n");
                            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        }
                    }
                }
            }
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
            myWriteBuffer = Encoding.ASCII.GetBytes("\r\n");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
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
            myWriteBuffer = Encoding.ASCII.GetBytes("\r\n");
            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
        }

        /// <summary>
        /// Asks user about game type
        /// </summary>
        /// <param name="tcpClient">User object</param>
        /// <param name="gameType">Return value: 0 if multi, 1 if single</param>
        void AskGameType(TcpClient tcpClient, ref bool gameType)
        {
            NetworkStream networkStream = tcpClient.GetStream();

            byte[] buffer = new byte[Buffer_size];
            byte[] myWriteBuffer;
            string userInput;

            while (true)
            {

                myWriteBuffer = Encoding.ASCII.GetBytes("Choose game type (single/multi):\r\n");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                buffer = new byte[16];
                networkStream.Read(buffer, 0, buffer.Length);
                userInput = Encoding.ASCII.GetString(buffer).Replace(" ", "");
                userInput = userInput.Replace("\0", string.Empty).ToLower();
                //networkStream.Read(buffer, 0, buffer.Length);
                if (userInput == "single")
                {
                    gameType = true;
                    return;
                }
                else if (userInput == "multi")
                {
                    gameType = false;
                    return;
                }
                else
                {
                    myWriteBuffer = Encoding.ASCII.GetBytes("Wrong answer. Try again:\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                }
            }
        }

        /// <summary>
        /// Handles game for user and bot
        /// </summary>
        /// <param name="tcpClient">User object</param>
        protected override void Run(TcpClient tcpClient)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            byte[] buffer = new byte[Buffer_size];
            string userInput;

            byte[] myWriteBuffer;

            while (true)
            {
                try
                {
                    //Variables declaration
                    TicTacToe game = new TicTacToe();
                    bool gameContinue = true;
                    int x, y;

                    myWriteBuffer = Encoding.ASCII.GetBytes("\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    myWriteBuffer = Encoding.ASCII.GetBytes("You are playing as x\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    myWriteBuffer = Encoding.ASCII.GetBytes("Type column and row\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

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

                            //networkStream.Read(buffer, 0, buffer.Length);
                            if (userInput.Length == 2)
                            {
                                y = userInput[0] - 49;
                                x = userInput[1] - 49;
                                if (x > -1 && x < 3 && y > -1 && y < 3)
                                {
                                    break;
                                }
                            }
                            myWriteBuffer = Encoding.ASCII.GetBytes("Wrong answer. Try again:\r\n");
                            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                        }

                        //Turn processing
                        gameContinue = game.playSingle(x, y);
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
                    //Dictionary<string, Ranking> rankDict = game.updateRanking(username);
                    //PrintRanking(networkStream, rankDict);

                    //Play again question
                    while (true)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("Do you want to play again? yes/no\r\n");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        buffer = new byte[16];
                        networkStream.Read(buffer, 0, buffer.Length);
                        userInput = Encoding.ASCII.GetString(buffer).Replace(" ", "");
                        userInput = userInput.Replace("\0", string.Empty).ToLower();
                        //networkStream.Read(buffer, 0, buffer.Length);
                        if (userInput == "no")
                        {
                            myWriteBuffer = Encoding.ASCII.GetBytes("exit");
                            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                            tcpClient.Close();
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

        //protected override void Run(NetworkStream networkStream) { }

        /// <summary>
        /// Starts server
        /// </summary>
        public override void Start()
        {
            StartListening();

            StartDelegate acceptClientDelegate = new StartDelegate(AcceptClient);
            StartDelegate managerDelegate = new StartDelegate(gameManager.Manager);
            Parallel.Invoke(
                () => acceptClientDelegate.Invoke(),
                () => managerDelegate.Invoke()
            );
        }
    }
}

