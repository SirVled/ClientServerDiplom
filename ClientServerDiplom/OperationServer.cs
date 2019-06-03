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
using System.Windows.Threading;
using static ClientServerDiplom.Project;

namespace ClientServerDiplom
{
    abstract public class OperationServer
    {

        private const int port = 5455;
        private const string server = "127.0.0.1";

        public static Socket serverSocket; // Сокет для подключения к серверу
        public static Thread thread; // Поток для получения данных от сервера;
        
        public static FileSend fileSend { get; set; } // Файл, который будет отправлятся на сервер;
        public static FileSend fileReceiving { get; set; } // Файл, который скачивается из сервера;
        
        /// <summary>
        /// Получение ответа от сервера
        /// </summary>
        /// <param name="soketClient">Сокет клиента</param>
        private static void GettingAnswerServer(object soketClient)
        {
            Socket soket = (Socket)soketClient;

            MemoryStream ms = new MemoryStream(new byte[2048], 0, 2048, true, true);
            BinaryReader reader = new BinaryReader(ms);
            
            try
            {
                while (true)
                {
                    // Если файл пользователь отправлял файл, продолжить отправку файла в другом окне;
                    if (fileSend != null)
                    {
                        ContinueSendFile(ref fileSend.bytesSend);
                    }

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
                                    Authorization.thisWindow.Dispatcher.BeginInvoke(new ThreadStart(()=> { 
                                    Authorization.thisWindow.signIn.IsEnabled = true;
                                    MessageBox.Show("Неверный логин или пароль!");
                                    }));
                                    break;
                            }
                            #endregion
                            break;

                        case 2:
                            #region Проверка на повторяющийся логин (CheckDataUser 2)

                            switch (reader.ReadBoolean())
                            {
                                case true:
                                    MessageBox.Show("Регистрация прошла успешно!");
                                    Authorization.CreateNewPerson();
                                    break;

                                case false:
                                    MessageBox.Show("Такой логин\\email уже существует!");
                                    break;
                            }
                            #endregion
                            break;

                        case 3:
                            #region Получение ответа от сервера насчёт данных о данном пользователе (CheckFullInfoOfPerson 3)

                            Person.thisUser.name = reader.ReadString();
                            Person.thisUser.lastname = reader.ReadString();
                            Person.thisUser.level = reader.ReadInt32();
                            Person.thisUser.likes = reader.ReadInt32();
                            Person.thisUser.image = reader.ReadString();
                            Person.thisUser.email = reader.ReadString();
                            Person.thisUser.countProject = Int32.Parse(reader.ReadString());
                            Person.thisUser.note = reader.ReadString();

                            PersonalArea.SetPersonalInfo();
                            #endregion
                            break;

                        case 4:
                            #region Проверка на наличие введенной почты в базе (CheckUnique 4)
                            SendMailPass.thisWindow.ShowPanelCode(!reader.ReadBoolean());
                            #endregion
                            break;

                        case 5:
                            #region Получение в изменении количества лайков на профиле пользователя от сервера

                            MessageBox.Show("321");

                            #endregion
                            break;

                        case 6:
                            #region Получение ответ от сервера по поводу отправки кода восстановления;
                            SendMailPass.codeU = reader.ReadInt32();
                            #endregion
                            break;

                        case 7:
                            #region Получение списка с категориями проекта
                            YourProject.thisWindow.Dispatcher.BeginInvoke(new ThreadStart(() =>{ 
                                foreach (string item in reader.ReadString().Split('#').ToList())
                                    YourProject.thisWindow.comboBoxTypeProj.Items.Add(item);

                                YourProject.thisWindow.comboBoxTypeProj.Items.RemoveAt(YourProject.thisWindow.comboBoxTypeProj.Items.Count - 1);
                                SendMsgClient(256, 9, Person.thisUser.login, (YourProject.thisWindow.listViewProjects.SelectedValue as Project.MyItemProject).nameProject);
                            }));
                            #endregion
                            break;

                        case 8:
                            #region Получение информации о проекте (YouProject)
                            YourProject.thisWindow.Dispatcher.BeginInvoke(new ThreadStart(() =>
                            {
                                string x = reader.ReadString();
                                string b = reader.ReadString();
                                double c = double.Parse(reader.ReadString().Replace('.', ','));
                                int y = Int32.Parse(reader.ReadString());
                                string n = reader.ReadString();
                                string s = reader.ReadString();
                                YourProject.SetInfoForSettingsPanel(YourProject.thisWindow, x, b,
                                   c, y, n, s, Int32.Parse(reader.ReadString()));
                                //YourProject.SetInfoForSettingsPanel(YourProject.thisWindow, reader.ReadString(), reader.ReadString(),
                                //        double.Parse(reader.ReadString().Replace('.', ',')), Int32.Parse(reader.ReadString()),
                                //        reader.ReadString(), reader.ReadString(), Int32.Parse(reader.ReadString()));

                                YourProject.thisWindow.settingsPanel.Visibility = Visibility.Visible;                               
                            }));
                            #endregion
                            break;

                        #region Работа с файлами (1000 - 1999)

                        case 1001:
                            #region Отправка файла серверу

                            if(fileSend != null)
                                ContinueSendFile(ref fileSend.bytesSend);

                            #endregion
                            break;

                        case 1002:
                            #region  Получение списка проектов сохраненные на сервере;

                            int idProject = reader.ReadInt32();
                            string name = reader.ReadString();
                            int countVote = reader.ReadInt32();
                            double rating = reader.ReadDouble();
                            string date = reader.ReadString();
                            string note = reader.ReadString();
                            string image = reader.ReadString();
                            string viewApplication = reader.ReadString();

                            Person.thisUser.listProject.Add(new Project(idProject, name, countVote, rating, date, viewApplication, note, image));
                         
                            #endregion
                            break;

                        case 1003:
                            #region Получение свойств файла который будет отправлятся от сервера;
                            fileReceiving = new FileSend(reader.ReadInt32(), reader.ReadString());
                            SendMsgClient(16, 1007);

                            YourProject.IsEnabledForm(false);
                            YourProject.SetSettingsPanelLoad(YourProject.thisWindow, fileReceiving.nameFile,false);
                            #endregion
                            break;

                        case 1004:
                            #region Получение пакетов файла;

                            int countRecByte = reader.ReadInt32();
                            byte[] byteFile = reader.ReadBytes(countRecByte);                           

                            ReceivedFile(fileReceiving, byteFile, countRecByte);
                            fileReceiving.bytesSend += countRecByte;

                            ///Отображение прогресса отправки
                            if (YourProject.loadUIPB != null)
                            {
                                double percent = ((double)fileReceiving.bytesSend / fileReceiving.fileByte.Length) * 100;
                                YourProject.SetValueProgressLoad((int)percent,true);
                            }
                            #endregion
                            break;

                        case 1005:
                            #region Создание файла по полученным байтам;                         
                            string nameFile = $"Project File\\{fileReceiving.nameFile}";
                            File.WriteAllBytes(nameFile, fileReceiving.fileByte);
                            fileReceiving = null;
                            #endregion


                            break;

                        case 1006:
                            #region Добавление проекта в лист;
                            YourProject.thisWindow.Dispatcher.BeginInvoke(new ThreadStart(() =>
                            {
                                YourProject.AddProjectToList(Int32.Parse(reader.ReadString()));
                            }));
                            #endregion
                            break;

                        case 1007:
                            #region Изменение имени у проекта 
                            YourProject.thisWindow.Dispatcher.BeginInvoke(new ThreadStart(() =>
                            {
                                YourProject.RenameProject(reader.ReadString());
                            }));
                            #endregion
                            break;
                        #endregion

                        #region Новостная лента(2000-3000)
                        case 2000:
                            #region Поиск людей по введеной строке
                            string loginUser = reader.ReadString();

                            if(!loginUser.Equals("###ThisNull###"))
                            {
                                string imageU = reader.ReadString();
                                int level = Int32.Parse(reader.ReadString());
                                int countProject = Int32.Parse(reader.ReadString());

                                FeedPublic.listSearchPeople.Add(new Person(loginUser, imageU, level, countProject));
                            }
                            else
                            {
                                FeedPublic.thisWindow.Dispatcher.BeginInvoke(new ThreadStart(()=> {
                                    FeedPublic.SetFindsPeople(FeedPublic.listSearchPeople);
                                }));
                            }
                            #endregion
                            break;

                        case 2001:
                            #region Получение списока проектов с лучшим рейтингом;
                            loginUser = reader.ReadString();

                            if (!loginUser.Equals("###ThisNull###"))
                            {
                                string nameProj = reader.ReadString();
                                string imageU = reader.ReadString();
                                double ratingU = double.Parse(reader.ReadString().Replace('.',','));
                                string noteU = reader.ReadString();

                                FeedPublic.listTopProject.Add(new Person(loginUser, new Project(new MyItemProject(nameProj,ratingU),imageU,noteU)));
                            }
                            else
                            {
                                FeedPublic.thisWindow.Dispatcher.BeginInvoke(new ThreadStart(() => {
                                    FeedPublic.SetTopProject(FeedPublic.listTopProject);
                                    SendMsgClient(128, 2002, Person.thisUser.login);
                                }));
                            }
                            #endregion
                            break;

                        case 2002:
                            #region Получение рандомных людей;
                            loginUser = reader.ReadString();

                            if (!loginUser.Equals("###ThisNull###"))
                            {
                                int level = Int32.Parse(reader.ReadString());
                                string imageU = reader.ReadString();
                             
                                FeedPublic.listInterestingPeople.Add(new Person(loginUser,imageU,level,-1));
                            }
                            else
                            {
                                FeedPublic.thisWindow.Dispatcher.BeginInvoke(new ThreadStart(() => {
                                    FeedPublic.PanelRandomPeople(FeedPublic.listInterestingPeople);
                                }));
                            }
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
            //catch(Exception ex)
            //{
            //    MessageBox.Show("GettingAnswerServer  :  " + ex.Message);

            //    try
            //    {
            //        Application.Current.Dispatcher.Invoke(new ThreadStart(()=> 
            //        {                  
            //            Application.Current.Shutdown();
                    
            //        }));
            //    }
            //    catch { }
            //}
            finally
            {
                Thread.CurrentThread.Abort();                
                soket.Close();               
            }
        }

        /// <summary>
        /// Создание пакетов файла и отправка пакета на сервер
        /// </summary>
        /// <param name="bytesSend">Количество отправленных байтов</param>
        private static void ContinueSendFile(ref int bytesSend)
        {
            if (fileSend != null && fileSend.fileByte.Length != 0)
            {
                uint lengthFile = (uint)fileSend.fileByte.Length;
                int nextPacketSize = (int)((lengthFile - bytesSend > FileSend.bufferSize) ? FileSend.bufferSize : lengthFile - bytesSend);

                if (bytesSend < lengthFile && fileSend != null && nextPacketSize != 0)
                {
                    MemoryStream packet = new MemoryStream(new byte[nextPacketSize + 8], 0, nextPacketSize + 8, true, true);

                    SendFile(520, 1002, bytesSend, packet, nextPacketSize);
                    bytesSend += nextPacketSize;
                }
                else
                {
                    fileSend = null;
                    SendMsgClient(16,1003);              
                }

                //if (fileSend != null)
                //    bytesSend += nextPacketSize;

                ///Отображение прогресса отправки
                if (YourProject.loadUIPB != null)
                {
                    double percent = ((double)bytesSend / lengthFile) * 100;
                    YourProject.SetValueProgressLoad((int)percent,false);
                }
            }
        }

        /// <summary>
        /// Получение файлов от сервера (по пакетам)
        /// </summary>
        /// <param name="fileSize">Записываемый буффер</param>  
        /// <param name="infoFile">Количество байт полученных от клиента</param>     
        /// <param name="countRecByte">Размер пакета</param>     
        internal static void ReceivedFile(FileSend file, byte[] infoFile, int countRecByte)
        {
            Buffer.BlockCopy(infoFile, 0, file.fileByte, file.bytesSend, countRecByte);
            SendMsgClient(16, 1007);
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
           // try
            //{     
                MemoryStream msTF = new MemoryStream(new byte[memoryBit], 0, memoryBit, true, true);
                BinaryWriter writer = new BinaryWriter(msTF);

                writer.Write(idOperation);

                for (int i = 0; i < sendArrData.Length; i++)
                {
                    if (sendArrData[i].GetType() != typeof(MemoryStream))
                        writer.Write(sendArrData[i]);
                }
                serverSocket.Send(msTF.GetBuffer());
         
            //}
            // catch(Exception ex)  { MessageBox.Show("SendMsgClient : " + ex.Message); }
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
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              