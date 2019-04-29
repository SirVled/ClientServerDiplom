using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            public MyItemProject (string name, string date, double rating)
            {
                nameProject = name;
                dateAddingProject = date;
                ratingProject = rating;
            }

            public MyItemProject(int id, string name, string status, string date, double rating) : this(name,date,rating)
            {
                idProject = id;   
                statusProject = status;
            }

            public int idProject { get; set; } // id проекта;
            public string nameProject { get; set; } // Панель с именем проекта;

            public string statusProject { get; set; } // Статус проекта;
            public string dateAddingProject { get; set; } // Дата добавление проекта;

            public double ratingProject { get; set; } // Рейтинг проекта;
        }


        public Project(string name, int countVote, double rating, string date, string viewApplication)
        {
            projectSettings = new MyItemProject(name, date, rating);           

            this.countVote = countVote;          
            this.viewApplication = viewApplication;
        }

        public Project(string name, int countVote, double rating, string date, string viewApplication , string note, string image) : this(name, countVote, rating, date, viewApplication)
        {
            this.note = note;
            this.image = image;
        }

        public MyItemProject projectSettings; // Проект;

        public int countVote { get; set; } // Количество голосов у проекта;
   
        public string viewApplication { get; set; } // Тип проекта;

        public string note { get; set; } // Описание проекта;

        public string image { get; set; } // Изображение проекта;
    }
}
