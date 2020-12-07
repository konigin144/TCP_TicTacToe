using ClassLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerUnitTests
{
    [TestClass]
    public class GameTestSet
    {
        [TestMethod]
        public void GameWithBot()
        {
            ClassLibrary.TicTacToe game = new ClassLibrary.TicTacToe();
            bool gameContinue = true;

            int rx = 10, ry = 10;

            try
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (rx != i || ry != j)
                            gameContinue = game.playSingle(i, j, ref rx, ref ry);

                        if (gameContinue == false)
                            break;
                    }
                    if (gameContinue == false)
                        break;
                }
                if (game.State == 1 || game.State == 2 || game.State == 3)
                {//Game ended successfully
                }
                else
                {
                    Assert.Fail();
                }
            }
            catch (AssertFailedException e)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GameWithPlayerWin()
        {
            ClassLibrary.TicTacToe game = new ClassLibrary.TicTacToe();
            bool gameContinue = true;

            try
            {
                game.playMulti(1, 0, 0);
                game.playMulti(2, 1, 0);
                game.playMulti(1, 0, 1);
                game.playMulti(2, 2, 1);
                game.playMulti(1, 0, 2);

                if (game.State != 1)
                {
                    Assert.Fail();
                }
            }
            catch (AssertFailedException e)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public void GameWithPlayerLoose()
        {
            ClassLibrary.TicTacToe game = new ClassLibrary.TicTacToe();
            bool gameContinue = true;

            try
            {
                game.playMulti(1, 0, 0);
                game.playMulti(2, 0, 1);
                game.playMulti(1, 0, 2);
                game.playMulti(2, 1, 1);
                game.playMulti(1, 0, 1);
                //Space should be taken
                if (game.WrongSpace != true)
                {
                    Assert.Fail();
                }
                game.playMulti(1, 1, 0);
                game.playMulti(2, 2, 1);

                if (game.State != 2)
                {
                    Assert.Fail();
                }
            }
            catch (AssertFailedException e)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public void GameWithPlayerDraw()
        {
            ClassLibrary.TicTacToe game = new ClassLibrary.TicTacToe();
            bool gameContinue = true;

            try
            {
                game.playMulti(1, 0, 0);
                game.playMulti(2, 0, 2);
                game.playMulti(1, 0, 1);
                game.playMulti(2, 1, 0);
                game.playMulti(1, 1, 2);
                game.playMulti(2, 1, 1);
                game.playMulti(1, 2, 0);
                game.playMulti(2, 2, 2);
                game.playMulti(1, 2, 1);

                if (game.State != 3)
                {
                    Assert.Fail();
                }
            }
            catch (AssertFailedException e)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public void RankingUpdateDraw()
        {
            string user1 = "test1", user2 = "test2";

            Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();

            //Get project path
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\ranking.json";

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
            //Reset ranking for test users
            Ranking temp = new Ranking();
            dict[user1] = temp;
            dict[user2] = temp;

            //Saves updated ranking to the file
            File.WriteAllText(@path, JsonConvert.SerializeObject(dict));

            ClassLibrary.TicTacToe game = new ClassLibrary.TicTacToe();
            bool gameContinue = true;

            try
            {
                if (dict[user1].wins != 0 && dict[user1].loses != 0 && dict[user1].draws != 0 && dict[user1].ratio != 0.0 &&
                dict[user2].wins != 0 && dict[user2].loses != 0 && dict[user2].draws != 0 && dict[user2].ratio != 0.0)
                {
                    Assert.Fail();
                }

                game.playMulti(1, 0, 0);
                game.playMulti(2, 0, 2);
                game.playMulti(1, 0, 1);
                game.playMulti(2, 1, 0);
                game.playMulti(1, 1, 2);
                game.playMulti(2, 1, 1);
                game.playMulti(1, 2, 0);
                game.playMulti(2, 2, 2);
                game.playMulti(1, 2, 1);

                if (game.State != 3)
                {
                    Assert.Fail();
                }

                game.updateRanking(user1, user2);

                //Load file to dict if the file exists
                using (StreamReader r = File.OpenText(path))
                {
                    string json = r.ReadToEnd();
                    dict = JsonConvert.DeserializeObject<Dictionary<string, Ranking>>(json);
                }

                if (dict[user1].draws != 1 && dict[user2].draws != 1 && dict[user1].ratio != 0.5 && dict[user2].ratio != 0.5)
                {
                    Assert.Fail();
                }

            }
            catch (AssertFailedException e)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public void RankingUpdateLoose()
        {
            string user1 = "test1", user2 = "test2";

            Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();

            //Get project path
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\ranking.json";

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
            //Reset ranking for test users
            Ranking temp = new Ranking();
            dict[user1] = temp;
            dict[user2] = temp;

            //Saves updated ranking to the file
            File.WriteAllText(@path, JsonConvert.SerializeObject(dict));

            ClassLibrary.TicTacToe game = new ClassLibrary.TicTacToe();
            bool gameContinue = true;

            try
            {
                if (dict[user1].wins != 0 && dict[user1].loses != 0 && dict[user1].draws != 0 && dict[user1].ratio != 0.0 &&
                dict[user2].wins != 0 && dict[user2].loses != 0 && dict[user2].draws != 0 && dict[user2].ratio != 0.0)
                {
                    Assert.Fail();
                }

                game.playMulti(1, 0, 0);
                game.playMulti(2, 0, 1);
                game.playMulti(1, 0, 2);
                game.playMulti(2, 1, 1);
                game.playMulti(1, 1, 0);
                game.playMulti(2, 2, 1);

                if (game.State != 2)
                {
                    Assert.Fail();
                }

                game.updateRanking(user1, user2);

                //Load file to dict if the file exists
                using (StreamReader r = File.OpenText(path))
                {
                    string json = r.ReadToEnd();
                    dict = JsonConvert.DeserializeObject<Dictionary<string, Ranking>>(json);
                }

                if (dict[user1].loses != 1 && dict[user2].wins != 1 && dict[user1].ratio != 0.0 && dict[user2].ratio != 1.0)
                {
                    Assert.Fail();
                }

            }
            catch (AssertFailedException e)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public void RankingUpdateWin()
        {
            string user1 = "test1", user2 = "test2";

            Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();

            //Get project path
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\ranking.json";

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
            //Reset ranking for test users
            Ranking temp = new Ranking();
            dict[user1] = temp;
            dict[user2] = temp;

            //Saves updated ranking to the file
            File.WriteAllText(@path, JsonConvert.SerializeObject(dict));

            ClassLibrary.TicTacToe game = new ClassLibrary.TicTacToe();
            bool gameContinue = true;

            try
            {
                if (dict[user1].wins != 0 && dict[user1].loses != 0 && dict[user1].draws != 0 && dict[user1].ratio != 0.0 &&
                dict[user2].wins != 0 && dict[user2].loses != 0 && dict[user2].draws != 0 && dict[user2].ratio != 0.0)
                {
                    Assert.Fail();
                }

                game.playMulti(1, 0, 0);
                game.playMulti(2, 1, 0);
                game.playMulti(1, 0, 1);
                game.playMulti(2, 2, 1);
                game.playMulti(1, 0, 2);

                if (game.State != 1)
                {
                    Assert.Fail();
                }

                game.updateRanking(user1, user2);

                //Load file to dict if the file exists
                using (StreamReader r = File.OpenText(path))
                {
                    string json = r.ReadToEnd();
                    dict = JsonConvert.DeserializeObject<Dictionary<string, Ranking>>(json);
                }

                if (dict[user1].wins != 1 && dict[user2].loses != 1 && dict[user1].ratio != 1.0 && dict[user2].ratio != 0.0)
                {
                    Assert.Fail();
                }

            }
            catch (AssertFailedException e)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }
    }
}
