using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    class GameRoom
    {
        TicTacToe game;

        string username1;
        string username2;

        NetworkStream networkStream1;
        NetworkStream networkStream2;

        delegate bool TurnDelegate(NetworkStream networkStream, int playerNum, TicTacToe game);
        delegate void WaitDelegate(NetworkStream networkStream);
        delegate bool PlayAgainDelegate(NetworkStream networkStream, bool x);
        delegate int PlayAgainDelegate2(NetworkStream networkStream);

        public GameRoom(NetworkStream networkStream1, NetworkStream networkStream2, string username1, string username2)
        {
            this.game = new TicTacToe();
            this.networkStream1 = networkStream1;
            this.networkStream2 = networkStream2;
            this.username1 = username1;
            this.username2 = username2;
        }

        bool PlayerTurn(NetworkStream networkStream, int playerNum, ref int x, ref int y, TicTacToe game)
        {

            string userInput;
            bool gameContinue = true;

            userInput = Packet.Read(networkStream);

            x = userInput[0] - 48;
            y = userInput[2] - 48;

            //Turn processing
            if (playerNum == 1)
                gameContinue = game.playMulti(1, x, y);

            else
                gameContinue = game.playMulti(2, x, y);

            return gameContinue;
        }

        int PlayAgain(NetworkStream networkStream)
        {
            string userInput;

            //buffer = new byte[16];
            while (true)
            {
                userInput = Packet.Read(networkStream);
                if (userInput == "no")
                {
                    return 0;
                }
                else if (userInput == "yes")
                    return 1;

                else if (userInput == "rev")
                    return 2;

                else
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
                    Packet.Send(networkStream, output);
                }
            }
        }

        public void Run(ref bool state1, ref bool state2)
        {
            int x = 0, y = 0;
            while (true)
            {
                try
                {
                    //Variables declaration
                    TicTacToe game = new TicTacToe();

                    Packet.Send(networkStream2, "0 " + username1);
                    Packet.Send(networkStream1, "1 " + username2);

                    Packet.Read(networkStream1);
                    Packet.Read(networkStream2);

                    Packet.Send(networkStream1, "elo");

                    //Main game loop
                    while (true)
                    {
                        bool turnResult = PlayerTurn(networkStream1, 1, ref x, ref y, game);
                        Packet.Send(networkStream2, "0 " + x + " " + y);

                        string msg;
                        if (!turnResult)
                            msg = "end";
                        else msg = "con";

                        Packet.Send(networkStream2, msg);

                        if (!turnResult) break;
                        else
                        {
                            turnResult = PlayerTurn(networkStream2, 2, ref x, ref y, game);
                            Packet.Send(networkStream1, "0 " + x + " " + y);

                            if (!turnResult)
                                msg = "end";
                            else msg = "con";

                            Packet.Send(networkStream1, msg);

                            if (!turnResult) break;
                        }
                    }

                    //Game is finished
                    if (game.State == 1)
                    {
                        Packet.Send(networkStream1, "2 1");
                        Packet.Send(networkStream2, "2 2");
                    }
                    else if (game.State == 2)
                    {
                        Packet.Send(networkStream1, "2 2");
                        Packet.Send(networkStream2, "2 1");
                    }
                    else if (game.State == 3)
                    {
                        Packet.Send(networkStream1, "2 3");
                        Packet.Send(networkStream2, "2 3");
                    }

                    //Updates and prints ranking
                    Dictionary<string, Ranking> rankDict = game.updateRanking(username1, username2);

                    //Play again question
                    PlayAgainDelegate2 playAgain1 = new PlayAgainDelegate2(PlayAgain);
                    PlayAgainDelegate2 playAgain2 = new PlayAgainDelegate2(PlayAgain);
                    var id = playAgain1.BeginInvoke(networkStream1, null, null);
                    var id2 = playAgain2.BeginInvoke(networkStream2, null, null);
                    int pa1 = playAgain1.EndInvoke(id);
                    int pa2 = playAgain2.EndInvoke(id2);

                    if (pa1 == 0 && pa2 != 0)
                    {
                        state1 = false;
                        state2 = true;
                        return;
                    }
                    else if (pa1 != 0 && pa2 == 0)
                    {
                        state1 = true;
                        state2 = false;
                        return;
                    }
                    else if (pa1 == 0 && pa2 == 0)
                    {
                        state1 = false;
                        state2 = false;
                        return;
                    }
                    else if ((pa2 == 1 && pa1 == 1) || (pa2 == 2 && pa1 == 1) || (pa1 == 2 && pa2 == 1))
                    {
                        state1 = true;
                        state2 = true;
                        return;
                    }
                    else
                    {
                        var temp = networkStream1;
                        networkStream1 = networkStream2;
                        networkStream2 = temp;
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
