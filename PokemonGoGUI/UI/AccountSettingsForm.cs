using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGoGUI.UI
{
    public partial class AccountSettingsForm : Form
    {
        private Manager _manager;

        public AccountSettingsForm(Manager manager)
        {
            InitializeComponent();

            _manager = manager;

            #region Catching

            olvColumnCatchId.AspectGetter = delegate(object x)
            {
                CatchSetting setting = (CatchSetting)x;

                return (int)setting.Id;
            };

            #endregion

            #region Evolving

            olvColumnEvolveId.AspectGetter = delegate(object x)
            {
                EvolveSetting setting = (EvolveSetting)x;

                return (int)setting.Id;
            };

            #endregion

            #region Transfer

            olvColumnTransferId.AspectGetter = delegate(object x)
            {
                TransferSetting setting = (TransferSetting)x;

                return (int)setting.Id;
            };

            #endregion

        }

        private void AccountSettingsForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            foreach (AccountState state in Enum.GetValues(typeof(AccountState)))
            {
                if(state == AccountState.Good)
                {
                    continue;
                }

                comboBoxMinAccountState.Items.Add(state);
            }

            UpdateDetails(_manager.UserSettings);

            UpdateListViews();

            comboBoxLocationPresets.DataSource = _manager.FarmLocations;
            comboBoxLocationPresets.DisplayMember = "Name";
        }

        private void UpdateListViews()
        {
            fastObjectListViewRecycling.SetObjects(_manager.UserSettings.ItemSettings);
            fastObjectListViewCatch.SetObjects(_manager.UserSettings.CatchSettings);
            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
            fastObjectListViewTransfer.SetObjects(_manager.UserSettings.TransferSettings);
        }

        private void UpdateDetails(Settings settings)
        {
            if (settings.AuthType == AuthType.Google)
            {
                radioButtonGoogle.Checked = true;
            }

            textBoxPtcPassword.Text = settings.PtcPassword;
            textBoxPtcUsername.Text = settings.PtcUsername;
            textBoxLat.Text = settings.DefaultLatitude.ToString();
            textBoxLong.Text = settings.DefaultLongitude.ToString();
            textBoxName.Text = settings.AccountName;
            textBoxMaxTravelDistance.Text = settings.MaxTravelDistance.ToString();
            textBoxWalkSpeed.Text = settings.WalkingSpeed.ToString();
            textBoxPokemonBeforeEvolve.Text = settings.MinPokemonBeforeEvolve.ToString();
            textBoxMaxLevel.Text = settings.MaxLevel.ToString();
            textBoxProxy.Text = settings.Proxy.ToString();
            checkBoxMimicWalking.Checked = settings.MimicWalking;
            checkBoxEncounterWhileWalking.Checked = settings.EncounterWhileWalking;
            checkBoxRecycle.Checked = settings.RecycleItems;
            checkBoxEvolve.Checked = settings.EvolvePokemon;
            checkBoxTransfers.Checked = settings.TransferPokemon;
            checkBoxUseLuckyEgg.Checked = settings.UseLuckyEgg;
            checkBoxIncubateEggs.Checked = settings.IncubateEggs;
            checkBoxCatchPokemon.Checked = settings.CatchPokemon;
            checkBoxSnipePokemon.Checked = settings.SnipePokemon;
            numericUpDownSnipeAfterStops.Value = settings.SnipeAfterPokestops;
            numericUpDownMinBallsToSnipe.Value = settings.MinBallsToSnipe;
            numericUpDownMaxPokemonPerSnipe.Value = settings.MaxPokemonPerSnipe;
            numericUpDownSnipeAfterLevel.Value = settings.SnipeAfterLevel;
            numericUpDownRunForHours.Value = new Decimal(settings.RunForHours);
            numericUpDownMaxLogs.Value = settings.MaxLogs;

            //Humanization
            checkBoxHumanizeThrows.Checked = settings.EnableHumanization;
            numericUpDownInsideReticuleChance.Value = settings.InsideReticuleChance;

            numericUpDownGeneralDelay.Value = settings.GeneralDelay;
            numericUpDownGeneralDelayRandom.Value = settings.GeneralDelayRandom;

            numericUpDownDelayBetweenSnipes.Value = settings.DelayBetweenSnipes;
            numericUpDownDelayBetweenSnipeRandom.Value = settings.BetweenSnipesDelayRandom;

            numericUpDownLocationUpdateDelay.Value = settings.DelayBetweenLocationUpdates;
            numericUpDownLocationUpdateRandom.Value = settings.LocationupdateDelayRandom;

            numericUpDownPlayerActionDelay.Value = settings.DelayBetweenPlayerActions;
            numericUpDownPlayerActionRandomiz.Value = settings.PlayerActionDelayRandom;
            //End humanization

            //Device settings
            textBoxDeviceId.Text = settings.DeviceId;
            textBoxDeviceModel.Text = settings.DeviceModel;
            textBoxDeviceBrand.Text = settings.DeviceBrand;
            textBoxDeviceModelBoot.Text = settings.DeviceModelBoot;
            textBoxDeviceModelIdentifier.Text = settings.DeviceModelIdentifier;
            textBoxFirmwareBrand.Text = settings.FirmwareBrand;
            textBoxFirmwareFingerprint.Text = settings.FirmwareFingerprint;
            textBoxFirmwareTags.Text = settings.FirmwareTags;
            textBoxFirmwareType.Text = settings.FirmwareType;
            textBoxAnroidBoardName.Text = settings.AndroidBoardName;
            textBoxAndroidBootLoader.Text = settings.AndroidBootloader;
            textBoxHardwareManufacturer.Text = settings.HardwareManufacturer;
            textBoxHardwareModel.Text = settings.HardwareModel;
            //End device settings

            for(int i = 0; i < comboBoxMinAccountState.Items.Count; i++)
            {
                if((AccountState)comboBoxMinAccountState.Items[i] == settings.StopAtMinAccountState)
                {
                    comboBoxMinAccountState.SelectedIndex = i;
                    break;
                }
            }
        }

        private void radioButtonPtc_CheckedChanged_1(object sender, EventArgs e)
        {
            labelUsername.Text = "Username*:";

            textBoxPtcPassword.Enabled = true;
            textBoxPtcUsername.Enabled = true;

            if(radioButtonGoogle.Checked)
            {
                labelUsername.Text = "Email*:";
            }
        }

        private void checkBoxMimicWalking_CheckedChanged(object sender, EventArgs e)
        {
            textBoxWalkSpeed.Enabled = false; 
            checkBoxEncounterWhileWalking.Enabled = false;

            if(checkBoxMimicWalking.Checked)
            {
                textBoxWalkSpeed.Enabled = true;
                checkBoxEncounterWhileWalking.Enabled = true;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
            {
                MessageBox.Show("Settings saved.\nSome settings won't take effect until the account stops running.");
            }
        }

        private bool SaveSettings()
        {
            Settings userSettings = _manager.UserSettings;

            double defaultLat = 0;
            double defaultLong = 0;
            int walkingSpeed = 0;
            int maxTravelDistance = 0;
            int minPokemonBeforeEvolve = 0;
            int maxLevel = 0;
            ProxyEx proxyEx = null;

            if (!Int32.TryParse(textBoxMaxLevel.Text, out maxLevel) || maxLevel < 0)
            {
                MessageBox.Show("Invalid Max level");
                return false;
            }

            if (!Int32.TryParse(textBoxPokemonBeforeEvolve.Text, out minPokemonBeforeEvolve) || minPokemonBeforeEvolve < 0)
            {
                MessageBox.Show("Invalid pokemon before evolve");
                return false;
            }

            if (!Int32.TryParse(textBoxWalkSpeed.Text, out walkingSpeed) || walkingSpeed <= 0)
            {
                MessageBox.Show("Invalid walking speed");
                return false;
            }

            if (!Int32.TryParse(textBoxMaxTravelDistance.Text, out maxTravelDistance) || maxTravelDistance <= 0)
            {
                MessageBox.Show("Invalid max travel distance");
                return false;
            }

            if (!Double.TryParse(textBoxLat.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out defaultLat))
            {
                MessageBox.Show("Invalid latitude");
                return false;
            }

            if (!Double.TryParse(textBoxLong.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out defaultLong))
            {
                MessageBox.Show("Invalid longitude");
                return false;
            }

            if (!String.IsNullOrEmpty(textBoxProxy.Text) && !ProxyEx.TryParse(textBoxProxy.Text, out proxyEx))
            {
                MessageBox.Show("Invalid proxy format");
                return false;
            }

            if (comboBoxMinAccountState.SelectedItem == null)
            {
                MessageBox.Show("Please select a valid min account state");
                return false;
            }

            if (radioButtonPtc.Checked)
            {
                userSettings.AuthType = AuthType.Ptc;
            }
            else
            {
                userSettings.AuthType = AuthType.Google;
            }

            userSettings.MimicWalking = checkBoxMimicWalking.Checked;
            userSettings.PtcUsername = textBoxPtcUsername.Text;
            userSettings.PtcPassword = textBoxPtcPassword.Text;
            userSettings.DefaultLatitude = defaultLat;
            userSettings.DefaultLongitude = defaultLong;
            userSettings.WalkingSpeed = walkingSpeed;
            userSettings.MaxTravelDistance = maxTravelDistance;
            userSettings.EncounterWhileWalking = checkBoxEncounterWhileWalking.Checked;
            userSettings.AccountName = textBoxName.Text;
            userSettings.TransferPokemon = checkBoxTransfers.Checked;
            userSettings.EvolvePokemon = checkBoxEvolve.Checked;
            userSettings.RecycleItems = checkBoxRecycle.Checked;
            userSettings.MinPokemonBeforeEvolve = minPokemonBeforeEvolve;
            userSettings.UseLuckyEgg = checkBoxUseLuckyEgg.Checked;
            userSettings.IncubateEggs = checkBoxIncubateEggs.Checked;
            userSettings.MaxLevel = maxLevel;
            userSettings.CatchPokemon = checkBoxCatchPokemon.Checked;
            userSettings.SnipePokemon = checkBoxSnipePokemon.Checked;
            userSettings.SnipeAfterPokestops = (int)numericUpDownSnipeAfterStops.Value;
            userSettings.MinBallsToSnipe = (int)numericUpDownMinBallsToSnipe.Value;
            userSettings.MaxPokemonPerSnipe = (int)numericUpDownMaxPokemonPerSnipe.Value;
            userSettings.SnipeAfterLevel = (int)numericUpDownSnipeAfterLevel.Value;
            userSettings.StopAtMinAccountState = (AccountState)comboBoxMinAccountState.SelectedItem;

            userSettings.RunForHours = (double)numericUpDownRunForHours.Value;
            userSettings.MaxLogs = (int)numericUpDownMaxLogs.Value;

            //Humanization
            userSettings.EnableHumanization = checkBoxHumanizeThrows.Checked;
            userSettings.InsideReticuleChance = (int)numericUpDownInsideReticuleChance.Value;

            userSettings.GeneralDelay = (int)numericUpDownGeneralDelay.Value;
            userSettings.GeneralDelayRandom = (int)numericUpDownGeneralDelayRandom.Value;

            userSettings.DelayBetweenSnipes = (int)numericUpDownDelayBetweenSnipes.Value;
            userSettings.BetweenSnipesDelayRandom = (int)numericUpDownDelayBetweenSnipeRandom.Value;

            userSettings.DelayBetweenLocationUpdates = (int)numericUpDownLocationUpdateDelay.Value;
            userSettings.LocationupdateDelayRandom = (int)numericUpDownLocationUpdateRandom.Value;

            userSettings.DelayBetweenPlayerActions = (int)numericUpDownPlayerActionDelay.Value;
            userSettings.PlayerActionDelayRandom = (int)numericUpDownPlayerActionRandomiz.Value;

            //End humanization

            //Device settings
            userSettings.DeviceId = textBoxDeviceId.Text;
            userSettings.DeviceModel = textBoxDeviceModel.Text;
            userSettings.DeviceBrand = textBoxDeviceBrand.Text;
            userSettings.DeviceModelBoot = textBoxDeviceModelBoot.Text;
            userSettings.DeviceModelIdentifier = textBoxDeviceModelIdentifier.Text;
            userSettings.FirmwareBrand = textBoxFirmwareBrand.Text;
            userSettings.FirmwareFingerprint = textBoxFirmwareFingerprint.Text;
            userSettings.FirmwareTags = textBoxFirmwareTags.Text;
            userSettings.FirmwareType = textBoxFirmwareType.Text;
            userSettings.AndroidBoardName = textBoxAnroidBoardName.Text;
            userSettings.AndroidBootloader = textBoxAndroidBootLoader.Text;
            userSettings.HardwareManufacturer = textBoxHardwareManufacturer.Text;
            userSettings.HardwareModel = textBoxHardwareModel.Text;
            //End device settings

            if (proxyEx != null)
            {
                userSettings.ProxyIP = proxyEx.Address;
                userSettings.ProxyPort = proxyEx.Port;
                userSettings.ProxyUsername = proxyEx.Username;
                userSettings.ProxyPassword = proxyEx.Password;
            }
            else
            {
                userSettings.ProxyUsername = null;
                userSettings.ProxyPassword = null;
                userSettings.ProxyIP = null;
                userSettings.ProxyPort = 0;
            }

            return true;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
            {
                DialogResult = DialogResult.OK;
            }
        }

        #region Recycling

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int totalObjects = fastObjectListViewRecycling.SelectedObjects.Count;

            if(totalObjects == 0)
            {
                return;
            }

            InventoryItemSetting iiSettings = fastObjectListViewRecycling.SelectedObjects[0] as InventoryItemSetting;

            if(iiSettings == null)
            {
                return;
            }

            string num = Prompt.ShowDialog("Max Inventory Amount", "Edit Amount", iiSettings.MaxInventory.ToString());

            if(String.IsNullOrEmpty(num))
            {
                return;
            }

            int maxInventory = 0;

            if(!Int32.TryParse(num, out maxInventory))
            {
                return;
            }

            foreach(InventoryItemSetting item in fastObjectListViewRecycling.SelectedObjects)
            {
                item.MaxInventory = maxInventory;
            }

            fastObjectListViewRecycling.SetObjects(_manager.UserSettings.ItemSettings);
        }

        #endregion

        #region CatchPokemon


        private void falseToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tSMI = sender as ToolStripMenuItem;

            if (tSMI == null)
            {
                return;
            }

            CheckType checkType = (CheckType)Int32.Parse(tSMI.Tag.ToString());

            foreach (CatchSetting cSetting in fastObjectListViewCatch.SelectedObjects)
            {
                if (checkType == CheckType.Toggle)
                {
                    cSetting.Snipe = !cSetting.Snipe;
                }
                else if (checkType == CheckType.True)
                {
                    cSetting.Snipe = true;
                }
                else
                {
                    cSetting.Snipe = false;
                }
            }

            fastObjectListViewCatch.RefreshSelectedObjects();
        }


        private void trueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tSMI = sender as ToolStripMenuItem;
 
            if(tSMI == null)
            {
                return;
            }

            CheckType checkType = (CheckType)Int32.Parse(tSMI.Tag.ToString());

            foreach(CatchSetting cSetting in fastObjectListViewCatch.SelectedObjects)
            {
                if(checkType == CheckType.Toggle)
                {
                    cSetting.Catch = !cSetting.Catch;
                }
                else if (checkType == CheckType.True)
                {
                    cSetting.Catch = true;
                }
                else
                {
                    cSetting.Catch = false;
                }
            }

            fastObjectListViewCatch.RefreshSelectedObjects();
        }

        private void restoreDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Reset defaults?", "Confirmation", MessageBoxButtons.YesNoCancel);

            if(result != DialogResult.Yes)
            {
                return;
            }

            _manager.RestoreCatchDefaults();

            fastObjectListViewCatch.SetObjects(_manager.UserSettings.CatchSettings);
        }

        #endregion

        #region Evolving

        private void restoreDefaultsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Reset defaults?", "Confirmation", MessageBoxButtons.YesNoCancel);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _manager.RestoreEvolveDefaults();

            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
        }

        private void editCPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = fastObjectListViewEvolve.SelectedObjects.Count;

            if(count == 0)
            {
                return;
            }

            int defaultCP = ((EvolveSetting)fastObjectListViewEvolve.SelectedObjects[0]).MinCP;

            string cp = Prompt.ShowDialog("Enter minimum CP:", "Edit CP", defaultCP.ToString());

            if(String.IsNullOrEmpty(cp))
            {
                return;
            }

            int changeCp = 0;

            if(!Int32.TryParse(cp, out changeCp) || changeCp < 0)
            {
                MessageBox.Show("Invalid amount");

                return;
            }

            foreach(EvolveSetting setting in fastObjectListViewEvolve.SelectedObjects)
            {
                setting.MinCP = changeCp;
            }

            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
        }

        private void trueToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tSMI = sender as ToolStripMenuItem;

            if (tSMI == null)
            {
                return;
            }

            CheckType checkType = (CheckType)Int32.Parse(tSMI.Tag.ToString());

            foreach (EvolveSetting eSetting in fastObjectListViewEvolve.SelectedObjects)
            {
                if (checkType == CheckType.Toggle)
                {
                    eSetting.Evolve = !eSetting.Evolve;
                }
                else if (checkType == CheckType.True)
                {
                    eSetting.Evolve = true;
                }
                else
                {
                    eSetting.Evolve = false;
                }
            }

            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
        }

        #endregion

        #region Transfer

        private void EditTransferSettings(List<TransferSetting> settings)
        {
            if(settings.Count == 0)
            {
                return;
            }

            TransferSettingsForm transferSettingForm = new TransferSettingsForm(settings);
            transferSettingForm.ShowDialog();

            fastObjectListViewTransfer.RefreshObjects(settings);
        }

        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            EditTransferSettings(fastObjectListViewTransfer.SelectedObjects.Cast<TransferSetting>().ToList());
        }

        private void restoreDefaultsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Reset defaults?", "Confirmation", MessageBoxButtons.YesNoCancel);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _manager.RestoreTransferDefaults();

            fastObjectListViewTransfer.SetObjects(_manager.UserSettings.TransferSettings);
        }

        #endregion

        private void comboBoxLocationPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            FarmLocation fLocation = comboBoxLocationPresets.SelectedItem as FarmLocation;

            if(fLocation != null)
            {
                if (fLocation.Name == "Current")
                {
                    textBoxLat.Text = _manager.UserSettings.DefaultLatitude.ToString();
                    textBoxLong.Text = _manager.UserSettings.DefaultLongitude.ToString();
                }
                else
                {
                    textBoxLat.Text = fLocation.Latitude.ToString();
                    textBoxLong.Text = fLocation.Longitude.ToString();
                }
            }
        }

        private async void buttonExportConfig_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                MethodResult result = await _manager.ExportConfig(sfd.FileName);

                if (result.Success)
                {
                    MessageBox.Show("Config exported");
                }
            }
        }

        private async void buttonImportConfig_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Open config file";
                ofd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                MethodResult result = await _manager.ImportConfigFromFile(ofd.FileName);

                if(!result.Success)
                {
                    return;
                }

                UpdateDetails(_manager.UserSettings);
                UpdateListViews();
            }
        }

        private void checkBoxSnipePokemon_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownSnipeAfterStops.Enabled = checkBoxSnipePokemon.Checked;
            numericUpDownMinBallsToSnipe.Enabled = checkBoxSnipePokemon.Checked;
            numericUpDownMaxPokemonPerSnipe.Enabled = checkBoxSnipePokemon.Checked;
            numericUpDownSnipeAfterLevel.Enabled = checkBoxSnipePokemon.Checked;
        }

        private void buttonDeviceRandom_Click(object sender, EventArgs e)
        {
            _manager.RandomDeviceId();

            textBoxDeviceId.Text = _manager.UserSettings.DeviceId;

            //UpdateDetails(_manager.UserSettings);
        }

        private void buttonResetDefaults_Click(object sender, EventArgs e)
        {
            _manager.RestoreDeviceDefaults();

            UpdateDetails(_manager.UserSettings);
        }

    }
}
