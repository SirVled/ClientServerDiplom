using MahApps.Metro.Controls;
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
using System.Windows.Threading;

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для FeedPublic.xaml
    /// </summary>
    public partial class FeedPublic : MetroWindow
    {
        public FeedPublic()
        {
            InitializeComponent();
            thisWindow = this;

            listTopProject = new List<Person>();
            listInterestingPeople = new List<Person>();
        }

        public static FeedPublic thisWindow { get; set; }

        public static List<Person> listSearchPeople { get; set; } // Список людей которые находятся в списке поиска; 
        public static List<Person> listTopProject { get; private set; } // Список проектов людей которые находятся в топе; 
        public static List<Person> listInterestingPeople { get; private set; } // Список людей который возможно будут интересны;

        #region События на ui элементах
        /// <summary>
        /// Загрузка окна
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Loaded</param>
        private void Start(object sender, RoutedEventArgs e)
        {

            // Получение списока проектов с лучшим рейтингом
            OperationServer.SendMsgClient(32, 2001);

     

            levelUser.Content = $"Level : {Person.thisUser.level}";
            nameUserLB.Text = Person.thisUser.login;
            try { imageUser.Fill = new ImageBrush(new BitmapImage(new Uri(Person.thisUser.image, UriKind.Absolute))); }
            catch { }
        }

        /// <summary>
        /// Скрыть или отобразить окно с меню
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void ShowOrHidePanel(object sender, RoutedEventArgs e)
        {
            Button but = (sender as Button);
            RotateTransform rotate = new RotateTransform();

            if (panelMoveProg.IsEnabled)
            {
                panelMoveProg.IsEnabled = false;

                EventHandler @event = (s,es) =>
                {
                    panelMoveProg.IsEnabled = true;
                };

                if (panelMoveProg.Height != 10)
                {
                    rotate.BeginAnimation(RotateTransform.AngleProperty, StyleUIE.DoubleAnimation(180, TimeSpan.FromSeconds(0.55), 360));
                    panelMoveProg.BeginAnimation(HeightProperty, StyleUIE.DoubleAnimation(180, TimeSpan.FromSeconds(0.55), 10, @event));
                }
                else
                {
                    rotate.BeginAnimation(RotateTransform.AngleProperty, StyleUIE.DoubleAnimation(0, TimeSpan.FromSeconds(0.5), 180));
                    panelMoveProg.BeginAnimation(HeightProperty, StyleUIE.DoubleAnimation(10, TimeSpan.FromSeconds(0.5), 180, @event));
                }
                but.RenderTransform = rotate;
            }
            else
            {
                panelMoveProg.IsEnabled = true;              
                rotate.BeginAnimation(RotateTransform.AngleProperty,null);
                panelMoveProg.BeginAnimation(HeightProperty, null);               
            }
           
        }

        /// <summary>
        /// Навидение на кнопку
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">MouseEnter</param>
        private void HoverButton(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button bt = (sender as Button);
            if (bt.Name.Equals("searchButton"))
            {
                bt.BorderThickness = new Thickness(1);
            }
            else
            {
                LinearGradientBrush gradientBrushBorder = (LinearGradientBrush)bt.BorderBrush;
                gradientBrushBorder.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#FFFFD100");
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

            if (bt.Name.Equals("searchButton"))
            {
                bt.BorderThickness = new Thickness(0);
            }
            else
            {
                LinearGradientBrush gradientBrush = (LinearGradientBrush)bt.BorderBrush;
                gradientBrush.GradientStops[1].Color = Color.FromRgb(0, 0, 0);
            }
        }

        /// <summary>
        /// Переход на следующие окно
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void MoveWindow(object sender, RoutedEventArgs e)
        {
            Button but = (sender as Button);

            if (but.Name.Equals("persArea"))
                (new PersonalArea()).Show();
            else if (but.Name.Equals("persProj"))
                (new YourProject()).Show();
            else
            {
                OperationServer.SendMsgClient(128, -1, Person.thisUser.login);
                Person.thisUser.email = null;
                (new Authorization()).Show();
            }

            Close();
        }

        /// <summary>
        /// Поиск людей
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void SearchPeople(object sender, RoutedEventArgs e)
        {
            if (searchTB.Text.Trim().Length > 0)
            {
                searchListBox.ItemsSource = null;
                searchNull.Visibility = Visibility.Hidden;

                listSearchPeople = new List<Person>();
                (sender as Button).IsEnabled = false;
                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
                timer.Tick += (s, es) =>
                {
                    (sender as Button).IsEnabled = true;
                    timer.Stop();
                };
                timer.Start();

                OperationServer.SendMsgClient(512, 2000, searchTB.Text);
            }
        }

        #endregion

        #region Поиск

        /// <summary>
        /// Заполняем ListBox людьми который были найдены по введенной строке
        /// </summary>
        /// <param name="listFind">Список людей</param>
        public static void SetFindsPeople(List<Person> listFind)
        {           
            listFind.Remove(listFind.FirstOrDefault(f => f.login == Person.thisUser.login));
            if (listFind.Count > 0)
            {
                List<Grid> stackPanels = new List<Grid>();

                foreach (var item in listFind)
                {
                    Grid gridUser = CreatePanelGridUser(item);
                    stackPanels.Add(gridUser);
                }

                thisWindow.searchListBox.ItemsSource = stackPanels;
            }
            else
                thisWindow.searchNull.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Создание столбца с информацией о пользователе (поиск)
        /// </summary>
        /// <param name="item">Человек</param>
        /// <returns>Столбец</returns>
        private static Grid CreatePanelGridUser(Person item)
        {
            Grid userPanel = new Grid
            {
                Margin = new Thickness(3,10,0,10),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Rectangle imageUser = new Rectangle
            {
                Width = 65,
                Height = 65,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            try { imageUser.Fill = new ImageBrush(new BitmapImage(new Uri(item.image, UriKind.Absolute))); }
            catch { imageUser.Fill = new ImageBrush(new BitmapImage(new Uri("https://api.icons8.com/download/bc2e75add07ff32fbbe55b371c9f3a03ee46846a/windows8/PNG/512/Users/user_male-512.png", UriKind.Absolute))); }      

            userPanel.Children.Add(imageUser);
            userPanel.Children.Add(CreatePanelInfoForSearchPeople(item));

            return userPanel;
        }

        /// <summary>
        /// Установка информации о пользователе (поиск)
        /// </summary>
        /// <param name="item">Человек</param>
        /// <returns>Столбец с информацией</returns>
        private static StackPanel CreatePanelInfoForSearchPeople(Person item)
        {
            StackPanel spInfo = new StackPanel { Margin = new Thickness(70, 0, 0, 0) };

            TextBlock nameUser = new TextBlock
            {
                Text = item.login,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                Width = 130,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            StackPanel spLevelUser = new StackPanel { Orientation = Orientation.Horizontal };
            spLevelUser.Children.Add(PanelInfo($"Ур: {item.level} ", "https://avatars.mds.yandex.net/get-pdb/1043736/19fcd063-dc04-4bc5-881a-80d86be2c713/orig"));
            spLevelUser.Children.Add(PanelInfo($"{0}", "http://gsm.meteolk.ru/img/reg.png", 3));

            spInfo.Children.Add(nameUser);
            spInfo.Children.Add(spLevelUser);
            spInfo.Children.Add(PanelInfo($"Проекты : {item.countProject} ", "https://api.icons8.com/download/6f772b46170bd7987130f8a01dbfc2368b95877f/office/PNG/512/Very_Basic/open_folder-512.png"));        

            return spInfo;
        }

        /// <summary>
        /// Заполнение информации о пользователе (поиска)
        /// </summary>
        /// <param name="text">Текст который будет отображатся</param>
        /// <param name="sourse">Ссылка на картинку</param>     
        /// <param name="marginLeft">Отступ с левого края</param>  
        /// <returns>Столбец с информацией</returns>
        private static StackPanel PanelInfo(string text, string sourse , int marginLeft = 0)
        {
            StackPanel info = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment= VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            info.Children.Add(new TextBlock
            {
                Text = text,
                Margin = new Thickness(marginLeft, 1, 0, 0),
                FontFamily = new FontFamily("Segoe WP Semibold"),
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5100DE")),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            });

            info.Children.Add(new Rectangle
            {
                Width = 20,
                Height = 20,
                Fill = new ImageBrush(new BitmapImage(new Uri(sourse, UriKind.Absolute))),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            });

            return info;
        }

        #endregion

        #region Топ проектов

        /// <summary>
        /// Заполняем ListBox проектами у которых самый высокий рейтинг
        /// </summary>
        /// <param name="listFind">Список проектов</param>
        public static void SetTopProject(List<Person> listFind)
        {     
            List<Grid> stackPanels = new List<Grid>();

            foreach (var item in listFind)
            {
                Grid gridUser = CreatePanelGridProject(item);
                stackPanels.Add(gridUser);
            }

            thisWindow.topProject.ItemsSource = stackPanels;     
        }

        /// <summary>
        /// Создание столбца с информацией о проекте (топ проектов)
        /// </summary>
        /// <param name="item">Человек с проектом</param>
        /// <returns>Столбец</returns>
        private static Grid CreatePanelGridProject(Person item)
        {
            Grid userPanel = new Grid
            {
                Margin = new Thickness(3,5,0,0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Ellipse image = new Ellipse
            {
                Width = 25,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            try { image.Fill = new ImageBrush(new BitmapImage(new Uri(item.project.image, UriKind.Absolute))); }
            catch { image.Fill = new ImageBrush(new BitmapImage(new Uri("https://api.icons8.com/download/6f772b46170bd7987130f8a01dbfc2368b95877f/office/PNG/512/Very_Basic/open_folder-512.png", UriKind.Absolute))); }

            userPanel.Children.Add(image);
            userPanel.Children.Add(CreatePanelInfoForProject(item));

            return userPanel;
        }

        /// <summary>
        /// Установка информации о проекте (топ проектов)
        /// </summary>
        /// <param name="item">Человек с проектом</param>
        /// <returns>Столбец с информацией</returns>
        private static StackPanel CreatePanelInfoForProject(Person item)
        {
            StackPanel spInfo = new StackPanel { Width = 185, Margin = new Thickness(30, 0, 0, 0) };

            #region sp1
            StackPanel sp1 = new StackPanel { Orientation = Orientation.Horizontal };           
            TextBlock nameProj = new TextBlock
            {
                Text = item.project.projectSettings.nameProject,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            sp1.Children.Add(nameProj);
            sp1.Children.Add(new TextBlock
            {
                Margin = new Thickness(10,0,0,0),
                FontSize = 10,
                Text = "Создатель :"
            });
            #endregion

            #region sp2          
            StackPanel sp2 = new StackPanel { Orientation = Orientation.Horizontal };
            TextBlock rating = new TextBlock
            {
                Text = $"Рейтинг : {item.project.projectSettings.ratingProject}",         
            };
            sp2.Children.Add(rating);
            sp2.Children.Add(new TextBlock
            {
                FontSize = 10,
                Text = item.login,
                TextDecorations = TextDecorations.Underline,
                TextAlignment = TextAlignment.Right,
                Width = 95
            });
            #endregion

            spInfo.Children.Add(sp1);
            spInfo.Children.Add(sp2);
            spInfo.Children.Add(new TextBlock
            {
                Text = (!item.project.note.Equals("null")) ?  item.project.note : "",
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.Gray,
                MaxHeight = 40
            });

            return spInfo;
        }


        #endregion

        #region Рандом людей
        /// <summary>
        /// Отображение людей
        /// </summary>
        /// <param name="rndPeople">Список людей</param>
        public static void PanelRandomPeople(List<Person> rndPeople)
        {
            List<StackPanel> stackPanels = new List<StackPanel>();

            foreach (var item in rndPeople)
            {
                StackPanel panelUser = CreatePanelGridRndPeople(item);
                stackPanels.Add(panelUser);
            }

            thisWindow.interestingLB.ItemsSource = stackPanels;
        }
        /// <summary>
        /// Создание мини колонки с человеком
        /// </summary>
        /// <param name="item">Человек</param>
        /// <returns>Колонка</returns>
        private static StackPanel CreatePanelGridRndPeople(Person item)
        {
            StackPanel spRnd = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Ellipse el = new Ellipse
            {
                Width = 25,
                Height = 25
            };

            try { el.Fill = new ImageBrush(new BitmapImage(new Uri(item.image, UriKind.Absolute))); }
            catch { el.Fill = new ImageBrush(new BitmapImage(new Uri("https://api.icons8.com/download/bc2e75add07ff32fbbe55b371c9f3a03ee46846a/windows8/PNG/512/Users/user_male-512.png", UriKind.Absolute))); }

            StackPanel sp = new StackPanel { Margin = new Thickness(5,0,0,0) };

            sp.Children.Add(new TextBlock
            {
                Text = item.login,
                FontFamily = new FontFamily("Segoe UI Semibold"),
            });
            StackPanel sp2 = new StackPanel { Orientation = Orientation.Horizontal };
            sp2.Children.Add(new TextBlock
            {
                Text = $"Ур : {item.level}",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5100DE")),
            });
            sp2.Children.Add(new Rectangle
            {
                Width = 15,
                Height = 15,
                Fill = new ImageBrush(new BitmapImage(new Uri("https://avatars.mds.yandex.net/get-pdb/1043736/19fcd063-dc04-4bc5-881a-80d86be2c713/orig", UriKind.Absolute)))
            });

            sp.Children.Add(sp2);
            spRnd.Children.Add(el);
            spRnd.Children.Add(sp);
            return spRnd;
        }
        #endregion
    }
}
