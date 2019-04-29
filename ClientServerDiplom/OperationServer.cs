using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClientServerDiplom
{
    abstract public class OperationServer
    {

        private const int port = 5455;
        private const string server = "127.0.0.1";

        public static Socket serverSocket; // Сокет для подключения к серверу
        public static Thread thread; // Поток для получения данных от сервера;
        
        public static FileSend fileSend { get; set; } // Файл который будет отправлятся на сервер;
        private const int sizePacket = 1024; // Размер пакета который отправляется на сервер;

        /// <summary>
        /// Получение ответа от сервера
        /// </summary>
        /// <param name="soketClient">Сокет клиента</param>
        private static void GettingAnswerServer(object soketClient)
        {
            Socket soket = (Socket)soketClient;

            MemoryStream ms = new MemoryStream(new byte[2048], 0, 2048, true, true);
            BinaryReader reader = new BinaryReader(ms);
            int bytesSend = 0;
            try
            {
                while (true)
                {
                    soket.Receive(ms.GetBuffer());
                    ms.Position = 0;

                    int idOperation = reader.ReadInt32();
                    switch (idOperation)
                    {

                        case 1:
                            #region Проверка данных на валидность (Connect 1)

                            switch (reader.ReadBoolean())
                            {
                                case true:
                                    Authorization.GoToPersonalArea();
                                    break;

                                case false:
                                    MessageBox.Show("Неверный логин или пароль!");
                                    break;
                            }
                            #endregion
                            break;

                        case 2:
                            #region Проверка на повторяющийся логин (CheckDataUser 1)

                            switch (reader.ReadBoolean())
                            {
                                case true:
                                    MessageBox.Show("Регистрация прошла успешно!");
                                    Regestration.CreateNewPerson();
                                    break;

                                case false:
                                    MessageBox.Show("Такой логин уже существует!");
                                    break;
                            }
                            #endregion
                            break;

                        case 3:
                            #region Получение ответа от сервера насчёт данных о данном пользователе (CheckFullInfoOfPerson 2)

                            Person.name = reader.ReadString();
                            Person.lastname = reader.ReadString();
                            Person.level = reader.ReadInt32();
                            Person.likes = reader.ReadInt32();
                            Person.image = reader.ReadString();
                            Person.email = reader.ReadString();

                            PersonalArea.SetPersonalInfo();
                            #endregion
                            break;

                        case 4:

                            break;

                        case 5:
                            #region Получение в изменении количества лайков на профиле пользователя от сервера

                            MessageBox.Show("321");

                            #endregion
                            break;


                        #region Работа с файлами (1000 - 1999)

                        case 1001:
                            #region Отправка файла серверу

                            if (fileSend != null && fileSend.fileByte.Length != 0)
                            {                               
                                int lengthFile = fileSend.fileByte.Length;                         
                                int nextPacketSize = (lengthFile - bytesSend > FileSend.bufferSize) ? FileSend.bufferSize : lengthFile - bytesSend;                            

                                if (bytesSend < lengthFile)
                                {
                                    MemoryStream packet = new MemoryStream(new byte[nextPacketSize + 8], 0, nextPacketSize + 8, true, true);
                                    
                                    SendFile(520, 1002, bytesSend , packet, nextPacketSize);                         
                                }
                                else
                                {
                                    SendMsgClient(16, 1003);
                                    bytesSend = 0;
                                }
                                bytesSend += nextPacketSize;
                            }
                            #endregion
                            break;

                        case 1002:
                            #region  Получение списка проектов сохраненные на сервере;

                            string name = reader.ReadString();
                            int countVote = reader.ReadInt32();
                            double rating = reader.ReadDouble();
                            string date = reader.ReadString();
                            string note = reader.ReadString();
                            string image = reader.ReadString();
                            string viewApplication = reader.ReadString();

                            Person.listProject.Add(new Project(name, countVote, rating, date, viewApplication, note, image));
                         
                            #endregion
                            break;

                        #endregion
                    }

                    if (!soket.Connected)
                    {
                        break;
                    }

                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("GettingAnswerServer  :  " + ex.Message);

                try
                {
                    Application.Current.Dispatcher.Invoke(new ThreadStart(()=> 
                    {                  
                        Application.Current.Shutdown();
                    
                    }));
                }
                catch { }
            }
            finally
            {
                Thread.CurrentThread.Abort();                
                soket.Close();               
            }
        }   

        /// <summary>
        /// Подключение к серверу.
        /// </summary>
        public static void Connected()
        {
            if (serverSocket == null)
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(server), port);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(ipPoint);

                thread = new Thread(GettingAnswerServer);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start(serverSocket);
            }
        }

        /// <summary>
        /// Отправляет ответ Серверу
        /// </summary>    
        /// <param name="memoryBit">Кол-во памяти</param>
        /// <param name="idOperation">Идентификатор опирации</param>
        /// <param name="sendArrData">Данные который получит сервер</param>
        public static void SendMsgClient( int memoryBit, int idOperation, params dynamic[] sendArrData)
        {
            try
            {
                MemoryStream msTF = new MemoryStream(new byte[memoryBit], 0, memoryBit, true, true);
                BinaryWriter writer = new BinaryWriter(msTF);

                writer.Write(idOperation);

                for (int i = 0; i < sendArrData.Length; i++)
                {
                    if (sendArrData[i].GetType() != typeof(MemoryStream))
                        writer.Write(sendArrData[i]);
                }
                serverSocket.Send(msTF.GetBuffer());
            }
            catch(Exception ex)  { MessageBox.Show("SendMsgClient : " + ex.Message); }
        }


        /// <summary>
        /// Отправляем файл Серверу
        /// </summary>    
        /// <param name="memoryBit">Кол-во памяти</param>
        /// <param name="idOperation">Идентификатор опирации</param>
        /// <param name="countSendByte">Количество отправленных байт</param>
        /// <param name="sendPacket">Байты пакета</param>
        /// <param name="countSendByte">Количество отправляемых байт</param>
        public static void SendFile(int memoryBit, int idOperation, int countSendingByte ,MemoryStream sendPacket, int countSendByte)
        {
            BinaryWriter writer = new BinaryWriter(sendPacket);
            writer.Write(idOperation);  
            writer.Write(countSendByte);
            
            Buffer.BlockCopy(fileSend.fileByte, countSendingByte, sendPacket.GetBuffer(), 8, countSendByte);

            serverSocket.Send(sendPacket.GetBuffer());
        }
    }
}
