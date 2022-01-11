using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tic_Tac_Toc_Core;
using Serilog;
using Tic_Tac_Toc_Core.Models;
using Tic_Tac_Toe_Server.Models;
using Tic_Tac_Toc_Core.Game;

namespace Tic_Tac_Toe_Server.Services
{
    class Tcp_Server
    {
        static private TcpListener Server;
        static private Tic_Tac_Toe_baseEntities db;
        static private List<string> Codes = new List<string>();
        static private List<Connected_Client> Clients = new List<Connected_Client>();
        static private Dictionary<string, Game> Game_List = new Dictionary<string, Game>();
        static private Random random = new Random();
        static public void Run()
        {
            db = new Tic_Tac_Toe_baseEntities();
            Server = new TcpListener(IPAddress.Any, 10123);
            Server.Start();
            while (true)
            {
                TcpClient client = Server.AcceptTcpClient();
                Task.Factory.StartNew(() =>
                {
                    NetworkStream stream = client.GetStream();
                    Log.Information("New Connection");
                    byte[] buffer = stream.ReadStream(); // Получение ключа
                    if (Encoding.ASCII.GetString(buffer) != Config.AppKey)
                    {
                        client.Close();
                        client.Dispose();
                        return;
                    }
                    Log.Information("User is logged in");
                    Connected_Client connected = new Connected_Client();
                    while (true)
                    {
                        try
                        {
                            var data = stream.ReadStringStream().FromJson();
                            Log.Debug($"{connected?.Client?.Login} : {(string)data["Method"]}");
                            switch ((string)data["Method"])
                            {
                                case "Sing_Up":
                                    Sing_Up(client, new Client_Model()
                                    {
                                        Login = (string)data["Data"]["Login"],
                                        Password = (string)data["Data"]["Password"]
                                    }, out connected);
                                    break;
                                case "Registration":
                                    Registration(client, new Client_Model()
                                    {
                                        Login = (string)data["Data"]["Login"],
                                        Password = (string)data["Data"]["Password"]
                                    }, out connected);
                                    break;
                                case "Create_Lobby":
                                    Create_Lobby(stream, connected);
                                    break;
                                case "Connect_To_Game":
                                    Connect_To_Game(stream, connected, (string)data["Data"]);
                                    break;
                                case "Send_Click":
                                    Send_Click(stream, connected, (int)data["Data"]);
                                    break;
                                case "Refresh_Game_Field":
                                    Refresh_Game_Field(stream, connected);
                                    break;
                                case "Wate_Partner":
                                    Wate_Partner(stream, connected);
                                    break;
                                case "Wate_Move":
                                    Wate_Move(stream, connected);
                                    break;
                                case "Get_Info":
                                    Get_Info(stream, connected);
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                            client.Close();
                            client.Dispose();
                            Clients.Remove(connected);
                            break;
                        }
                    }
                });
            }
        }

        private static void Refresh_Game_Field(NetworkStream stream, Connected_Client connected)
        {
            if (!Game_List.ContainsKey(connected.Code))
            {
                Error_Model error = new Error_Model()
                {
                    Method = "Error",
                    Message = "Error Code"
                };
                stream.WriteStringStream(new Request_Model() { Method = "Error", Data = error }.ToJson());
                return;
            }
            Game game = Game_List[connected.Code];
            stream.WriteStringStream(new Request_Model() { Method = "Refresh_Game_Field", Data = new
            {
                Fields = game.Fields,
                Flag = game.Is_End_Game
            }
            }.ToJson());
        }
        static private void Sing_Up(TcpClient tcp, Client_Model client, out Connected_Client connected)
        {
            NetworkStream stream = tcp.GetStream();
            if (db.Users.Any(user => user.Login == client.Login && user.Password == client.Password))
            {
                if (Clients.Any(c => c.Client.Login == client.Login))
                {
                    Error_Model error1 = new Error_Model()
                    {
                        Method = "Error",
                        Message = "Client is connected"
                    };
                    stream.WriteStringStream(new Request_Model() { Method = "Error", Data = error1 }.ToJson());
                    connected = null;
                    return;
                }
                Users data = db.Users.SingleOrDefault(user => user.Login == client.Login);
                connected = new Connected_Client()
                {
                    Client = client,
                    Tcp = tcp
                };
                Clients.Add(connected);
                stream.WriteStringStream(new Request_Model() { Method = "Sing_Up", Data = data }.ToJson());
                return;
            }
            Error_Model error = new Error_Model()
            {
                Method = "Sing_Up",
                Message = "Login or password is incorrect"
            };
            stream.WriteStringStream(new Request_Model() { Method = "Error", Data = error }.ToJson());
            connected = null;
        }
        static private void Registration(TcpClient tcp, Client_Model client, out Connected_Client connected)
        {
            NetworkStream stream = tcp.GetStream();
            if (db.Users.Any(user => user.Login == client.Login))
            {
                Error_Model error = new Error_Model()
                {
                    Method = "Registration",
                    Message = "User exist"
                };
                stream.WriteStringStream(new Request_Model() { Method = "Error", Data = error }.ToJson());
                connected = null;
                return;
            }
            if (String.IsNullOrWhiteSpace(client.Login) && String.IsNullOrWhiteSpace(client.Password) &&
                client.Password.Length < 4)
            {
                Error_Model error = new Error_Model()
                {
                    Method = "Registration",
                    Message = "Login or Password is incorrect"
                };
                stream.WriteStringStream(new Request_Model() { Method = "Error", Data = error }.ToJson());
                connected = null;
                return;
            }
            connected = new Connected_Client()
            {
                Client = client,
                Tcp = tcp
            };
            Clients.Add(connected);
            db.Users.Add(new Users()
            {
                Login = client.Login,
                Password = client.Password
            });
            db.SaveChanges();
            stream.WriteStringStream(new Request_Model() { Method = "Registration", Data = "OK" }.ToJson());
        }
        static private string Generate_Code()
        {
            string tmp = "1234567890ZYXWVUTSRQPONMLKJIHGFEDCBA";
            string res = "";
            for (int i = 0; i < 4; i++)
            {
                res += tmp[random.Next(0, 34)];
            }
            return res;
        }
        static private void Create_Lobby(NetworkStream stream, Connected_Client client)
        {
            if (!String.IsNullOrWhiteSpace(client.Code) &&
                Game_List.ContainsKey(client.Code) && Game_List[client.Code].Is_End_Game)
                Game_List.Remove(client.Code);
            string code = Generate_Code();
            bool flag = true;
            while (flag)
            {
                foreach (string item in Codes)
                {
                    if (item == code)
                    {
                        code = Generate_Code();
                    }
                }
                flag = false;
            }
            client.Code = code;
            client.Is_Creater = true;
            stream.WriteStringStream(new Request_Model() { Method = "Create_Lobby", Data = code }.ToJson());
        }
        static private void Connect_To_Game(NetworkStream stream, Connected_Client client, string Code)
        {
            if(Clients.Count(c => c.Code == Code) >= 2 || Clients.Count(c => c.Code == Code) == 0)
            {
                Error_Model error = new Error_Model()
                {
                    Method = "Connect_To_Game",
                    Message = "Codes invalide"
                };
                stream.WriteStringStream(new Request_Model() { Method = "Error", Data = error }.ToJson());
                return;
            }
            Game_List.Add(Code, new Game());
            client.Code = Code;
            stream.WriteStringStream(new Request_Model() { Method = "Connect_To_Game", Data = Clients.Single(c => c.Code == Code && c.Client.Login != client.Client.Login).Client.Login }.ToJson());
        }
        private static void Stop_Game(string Code)
        {
            Game_List.Remove(Code);
            foreach (Connected_Client item in Clients.Where(c => c.Code == Code))
            {
                item.Is_Creater = false;
                item.Code = "";
            }
        }
        private static void Send_Click(NetworkStream stream, Connected_Client client, int Tag)
        {
            Game game = Game_List[client.Code];
            if (client.Is_Creater)
            {
                if (game.Flag_Step == Game_Field.X)
                {
                    bool tmp = game.Set_Cell(Tag, Game_Field.X);
                    Cheker(tmp);
                }
                else
                {
                    Error_Model error = new Error_Model()
                    {
                        Method = "Send_Click",
                        Message = "Field is busy"
                    };
                    stream.WriteStringStream(new Request_Model() { Method = "Error", Data = error }.ToJson());
                    return;
                }
            }
            else
            {
                if (game.Flag_Step == Game_Field.O)
                {
                    bool tmp = game.Set_Cell(Tag, Game_Field.O);
                    Cheker(tmp);
                }
                else
                {
                    Error_Model error = new Error_Model()
                    {
                        Method = "Send_Click",
                        Message = "Field is busy"
                    };
                    stream.WriteStringStream(new Request_Model() { Method = "Error", Data = error }.ToJson());
                    return;
                }
            }
            void Cheker(bool tmp)
            {
                Connected_Client Parther = Clients.Single(c => 
                    c.Code == client.Code && c.Client.Login != client.Client.Login);
                string str = "";
                Game_Field tmp_win_field;
                if (tmp)
                    str = "OK";
                else
                    str = "Error";
                if (game.Check_Win(out tmp_win_field))
                {
                    str = "WIN";
                    db.Users.Single(u => u.Login == client.Client.Login).Win_Count++;
                    db.Users.Single(u => u.Login == Parther.Client.Login).Lose_Count++;
                    db.SaveChanges();
                }
                if (game.Check_Draw())
                {
                    str = "DRAW";
                    db.Users.Single(u => u.Login == client.Client.Login).Draw_Count++;
                    db.Users.Single(u => u.Login == Parther.Client.Login).Draw_Count++;
                    db.SaveChanges();
                }
                var request = new Request_Model()
                {
                    Method = "Send_Click",
                    Data = str
                };
                if(str == "WIN" || str == "DRAW")
                {
                    client.Is_Creater = false;
                    Parther.Is_Creater = false;
                    Game_List[client.Code].Is_End_Game = true;
                    request.Method = "End_Game";
                }
                stream.WriteStringStream(request.ToJson());
            }
        }
        private static void Wate_Partner(NetworkStream stream, Connected_Client client)
        {
            if (Clients.Any(c => c != client && c.Code == client.Code))
            {
                stream.WriteStringStream(new Request_Model() { Method = "Wate_Partner", Data = Clients.Single(c => c != client && c.Code == client.Code).Client.Login }.ToJson());
            }
            else
            {
                stream.WriteStringStream(new Request_Model() { Method = "Wate_Partner", Data = "" }.ToJson());
            }
        }
        private static void Wate_Move(NetworkStream stream, Connected_Client client)
        {
            Game game = Game_List[client.Code];
            if (game.Is_End_Game)
            {
                stream.WriteStringStream(new Request_Model() { Method = "End_Game", Data = "WIN" }.ToJson());
                return;
            }
            if ((client.Is_Creater && game.Flag_Step == Game_Field.X) ||
                (!client.Is_Creater && game.Flag_Step == Game_Field.O))
                stream.WriteStringStream(new Request_Model() { Method = "Wate_Move", Data = 1 }.ToJson());
            else
                stream.WriteStringStream(new Request_Model() { Method = "Wate_Move", Data = 0 }.ToJson());
        }
        private static void Get_Info(NetworkStream stream, Connected_Client client)
        {
            var Data = db.Users.Single(u => u.Login == client.Client.Login);
            stream.WriteStringStream(new Request_Model() { Method = "Get_Info", Data = Data }.ToJson());
        }
    }
}
