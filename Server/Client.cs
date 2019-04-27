using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ServerDiplom
{
    public class Client
    {

        public int id { get; set; }

        public string name { get; set; } 
     
        public int countLike { get; set; } // Кол-во лайков на профиле у пользователя;

        public Socket socket { get; set; }

    }
}
