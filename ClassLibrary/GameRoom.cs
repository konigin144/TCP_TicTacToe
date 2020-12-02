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

        /// <summary>
        /// Handles player turn
        /// </summary>
        /// <param name="networkStream">User stream</param>
        /// <param name="playerNum">User number: 1 or 2</param>
        /// <param name="game">Game object</param>
        /// <returns></returns>
        bool PlayerTurn(NetworkStream networkStream, int playerNum, TicTacToe game)
        {
            byte[] buffer = new byte[16];
            string userInput;
            byte[] myWriteBuffer;
            bool gameContinue = true;
            int x, y;

            while (true)
            {
                while (true)
                {
                    buffer = new byte[16];
                    networkStream.Read(buffer, 0, buffer.Length);
                    userInput = Encoding.ASCII.GetString(buffer).Replace(" ", "");
                    userInput = userInput.Replace("\0", string.Empty);

                    networkStream.Read(buffer, 0, buffer.Length);
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
                if (playerNum == 1)
                    gameContinue = game.playMulti(1, x, y);
                else
                    gameContinue = game.playMulti(2, x, y);
                if (game.WrongSpace)
                {
                    myWriteBuffer = Encoding.ASCII.GetBytes("Chosen space is unavailable. Try again:\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                }
                else
                    break;
            }
            return gameContinue;
        }

        /// <summary>
        /// Handles player's waiting for turn
        /// </summary>
        /// <param name="networkStream">User stream</param>
        void Waiting(NetworkStream networkStream)
        {
            byte[] buffer = new byte[16];
            while (!turnFinishedFlag) { }
            while (networkStream.DataAvailable)
            {
                networkStream.Read(buffer, 0, buffer.Length);
            }
            return;
        }

        /// <summary>
        /// Asks player f he wants to play again
        /// </summary>
        /// <param name="networkStream">User stream</param>
        /// <param name="x">Question type: true if about disconnecting, false if about playing with the same opponent</param>
        /// <returns></returns>
        bool PlayAgain(NetworkStream networkStream, bool x)
        {
            byte[] myWriteBuffer;
            byte[] buffer = new byte[16];
            string userInput;

            while (true)
            {
                if (x)
                    myWriteBuffer = Encoding.ASCII.GetBytes("Do you want to play again? yes/no\r\n");
                else myWriteBuffer = Encoding.ASCII.GetBytes("Do you want to play again with the same user? yes/no\r\n");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                buffer = new byte[16];
                networkStream.Read(buffer, 0, buffer.Length);
                userInput = Encoding.ASCII.GetString(buffer).Replace(" ", "");
                userInput = userInput.Replace("\0", string.Empty).ToLower();
                networkStream.Read(buffer, 0, buffer.Length);
                if (userInput == "no")
                {
                    return false;
                }
                else if (userInput == "yes")
                    return true;
                else
                {
                    myWriteBuffer = Encoding.ASCII.GetBytes("Wrong answer. Try again:\r\n");
                    networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                }
            }
        }

        /// <summary>
        /// Prints grid
        /// </summary>
        /// <param name="networkStream">User stream</param>
        /// <param name="game">Game object</param>
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
        /// Prints ranking
        /// </summary>
        /// <param name="networkStream">User stream</param>
        /// <param name="dict">Ranking</param>
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
                myWriteBuffer = Encoding.ASCII.GetBytes("Ratio: " + element.Value.ratio.ToString() + "\n\r");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                myWriteBuffer = Encoding.ASCII.GetBytes("\n\r");
                networkStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
            }
        }

        /// <summary>
        /// Handles multiplayer game
        /// </summary>
        /// <param name="state1">Return value: false if player1 wants to disconnect; true otherwise</param>
        /// <param name="state2">Return value: false if player2 wants to disconnect; true otherwise</param>
        public void Run(ref bool state1, ref bool state2)
        {
            byte[] buffer = new byte[16];
            byte[] myWriteBuffer;

            TurnDelegate turnDelegate = new TurnDelegate(PlayerTurn);
            WaitDelegate waitDelegate = new WaitDelegate(Waiting);

            while (true)
            {
                try
                {
                    //Variables declaration
                    TicTacToe game = new TicTacToe();
                    myWriteBuffer = Encoding.ASCII.GetBytes("\r\n");
                    networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    myWriteBuffer = Encoding.ASCII.GetBytes("You are playing with " + username2 + "\r\n");
                    networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    myWriteBuffer = Encoding.ASCII.GetBytes("You are playing with " + username1 + "\r\n");
                    networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    myWriteBuffer = Encoding.ASCII.GetBytes("You are playing as x\r\n");
                    networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    myWriteBuffer = Encoding.ASCII.GetBytes("You are playing as o\r\n");
                    networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                    //Main game loop
                    while (true)
                    {
                        Print(networkStream1, game);
                        Print(networkStream2, game);

                        myWriteBuffer = Encoding.ASCII.GetBytes("Type column and row\r\n");
                        networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        myWriteBuffer = Encoding.ASCII.GetBytes("Enemy turn\r\n");
                        networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                        turnFinishedFlag = false;
                        var idTurn = turnDelegate.BeginInvoke(networkStream1, 1, game, null, null);
                        var idWait = waitDelegate.BeginInvoke(networkStream2, null, null);
                        bool turnResult = turnDelegate.EndInvoke(idTurn);
                        turnFinishedFlag = true;
                        waitDelegate.EndInvoke(idWait);

                        if (!turnResult) break;
                        else
                        {
                            Print(networkStream1, game);
                            Print(networkStream2, game);

                            myWriteBuffer = Encoding.ASCII.GetBytes("Type column and row\r\n");
                            networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                            myWriteBuffer = Encoding.ASCII.GetBytes("Enemy turn\r\n");
                            networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                            turnFinishedFlag = false;
                            idTurn = turnDelegate.BeginInvoke(networkStream2, 2, game, null, null);
                            idWait = waitDelegate.BeginInvoke(networkStream1, null, null);
                            turnResult = turnDelegate.EndInvoke(idTurn);
                            turnFinishedFlag = true;
                            waitDelegate.EndInvoke(idWait);

                            if (!turnResult) break;
                        }

                    }

                    //Game is finished
                    Print(networkStream1, game);
                    Print(networkStream2, game);
                    if (game.State == 1)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes(username1 + " won!\r\n");
                        networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                    else if (game.State == 2)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes(username2 + " won!\r\n");
                        networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }
                    else if (game.State == 3)
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes("Draw!\r\n");
                        networkStream1.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                        networkStream2.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                    }

                    //Updates and prints ranking
                    Dictionary<string, Ranking> rankDict = game.updateRanking(username1, username2);
                    PrintRanking(networkStream1, rankDict);
                    PrintRanking(networkStream2, rankDict);

                    //Play again question
                    PlayAgainDelegate playAgain1 = new PlayAgainDelegate(PlayAgain);
                    PlayAgainDelegate playAgain2 = new PlayAgainDelegate(PlayAgain);
                    var id = playAgain1.BeginInvoke(networkStream1, true, null, null);
                    var id2 = playAgain2.BeginInvoke(networkStream2, true, null, null);
                    bool pa1 = playAgain1.EndInvoke(id);
                    bool pa2 = playAgain2.EndInvoke(id2);

                    if (pa1 && pa2)
                    {
                        id = playAgain1.BeginInvoke(networkStream1, false, null, null);
                        id2 = playAgain2.BeginInvoke(networkStream2, false, null, null);
                        pa1 = playAgain1.EndInvoke(id);
                        pa2 = playAgain2.EndInvoke(id2);

                        if (!(pa1 && pa2))
                        {
                            state1 = true;
                            state2 = true;
                            return;
                        }
                    }
                    else if (pa1 && !pa2)
                    {
                        state1 = true;
                        state2 = false;
                        return;
                    }
                    else if (!pa1 && pa2)
                    {
                        state1 = false;
                        state2 = true;
                        return;
                    }

                    else
                    {
                        state1 = false;
                        state2 = false;
                        return;
                    }
                }
                catch (IOException e)
                {
                    break;
                }
            }
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

            buffer = new byte[16];
            while (true)
            {
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
                    Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();
                    string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\ranking.json";
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

                }
                catch (IOException e)
                {
                    break;
                }
            }
        }
    }
}
