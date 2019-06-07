using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static ClientServerDiplom.Project;

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для YourProject.xaml
    /// </summary>
    public partial class YourProject : MetroWindow
    {
        public YourProject()
        {
            InitializeComponent();
            thisWindow = this;
        }

        private static ObservableCollection<MyItemProject> myItems = new ObservableCollection<MyItemProject>(); // Список проектов в листе;
        private static List<StackPanel> listPanelStars = new List<StackPanel>(); // Список с панелью рейтинга;

        #region Работа с отображением информации о прогрессе отправки файла на сервер
        public static YourProject thisWindow { get; private set; }

        private static ListView listStatic { get; set; } // Лист с проектами;
        private static StackPanel panelLoading { get; set; } // Панель загрузки;
        public static ProgressBar loadUIPB { get; set; } // Элемент отображения информации о прогрессе загрузки проекта;
        public static TextBlock loadUITB { get; set; } // Отображение процент прогресса;
        public static TextBlock nameProjectLoadUI { get; set; } // Имя проекта который отправляется на сервер;

        #endregion

        #region Нажатия на кнопки и другие действие привязанные к объктам на форме

        #region Лист с проектами

        /// <summary>
        /// Загрузка окна
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Loaded</param>
        private void Start(object sender, RoutedEventArgs e)
        {
            if(OperationServer.fileSend != null)
            {
                SetSettingsPanelLoad(this, OperationServer.fileSend.nameFile);
            }

            if (OperationServer.fileReceiving != null)
            {
                SetSettingsPanelLoad(this, OperationServer.fileReceiving.nameFile, false);
            }

            int tempProject = 0;
            myItems.Clear();
            
            foreach (var item in Person.thisUser.listProject)
            {
                item.projectSettings.idProject = ++tempProject;
                myItems.Add(item.projectSettings);
            }

            listStatic = listViewProjects;
            //MessageBox.Show(Person.listProject.OfType<MyItemProject>().ToList().ToString() + "     " + myItems.ToString());
            // myItems.Add(SetNewInfoAtListView(++countProject, "123", "Не проверен", DateTime.Now.ToString("dd-MM-yyyy"), 5));

            //RefreshListView(myItems);
            listStatic.ItemsSource = myItems;
            //Перемешение панели с настройками проекта
            thisWindow.settingsPanel.Margin = new Thickness(0,settingsPanel.Margin.Top,-settingsPanel.ActualWidth - 5, 0);
            settingsPanel.Visibility = Visibility.Hidden;
        }  

        /// <summary>
        /// Переход в личный кабинет
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToBack(object sender, RoutedEventArgs e)
        {
            if (OperationServer.fileReceiving != null)
            {
                if(MessageBox.Show("Загрузка еще не завершена! Остановить загрузку?","Загрузка",MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                {
                    OperationServer.fileReceiving = null;
                    (new PersonalArea()).Show();                
                    Close();
                }           
            }
            else
            {
                (new PersonalArea()).Show();
                Close();
            }
        }


        /// <summary>
        /// Выбор проекта в списке
        /// </summary>
        /// <param name="sender">ListBox</param>
        /// <param name="e">SelectionChanged</param>
        private void SelectProject(object sender, SelectionChangedEventArgs e)
        {
            if (listViewProjects.SelectedValue != null)
            {
                settingButProject.IsEnabled = true;
                deleteButProject.IsEnabled = true;

                if(settingsPanel.Visibility == Visibility.Visible)
                {
                    nameProjTB.IsEnabled = false;    
                    //Установка данных на настройки проекта
                    OperationServer.SendMsgClient(256, 9, Person.thisUser.login, (listViewProjects.SelectedValue as Project.MyItemProject).nameProject);
                }

            }
            else
            {
                settingButProject.IsEnabled = false;
                deleteButProject.IsEnabled = false;
            }           
        }

        /// <summary>
        /// Удаляет выбранный проект в listView
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void DeleteProject(object sender, RoutedEventArgs e)
        {
            if((sender as Button).Tag != null)
            {
                listViewProjects.SelectedIndex = Int32.Parse((sender as Button).Tag.ToString()) - 1;
            }

            foreach(var selectValue in myItems)
            {
                if(selectValue.Equals((MyItemProject)listViewProjects.SelectedValue))
                {
                    OperationServer.SendMsgClient(128, 1004, selectValue.nameProject);
                    break;
                }
            }

            int elementDelete = -1; // Элемент в массиве который будет удалён;

            int temp = -1;
            foreach(var item in Person.thisUser.listProject)
            {
                if(elementDelete != -1)
                {
                    item.projectSettings.idProject--;
                }       
                else
                    temp++;

                if (item.projectSettings.Equals((MyItemProject)listViewProjects.SelectedValue))
                {
                    //   Person.listProject.Remove(item);
                    
                    elementDelete = temp;
                }
            }
            Person.thisUser.countProject--;
            Person.thisUser.listProject.RemoveAt(elementDelete);
            myItems?.Remove((MyItemProject)listViewProjects.SelectedValue);
           
          //  RefreshListView(myItems);
       
            MessageBox.Show("Проект успешно удален!");
        }

        /// <summary>
        /// Добавление нового проекта в список
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void AddNewProject(object sender, RoutedEventArgs e)
        {
            OpenFileDialog project = new OpenFileDialog
            {
                FileName = string.Empty,
               // Filter = "Project files| *.zip"
            };
            bool? result = project.ShowDialog();

            if(result == true)
            {
                string[] fullName = project.FileName.Split('\\');

                if (fullName[fullName.Length - 1].Length <= 45)
                {
                    if (CheckForDuplicateNames(fullName[fullName.Length - 1]))
                    {                       
                        byte[] arrByte = File.ReadAllBytes(project.FileName);

                        if (arrByte.Length > 0)
                        {
                            string[] nameSendFile = fullName[fullName.Length - 1].Split('.');
                            OperationServer.fileSend = new FileSend(arrByte, nameSendFile[0], nameSendFile[nameSendFile.Length - 1]);

                            SetSettingsPanelLoad(this, fullName[fullName.Length - 1]);

                            IsEnabledForm(false);
                        }
                        else
                            MessageBox.Show("Файл не может иметь размер 0!");
                    }
                }
                else
                    MessageBox.Show("Длина имени файла не должна превышать 45 символов");
            }
        }


        /// <summary>
        /// Drop файла в лист проектов 
        /// </summary>
        /// <param name="sender">ListBox</param>
        /// <param name="e">Drop</param>
        private void DropProjectAtList(object sender, DragEventArgs e)
        {
            bool isDontLoadFile = false; 
            string[] fileDrop = (string[])e.Data.GetData(DataFormats.FileDrop);
         
            for (int i = 0; i < fileDrop.Length; i++)
            {            
                string[] fileName = fileDrop[i].Split('\\');
                string[] expansionFile = fileName[fileName.Length - 1].Split('.');

                if (expansionFile[expansionFile.Length - 1].Equals("zip"))
                {
                    if (fileName[fileName.Length - 1].Length <= 45)
                    {
                        if (CheckForDuplicateNames(fileName[fileName.Length - 1]))
                        {
                            myItems.Add(new MyItemProject(++Person.thisUser.countProject, -1,fileName[fileName.Length - 1], "Загружен", DateTime.Now.ToString("dd-MM-yyyy"), 0));
                     //       RefreshListView(myItems);

                            string[] nameSendFile = fileName[fileName.Length - 1].Split('.');

                            OperationServer.fileSend = new FileSend(File.ReadAllBytes(fileDrop[0]), nameSendFile[0], nameSendFile[nameSendFile.Length - 1]);

                            SetSettingsPanelLoad(this,fileName[fileName.Length - 1]);

                            IsEnabledForm(false);
                        }                   
                    }
                    else
                        MessageBox.Show("Длина имени файла не должна превышать 45 символов");
                }
                else
                {
                    isDontLoadFile = true;
                }
            }

            if (isDontLoadFile)
                MessageBox.Show("Неверное расширение файла! Файл должен быть расширения *.zip");
            
        }

        /// <summary>
        /// Загрузка панели с звёздами для оторбражения рейтинга
        /// </summary>
        /// <param name="sender">Панель с звёздами</param>
        /// <param name="e">Loaded</param>
        private void StartStar(object sender, RoutedEventArgs e)
        {
            int idProject = Int32.Parse((sender as StackPanel).Tag.ToString()) - 1;
            listPanelStars.Add((sender as StackPanel));
            DrawStarsForProject(sender as StackPanel, Person.thisUser.listProject[idProject].projectSettings.ratingProject);
        }

        /// <summary>
        /// Рисует звёзды рейтинга у данного проекта
        /// </summary>
        /// <param name="sender">Панель с звёздами</param>
        /// <param name="ratingProject">Рейниг проекта</param>
        public static void DrawStarsForProject(StackPanel sender, double ratingProject)
        {              
            sender.ToolTip = $"Рейтинг: {ratingProject}";

            if (ratingProject != 0)
            {
                List<Polygon> stars = (sender as StackPanel).Children.OfType<Polygon>().ToList();
                int tempStar = 0;

                for (; ratingProject-- >= 1; tempStar++)
                {
                    try
                    {
                        stars[tempStar].Fill = Brushes.Yellow;
                    }
                    catch { break; }
                }
                /// Если осталась дробная часть в рейтинге
                if (++ratingProject > 0)
                {
                    LinearGradientBrush linearGradient = new LinearGradientBrush
                    {
                        StartPoint = new Point(1, 0),
                        EndPoint = new Point(0, 0)
                    };

                    linearGradient.GradientStops.Add(new GradientStop
                    {
                        Color = Color.FromRgb(128, 128, 128),
                        Offset = (1 - ratingProject) - 0.01
                    });

                    linearGradient.GradientStops.Add(new GradientStop
                    {
                        Color = Color.FromRgb(255, 255, 0),
                        Offset = 1 - ratingProject
                    });

                    stars[tempStar].Fill = linearGradient;
                }
            }
        }


        /// <summary>
        /// Нажание на панель с проектами
        /// </summary>
        /// <param name="sender">Grid</param>
        /// <param name="e">MouseLeftDown</param>
        private void HideSettigsProj(object sender, MouseButtonEventArgs e)
        {
            if (settingsPanel.Visibility.Equals(Visibility.Visible))         
                SetHiddenSettingsPanel();          
        }

        #endregion

        #region Settings Project

        /// <summary>
        /// Отображение панели с настройками проекта
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void ShowSettingsPanel(object sender, RoutedEventArgs e)
        {
            if (comboBoxTypeProj.Items.Count <= 0)
                OperationServer.SendMsgClient(32, 8); // Получение списка категорий проекта;
            else
            {              
                OperationServer.SendMsgClient(256, 9, Person.thisUser.login, (listViewProjects.SelectedValue as Project.MyItemProject).nameProject);
            }
        }

        /// <summary>
        /// Отмента установки настроек проекта\файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelSetSettingsProject(object sender, RoutedEventArgs e)
        {
            SetHiddenSettingsPanel();
        }

        /// <summary>
        /// Изменение картинки проекта
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">TextChanged</param>
        private void SetProjImage(object sender, TextChangedEventArgs e)
        {
            try
            {
                imageProj.Fill = new ImageBrush(new BitmapImage(new Uri(((sender as TextBox).Text.Trim()), UriKind.Absolute)));  
            }
            catch
            {          
                imageProj.Fill = new ImageBrush(new BitmapImage(new Uri(("https://pngimage.net/wp-content/uploads/2018/06/%D0%B1%D0%B5%D0%BB%D1%8B%D0%B9-%D0%B7%D0%BD%D0%B0%D0%BA-%D0%B2%D0%BE%D0%BF%D1%80%D0%BE%D1%81%D0%B0-png-2.png"), UriKind.Absolute)));              
            }
        }

        /// <summary>
        /// Изменение имени проекта
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void RenameProject(object sender, RoutedEventArgs e)
        {
            nameProjTB.IsEnabled = true;
        }

        /// <summary>
        /// Подтверждение изменений для информации о проекте
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void AcceptUpdateInfoProject(object sender, RoutedEventArgs e)
        {
            if (new TextRange(noteTextBox.Document.ContentStart, noteTextBox.Document.ContentEnd).Text.Length <= 900)
            {        
                string nameProj = nameProjTB.Text + expasionText.Text;
                if (comboBoxTypeProj.SelectedIndex != -1)
                {
                    int idProject = (listViewProjects.SelectedItem as MyItemProject).idProjectAtDataBase;
                    if (nameProjTB.Text.Trim().Length != 0)
                    {
                        if (CheckToDiscName(listViewProjects.SelectedValue, nameProj))
                        {                       
                            SendInfo(idProject, nameProj);
                        }
                        else

                            MessageBox.Show($"Проект с таким именем {nameProj} уже существует!");
                    }
                    else
                        SendInfo(idProject, nameProjTB.Tag.ToString());
                }
                else
                    MessageBox.Show("Выберите категорию!");
            }
            else
                MessageBox.Show("Описание проекта слишком большое. Максимальное значение 900!");

        }

        #endregion

        #endregion

        /// <summary>
        /// Отправка данных на сервер 
        /// </summary>
        /// <param name="idProject">id проекта</param>
        /// <param name="nameProject">Имя проекта</param>
        private void SendInfo(int idProject,string nameProject)
        {
            int viewApp = comboBoxTypeProj.SelectedIndex;
            string hrefImage = hrefImageTb.Text;
            string note = new TextRange(noteTextBox.Document.ContentStart, noteTextBox.Document.ContentEnd).Text;

            OperationServer.SendMsgClient(520, 10, idProject, nameProject, viewApp + 1, note, hrefImage);
            SetHiddenSettingsPanel();
        }

        /// <summary>
        /// Проверка на повторение имен
        /// </summary>
        /// <param name="project">Проект который проверяется</param>
        /// <param name="name">Имя проекта</param>
        /// <returns>Состояние проверки</returns>
        private bool CheckToDiscName(object project, string name)
        {       
            foreach(var item in listViewProjects.Items)
            {
                if(!item.Equals(project))
                {
                    if ((item as MyItemProject).nameProject == name)
                        return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Hide панели с настройками проекта
        /// </summary>
        private void SetHiddenSettingsPanel()
        {
            #region Анимация панели
            thisWindow.settingsPanel.BeginAnimation(MarginProperty,
                StyleUIE.AnimationObject(thisWindow.settingsPanel,
                TimeSpan.FromSeconds(0.33),
                new Thickness(0, settingsPanel.Margin.Top, -settingsPanel.ActualWidth - 5, 0),
                new EventHandler((sen, e) =>  { settingsPanel.Visibility = Visibility.Hidden; })));
            #endregion

            nameProjTB.IsEnabled = false;
            comboBoxTypeProj.SelectedIndex = -1;
            noteTextBox.Document.Blocks.Clear();
        }

        /// <summary>
        /// Установка информации о проекте
        /// </summary>
        /// <param name="thisWindow">Текущее окно</param>
        /// <param name="nameProject">Имя проекта</param>
        /// <param name="datePub">Дата публикации</param>
        /// <param name="rating">Рейтинг</param>
        /// <param name="countVote">Количество голосов</param>
        /// <param name="note">Описание</param>
        /// <param name="hrefImage">Ссылка на картинку</param>
        /// <param name="viewApp">Категория проекта</param>
        public static void SetInfoForSettingsPanel(YourProject thisWindow,string nameProject, string datePub,
                double rating, int countVote, string note, string hrefImage, int viewApp)
        {
            #region Анимация панели
           
           thisWindow.settingsPanel.BeginAnimation(MarginProperty,
               StyleUIE.AnimationObject(thisWindow.settingsPanel, 
               TimeSpan.FromSeconds(0.33), 
               new Thickness(0, thisWindow.settingsPanel.Margin.Top, 0, 0)));

            #endregion       

            string[] exp = nameProject.Split('.');
     
            thisWindow.nameProjTB.Text = nameProject.Replace("." + exp[exp.Length - 1], string.Empty);
            thisWindow.nameProjTB.Tag = nameProject;
            thisWindow.nameProjTB.MaxLength = 45 - (exp[exp.Length - 1].Length + 1);
            thisWindow.expasionText.Text = "." + exp[exp.Length - 1];

            thisWindow.dateProjLB.Content = $"Date : {datePub}";
            thisWindow.ratingProjLB.Content = $"Rating : {rating}";
            thisWindow.countVoteProjLB.Content = $"Count Vote : {countVote}";

            if (!hrefImage.Equals(string.Empty) || hrefImage != null)
            {
                thisWindow.hrefImageTb.Text = hrefImage;
                try
                {
                    thisWindow.imageProj.Fill = new ImageBrush(new BitmapImage(new Uri(hrefImage, UriKind.Absolute)));
                }
                catch { }
            }

            thisWindow.noteTextBox.Document.Blocks.Clear();
            if (!note.Equals(string.Empty) || note != null)
                thisWindow.noteTextBox.AppendText(note);

            if (viewApp > 0)
            {
                thisWindow.comboBoxTypeProj.SelectedIndex = viewApp - 1;
            }

            #region Устанавливаем новый рейтинг у проекта
            double oldRating = (thisWindow.listViewProjects.SelectedItem as MyItemProject).ratingProject;
          
            if(rating != oldRating)
            {
                int index = thisWindow.listViewProjects.SelectedIndex;
                StackPanel panelStar = listPanelStars[index];

                DrawStarsForProject(panelStar, rating);
            }

            #endregion
        }



        /// <summary>
        /// Установка панели с загрузкой файла
        /// </summary>
        /// <param name="thisWindow">Текущее окно</param>
        /// <param name="nameProject">Имя файла</param>
        /// <param name="isSend">Состояние отправки</param>
        public static void SetSettingsPanelLoad(YourProject thisWindow,  string nameProject = "Error", bool isSend = true)
        {
            thisWindow.Dispatcher.Invoke(new ThreadStart(() =>
            {
                panelLoading = thisWindow.panelLoad;
                thisWindow.panelLoad.Visibility = Visibility.Visible;
                thisWindow.loadNameProj.Text = nameProject;
                thisWindow.loadProgressPB.Value = 0;
                thisWindow.loadProgressTB.Text = thisWindow.loadProgressPB.Value + "%";

                if (!isSend)
                   thisWindow.nameLoadTB.Text = "Загрузка : ";
                else
                    thisWindow.nameLoadTB.Text = "Отправка : ";

                nameProjectLoadUI = thisWindow.loadNameProj;
                loadUIPB = thisWindow.loadProgressPB;
                loadUITB = thisWindow.loadProgressTB;
            }));
        }

        /// <summary>
        /// Устанавливает значение для прогресс загрузки проекта
        /// </summary>
        /// <param name="temp">Прогресс</param>
        /// <param name="isLoadFileServer">Загрузка на сервер или из сервера</param>
        public static void SetValueProgressLoad(int temp, bool isLoadFileServer)
        {
            loadUIPB.Dispatcher.Invoke(new ThreadStart(async () =>
            {

                if (panelLoading.Visibility != Visibility.Visible)
                {
                    panelLoading.Visibility = Visibility.Visible;
                }

                loadUIPB.Value = temp;
                loadUITB.Text = temp + "%";

                if(temp >= 100)
                {
                    loadUIPB = null;
                    loadUITB = null;

                    //if (!isLoadFileServer)
                    //{
                    //    MyItemProject myItem = new MyItemProject(++Person.countProject,-1, nameProjectLoadUI.Text, "Загружен", DateTime.Now.ToString("dd-MM-yyyy"), 0);
                    //    myItems.Add(myItem);
                    //    Person.listProject.Add(new Project(myItem));
                    //    RefreshListView(myItems);
                    //    MessageBox.Show("Файл успешно добавлен!");
                    //}
                    //else
                    if(isLoadFileServer)
                    {
                        MessageBox.Show("Файл успешно скачен!");
                    }

                    IsEnabledForm(true);

                    await Task.Delay(10000);
                    panelLoading.Visibility = Visibility.Hidden;
                }
            }));
        }

        /// <summary>
        /// Блокировка возможности отравки файла на сервер
        /// </summary>
        /// <param name="enabled">Состояние блокировки</param>
        public static void IsEnabledForm(bool enabled)
        {
            thisWindow.Dispatcher.Invoke(new ThreadStart(() =>
            {
                thisWindow.addButProject.IsEnabled = enabled;
                thisWindow.AllowDrop = enabled;
            }));
        }

        /// <summary>
        /// Проверка на повторяющиеся имена в листе с проектами
        /// </summary>
        /// <param name="nameProject">Имя проекта</param>
        /// <returns>Состояние проверки</returns>
        private bool CheckForDuplicateNames(string nameProject)
        {
            foreach(var nameProjectU in myItems)
            {
                if(nameProjectU.nameProject.Equals(nameProject))
                {
                    MessageBox.Show(nameProject + " такой проект уже есть в списке!");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Добавление проекта в список с проектами
        /// </summary>
        /// <param name="idProjectAtDB">id проекта в базе данных</param>
        public static void AddProjectToList(int idProjectAtDB)
        {
            MyItemProject myItem = new MyItemProject(++Person.thisUser.countProject, idProjectAtDB, nameProjectLoadUI.Text, "Загружен", DateTime.Now.ToString("dd-MM-yyyy"), 0);
            myItems.Add(myItem);
            Person.thisUser.listProject.Add(new Project(myItem));
       //     RefreshListView(myItems);
            MessageBox.Show("Файл успешно добавлен!");        
        }

        /// <summary>
        /// Изменение имени у проекта
        /// </summary>
        /// <param name="newNameProject">Новое имя</param>
        public static void RenameProject(string newNameProject)
        {
            myItems[listStatic.SelectedIndex].nameProject = newNameProject;
       //     RefreshListView(myItems);
        }

        ///// <summary>
        ///// Обновляет таблицу с проектами
        ///// </summary>
        ///// <param name="myItems">Данные которые будут отображатся в ListView</param>
        //private static void RefreshListView(List<MyItemProject> myItems)
        //{          
        //    listStatic.ItemsSource = myItems;
        //    listStatic.Items.Refresh();
        //}

        /// <summary>
        /// Загрузка выбранного файла
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">Click</param>
        private void DownloadFile(object sender, RoutedEventArgs e)
        {
            listViewProjects.SelectedIndex = Int32.Parse((sender as Button).Tag.ToString()) - 1;

            foreach (var selectValue in myItems)
            {
                if (selectValue.Equals((MyItemProject)listViewProjects.SelectedValue))
                {
                    OperationServer.SendMsgClient(256, 1006, Person.thisUser.login, selectValue.nameProject);
                    break;
                }
            }

            IsEnabledForm(false);
        }
    } 
}
