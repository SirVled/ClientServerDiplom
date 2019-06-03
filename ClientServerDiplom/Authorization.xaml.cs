using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Authorization : MetroWindow
    {

        public Authorization()
        {
            InitializeComponent();
            thisWindow = this;
        }

        public static Authorization thisWindow { get; set; } // Окно;
        private static PasswordBox[] passwords { get; set; } // Объекты на форме регистрации
        private static TextBox username { get; set; }
        private static TextBox email { get; set; }

        private static Regex regex = new Regex(@"\w"); 

        #region Работа с UI элементами на форме

        /// <summary>
        /// Загрузка окна 
        /// </summary>
        /// <param name="sender">Окно</param>
        /// <param name="e">Загрузка</param>
        private void Start(object sender, RoutedEventArgs e)
        {
            signUpGrid.Margin = new Thickness(signUpGrid.ActualWidth, 0, 0, 0);
            signUpGrid.Visibility = Visibility.Visible;

            passwords = new PasswordBox[2] { signUpPass1TB, signUpPass2TB };
            username = signUpUserTB;
            email = signUpMailTB;

            Application.Current.Exit += (s, es) =>
            {
                if (OperationServer.thread != null)
                    OperationServer.thread.Abort();
                
                if (OperationServer.serverSocket != null)
                {
                    if (Person.thisUser.login != null)
                        OperationServer.SendMsgClient(4, -1, Person.thisUser.login);

                    OperationServer.serverSocket.Close();
                }

            };
        }

        /// <summary>
        /// Переход на панель входа\регистрации
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void ClickSignBut(object sender, RoutedEventArgs e)
        {
            Button sign = (sender as Button);

            if (sign.Name.Equals("signInBut") && sign.FontSize != 22)
            {
                StyleUIE.SetFocusButton(sign, signUpBut);
                StyleUIE.SwapSignPanel(signInGrid, signUpGrid, signInBut, signUpBut);

                signIn.IsEnabled = true;
                signUp.IsEnabled = false;
            }
            else if(sign.Name.Equals("signUpBut") && sign.FontSize != 22)
            {
                StyleUIE.SetFocusButton(sign, signInBut);
                StyleUIE.SwapSignPanel(signUpGrid, signInGrid, signInBut, signUpBut);

                signIn.IsEnabled = false;
                signUp.IsEnabled = true;
            }
        }

        /// <summary>
        /// Получение фокуса у TextBox
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">GotFocus</param>
        private void GotFocusTB(object sender, RoutedEventArgs e)
        {
            Border s = ((sender is TextBox) == true) ? (Border)(sender as TextBox).Parent : (Border)(sender as PasswordBox).Parent;
            s.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8B93B8"));
        }

        /// <summary>
        /// Потеря фокуса у TextBox
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">LostFocus</param>
        private void LostFocusTB(object sender, RoutedEventArgs e)
        {
            Border s = ((sender is TextBox) == true) ? (Border)(sender as TextBox).Parent : (Border)(sender as PasswordBox).Parent;
            s.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6D738E"));
        }

        /// <summary>
        /// Hover кнопки
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">MouseEnter</param>
        private void HoverBut(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as Button).BorderThickness = new Thickness(0, 0, 0, 1);
        }

        /// <summary>
        /// Leave кнопки
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">MouseLeave</param>
        private void LeaveBut(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as Button).BorderThickness = new Thickness(0);
        }

        /// <summary>
        /// Нажатие на окно
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">MouseLeftDown</param>
        private void LostFocusUIE(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (signIn.IsEnabled)
                signIn.Focus();
            else
                signUp.Focus();
        }

        /// <summary>
        /// Вход в личный кабинет
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void SignIn(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            OperationServer.Connected();
            OperationServer.SendMsgClient(256, 1, userNameTB.Text, passwordPB.Password);       
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void SignUp(object sender, RoutedEventArgs e)
        {
            OperationServer.Connected();
            if (CheckValidVisitorAsync())
            {
                //Если данные валидны, то сервер отошлет данные на клиент и регистрация будет успешно завершена;
                OperationServer.SendMsgClient(512, 2, username.Text, email.Text); 
            }
        }

        /// <summary>
        /// Установка правил на ввод данных в TextBox
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">TextInput</param>
        private void TextValidInput(object sender, TextCompositionEventArgs e)
        {
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Открытие окна с восстановлением пароля
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void RecoveryPassword(object sender, RoutedEventArgs e)
        {
            OperationServer.Connected();
            (new SendMailPass()).ShowDialog();
        }
        #endregion

        /// <summary>
        /// Проверяет данные на пригодность к регистрации
        /// </summary>    
        private bool CheckValidVisitorAsync()
        {
            if (!passwords[0].Password.Equals(passwords[1].Password) || passwords[0].Password.Replace(" ", "").Length < 1)
            {
                //   MessageBox.Show("Неверный пароль");
                this.ShowMessageAsync("Неверный пароль", "Пароли не совпадают!");
                return false;
            }

            if (username.Text.Replace(" ", "").Length == 0)
            {
                //      MessageBox.Show("Введите логин!");
                this.ShowMessageAsync("Введите логин!", "Поле с логином пустое!");
                return false;
            }

            if(!StyleUIE.regex.IsMatch(email.Text))
            {
                //MessageBox.Show("Неверно введенный email!");
                this.ShowMessageAsync("Неверно введенный email!", "Неверный формат почты!");
                return false;
            }
            

            return true;
        }

        /// <summary>
        /// Добавление в базу данных данные о новом пользователе
        /// </summary>
        public static void CreateNewPerson()
        {
            thisWindow.Dispatcher.Invoke(new ThreadStart(() =>
            {
                OperationServer.SendMsgClient(512, 3, username.Text , passwords[0].Password, email.Text );
            }));
        }

        /// <summary>
        /// Переход в личный кабинет
        /// </summary>
        public static void GoToPersonalArea()
        {
            thisWindow.Dispatcher.Invoke(new ThreadStart(() =>
            {
                Person.thisUser.login = thisWindow.userNameTB.Text;
                Person.thisUser.listProject = new System.Collections.Generic.List<Project>();
                OperationServer.SendMsgClient(128, 1005, Person.thisUser.login);
                (new PersonalArea()).Show();                     
                thisWindow.Close();
            }));
        }     
    }
}
