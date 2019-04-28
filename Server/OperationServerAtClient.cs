using ServerDiplom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Server
{
    public abstract class OperationServerAtClient
    {

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
    }
}