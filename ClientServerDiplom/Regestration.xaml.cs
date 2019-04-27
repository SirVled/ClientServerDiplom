using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для Regestration.xaml
    /// </summary>
    public partial class Regestration : Window
    {
        public Regestration()
        {
            InitializeComponent();

            regs = this;
        }

        private static Regestration regs { get; set; } // Окно с регистрацией; 
        private static string loginU { get; set; } // Введенный логин;
        private static string passwordU { get; set; } // Введенный пароль;

        /// <summary>
        /// Возращение на форму с логином и паролем
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void BackToEscape(object sender, RoutedEventArgs e)
        {
            ExitRegisterForm();
        }

        /// <summary>
        /// Выход с формы регистрации
        /// </summary>
        private void ExitRegisterForm()
        {
            Close();
        }

        /// <summary>
        /// Добавление нового пользователя в базу данных
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void CreateNewVisitor(object sender, RoutedEventArgs e)
        {

            loginU = RegLogin.Text;
            passwordU = Pass1.Password;
            CheckValidVisitor();
        }

        /// <summary>
        /// Проверяет данные на пригодность к регистрации
        /// </summary>    
        private void CheckValidVisitor()
        {
            if (!Pass1.Password.Equals(Pass2.Password) || Pass1.Password.Replace(" ","").Length < 1)
            {
                MessageBox.Show("Неверный пароль");
            }
            else
            {
                if (RegLogin.Text.Replace(" ", "").Length > 0)
                {
                    //Если данные валидны, то сервер отошлет данные на сервер и регистрация будет успешно завершена;
                    OperationServer.SendMsgClient(512, 2, RegLogin.Text);
                }
                else
                    MessageBox.Show("Введите логин!");
            }
        }

        /// <summary>
        /// Добавление в базу данных данные о новом пользователе
        /// </summary>
        public static void CreateNewPerson()
        {
            OperationServer.SendMsgClient(256, 3, loginU, passwordU);

            regs.Dispatcher.Invoke(new ThreadStart(() => { 
                regs.Close();              
            }));
           
        }
      
    }
}
