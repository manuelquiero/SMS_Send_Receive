using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMS_Send_Receive
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        string receivedRawSMS = "";
        int msgCount = 0;
        bool hasRecipient = false;
        List<MultiSMS> multiSMS = new List<MultiSMS>();
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            
            if (Encoder.serial.IsOpen)
            {
                Encoder.serial.Close();
                ModemStatus(2);
            }
            else
            {
                ModemStatus(Encoder.ConnectToModem());
                string res = await Encoder.ExecuteATCommand("AT+CMGF=0");//set modem to PDU mode
                await Encoder.ExecuteATCommand("AT+CNMI=2,2,0,2,0");//enable receiving SMS
                await Encoder.ExecuteATCommand("AT+CMEE=1");
                lblSignalQuality.Text = $"Signal strength: {await Encoder.SignalStrength()}%";
                bgwrkrReceiveSMS.RunWorkerAsync();//run worker on connect
            }
        }
        private void ModemStatus(int modemStat)
        {
            btnConnect.Enabled = false;
            if(modemStat == 0)//no modem
            {
                lblModem.Text = "No modem detected";
                lblModem.ForeColor = Color.Red;
                btnConnect.Text = "Connect";
            }
            else
            {
                if(modemStat == 1)
                {
                    txtRecipient.Enabled = true;
                    rtxtbxMessage.Enabled = true;
                    btnConnect.Enabled = true;
                    lblModem.Text = $"Connected {GlobalVariables.port}";
                    lblModem.ForeColor = Color.Green;
                    btnConnect.Text = "Disconnect";
                }
                else
                {
                    txtRecipient.Enabled = false;
                    rtxtbxMessage.Enabled = false;
                    lblModem.Text = "Disconnected";
                    lblModem.ForeColor = Color.Red;
                    btnConnect.Text = "Connect";
                }
            }
        }

        private void rtxtbxMessage_TextChanged(object sender, EventArgs e)
        {
            hasRecipient = rtxtbxMessage.Text != "" ? true : false;
            msgCount = rtxtbxMessage.Text.Length;
            btnSend.Enabled = (msgCount > 0 && hasRecipient ? true:false);
            lblCountChars.Text = msgCount.ToString();
        }

        private void txtRecipient_TextChanged(object sender, EventArgs e)
        {
            string msg = txtRecipient.Text;
            hasRecipient = msg != "" ? true : false;
            btnSend.Enabled = msgCount > 0 && hasRecipient ? true : false;
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            int success = 0;
            string[] destinationNumbers = txtRecipient.Text.Split(';');
            int totalMsgsToSend = destinationNumbers.Count();

            btnSend.Enabled = false;
            btnConnect.Enabled = false;
            GlobalVariables.isSending = true;

            foreach(string destinationNumber in destinationNumbers)
            {
                success += await Encoder.SendSMS(rtxtbxMessage.Text, destinationNumber);
                btnSend.Text = $"Sending {success}/{totalMsgsToSend}";
            }

            MessageBox.Show($"Sent: {success}\rFailed: {totalMsgsToSend - success}", "Send result", MessageBoxButtons.OK, MessageBoxIcon.Information);

            btnSend.Text = "Send";
            btnSend.Enabled = true;
            btnConnect.Enabled = true;
            GlobalVariables.isSending = false;
            
        }

        private void bgwrkrReceiveSMS_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            do
            {
                if (!GlobalVariables.isSending)
                {
                    Thread.Sleep(1000);
                    receivedRawSMS = Encoder.serial.ReadExisting();
                    if (receivedRawSMS.Contains("+CMT:"))
                    {
                        bgwrkrReceiveSMS.CancelAsync();
                        e.Cancel = true;
                    }
                }
            }
            while (!worker.CancellationPending);
        }

        private void bgwrkrReceiveSMS_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var inbox = Encoder.ParseReceivedSMS(receivedRawSMS);
            if (inbox.Count() > 0)
                ShowInboxData(inbox);
            bgwrkrReceiveSMS.RunWorkerAsync();//run worker again
        }
        private void ShowInboxData(List<Inbox> inbox = null)
        {
            var retrievedInbox = Encoder.GetSaveInboxJsonData(inbox);
            dgvInbox.DataSource = retrievedInbox;//populate datagrid with inbox data

            int unread = retrievedInbox.Where(x => x.isRead == false).Count();
            lblUnreadMsgsCount.Text = $"{unread} unread message{(unread > 1 ? "s":"")}";
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            ShowInboxData();
        }

        private void dgvInbox_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dgvInbox_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowSelected = dgvInbox.CurrentCell.RowIndex;
            int inboxId = Convert.ToInt32(dgvInbox.Rows[rowSelected].Cells[0].Value.ToString());
            var retrievedInbox = Encoder.GetSaveInboxJsonData();

            string mobileNumber = dgvInbox.Rows[rowSelected].Cells[1].Value.ToString();
            string message = dgvInbox.Rows[rowSelected].Cells[2].Value.ToString();
            string date = dgvInbox.Rows[rowSelected].Cells[3].Value.ToString();

            MessageBox.Show($"From: {mobileNumber}\r\n{message}\r\n{date}", "Read", MessageBoxButtons.OK);
            Encoder.ReadMessage(inboxId);

            ShowInboxData();
        }
    }
}
