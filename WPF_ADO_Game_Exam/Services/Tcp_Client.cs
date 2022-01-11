using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tic_Tac_Toc_Core.Models;
using Tic_Tac_Toc_Core;
using System.Windows;
using Tic_Tac_Toc_Core.Game;
using System.Text.Json;

namespace WPF_ADO_Game_Exam.Services
{
    static public class Tcp_Client
    {
        static private TcpClient client;
        static private NetworkStream stream => client.GetStream();
        static public bool Is_Connected { get; private set; }
        static public void Run()
        {
            if (!Is_Connected)
            {
                client = new TcpClient();
                client.Connect("localhost", 10123);
                client.GetStream().WriteStringStream(Config.AppKey);
            }
        }
        static public bool Sing_Up(Client_Model client, out Client_Model info)
        {
            stream.WriteStringStream(new Request_Model() { Method = "Sing_Up", Data = client }.ToJson());
            var Response = stream.ReadStringStream().FromJson();
            if ((string)Response["Method"] == "Error")
            {
                MessageBox.Show((string)Response["Data"]["Message"]);
                info = null;
                return false;
            }
            info = new Client_Model()
            {
                Id = (int)Response["Data"]["Id"],
                Login = (string)Response["Data"]["Login"],
                Password = (string)Response["Data"]["Password"],
                Win_Count = (int)Response["Data"]["Win_Count"],
                Draw_Count = (int)Response["Data"]["Draw_Count"],
                Lose_Count = (int)Response["Data"]["Lose_Count"]
            };
            return true;
        }
        static public bool Registration(Client_Model client)
        {
            stream.WriteStringStream(new Request_Model() { Method = "Registration", Data = client }.ToJson());
            var Response = stream.ReadStringStream().FromJson();
            if((string)Response["Method"] == "Error")
            {
                MessageBox.Show((string)Response["Data"]["Message"]);
                return false;
            }
            return (string)Response["Data"] == "OK";
        }
        static public void Create_Lobby(out string Code)
        {
            stream.WriteStringStream(new Request_Model() { Method = "Create_Lobby", Data = null }.ToJson());
            var Response = stream.ReadStringStream().FromJson();
            if ((string)Response["Method"] == "Error")
            {
                MessageBox.Show((string)Response["Data"]["Message"]);
                Code = "";
            }
            Code = (string)Response["Data"];
        }
        static public bool Connect_To_Game(string Code, out string Login_Player_2)
        {
            stream.WriteStringStream(new Request_Model() { Method = "Connect_To_Game", Data = Code }.ToJson());
            var Response = stream.ReadStringStream().FromJson();
            if ((string)Response["Method"] == "Error")
            {
                MessageBox.Show((string)Response["Data"]["Message"]);
                Login_Player_2 = "";
                return false;
            }
            Login_Player_2 = (string)Response["Data"];
            return true;
        }
        static public bool Refresh_Game_Field(out List<Game_Field> fields, out bool End_Game)
        {
            End_Game = false;
            while (true)
            {
                stream.WriteStringStream(new Request_Model() { Method = "Refresh_Game_Field", Data = "OK" }.ToJson());
                var Response = stream.ReadStringStream().FromJson();
                
                if ((string)Response["Method"] == "Error")
                {
                    MessageBox.Show((string)Response["Data"]["Message"]);
                    fields = null;
                    return false;
                }
                if((string)Response["Method"] != "Refresh_Game_Field")
                    continue;
                End_Game = (bool)Response["Data"]["Flag"];
                fields = new List<Game_Field>();
                for (int i = 0; i < 9; i++)
                {
                    fields.Add((Game_Field)Response["Data"]["Fields"][i]);
                }
                return true;
            }
        }
        static public bool Send_Click(int tag, out bool Flag)
        {
            stream.WriteStringStream(new Request_Model() { Method = "Send_Click", Data = tag }.ToJson());
            var Response = stream.ReadStringStream().FromJson();
            if ((string)Response["Method"] == "Error")
            {
                MessageBox.Show((string)Response["Data"]["Message"]);
                Flag = false;
                return false;
            }
            if ((string)Response["Method"] == "End_Game" && (string)Response["Data"] == "WIN" || (string)Response["Data"] == "DRAW")
            {
                Flag = true;
                return true;
            }
            Flag = false;
            return (string)Response["Data"] == "OK";
        }
        static public string Wate_Partner()
        {
            while (true)
            {
                stream.WriteStringStream(new Request_Model() { Method = "Wate_Partner", Data = "OK" }.ToJson());
                var Response = stream.ReadStringStream().FromJson();
                if ((string)Response["Data"] != "")
                    return (string)Response["Data"];
                Thread.Sleep(500);
            }
        }
        static public void Wate_Move(out bool Is_End_Game)
        {
            Is_End_Game = false;
            while (true)
            {
                stream.WriteStringStream(new Request_Model() { Method = "Wate_Move", Data = "OK" }.ToJson());
                string str = stream.ReadStringStream();
                var Response = str.FromJson();
                if ((string)Response["Method"] == "Wate_Move" && (int)Response["Data"] == 1)
                    break;
                if ((string)Response["Method"] == "End_Game")
                {
                    Is_End_Game = true;
                    break;
                }
                Thread.Sleep(500);
            }
        }
        static public void Stop()
        {
            Is_Connected = false;
            if (client != null)
            {
                client.Close();
                client.Dispose();
            }
        }
        static public void Get_Info(out Client_Model client)
        {
            while (true)
            {
                stream.WriteStringStream(new Request_Model() { Method = "Get_Info", Data = "OK" }.ToJson());
                var Response = stream.ReadStringStream().FromJson();
                if ((string)Response["Method"] != "Get_Info")
                    continue;
                client = new Client_Model();
                client.Id = (int)Response["Data"]["Id"];
                client.Login = (string)Response["Data"]["Login"];
                client.Password = (string)Response["Data"]["Password"];
                client.Draw_Count = (int)Response["Data"]["Draw_Count"];
                client.Lose_Count = (int)Response["Data"]["Lose_Count"];
                client.Win_Count = (int)Response["Data"]["Win_Count"];
                return;
            }
        }
    }
}
