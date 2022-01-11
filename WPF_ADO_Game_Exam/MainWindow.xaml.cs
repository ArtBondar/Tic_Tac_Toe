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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tic_Tac_Toc_Core;
using Tic_Tac_Toc_Core.Models;
using Tic_Tac_Toc_Core.Game;
using WPF_ADO_Game_Exam.Services;

namespace WPF_ADO_Game_Exam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Tcp_Client.Run(); 
        }
        private void Reg_Sing_Click(object sender, RoutedEventArgs e)
        {
            Client_Model tmp = new Client_Model();
            Client_Model client = new Client_Model()
            {
                Login = Reg_Login.Text,
                Password = Reg_Password.Text
            };
            if (Tcp_Client.Sing_Up(client, out tmp))
            {
                // Вход в систему
                Hide();
                Main_Menu form = new Main_Menu(tmp);
                form.Show();
            }
            else
            {
                exeption.Content = "Invalide Login or Password";
            }
        }

        private void Reg_Reg_Click(object sender, RoutedEventArgs e)
        {
            Client_Model client = new Client_Model()
            {
                Login = Reg_Login.Text,
                Password = Reg_Password.Text
            };
            if (Tcp_Client.Registration(client))
            {
                // Вход в систему
                Hide();
                Main_Menu form = new Main_Menu(client);
                form.Show();
            }
            else
            {
                exeption.Content = "Login busy or password very easy";
            }
        }
    }
}
