using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

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
            if (Person.thisUser.email == null)          
                OperationServer.SendMsgClient(64, 4, Person.thisUser.login);         
            else
                SetPersonalInfo();

            loginUser.Content = Person.thisUser.login;

            
        }


        /// <summary>
        /// Устанавливает полученную информацию из базы в Textbox-ы
        /// </summary>
        public static void SetPersonalInfo()
        {
            try
            {
                thisWindow.Dispatcher.Invoke(new ThreadStart(async () =>
                {
                    thisWindow.nameUser.Text = Person.thisUser.name;
                    thisWindow.lastnameUser.Text = Person.thisUser.lastname;
                    thisWindow.levelUser.Content = "Level : " + Person.thisUser.level;
                    thisWindow.countLikeUser.Content = Person.thisUser.likes;
                    thisWindow.emailUser.Text = Person.thisUser.email;
                    thisWindow.noteUser.AppendText(Person.thisUser.note);

                    thisWindow.countProject.Content = $"Количество ваших проектов : {Person.thisUser.countProject}";
                    try
                    {
                        thisWindow.image.Fill = new ImageBrush(new BitmapImage(new Uri(Person.thisUser.image, UriKind.Absolute)));
                        thisWindow.refImage.Text = Person.thisUser.image;
                    }
                    catch { thisWindow.image.Fill = Brushes.Gray; }
                    thisWindow.IsEnabledObject(false);

                   await SetStatisticPublicUser();
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
            OperationServer.SendMsgClient(64,5, Person.thisUser.login);
        }

        /// <summary>
        /// Выход в окно входа
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void LogoutUser(object sender, RoutedEventArgs e)
        {
            OperationServer.SendMsgClient(128, -1, Person.thisUser.login);
            Person.thisUser.email = null;
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
                            /// Если пользователь впевые вводит данные 
                            
                            string note = new TextRange(noteUser.Document.ContentStart, noteUser.Document.ContentEnd).Text;
                            if (Person.thisUser.name == null)
                            {
                                OperationServer.SendMsgClient(1024, 6, Person.thisUser.login, nameUser.Text, lastnameUser.Text, CheckRefImage(refImage.Text), emailUser.Text, note);
                            }
                            else
                            {
                                OperationServer.SendMsgClient(1024, 7, Person.thisUser.login, nameUser.Text, lastnameUser.Text, CheckRefImage(refImage.Text), emailUser.Text, note);
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
            panelPopup.BeginAnimation(MarginProperty, StyleUIE.AnimationObject(panelPopup, TimeSpan.FromSeconds(0.20), new Thickness(this.ActualWidth - (panelPopup.ActualWidth + 5), 10, 0, 0), handler));
        }

        /// <summary>
        /// Отображение новостной ленты
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void ShowFeedWindow(object sender, RoutedEventArgs e)
        {
            (new FeedPublic()).Show();
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
            Person.thisUser.name = nameUser.Text;
            Person.thisUser.lastname = lastnameUser.Text;
            Person.thisUser.image = refImage.Text;
            Person.thisUser.email = emailUser.Text;
            Person.thisUser.note = new TextRange(noteUser.Document.ContentStart, noteUser.Document.ContentEnd).Text;
        }

        /// <summary>
        /// Устанавливает статистику публикаций пользователя за год
        /// </summary>
        private static Task SetStatisticPublicUser()
        {
            thisWindow.statisticPublic.Children.Add(SetMonthPublic(DateTime.Now.AddMonths(1)));

            List<StackPanel> listPanelWeek = new List<StackPanel>();
           
            for(int i = 0; i < 7; i++)
            {
                StackPanel sp = new StackPanel{ Orientation = Orientation.Horizontal , Margin = new Thickness(0,3,0,0)};
                listPanelWeek.Add(sp);
                thisWindow.statisticPublic.Children.Add(sp);
            }

            /// Отнимаем у текущей даты 52 недели
            DateTime dateYear = DateTime.Now.AddDays(-364);

            int maxCount = 0;
            Dictionary<string, int> countDist = new Dictionary<string, int>();
            for (int i = 0; i < Person.thisUser.listProject.Count; i++)
            {
                for (int j = i; j < Person.thisUser.listProject.Count; j++)
                {
                    if (Person.thisUser.listProject[i].projectSettings.dateAddingProject !=
                        Person.thisUser.listProject[j].projectSettings.dateAddingProject)
                    {
                        countDist.Add(Person.thisUser.listProject[i].projectSettings.dateAddingProject, j - i);
                        if (maxCount < j - i)
                            maxCount = j - i;

                        i = j - 1;
                        break;
                    }
                    if (j == Person.thisUser.listProject.Count - 1)
                    {
                        countDist.Add(Person.thisUser.listProject[i].projectSettings.dateAddingProject, j - i + 1);
                        if (maxCount < j - i + 1)
                            maxCount = j - i + 1;
                        i = j;
                    }
                }
            }

            // Заполняем прошедшую статистику за 52 недели 
            for (int i = 0; i < 52; i++)
            {
                for(int j = 0; j < 7; j++)
                {
                    dateYear = dateYear.AddDays(1);
                    if (countDist.ContainsKey(dateYear.ToString("dd-MM-yyyy")))                
                        listPanelWeek[j].Children.Add(CreateBlockDate(dateYear, maxCount, countDist[dateYear.ToString("dd-MM-yyyy")]));                 
                    else
                        listPanelWeek[j].Children.Add(CreateBlockDate(dateYear, maxCount));
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Устанавливает панель с месяцами у статисткики с публикациями 
        /// </summary>
        /// <param name="date">Дата с которой идёт отчет месяцев</param>
        /// <returns>Панель</returns>
        private static StackPanel SetMonthPublic(DateTime date)
        {
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,5,0,5) };

            
            for(int i = 0; i < 12; i++)
            {
                sp.Children.Add(new TextBlock
                {
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    FontFamily = new System.Windows.Media.FontFamily("Candara"),
                    Text = date.ToString("MMMM"),
                    Margin = new Thickness(20,0,0,0)
                });
                date = date.AddMonths(1);
            }

            return sp;
        }

        /// <summary>
        /// Создание блока с публикациями для статистики
        /// </summary>
        /// <param name="date">Текущая дата в статистике</param>
        /// <param name="maxCountPublic">Максимальное количество публикаций за один день</param>
        /// <param name="tempPublic">Количество публикаций в этот день</param>
        /// <returns>Блок</returns>
        private static Rectangle CreateBlockDate(DateTime date, int maxCountPublic , int tempPublic = 0)
        {
            Rectangle rec = new Rectangle
            {             
                Width = 10,
                Height = 10,
                Opacity = 0.75,
                Margin = new Thickness(2, 0, 0, 0)
            };


            StackPanel sp = new StackPanel();

            TextBlock countPub = new TextBlock
            {
                FontSize = 11,
              //  FontFamily = new System.Windows.Media.FontFamily("Times new Roman"),
                FontWeight = FontWeights.UltraBold,
                Text = (tempPublic != 0) ? $"{tempPublic} публикаций на " : "Нет публикаций на "
            };

            sp.Children.Add(countPub);
            sp.Children.Add(new TextBlock { Text = date.ToString($"d {MouthName(date.ToString("MMMM"))} yyyy")});

            ToolTip tt = new ToolTip { Content = sp };
            ToolTipService.SetShowDuration(tt, 0);
            rec.ToolTip = tt;

            // Если есть публикации в этот день
            if (tempPublic != 0)
            {
                double opacity = 0;
                rec.Fill = SetPublicColorStatistic(((double)tempPublic / maxCountPublic) * 100, out opacity);
                rec.Opacity = opacity;
                
            }
            else
            {
                rec.Fill = Brushes.LightGray;
            }
            return rec;
        }

        /// <summary>
        /// Устанавливает цвет у блока с статистикой по его значению
        /// </summary>
        /// <param name="percent">Процентное соотношение максимума / и текщего</param>
        /// <returns>Цвет блока</returns>
        private static SolidColorBrush SetPublicColorStatistic(double percent, out double opacity)
        {
            if (percent < 20)
                opacity = 0.25;
            else if (percent >= 20 && percent < 40)
                opacity = 0.35;
            else if (percent >= 40 && percent < 60)
                opacity = 0.55;
            else if (percent >= 60 && percent < 80)
                opacity = 0.80;
            else
                opacity = 1;

            return Brushes.DarkGreen;
        }

        private static string MouthName(string mouth)
        {
            switch(mouth)
            {
                case "Январь":
                    return "Января";
                case "Февраль":
                    return "Февраля";
                case "Март":
                    return "Марта";
                case "Апрель":
                    return "Апреля";
                case "Май":
                    return "Мая";
                case "Июнь":
                    return "Июня";
                case "Июль":
                    return "Июля";
                case "Август":
                    return "Августа";
                case "Сентябрь":
                    return "Сентября";
                case "Октябрь":
                    return "Октября";
                case "Ноябрь":
                    return "Ноября";
                case "Декабрь":
                    return "Декарбря";
            }
            return null;
        }
    
    }
}
