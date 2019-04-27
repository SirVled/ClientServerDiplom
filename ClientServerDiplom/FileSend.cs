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

        public FileSend(byte[] fileByte, string nameF)
        {      
            this.fileByte = fileByte;
            OperationServer.SendMsgClient(256, 1001, nameF , fileByte.Length);
            OperationServer.SendMsgClient(1032, 1002, new System.IO.MemoryStream(fileByte, 0, 1024, true ,true), 1024);
        }

        public byte[] fileByte { get; private set; } // Файл в байтах;

        public int countSendByte { get; set; } // Количество отправленных байтов;

        public int countIteration = 0; // Количество пакетов которые были отправлены на сервер; 

        public int countIterationError = 0; // Количество ошибок в одной последовательности итераций в передаче файла к серверу;  
       
    }
}
