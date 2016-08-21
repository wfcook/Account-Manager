namespace PokemonGoGUI.UI
{
    partial class SchedulerSettingForm
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
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownStartTime = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownEndTime = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownPokemonMin = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDownPokemonMax = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDownPokestopsMin = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownPokestopsMax = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBoxPokemonAction = new System.Windows.Forms.ComboBox();
            this.comboBoxPokestopAction = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBoxMasterAction = new System.Windows.Forms.ComboBox();
            this.toolTipMasterAction = new System.Windows.Forms.ToolTip(this.components);
            this.label10 = new System.Windows.Forms.Label();
            this.numericUpDownCheckSpeed = new System.Windows.Forms.NumericUpDown();
            this.buttonDone = new System.Windows.Forms.Button();
            this.colorDialogNameColor = new System.Windows.Forms.ColorDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.textBoxChosenColor = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEndTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPokemonMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPokemonMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPokestopsMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPokestopsMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCheckSpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(76, 12);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(168, 22);
            this.textBoxName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Scheduler Time:";
            // 
            // numericUpDownStartTime
            // 
            this.numericUpDownStartTime.DecimalPlaces = 2;
            this.numericUpDownStartTime.Location = new System.Drawing.Point(136, 69);
            this.numericUpDownStartTime.Maximum = new decimal(new int[] {
            2399,
            0,
            0,
            131072});
            this.numericUpDownStartTime.Name = "numericUpDownStartTime";
            this.numericUpDownStartTime.Size = new System.Drawing.Size(68, 22);
            this.numericUpDownStartTime.TabIndex = 2;
            // 
            // numericUpDownEndTime
            // 
            this.numericUpDownEndTime.DecimalPlaces = 2;
            this.numericUpDownEndTime.Location = new System.Drawing.Point(235, 69);
            this.numericUpDownEndTime.Maximum = new decimal(new int[] {
            2399,
            0,
            0,
            131072});
            this.numericUpDownEndTime.Name = "numericUpDownEndTime";
            this.numericUpDownEndTime.Size = new System.Drawing.Size(68, 22);
            this.numericUpDownEndTime.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(210, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(19, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "to";
            // 
            // numericUpDownPokemonMin
            // 
            this.numericUpDownPokemonMin.Location = new System.Drawing.Point(97, 158);
            this.numericUpDownPokemonMin.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPokemonMin.Name = "numericUpDownPokemonMin";
            this.numericUpDownPokemonMin.Size = new System.Drawing.Size(75, 22);
            this.numericUpDownPokemonMin.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 160);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "Pokemon:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(103, 139);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 16);
            this.label5.TabIndex = 2;
            this.label5.Text = "Minimum";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(208, 139);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 16);
            this.label6.TabIndex = 2;
            this.label6.Text = "Maximum";
            // 
            // numericUpDownPokemonMax
            // 
            this.numericUpDownPokemonMax.Location = new System.Drawing.Point(198, 158);
            this.numericUpDownPokemonMax.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPokemonMax.Name = "numericUpDownPokemonMax";
            this.numericUpDownPokemonMax.Size = new System.Drawing.Size(75, 22);
            this.numericUpDownPokemonMax.TabIndex = 6;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 191);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 16);
            this.label7.TabIndex = 2;
            this.label7.Text = "Pokestops:";
            // 
            // numericUpDownPokestopsMin
            // 
            this.numericUpDownPokestopsMin.Location = new System.Drawing.Point(97, 189);
            this.numericUpDownPokestopsMin.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPokestopsMin.Name = "numericUpDownPokestopsMin";
            this.numericUpDownPokestopsMin.Size = new System.Drawing.Size(75, 22);
            this.numericUpDownPokestopsMin.TabIndex = 8;
            // 
            // numericUpDownPokestopsMax
            // 
            this.numericUpDownPokestopsMax.Location = new System.Drawing.Point(198, 189);
            this.numericUpDownPokestopsMax.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPokestopsMax.Name = "numericUpDownPokestopsMax";
            this.numericUpDownPokestopsMax.Size = new System.Drawing.Size(75, 22);
            this.numericUpDownPokestopsMax.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(326, 138);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 16);
            this.label8.TabIndex = 5;
            this.label8.Text = "Action";
            // 
            // comboBoxPokemonAction
            // 
            this.comboBoxPokemonAction.FormattingEnabled = true;
            this.comboBoxPokemonAction.Location = new System.Drawing.Point(299, 157);
            this.comboBoxPokemonAction.Name = "comboBoxPokemonAction";
            this.comboBoxPokemonAction.Size = new System.Drawing.Size(121, 24);
            this.comboBoxPokemonAction.TabIndex = 7;
            // 
            // comboBoxPokestopAction
            // 
            this.comboBoxPokestopAction.FormattingEnabled = true;
            this.comboBoxPokestopAction.Location = new System.Drawing.Point(299, 188);
            this.comboBoxPokestopAction.Name = "comboBoxPokestopAction";
            this.comboBoxPokestopAction.Size = new System.Drawing.Size(121, 24);
            this.comboBoxPokestopAction.TabIndex = 10;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(179, 221);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 16);
            this.label9.TabIndex = 7;
            this.label9.Text = "Overall Action:";
            // 
            // comboBoxMasterAction
            // 
            this.comboBoxMasterAction.FormattingEnabled = true;
            this.comboBoxMasterAction.Location = new System.Drawing.Point(299, 218);
            this.comboBoxMasterAction.Name = "comboBoxMasterAction";
            this.comboBoxMasterAction.Size = new System.Drawing.Size(121, 24);
            this.comboBoxMasterAction.TabIndex = 11;
            this.toolTipMasterAction.SetToolTip(this.comboBoxMasterAction, "This occurs when all maximum values have been reached.");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(24, 105);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(150, 16);
            this.label10.TabIndex = 8;
            this.label10.Text = "Check Speed (minutes):";
            // 
            // numericUpDownCheckSpeed
            // 
            this.numericUpDownCheckSpeed.Location = new System.Drawing.Point(182, 103);
            this.numericUpDownCheckSpeed.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownCheckSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownCheckSpeed.Name = "numericUpDownCheckSpeed";
            this.numericUpDownCheckSpeed.Size = new System.Drawing.Size(68, 22);
            this.numericUpDownCheckSpeed.TabIndex = 4;
            this.numericUpDownCheckSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonDone
            // 
            this.buttonDone.Location = new System.Drawing.Point(270, 248);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(150, 23);
            this.buttonDone.TabIndex = 12;
            this.buttonDone.Text = "Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
            // 
            // colorDialogNameColor
            // 
            this.colorDialogNameColor.AnyColor = true;
            this.colorDialogNameColor.Color = System.Drawing.Color.LightGray;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(213, 41);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Choose ...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(22, 43);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(83, 16);
            this.label11.TabIndex = 7;
            this.label11.Text = "Name Color:";
            // 
            // textBoxChosenColor
            // 
            this.textBoxChosenColor.Location = new System.Drawing.Point(106, 41);
            this.textBoxChosenColor.Name = "textBoxChosenColor";
            this.textBoxChosenColor.Size = new System.Drawing.Size(98, 22);
            this.textBoxChosenColor.TabIndex = 1;
            this.textBoxChosenColor.Text = "Color";
            // 
            // SchedulerSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 283);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.comboBoxMasterAction);
            this.Controls.Add(this.comboBoxPokestopAction);
            this.Controls.Add(this.comboBoxPokemonAction);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.numericUpDownPokestopsMax);
            this.Controls.Add(this.numericUpDownPokestopsMin);
            this.Controls.Add(this.numericUpDownPokemonMax);
            this.Controls.Add(this.numericUpDownPokemonMin);
            this.Controls.Add(this.numericUpDownEndTime);
            this.Controls.Add(this.numericUpDownCheckSpeed);
            this.Controls.Add(this.numericUpDownStartTime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxChosenColor);
            this.Controls.Add(this.textBoxName);
            this.Name = "SchedulerSettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SchedulerSettingForm";
            this.Load += new System.EventHandler(this.SchedulerSettingForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEndTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPokemonMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPokemonMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPokestopsMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPokestopsMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCheckSpeed)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownStartTime;
        private System.Windows.Forms.NumericUpDown numericUpDownEndTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownPokemonMin;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUpDownPokemonMax;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDownPokestopsMin;
        private System.Windows.Forms.NumericUpDown numericUpDownPokestopsMax;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBoxPokemonAction;
        private System.Windows.Forms.ComboBox comboBoxPokestopAction;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox comboBoxMasterAction;
        private System.Windows.Forms.ToolTip toolTipMasterAction;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericUpDownCheckSpeed;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.ColorDialog colorDialogNameColor;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBoxChosenColor;
    }
}