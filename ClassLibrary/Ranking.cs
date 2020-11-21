using System;

namespace ClassLibrary
{
    /// <summary>
    /// Class containing results of a player
    /// </summary>
    public class Ranking
    {
        #region variables
        public string password;
        public int wins;
        public int loses;
        public int draws;
        public double ratio;
        #endregion

        public Ranking()
        {
            this.password = "";
            this.wins = 0;
            this.loses = 0;
            this.draws = 0;
            this.ratio = 0;
        }

        public Ranking(string password)
        {
            this.password = password;
            this.wins = 0;
            this.loses = 0;
            this.draws = 0;
            this.ratio = 0;
        }

        public Ranking(string password, int wins, int loses, int draws)
        {
            this.password = password;
            this.wins = wins;
            this.loses = loses;
            this.draws = draws;
            this.ratio = Math.Round((this.wins + 0.5 * this.draws) / (this.wins + this.draws + this.loses), 3);
        }

        /// <summary>
        /// Updates ratio
        /// </summary>
        public void updateRatio()
        {
            this.ratio = Math.Round((this.wins + 0.5 * this.draws) / (this.wins + this.draws + this.loses), 3);
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
