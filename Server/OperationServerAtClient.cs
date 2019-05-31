using ServerDiplom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Server
{
    public abstract class OperationServerAtClient
    {

        internal static List<FileSett> listFileSend = new List<FileSett>(); // Список файлов который в данный момент скачиваются из сервера

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
        /// Проверяет логин\email на уникальность
        /// </summary>
        /// <param name="client">Сокет клиента</param>
        /// <param name="login">Логин</param>
        /// <param name="email">Почта</param>
        /// <param name="idOperation">Номер операции которая выполнется в клиенте</param>
        public static void CheckUnique(Socket client, string login, string email, int idOperation = 2)
        {
            string[] arrParm = { "@loginIn", "@emailIn", "@loginOutBool" };
            string[] arrParmData = { login, email };
            string[] arrParmOut = MySqlClass.MySQLInOut("CheckUnique", arrParm, arrParmData);

            ServerClass.SendMsgClient(client, 64, idOperation, Int32.Parse(arrParmOut[0]));
        }

        /// <summary>
        /// Добавляет в базу нового пользователя
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <param name="email">Пароль</param>
        public static void CreateNewUser(string login, string password, string email)
        {
            string[] arrParm = { "@loginIn", "@passwordIn", "@emailIn" };
            string[] arrParmData = { login, password , email};
            MySqlClass.MySQLIn("AddUser", arrParm, arrParmData);
        }

        /// <summary>
        /// Получаем всю информацию о пользователе из базы
        /// </summary>
        /// <param name="client">Сокет клиента</param>
        /// <param name="login">Логин</param>
        public static void CheckFullInfoOfPerson(Socket client, string login)
        {
            string[] arrParm = { "@loginUser", "@nameOut", "@lastnameOut", "@levelOut", "@likeOut", "@imageOut", "@emailOut", "@countProjectOut","@noteOut" };
            string[] arrParmData = { login };
            string[] arrParmOut = MySqlClass.MySQLInOut("CheckFullInfoPerson", arrParm, arrParmData);
         
            foreach(var clientU in ServerClass.clients)
            {
                if (clientU.socket == client)
                {
                    clientU.countLike = Int32.Parse(arrParmOut[3]);
                    break;
                }
            }

            ServerClass.SendMsgClient(client, 1024, 3, arrParmOut[0], arrParmOut[1], Int32.Parse(arrParmOut[2]),
                                    Int32.Parse(arrParmOut[3]), arrParmOut[4], arrParmOut[5], arrParmOut[6], arrParmOut[7]);
            
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
            string[] arrParm = { "@loginIn", "@nameIn", "@lastnameIn", "@imageIn", "@emailIn", "@noteIn" };
            string[] arrParmData = { login, infoUser[0], infoUser[1], infoUser[2], infoUser[3], infoUser[4] };
            MySqlClass.MySQLIn("UpdateInfoAboutPerson", arrParm, arrParmData);
        }

        /// <summary>
        /// Изменение пароля (через восстановление по почте)
        /// </summary>
        /// <param name="email">Почта</param>
        /// <param name="password">Пароль</param>
        public static void UpdatePasswordMail(string email, string password)
        {
            string[] arrParm = { "@emailIn", "@passwordIn", };
            string[] arrParmData = { email, password };
            MySqlClass.MySQLIn("UpdatePasswordMail", arrParm, arrParmData);
        }

        #region Проекты/файлы пользователя

        /// <summary>
        /// Добавление свойства проекта в базу
        /// </summary>
        /// <param name="client">Сокет клиента</param>
        /// <param name="login">Логин пользователя</param>
        /// <param name="nameProject">Имя проекта</param>
        public static void AddNewProject(Socket client,string login, string nameProject)
        {
            string[] arrParm = { "@nameUserIn", "@nameProjectIn", "@datePublication" , "@idProjectOut" };
            string[] arrParmData = { login, nameProject , DateTime.Now.ToString("yyyy-MM-dd") };
            string[] arrParmOut = MySqlClass.MySQLInOut("AddProject", arrParm, arrParmData);

            ServerClass.SendMsgClient(client, 64, 1006, arrParmOut[0]);
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
                         $"where `User`.Login = '{login}'";

                MySqlClass.mySQLConn.Open();

                MySql.Data.MySqlClient.MySqlDataReader reader = (new MySql.Data.MySqlClient.MySqlCommand(quary, MySqlClass.mySQLConn)).ExecuteReader();
         
                while (reader.Read())
                {
                    int idProject = reader.GetInt32(0);
                    string name = reader.GetString(2);
                    int countVote = reader.GetInt32(3);
                    double rating = reader.GetDouble(4);
                    string date = reader.GetDateTime(5).ToString("dd-MM-yyyy");
                  
                    string note = (reader.IsDBNull(7) == true) ? string.Empty : reader.GetString(7);
                    string image = (reader.IsDBNull(8) == true) ? string.Empty : reader.GetString(8);

                    string viewApplication = reader.GetString(9);
                  
                    ServerClass.SendMsgClient(client, 2048, 1002, idProject, name, countVote, rating, date, note, image, viewApplication);
                }
            }
            catch(Exception ex) { ServerClass.WriteConsoleMsg("GetListProject : " + ex.Message); }
            finally { MySqlClass.mySQLConn.Close(); }
        }
        
        /// <summary>
        /// Получение списка категорий проекта разделеными символом '#' 
        /// </summary>
        /// <returns>Строка с списком</returns>
        public static string GetListViewApplication()
        {
            string listData = string.Empty;

            try
            {           
                string quary = "select Name from ViewApplication " +
                            "where idViewApplication != -1";
                MySqlClass.mySQLConn.Open();
                MySql.Data.MySqlClient.MySqlDataReader reader = (new MySql.Data.MySqlClient.MySqlCommand(quary, MySqlClass.mySQLConn)).ExecuteReader();

                while (reader.Read())
                {
                    listData += reader.GetString(0) + " #";
                }
            }
            catch (Exception ex) { ServerClass.WriteConsoleMsg("GetListViewApplication : " + ex.Message); }

            finally
            {
                MySqlClass.mySQLConn.Close();            
            }

            return listData;
        }

        /// <summary>
        /// Получение всей информации о проекте из базы
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <param name="loginUser">Логин</param>
        /// <param name="nameProject">Имя проекта</param>
        public static void GetFullInfoForProject(Socket client, string loginUser, string nameProject)
        {
            string[] arrParm = { "@loginUserIn", "@nameProjectIn", "@datePub", "@rating", "@countVote", "@note", "@image", "@viewApp" };
            string[] arrParmData = { loginUser, nameProject };
            string[] arrParmOut = MySqlClass.MySQLInOut("GetInfoProject", arrParm, arrParmData);

            ServerClass.SendMsgClient(client, 512, 8, nameProject, arrParmOut[0],
                 arrParmOut[1], arrParmOut[2], arrParmOut[3], arrParmOut[4], arrParmOut[5]);
        }

        /// <summary>
        /// Изменяет данные у проекта в базе 
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <param name="id">id проекта</param>
        /// <param name="name">Имя</param>
        /// <param name="viewApp">Категория</param>
        /// <param name="note">Описание</param>
        /// <param name="hrefImage">Ссылка на изображение</param>
        public static void UpdateInfoForProject(Socket client,int id,string name, int viewApp, string note, string hrefImage)
        {
            string[] arrParm = { "@idProj", "@nameProj", "@viewApp", "@note", "@hrefImage", "@isRenameProj" , "@oldNameProj" };
            string[] arrParmData = { id.ToString(), name, viewApp.ToString(), note, hrefImage };
            string[] arrParmOut = MySqlClass.MySQLInOut("UpdateInfoForProject", arrParm, arrParmData);

            if (arrParmOut[0].Equals("1"))
            {
                ServerClass.SendMsgClient(client, 512, 1007, name);

                string nameUser = ServerClass.clients.FirstOrDefault(f => f.socket == client).name;
                File.Move(ServerClass.pathProjectFile + "\\" + nameUser + "\\" + arrParmOut[1]  , ServerClass.pathProjectFile + "\\" + nameUser + "\\" + name);
            }

        }

        #endregion 

        #endregion


        /// <summary>
        /// Создание пакетов файла и отправка пакета на сервер
        /// </summary>
        /// <param name="client">Socket клиента</param>
        public static void ContinueSendFile(Socket client)
        {
            foreach(var fileSend in listFileSend)
            {
                if (fileSend.user.socket == client)
                {
                    if (fileSend != null && fileSend.progressSend.Length != 0)
                    {
                        uint lengthFile = (uint)fileSend.progressSend.Length;
                        int nextPacketSize = (int)((lengthFile - fileSend.progress > FileSett.bufferSize) ? FileSett.bufferSize : lengthFile - fileSend.progress);

                        if (fileSend.progress < lengthFile)
                        {
                            MemoryStream packet = new MemoryStream(new byte[nextPacketSize + 8], 0, nextPacketSize + 8, true, true);

                            ServerClass.SendFileClient(fileSend.user.socket, 520, 1004, fileSend.progress, packet, nextPacketSize, fileSend.progressSend);
                        }
                        else
                        {
                            ServerClass.SendMsgClient(client, 16, 1005);
                            listFileSend.Remove(fileSend);
                        }

                        fileSend.progress += nextPacketSize;

                        break;
                    }
                }
            }
        }   

        /// <summary>
        /// Получение файлов от клиента (по пакетам)
        /// </summary>
        /// <param name="fileSize">Записываемый буффер</param>  
        /// <param name="infoFile">Количество байт полученных от клиента</param>     
        /// <param name="countRecByte">Размер пакета</param>     
        internal static void ReceivedFile(FileSett file, byte[] infoFile, int countRecByte)
        {
            /*
            if (file.progressSend == null)           
                file.progressSend = infoFile;
            
            else
            {
                byte[] buf = file.progressSend;
                Array.Resize(ref file.progressSend, countRecByte + file.progressSend.Length);
                Buffer.BlockCopy(buf, 0, file.progressSend, 0, buf.Length);
                Buffer.BlockCopy(infoFile, 0, file.progressSend, file.progress, countRecByte);
            }
            */
            
            Buffer.BlockCopy(infoFile, 0, file.progressSend, file.progress - countRecByte, countRecByte);

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