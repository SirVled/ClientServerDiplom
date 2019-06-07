using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerDiplom
{
    class ServerClass
    {

        private const int port = 5455; // Порт для прослушивания подключений

        public static List<Client> clients = new List<Client>(); // Список юзеров которые успешно вошли;
        public static List<FileSett> fileSettList = new List<FileSett>(); // Список файлов которые отправляются на серверв в данный момент;
        private static Socket soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Принимающий сокет;

        public static int nextID = 0; // Уникальный id юзера;

        public static readonly string pathProjectFile = (Environment.CurrentDirectory + @"\Project File"); // Путь к файлам пользователей которые храняться на сервере;

        #region Интерфейсы

        private static IPeople workPeople = new OperationServerAtClient(); // Интерфейс для работы с новостной лентой;

        #endregion

        static void Main(string[] args)
        {
            soket.Bind(new IPEndPoint(IPAddress.Any, port));
            soket.Listen(0);

            soket.BeginAccept(AceptCallback, null);
            
            Console.WriteLine("Хост на связе!");
            Console.ReadLine();
        }

        /// <summary>
        /// Получаем нового клиента 
        /// </summary>
        /// <param name="clientEr">Клиент который подключился к серверу</param>
        private static void AceptCallback(IAsyncResult clientEr)
        {
            Socket client = soket.EndAccept(clientEr);
            Console.WriteLine("Подключение нового пользователя!");
            Thread thread = new Thread(CoorCallback);
            thread.Start(client);
            
            soket.BeginAccept(AceptCallback, null);
        }

        /// <summary>
        /// Поток для прослушивания клиентов
        /// </summary>
        /// <param name="obj">Сокет клиента</param>
        private static void CoorCallback(object obj)
        {
            Socket client = (Socket)obj;
            MemoryStream ms = new MemoryStream(new byte[520], 0, 520, true, true);
            BinaryReader reader = new BinaryReader(ms);
           
            try
            {
                while (true)
                {
                    if (client.Connected)
                    {
                        client.Receive(ms.GetBuffer());
                        ms.Position = 0;

                        int idOperationMySql = reader.ReadInt32();

                        string login, password, email, nameProject;
                        string[] infoUser = new string[5];

                        switch (idOperationMySql)
                        {

                            #region Работа с базой и сервером (1 - 999)

                            // Disconnect -1.
                            case -1:
                                login = reader.ReadString();
                                OperationServerAtClient.Disconnect(login);
                                break;

                            // Connect 1.
                            case 1:
                                login = reader.ReadString();
                                password = reader.ReadString();
                                OperationServerAtClient.CheckLoginAndPasswordUser(client, login, password);                               
                                break;

                            // CheckData 2. Проверка на валидность данных авторизации юзера в базе;
                            case 2:
                                login = reader.ReadString();
                                email = reader.ReadString();
                                OperationServerAtClient.CheckUnique(client, login, email);
                                break;

                            // AddUser 3. Добавляет в базу нового пользователя;
                            case 3:
                                login = reader.ReadString();
                                password = reader.ReadString();
                                email = reader.ReadString();
                                OperationServerAtClient.CreateNewUser(login, password, email);
                                break;

                            // InfoUser 4. Узнаем информацию о юзере;
                            case 4:
                                login = reader.ReadString();
                                OperationServerAtClient.CheckFullInfoOfPerson(client, login, reader.ReadBoolean());
                                break;

                            // CheckNewLike 5. Проверяем изменение в количестве лайков у пользователя;
                            case 5:
                                login = reader.ReadString();
                                OperationServerAtClient.CheckNewLike(client, login);
                                break;

                            // AddNewInfoOfPerson 6. Добавлем новую информацию о пользователе;
                            case 6:
                                login = reader.ReadString();
                                for (int i = 0; i < infoUser.Length; i++)
                                    infoUser[i] = reader.ReadString();
                                OperationServerAtClient.AddNewInfoForPerson(login, infoUser);
                                break;

                            // UpdateInfoAboutPerson 7. Обновляем информацию о пользователе;
                            case 7:
                                login = reader.ReadString();
                                for (int i = 0; i < infoUser.Length; i++)
                                    infoUser[i] = reader.ReadString();
                                OperationServerAtClient.UpdateInfoAboutPerson(login, infoUser);
                                break;

                            // GetViewApplication 8. Получение списка категорий приложения;
                            case 8:
                                SendMsgClient(client, 256, 7, OperationServerAtClient.GetListViewApplication());
                                break;
                            
                            //GetFullInfoForProject 9. Получение всей информации о выбранном проекте;
                            case 9:
                                login = reader.ReadString();
                                nameProject = reader.ReadString();

                                OperationServerAtClient.GetFullInfoForProject(client, login, nameProject);
                                break;

                            //UpdateInfoForProject 10. Изменяем информацию о проекте;
                            case 10:                          
                                OperationServerAtClient.UpdateInfoForProject(client,reader.ReadInt32(), reader.ReadString(), reader.ReadInt32(), reader.ReadString(), reader.ReadString());
                                break;
                            #region Работа с почтой
                            // CheckEmail 101. Проверка почты пользователя;
                            case 101:
                                email = reader.ReadString();
                                OperationServerAtClient.CheckUnique(client, string.Empty, email,4);
                                break;

                            // SendMailCode 102. Отправка кода восстановления на почту пользователя;
                            case 102:
                                email = reader.ReadString();
                                int code = EmailBot.TextCode();

                                string msg = "Код восстановления : " + code;
                                EmailBot.SendMail(EmailBot.smtp, EmailBot.mail, EmailBot.password, email, EmailBot.themeMsg, msg);
                                SendMsgClient(client, 64, 6, code);
                                break;

                            // UpdatePassword 103. Отправка пароля на почту, и изменение его в базе;
                            case 103:
                                email = reader.ReadString();
                                password = EmailBot.RandomPassword(8);
                                msg = "Новый пароль : " + password;

                                EmailBot.SendMail(EmailBot.smtp, EmailBot.mail, EmailBot.password, email, EmailBot.themeMsg, msg);
                                OperationServerAtClient.UpdatePasswordMail(email, password);
                                break;
                            #endregion 

                            #endregion

                            #region Работа с файлами которые храняться на сервере (1000 - 1999) 

                            // Получение свойств файла от клиента;
                            case 1001:
                                foreach(var clientS in clients)
                                {
                                    if(clientS.socket == client)
                                    {
                                        uint sizeFile = reader.ReadUInt32();
                                        fileSettList.Add(new FileSett(reader.ReadString(),reader.ReadString(), sizeFile, clientS));
                                        Console.WriteLine("Размер файла : " + sizeFile);
                                        
                                        SendMsgClient(client, 16, 1001);
                                        break;
                                    }
                                }
                                break;

                            // Получение пакетов;
                            case 1002:
                                foreach (var fileSend in fileSettList)
                                {
                                    if (fileSend.user.socket == client)
                                    {
                                        int countRecByte = reader.ReadInt32();
                                        byte[] byteFile = reader.ReadBytes(countRecByte);
                    
                                        if (fileSend.progressSend == null)
                                        {
                                            fileSend.progress += countRecByte;
                                            OperationServerAtClient.ReceivedFile(fileSend, byteFile, countRecByte);                                      
                                        }
                                        else
                                        {
                                            fileSend.progress += countRecByte;
                                            Console.WriteLine(fileSend.progress + "  " + fileSend.progressSend.Length);
                                            OperationServerAtClient.ReceivedFile(fileSend, byteFile, countRecByte);                                                                 
                                        }

                                        if(fileSend.progressSend.Length <= fileSend.progress || countRecByte == 0)
                                        {
                                            Console.WriteLine(fileSend.progress + "  " + (fileSend.progressSend.Length));
                                            CreateFile(client);
                                        }

                                        break;
                                    }
                                }
                                break;

                            // Создание файла по полученным данным от клиента;
                            case 1003:
                                CreateFile(client);
                                break;

                            // Удаление файла с архива;
                            case 1004:
                                foreach (var clientU in clients)
                                {
                                    if (clientU.socket == client)
                                    {
                                        string directoryUser = pathProjectFile + "\\" + clientU.name;

                                        string nameFile = reader.ReadString();
                                        WriteConsoleMsg(clientU.name + " удалил файл : " + nameFile);
        
                                        string fullPathFile = directoryUser + "\\" + nameFile;

                                        OperationServerAtClient.DeleteProjectDatabase(clientU.name, nameFile);
                                        File.Delete(fullPathFile);

                                        break;
                                    }                                
                                }
                                break;

                            // Получение списка проектов у данного пользователя;
                            case 1005:
                                OperationServerAtClient.GetListProject(client, reader.ReadString(),reader.ReadBoolean());                                                          
                                break;

                            // Получение массив байт файла который выбрал клиент для скачивания;
                            case 1006:          
                                foreach (var user in clients)
                                {
                                    if (user.socket == client)
                                    {
                                        string nameUser = reader.ReadString();
                                        string nameFile = reader.ReadString();

                                        string pathFile = $"{pathProjectFile}\\{nameUser}\\{nameFile}";

                                        byte[] fileByte = File.ReadAllBytes(pathFile);
                                        OperationServerAtClient.listFileSend.Add(new FileSett(nameFile, fileByte, user));
                                        Console.WriteLine($"{user.name} начиниет получать файл размера : {fileByte.Length}");

                                        SendMsgClient(user.socket, 256, 1003, fileByte.Length,nameFile);

                                        break;
                                    }
                                }
                                break;

                            // Отправка файла по пакетам;
                            case 1007:
                                OperationServerAtClient.ContinueSendFile(client);
                                break;
                            #endregion

                            #region Работа с людьми\проектами и т.д (2000 - 3000)

                            // Поиск людей по введеной строке;
                            case 2000:
                                workPeople.SearchPeople(client,reader.ReadString());
                                break;
                            // Получение списока проектов с лучшим рейтингом;
                            case 2001:
                                workPeople.SendTopProject(client);
                                break;
                            // Получение рандомных людей;
                            case 2002:
                                workPeople.RandomPeople(client, reader.ReadString());
                                break;
                            // Подписка\Отписка от данного пользователя;
                            case 2003:                            
                                string profile = reader.ReadString();
                                string subUser = reader.ReadString();
                                bool isSub = reader.ReadBoolean();
                                workPeople.SubscribeUser(profile, subUser, isSub);
                                break;
                            // Проверка на подписку пользователя;
                            case 2004:
                                profile = reader.ReadString();
                                subUser = reader.ReadString();
                                SendMsgClient(client,128,2003,workPeople.CheckSubscribe(profile, subUser));
                                break;
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (client != null)
                {
                    client.Close();

                    foreach (var clientU in clients)
                    {
                        if (clientU.socket.Equals(client))
                        {
                            clients.Remove(clientU);
                            break;
                        }
                    }
                }
                WriteConsoleMsg(ex.Message);
            }
            finally
            {            
                Thread.CurrentThread.Abort();          
            }
           
        }

        /// <summary>
        /// Создание файла по полученным байтом от клиента
        /// </summary>
        /// <param name="client">Клиент</param>
        private static void CreateFile(Socket client)
        {
            foreach (var fileSend in fileSettList)
            {
                if (fileSend.user.socket == client)
                {
                    string directoryUser = pathProjectFile + "\\" + fileSend.user.name;

                    if (!Directory.Exists(Path.GetExtension(directoryUser)))
                    {
                        Directory.CreateDirectory(directoryUser);
                    }
                    string nameFile = directoryUser + "\\" + fileSend.nameF + fileSend.extensionFile;

                    OperationServerAtClient.AddNewProject(client, fileSend.user.name, (fileSend.nameF + fileSend.extensionFile));

                    File.WriteAllBytes(nameFile, fileSend.progressSend);
                    fileSettList.Remove(fileSend);

                    break;
                }

            }
        }

        /// <summary>
        /// Печать в консоль сообщение
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public static void WriteConsoleMsg(string message)
        {
            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine();
        }
       
        /// <summary>
        /// Отправляет ответ Клиенту
        /// </summary>
        /// <param name="client">Сокет пользователя</param>
        /// <param name="memoryBit">Кол-во памяти</param>
        /// <param name="idOperation">Идентификатор опирации</param>
        /// <param name="sendArrData">Данные который получит пользователь</param>
        public static void SendMsgClient(Socket client, int memoryBit, int idOperation, params dynamic[] sendArrData)
        {
            MemoryStream msTF = new MemoryStream(new byte[memoryBit], 0, memoryBit, true, true);
            BinaryWriter writer = new BinaryWriter(msTF);

            writer.Write(idOperation);

            for (int i = 0; i < sendArrData.Length; i++)
                writer.Write(sendArrData[i]);

            client.Send(msTF.GetBuffer());
        }

        /// <summary>
        /// Отправляем файл Клиенту
        /// </summary>    
        /// <param name="client">Клиент</param>
        /// <param name="memoryBit">Кол-во памяти</param>
        /// <param name="idOperation">Идентификатор опирации</param>
        /// <param name="countSendByte">Количество отправленных байт</param>
        /// <param name="sendPacket">Байты пакета</param>
        /// <param name="countSendByte">Количество отправляемых байт</param>
        /// <param name="file">Файл который отправляем в байтах</param>
        public static void SendFileClient(Socket client,int memoryBit, int idOperation, int countSendingByte, MemoryStream sendPacket, int countSendByte, byte[] file)
        {
            BinaryWriter writer = new BinaryWriter(sendPacket);
            writer.Write(idOperation);
            writer.Write(countSendByte);

            Buffer.BlockCopy(file, countSendingByte, sendPacket.GetBuffer(), 8, countSendByte);

            client.Send(sendPacket.GetBuffer());
        }
    }
}
