namespace PokemonGoGUI.UI
{
    partial class LogViewerForm
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
            this.fastObjectListViewLogs = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumnDate = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnStatus = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnMessage = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnException = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewLogs)).BeginInit();
            this.SuspendLayout();
            // 
            // fastObjectListViewLogs
            // 
            this.fastObjectListViewLogs.AllColumns.Add(this.olvColumnDate);
            this.fastObjectListViewLogs.AllColumns.Add(this.olvColumnStatus);
            this.fastObjectListViewLogs.AllColumns.Add(this.olvColumnMessage);
            this.fastObjectListViewLogs.AllColumns.Add(this.olvColumnException);
            this.fastObjectListViewLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fastObjectListViewLogs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnDate,
            this.olvColumnStatus,
            this.olvColumnMessage,
            this.olvColumnException});
            this.fastObjectListViewLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastObjectListViewLogs.FullRowSelect = true;
            this.fastObjectListViewLogs.Location = new System.Drawing.Point(0, 0);
            this.fastObjectListViewLogs.Margin = new System.Windows.Forms.Padding(4);
            this.fastObjectListViewLogs.Name = "fastObjectListViewLogs";
            this.fastObjectListViewLogs.ShowGroups = false;
            this.fastObjectListViewLogs.Size = new System.Drawing.Size(688, 434);
            this.fastObjectListViewLogs.TabIndex = 3;
            this.fastObjectListViewLogs.UseCellFormatEvents = true;
            this.fastObjectListViewLogs.UseCompatibleStateImageBehavior = false;
            this.fastObjectListViewLogs.UseFiltering = true;
            this.fastObjectListViewLogs.View = System.Windows.Forms.View.Details;
            this.fastObjectListViewLogs.VirtualMode = true;
            this.fastObjectListViewLogs.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.fastObjectListViewLogs_FormatRow);
            // 
            // olvColumnDate
            // 
            this.olvColumnDate.AspectName = "Date";
            this.olvColumnDate.Text = "Date";
            this.olvColumnDate.UseFiltering = false;
            // 
            // olvColumnStatus
            // 
            this.olvColumnStatus.AspectName = "LoggerType";
            this.olvColumnStatus.Text = "Type";
            // 
            // olvColumnMessage
            // 
            this.olvColumnMessage.AspectName = "Message";
            this.olvColumnMessage.Text = "Message";
            this.olvColumnMessage.UseFiltering = false;
            this.olvColumnMessage.Width = 168;
            // 
            // olvColumnException
            // 
            this.olvColumnException.AspectName = "ExceptionMessage";
            this.olvColumnException.FillsFreeSpace = true;
            this.olvColumnException.Text = "Exception";
            this.olvColumnException.UseFiltering = false;
            this.olvColumnException.Width = 97;
            // 
            // LogViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 434);
            this.Controls.Add(this.fastObjectListViewLogs);
            this.Name = "LogViewerForm";
            this.Text = "Log Viewer";
            this.Load += new System.EventHandler(this.LogViewerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListViewLogs)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.FastObjectListView fastObjectListViewLogs;
        private BrightIdeasSoftware.OLVColumn olvColumnDate;
        private BrightIdeasSoftware.OLVColumn olvColumnStatus;
        private BrightIdeasSoftware.OLVColumn olvColumnMessage;
        private BrightIdeasSoftware.OLVColumn olvColumnException;
    }
}