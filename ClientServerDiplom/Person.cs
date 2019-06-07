using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientServerDiplom
{
    public class Person
    {
        public static Person thisUser = new Person();

        public Person()
        {

        }

        public Person(string login, Project project)
        {
            this.login = login;
            this.project = project;
        }

        public Person(string login, string image, int level, int countProject)
        {
            this.login = login;
            this.image = image;
            this.level = level;
            this.countProject = countProject;
        }

        public string login { get; set; }// Логин пользователя;

        public string name { get; set; } // Имя пользователя;
        public string lastname { get; set; }  // Фамилия пользователя;

        public int level { get; set; } // Уровень пользователя;
        public int likes { get; set; }  // Кол-во лайков на аккаунте пользователя;
        public int countSub { get; set; } // Кол-во подписчиков;

        public string image { get; set; }  // Аватарка пользователя;
        public string email { get; set; }  // Почта пользователя;

        public int countProject { get; set; } // Количество проектов;
        public string note { get; set; } // Описание (О себе);


        public List<Project> listProject { get; set; } // Лист с списком проектов у данного пользователя;

        public Project project { get; set; } // Проект 
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

            public MyItemProject(string name, double rating)
            {
                nameProject = name;
                ratingProject = rating;
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

        public Project(MyItemProject myItem, string image, string note)
        {
            projectSettings = myItem;

            this.image = image;
            this.note = note;
        }

        public MyItemProject projectSettings; // Проект;

        public int countVote { get; set; } // Количество голосов у проекта;  
        public string viewApplication { get; set; } // Тип проекта;
        public string note { get; set; } // Описание проекта;
        public string image { get; set; } // Изображение проекта;
    }
}
