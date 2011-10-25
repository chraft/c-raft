namespace ChraftTestClient
{
    partial class ClientsLauncher
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
            this.clientsNumText = new System.Windows.Forms.TextBox();
            this.launchButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.disposeButton = new System.Windows.Forms.Button();
            this.portText = new System.Windows.Forms.TextBox();
            this.ipText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SendMessage = new System.Windows.Forms.Button();
            this.ChatBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // clientsNumText
            // 
            this.clientsNumText.Location = new System.Drawing.Point(116, 92);
            this.clientsNumText.Name = "clientsNumText";
            this.clientsNumText.Size = new System.Drawing.Size(100, 20);
            this.clientsNumText.TabIndex = 2;
            // 
            // launchButton
            // 
            this.launchButton.Location = new System.Drawing.Point(60, 127);
            this.launchButton.Name = "launchButton";
            this.launchButton.Size = new System.Drawing.Size(75, 23);
            this.launchButton.TabIndex = 3;
            this.launchButton.Text = "Launch";
            this.launchButton.UseVisualStyleBackColor = true;
            this.launchButton.Click += new System.EventHandler(this.launchButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Clients N*";
            // 
            // disposeButton
            // 
            this.disposeButton.Location = new System.Drawing.Point(141, 127);
            this.disposeButton.Name = "disposeButton";
            this.disposeButton.Size = new System.Drawing.Size(75, 23);
            this.disposeButton.TabIndex = 4;
            this.disposeButton.Text = "Dispose All";
            this.disposeButton.UseVisualStyleBackColor = true;
            this.disposeButton.Click += new System.EventHandler(this.disposeButton_Click);
            // 
            // portText
            // 
            this.portText.Location = new System.Drawing.Point(116, 66);
            this.portText.Name = "portText";
            this.portText.Size = new System.Drawing.Size(100, 20);
            this.portText.TabIndex = 1;
            // 
            // ipText
            // 
            this.ipText.Location = new System.Drawing.Point(116, 40);
            this.ipText.Name = "ipText";
            this.ipText.Size = new System.Drawing.Size(100, 20);
            this.ipText.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(93, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "IP";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(84, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Port";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(222, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(199, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "(leaving this empty will use local address)";
            // 
            // SendMessage
            // 
            this.SendMessage.Location = new System.Drawing.Point(206, 189);
            this.SendMessage.Name = "SendMessage";
            this.SendMessage.Size = new System.Drawing.Size(75, 23);
            this.SendMessage.TabIndex = 9;
            this.SendMessage.Text = "Send";
            this.SendMessage.UseVisualStyleBackColor = true;
            this.SendMessage.Click += new System.EventHandler(this.SendMessage_Click);
            // 
            // ChatBox
            // 
            this.ChatBox.Location = new System.Drawing.Point(30, 191);
            this.ChatBox.Name = "ChatBox";
            this.ChatBox.Size = new System.Drawing.Size(148, 20);
            this.ChatBox.TabIndex = 10;
            this.ChatBox.Text = "Hi! :)";
            // 
            // ClientsLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 428);
            this.Controls.Add(this.ChatBox);
            this.Controls.Add(this.SendMessage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ipText);
            this.Controls.Add(this.portText);
            this.Controls.Add(this.disposeButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.launchButton);
            this.Controls.Add(this.clientsNumText);
            this.Name = "ClientsLauncher";
            this.Text = "ClientsLauncher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientsLauncher_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox clientsNumText;
        private System.Windows.Forms.Button launchButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button disposeButton;
        private System.Windows.Forms.TextBox portText;
        private System.Windows.Forms.TextBox ipText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button SendMessage;
        private System.Windows.Forms.TextBox ChatBox;
    }
}

