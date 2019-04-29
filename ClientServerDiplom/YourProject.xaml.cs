﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using static ClientServerDiplom.Project;

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
            foreach(var item in Person.listProject)
            {
                item.projectSettings.idProject = ++countProject;
                myItems.Add(item.projectSettings);
            }
            
            //MessageBox.Show(Person.listProject.OfType<MyItemProject>().ToList().ToString() + "     " + myItems.ToString());
            // myItems.Add(SetNewInfoAtListView(++countProject, "123", "Не проверен", DateTime.Now.ToString("dd-MM-yyyy"), 5));
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
            foreach(var selectValue in myItems)
            {
                if(selectValue.Equals((MyItemProject)listViewProjects.SelectedValue))
                {
                    OperationServer.SendMsgClient(128, 1004, selectValue.nameProject);
                    break;
                }
            }

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
                        myItems.Add(new MyItemProject(++countProject, fullName[fullName.Length - 1], "Не проверен", DateTime.Now.ToString("dd-MM-yyyy"), 0));
                        RefreshListView(myItems);

                        byte[] arrByte = File.ReadAllBytes(project.FileName);

                        string[] nameSendFile = fullName[fullName.Length - 1].Split('.');
                        OperationServer.fileSend = new FileSend(arrByte, nameSendFile[0], nameSendFile[nameSendFile.Length - 1]);
  
                        MessageBox.Show("Файл(ы) успешно добавлен(ы)!");
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
                            myItems.Add(new MyItemProject(++countProject, fileName[fileName.Length - 1], "Не проверен", DateTime.Now.ToString("dd-MM-yyyy"), 0));
                            RefreshListView(myItems);

                            string[] nameSendFile = fileName[fileName.Length - 1].Split('.');

                            OperationServer.fileSend = new FileSend(File.ReadAllBytes(fileDrop[0]), nameSendFile[0], nameSendFile[nameSendFile.Length - 1]);
                            MessageBox.Show("Файл(ы) успешно добавлены!");
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
    }

    
}
