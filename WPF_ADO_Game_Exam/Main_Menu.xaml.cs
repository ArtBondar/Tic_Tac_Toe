using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Tic_Tac_Toc_Core;
using Tic_Tac_Toc_Core.Models;
using Tic_Tac_Toc_Core.Game;
using System.Net.Sockets;
using WPF_ADO_Game_Exam.Services;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Threading;

namespace WPF_ADO_Game_Exam
{
    /// <summary>
    /// Interaction logic for Main_Menu.xaml
    /// </summary>
    public partial class Main_Menu : Window
    {
        private Game Game;
        private bool Is_Start_Game = false;
        private bool Flag = false;
        private List<Image> Images;
        private List<Game_Field> Fields;
        public Main_Menu(Client_Model login)
        {
            InitializeComponent();
            Refresh_Info(login);
            Create_new_Game_Field();
        }
        private void Create_new_Game_Field()
        {
            Game = new Game();
            Images = new List<Image>();
            int row = 0;
            int colum = 0;
            for (int i = 0; i < 9; i++)
            {
                Image image = new Image()
                {
                    Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}Img\\NULL.png")),
                    Tag = i
                };
                image.MouseDown += Image_MouseDown;
                Grid.SetColumn(image, colum);
                Grid.SetRow(image, row);
                Images.Add(image);
                Game_Fuild.Children.Add(image);
                if (colum == 2)
                {
                    colum = 0;
                    row++;
                }
                else
                    colum++;
            }
        }
        private void Refresh_Info(Client_Model login)
        {
            Aut_Id.Content = login.Id;
            Aut_Login.Content = login.Login;
            Win_Count.Content = login.Win_Count;
            Lose_Count.Content = login.Lose_Count;
            Draw_Count.Content = login.Draw_Count;
        }
        private void Image_MouseDown(object sender, MouseEventArgs e)
        {
            bool end_game;
            if (!Is_Start_Game)
                return;
            if (Tcp_Client.Send_Click((int)(sender as Image).Tag,out Flag))
            {
                Refresh_Game_Fuild();
                Task.Factory.StartNew(() => {
                    Tcp_Client.Wate_Move(out end_game);
                    Dispatcher.Invoke(() =>
                    {
                        Refresh_Game_Fuild();
                    });
                    if (end_game)
                    {
                        Is_Start_Game = false;
                    }
                    return;
                });
            }
            if (Flag)
            {
                Is_Start_Game = false;
                return;
            }
        }
        private void Refresh_Game_Fuild()
        {
            bool end_game;
            Tcp_Client.Refresh_Game_Field(out Fields, out end_game);
            for (int i = 0; i < 9; i++)
            {
                switch (Fields[i])
                {
                    case Game_Field.X:
                        Images[i].Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}Img\\X.png"));
                        break;
                    case Game_Field.O:
                        Images[i].Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}Img\\O.png"));
                        break;
                    case Game_Field.NULL:
                        Images[i].Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}Img\\NULL.png"));
                        break;
                    default:
                        break;
                }
            }
            if (end_game)
            {
                Is_Start_Game = false;
                Dispatcher.Invoke(Clear);
            }
        }
        private void Create_Game_Click(object sender, RoutedEventArgs e)
        {
            string tmp = "";
            Tcp_Client.Create_Lobby(out tmp);
            Room_Code.Content = tmp;
            Room_P1.Content = Aut_Login.Content;
            Room_P2.Content = "Wate the join...";
            IsEnabled = false;
            Task.Factory.StartNew(() =>
            {
                string login = Tcp_Client.Wate_Partner();
                Dispatcher.Invoke(() =>
                {
                    Room_P2.Content = login;
                    Is_Start_Game = true;
                    IsEnabled = true;
                });
                MessageBox.Show("Game is started...");
            });
        }
        private void Join_Click(object sender, RoutedEventArgs e)
        {
            string tmp = "";
            if (!String.IsNullOrWhiteSpace(Game_Code.Text))
            {
                if (Tcp_Client.Connect_To_Game(Game_Code.Text, out tmp))
                {
                    bool tmp1;
                    Is_Start_Game = true;
                    Room_P1.Content = Aut_Login.Content;
                    Room_P2.Content = tmp;
                    Room_Code.Content = Game_Code.Text;
                    Game_Code.Text = "";
                    Task.Factory.StartNew(() => {
                        Tcp_Client.Wate_Move(out tmp1);
                        Dispatcher.Invoke(() => {
                            Refresh_Game_Fuild();
                        });
                    });
                }
                else
                {
                    MessageBox.Show("Error connect UI");
                }
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Tcp_Client.Stop();
        }
        private void Clear()
        {
            Room_P1.Content = "";
            Room_P2.Content = "";
            Room_Code.Content = "";
            Client_Model client;
            Tcp_Client.Get_Info(out client);
            Refresh_Info(client);
            Create_new_Game_Field();
        }
    }
}
