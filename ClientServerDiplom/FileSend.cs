namespace ClientServerDiplom
{
    /// <summary>
    /// Файл отправки на сервек
    /// </summary>
    public class FileSend
    {

        public FileSend(byte[] fileByte, string nameF,string extensionFile)
        {      
            this.fileByte = fileByte;
            nameFile = nameF;
            OperationServer.SendMsgClient(512, 1001, this.fileByte.Length, nameF , extensionFile);
         //   OperationServer.SendMsgClient(1032, 1002, new System.IO.MemoryStream(fileByte, 0, 1024, true ,true), 1024);
        }

        public FileSend(int sizeBuff, string nameF)
        {
            nameFile = nameF;

            fileByte = new byte[sizeBuff];
        }

        public string nameFile { get; set; } // Имя отправляющего файла;

        public byte[] fileByte { get; private set; } // Файл в байтах;
    
        public const int bufferSize = 512; // Максимальный размер пакета, который будет отправляться на сервер; 

        public int bytesSend = 0; // Количество отправденных байтов;

    }
}
