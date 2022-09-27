using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SMS_Send_Receive
{
    class Encoder
    {
        public static SerialPort serial = new SerialPort();
        static Timer tmrTimeout = new Timer();
        static int timerTimeout = 0;
        static List<MultiSMS> multiSMS = new List<MultiSMS>();
        public static string GetPortInfo()
        {
            string port = null;
            ManagementObjectSearcher searchPort = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnpEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\"");
            foreach(var item in searchPort.Get())
            {
                string caption = item["Caption"].ToString();
                string[] a = caption.Split('(');
                port = a[1].TrimEnd(')').Trim();
            }
            return port;
        }
        public static int ConnectToModem()
        {
            string portNumber = GetPortInfo();//get port number eg: COM3
            GlobalVariables.port = portNumber;

            if (portNumber != null)
            {
                SetPortSettings(portNumber);
                if (serial.IsOpen)
                    return 1;
                else
                    return 2;
            }
            else
                return 0;
        }
        public static SerialPort SetPortSettings(string port)
        {
            serial.PortName = port;
            serial.BaudRate = 115200;
            serial.DtrEnable = true;
            serial.RtsEnable = true;
            serial.DataBits = 8;
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;
            serial.Handshake = Handshake.None;
            serial.ReceivedBytesThreshold = 1;
            serial.Open();//open serial
            return serial;
        }
        public static async Task<string> ExecuteATCommand(string cmd, int delay = 100, char terminator = '\r')
        {
            serial.Write($"{cmd}{terminator}");//write command to serial
            await Task.Delay(delay);//wait for .5 secs
            return serial.ReadExisting();
        }
        public static async Task<string> SignalStrength()
        {
            int f = 0;
            string maxDb = await ExecuteATCommand("AT+CSQ=?");//get max signal
            string maxSignal = maxDb.Split(new char[] { '(', '-', ')' }, StringSplitOptions.RemoveEmptyEntries)[2];
            string a = await ExecuteATCommand("AT+CSQ");
            a = a.Replace(@"\r\n", @"\r");
            string[] b = a.Split('\r');
            var c = from x in b where x.Contains("+CSQ:") select x;
            foreach(var s in c)
            {
                string[] d = s.Split(':');
                string[] e = d[1].Trim().Split(',');
                f = Convert.ToInt32(e[0]);
            }
            double sig = (f * 100) / Convert.ToInt32(maxSignal.Trim());//get signal in percentage
            sig = Math.Round(sig, 2, MidpointRounding.ToEven);
            return sig.ToString();
        }
        public static async Task<int> SendSMS(string plainTxtMsg, string destinationNumber)
        {
            bool result = false;
            string reply = "";

            foreach(string pduChunk in PDUStringBuilder(plainTxtMsg, destinationNumber.Trim()))
            {
                StartStopTimer();//start timer
                do
                {
                    reply = await ExecuteATCommand($"AT+CMGS={PDUStringLength(pduChunk)}", 500);
                    result = (!reply.Contains("ERROR") && reply.Contains(">")) || reply.Contains("OK");
                }
                while (!result && timerTimeout <= 10);//retry in 10 secs
                StartStopTimer(true);//stop timer

                if (result)//success
                {
                    reply = await ExecuteATCommand(pduChunk, 5000, (char)26);
                    result = !reply.Contains("ERROR");
                }

                if(!result)//failed
                {
                    serial.DiscardInBuffer();
                    serial.DiscardOutBuffer();
                    serial.BaseStream.Flush();

                    if (plainTxtMsg.Length > 160)
                        break;//one chunk failed means whole message failed
                }
                
            }
            return result ? 1 : 0;
        }
        public static string PDUStringLength(string pdu)
        {
            int x = ((pdu.Length) / 2) - 1;
            return x.ToString();
        }
        private static void StartStopTimer(bool stop = false)
        {
            tmrTimeout.Interval = 1000;//1 sec
            if (!stop)
            {
                tmrTimeout.Tick += new EventHandler(tmrTimeout_Tick);
                tmrTimeout.Start();
            }
            else
            {
                tmrTimeout.Stop();
                timerTimeout = 0;
            }
        }
        private static void tmrTimeout_Tick(Object sender, EventArgs eArgs)
        {
            timerTimeout++;
        }
        public static IEnumerable<string> PDUStringBuilder(string plainTxtMsg, string destinationNumber)
        {
            StringBuilder pduString = new StringBuilder();
            StringBuilder pduHeader = new StringBuilder();

            string destinationNum = DestinationNumberSwapper(destinationNumber);

            if(plainTxtMsg.Length > 160)//multipart sms
            {
                int currentPartNumber = 0;
                int PDUReferenceNum = 0;
                int ChunkReferenceNum = ReferenceNumber();
                int msgTotalParts = SplitbyChunks(plainTxtMsg).Count();

                foreach(string chunk in SplitbyChunks(plainTxtMsg))//loop through every parts
                {
                    currentPartNumber++;
                    pduString.Clear();
                    pduHeader.Clear();

                    pduString.Append("0041");
                    pduString.Append($"{PDUReferenceNum:X2}");
                    pduString.Append("0B81");
                    pduString.Append(destinationNum);
                    pduString.Append("0000");

                    pduHeader.Append("050003");
                    pduHeader.Append($"{ChunkReferenceNum:X2}");
                    pduHeader.Append($"{msgTotalParts:X2}");
                    pduHeader.Append($"{currentPartNumber:X2}");

                    string toGSM7Hex = PlainTextToGSM7Hex(chunk);
                    string toBinary = GSM7HexToBinary(toGSM7Hex);
                    string shiftedBinary = BinaryShifterLeftToRightDesc(toBinary);
                    string toHexadecimal = BinaryToHexadecimal(shiftedBinary);

                    pduHeader.Append(toHexadecimal);
                    string pduHeaderLength = UserDataLength(pduHeader.ToString(), true);

                    pduString.Append(pduHeaderLength);
                    pduString.Append(pduHeader.ToString());

                    PDUReferenceNum++;

                    yield return pduString.ToString();
                }
            }
            else//single sms
            {
                pduString.Append("0001000B81");
                pduString.Append(destinationNum);
                pduString.Append("0000");

                string pduLength = UserDataLength(plainTxtMsg, false);
                pduString.Append(pduLength);

                string toGSM7Hex = PlainTextToGSM7Hex(plainTxtMsg);

                pduString.Append(toGSM7Hex);

                yield return pduString.ToString();
            }
        }
        public static string UserDataLength(string msg, bool multiSMS)
        {
            int x = (multiSMS ? (((msg.Length) * 4) / 7) : msg.Length);
            return $"{x:X2}";
        }
        public static string BinaryToHexadecimal(string binary)
        {
            if (string.IsNullOrEmpty(binary))
                return binary;

            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);

            // TODO: check all 1's or 0's... throw otherwise

            int mod4Len = binary.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                binary = binary.PadLeft(((binary.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binary.Length; i += 8)
            {
                string eightBits = binary.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }
        public static string BinaryShifterLeftToRightDesc(string bin)
        {
            string previousOctetMsb = "";
            string finalOctet = "";
            string oct = "";

            foreach (string bytes8 in SplitString(bin, 8))
            {
                oct += $"_{bytes8}";
            }
            string[] octets = oct.Split('_');
            for (int i = 1; i <= octets.Length - 1; i++)
            {
                previousOctetMsb = octets[(i == 1 ? octets.Length - 1 : i - 1)].Substring(0, 1);
                finalOctet += $"{octets[i].Remove(0, 1)}{previousOctetMsb}";
            }
            return finalOctet;
        }
        public static IEnumerable<string> SplitString(string str, int length)
        {
            int index = 0;
            while (index + length < str.Length)
            {
                yield return str.Substring(index, length);
                index += length;
            }
            yield return str.Substring(index);
        }
        public static string GSM7HexToBinary(string hex)
        {
            return String.Join(String.Empty, hex.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        }
        public static string PlainTextToGSM7Hex(string plaintTxt)
        {
            string empty = string.Empty;
            string newBin = string.Empty;
            for (int index = plaintTxt.Length - 1; index >= 0; --index)
            {
                switch (Convert.ToString((byte)plaintTxt[index], 2))
                {
                    case "1011111":
                        newBin = "0010001";
                        break;
                    case "1000000":
                        newBin = "0000000";
                        break;
                    default:
                        newBin = Convert.ToString((byte)plaintTxt[index], 2);
                        break;
                }
                empty += newBin.PadLeft(8, '0').Substring(1);
            }
            string str1 = empty.PadLeft((int)Math.Ceiling((Decimal)empty.Length / new Decimal(8)) * 8, '0');
            List<byte> byteList = new List<byte>();
            while (str1 != string.Empty)
            {
                string str2 = str1.Substring(0, str1.Length > 7 ? 8 : str1.Length).PadRight(8, '0');
                str1 = str1.Length > 7 ? str1.Substring(8) : string.Empty;
                byteList.Add(Convert.ToByte(str2, 2));
            }
            byteList.Reverse();
            var messageBytes = byteList.ToArray();
            var encodedData = "";
            foreach (byte b in messageBytes)
            {
                encodedData += Convert.ToString(b, 16).PadLeft(2, '0');
            }
            return encodedData.ToUpper();
        }
        public static IEnumerable<string> SplitByLength(string str)
        {
            int index = 0;
            while (index + 153 < str.Length)
            {
                yield return str.Substring(index, 153);
                index += 153;
            }
            yield return str.Substring(index);
        }
        public static List<string> SplitbyChunks(string txt)
        {
            //max = 153
            List<string> msgChunks = new List<string>();
            foreach (var chunks in SplitByLength(txt))
            {

                StringBuilder msg = new StringBuilder(chunks);
                msg.Replace("€", " ");
                msg.Replace("{", " ");
                msg.Replace("}", " ");
                msg.Replace("[", " ");
                msg.Replace("]", " ");
                msg.Replace("^", " ");
                msg.Replace("|", " ");
                msg.Replace("~", " ");
                msg.Replace(@"\", " ");
                msg.Replace("\xC", " ");
                msg.Replace("’", "'");
                msg.Replace("`", "'");
                msg.Replace("–", "-");
                msg.Replace("\r", "\n");
                msg.Replace("ñ", "n");
                msg.Replace("Ñ", "N");
                msgChunks.Add(msg.ToString());

            }
            return msgChunks;
        }
        public static string DestinationNumberSwapper(string plainNumber, bool toOrig = false)
        {
            string x = $"{plainNumber}{(!toOrig ? "F" : "")}";
            StringBuilder phone = new StringBuilder();
            phone.Clear();
            char[] characters = x.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                if (i % 2 == 0)
                {
                    if ((i + 1) < characters.Length)
                    {
                        phone.Append(characters[i + 1]);
                    }
                    phone.Append(characters[i]);
                }
            }
            return !toOrig ? phone.ToString() : $"+{phone.ToString()}";

        }
        public static int ReferenceNumber()
        {
            Random _rdm = new Random();
            int _min = 0;
            int _max = 255;
            return _rdm.Next(_min, _max);
        }
        public static List<Inbox> ParseReceivedSMS(string rawPDUString)
        {
            List<Inbox> inbox = new List<Inbox>();
            foreach (string pduBody in ParseReceivedPDUSMS(rawPDUString))
            {
                string plainNumber = pduBody.Substring(22, 12);
                plainNumber = DestinationNumberSwapper(plainNumber, true);

                if (pduBody.Substring(16, 1) == "4")//multipart sms
                {
                    string udh = pduBody.Substring(54, 12);//user data header 
                    string referenceNo = udh.Substring(6, 2);
                    string totalParts = udh.Substring(8, 2);
                    string currentPart = udh.Substring(10, 2);

                    multiSMS.Add(new MultiSMS
                    {
                        refno = referenceNo,
                        total = totalParts,
                        currentPart = currentPart,
                        pduMsg = pduBody.Substring(66),
                        phone = plainNumber
                    });

                    if (currentPart == totalParts)//received the final part; message completed
                    {
                        StringBuilder concatenatedPlainMessage = new StringBuilder();

                        foreach (var x in multiSMS.Where(x => x.refno == referenceNo && x.phone == plainNumber).ToList())
                            concatenatedPlainMessage.Append(DecodePDU(x.pduMsg));

                        inbox.Add(new Inbox
                        {
                            number = plainNumber,
                            message = concatenatedPlainMessage.ToString(),
                            dateReceived = DateTime.Now.ToString("MMM dd yyyy hh:mm tt")
                        });
                        multiSMS.Clear();
                    }
                }
                else//single sms
                {
                    inbox.Add(new Inbox
                    {
                        number = plainNumber,
                        message = DecodePDU(pduBody.Substring(54), false).TrimEnd('@'),
                        dateReceived = DateTime.Now.ToString("MMM dd yyyy hh:mm tt")
                    });
                }
            }
            return inbox;
        }
        public static string DecodePDU(string rawPDU, bool isMultipart = true)
        {
            if (isMultipart)
            {
                string toBin = GSM7HexToBinary(rawPDU);
                string toReversedBinary = BinaryShifterRightToLeftDesc(toBin);
                string toHex = BinaryToHexadecimal(toReversedBinary);
                string toPlainTxt = GSM7ToPlainText(toHex);

                return toPlainTxt.TrimEnd('@');
            }
            else
            {
                return GSM7ToPlainText(rawPDU.TrimEnd('@'));
            }
        }
        #region PDUdecoder
        // Basic Character Set
        private const string BASIC_SET =
                "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞ\x1bÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?" +
                "¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà";

        // Basic Character Set Extension 
        private const string EXTENSION_SET =
                "````````````````````^```````````````````{}`````\\````````````[~]`" +
                "|````````````````````````````````````€``````````````````````````";


        static string[] BASIC_SET_ARRAY = BASIC_SET.Select(x => x.ToString()).ToArray();
        static string[] EXTENSION_SET_ARRAY = EXTENSION_SET.Select(x => x.ToString()).ToArray();

        enum circle { Start = 1, Complete = 8 }
        static string GetChar(string bin)
        {
            try
            {
                if (Convert.ToInt32(bin, 2).Equals(27))
                    return EXTENSION_SET_ARRAY[Convert.ToInt32(bin, 2)];
                else
                    return BASIC_SET_ARRAY[Convert.ToInt32(bin, 2)];
            }
            catch { return string.Empty; }
        }
        public static string GSM7ToPlainText(string rawPDU)
        {
            var suffix = string.Empty;
            var septet = string.Empty;
            var CurSubstr = string.Empty;
            var counter = 1;
            List<string> septets = new List<string>();
            List<string> sectets = new List<string>();

            //Prepare Octets
            var octets = Enumerable.Range(0, rawPDU.Length / 2).Select(i =>
            {
                return Convert.ToString(Convert.ToInt64(rawPDU.Substring(i * 2, 2), 16), 2).PadLeft(8, '0');

            }).ToList();


            for (var index = 0; index < octets.Count; index = index + 1)
            {
                //Generate Septets
                septet = octets[index].Substring(counter);
                CurSubstr = octets[index].Substring(0, counter);

                if (counter.Equals((int)circle.Start))
                    septets.Add(septet);
                else
                    septets.Add(septet + suffix);

                //Organize Sectets
                sectets.Add(GetChar(septets[index]));

                suffix = CurSubstr;
                counter++;

                //Reset counter when the circle is complete.
                if (counter == (int)circle.Complete)
                {
                    counter = (int)circle.Start;
                    sectets.Add(GetChar(suffix));
                }

            }
            return string.Join("", sectets);
        }
        #endregion
        public static string BinaryShifterRightToLeftDesc(string bin)//23
        {
            string nextOctetMsb = "";
            string finalOctet = "";
            int c = 0;
            string z = "";
            string oct = "";
            foreach (string bytes8 in SplitString(bin, 8))
            {
                oct += $"_{bytes8}";
            }
            string[] octets = oct.Split('_');
            for (int i = 1; i <= octets.Length - 1; i++)
            {
                nextOctetMsb = octets[(i == (octets.Length - 1) ? 1 : i + 1)].Substring(7, 1);
                finalOctet += $"{nextOctetMsb}{octets[i].Remove(octets[i].Length - 1)}";
            }
            return finalOctet;
        }
        public static List<string> ParseReceivedPDUSMS(string rawPDU)
        {
            rawPDU = rawPDU.Replace("\r\n", "_");

            string[] rawsplit = rawPDU.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            return rawsplit.Where(x => x.Substring(0, 2) == "07").ToList();
        }
        public static List<Inbox> GetSaveInboxJsonData(List<Inbox> receivedMsg = null)
        {
            List<Inbox> retrievedInbox = new List<Inbox>();
            string jsonFilePath = $@"{Application.StartupPath}\inbox.json";
            string inboxHistory = "";
            if (receivedMsg == null)
            {
                if (File.Exists(jsonFilePath))//exists,  retrive inbox
                {
                    inboxHistory = File.ReadAllText(jsonFilePath);//get inbox data from json to string
                    retrievedInbox = JsonConvert.DeserializeObject<List<Inbox>>(inboxHistory);//json to list<inbox>
                }
            }
            else
            {
                if (File.Exists(jsonFilePath))
                {
                    inboxHistory = File.ReadAllText(jsonFilePath);
                    retrievedInbox = JsonConvert.DeserializeObject<List<Inbox>>(inboxHistory);

                    int lastId = retrievedInbox.Max(x => x.id);

                    retrievedInbox.Add(new Inbox
                    {
                        id = lastId + 1,
                        isRead = false,
                        number = receivedMsg.Select(x => x.number).First().ToString(),
                        message = receivedMsg.Select(x => x.message).First().ToString(),
                        dateReceived = receivedMsg.Select(x => x.dateReceived).First().ToString()
                    });

                    string json = JsonConvert.SerializeObject(retrievedInbox, Formatting.Indented);//list<inbox> to json
                    File.WriteAllText(jsonFilePath, json);//save to file.json
                }
                else
                {
                    retrievedInbox = receivedMsg;
                    retrievedInbox[0].id = 1;
                    retrievedInbox[0].isRead = false;
                    string json = JsonConvert.SerializeObject(retrievedInbox, Formatting.Indented);
                    File.WriteAllText(jsonFilePath, json);
                }
            }
            return retrievedInbox;
        }
        public static void ReadMessage(int id)
        {
            string inboxFilePath = $@"{Application.StartupPath}\inbox.json";
            string inboxTxt = File.ReadAllText(inboxFilePath);

            var retrievedInbox = JsonConvert.DeserializeObject<List<Inbox>>(inboxTxt);

            for(int i = 0; i < retrievedInbox.Count(); i++)
            {
                if(retrievedInbox[i].id == id)
                {
                    retrievedInbox[i].isRead = true;
                    break;
                }
            }

            string tojson = JsonConvert.SerializeObject(retrievedInbox, Formatting.Indented);
            File.WriteAllText(inboxFilePath, tojson);
        }
    }
}
