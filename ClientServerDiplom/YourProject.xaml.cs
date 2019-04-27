using Microsoft.Win32;
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

namespace ClientServerDiplom
{
    /// <summary>
    /// Логика взаимодействия для YourProject.xaml
    /// </summary>
    public partial class YourProject : Window
    {
        public YourProject()
        {
            InitializeComponent();
        }

        private int countProject = 0; // Количество проектов;
        private List<MyItemProject> myItems = new List<MyItemProject>(); // Список проектов в листе;

        #region Нажатия на кнопки и другие действие привязанные к объктам на форме

        /// <summary>
        /// Загрузка окна
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Loaded</param>
        private void Start(object sender, RoutedEventArgs e)
        {
            myItems.Add(SetNewInfoAtListView(++countProject, "123", "Не проверен", DateTime.Now.ToString("dd-MM-yyyy"), 5));
            RefreshListView(myItems);        
        }

        /// <summary>
        /// Переход в личный кабинет
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToBack(object sender, RoutedEventArgs e)
        {
            (new PersonalArea()).Show();
            Close();
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
            myItems?.Remove((MyItemProject)listViewProjects.SelectedValue);
            RefreshListView(myItems);

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
                Filter = "Project files| *.csproj"
            };

            bool? result = project.ShowDialog();

            if(result == true)
            {
                string[] fullName = project.FileName.Split('\\');

                if (fullName[fullName.Length - 1].Length <= 45)
                {
                    if (CheckForDuplicateNames(fullName[fullName.Length - 1]))
                    {
                        myItems.Add(SetNewInfoAtListView(++countProject, fullName[fullName.Length - 1], "Не проверен", DateTime.Now.ToString("dd-MM-yyyy"), 0));
                        RefreshListView(myItems);
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

                if (expansionFile[expansionFile.Length - 1].Equals("csproj"))
                {
                    if (fileName[fileName.Length - 1].Length <= 45)
                    {
                        if (CheckForDuplicateNames(fileName[fileName.Length - 1]))
                        {
                            myItems.Add(SetNewInfoAtListView(++countProject, fileName[fileName.Length - 1], "Не проверен", DateTime.Now.ToString("dd-MM-yyyy"), 0));
                            RefreshListView(myItems);
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

            if(isDontLoadFile)
                MessageBox.Show("Неверное расширение файла! Файл должен быть расширения *.csproj");
        }

        #endregion

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
        /// Обновляет таблицу с проектами
        /// </summary>
        /// <param name="myItems">Данные которые будут отображатся в ListView</param>
        private void RefreshListView(List<MyItemProject> myItems)
        {
            listViewProjects.ItemsSource = myItems;
            listViewProjects.Items.Refresh();
        }

        /// <summary>
        /// Создает колонку с информацией в ListView
        /// </summary>
        /// <param name="id">Номер проекта</param>
        /// <param name="name">Имя проекта</param>
        /// <param name="status">Статус проекта</param>
        /// <param name="date">Дата добавление проекта</param>
        /// <param name="rating">Рейтинг проекта</param>
        /// <returns>Информация о проекте</returns>
        private MyItemProject SetNewInfoAtListView(int id, string name , string status, string date, int rating)
        {
            MyItemProject myProject = new MyItemProject
            {
                idProject = id,
                nameProject = name,
                statusProject = status,
                dateAddingProject = date,
                ratingProject = rating
            };
       
            return myProject;
        }   
    }

    /// <summary>
    /// Класс который хранит в себе данные которые нужно добавить в ListView
    /// </summary>
    class MyItemProject
    {
        public int idProject { get; set; } // id проекта;
        public string nameProject { get; set; } // Панель с именем проекта;

        public string statusProject { get; set; } // Статус проекта;
        public string dateAddingProject { get; set; } // Дата добавление проекта;

        public int ratingProject { get; set; } // Рейтинг проекта;
    }
}
