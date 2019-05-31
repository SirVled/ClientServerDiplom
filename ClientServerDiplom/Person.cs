using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientServerDiplom
{
    public abstract class Person
    {
        public static string login { get; set; }// Логин пользователя;

        public static string name { get; set; } // Имя пользователя;
        public static string lastname { get; set; }  // Фамилия пользователя;

        public static int level { get; set; } // Уровень пользователя;
        public static int likes { get; set; }  // Кол-во лайков на аккаунте пользователя;

        public static string image { get; set; }  // Аватарка пользователя;
        public static string email { get; set; }  // Почта пользователя;

        public static int countProject { get; set; } // Количество проектов;
        public static string note { get; set; } // Описание (О себе);

        public static List<Project> listProject { get; set; } // Лист с списком проектов у данного пользователя;
    }
    
    /// <summary>
    /// Класс с свойствами проекта
    /// </summary>
    public class Project
    {

        /// <summary>
        /// Класс проекта, который хранит в себе данные которые нужно добавить в ListView
        /// </summary>
        public class MyItemProject
        {

            public MyItemProject (int idProject ,string name, string date, double rating)
            {
                idProjectAtDataBase = idProject;
                nameProject = name;
                dateAddingProject = date;
                ratingProject = rating;
            }

            public MyItemProject(int id, int idProject, string name, string status, string date, double rating) : this(idProject,name,date,rating)
            {
                this.idProject = id;   
                statusProject = status;
            }

            public int idProject { get; set; } // id проекта;

            public int idProjectAtDataBase { get; private set; } // id проекта в базе данных;
            public string nameProject { get; set; } // Панель с именем проекта;
            public string statusProject { get; set; } // Статус проекта;
            public string dateAddingProject { get; set; } // Дата добавление проекта;

            public double ratingProject { get; set; } // Рейтинг проекта;
        }

        public Project(int idProject,string name, int countVote, double rating, string date, string viewApplication)
        {
            projectSettings = new MyItemProject(idProject, name, date, rating);

            this.countVote = countVote;          
            this.viewApplication = viewApplication;
        }


        public Project(int idProject, string name, int countVote, double rating, string date, string viewApplication , string note, string image) : this(idProject ,name, countVote, rating, date, viewApplication)
        {
            this.note = note;
            this.image = image;
        }

        public Project(MyItemProject myItem)
        {
            projectSettings = myItem;
        }

        public MyItemProject projectSettings; // Проект;

        public int countVote { get; set; } // Количество голосов у проекта;  
        public string viewApplication { get; set; } // Тип проекта;
        public string note { get; set; } // Описание проекта;
        public string image { get; set; } // Изображение проекта;
    }
}
