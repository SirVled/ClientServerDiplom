using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace ServerDiplom
{
    class MySqlClass
    {
        public static string sConnString = @"server = 81.200.119.82;
            database=OstapFckMyAss;
            user id =student;
            port=3306;
            password=Student!@#;
            Convert Zero Datetime=true;"; //Строка подключения к серверу

        public static string sConnStringLocal = @"server = localhost;
            database=OstapFckMyAss;
            user id =root;
            port=3306;
            password=root;
            Convert Zero Datetime=true;";

        public static int idUser { get; set; } // id юзера;
        public static string nameUser { get; set; } // Логин юзера;

        public static MySqlConnection mySQLConn = new MySqlConnection(sConnString); // Объект подключения к БД    

        /// <summary>
        /// Быстрые запросы в базу (без получения данных)
        /// </summary>
        /// <param name="quary">Запрос</param>
        public static void CreateExecuteNonQuery(string quary)
        {
            OpenConnect(ref mySQLConn);
           
            (new MySqlCommand(quary, mySQLConn)).ExecuteNonQuery();
            mySQLConn.Close();
        }

        /// <summary>
        /// Открывает соединение с базой
        /// </summary>
        /// <param name="myConnect">Подключение к базе</param>
        private static void OpenConnect(ref MySqlConnection myConnect)
        {
            myConnect.Open();
        }

        /// <summary>
        /// Возвращает список с данными из таблицы
        /// </summary>
        /// <param name="quary">Запрос</param>
        /// <param name="numberReadRow">Номера тех столбцов, с которых будут браться данные </param>
        /// <returns></returns>
        public static List<List<dynamic>> MySqlQuaryOut(string quary, params int[] numberReadRow)
        {
            List<List<dynamic>> arrData = new List<List<dynamic>>();

            OpenConnect(ref mySQLConn);
         
            MySqlCommand myCommand = new MySqlCommand(quary, mySQLConn);
            MySqlDataReader MyDataReader;
            MyDataReader = myCommand.ExecuteReader();

            for (int i = 0; i < numberReadRow.Length; i++)
                arrData.Add(new List<dynamic>());

            while (MyDataReader.Read())
            {
                for (int i = 0; i < numberReadRow.Length; i++)
                {
                    arrData[i].Add(MyDataReader.GetString(numberReadRow[i]));
                }
            }
            mySQLConn.Close();
            return arrData;
        }

        // Фуннкция только с входными переменными
        public static void MySQLIn(string Command, string[] arrParam, string[] arrParamData)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = mySQLConn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = Command;
            for (int i = 0; i < arrParam.Length; i++)
            {
                cmd.Parameters.Add(new MySqlParameter(arrParam[i], MySqlDbType.VarChar, 1000));
                cmd.Parameters[arrParam[i]].Value = arrParamData[i];
            }
            mySQLConn.Open();
            cmd.ExecuteNonQuery();
            mySQLConn.Close();
        }
        // Функция с входными перемеенными и выходными
        public static string[] MySQLInOut(string Command, string[] arrParam, string[] arrParamData)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = mySQLConn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = Command;
            string[] arrParamOut = new string[arrParam.Length - arrParamData.Length];
            for (int i = 0; i < arrParam.Length; i++)
            {
                cmd.Parameters.Add(new MySqlParameter(arrParam[i], MySqlDbType.VarChar, 1000));
                if (i < arrParamData.Length)
                {
                    cmd.Parameters[arrParam[i]].Value = arrParamData[i];
                }
                else
                {
                    cmd.Parameters[arrParam[i]].Direction = ParameterDirection.Output;
                }
            }
            mySQLConn.Open();
            cmd.ExecuteNonQuery();
            mySQLConn.Close();
            for (int i = 0; i < arrParamOut.Length; i++)
            {
                arrParamOut[i] = Convert.ToString(cmd.Parameters[arrParam[arrParamData.Length + i]].Value);
            }
            return arrParamOut;
        }
    }
}