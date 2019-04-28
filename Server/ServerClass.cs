using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace ServerDiplom
{
    class ServerClass
    {

        private const int port = 5455; // Порт для прослушивания подключений

        public static List<Client> clients = new List<Client>(); // Список юзеров которые успешно вошли;
        public static List<FileSett> fileSettList = new List<FileSett>(); // Список файлов которые отправляются на серверв в данный момент;
        private static Socket soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Принимающий сокет;

        public static int nextID = 0; // Уникальный id юзера;


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

                        string login, password;
                        string[] infoUser = new string[4];

                        switch (idOperationMySql)
                        {
                            //Connect 1.
                            case 1:
                                login = reader.ReadString();
                                password = reader.ReadString();
                                OperationServerAtClient.CheckLoginAndPasswordUser(client, login, password);                               
                                break;

                            // CheckData 2. Проверка на валидность данных авторизации юзера в базе;
                            case 2:
                                login = reader.ReadString();
                                OperationServerAtClient.CheckUniqueLogin(client, login);
                                break;

                            // AddUser 3. Добавляет в базу нового пользователя;
                            case 3:
                                login = reader.ReadString();
                                password = reader.ReadString();
                                OperationServerAtClient.CreateNewUser(login, password);
                                break;

                            // InfoUser 4. Узнаем информацию о юзере;
                            case 4:
                                login = reader.ReadString();
                                OperationServerAtClient.CheckFullInfoOfPerson(client, login);
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

                            #region Работа с получением файлов

                            // Получение свойств файла от клиента;
                            case 1001:
                                foreach(var clientS in clients)
                                {
                                    if(clientS.socket == client)
                                    {
                                        int sizeFile = reader.ReadInt32();
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
                                            OperationServerAtClient.ReceivedFile(fileSend, byteFile, countRecByte);
                                            fileSend.progress += countRecByte;
                                        }
                                        else
                                        {
                                            Console.WriteLine(fileSend.progress + "  " + fileSend.progressSend.Length);
                                          
                                            OperationServerAtClient.ReceivedFile(fileSend, byteFile, countRecByte);
                                            fileSend.progress += countRecByte;                             
                                        }
                                        break;
                                    }
                                }
                                break;

                            // Создание файла по полученным данным от клиента;
                            case 1003:
                                foreach (var fileSend in fileSettList)
                                {
                                    if (fileSend.user.socket == client)
                                    {
                                        string nameFile = fileSend.nameF + fileSend.extensionFile;
                                        File.WriteAllBytes(nameFile, fileSend.progressSend);
                                        fileSettList.Remove(fileSend);
                                    }
                                        break;
                                }                               
                                break;
                                #endregion
                        }
                    }
                }
            }
          /*  catch (Exception ex)
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
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }*/
            finally
            {            
                Thread.CurrentThread.Abort();          
            }
           
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

    }
}
