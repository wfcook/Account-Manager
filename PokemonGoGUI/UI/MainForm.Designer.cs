namespace PokemonGoGUI
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.fastObjectListViewMain = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumnUsername = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnLevel = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnExp = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnExpPerHour = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTillRankUp = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnRunning = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnBotState = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnRunningTime = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTotalLogs = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnLastLogMessage = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.contextMenuStripAccounts = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerListViewUpdate = new System.Windows.Forms.Timer(this.components);
            this.olvColumnProxy = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewMain)).BeginInit();
            this.contextMenuStripAccounts.SuspendLayout();
            this.SuspendLayout();
            // 
            // fastObjectListViewMain
            // 
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnUsername);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnLevel);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnExp);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnExpPerHour);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnTillRankUp);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnRunning);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnBotState);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnRunningTime);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnTotalLogs);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnProxy);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnLastLogMessage);
            this.fastObjectListViewMain.CellEditUseWholeCell = false;
            this.fastObjectListViewMain.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnUsername,
            this.olvColumnLevel,
            this.olvColumnExp,
            this.olvColumnExpPerHour,
            this.olvColumnTillRankUp,
            this.olvColumnBotState,
            this.olvColumnRunningTime,
            this.olvColumnProxy,
            this.olvColumnLastLogMessage});
            this.fastObjectListViewMain.ContextMenuStrip = this.contextMenuStripAccounts;
            this.fastObjectListViewMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastObjectListViewMain.FullRowSelect = true;
            this.fastObjectListViewMain.Location = new System.Drawing.Point(0, 0);
            this.fastObjectListViewMain.Name = "fastObjectListViewMain";
            this.fastObjectListViewMain.ShowGroups = false;
            this.fastObjectListViewMain.Size = new System.Drawing.Size(1041, 482);
            this.fastObjectListViewMain.TabIndex = 0;
            this.fastObjectListViewMain.UseCompatibleStateImageBehavior = false;
            this.fastObjectListViewMain.UseFiltering = true;
            this.fastObjectListViewMain.View = System.Windows.Forms.View.Details;
            this.fastObjectListViewMain.VirtualMode = true;
            this.fastObjectListViewMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fastObjectListViewMain_KeyDown);
            // 
            // olvColumnUsername
            // 
            this.olvColumnUsername.AspectName = "AccountName";
            this.olvColumnUsername.Text = "Name";
            this.olvColumnUsername.Width = 108;
            // 
            // olvColumnLevel
            // 
            this.olvColumnLevel.AspectName = "Level";
            this.olvColumnLevel.Text = "Level";
            // 
            // olvColumnExp
            // 
            this.olvColumnExp.AspectName = "ExpRatio";
            this.olvColumnExp.Text = "Exp";
            // 
            // olvColumnExpPerHour
            // 
            this.olvColumnExpPerHour.AspectName = "ExpPerHour";
            this.olvColumnExpPerHour.Text = "Exp/Hr";
            this.olvColumnExpPerHour.Width = 98;
            // 
            // olvColumnTillRankUp
            // 
            this.olvColumnTillRankUp.AspectName = "TillLevelUp";
            this.olvColumnTillRankUp.Text = "Level Up";
            // 
            // olvColumnRunning
            // 
            this.olvColumnRunning.AspectName = "IsRunning";
            this.olvColumnRunning.DisplayIndex = 5;
            this.olvColumnRunning.IsVisible = false;
            this.olvColumnRunning.Text = "Running";
            this.olvColumnRunning.Width = 84;
            // 
            // olvColumnBotState
            // 
            this.olvColumnBotState.AspectName = "State";
            this.olvColumnBotState.Text = "State";
            // 
            // olvColumnRunningTime
            // 
            this.olvColumnRunningTime.AspectName = "RunningTime";
            this.olvColumnRunningTime.Text = "Time";
            // 
            // olvColumnTotalLogs
            // 
            this.olvColumnTotalLogs.AspectName = "TotalLogs";
            this.olvColumnTotalLogs.DisplayIndex = 4;
            this.olvColumnTotalLogs.IsVisible = false;
            this.olvColumnTotalLogs.Text = "Total Logs";
            this.olvColumnTotalLogs.Width = 86;
            // 
            // olvColumnLastLogMessage
            // 
            this.olvColumnLastLogMessage.AspectName = "LastLogMessage";
            this.olvColumnLastLogMessage.Text = "Last Log";
            this.olvColumnLastLogMessage.Width = 248;
            // 
            // contextMenuStripAccounts
            // 
            this.contextMenuStripAccounts.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.contextMenuStripAccounts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewDetailsToolStripMenuItem,
            this.importToolStripMenuItem,
            this.addNewToolStripMenuItem,
            this.editToolStripMenuItem,
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.exportAccountsToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStripAccounts.Name = "contextMenuStrip1";
            this.contextMenuStripAccounts.Size = new System.Drawing.Size(215, 228);
            // 
            // viewDetailsToolStripMenuItem
            // 
            this.viewDetailsToolStripMenuItem.Name = "viewDetailsToolStripMenuItem";
            this.viewDetailsToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.viewDetailsToolStripMenuItem.Text = "View Details";
            this.viewDetailsToolStripMenuItem.Click += new System.EventHandler(this.viewDetailsToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wConfigToolStripMenuItem,
            this.defaultToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.importToolStripMenuItem.Text = "Import";
            // 
            // wConfigToolStripMenuItem
            // 
            this.wConfigToolStripMenuItem.Name = "wConfigToolStripMenuItem";
            this.wConfigToolStripMenuItem.Size = new System.Drawing.Size(164, 28);
            this.wConfigToolStripMenuItem.Text = "w/ Config";
            this.wConfigToolStripMenuItem.Click += new System.EventHandler(this.wConfigToolStripMenuItem_Click);
            // 
            // defaultToolStripMenuItem
            // 
            this.defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            this.defaultToolStripMenuItem.Size = new System.Drawing.Size(164, 28);
            this.defaultToolStripMenuItem.Text = "Default";
            this.defaultToolStripMenuItem.Click += new System.EventHandler(this.defaultToolStripMenuItem_Click);
            // 
            // addNewToolStripMenuItem
            // 
            this.addNewToolStripMenuItem.Name = "addNewToolStripMenuItem";
            this.addNewToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.addNewToolStripMenuItem.Text = "Add New";
            this.addNewToolStripMenuItem.Click += new System.EventHandler(this.addNewToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // exportAccountsToolStripMenuItem
            // 
            this.exportAccountsToolStripMenuItem.Name = "exportAccountsToolStripMenuItem";
            this.exportAccountsToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.exportAccountsToolStripMenuItem.Text = "Export Accounts";
            this.exportAccountsToolStripMenuItem.Click += new System.EventHandler(this.exportAccountsToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // timerListViewUpdate
            // 
            this.timerListViewUpdate.Enabled = true;
            this.timerListViewUpdate.Interval = 1000;
            this.timerListViewUpdate.Tick += new System.EventHandler(this.timerListViewUpdate_Tick);
            // 
            // olvColumnProxy
            // 
            this.olvColumnProxy.AspectName = "Proxy";
            this.olvColumnProxy.Text = "Proxy";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1041, 482);
            this.Controls.Add(this.fastObjectListViewMain);
            this.Name = "MainForm";
            this.Text = "GoManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewMain)).EndInit();
            this.contextMenuStripAccounts.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.FastObjectListView fastObjectListViewMain;
        private BrightIdeasSoftware.OLVColumn olvColumnUsername;
        private BrightIdeasSoftware.OLVColumn olvColumnLevel;
        private BrightIdeasSoftware.OLVColumn olvColumnExp;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAccounts;
        private System.Windows.Forms.ToolStripMenuItem addNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private BrightIdeasSoftware.OLVColumn olvColumnRunning;
        private System.Windows.Forms.ToolStripMenuItem viewDetailsToolStripMenuItem;
        private BrightIdeasSoftware.OLVColumn olvColumnTotalLogs;
        private BrightIdeasSoftware.OLVColumn olvColumnLastLogMessage;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem;
        private BrightIdeasSoftware.OLVColumn olvColumnExpPerHour;
        private BrightIdeasSoftware.OLVColumn olvColumnRunningTime;
        private BrightIdeasSoftware.OLVColumn olvColumnTillRankUp;
        private System.Windows.Forms.Timer timerListViewUpdate;
        private System.Windows.Forms.ToolStripMenuItem exportAccountsToolStripMenuItem;
        private BrightIdeasSoftware.OLVColumn olvColumnBotState;
        private BrightIdeasSoftware.OLVColumn olvColumnProxy;
    }
}

