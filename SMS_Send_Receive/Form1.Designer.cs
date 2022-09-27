namespace SMS_Send_Receive
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblModem = new System.Windows.Forms.Label();
            this.lblSignalQuality = new System.Windows.Forms.Label();
            this.txtRecipient = new System.Windows.Forms.TextBox();
            this.rtxtbxMessage = new System.Windows.Forms.RichTextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblCountChars = new System.Windows.Forms.Label();
            this.dgvInbox = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.lblUnreadMsgsCount = new System.Windows.Forms.Label();
            this.bgwrkrReceiveSMS = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInbox)).BeginInit();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(39, 27);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(133, 44);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblModem
            // 
            this.lblModem.AutoSize = true;
            this.lblModem.Location = new System.Drawing.Point(178, 41);
            this.lblModem.Name = "lblModem";
            this.lblModem.Size = new System.Drawing.Size(45, 16);
            this.lblModem.TabIndex = 1;
            this.lblModem.Text = "label1";
            // 
            // lblSignalQuality
            // 
            this.lblSignalQuality.AutoSize = true;
            this.lblSignalQuality.Location = new System.Drawing.Point(36, 83);
            this.lblSignalQuality.Name = "lblSignalQuality";
            this.lblSignalQuality.Size = new System.Drawing.Size(45, 16);
            this.lblSignalQuality.TabIndex = 2;
            this.lblSignalQuality.Text = "label1";
            // 
            // txtRecipient
            // 
            this.txtRecipient.Enabled = false;
            this.txtRecipient.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRecipient.Location = new System.Drawing.Point(43, 120);
            this.txtRecipient.Name = "txtRecipient";
            this.txtRecipient.Size = new System.Drawing.Size(349, 29);
            this.txtRecipient.TabIndex = 3;
            this.txtRecipient.TextChanged += new System.EventHandler(this.txtRecipient_TextChanged);
            // 
            // rtxtbxMessage
            // 
            this.rtxtbxMessage.Enabled = false;
            this.rtxtbxMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtxtbxMessage.Location = new System.Drawing.Point(43, 164);
            this.rtxtbxMessage.Name = "rtxtbxMessage";
            this.rtxtbxMessage.Size = new System.Drawing.Size(349, 350);
            this.rtxtbxMessage.TabIndex = 4;
            this.rtxtbxMessage.Text = "";
            this.rtxtbxMessage.TextChanged += new System.EventHandler(this.rtxtbxMessage_TextChanged);
            // 
            // btnSend
            // 
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(43, 537);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(349, 53);
            this.btnSend.TabIndex = 5;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblCountChars
            // 
            this.lblCountChars.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCountChars.Location = new System.Drawing.Point(206, 517);
            this.lblCountChars.Name = "lblCountChars";
            this.lblCountChars.Size = new System.Drawing.Size(186, 17);
            this.lblCountChars.TabIndex = 6;
            this.lblCountChars.Text = "0";
            this.lblCountChars.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dgvInbox
            // 
            this.dgvInbox.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInbox.Location = new System.Drawing.Point(432, 89);
            this.dgvInbox.Name = "dgvInbox";
            this.dgvInbox.RowHeadersVisible = false;
            this.dgvInbox.RowHeadersWidth = 51;
            this.dgvInbox.RowTemplate.Height = 24;
            this.dgvInbox.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvInbox.Size = new System.Drawing.Size(547, 500);
            this.dgvInbox.TabIndex = 7;
            this.dgvInbox.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvInbox_CellClick);
            this.dgvInbox.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvInbox_CellDoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(428, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 24);
            this.label1.TabIndex = 8;
            this.label1.Text = "Inbox";
            // 
            // lblUnreadMsgsCount
            // 
            this.lblUnreadMsgsCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnreadMsgsCount.Location = new System.Drawing.Point(696, 61);
            this.lblUnreadMsgsCount.Name = "lblUnreadMsgsCount";
            this.lblUnreadMsgsCount.Size = new System.Drawing.Size(283, 24);
            this.lblUnreadMsgsCount.TabIndex = 9;
            this.lblUnreadMsgsCount.Text = "0 unread messages";
            this.lblUnreadMsgsCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // bgwrkrReceiveSMS
            // 
            this.bgwrkrReceiveSMS.WorkerReportsProgress = true;
            this.bgwrkrReceiveSMS.WorkerSupportsCancellation = true;
            this.bgwrkrReceiveSMS.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgwrkrReceiveSMS_DoWork);
            this.bgwrkrReceiveSMS.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgwrkrReceiveSMS_RunWorkerCompleted);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(997, 667);
            this.Controls.Add(this.lblUnreadMsgsCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvInbox);
            this.Controls.Add(this.lblCountChars);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.rtxtbxMessage);
            this.Controls.Add(this.txtRecipient);
            this.Controls.Add(this.lblSignalQuality);
            this.Controls.Add(this.lblModem);
            this.Controls.Add(this.btnConnect);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SMS Send Receive";
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvInbox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblModem;
        private System.Windows.Forms.Label lblSignalQuality;
        private System.Windows.Forms.TextBox txtRecipient;
        private System.Windows.Forms.RichTextBox rtxtbxMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblCountChars;
        private System.Windows.Forms.DataGridView dgvInbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblUnreadMsgsCount;
        private System.ComponentModel.BackgroundWorker bgwrkrReceiveSMS;
    }
}

