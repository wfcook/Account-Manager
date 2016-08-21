namespace PokemonGoGUI.UI
{
    partial class StartupForm
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
            this.checkBoxShowOnStartup = new System.Windows.Forms.CheckBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabelDiscordChat = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // checkBoxShowOnStartup
            // 
            this.checkBoxShowOnStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxShowOnStartup.AutoSize = true;
            this.checkBoxShowOnStartup.Checked = true;
            this.checkBoxShowOnStartup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowOnStartup.Location = new System.Drawing.Point(182, 159);
            this.checkBoxShowOnStartup.Name = "checkBoxShowOnStartup";
            this.checkBoxShowOnStartup.Size = new System.Drawing.Size(126, 20);
            this.checkBoxShowOnStartup.TabIndex = 0;
            this.checkBoxShowOnStartup.Text = "Show on Startup";
            this.checkBoxShowOnStartup.UseVisualStyleBackColor = true;
            this.checkBoxShowOnStartup.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(314, 157);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(346, 80);
            this.label1.TabIndex = 2;
            this.label1.Text = "Welcome to GoManager! \r\nRight Clicking on the next window will help you get start" +
    "ed.\r\n\r\nIf you require any additional help, \r\nyou can always ask questions in our" +
    " Discord chat.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Discord Chat:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(46, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Source:";
            // 
            // linkLabelDiscordChat
            // 
            this.linkLabelDiscordChat.AutoSize = true;
            this.linkLabelDiscordChat.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelDiscordChat.LinkColor = System.Drawing.Color.Gray;
            this.linkLabelDiscordChat.Location = new System.Drawing.Point(106, 101);
            this.linkLabelDiscordChat.Name = "linkLabelDiscordChat";
            this.linkLabelDiscordChat.Size = new System.Drawing.Size(164, 16);
            this.linkLabelDiscordChat.TabIndex = 5;
            this.linkLabelDiscordChat.TabStop = true;
            this.linkLabelDiscordChat.Text = "https://discord.gg/4qn5gCf";
            this.linkLabelDiscordChat.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDiscordChat_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabel1.LinkColor = System.Drawing.Color.Gray;
            this.linkLabel1.Location = new System.Drawing.Point(106, 121);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(49, 16);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "GitHub";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDiscordChat_LinkClicked);
            // 
            // StartupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 192);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.linkLabelDiscordChat);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.checkBoxShowOnStartup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "StartupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Welcome!";
            this.Load += new System.EventHandler(this.StartupForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxShowOnStartup;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabelDiscordChat;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}