using PokemonGoGUI.Enums;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGoGUI.UI
{
    public partial class TransferSettingsForm : Form
    {
        private List<TransferSetting> _settings;

        public TransferSettingsForm(List<TransferSetting> settings)
        {
            InitializeComponent();

            _settings = settings;
        }

        private void SetSettings()
        {
            foreach (TransferSetting setting in _settings)
            {
                setting.Transfer = checkBoxTransfer.Checked;
                setting.MinCP = (int)numericUpDownMinCP.Value;
                setting.KeepMax = (int)numericUpDownKeepMax.Value;
                setting.Type = (TransferType)comboBoxTransferType.SelectedItem;
                setting.IVPercent = (int)numericUpDownIVPercent.Value;
            }
        }
        
        private void TransferSettingsForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            foreach (TransferType setting in Enum.GetValues(typeof(TransferType)))
            {
                comboBoxTransferType.Items.Add(setting);
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            TransferSetting setting = _settings.First();

            for(int i = 0; i < comboBoxTransferType.Items.Count; i++)
            {
                if (((TransferType)comboBoxTransferType.Items[i]) == setting.Type)
                {
                    comboBoxTransferType.SelectedIndex = i;

                    break;
                }
            }

            numericUpDownKeepMax.Value = setting.KeepMax;
            numericUpDownMinCP.Value = setting.MinCP;
            numericUpDownIVPercent.Value = setting.IVPercent;
            checkBoxTransfer.Checked = setting.Transfer;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SetSettings();

            DialogResult = DialogResult.OK;
        }

        private void comboBoxTransferType_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDownMinCP.Enabled = false;
            numericUpDownKeepMax.Enabled = false;
            numericUpDownIVPercent.Enabled = false;

            TransferType type = (TransferType)comboBoxTransferType.SelectedItem;

            switch (type)
            {
                case TransferType.KeepStrongestX:
                    numericUpDownKeepMax.Enabled = true;
                    break;
                case TransferType.KeepPossibleEvolves:
                    numericUpDownKeepMax.Enabled = true;
                    break;
                case TransferType.BelowCP:
                    numericUpDownMinCP.Enabled = true;
                    break;
                case TransferType.BelowIVPercentage:
                    numericUpDownIVPercent.Enabled = true;
                    break;
                case TransferType.BelowCPOrIVAmount:
                    numericUpDownIVPercent.Enabled = true;
                    numericUpDownMinCP.Enabled = true;
                    break;
                case TransferType.BelowCPAndIVAmount:
                    numericUpDownIVPercent.Enabled = true;
                    numericUpDownMinCP.Enabled = true;
                    break;
                case TransferType.KeepXHighestIV:
                    numericUpDownKeepMax.Enabled = true;
                    break;
            }
        }

    }
}
