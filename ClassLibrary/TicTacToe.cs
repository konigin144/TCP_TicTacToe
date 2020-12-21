using ClassLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace ClassLibrary
{
    /// <summary>
    /// TicTacToe game implementation
    /// </summary>
    public class TicTacToe
    {
        #region variables
        /// <summary>
        /// Grid containing marks
        /// </summary>
        public char[,] Grid { get; set; } = new char[3, 3];
        /// <summary>
        /// Game state
        /// 0 - game continues
        /// 1 - player won
        /// 2 - AI won
        /// 3 - draw
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// True if the place chosen by player is already in use
        /// False otherwise
        /// </summary>
        public bool WrongSpace { get; set; }
        #endregion

        /// <summary>
        /// Constructor sets all grid's spaces as " ", State as game continues and WrongSpace as false 
        /// </summary>
        public TicTacToe()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    Grid[i, j] = ' ';
            State = 0;
            WrongSpace = false;
        }

        /// <summary>
        /// Returns true if game is already finished. Sets State variable according to the result
        /// </summary>
        /// <returns></returns>
        private bool ifFinished()
        {
            for (int i = 0; i < 3; i++)
            {
                //Checks rows
                if (!Grid[i, 0].Equals(' ') && Grid[i, 0].Equals(Grid[i, 1]) && Grid[i, 0].Equals(Grid[i, 2]))
                {
                    if (Grid[i, 0].Equals('x')) State = 1;
                    else State = 2;
                    return true;
                }

                //Checks columns
                else if (!Grid[0, i].Equals(' ') && Grid[0, i].Equals(Grid[1, i]) && Grid[0, i].Equals(Grid[2, i]))
                {
                    if (Grid[0, i].Equals('x')) State = 1;
                    else State = 2;
                    return true;
                }
            }

            //Checks diagonals
            if (!Grid[0, 0].Equals(' ') && Grid[0, 0].Equals(Grid[1, 1]) && Grid[0, 0].Equals(Grid[2, 2]))
            {
                if (Grid[0, 0].Equals('x')) State = 1;
                else State = 2;
                return true;
            }
            else if (!Grid[2, 0].Equals(' ') && Grid[2, 0].Equals(Grid[1, 1]) && Grid[2, 0].Equals(Grid[0, 2]))
            {
                if (Grid[2, 0].Equals('x')) State = 1;
                else State = 2;
                return true;
            }

            //Draw check
            bool draw = false;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Grid[i, j] == ' ')
                    {
                        draw = false;
                        break;
                    }
                    else
                        draw = true;
                }
                if (!draw) break;
            }
            if (draw)
            {
                State = 3;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Updates ranking and saves it to the file
        /// </summary>
        /// <param name="username">Player name</param>
        /// <returns>Updated ranking</returns>
        public Dictionary<string, Ranking> updateRanking(string username1, string username2)
        {
            Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();

            //Get project path
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName+"\\ranking.json";

            //Create a file if does not exist
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                dict = new Dictionary<string, Ranking>();
                fs.Close();
            }
            //Load file to dict if the file exists
            else
            {
                using (StreamReader r = File.OpenText(path))
                {
                    string json = r.ReadToEnd();
                    dict = JsonConvert.DeserializeObject<Dictionary<string, Ranking>>(json);
                }
            }

            //Updates player's results
            Ranking temp;
            if (!dict.TryGetValue(username1, out temp))
            {
                temp = new Ranking();
            }
            if (State == 1)
            {
                temp.wins++;
            }
            else if (State == 2)
            {
                temp.loses++;
            }
            else if (State == 3)
            {
                temp.draws++;
            }
            temp.updateRatio();
            dict[username1] = temp;

            if (!dict.TryGetValue(username2, out temp))
            {
                temp = new Ranking();
            }
            if (State == 1)
            {
                temp.loses++;
            }
            else if (State == 2)
            {
                temp.wins++;
            }
            else if (State == 3)
            {
                temp.draws++;
            }
            temp.updateRatio();
            dict[username2] = temp;

            //Sorts dictionary by ratio DESC
            Dictionary<string, Ranking> sortDict = new Dictionary<string, Ranking>();
            foreach (var el in dict.OrderByDescending(value => value.Value.ratio))
            {
                Console.WriteLine(el.Value.ratio);
                sortDict.Add(el.Key, dict[el.Key]);
            }


            //Saves updated ranking to the file
            File.WriteAllText(@path, JsonConvert.SerializeObject(sortDict));

            return dict;
        }

        /// <summary>
        /// Main function processing the turn. Sets player's mark and randmoize AI's choice. Checks if the game is finished after every mark set
        /// </summary>
        /// <param name="x">row chosen by user</param>
        /// <param name="y">column chosen by user</param>
        /// <returns>False if the game is finished. True otherwise</returns>
        public bool playSingle(int x, int y, ref int botX, ref int botY)
        {
            //Checks if user has chosen a proper space
            if (Grid[x, y] != ' ')
            {
                WrongSpace = true;
                return true;
            }
            else WrongSpace = false;

            //Sets user's mark
            Grid[x, y] = 'x';

            //Set's AI's mark
            if (!ifFinished())
            {
                Random r = new Random();
                int rX, rY;
                while (true)
                {
                    rX = r.Next(3);
                    rY = r.Next(3);
                    if (Grid[rX, rY] == ' ') break;
                }
                botX = rX;
                botY = rY;
                Grid[rX, rY] = 'o';
            }

            return !ifFinished();
        }

        public bool playMulti(int player, int x, int y)
        {
            //Checks if user has chosen a proper space
            if (Grid[x, y] != ' ')
            {
                WrongSpace = true;
                return true;
            }
            else WrongSpace = false;

            //Sets user's mark
            if (player == 1)
                Grid[x, y] = 'x';
            else Grid[x, y] = 'o';

            return !ifFinished();
        }

    }
}
