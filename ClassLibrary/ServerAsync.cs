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
        private GameManager gameManager = new GameManager();

        private delegate void StartDelegate();
        private delegate void LoginDelegate(TcpClient tcpClient);

        public ServerAsync(string IP, int port) : base(IP, port) { }

        /// <summary>
        /// Accepts clients
        /// </summary>
        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
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
                try
                {
                    string usernamePassword = Packet.Read(networkStream);

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
                                Packet.Send(networkStream, "01");
                            }
                            else
                            {
                                if (temp.password == password)
                                {
                                    Packet.Send(networkStream, "1");
                                    gameManager.AddClient(0, tcpClient, username);
                                    AskGameType(tcpClient, username);
                                    return;
                                }
                                else
                                {
                                    Packet.Send(networkStream, "00");
                                }
                            }
                        }
                        else
                        {
                            Packet.Send(networkStream, "00");
                        }
                    }
                    else
                    {
                        if (!dict.TryGetValue(username, out temp))
                        {
                            temp = new Ranking(password);
                            dict[username] = temp;
                            File.WriteAllText(@path, JsonConvert.SerializeObject(dict));

                            Packet.Send(networkStream, "1");

                            AskGameType(tcpClient, username);
                            return;
                        }
                        else
                        {
                            Packet.Send(networkStream, "0");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Connection lost: Login");
                    return;
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
            string userInput;

            while (true)
            {
                try
                {
                    userInput = Packet.Read(networkStream);
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
                } catch (Exception e)
                {
                    Console.WriteLine("Connection lost: AskGameType");
                    gameManager.removeClient("waiting", tcpClient);
                    gameManager.removeClient("allUsers", tcpClient);
                    return;
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
            string userInput;

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
                        userInput = Packet.Read(networkStream);

                        x = userInput[0] - 48;
                        y = userInput[2] - 48;

                        //Turn processing
                        gameContinue = game.playSingle(x, y, ref botX, ref botY);
                        if (gameContinue)
                        {
                            string msg = "0 " + botX + " " + botY;
                            Packet.Send(networkStream, msg);
                        }
                    }

                    //Game is finished
                    if (game.State == 1)
                    {
                        string msg = "1 1";
                        Packet.Send(networkStream, msg);
                    }
                    else if (game.State == 2)
                    {
                        string msg = "1 2 " + botX + " " + botY;
                        Packet.Send(networkStream, msg);
                    }
                    else if (game.State == 3)
                    {
                        string msg = "1 3 " + botX + " " + botY;
                        Packet.Send(networkStream, msg);
                    }

                    //Play again question
                    userInput = Packet.Read(networkStream);
                    userInput = userInput.Replace("\0", string.Empty).ToLower();

                    if (userInput == "0")
                    {
                        gameManager.removeClient("allUsers", tcpClient);
                        tcpClient.Close();
                        return;
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("Connection lost: Run");
                    gameManager.removeClient("waiting", tcpClient);
                    gameManager.removeClient("allUsers", tcpClient);
                    return;
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

