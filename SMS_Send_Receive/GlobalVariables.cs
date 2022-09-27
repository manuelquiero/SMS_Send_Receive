using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Send_Receive
{
    class GlobalVariables
    {
        public static string port { get; set; }
        public static bool isSending { get; set; } = false;
    }
    public class Inbox
    {
        public int id { get; set; }
        public string number { get; set; }
        public string message { get; set; }
        public string dateReceived { get; set; }
        public bool isRead { get; set; }
    }
    public class MultiSMS
    {
        public string refno { get; set; }
        public string total { get; set; }
        public string currentPart { get; set; }
        public string pduMsg { get; set; }
        public string phone { get; set; }
    }
}
