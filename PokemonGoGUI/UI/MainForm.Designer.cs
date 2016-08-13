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
            this.olvColumnAccountState = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnMaxLevel = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnLevel = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnPokestopsFarmed = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnPokemonCaught = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnExp = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnExpPerHour = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTillRankUp = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnRunning = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnBotState = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnRunningTime = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTotalLogs = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnProxy = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnLastLogMessage = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.contextMenuStripAccounts = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.updateDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.clearCountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.proxiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importProxiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.clearProxiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportProxiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportStatsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.enableColorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.devToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.garbageCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerListViewUpdate = new System.Windows.Forms.Timer(this.components);
            this.countsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewMain)).BeginInit();
            this.contextMenuStripAccounts.SuspendLayout();
            this.SuspendLayout();
            // 
            // fastObjectListViewMain
            // 
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnUsername);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnAccountState);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnMaxLevel);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnLevel);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnPokestopsFarmed);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnPokemonCaught);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnExp);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnExpPerHour);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnTillRankUp);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnRunning);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnBotState);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnRunningTime);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnTotalLogs);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnProxy);
            this.fastObjectListViewMain.AllColumns.Add(this.olvColumnLastLogMessage);
            this.fastObjectListViewMain.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnUsername,
            this.olvColumnAccountState,
            this.olvColumnLevel,
            this.olvColumnPokestopsFarmed,
            this.olvColumnPokemonCaught,
            this.olvColumnExp,
            this.olvColumnExpPerHour,
            this.olvColumnTillRankUp,
            this.olvColumnBotState,
            this.olvColumnRunningTime,
            this.olvColumnProxy,
            this.olvColumnLastLogMessage});
            this.fastObjectListViewMain.ContextMenuStrip = this.contextMenuStripAccounts;
            this.fastObjectListViewMain.Cursor = System.Windows.Forms.Cursors.Default;
            this.fastObjectListViewMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastObjectListViewMain.FullRowSelect = true;
            this.fastObjectListViewMain.Location = new System.Drawing.Point(0, 0);
            this.fastObjectListViewMain.Name = "fastObjectListViewMain";
            this.fastObjectListViewMain.ShowGroups = false;
            this.fastObjectListViewMain.Size = new System.Drawing.Size(1041, 482);
            this.fastObjectListViewMain.TabIndex = 0;
            this.fastObjectListViewMain.UseCellFormatEvents = true;
            this.fastObjectListViewMain.UseCompatibleStateImageBehavior = false;
            this.fastObjectListViewMain.UseFiltering = true;
            this.fastObjectListViewMain.View = System.Windows.Forms.View.Details;
            this.fastObjectListViewMain.VirtualMode = true;
            this.fastObjectListViewMain.FormatCell += new System.EventHandler<BrightIdeasSoftware.FormatCellEventArgs>(this.fastObjectListViewMain_FormatCell);
            this.fastObjectListViewMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fastObjectListViewMain_KeyDown);
            // 
            // olvColumnUsername
            // 
            this.olvColumnUsername.AspectName = "AccountName";
            this.olvColumnUsername.Text = "Name";
            this.olvColumnUsername.Width = 108;
            // 
            // olvColumnAccountState
            // 
            this.olvColumnAccountState.AspectName = "AccountState";
            this.olvColumnAccountState.Text = "Account Status";
            // 
            // olvColumnMaxLevel
            // 
            this.olvColumnMaxLevel.AspectName = "MaxLevel";
            this.olvColumnMaxLevel.DisplayIndex = 1;
            this.olvColumnMaxLevel.IsVisible = false;
            this.olvColumnMaxLevel.Text = "Max Level";
            // 
            // olvColumnLevel
            // 
            this.olvColumnLevel.AspectName = "Level";
            this.olvColumnLevel.Text = "Level";
            // 
            // olvColumnPokestopsFarmed
            // 
            this.olvColumnPokestopsFarmed.AspectName = "PokestopsFarmed";
            this.olvColumnPokestopsFarmed.Text = "Pokestops";
            // 
            // olvColumnPokemonCaught
            // 
            this.olvColumnPokemonCaught.AspectName = "PokemonCaught";
            this.olvColumnPokemonCaught.Text = "Pokemon";
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
            this.olvColumnRunning.DisplayIndex = 8;
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
            this.olvColumnRunningTime.DisplayIndex = 10;
            this.olvColumnRunningTime.Text = "Time";
            // 
            // olvColumnTotalLogs
            // 
            this.olvColumnTotalLogs.AspectName = "TotalLogs";
            this.olvColumnTotalLogs.DisplayIndex = 11;
            this.olvColumnTotalLogs.IsVisible = false;
            this.olvColumnTotalLogs.Text = "Total Logs";
            this.olvColumnTotalLogs.Width = 86;
            // 
            // olvColumnProxy
            // 
            this.olvColumnProxy.AspectName = "Proxy";
            this.olvColumnProxy.DisplayIndex = 9;
            this.olvColumnProxy.Text = "Proxy";
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
            this.updateDetailsToolStripMenuItem,
            this.viewDetailsToolStripMenuItem,
            this.toolStripSeparator3,
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.toolStripSeparator4,
            this.clearCountsToolStripMenuItem,
            this.addNewToolStripMenuItem,
            this.editToolStripMenuItem,
            this.importToolStripMenuItem,
            this.toolStripSeparator5,
            this.proxiesToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripSeparator1,
            this.enableColorsToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.devToolsToolStripMenuItem});
            this.contextMenuStripAccounts.Name = "contextMenuStrip1";
            this.contextMenuStripAccounts.Size = new System.Drawing.Size(218, 423);
            // 
            // updateDetailsToolStripMenuItem
            // 
            this.updateDetailsToolStripMenuItem.Name = "updateDetailsToolStripMenuItem";
            this.updateDetailsToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.updateDetailsToolStripMenuItem.Text = "Update Stats";
            this.updateDetailsToolStripMenuItem.Click += new System.EventHandler(this.updateDetailsToolStripMenuItem_Click);
            // 
            // viewDetailsToolStripMenuItem
            // 
            this.viewDetailsToolStripMenuItem.Name = "viewDetailsToolStripMenuItem";
            this.viewDetailsToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.viewDetailsToolStripMenuItem.Text = "View Details";
            this.viewDetailsToolStripMenuItem.Click += new System.EventHandler(this.viewDetailsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(214, 6);
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(214, 6);
            // 
            // clearCountsToolStripMenuItem
            // 
            this.clearCountsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.countsToolStripMenuItem,
            this.logsToolStripMenuItem});
            this.clearCountsToolStripMenuItem.Name = "clearCountsToolStripMenuItem";
            this.clearCountsToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.clearCountsToolStripMenuItem.Text = "Clear ";
            // 
            // addNewToolStripMenuItem
            // 
            this.addNewToolStripMenuItem.Name = "addNewToolStripMenuItem";
            this.addNewToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.addNewToolStripMenuItem.Text = "Add New";
            this.addNewToolStripMenuItem.Click += new System.EventHandler(this.addNewToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wConfigToolStripMenuItem,
            this.defaultToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.importToolStripMenuItem.Text = "Import Accounts";
            // 
            // wConfigToolStripMenuItem
            // 
            this.wConfigToolStripMenuItem.Name = "wConfigToolStripMenuItem";
            this.wConfigToolStripMenuItem.Size = new System.Drawing.Size(164, 28);
            this.wConfigToolStripMenuItem.Tag = "true";
            this.wConfigToolStripMenuItem.Text = "w/ Config";
            this.wConfigToolStripMenuItem.Click += new System.EventHandler(this.wConfigToolStripMenuItem_Click);
            // 
            // defaultToolStripMenuItem
            // 
            this.defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            this.defaultToolStripMenuItem.Size = new System.Drawing.Size(164, 28);
            this.defaultToolStripMenuItem.Tag = "false";
            this.defaultToolStripMenuItem.Text = "Default";
            this.defaultToolStripMenuItem.Click += new System.EventHandler(this.wConfigToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(214, 6);
            // 
            // proxiesToolStripMenuItem
            // 
            this.proxiesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importProxiesToolStripMenuItem,
            this.toolStripSeparator2,
            this.clearProxiesToolStripMenuItem});
            this.proxiesToolStripMenuItem.Name = "proxiesToolStripMenuItem";
            this.proxiesToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.proxiesToolStripMenuItem.Text = "Proxies";
            // 
            // importProxiesToolStripMenuItem
            // 
            this.importProxiesToolStripMenuItem.Name = "importProxiesToolStripMenuItem";
            this.importProxiesToolStripMenuItem.Size = new System.Drawing.Size(201, 28);
            this.importProxiesToolStripMenuItem.Text = "Import Proxies";
            this.importProxiesToolStripMenuItem.Click += new System.EventHandler(this.importProxiesToolStripMenuItem_Click_1);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(198, 6);
            // 
            // clearProxiesToolStripMenuItem
            // 
            this.clearProxiesToolStripMenuItem.Name = "clearProxiesToolStripMenuItem";
            this.clearProxiesToolStripMenuItem.Size = new System.Drawing.Size(201, 28);
            this.clearProxiesToolStripMenuItem.Text = "Clear Proxies";
            this.clearProxiesToolStripMenuItem.Click += new System.EventHandler(this.clearProxiesToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportProxiesToolStripMenuItem,
            this.exportAccountsToolStripMenuItem,
            this.exportStatsToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // exportProxiesToolStripMenuItem
            // 
            this.exportProxiesToolStripMenuItem.Name = "exportProxiesToolStripMenuItem";
            this.exportProxiesToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.exportProxiesToolStripMenuItem.Text = "Export Proxies";
            this.exportProxiesToolStripMenuItem.Click += new System.EventHandler(this.exportProxiesToolStripMenuItem_Click);
            // 
            // exportAccountsToolStripMenuItem
            // 
            this.exportAccountsToolStripMenuItem.Name = "exportAccountsToolStripMenuItem";
            this.exportAccountsToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.exportAccountsToolStripMenuItem.Text = "Export Accounts";
            this.exportAccountsToolStripMenuItem.Click += new System.EventHandler(this.exportAccountsToolStripMenuItem_Click_1);
            // 
            // exportStatsToolStripMenuItem
            // 
            this.exportStatsToolStripMenuItem.Name = "exportStatsToolStripMenuItem";
            this.exportStatsToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.exportStatsToolStripMenuItem.Text = "Export Stats";
            this.exportStatsToolStripMenuItem.Click += new System.EventHandler(this.exportStatsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(214, 6);
            // 
            // enableColorsToolStripMenuItem
            // 
            this.enableColorsToolStripMenuItem.Checked = true;
            this.enableColorsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableColorsToolStripMenuItem.Name = "enableColorsToolStripMenuItem";
            this.enableColorsToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.enableColorsToolStripMenuItem.Text = "Enable Colors";
            this.enableColorsToolStripMenuItem.Click += new System.EventHandler(this.enableColorsToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // devToolsToolStripMenuItem
            // 
            this.devToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.garbageCollectionToolStripMenuItem});
            this.devToolsToolStripMenuItem.Name = "devToolsToolStripMenuItem";
            this.devToolsToolStripMenuItem.Size = new System.Drawing.Size(217, 28);
            this.devToolsToolStripMenuItem.Text = "Dev Tools";
            this.devToolsToolStripMenuItem.Visible = false;
            // 
            // garbageCollectionToolStripMenuItem
            // 
            this.garbageCollectionToolStripMenuItem.Name = "garbageCollectionToolStripMenuItem";
            this.garbageCollectionToolStripMenuItem.Size = new System.Drawing.Size(236, 28);
            this.garbageCollectionToolStripMenuItem.Text = "Garbage Collection";
            this.garbageCollectionToolStripMenuItem.Click += new System.EventHandler(this.garbageCollectionToolStripMenuItem_Click);
            // 
            // timerListViewUpdate
            // 
            this.timerListViewUpdate.Enabled = true;
            this.timerListViewUpdate.Interval = 1000;
            this.timerListViewUpdate.Tick += new System.EventHandler(this.timerListViewUpdate_Tick);
            // 
            // countsToolStripMenuItem
            // 
            this.countsToolStripMenuItem.Name = "countsToolStripMenuItem";
            this.countsToolStripMenuItem.Size = new System.Drawing.Size(198, 28);
            this.countsToolStripMenuItem.Text = "Counts";
            this.countsToolStripMenuItem.Click += new System.EventHandler(this.clearCountsToolStripMenuItem_Click);
            // 
            // logsToolStripMenuItem
            // 
            this.logsToolStripMenuItem.Name = "logsToolStripMenuItem";
            this.logsToolStripMenuItem.Size = new System.Drawing.Size(198, 28);
            this.logsToolStripMenuItem.Text = "Logs";
            this.logsToolStripMenuItem.Click += new System.EventHandler(this.logsToolStripMenuItem_Click);
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
        private BrightIdeasSoftware.OLVColumn olvColumnBotState;
        private BrightIdeasSoftware.OLVColumn olvColumnProxy;
        private BrightIdeasSoftware.OLVColumn olvColumnMaxLevel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem proxiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importProxiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem clearProxiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAccountsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportStatsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem enableColorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem devToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem garbageCollectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportProxiesToolStripMenuItem;
        private BrightIdeasSoftware.OLVColumn olvColumnAccountState;
        private BrightIdeasSoftware.OLVColumn olvColumnPokemonCaught;
        private BrightIdeasSoftware.OLVColumn olvColumnPokestopsFarmed;
        private System.Windows.Forms.ToolStripMenuItem clearCountsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem countsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logsToolStripMenuItem;
    }
}

