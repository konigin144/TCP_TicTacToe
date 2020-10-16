using System;

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
            else return false;
        }

        /// <summary>
        /// Main function processing the turn. Sets player's mark and randmoize AI's choice. Checks if the game is finished after every mark set
        /// </summary>
        /// <param name="x">row chosen by user</param>
        /// <param name="y">column chosen by user</param>
        /// <returns>False if the game is finished. True otherwise</returns>
        public bool play(int x, int y)
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

            //Checks if game is finished
            if (ifFinished()) {
                if (State == 0)
                    State = 3;
                return false;
            }
            //If not, set's AI's mark
            else
            {
                Random r = new Random();
                int rX, rY;
                while (true)
                {
                    rX = r.Next(3);
                    rY = r.Next(3);
                    if (Grid[rX, rY] == ' ') break;
                }
                Grid[rX, rY] = 'o';
            }

            //Again checks if game is finished
            if (ifFinished())
            {
                if (State == 0)
                    State = 3;
                return false;
            }
            return true;
        }

    }
}
