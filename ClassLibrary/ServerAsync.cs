using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class ServerAsync : Server
    {
        public delegate void StartDelegate();
        public delegate void LoginDelegate(TcpClient tcpClient);

        public ServerAsync(string IP, int port) : base(IP, port) { }

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
                LoginDelegate LoginDelegate = new LoginDelegate(LoginForms);
                LoginDelegate.BeginInvoke(tcpClient, null, null);
            }
        }

        /// <summary>
        /// Signs in/up new users
        /// </summary>
        /// <param name="tcpClient">New user object</param>
        void LoginForms(TcpClient tcpClient)
        {
            NetworkStream networkStream = tcpClient.GetStream();

            while (true)
            {
                byte[] buffer = new byte[128];
                byte[] myWriteBuffer;
                networkStream.Read(buffer, 0, buffer.Length);
                string usernamePassword = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
                string mode = usernamePassword.Split(' ')[0];
                string username = usernamePassword.Split(' ')[1];
                string password = usernamePassword.Split(' ')[2];

                Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();
                string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\database.json";
                using (StreamReader r = File.OpenText(path))
                {
                    string json = r.ReadToEnd();
                    dict = JsonConvert.DeserializeObject<Dictionary<string, Ranking>>(json);
                }
                Ranking temp;
                if (mode == "login")
                {
                    if (dict.TryGetValue(username, out temp))
                    {
                        if (gameManager.IsLogged(username))
                        {
                            myWriteBuffer = Encoding.ASCII.GetBytes("01");
                            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        }
                        else
                        {
                            if (temp.password == password)
                            {
                                myWriteBuffer = Encoding.ASCII.GetBytes("1");
                                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                                gameManager.AddClient(0, tcpClient, username);
                                AskGameType(tcpClient, username);
                                return;
                            }
                            else
                            {
                                myWriteBuffer = Encoding.ASCII.GetBytes("00");
                                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                            }
                        }
                    }
                    else
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("00");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                }
                else
                {
                    if (!dict.TryGetValue(username, out temp))
                    {
                        HashAlgorithm algorithm = SHA256.Create();
                        var passHash = Encoding.ASCII.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(password)));
                        temp = new Ranking(passHash);
                        dict[username] = temp;
                        File.WriteAllText(@path, JsonConvert.SerializeObject(dict));
                        myWriteBuffer = Encoding.ASCII.GetBytes("1");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                        AskGameType(tcpClient, username);
                        return;
                    }
                    else
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("0");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Asks user about game type
        /// </summary>
        /// <param name="tcpClient">User object</param>
        /// <param name="username">User name</param>
        void AskGameType(TcpClient tcpClient, string username)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            byte[] buffer = new byte[16];
            string userInput;
    
            while (true)
            {
                networkStream.Read(buffer, 0, buffer.Length);
                userInput = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
                if (userInput == "single")
                {
                    Run(tcpClient);
                    return;
                }
                else if (userInput == "multi")
                {
                    gameManager.AddClient(1, tcpClient, username);
                    return;
                }
                if (userInput == "rank")
                {
                    Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();
                    string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\database.json";
                    using (StreamReader r = File.OpenText(path))
                    {
                        string json = r.ReadToEnd();
                        dict = JsonConvert.DeserializeObject<Dictionary<string, Ranking>>(json);
                    }
                    string output = "Username\tWins\tLoses\tDraws\tRatio\n";
                    foreach (KeyValuePair<string, Ranking> entry in dict)
                    {
                        output += entry.Key + "\t\t" + entry.Value.wins + "\t" + entry.Value.loses + "\t" + entry.Value.draws + "\t" + entry.Value.ratio + "\n";
                    }
                    byte[] myWriteBuffer = Encoding.ASCII.GetBytes(output);
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                }
            }
        }

        /// <summary>
        /// Runs singleplayer game
        /// </summary>
        /// <param name="tcpClient">User object</param>
        protected override void Run(TcpClient tcpClient)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            byte[] buffer;
            string userInput;

            byte[] myWriteBuffer;

            while (true)
            {
                try
                {
                    //Variables declaration
                    TicTacToe game = new TicTacToe();
                    bool gameContinue = true;
                    int x, y, botX = 0, botY = 0;

                    //Main game loop
                    while (gameContinue)
                    {
                        //User input
                        buffer = new byte[16];
                        networkStream.Read(buffer, 0, buffer.Length);
                        userInput = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);

                        x = userInput[0] - 48;
                        y = userInput[2] - 48;

                        //Turn processing
                        gameContinue = game.playSingle(x, y, ref botX, ref botY);
                        if (gameContinue)
                        {
                            //kordy bota
                            string msg = "0 " + botX + " " + botY;
                            myWriteBuffer = Encoding.ASCII.GetBytes(msg);
                            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        }

                        if (game.WrongSpace)
                        {
                            myWriteBuffer = Encoding.ASCII.GetBytes("Chosen space is unavailable. Try again:\r\n");
                            networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        }
                    }

                    //Game is finished
                    if (game.State == 1)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("1 1");
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                    else if (game.State == 2)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("1 2 " + botX + " " + botY);
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                    else if (game.State == 3)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("1 3 " + botX + " " + botY);
                        networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }

                    //Play again question
                    buffer = new byte[16];
                    networkStream.Read(buffer, 0, buffer.Length);
                    userInput = Encoding.ASCII.GetString(buffer).Replace(" ", "");
                    userInput = userInput.Replace("\0", string.Empty).ToLower();

                    if (userInput == "0")
                    {
                        tcpClient.Close();
                        return;
                    }
                }
                catch (IOException e)
                {
                    break;
                }
            }
        }

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

