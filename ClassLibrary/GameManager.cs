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
    class GameManager
    {
        Dictionary<TcpClient, string> waiting = new Dictionary<TcpClient, string>();
        Dictionary<TcpClient, string> allUsers = new Dictionary<TcpClient, string>();

        /// <summary>
        /// Handles waiting room, connects players to game rooms
        /// </summary>
        public void Manager()
        {
            while (true)
            {
                if (waiting.Count > 1)
                {
                    Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();
                    string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\ranking.json";
                    if (!File.Exists(path))
                    {
                        FileStream fs = File.Create(path);
                        dict = new Dictionary<string, Ranking>();
                        fs.Close();
                    }
                    else
                    {
                        using (StreamReader r = File.OpenText(path))
                        {
                            string json = r.ReadToEnd();
                            dict = JsonConvert.DeserializeObject<Dictionary<string, Ranking>>(json);
                        }
                    }

                    for (int i = 0; i < waiting.Count; i++)
                    {
                        dict.TryGetValue(waiting.ElementAt(i).Value, out Ranking user1);
                        for (int j = i + 1; j < waiting.Count; j++)
                        {
                            dict.TryGetValue(waiting.ElementAt(j).Value, out Ranking user2);
                            if (Math.Abs(user1.ratio - user2.ratio) <= 0.2)
                            {                                
                                GameRoom gameRoom = new GameRoom(waiting.ElementAt(i).Key.GetStream(), waiting.ElementAt(j).Key.GetStream(), waiting.ElementAt(i).Value, waiting.ElementAt(j).Value);
                                TcpClient client1 = waiting.ElementAt(i).Key;
                                TcpClient client2 = waiting.ElementAt(j).Key;
                                string clientname1 = waiting.ElementAt(i).Value;
                                string clientname2 = waiting.ElementAt(j).Value;
                                waiting.Remove(waiting.ElementAt(j).Key);
                                waiting.Remove(waiting.ElementAt(i).Key);
                                bool state1 = false, state2 = false;
                                gameRoom.Run(ref state1, ref state2);
                                if (!state2)
                                {
                                    allUsers.Remove(client2);
                                    byte[] myWriteBuffer = Encoding.ASCII.GetBytes("\\exit");
                                    client2.GetStream().Write(myWriteBuffer, 0, myWriteBuffer.Length);
                                    client2.Close();
                                }
                                else
                                {
                                    waiting.Add(client2, clientname2);
                                }
                                if (!state1)
                                {
                                    byte[] myWriteBuffer = Encoding.ASCII.GetBytes("\\exit");
                                    client1.GetStream().Write(myWriteBuffer, 0, myWriteBuffer.Length);
                                    allUsers.Remove(client1);
                                    client1.Close();
                                }
                                else
                                {
                                    waiting.Add(client1, clientname1);
                                }
                            }
                        }
                    }     
                }
            }
        }

        /// <summary>
        /// Adds client to waiting room
        /// </summary>
        /// <param name="tcpClient">User object</param>
        /// <param name="username">User name</param>
        public void AddClient(TcpClient tcpClient, string username)
        {
            allUsers.Add(tcpClient, username);
            waiting.Add(tcpClient, username);
        }

        /// <summary>
        /// Checks if player is logged
        /// </summary>
        /// <param name="username">Player name</param>
        /// <returns></returns>
        public bool IsLogged(string username)
        {
            if (allUsers.ContainsValue(username)) return true;
            else return false;
        }
    }
}
