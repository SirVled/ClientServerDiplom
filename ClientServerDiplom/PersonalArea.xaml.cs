using MahApps.Metro.Controls;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для PersonalArea.xaml
    /// </summary>
    public partial class PersonalArea : MetroWindow
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
            if (Person.email == null)          
                OperationServer.SendMsgClient(64, 4, Person.login);         
            else
                SetPersonalInfo();

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
                    thisWindow.countProject.Content = $"Количество ваших проектов : {Person.countProject}";
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
            Person.email = null;
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
            if (changeInfoUser)
            {
                if (StyleUIE.regex.IsMatch(emailUser.Text))
                {
                    if (nameUser.Text.Replace(" ", "").Length > 0)
                    {
                        if (lastnameUser.Text.Replace(" ", "").Length > 0)
                        {
                            if (Person.name == null)
                            {
                                OperationServer.SendMsgClient(1024, 6, Person.login, nameUser.Text, lastnameUser.Text, CheckRefImage(refImage.Text), emailUser.Text);
                            }
                            else
                            {
                                OperationServer.SendMsgClient(1024, 7, Person.login, nameUser.Text, lastnameUser.Text, CheckRefImage(refImage.Text), emailUser.Text);
                            }

                            IsEnabledObject(false);
                            SetInfoAboutPerson();
                        }
                        else
                            MessageBox.Show("Введите фамилию!");
                    }
                    else
                        MessageBox.Show("Введите имя!");
                }
                else          
                    MessageBox.Show("Неверный формат введенной почты!");
               
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

        /// <summary>
        /// Навидение на кнопку
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">MouseEnter</param>
        private void HoverButton(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button bt = (sender as Button);
            if (bt.Name.Equals("contextBut"))
            {
                bt.Background = new ImageBrush(new BitmapImage(new Uri("Image/ContexButHover.png", UriKind.Relative)));          
            }
            else if (bt.Name.Equals("strelkaBut"))
            {
                strelkaPopup.BorderBrush = Brushes.DarkBlue;
            }
            else
            {
                LinearGradientBrush gradientBrushBorder = (LinearGradientBrush)bt.BorderBrush;
                gradientBrushBorder.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#FFFFD100");

                if ((LinearGradientBrush)bt.Background != null)
                {
                    LinearGradientBrush linearGradientBackground = (LinearGradientBrush)bt.Background;
                    linearGradientBackground.GradientStops[2].Color = Color.FromArgb(0, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// Leave кнопки
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">MouseLeave</param>
        private void LeaveButton(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button bt = (sender as Button);
            if (bt.Name.Equals("contextBut"))
            {
                bt.Background = new ImageBrush(new BitmapImage(new Uri("Image/ContexBut.png", UriKind.Relative)));
            }
            else if(bt.Name.Equals("strelkaBut"))
            {
                strelkaPopup.BorderBrush = Brushes.Gray;
            }
            else
            {
                LinearGradientBrush gradientBrush = (LinearGradientBrush)bt.BorderBrush;
                gradientBrush.GradientStops[1].Color = Color.FromRgb(0, 0, 0);

                if ((LinearGradientBrush)bt.Background != null)
                {
                    LinearGradientBrush linearGradientBackground = (LinearGradientBrush)bt.Background;
                    linearGradientBackground.GradientStops[2].Color = Color.FromArgb(10, 0, 0, 0);
                }
            }
        }


        /// <summary>
        /// Скрытие всплывающего окна
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void HidePopupPanel(object sender, RoutedEventArgs e)
        {
            strelkaPopup.Visibility = Visibility.Hidden;

            EventHandler handler = (s, es) =>
            {
                contextButBorder.Visibility = Visibility.Visible;
                panelPopup.Visibility = Visibility.Hidden;
            };
            panelPopup.BeginAnimation(MarginProperty, StyleUIE.AnimationObject(panelPopup,TimeSpan.FromSeconds(0.30), new Thickness(this.ActualWidth,10,0,0),handler));
        }

        /// <summary>
        /// Появление всплывающего окна
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void ShowPopupPanel(object sender, RoutedEventArgs e)
        {
            contextButBorder.Visibility = Visibility.Hidden;

            panelPopup.Visibility = Visibility.Visible;
            EventHandler handler = (s, es) =>
            {
                strelkaPopup.Visibility = Visibility.Visible;
            };
            panelPopup.BeginAnimation(MarginProperty, StyleUIE.AnimationObject(panelPopup, TimeSpan.FromSeconds(0.30), new Thickness(this.ActualWidth - (panelPopup.ActualWidth + 5), 10, 0, 0), handler));
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
            Person.image = refImage.Text;
            Person.email = emailUser.Text;
        }

       
    }
}
