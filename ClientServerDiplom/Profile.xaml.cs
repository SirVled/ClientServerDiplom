using MahApps.Metro.Controls;
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
using System.Windows.Threading;

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : MetroWindow
    {
        public static Person profileUser { get; set; }

        public static Profile thisWindow { get; private set; }

        public Profile(Person user)
        {
            profileUser = user;           
            InitializeComponent();
        }

        /// <summary>
        /// Загрузка окна
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Loaded</param>
        private void Start(object sender, RoutedEventArgs e)
        {
            thisWindow = this;

            try { imageThisUser.Fill = new ImageBrush(new BitmapImage(new Uri(Person.thisUser.image, UriKind.Absolute))); }
            catch { imageThisUser.Fill = new ImageBrush(new BitmapImage(new Uri("https://api.icons8.com/download/bc2e75add07ff32fbbe55b371c9f3a03ee46846a/windows8/PNG/512/Users/user_male-512.png", UriKind.Absolute))); }

            profileUser.listProject = new List<Project>();
            // Получение списков проектов;
            OperationServer.SendMsgClient(128, 1005, profileUser.login, false);
            // Получение информации о пользователеl
            OperationServer.SendMsgClient(128, 4, profileUser.login ,false);
        }

        /// <summary>
        /// Определяет состояние подписки
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void SubOrUnsub(object sender, RoutedEventArgs e)
        {
            Button but = (sender as Button);
            if (but.Content.Equals("Подписаться"))
            {
                OperationServer.SendMsgClient(256, 2003, Profile.profileUser.login, Person.thisUser.login, true);

                IsSubscribe(but, false);
            }
            else
            {
                OperationServer.SendMsgClient(256, 2003, Profile.profileUser.login, Person.thisUser.login, false);

                IsSubscribe(but, true);
            }

            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            but.IsEnabled = false;
            timer.Tick += (s, es) =>
            {
                but.IsEnabled = true;
                timer.Stop();
            };
            timer.Start();
        }

        /// <summary>
        /// Меняет значение кнопки подписаться на отписаться и наоборот
        /// </summary>
        /// <param name="but">Кнопка</param>
        /// <param name="isSub">Состояние подписки</param>
        public static void IsSubscribe(Button but, bool isSub)
        {
            if(isSub)
            {
                but.Content = "Подписаться";
                but.Background = Brushes.DodgerBlue;
            }
            else
            {
                but.Content = "Отписатся";
                but.Background = Brushes.LightCoral;
            }
        }

        /// <summary>
        /// Установка контента
        /// </summary>
        /// <param name="profile">Окно</param>
        /// <param name="person">Профиль пользователя</param>
        public static void SetPanels(Profile profile, Person person)
        {
            profile.Dispatcher.Invoke(new ThreadStart(async () =>
            {
                OperationServer.SendMsgClient(256, 2004, person.login, Person.thisUser.login);

                try
                {
                    profile.imageRec.Fill = new ImageBrush(new BitmapImage(new Uri(person.image, UriKind.Absolute)));
                }
                catch { profile.imageRec.Fill = new ImageBrush(new BitmapImage(new Uri("https://api.icons8.com/download/bc2e75add07ff32fbbe55b371c9f3a03ee46846a/windows8/PNG/512/Users/user_male-512.png", UriKind.Absolute))); }

                profile.loginUser.Content = person.login;
                profile.fioUser.Text = person.lastname + " " + person.name;
                profile.emailUser.Text = person.email;
                profile.levelUser.Content = $"Ур : {person.level}";
                profile.noteUser.Text = person.note;
                profile.countProject.Content = $"Количество проектов : {person.countProject}";
                profile.countLikeUser.Content = person.likes;
                profile.countSub.Content = $"Количество подписчиков : {person.countSub}";

                await PersonalArea.SetStatisticPublicUser(profile.statisticPublic, person);
                SetProjectList(profile, person.listProject);
             
            }));
        }

        /// <summary>
        /// Установка листа с проектами
        /// </summary>
        /// <param name="profile">Окно</param>
        /// <param name="projects">Проекты</param>
        private static void SetProjectList(Profile profile, List<Project> projects)
        {
            int temp = 0;
            StackPanel sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            }; ;

            foreach (var project in projects)
            {                             
                Border border = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    CornerRadius = new CornerRadius(3),
                    Margin = new Thickness(20,0,0,20)
                };
                border.Child = CreatePanelProject(project);

                sp.Children.Add(border);
               
                if(temp % 2 == 1)
                {
                    profile.listProject.Children.Add(sp);
                    sp = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                }

                temp++;
                if(projects.Count % 2 != 0 && temp == projects.Count)
                {
                    profile.listProject.Children.Add(sp);
                }
            }
        }

        /// <summary>
        /// Создает панель для проекта
        /// </summary>
        /// <param name="project">Проект</param>
        /// <returns>Панель</returns>
        private static Grid CreatePanelProject(Project project)
        {
            Grid panel = new Grid
            {    
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10)
            };

            Rectangle rec = new Rectangle
            {
                Height = 58,
                Width = 60,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            try { rec.Fill = new ImageBrush(new BitmapImage(new Uri(project.image, UriKind.Absolute))); }
            catch { rec.Fill = new ImageBrush(new BitmapImage(new Uri(" https://api.icons8.com/download/6f772b46170bd7987130f8a01dbfc2368b95877f/office/PNG/512/Very_Basic/open_folder-512.png", UriKind.Absolute))); }

            Label nameP = new Label
            {
                Content = project.projectSettings.nameProject,
                Margin = new Thickness(65,0,0,0),
                Width = 177,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            StackPanel rating = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(65,26,0,0),              
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            //Рисуем звёзды
            for(int i = 0; i < 5; i++)
            {
                List<Point> points = new List<Point>();
                points.Add(new Point(1, 12));
                points.Add(new Point(6.4, 0));
                points.Add(new Point(11, 12));
                points.Add(new Point(0, 5));
                points.Add(new Point(13, 5));

                rating.Children.Add(new Polygon
                {
                    StrokeThickness = 1,
                    FillRule = FillRule.Nonzero,
                    Points = new PointCollection(points),
                    Fill = Brushes.Gray
                });
            }
            YourProject.DrawStarsForProject(rating, project.projectSettings.ratingProject);

            TextBlock countVoteP = new TextBlock
            {
                Margin = new Thickness(0,63,0,0),
                TextWrapping = TextWrapping.Wrap,
                Width = 60,
                Text = $"Кол-во голосов: {project.countVote}",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            StackPanel spKat = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(65, 58, 0, 0),
                Width = 179,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            spKat.Children.Add(new TextBlock { Text = "Категория : " });
            spKat.Children.Add(new TextBlock
            {
                Text = project.viewApplication,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5,0,0,0)
            });

            ScrollViewer noteP = new ScrollViewer
            {
                Margin = new Thickness(65, 79, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            noteP.Content = new TextBlock
            {
                Text = project.note,
                TextWrapping = TextWrapping.Wrap,
                Width = 163,
                MinHeight = 50
            };

            StackPanel spRatingPanel = CreatePanelRating(thisWindow);

            Button buttLoad = new Button
            {
                BorderThickness = new Thickness(1),
                Width = 32,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0,0,10,5),
                Background = new ImageBrush(new BitmapImage(new Uri("https://image.flaticon.com/icons/png/512/12/12196.png", UriKind.Absolute)))
            };

            panel.Children.Add(rec);
            panel.Children.Add(nameP);
            panel.Children.Add(rating);
            panel.Children.Add(countVoteP);
            panel.Children.Add(spKat);
            panel.Children.Add(noteP);
            panel.Children.Add(spRatingPanel);
            panel.Children.Add(buttLoad);
            return panel;
        }

        /// <summary>
        /// Панель с рейтингом
        /// </summary>
        /// <param name="profile">Окно</param>
        /// <returns>Панель</returns>
        private static StackPanel CreatePanelRating(Profile profile)
        {
            StackPanel spMain = new StackPanel
            {
                Margin = new Thickness(10, 139, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            spMain.Children.Add(new Label
            {
                Content = "Оцените проект",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            });

            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal }; 

            for(int i = 0; i < 5; i++)
            {
                Button butRating = new Button
                {
                    Content = i+1,
                    Width = 30,
                    Height = 30,
                    FontFamily = new FontFamily("Candara"),
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF110D2E")),
                    Style = profile.styleButton.Style
                };

                sp.Children.Add(butRating);
            }

            spMain.Children.Add(sp);
            return spMain;
        }

    }
}
