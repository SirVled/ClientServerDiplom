using System;
using System.Collections.Generic;
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

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для PersonalArea.xaml
    /// </summary>
    public partial class PersonalArea : Window
    {

        private static PersonalArea thisWindow; // Текущее окно;
        private bool changeInfoUser = false; // Проверка на изменение данных пользователя;

        public PersonalArea()
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
            OperationServer.SendMsgClient(64, 4, Person.login);
            loginUser.Content = Person.login;      
        }


        /// <summary>
        /// Устанавливает полученную информацию из базы в Textbox-ы
        /// </summary>
        public static void SetPersonalInfo()
        {
            try
            {
                thisWindow.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    thisWindow.nameUser.Text = Person.name;
                    thisWindow.lastnameUser.Text = Person.lastname;
                    thisWindow.levelUser.Content = "Level : " + Person.level;
                    thisWindow.countLikeUser.Content = Person.likes;
                    thisWindow.emailUser.Text = Person.email;

                    try
                    {
                        thisWindow.image.Fill = new ImageBrush(new BitmapImage(new Uri(Person.image, UriKind.Absolute)));
                        thisWindow.refImage.Text = Person.image;
                    }
                    catch { thisWindow.image.Fill = Brushes.Gray; }


                    thisWindow.IsEnabledObject(false);
                }));
            }
            catch(Exception ex) { MessageBox.Show("SetPersonalInfo : " + ex.Message); }
        }

        #region Нажатия на кнопки и другие действие привязанные к объктам на форме

        /// <summary>
        /// Переход к окну истории пользователя
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void GoToHistoryWind(object sender, RoutedEventArgs e)
        {
            OperationServer.SendMsgClient(64,5,Person.login);
        }

        /// <summary>
        /// Выход в окно входа
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void LogoutUser(object sender, RoutedEventArgs e)
        {

            OperationServer.SendMsgClient(128, -1, Person.login);
            (new Authorization()).Show();
            Close();
        }

        /// <summary>
        /// Разблокировка формы с информацией 
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void ChangeInfo(object sender, RoutedEventArgs e)
        {
            IsEnabledObject(true);
        }

        /// <summary>
        /// Отправка на сервер информацию о пользователе
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void ConfirmInfoUser(object sender, RoutedEventArgs e)
        {
            if(nameUser.Text.Replace(" ","").Length > 0 && 
               lastnameUser.Text.Replace(" ", "").Length > 0 && 
               emailUser.Text.Replace(" ","").Length > 0 && changeInfoUser)
            {
                if (Person.name == null)
                {
                    OperationServer.SendMsgClient(1024, 6, Person.login , nameUser.Text, lastnameUser.Text, CheckRefImage(refImage.Text), emailUser.Text);             
                }
                else
                {
                    OperationServer.SendMsgClient(1024, 7, Person.login, nameUser.Text, lastnameUser.Text, CheckRefImage(refImage.Text), emailUser.Text);
                }

                IsEnabledObject(false);
                SetInfoAboutPerson();
            }
        } 

        /// <summary>
        /// Изменение в данных пользователя
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">TextChanged</param>
        private void InfoChanged(object sender, TextChangedEventArgs e)
        {
            confirmInfoUser.IsEnabled = true;
            changeInfoUser = true;
        }

        /// <summary>
        /// Переход на форму с проектами
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void GoToYourProject(object sender, RoutedEventArgs e)
        {
            (new YourProject()).Show();
            Close();
        }

        #endregion

        /// <summary>
        /// Читает ссылку и возращает её
        /// </summary>
        /// <param name="reference">Ссылка картинки</param>
        /// <returns>Ссылку</returns>
        private string CheckRefImage(string reference)
        {
            try
            {
                image.Fill = new ImageBrush(new BitmapImage(new Uri(reference, UriKind.Absolute)));
                return reference;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Блокируем форму ввода данных
        /// </summary>
        /// <param name="isEnebled">Состояние блокировки формы</param>
        private void IsEnabledObject(bool isEnebled)
        {         
            foreach(UIElement panelInfo in infoPanel.Children)
            {
                panelInfo.IsEnabled = isEnebled;
            }
            infoPanel.Children[infoPanel.Children.Count - 1].IsEnabled = true;
            confirmInfoUser.IsEnabled = false;
            changeUserInfo.IsEnabled = !isEnebled;
        }

        /// <summary>
        /// Устанавливаем новую информацию
        /// </summary>
        private void SetInfoAboutPerson()
        {
            Person.name = nameUser.Text;
            Person.lastname = lastnameUser.Text;
            // Person.image =
            Person.email = emailUser.Text;
        }

        
    }
}
