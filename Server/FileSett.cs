using ServerDiplom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// Свойства файла который отправляется на сервер
    /// </summary>
    internal sealed class FileSett
    {
        public FileSett(string nameF,string extensionFile, uint sizeF, Client user)
        {
            this.nameF = nameF;
            this.extensionFile = "." + extensionFile;
            this.sizeF = sizeF;
            this.user = user;

            progressSend = new byte[sizeF];
        }

        public FileSett(string nameF, byte[] fileByte, Client user)
        {
            this.nameF = nameF;
            this.user = user;

            progressSend = fileByte;
        }

        public string nameF { get; private set; } // Имя файла;

        public string extensionFile { get; private set; } // Расширение файла;

        public uint sizeF { get; private set; } // Размер файла;

        public byte[] progressSend; // Прогресс отправки файла; 

        public int progress { get; set; } // Количество полученных байт;

        public const int bufferSize = 512; // Максимальный размер пакета; 

        public Client user  { get; private set; } // Отправитель;
     
    }
}
