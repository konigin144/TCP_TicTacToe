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
        //Dictionary<TcpClient, string> waiting = new Dictionary<TcpClient, string>();
        //Dictionary<TcpClient, string> allUsers = new Dictionary<TcpClient, string>();
        List<(TcpClient Key, string Value)> waiting = new List<(TcpClient, string)> { };
        List<(TcpClient Key, string Value)> allUsers = new List<(TcpClient, string)> { };

        delegate void RoomDelegate(ref bool state1, ref bool state2);
        delegate void RoomManagementDelegate(GameRoom gameRoom, TcpClient client1, TcpClient client2, string clientname1, string clientname2);

        /// <summary>
        /// Handles waiting room, connects players to game rooms
        /// </summary>
        public void Manager()
        {
            while (true)
            {
                if (waiting.Count > 1)
                {
                    Console.WriteLine("---");
                    foreach ((TcpClient, string) u in waiting)
                    {
                        Console.WriteLine(u);
                    }
                    Console.WriteLine("---");
                    Dictionary<string, Ranking> dict = new Dictionary<string, Ranking>();
                    string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\database.json";
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

                                removeClient("waiting", waiting.ElementAt(j).Key);
                                removeClient("waiting", waiting.ElementAt(i).Key);

                                Console.WriteLine(clientname1 + " " + clientname2);

                                RoomManagementDelegate run2 = new RoomManagementDelegate(RoomManagement);
                                var id = run2.BeginInvoke(gameRoom, client1, client2, clientname1, clientname2, null, null);
                            }
                        }
                    }     
                }
            }
        }

        void RoomManagement(GameRoom gameRoom, TcpClient client1, TcpClient client2, string clientname1, string clientname2)
        {
            bool state1 = false, state2 = false;
            RoomDelegate run2 = new RoomDelegate(gameRoom.Run2);
            var id = run2.BeginInvoke(ref state1, ref state2, null, null);
            run2.EndInvoke(ref state1, ref state2, id);


            Console.WriteLine(state1 + " " + state2);
            //Finished game handling
            if (!state2)
            {
                removeClient("allUsers", client2);

                client2.Close();
            }
            else
            {
                waiting.Add((client2, clientname2));
            }
            if (!state1)
            {
                removeClient("allUsers", client1);
                client1.Close();
            }
            else
            {
                waiting.Add((client1, clientname1));
            }
        }

        /// <summary>
        /// Adds client to waiting room
        /// </summary>
        /// <param name="queue">0 - adds to allUsers, 1 - adds to waiting, 2 - adds to both</param>
        /// <param name="tcpClient">User object</param>
        /// <param name="username">User name</param>
        public void AddClient(int queue, TcpClient tcpClient, string username)
        {
            if (queue == 0)
                allUsers.Add((tcpClient, username));
            else if (queue == 1)
                waiting.Add((tcpClient, username));
            else
            {
                allUsers.Add((tcpClient, username));
                waiting.Add((tcpClient, username));
            }
        }

        private void removeClient(string list, TcpClient tcpClient)
        {
            if (list == "waiting")
            {
                int index = waiting.FindIndex(t => t.Key == tcpClient);
                if (index != -1)
                {
                    waiting.RemoveAt(index);
                }
            }
            else
            {
                int index = allUsers.FindIndex(t => t.Key == tcpClient);
                if (index != -1)
                {
                    allUsers.RemoveAt(index);
                }
            }

        }

        /// <summary>
        /// Checks if player is logged
        /// </summary>
        /// <param name="username">Player name</param>
        /// <returns></returns>
        public bool IsLogged(string username)
        {
            if (allUsers.FindIndex(t => t.Value == username) == -1) return false;
            else return true;
        }
    }
}
