using PokemonGoGUI.Enums;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PokemonGoGUI.UI
{
    public partial class TransferSettingsForm : Form
    {
        private List<CatchSetting> _settings;

        public TransferSettingsForm(List<CatchSetting> settings)
        {
            InitializeComponent();

            _settings = settings;
        }

        private void SetSettings()
        {
            foreach (CatchSetting setting in _settings)
            {
                setting.Transfer = checkBoxTransfer.Checked;
                setting.MinTransferCP = (int)numericUpDownMinCP.Value;
                setting.KeepMax = (int)numericUpDownKeepMax.Value;
                setting.TransferType = (TransferType)comboBoxTransferType.SelectedItem;
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
            CatchSetting setting = _settings.First();

            for(int i = 0; i < comboBoxTransferType.Items.Count; i++)
            {
                if (((TransferType)comboBoxTransferType.Items[i]) == setting.TransferType)
                {
                    comboBoxTransferType.SelectedIndex = i;

                    break;
                }
            }

            numericUpDownKeepMax.Value = setting.KeepMax;
            numericUpDownMinCP.Value = setting.MinTransferCP;
            numericUpDownIVPercent.Value = setting.IVPercent;
            checkBoxTransfer.Checked = setting.Transfer;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            SetSettings();

            DialogResult = DialogResult.OK;
        }

        private void ComboBoxTransferType_SelectedIndexChanged(object sender, EventArgs e)
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
