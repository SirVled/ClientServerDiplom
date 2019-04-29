﻿
using ServerDiplom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Server
{
    public abstract class OperationServerAtClient
    {

        #region База - Сервер

        /// <summary>
        /// Читает данные о пользователе и отправляет ответ клиенту
        /// </summary>
        /// <param name="client">Сокет клиента</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        public static void CheckLoginAndPasswordUser(Socket client,  string login, string password)
        {         
            string[] arrParm = { "@loginIn", "@passIn" , "@loginOut" };
            string[] arrParmData = { login, password };
            string[] arrParmOut = MySqlClass.MySQLInOut("CheckLoginAndPassUser", arrParm, arrParmData);

           Connect(ServerClass.nextID++, login, client);

            ServerClass.SendMsgClient(client, 64, 1, Int32.Parse(arrParmOut[0]));
        }

        /// <summary>
        /// Проверяет логин на уникальность
        /// </summary>
        /// <param name="client">Сокет клиента</param>
        /// <param name="login">Логин</param>
        public static void CheckUniqueLogin(Socket client, string login)
        {
            string[] arrParm = { "@loginIn" , "@loginOutBool" };
            string[] arrParmData = { login };
            string[] arrParmOut = MySqlClass.MySQLInOut("CheckUniqueLogin", arrParm, arrParmData);

            ServerClass.SendMsgClient(client, 64, 2, Int32.Parse(arrParmOut[0]));
        }

        /// <summary>
        /// Добавляет в базу нового пользователя
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        public static void CreateNewUser(string login, string password)
        {
            string[] arrParm = { "@loginIn", "@passwordIn" };
            string[] arrParmData = { login, password };
            MySqlClass.MySQLIn("AddUser", arrParm, arrParmData);
        }

        /// <summary>
        /// Получаем всю информацию о пользователе из базы
        /// </summary>
        /// <param name="client">Сокет клиента</param>
        /// <param name="login">Логин</param>
        public static void CheckFullInfoOfPerson(Socket client, string login)
        {
            string[] arrParm = { "@loginUser", "@nameOut", "@lastnameOut", "@levelOut", "@likeOut", "@imageOut", "@emailOut" };
            string[] arrParmData = { login };
            string[] arrParmOut = MySqlClass.MySQLInOut("CheckFullInfoPerson", arrParm, arrParmData);

            if (arrParmOut[0].Length > 0)
            {
                foreach(var clientU in ServerClass.clients)
                {
                    if (clientU.socket == client)
                    {
                        clientU.countLike = Int32.Parse(arrParmOut[3]);
                        break;
                    }
                }

                ServerClass.SendMsgClient(client, 1024, 3, arrParmOut[0], arrParmOut[1], Int32.Parse(arrParmOut[2]),
                                        Int32.Parse(arrParmOut[3]), arrParmOut[4], arrParmOut[5]);
            }
        }

        /// <summary>
        /// Проверяем изменение в количестве лайков у пользователя
        /// </summary>
        /// <param name="client">Сокет клиента</param>
        /// <param name="login">Логин</param>
        public static void CheckNewLike(Socket client, string login)
        {
            string[] arrParm = { "@loginUser", "@likeOut" };
            string[] arrParmData = { login };
            string[] arrParmOut = MySqlClass.MySQLInOut("CheckNewLike", arrParm, arrParmData);

            foreach (var clientU in ServerClass.clients)
            {
                if (clientU.socket == client)
                {
                    if(clientU.countLike != Int32.Parse(arrParmOut[0]))
                    {                       
                        clientU.countLike = Int32.Parse(arrParmOut[0]);
                        ServerClass.SendMsgClient(client, 64, 5, arrParmOut[0]);
                    }
                }
            }       
        }

        /// <summary>
        /// Добавление информации о пользователе в базу
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="infoUser">Информация пользователя</param>
        public static void AddNewInfoForPerson(string login, string[] infoUser)
        {
            string[] arrParm = { "@loginUser", "@nameUser", "@lastName", "@imageUser", "@email" };
            string[] arrParmData = { login, infoUser[0], infoUser[1], infoUser[2], infoUser[3] };
            MySqlClass.MySQLIn("AddPersonAtDatabase", arrParm, arrParmData);
        }

        /// <summary>
        /// Изменение информации о пользователе в базе
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="infoUser">Информация пользователя</param>
        public static void UpdateInfoAboutPerson(string login , string[] infoUser)
        {
            string[] arrParm = { "@loginIn", "@nameIn", "@lastnameIn", "@imageIn", "@emailIn" };
            string[] arrParmData = { login, infoUser[0], infoUser[1], infoUser[2], infoUser[3] };
            MySqlClass.MySQLIn("UpdateInfoAboutPerson", arrParm, arrParmData);
        }

        #region Проекты/файлы пользователя

        /// <summary>
        /// Добавление свойства проекта в базу
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="nameProject">Имя проекта</param>
        public static void AddNewProject(string login, string nameProject)
        {
            string[] arrParm = { "@nameUserIn", "@nameProjectIn", "@datePublication" };
            string[] arrParmData = { login, nameProject , DateTime.Now.ToString("yyyy-MM-dd") };
            MySqlClass.MySQLIn("AddProject", arrParm, arrParmData);
        }

        /// <summary>
        /// Удаление свойств проекта из базы
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="nameProject">Имя проекта</param>
        public static void DeleteProjectDatabase(string login, string nameProject)
        {
            string[] arrParm = { "@nameUserIn", "@nameProjectIn" };
            string[] arrParmData = { login, nameProject};
            MySqlClass.MySQLIn("DeleteProject", arrParm, arrParmData);
        }

        /// <summary>
        /// Получение списка проектов и файлов сохраненные на серевер у данного пользователя
        /// </summary>
        /// <param name="client">Сокет клиента</param>
        /// <param name="login">Логин</param>
        public static void GetListProject(Socket client, string login)
        {
            try
            {
                string quary = "select Project.*, ViewApplication.Name from Project " +
                                       "inner join HistoryDownload on HistoryDownload.idProject = Project.idProject " +
                                       "inner join Person on Person.idPerson = HistoryDownload.idPerson " +
                                       "inner join `User` on `User`.idUser = Person.idUser " +
                                       "inner join ViewApplication on ViewApplication.idViewApplication = Project.idViewApplication " +
                         $"where `User`.Login = '{login}' ";

                MySqlClass.mySQLConn.Open();

                MySql.Data.MySqlClient.MySqlDataReader reader = (new MySql.Data.MySqlClient.MySqlCommand(quary, MySqlClass.mySQLConn)).ExecuteReader();
         
                while (reader.Read())
                {
                    string name = reader.GetString(2);
                    int countVote = reader.GetInt32(3);
                    double rating = reader.GetDouble(4);
                    string date = reader.GetDateTime(5).ToString("dd-MM-yyyy");
                  
                    string note = (reader.IsDBNull(7) == true) ? string.Empty : reader.GetString(7);
                    string image = (reader.IsDBNull(8) == true) ? string.Empty : reader.GetString(8);

                    string viewApplication = reader.GetString(9);
                  
                    ServerClass.SendMsgClient(client, 2048, 1002, name, countVote, rating, date, note, image, viewApplication);
                }
            }
            catch(Exception ex) { ServerClass.WriteConsoleMsg("GetListProject : " + ex.Message); }
            finally { MySqlClass.mySQLConn.Close(); }
        }

        #endregion 

        #endregion

        /// <summary>
        /// Получение файлов от клиента (по пакетам)
        /// </summary>
        /// <param name="fileSize">Записываемый буффер</param>  
        /// <param name="infoFile">Количество байт полученных от клиента</param>     
        /// <param name="countRecByte">Размер пакета</param>     
        internal static void ReceivedFile(FileSett file, byte[] infoFile, int countRecByte)
        {
            if (file.progressSend == null)           
                file.progressSend = infoFile;
            
            else
            {
                byte[] buf = file.progressSend;
                Array.Resize(ref file.progressSend, countRecByte + file.progressSend.Length);
                Buffer.BlockCopy(buf, 0, file.progressSend, 0, buf.Length);
                Buffer.BlockCopy(infoFile, 0, file.progressSend, file.progress, countRecByte);
            }
            
            ServerClass.SendMsgClient(file.user.socket, 256, 1001);
        }

        /// <summary>
        /// Добавление пользователя в список
        /// </summary>
        /// <param name="nextID">id пользователя</param>
        /// <param name="loginUser">Логин пользователя</param>
        /// <param name="client">Сокет пользователя</param>
        private static void Connect(int nextID, string loginUser, Socket client)
        {
            Client cl = new Client
            {
                id = nextID,
                name = loginUser,
                socket = client
            };

            ServerClass.clients.Add(cl);
        }

        /// <summary>
        /// Удаление пользователя с списка
        /// </summary>
        /// <param name="loginUser">Логин пользователя</param>
        public static void Disconnect(string loginUser)
        {
            foreach(var client in ServerClass.clients)
            {
                if(client.name.Equals(loginUser))
                {
                    ServerClass.clients.Remove(client);
                    break;
                }
            }
        }
    }
}