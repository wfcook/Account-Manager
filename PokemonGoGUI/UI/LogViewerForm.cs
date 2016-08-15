using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGoGUI.UI
{
    public partial class LogViewerForm : Form
    {
        private string _fileName;

        public LogViewerForm(string filename)
        {
            InitializeComponent();

            _fileName = filename;

            fastObjectListViewLogs.PrimarySortColumn = olvColumnDate;
            fastObjectListViewLogs.PrimarySortOrder = SortOrder.Descending;
        }

        private async void LogViewerForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            try
            {
                string data = await Task.Run(() => File.ReadAllText(_fileName));

                List<Log> logs = Serializer.FromJson<List<Log>>(data);

                fastObjectListViewLogs.SetObjects(logs);
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format("Failed to import log. Ex: {0}", ex.Message));
            }
        }
        private void fastObjectListViewLogs_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            Log log = e.Model as Log;

            if (log == null)
            {
                return;
            }

            e.Item.ForeColor = log.GetLogColor();
        }
    }
}
