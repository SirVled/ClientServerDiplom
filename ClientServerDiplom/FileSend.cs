using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            OperationServer.SendMsgClient(512, 1001, this.fileByte.Length, nameF , extensionFile);
         //   OperationServer.SendMsgClient(1032, 1002, new System.IO.MemoryStream(fileByte, 0, 1024, true ,true), 1024);
        }

        public byte[] fileByte { get; private set; } // Файл в байтах;

        public int countSendByte { get; set; } // Количество отправленных байтов;

        public const int bufferSize = 512; // Максимальный размер пакета, который будет отправляться на сервер; 

    }
}
