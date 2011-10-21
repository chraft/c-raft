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
            this.SuspendLayout();
            // 
            // clientsNumText
            // 
            this.clientsNumText.Location = new System.Drawing.Point(104, 31);
            this.clientsNumText.Name = "clientsNumText";
            this.clientsNumText.Size = new System.Drawing.Size(100, 20);
            this.clientsNumText.TabIndex = 0;
            // 
            // launchButton
            // 
            this.launchButton.Location = new System.Drawing.Point(65, 57);
            this.launchButton.Name = "launchButton";
            this.launchButton.Size = new System.Drawing.Size(75, 23);
            this.launchButton.TabIndex = 1;
            this.launchButton.Text = "Launch";
            this.launchButton.UseVisualStyleBackColor = true;
            this.launchButton.Click += new System.EventHandler(this.launchButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Clients N*";
            // 
            // disposeButton
            // 
            this.disposeButton.Location = new System.Drawing.Point(146, 57);
            this.disposeButton.Name = "disposeButton";
            this.disposeButton.Size = new System.Drawing.Size(75, 23);
            this.disposeButton.TabIndex = 3;
            this.disposeButton.Text = "Dispose All";
            this.disposeButton.UseVisualStyleBackColor = true;
            this.disposeButton.Click += new System.EventHandler(this.disposeButton_Click);
            // 
            // ClientsLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 428);
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
    }
}

