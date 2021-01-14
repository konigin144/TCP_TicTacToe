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

        bool turnFinishedFlag;
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

        bool PlayerTurn2(NetworkStream networkStream, int playerNum, ref int x, ref int y, TicTacToe game)
        {
            string userInput;
            bool gameContinue = true;

            byte[] buffer = new byte[16];
            networkStream.Read(buffer, 0, buffer.Length);
            userInput = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);

            x = userInput[0] - 48;
            y = userInput[2] - 48;

            //Turn processing
            if (playerNum == 1)
                gameContinue = game.playMulti(1, x, y);

            else
                gameContinue = game.playMulti(2, x, y);

            return gameContinue;
        }

        int PlayAgain2(NetworkStream networkStream)
        {
            byte[] myWriteBuffer;
            byte[] buffer = new byte[512];
            string userInput;

            //buffer = new byte[16];
            while (true)
            {
                buffer = new byte[16];
                networkStream.Read(buffer, 0, buffer.Length);
                userInput = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
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
                    Console.WriteLine("ranking");
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
                    myWriteBuffer = Encoding.ASCII.GetBytes(output);
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                    //return 3;
                }
            }
        }

        public void Run2(ref bool state1, ref bool state2)
        {
            byte[] buffer = new byte[16];
            byte[] myWriteBuffer;
            int x = 0, y = 0;
            while (true)
            {
                try
                {
                    //Variables declaration
                    TicTacToe game = new TicTacToe();

                    myWriteBuffer = Encoding.ASCII.GetBytes("0 " + username1);
                    networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    myWriteBuffer = Encoding.ASCII.GetBytes("1 " + username2);
                    networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                    networkStream1.Read(buffer, 0, buffer.Length);
                    buffer = new byte[512];
                    networkStream2.Read(buffer, 0, buffer.Length);
                    buffer = new byte[512];

                    myWriteBuffer = Encoding.ASCII.GetBytes("elo");
                    networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                    //Main game loop
                    while (true)
                    {
                        turnFinishedFlag = false;

                        bool turnResult = PlayerTurn2(networkStream1, 1, ref x, ref y, game);
                        myWriteBuffer = Encoding.ASCII.GetBytes("0 " + x + " " + y);
                        networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                        string msg;
                        if (!turnResult)
                            msg = "end";
                        else msg = "con";

                        myWriteBuffer = Encoding.ASCII.GetBytes(msg);
                        networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                        turnFinishedFlag = true;

                        if (!turnResult) break;
                        else
                        {
                            turnFinishedFlag = false;

                            turnResult = PlayerTurn2(networkStream2, 2, ref x, ref y, game);
                            myWriteBuffer = Encoding.ASCII.GetBytes("0 " + x + " " + y);
                            networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                            turnFinishedFlag = true;

                            if (!turnResult)
                                msg = "end";
                            else msg = "con";

                            myWriteBuffer = Encoding.ASCII.GetBytes(msg);
                            networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                            if (!turnResult) break;
                        }
                    }

                    //Game is finished
                    if (game.State == 1)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("2 1");
                        networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        myWriteBuffer = Encoding.ASCII.GetBytes("2 2");
                        networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                    else if (game.State == 2)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("2 2");
                        networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        myWriteBuffer = Encoding.ASCII.GetBytes("2 1");
                        networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                    else if (game.State == 3)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("2 3");
                        networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }

                    //Updates and prints ranking
                    Dictionary<string, Ranking> rankDict = game.updateRanking(username1, username2);
                    //PrintRanking(networkStream1, rankDict);
                    //PrintRanking(networkStream2, rankDict);

                    //Play again question
                    PlayAgainDelegate2 playAgain1 = new PlayAgainDelegate2(PlayAgain2);
                    PlayAgainDelegate2 playAgain2 = new PlayAgainDelegate2(PlayAgain2);
                    var id = playAgain1.BeginInvoke(networkStream1, null, null);
                    var id2 = playAgain2.BeginInvoke(networkStream2, null, null);
                    int pa1 = playAgain1.EndInvoke(id);
                    int pa2 = playAgain2.EndInvoke(id2);

                    if (pa1==0 && pa2!=0)
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
