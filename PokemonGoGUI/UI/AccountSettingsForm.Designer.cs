namespace PokemonGoGUI.UI
{
    partial class AccountSettingsForm
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
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageDetails = new System.Windows.Forms.TabPage();
            this.buttonImportConfig = new System.Windows.Forms.Button();
            this.buttonExportConfig = new System.Windows.Forms.Button();
            this.comboBoxLocationPresets = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.checkBoxIncubateEggs = new System.Windows.Forms.CheckBox();
            this.checkBoxUseLuckyEgg = new System.Windows.Forms.CheckBox();
            this.checkBoxRecycle = new System.Windows.Forms.CheckBox();
            this.checkBoxEvolve = new System.Windows.Forms.CheckBox();
            this.checkBoxCatchPokemon = new System.Windows.Forms.CheckBox();
            this.checkBoxTransfers = new System.Windows.Forms.CheckBox();
            this.buttonDone = new System.Windows.Forms.Button();
            this.checkBoxEncounterWhileWalking = new System.Windows.Forms.CheckBox();
            this.checkBoxMimicWalking = new System.Windows.Forms.CheckBox();
            this.textBoxMaxLevel = new System.Windows.Forms.TextBox();
            this.textBoxPokemonBeforeEvolve = new System.Windows.Forms.TextBox();
            this.textBoxWalkSpeed = new System.Windows.Forms.TextBox();
            this.textBoxMaxTravelDistance = new System.Windows.Forms.TextBox();
            this.textBoxLong = new System.Windows.Forms.TextBox();
            this.textBoxLat = new System.Windows.Forms.TextBox();
            this.textBoxPtcPassword = new System.Windows.Forms.TextBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBoxPtcUsername = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.radioButtonGoogle = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.radioButtonPtc = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.tabPageRecycling = new System.Windows.Forms.TabPage();
            this.fastObjectListViewRecycling = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumnItemName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnItemMax = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.contextMenuStripRecycling = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageEvolving = new System.Windows.Forms.TabPage();
            this.fastObjectListViewEvolve = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumnEvolveId = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnEvolveName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnEvolve = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnEvolveMinCP = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.contextMenuStripEvolve = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setEvolveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trueToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.falseToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editCPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreDefaultsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageCatching = new System.Windows.Forms.TabPage();
            this.fastObjectListViewCatch = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumnCatchId = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnCatchName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnCatch = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.contextMenuStripCatching = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setCatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.falseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreDefaultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageTransfer = new System.Windows.Forms.TabPage();
            this.fastObjectListViewTransfer = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumnTransferId = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTransferName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTransfer = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTransferType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnCPPercent = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.contextMenuStripTransfer = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreDefaultsToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.label12 = new System.Windows.Forms.Label();
            this.textBoxProxy = new System.Windows.Forms.TextBox();
            this.toolTipProxy = new System.Windows.Forms.ToolTip(this.components);
            this.tabControlMain.SuspendLayout();
            this.tabPageDetails.SuspendLayout();
            this.tabPageRecycling.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewRecycling)).BeginInit();
            this.contextMenuStripRecycling.SuspendLayout();
            this.tabPageEvolving.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewEvolve)).BeginInit();
            this.contextMenuStripEvolve.SuspendLayout();
            this.tabPageCatching.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewCatch)).BeginInit();
            this.contextMenuStripCatching.SuspendLayout();
            this.tabPageTransfer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewTransfer)).BeginInit();
            this.contextMenuStripTransfer.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageDetails);
            this.tabControlMain.Controls.Add(this.tabPageRecycling);
            this.tabControlMain.Controls.Add(this.tabPageEvolving);
            this.tabControlMain.Controls.Add(this.tabPageCatching);
            this.tabControlMain.Controls.Add(this.tabPageTransfer);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(502, 381);
            this.tabControlMain.TabIndex = 0;
            // 
            // tabPageDetails
            // 
            this.tabPageDetails.Controls.Add(this.buttonImportConfig);
            this.tabPageDetails.Controls.Add(this.buttonExportConfig);
            this.tabPageDetails.Controls.Add(this.comboBoxLocationPresets);
            this.tabPageDetails.Controls.Add(this.label10);
            this.tabPageDetails.Controls.Add(this.checkBoxIncubateEggs);
            this.tabPageDetails.Controls.Add(this.checkBoxUseLuckyEgg);
            this.tabPageDetails.Controls.Add(this.checkBoxRecycle);
            this.tabPageDetails.Controls.Add(this.checkBoxEvolve);
            this.tabPageDetails.Controls.Add(this.checkBoxCatchPokemon);
            this.tabPageDetails.Controls.Add(this.checkBoxTransfers);
            this.tabPageDetails.Controls.Add(this.buttonDone);
            this.tabPageDetails.Controls.Add(this.checkBoxEncounterWhileWalking);
            this.tabPageDetails.Controls.Add(this.checkBoxMimicWalking);
            this.tabPageDetails.Controls.Add(this.textBoxMaxLevel);
            this.tabPageDetails.Controls.Add(this.textBoxProxy);
            this.tabPageDetails.Controls.Add(this.textBoxPokemonBeforeEvolve);
            this.tabPageDetails.Controls.Add(this.textBoxWalkSpeed);
            this.tabPageDetails.Controls.Add(this.textBoxMaxTravelDistance);
            this.tabPageDetails.Controls.Add(this.textBoxLong);
            this.tabPageDetails.Controls.Add(this.textBoxLat);
            this.tabPageDetails.Controls.Add(this.textBoxPtcPassword);
            this.tabPageDetails.Controls.Add(this.textBoxName);
            this.tabPageDetails.Controls.Add(this.label11);
            this.tabPageDetails.Controls.Add(this.label12);
            this.tabPageDetails.Controls.Add(this.textBoxPtcUsername);
            this.tabPageDetails.Controls.Add(this.label9);
            this.tabPageDetails.Controls.Add(this.radioButtonGoogle);
            this.tabPageDetails.Controls.Add(this.label6);
            this.tabPageDetails.Controls.Add(this.label8);
            this.tabPageDetails.Controls.Add(this.radioButtonPtc);
            this.tabPageDetails.Controls.Add(this.label5);
            this.tabPageDetails.Controls.Add(this.label4);
            this.tabPageDetails.Controls.Add(this.label7);
            this.tabPageDetails.Controls.Add(this.label3);
            this.tabPageDetails.Controls.Add(this.label2);
            this.tabPageDetails.Controls.Add(this.label1);
            this.tabPageDetails.Controls.Add(this.buttonSave);
            this.tabPageDetails.Location = new System.Drawing.Point(4, 25);
            this.tabPageDetails.Name = "tabPageDetails";
            this.tabPageDetails.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDetails.Size = new System.Drawing.Size(494, 352);
            this.tabPageDetails.TabIndex = 1;
            this.tabPageDetails.Text = "Details";
            this.tabPageDetails.UseVisualStyleBackColor = true;
            // 
            // buttonImportConfig
            // 
            this.buttonImportConfig.Location = new System.Drawing.Point(93, 321);
            this.buttonImportConfig.Name = "buttonImportConfig";
            this.buttonImportConfig.Size = new System.Drawing.Size(128, 23);
            this.buttonImportConfig.TabIndex = 11;
            this.buttonImportConfig.Text = "Import Config";
            this.buttonImportConfig.UseVisualStyleBackColor = true;
            this.buttonImportConfig.Click += new System.EventHandler(this.buttonImportConfig_Click);
            // 
            // buttonExportConfig
            // 
            this.buttonExportConfig.Location = new System.Drawing.Point(227, 321);
            this.buttonExportConfig.Name = "buttonExportConfig";
            this.buttonExportConfig.Size = new System.Drawing.Size(128, 23);
            this.buttonExportConfig.TabIndex = 10;
            this.buttonExportConfig.Text = "Export Config";
            this.buttonExportConfig.UseVisualStyleBackColor = true;
            this.buttonExportConfig.Click += new System.EventHandler(this.buttonExportConfig_Click);
            // 
            // comboBoxLocationPresets
            // 
            this.comboBoxLocationPresets.FormattingEnabled = true;
            this.comboBoxLocationPresets.Location = new System.Drawing.Point(85, 111);
            this.comboBoxLocationPresets.Name = "comboBoxLocationPresets";
            this.comboBoxLocationPresets.Size = new System.Drawing.Size(146, 24);
            this.comboBoxLocationPresets.TabIndex = 9;
            this.comboBoxLocationPresets.SelectedIndexChanged += new System.EventHandler(this.comboBoxLocationPresets_SelectedIndexChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(28, 114);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(50, 16);
            this.label10.TabIndex = 8;
            this.label10.Text = "Preset:";
            // 
            // checkBoxIncubateEggs
            // 
            this.checkBoxIncubateEggs.AutoSize = true;
            this.checkBoxIncubateEggs.Location = new System.Drawing.Point(355, 110);
            this.checkBoxIncubateEggs.Name = "checkBoxIncubateEggs";
            this.checkBoxIncubateEggs.Size = new System.Drawing.Size(116, 20);
            this.checkBoxIncubateEggs.TabIndex = 7;
            this.checkBoxIncubateEggs.Text = "Incubate Eggs";
            this.checkBoxIncubateEggs.UseVisualStyleBackColor = true;
            // 
            // checkBoxUseLuckyEgg
            // 
            this.checkBoxUseLuckyEgg.AutoSize = true;
            this.checkBoxUseLuckyEgg.Location = new System.Drawing.Point(355, 136);
            this.checkBoxUseLuckyEgg.Name = "checkBoxUseLuckyEgg";
            this.checkBoxUseLuckyEgg.Size = new System.Drawing.Size(121, 20);
            this.checkBoxUseLuckyEgg.TabIndex = 7;
            this.checkBoxUseLuckyEgg.Text = "Use Lucky Egg";
            this.checkBoxUseLuckyEgg.UseVisualStyleBackColor = true;
            // 
            // checkBoxRecycle
            // 
            this.checkBoxRecycle.AutoSize = true;
            this.checkBoxRecycle.Location = new System.Drawing.Point(355, 84);
            this.checkBoxRecycle.Name = "checkBoxRecycle";
            this.checkBoxRecycle.Size = new System.Drawing.Size(110, 20);
            this.checkBoxRecycle.TabIndex = 7;
            this.checkBoxRecycle.Text = "Auto Recycle";
            this.checkBoxRecycle.UseVisualStyleBackColor = true;
            // 
            // checkBoxEvolve
            // 
            this.checkBoxEvolve.AutoSize = true;
            this.checkBoxEvolve.Location = new System.Drawing.Point(355, 58);
            this.checkBoxEvolve.Name = "checkBoxEvolve";
            this.checkBoxEvolve.Size = new System.Drawing.Size(102, 20);
            this.checkBoxEvolve.TabIndex = 7;
            this.checkBoxEvolve.Text = "Auto Evolve";
            this.checkBoxEvolve.UseVisualStyleBackColor = true;
            // 
            // checkBoxCatchPokemon
            // 
            this.checkBoxCatchPokemon.AutoSize = true;
            this.checkBoxCatchPokemon.Location = new System.Drawing.Point(355, 8);
            this.checkBoxCatchPokemon.Name = "checkBoxCatchPokemon";
            this.checkBoxCatchPokemon.Size = new System.Drawing.Size(122, 20);
            this.checkBoxCatchPokemon.TabIndex = 7;
            this.checkBoxCatchPokemon.Text = "CatchPokemon";
            this.checkBoxCatchPokemon.UseVisualStyleBackColor = true;
            // 
            // checkBoxTransfers
            // 
            this.checkBoxTransfers.AutoSize = true;
            this.checkBoxTransfers.Location = new System.Drawing.Point(355, 32);
            this.checkBoxTransfers.Name = "checkBoxTransfers";
            this.checkBoxTransfers.Size = new System.Drawing.Size(110, 20);
            this.checkBoxTransfers.TabIndex = 7;
            this.checkBoxTransfers.Text = "Auto Transfer";
            this.checkBoxTransfers.UseVisualStyleBackColor = true;
            // 
            // buttonDone
            // 
            this.buttonDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDone.Location = new System.Drawing.Point(411, 321);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(75, 23);
            this.buttonDone.TabIndex = 6;
            this.buttonDone.Text = "Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
            // 
            // checkBoxEncounterWhileWalking
            // 
            this.checkBoxEncounterWhileWalking.AutoSize = true;
            this.checkBoxEncounterWhileWalking.Enabled = false;
            this.checkBoxEncounterWhileWalking.Location = new System.Drawing.Point(8, 292);
            this.checkBoxEncounterWhileWalking.Name = "checkBoxEncounterWhileWalking";
            this.checkBoxEncounterWhileWalking.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxEncounterWhileWalking.Size = new System.Drawing.Size(179, 20);
            this.checkBoxEncounterWhileWalking.TabIndex = 5;
            this.checkBoxEncounterWhileWalking.Text = "Encounter While Walking";
            this.checkBoxEncounterWhileWalking.UseVisualStyleBackColor = true;
            // 
            // checkBoxMimicWalking
            // 
            this.checkBoxMimicWalking.AutoSize = true;
            this.checkBoxMimicWalking.Location = new System.Drawing.Point(5, 229);
            this.checkBoxMimicWalking.Name = "checkBoxMimicWalking";
            this.checkBoxMimicWalking.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxMimicWalking.Size = new System.Drawing.Size(117, 20);
            this.checkBoxMimicWalking.TabIndex = 4;
            this.checkBoxMimicWalking.Text = "Mimic Walking";
            this.checkBoxMimicWalking.UseVisualStyleBackColor = true;
            this.checkBoxMimicWalking.CheckedChanged += new System.EventHandler(this.checkBoxMimicWalking_CheckedChanged);
            // 
            // textBoxMaxLevel
            // 
            this.textBoxMaxLevel.Location = new System.Drawing.Point(400, 190);
            this.textBoxMaxLevel.Name = "textBoxMaxLevel";
            this.textBoxMaxLevel.Size = new System.Drawing.Size(65, 22);
            this.textBoxMaxLevel.TabIndex = 3;
            // 
            // textBoxPokemonBeforeEvolve
            // 
            this.textBoxPokemonBeforeEvolve.Location = new System.Drawing.Point(400, 162);
            this.textBoxPokemonBeforeEvolve.Name = "textBoxPokemonBeforeEvolve";
            this.textBoxPokemonBeforeEvolve.Size = new System.Drawing.Size(65, 22);
            this.textBoxPokemonBeforeEvolve.TabIndex = 3;
            // 
            // textBoxWalkSpeed
            // 
            this.textBoxWalkSpeed.Enabled = false;
            this.textBoxWalkSpeed.Location = new System.Drawing.Point(105, 255);
            this.textBoxWalkSpeed.Name = "textBoxWalkSpeed";
            this.textBoxWalkSpeed.Size = new System.Drawing.Size(127, 22);
            this.textBoxWalkSpeed.TabIndex = 3;
            // 
            // textBoxMaxTravelDistance
            // 
            this.textBoxMaxTravelDistance.Location = new System.Drawing.Point(85, 201);
            this.textBoxMaxTravelDistance.Name = "textBoxMaxTravelDistance";
            this.textBoxMaxTravelDistance.Size = new System.Drawing.Size(146, 22);
            this.textBoxMaxTravelDistance.TabIndex = 3;
            // 
            // textBoxLong
            // 
            this.textBoxLong.Location = new System.Drawing.Point(85, 173);
            this.textBoxLong.Name = "textBoxLong";
            this.textBoxLong.Size = new System.Drawing.Size(146, 22);
            this.textBoxLong.TabIndex = 3;
            // 
            // textBoxLat
            // 
            this.textBoxLat.Location = new System.Drawing.Point(85, 145);
            this.textBoxLat.Name = "textBoxLat";
            this.textBoxLat.Size = new System.Drawing.Size(146, 22);
            this.textBoxLat.TabIndex = 3;
            // 
            // textBoxPtcPassword
            // 
            this.textBoxPtcPassword.Location = new System.Drawing.Point(86, 83);
            this.textBoxPtcPassword.Name = "textBoxPtcPassword";
            this.textBoxPtcPassword.Size = new System.Drawing.Size(146, 22);
            this.textBoxPtcPassword.TabIndex = 3;
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(103, 6);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(128, 22);
            this.textBoxName.TabIndex = 3;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(322, 193);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 16);
            this.label11.TabIndex = 1;
            this.label11.Text = "Max Level:";
            // 
            // textBoxPtcUsername
            // 
            this.textBoxPtcUsername.Location = new System.Drawing.Point(86, 55);
            this.textBoxPtcUsername.Name = "textBoxPtcUsername";
            this.textBoxPtcUsername.Size = new System.Drawing.Size(146, 22);
            this.textBoxPtcUsername.TabIndex = 3;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(237, 165);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(157, 16);
            this.label9.TabIndex = 1;
            this.label9.Text = "Pokemon Before Evolve:";
            // 
            // radioButtonGoogle
            // 
            this.radioButtonGoogle.AutoSize = true;
            this.radioButtonGoogle.Enabled = false;
            this.radioButtonGoogle.Location = new System.Drawing.Point(211, 29);
            this.radioButtonGoogle.Name = "radioButtonGoogle";
            this.radioButtonGoogle.Size = new System.Drawing.Size(74, 20);
            this.radioButtonGoogle.TabIndex = 2;
            this.radioButtonGoogle.Text = "Google";
            this.radioButtonGoogle.UseVisualStyleBackColor = true;
            this.radioButtonGoogle.CheckedChanged += new System.EventHandler(this.radioButtonPtc_CheckedChanged_1);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 258);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 16);
            this.label6.TabIndex = 1;
            this.label6.Text = "Speed (km/h):";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(-1, 204);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 16);
            this.label8.TabIndex = 1;
            this.label8.Text = "Max Travel:";
            // 
            // radioButtonPtc
            // 
            this.radioButtonPtc.AutoSize = true;
            this.radioButtonPtc.Checked = true;
            this.radioButtonPtc.Location = new System.Drawing.Point(138, 29);
            this.radioButtonPtc.Name = "radioButtonPtc";
            this.radioButtonPtc.Size = new System.Drawing.Size(48, 20);
            this.radioButtonPtc.TabIndex = 2;
            this.radioButtonPtc.TabStop = true;
            this.radioButtonPtc.Text = "Ptc";
            this.radioButtonPtc.UseVisualStyleBackColor = true;
            this.radioButtonPtc.CheckedChanged += new System.EventHandler(this.radioButtonPtc_CheckedChanged_1);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 176);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 16);
            this.label5.TabIndex = 1;
            this.label5.Text = "Longitude:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 16);
            this.label4.TabIndex = 1;
            this.label4.Text = "Latitude:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(30, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 16);
            this.label7.TabIndex = 1;
            this.label7.Text = "Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 16);
            this.label3.TabIndex = 1;
            this.label3.Text = "Password:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Username:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Authentication Type:";
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(12, 321);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 0;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // tabPageRecycling
            // 
            this.tabPageRecycling.Controls.Add(this.fastObjectListViewRecycling);
            this.tabPageRecycling.Location = new System.Drawing.Point(4, 25);
            this.tabPageRecycling.Name = "tabPageRecycling";
            this.tabPageRecycling.Size = new System.Drawing.Size(494, 352);
            this.tabPageRecycling.TabIndex = 2;
            this.tabPageRecycling.Text = "Recycling";
            this.tabPageRecycling.UseVisualStyleBackColor = true;
            // 
            // fastObjectListViewRecycling
            // 
            this.fastObjectListViewRecycling.AllColumns.Add(this.olvColumnItemName);
            this.fastObjectListViewRecycling.AllColumns.Add(this.olvColumnItemMax);
            this.fastObjectListViewRecycling.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnItemName,
            this.olvColumnItemMax});
            this.fastObjectListViewRecycling.ContextMenuStrip = this.contextMenuStripRecycling;
            this.fastObjectListViewRecycling.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastObjectListViewRecycling.FullRowSelect = true;
            this.fastObjectListViewRecycling.Location = new System.Drawing.Point(0, 0);
            this.fastObjectListViewRecycling.Name = "fastObjectListViewRecycling";
            this.fastObjectListViewRecycling.ShowGroups = false;
            this.fastObjectListViewRecycling.Size = new System.Drawing.Size(494, 352);
            this.fastObjectListViewRecycling.TabIndex = 0;
            this.fastObjectListViewRecycling.UseCompatibleStateImageBehavior = false;
            this.fastObjectListViewRecycling.View = System.Windows.Forms.View.Details;
            this.fastObjectListViewRecycling.VirtualMode = true;
            // 
            // olvColumnItemName
            // 
            this.olvColumnItemName.AspectName = "FriendlyName";
            this.olvColumnItemName.Text = "Name";
            this.olvColumnItemName.Width = 146;
            // 
            // olvColumnItemMax
            // 
            this.olvColumnItemMax.AspectName = "MaxInventory";
            this.olvColumnItemMax.Text = "Max Inventory";
            this.olvColumnItemMax.Width = 129;
            // 
            // contextMenuStripRecycling
            // 
            this.contextMenuStripRecycling.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.contextMenuStripRecycling.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.contextMenuStripRecycling.Name = "contextMenuStripRecycling";
            this.contextMenuStripRecycling.Size = new System.Drawing.Size(120, 32);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(119, 28);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // tabPageEvolving
            // 
            this.tabPageEvolving.Controls.Add(this.fastObjectListViewEvolve);
            this.tabPageEvolving.Location = new System.Drawing.Point(4, 25);
            this.tabPageEvolving.Name = "tabPageEvolving";
            this.tabPageEvolving.Size = new System.Drawing.Size(494, 352);
            this.tabPageEvolving.TabIndex = 3;
            this.tabPageEvolving.Text = "Evolving";
            this.tabPageEvolving.UseVisualStyleBackColor = true;
            // 
            // fastObjectListViewEvolve
            // 
            this.fastObjectListViewEvolve.AllColumns.Add(this.olvColumnEvolveId);
            this.fastObjectListViewEvolve.AllColumns.Add(this.olvColumnEvolveName);
            this.fastObjectListViewEvolve.AllColumns.Add(this.olvColumnEvolve);
            this.fastObjectListViewEvolve.AllColumns.Add(this.olvColumnEvolveMinCP);
            this.fastObjectListViewEvolve.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnEvolveId,
            this.olvColumnEvolveName,
            this.olvColumnEvolve,
            this.olvColumnEvolveMinCP});
            this.fastObjectListViewEvolve.ContextMenuStrip = this.contextMenuStripEvolve;
            this.fastObjectListViewEvolve.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastObjectListViewEvolve.FullRowSelect = true;
            this.fastObjectListViewEvolve.Location = new System.Drawing.Point(0, 0);
            this.fastObjectListViewEvolve.Name = "fastObjectListViewEvolve";
            this.fastObjectListViewEvolve.ShowGroups = false;
            this.fastObjectListViewEvolve.Size = new System.Drawing.Size(494, 352);
            this.fastObjectListViewEvolve.TabIndex = 2;
            this.fastObjectListViewEvolve.UseCompatibleStateImageBehavior = false;
            this.fastObjectListViewEvolve.View = System.Windows.Forms.View.Details;
            this.fastObjectListViewEvolve.VirtualMode = true;
            // 
            // olvColumnEvolveId
            // 
            this.olvColumnEvolveId.Text = "Id";
            // 
            // olvColumnEvolveName
            // 
            this.olvColumnEvolveName.AspectName = "Name";
            this.olvColumnEvolveName.Text = "Name";
            this.olvColumnEvolveName.Width = 128;
            // 
            // olvColumnEvolve
            // 
            this.olvColumnEvolve.AspectName = "Evolve";
            this.olvColumnEvolve.Text = "Evolve";
            this.olvColumnEvolve.Width = 74;
            // 
            // olvColumnEvolveMinCP
            // 
            this.olvColumnEvolveMinCP.AspectName = "MinCP";
            this.olvColumnEvolveMinCP.Text = "Min CP";
            // 
            // contextMenuStripEvolve
            // 
            this.contextMenuStripEvolve.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.contextMenuStripEvolve.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setEvolveToolStripMenuItem,
            this.editCPToolStripMenuItem,
            this.restoreDefaultsToolStripMenuItem1});
            this.contextMenuStripEvolve.Name = "contextMenuStripEvolve";
            this.contextMenuStripEvolve.Size = new System.Drawing.Size(215, 88);
            // 
            // setEvolveToolStripMenuItem
            // 
            this.setEvolveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trueToolStripMenuItem1,
            this.falseToolStripMenuItem1,
            this.toggleToolStripMenuItem1});
            this.setEvolveToolStripMenuItem.Name = "setEvolveToolStripMenuItem";
            this.setEvolveToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.setEvolveToolStripMenuItem.Text = "Set Evolve";
            // 
            // trueToolStripMenuItem1
            // 
            this.trueToolStripMenuItem1.Name = "trueToolStripMenuItem1";
            this.trueToolStripMenuItem1.Size = new System.Drawing.Size(140, 28);
            this.trueToolStripMenuItem1.Tag = "1";
            this.trueToolStripMenuItem1.Text = "True";
            this.trueToolStripMenuItem1.Click += new System.EventHandler(this.trueToolStripMenuItem1_Click);
            // 
            // falseToolStripMenuItem1
            // 
            this.falseToolStripMenuItem1.Name = "falseToolStripMenuItem1";
            this.falseToolStripMenuItem1.Size = new System.Drawing.Size(140, 28);
            this.falseToolStripMenuItem1.Tag = "0";
            this.falseToolStripMenuItem1.Text = "False";
            this.falseToolStripMenuItem1.Click += new System.EventHandler(this.trueToolStripMenuItem1_Click);
            // 
            // toggleToolStripMenuItem1
            // 
            this.toggleToolStripMenuItem1.Name = "toggleToolStripMenuItem1";
            this.toggleToolStripMenuItem1.Size = new System.Drawing.Size(140, 28);
            this.toggleToolStripMenuItem1.Tag = "2";
            this.toggleToolStripMenuItem1.Text = "Toggle";
            this.toggleToolStripMenuItem1.Click += new System.EventHandler(this.trueToolStripMenuItem1_Click);
            // 
            // editCPToolStripMenuItem
            // 
            this.editCPToolStripMenuItem.Name = "editCPToolStripMenuItem";
            this.editCPToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.editCPToolStripMenuItem.Text = "Edit CP";
            this.editCPToolStripMenuItem.Click += new System.EventHandler(this.editCPToolStripMenuItem_Click);
            // 
            // restoreDefaultsToolStripMenuItem1
            // 
            this.restoreDefaultsToolStripMenuItem1.Name = "restoreDefaultsToolStripMenuItem1";
            this.restoreDefaultsToolStripMenuItem1.Size = new System.Drawing.Size(214, 28);
            this.restoreDefaultsToolStripMenuItem1.Text = "Restore Defaults";
            this.restoreDefaultsToolStripMenuItem1.Click += new System.EventHandler(this.restoreDefaultsToolStripMenuItem1_Click);
            // 
            // tabPageCatching
            // 
            this.tabPageCatching.Controls.Add(this.fastObjectListViewCatch);
            this.tabPageCatching.Location = new System.Drawing.Point(4, 25);
            this.tabPageCatching.Name = "tabPageCatching";
            this.tabPageCatching.Size = new System.Drawing.Size(494, 352);
            this.tabPageCatching.TabIndex = 4;
            this.tabPageCatching.Text = "Catching";
            this.tabPageCatching.UseVisualStyleBackColor = true;
            // 
            // fastObjectListViewCatch
            // 
            this.fastObjectListViewCatch.AllColumns.Add(this.olvColumnCatchId);
            this.fastObjectListViewCatch.AllColumns.Add(this.olvColumnCatchName);
            this.fastObjectListViewCatch.AllColumns.Add(this.olvColumnCatch);
            this.fastObjectListViewCatch.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnCatchId,
            this.olvColumnCatchName,
            this.olvColumnCatch});
            this.fastObjectListViewCatch.ContextMenuStrip = this.contextMenuStripCatching;
            this.fastObjectListViewCatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastObjectListViewCatch.FullRowSelect = true;
            this.fastObjectListViewCatch.Location = new System.Drawing.Point(0, 0);
            this.fastObjectListViewCatch.Name = "fastObjectListViewCatch";
            this.fastObjectListViewCatch.ShowGroups = false;
            this.fastObjectListViewCatch.Size = new System.Drawing.Size(494, 352);
            this.fastObjectListViewCatch.TabIndex = 1;
            this.fastObjectListViewCatch.UseCompatibleStateImageBehavior = false;
            this.fastObjectListViewCatch.View = System.Windows.Forms.View.Details;
            this.fastObjectListViewCatch.VirtualMode = true;
            // 
            // olvColumnCatchId
            // 
            this.olvColumnCatchId.Text = "Id";
            // 
            // olvColumnCatchName
            // 
            this.olvColumnCatchName.AspectName = "Name";
            this.olvColumnCatchName.Text = "Name";
            this.olvColumnCatchName.Width = 146;
            // 
            // olvColumnCatch
            // 
            this.olvColumnCatch.AspectName = "Catch";
            this.olvColumnCatch.Text = "Catch";
            this.olvColumnCatch.Width = 129;
            // 
            // contextMenuStripCatching
            // 
            this.contextMenuStripCatching.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.contextMenuStripCatching.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setCatchToolStripMenuItem,
            this.restoreDefaultsToolStripMenuItem});
            this.contextMenuStripCatching.Name = "contextMenuStripCatching";
            this.contextMenuStripCatching.Size = new System.Drawing.Size(215, 60);
            // 
            // setCatchToolStripMenuItem
            // 
            this.setCatchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trueToolStripMenuItem,
            this.falseToolStripMenuItem,
            this.toggleToolStripMenuItem});
            this.setCatchToolStripMenuItem.Name = "setCatchToolStripMenuItem";
            this.setCatchToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.setCatchToolStripMenuItem.Text = "Set Catch";
            // 
            // trueToolStripMenuItem
            // 
            this.trueToolStripMenuItem.Name = "trueToolStripMenuItem";
            this.trueToolStripMenuItem.Size = new System.Drawing.Size(140, 28);
            this.trueToolStripMenuItem.Tag = "1";
            this.trueToolStripMenuItem.Text = "True";
            this.trueToolStripMenuItem.Click += new System.EventHandler(this.trueToolStripMenuItem_Click);
            // 
            // falseToolStripMenuItem
            // 
            this.falseToolStripMenuItem.Name = "falseToolStripMenuItem";
            this.falseToolStripMenuItem.Size = new System.Drawing.Size(140, 28);
            this.falseToolStripMenuItem.Tag = "0";
            this.falseToolStripMenuItem.Text = "False";
            this.falseToolStripMenuItem.Click += new System.EventHandler(this.trueToolStripMenuItem_Click);
            // 
            // toggleToolStripMenuItem
            // 
            this.toggleToolStripMenuItem.Name = "toggleToolStripMenuItem";
            this.toggleToolStripMenuItem.Size = new System.Drawing.Size(140, 28);
            this.toggleToolStripMenuItem.Tag = "2";
            this.toggleToolStripMenuItem.Text = "Toggle";
            this.toggleToolStripMenuItem.Click += new System.EventHandler(this.trueToolStripMenuItem_Click);
            // 
            // restoreDefaultsToolStripMenuItem
            // 
            this.restoreDefaultsToolStripMenuItem.Name = "restoreDefaultsToolStripMenuItem";
            this.restoreDefaultsToolStripMenuItem.Size = new System.Drawing.Size(214, 28);
            this.restoreDefaultsToolStripMenuItem.Text = "Restore Defaults";
            this.restoreDefaultsToolStripMenuItem.Click += new System.EventHandler(this.restoreDefaultsToolStripMenuItem_Click);
            // 
            // tabPageTransfer
            // 
            this.tabPageTransfer.Controls.Add(this.fastObjectListViewTransfer);
            this.tabPageTransfer.Location = new System.Drawing.Point(4, 25);
            this.tabPageTransfer.Name = "tabPageTransfer";
            this.tabPageTransfer.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTransfer.Size = new System.Drawing.Size(494, 352);
            this.tabPageTransfer.TabIndex = 5;
            this.tabPageTransfer.Text = "Transfer";
            this.tabPageTransfer.UseVisualStyleBackColor = true;
            // 
            // fastObjectListViewTransfer
            // 
            this.fastObjectListViewTransfer.AllColumns.Add(this.olvColumnTransferId);
            this.fastObjectListViewTransfer.AllColumns.Add(this.olvColumnTransferName);
            this.fastObjectListViewTransfer.AllColumns.Add(this.olvColumnTransfer);
            this.fastObjectListViewTransfer.AllColumns.Add(this.olvColumnTransferType);
            this.fastObjectListViewTransfer.AllColumns.Add(this.olvColumn1);
            this.fastObjectListViewTransfer.AllColumns.Add(this.olvColumn2);
            this.fastObjectListViewTransfer.AllColumns.Add(this.olvColumnCPPercent);
            this.fastObjectListViewTransfer.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnTransferId,
            this.olvColumnTransferName,
            this.olvColumnTransfer,
            this.olvColumnTransferType,
            this.olvColumn1,
            this.olvColumn2,
            this.olvColumnCPPercent});
            this.fastObjectListViewTransfer.ContextMenuStrip = this.contextMenuStripTransfer;
            this.fastObjectListViewTransfer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastObjectListViewTransfer.FullRowSelect = true;
            this.fastObjectListViewTransfer.Location = new System.Drawing.Point(3, 3);
            this.fastObjectListViewTransfer.Name = "fastObjectListViewTransfer";
            this.fastObjectListViewTransfer.ShowGroups = false;
            this.fastObjectListViewTransfer.Size = new System.Drawing.Size(488, 346);
            this.fastObjectListViewTransfer.TabIndex = 2;
            this.fastObjectListViewTransfer.UseCompatibleStateImageBehavior = false;
            this.fastObjectListViewTransfer.View = System.Windows.Forms.View.Details;
            this.fastObjectListViewTransfer.VirtualMode = true;
            // 
            // olvColumnTransferId
            // 
            this.olvColumnTransferId.Text = "Id";
            // 
            // olvColumnTransferName
            // 
            this.olvColumnTransferName.AspectName = "Name";
            this.olvColumnTransferName.Text = "Name";
            this.olvColumnTransferName.Width = 146;
            // 
            // olvColumnTransfer
            // 
            this.olvColumnTransfer.AspectName = "Transfer";
            this.olvColumnTransfer.Text = "Transfer";
            this.olvColumnTransfer.Width = 129;
            // 
            // olvColumnTransferType
            // 
            this.olvColumnTransferType.AspectName = "Type";
            this.olvColumnTransferType.Text = "Type";
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "KeepMax";
            this.olvColumn1.Text = "Max";
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "MinCP";
            this.olvColumn2.Text = "Min CP";
            // 
            // olvColumnCPPercent
            // 
            this.olvColumnCPPercent.AspectName = "CPPercent";
            this.olvColumnCPPercent.Text = "Min CP %";
            // 
            // contextMenuStripTransfer
            // 
            this.contextMenuStripTransfer.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.contextMenuStripTransfer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem1,
            this.restoreDefaultsToolStripMenuItem2});
            this.contextMenuStripTransfer.Name = "contextMenuStripTransfer";
            this.contextMenuStripTransfer.Size = new System.Drawing.Size(215, 60);
            // 
            // editToolStripMenuItem1
            // 
            this.editToolStripMenuItem1.Name = "editToolStripMenuItem1";
            this.editToolStripMenuItem1.Size = new System.Drawing.Size(214, 28);
            this.editToolStripMenuItem1.Text = "Edit";
            this.editToolStripMenuItem1.Click += new System.EventHandler(this.editToolStripMenuItem1_Click);
            // 
            // restoreDefaultsToolStripMenuItem2
            // 
            this.restoreDefaultsToolStripMenuItem2.Name = "restoreDefaultsToolStripMenuItem2";
            this.restoreDefaultsToolStripMenuItem2.Size = new System.Drawing.Size(214, 28);
            this.restoreDefaultsToolStripMenuItem2.Text = "Restore Defaults";
            this.restoreDefaultsToolStripMenuItem2.Click += new System.EventHandler(this.restoreDefaultsToolStripMenuItem2_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(240, 230);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(45, 16);
            this.label12.TabIndex = 1;
            this.label12.Text = "Proxy:";
            // 
            // textBoxProxy
            // 
            this.textBoxProxy.Location = new System.Drawing.Point(291, 227);
            this.textBoxProxy.Name = "textBoxProxy";
            this.textBoxProxy.Size = new System.Drawing.Size(180, 22);
            this.textBoxProxy.TabIndex = 3;
            this.toolTipProxy.SetToolTip(this.textBoxProxy, "Valid Formats:\r\nIP:Port\r\nIP:Port:Username\r\nIP:Port:Username:Password");
            // 
            // AccountSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 381);
            this.Controls.Add(this.tabControlMain);
            this.MinimumSize = new System.Drawing.Size(522, 433);
            this.Name = "AccountSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Account Settings";
            this.Load += new System.EventHandler(this.AccountSettingsForm_Load);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageDetails.ResumeLayout(false);
            this.tabPageDetails.PerformLayout();
            this.tabPageRecycling.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewRecycling)).EndInit();
            this.contextMenuStripRecycling.ResumeLayout(false);
            this.tabPageEvolving.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewEvolve)).EndInit();
            this.contextMenuStripEvolve.ResumeLayout(false);
            this.tabPageCatching.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewCatch)).EndInit();
            this.contextMenuStripCatching.ResumeLayout(false);
            this.tabPageTransfer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewTransfer)).EndInit();
            this.contextMenuStripTransfer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageDetails;
        private System.Windows.Forms.RadioButton radioButtonGoogle;
        private System.Windows.Forms.RadioButton radioButtonPtc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.TabPage tabPageRecycling;
        private System.Windows.Forms.TabPage tabPageEvolving;
        private System.Windows.Forms.TabPage tabPageCatching;
        private System.Windows.Forms.TextBox textBoxPtcUsername;
        private System.Windows.Forms.TextBox textBoxPtcPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxLong;
        private System.Windows.Forms.TextBox textBoxLat;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxMimicWalking;
        private System.Windows.Forms.TextBox textBoxWalkSpeed;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxEncounterWhileWalking;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox checkBoxRecycle;
        private System.Windows.Forms.CheckBox checkBoxEvolve;
        private System.Windows.Forms.CheckBox checkBoxTransfers;
        private System.Windows.Forms.TabPage tabPageTransfer;
        private BrightIdeasSoftware.FastObjectListView fastObjectListViewRecycling;
        private BrightIdeasSoftware.OLVColumn olvColumnItemName;
        private BrightIdeasSoftware.OLVColumn olvColumnItemMax;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRecycling;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private BrightIdeasSoftware.FastObjectListView fastObjectListViewCatch;
        private BrightIdeasSoftware.OLVColumn olvColumnCatchName;
        private BrightIdeasSoftware.OLVColumn olvColumnCatch;
        private BrightIdeasSoftware.OLVColumn olvColumnCatchId;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripCatching;
        private System.Windows.Forms.ToolStripMenuItem setCatchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem falseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultsToolStripMenuItem;
        private BrightIdeasSoftware.FastObjectListView fastObjectListViewEvolve;
        private BrightIdeasSoftware.OLVColumn olvColumnEvolveId;
        private BrightIdeasSoftware.OLVColumn olvColumnEvolveName;
        private BrightIdeasSoftware.OLVColumn olvColumnEvolve;
        private BrightIdeasSoftware.OLVColumn olvColumnEvolveMinCP;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripEvolve;
        private System.Windows.Forms.ToolStripMenuItem setEvolveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trueToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem falseToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toggleToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem editCPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultsToolStripMenuItem1;
        private BrightIdeasSoftware.FastObjectListView fastObjectListViewTransfer;
        private BrightIdeasSoftware.OLVColumn olvColumnTransferId;
        private BrightIdeasSoftware.OLVColumn olvColumnTransferName;
        private BrightIdeasSoftware.OLVColumn olvColumnTransfer;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTransfer;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem1;
        private BrightIdeasSoftware.OLVColumn olvColumnTransferType;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultsToolStripMenuItem2;
        private BrightIdeasSoftware.OLVColumn olvColumnCPPercent;
        private System.Windows.Forms.TextBox textBoxMaxTravelDistance;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBoxUseLuckyEgg;
        private System.Windows.Forms.TextBox textBoxPokemonBeforeEvolve;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox comboBoxLocationPresets;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button buttonExportConfig;
        private System.Windows.Forms.CheckBox checkBoxIncubateEggs;
        private System.Windows.Forms.Button buttonImportConfig;
        private System.Windows.Forms.TextBox textBoxMaxLevel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox checkBoxCatchPokemon;
        private System.Windows.Forms.TextBox textBoxProxy;
        private System.Windows.Forms.ToolTip toolTipProxy;
        private System.Windows.Forms.Label label12;
    }
}