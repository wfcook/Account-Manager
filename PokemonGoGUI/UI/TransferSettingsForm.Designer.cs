namespace PokemonGoGUI.UI
{
    partial class TransferSettingsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.checkBoxTransfer = new System.Windows.Forms.CheckBox();
            this.comboBoxTransferType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownKeepMax = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownMinCP = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownIVPercent = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownKeepMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinCP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIVPercent)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Transfer Type:";
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(248, 303);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // checkBoxTransfer
            // 
            this.checkBoxTransfer.AutoSize = true;
            this.checkBoxTransfer.Location = new System.Drawing.Point(28, 12);
            this.checkBoxTransfer.Name = "checkBoxTransfer";
            this.checkBoxTransfer.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxTransfer.Size = new System.Drawing.Size(80, 20);
            this.checkBoxTransfer.TabIndex = 2;
            this.checkBoxTransfer.Text = "Transfer";
            this.checkBoxTransfer.UseVisualStyleBackColor = true;
            // 
            // comboBoxTransferType
            // 
            this.comboBoxTransferType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTransferType.FormattingEnabled = true;
            this.comboBoxTransferType.Location = new System.Drawing.Point(114, 46);
            this.comboBoxTransferType.Name = "comboBoxTransferType";
            this.comboBoxTransferType.Size = new System.Drawing.Size(208, 24);
            this.comboBoxTransferType.TabIndex = 3;
            this.comboBoxTransferType.SelectedIndexChanged += new System.EventHandler(this.comboBoxTransferType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 16);
            this.label2.TabIndex = 0;
            this.label2.Text = "Keep Max:";
            // 
            // numericUpDownKeepMax
            // 
            this.numericUpDownKeepMax.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownKeepMax.Location = new System.Drawing.Point(114, 76);
            this.numericUpDownKeepMax.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numericUpDownKeepMax.Name = "numericUpDownKeepMax";
            this.numericUpDownKeepMax.Size = new System.Drawing.Size(208, 22);
            this.numericUpDownKeepMax.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(56, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "Min CP:";
            // 
            // numericUpDownMinCP
            // 
            this.numericUpDownMinCP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownMinCP.Location = new System.Drawing.Point(115, 104);
            this.numericUpDownMinCP.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numericUpDownMinCP.Name = "numericUpDownMinCP";
            this.numericUpDownMinCP.Size = new System.Drawing.Size(208, 22);
            this.numericUpDownMinCP.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(70, 134);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 16);
            this.label4.TabIndex = 0;
            this.label4.Text = "IV %:";
            // 
            // numericUpDownIVPercent
            // 
            this.numericUpDownIVPercent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownIVPercent.Location = new System.Drawing.Point(115, 132);
            this.numericUpDownIVPercent.Name = "numericUpDownIVPercent";
            this.numericUpDownIVPercent.Size = new System.Drawing.Size(208, 22);
            this.numericUpDownIVPercent.TabIndex = 4;
            this.numericUpDownIVPercent.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
            // 
            // TransferSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(335, 338);
            this.Controls.Add(this.numericUpDownIVPercent);
            this.Controls.Add(this.numericUpDownMinCP);
            this.Controls.Add(this.numericUpDownKeepMax);
            this.Controls.Add(this.comboBoxTransferType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkBoxTransfer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "TransferSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Transfer Settings";
            this.Load += new System.EventHandler(this.TransferSettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownKeepMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinCP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIVPercent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.CheckBox checkBoxTransfer;
        private System.Windows.Forms.ComboBox comboBoxTransferType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownKeepMax;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownMinCP;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownIVPercent;
    }
}