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
    }
}
