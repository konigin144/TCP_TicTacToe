using System;

namespace ClassLibrary
{
    /// <summary>
    /// Class containing results of a player
    /// </summary>
    public class Ranking
    {
        #region variables
        public int wins;
        public int loses;
        public int draws;
        #endregion

        public Ranking()
        {
            this.wins = 0;
            this.loses = 0;
            this.draws = 0;
        }

        public Ranking(int wins, int loses, int draws)
        {
            this.wins = wins;
            this.loses = loses;
            this.draws = draws;
        }

        /// <summary>
        /// Prints results in console
        /// </summary>
        public void print()
        {
            Console.WriteLine(wins + " " + loses + " " + draws);
        }
    }
}
