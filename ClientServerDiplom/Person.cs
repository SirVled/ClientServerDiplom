using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerDiplom
{
    public abstract class Person
    {
        public static string login { get; set; }// Логие пользователя;

        public static string name { get; set; } // Имя пользователя;
        public static string lastname { get; set; }  // Фамилия пользователя;

        public static int level { get; set; } // Уровень пользователя;
        public static int likes { get; set; }  // Кол-во лайков на аккаунте пользователя;

        public static string image { get; set; }  // Аватарка пользователя;
        public static string email { get; set; }  // Почта пользователя;
    }
}
