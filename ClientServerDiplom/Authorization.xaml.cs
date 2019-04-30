using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Authorization : Window
    {

        private static Authorization thisWindow; // Окно;

        public Authorization()
        {
            InitializeComponent();
            thisWindow = this;
        }

        /// <summary>
        /// Загрузка окна 
        /// </summary>
        /// <param name="sender">Окно</param>
        /// <param name="e">Загрузка</param>
        private void Start(object sender, RoutedEventArgs e)
        {         
            Application.Current.Exit += (s, es) =>
            {
                if (OperationServer.thread != null)
                   OperationServer.thread.Abort();

                if (OperationServer.serverSocket != null)
                {
                    if(Person.login != null)
                        OperationServer.SendMsgClient(4, -1, Person.login);

                    OperationServer.serverSocket.Close();
                }
               
            };         
        }

        /// <summary>
        /// Получение фокуса в TextBox
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">GotFocus</param>
        private void GotFocusLoginLabel(object sender, RoutedEventArgs e)
        {
            TextBox login = (sender as TextBox);

            login.Foreground = Brushes.Black;
            login.Text = string.Empty;

            login.GotFocus -= GotFocusLoginLabel;
        }

        /// <summary>
        /// Переход на форму регистрации
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void ShowWindowRegestration(object sender, RoutedEventArgs e)
        {
            OperationServer.Connected();

            Regestration reg = new Regestration();       
            reg.ShowDialog();
            
        }

        /// <summary>
        /// Вход в аккаунт 
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void GoEnter(object sender, RoutedEventArgs e)
        {
            OperationServer.Connected();
            OperationServer.SendMsgClient(256, 1, loginTb.Text, passwordPb.Password);
            Person.login = loginTb.Text;
        }     

       /// <summary>
       /// Переход в личный кабинет
       /// </summary>
        public static void GoToPersonalArea()
        {        
            thisWindow.Dispatcher.Invoke(new ThreadStart(() =>
            {              
                (new PersonalArea()).Show();
                Person.listProject = new System.Collections.Generic.List<Project>();
                OperationServer.SendMsgClient(128, 1005, Person.login);
                thisWindow.Close();
            }));
        }
    }
}
