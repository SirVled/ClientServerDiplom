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
    class Program
    {

        private const int port = 5455; // Порт для прослушивания подключений

        private static List<Client> clients = new List<Client>(); // Список юзеров;
        private static Socket soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static int nextID = 0; // Уникальный id юзера;


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
            Console.WriteLine("Опа, новый чел");
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
            MemoryStream ms = new MemoryStream(new byte[256], 0, 256, true, true);
            BinaryReader reader = new BinaryReader(ms);
            
            try
            {
                while (true)
                {
                    if (client.Connected)
                    {
                        client.Receive(ms.GetBuffer());
                        ms.Position = 0;
                        switch (reader.ReadInt32())
                        {
                            //Connect 0
                            case 0:
                                string quary = "select Login, Password from indentification";
                                List<List<dynamic>> arrData = MySqlClass.MySqlQuaryOut(quary,1,2);

                                string login = reader.ReadString();
                                string password = reader.ReadString();
                                Console.WriteLine("123");
                                bool sendTrueAuthorization = false;
                                for (int i = 0; i < arrData[0].Count; i++)
                                {
                                    if(arrData[0][i] == login && arrData[1][i] == password)
                                    {
                                        Connect(nextID, login, client);
                                        nextID++;
                                        sendTrueAuthorization = true;
                                        break;
                                    }
                                }
                           
                                MemoryStream msTF = new MemoryStream(new byte[64], 0, 64, true, true);
                                BinaryWriter writer = new BinaryWriter(msTF);

                                writer.Write(sendTrueAuthorization);

                                client.Send(msTF.GetBuffer());
                                break;
                        }
                    }
                    Console.WriteLine("12345");
                }
            }
            finally
            {
                Thread.CurrentThread.Abort();
                foreach(var clientU in clients)
                {
                    clientU.socket.Close();
                }
            }
          //  catch { }
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

            clients.Add(cl);
        }
    }
}


//TcpListener server = null;

//              try
//                {
//            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
//server = new TcpListener(localAddr, port);

//server.Server.Listen(0);
//                // запуск слушателя
//                server.Start();

//                while (true)
//                {
//                    Console.WriteLine("Ожидание подключений... ");

//                    // получаем входящее подключение
//                    TcpClient client = server.AcceptTcpClient();
//Console.WriteLine("Подключен клиент. Выполнение запроса...");

//                    // получаем сетевой поток для чтения и записи

//                    byte[] data = new byte[256];
//StringBuilder response = new StringBuilder();
//NetworkStream stream = client.GetStream();

//                    do
//                    {
//                        int bytes = stream.Read(data, 0, data.Length);
//response.Append(Encoding.UTF8.GetString(data, 0, bytes));
//                    }
//                    while (stream.DataAvailable); // пока данные есть в потоке

//                    Console.WriteLine(response.ToString());
//                    stream.Close();
//                    // закрываем подключение
//                    client.Close();
//                }
//            }
//            catch { Console.WriteLine("Пока"); }
//            finally
//            {
//                if (server != null)
//                    server.Stop();
//            }