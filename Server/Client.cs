using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ServerDiplom
{
    internal class Client
    {

        public int id { get; set; }

        public string name { get; set; }  
       
        public Socket socket { get; set; }
    
    }
}
