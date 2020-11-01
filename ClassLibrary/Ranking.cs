using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Ranking
    {
        //public string name;
        public int wins;
        public int loses;
        public int draws;

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

        public void print()
        {
            Console.WriteLine(wins + " " + loses + " " + draws);
        }
    }
}
