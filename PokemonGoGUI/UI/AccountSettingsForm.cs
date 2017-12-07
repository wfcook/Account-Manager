using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
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

            //Api config load keys
            if (File.Exists("HashKeys.txt"))
            {
                string[] lineOfContents = File.ReadAllLines("HashKeys.txt");
                foreach (var line in lineOfContents)
                {
                    string[] tokens = line.Split(',');
                    cbAuthAPIKey.Items.Add(tokens[0]);
                }
            }
            else
                File.CreateText("HashKeys.txt");

            //Location time zones
            var zones = new TimeZoneIds().GetTimeZoneIds();
            foreach (var tz in zones)
            {
                cbTimeZones.Items.Add(tz.Key);
            }

            cbTimeZones.Text = _manager.UserSettings.TimeZone;
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
            numericUpDownRunForHours.Value = new Decimal(settings.RunForHours);
            numericUpDownMaxLogs.Value = settings.MaxLogs;
            numericUpDownMaxFailBeforeReset.Value = settings.MaxFailBeforeReset;
            checkBoxStopOnIPBan.Checked = settings.StopOnIPBan;
            checkBoxAutoRotateProxies.Checked = settings.AutoRotateProxies;
            checkBoxRemoveOnStop.Checked = settings.AutoRemoveOnStop;
            checkBoxClaimLevelUp.Checked = settings.ClaimLevelUpRewards;
            numericUpDownSearchFortBelow.Value = new Decimal(settings.SearchFortBelowPercent);
            numericUpDownForceEvolveAbove.Value = new Decimal(settings.ForceEvolveAbovePercent);
            checkBoxStopOnAPIUpdate.Checked = settings.StopOnAPIUpdate;

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

            numericUpDownWalkingOffset.Value = new Decimal(settings.WalkingSpeedOffset);
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

            //Api config
            cbHashHost.Text = settings.HashHost.ToString();
            cbHashEndpoint.Text = settings.HashEndpoint;
            cbAuthAPIKey.Text = settings.AuthAPIKey;

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

        private void RadioButtonPtc_CheckedChanged_1(object sender, EventArgs e)
        {
            labelUsername.Text = "Username*:";

            textBoxPtcPassword.Enabled = true;
            textBoxPtcUsername.Enabled = true;

            if(radioButtonGoogle.Checked)
            {
                labelUsername.Text = "Email*:";
            }
        }

        private void CheckBoxMimicWalking_CheckedChanged(object sender, EventArgs e)
        {
            textBoxWalkSpeed.Enabled = false; 
            checkBoxEncounterWhileWalking.Enabled = false;

            if(checkBoxMimicWalking.Checked)
            {
                textBoxWalkSpeed.Enabled = true;
                checkBoxEncounterWhileWalking.Enabled = true;
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
            {
                MessageBox.Show("Settings saved.\nSome settings won't take effect until the account stops running.", "Info");
            }
        }

        private bool SaveSettings()
        {
            Settings userSettings = _manager.UserSettings;

            ProxyEx proxyEx = null;

            if (!Int32.TryParse(textBoxMaxLevel.Text, out int maxLevel) || maxLevel < 0)
            {
                MessageBox.Show("Invalid Max level", "Warning");
                return false;
            }

            if (!Int32.TryParse(textBoxPokemonBeforeEvolve.Text, out int minPokemonBeforeEvolve) || minPokemonBeforeEvolve < 0)
            {
                MessageBox.Show("Invalid pokemon before evolve", "Warning");
                return false;
            }

            if (!Int32.TryParse(textBoxWalkSpeed.Text, out int walkingSpeed) || walkingSpeed <= 0)
            {
                MessageBox.Show("Invalid walking speed", "Warning");
                return false;
            }

            if (!Int32.TryParse(textBoxMaxTravelDistance.Text, out int maxTravelDistance) || maxTravelDistance <= 0)
            {
                MessageBox.Show("Invalid max travel distance", "Warning");
                return false;
            }

            if (!Double.TryParse(textBoxLat.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double defaultLat))
            {
                MessageBox.Show("Invalid latitude", "Warning");
                return false;
            }

            if (!Double.TryParse(textBoxLong.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double defaultLong))
            {
                MessageBox.Show("Invalid longitude", "Warning");
                return false;
            }

            if (!String.IsNullOrEmpty(textBoxProxy.Text) && !ProxyEx.TryParse(textBoxProxy.Text, out proxyEx))
            {
                MessageBox.Show("Invalid proxy format", "Warning");
                return false;
            }

            if (comboBoxMinAccountState.SelectedItem == null)
            {
                MessageBox.Show("Please select a valid min account state", "Warning");
                return false;
            }

            if (walkingSpeed < (double)numericUpDownWalkingOffset.Value)
            {
                MessageBox.Show("Walking offset must be more than walking speed", "Warning");
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
            userSettings.PtcUsername = textBoxPtcUsername.Text.Trim();
            userSettings.PtcPassword = textBoxPtcPassword.Text.Trim();
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
            userSettings.StopAtMinAccountState = (AccountState)comboBoxMinAccountState.SelectedItem;
            userSettings.SearchFortBelowPercent = (double)numericUpDownSearchFortBelow.Value;
            userSettings.ForceEvolveAbovePercent = (double) numericUpDownForceEvolveAbove.Value;
            userSettings.ClaimLevelUpRewards = checkBoxClaimLevelUp.Checked;
            userSettings.StopOnAPIUpdate = checkBoxStopOnAPIUpdate.Checked;

            userSettings.RunForHours = (double)numericUpDownRunForHours.Value;
            userSettings.MaxLogs = (int)numericUpDownMaxLogs.Value;
            userSettings.StopOnIPBan = checkBoxStopOnIPBan.Checked;
            userSettings.MaxFailBeforeReset = (int)numericUpDownMaxFailBeforeReset.Value;
            userSettings.AutoRotateProxies = checkBoxAutoRotateProxies.Checked;
            userSettings.AutoRemoveOnStop = checkBoxRemoveOnStop.Checked;

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

            userSettings.WalkingSpeedOffset = (double)numericUpDownWalkingOffset.Value;
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

            //Api config
            userSettings.HashHost = new Uri(cbHashHost.Text);
            userSettings.HashEndpoint = cbHashEndpoint.Text;
            userSettings.AuthAPIKey = cbAuthAPIKey.Text;
            //End api config

            //Location time zones
            var x = new TimeZoneIds().GetTimeZoneIds();
            userSettings.TimeZone = cbTimeZones.Text;
            userSettings.Country = x[cbTimeZones.Text].Item1;
            userSettings.Language = x[cbTimeZones.Text].Item2;
            userSettings.POSIX = x[cbTimeZones.Text].Item3;
            //End location time zones

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

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
            {
                DialogResult = DialogResult.OK;
            }
        }

        #region Recycling

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
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

            if (!Int32.TryParse(num, out int maxInventory))
            {
                return;
            }

            foreach (InventoryItemSetting item in fastObjectListViewRecycling.SelectedObjects)
            {
                item.MaxInventory = maxInventory;
            }

            fastObjectListViewRecycling.SetObjects(_manager.UserSettings.ItemSettings);
        }

        #endregion

        #region CatchPokemon


        private void FalseToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tSMI = sender as ToolStripMenuItem;

            if (tSMI == null)
            {
                return;
            }

            fastObjectListViewCatch.RefreshSelectedObjects();
        }


        private void TrueToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void RestoreDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void RestoreDefaultsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Reset defaults?", "Confirmation", MessageBoxButtons.YesNoCancel);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _manager.RestoreEvolveDefaults();

            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
        }

        private void EditCPToolStripMenuItem_Click(object sender, EventArgs e)
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

            if (!Int32.TryParse(cp, out int changeCp) || changeCp < 0)
            {
                MessageBox.Show("Invalid amount", "Warning");

                return;
            }

            foreach (EvolveSetting setting in fastObjectListViewEvolve.SelectedObjects)
            {
                setting.MinCP = changeCp;
            }

            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
        }

        private void TrueToolStripMenuItem1_Click(object sender, EventArgs e)
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

        private void EditToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            EditTransferSettings(fastObjectListViewTransfer.SelectedObjects.Cast<TransferSetting>().ToList());
        }

        private void RestoreDefaultsToolStripMenuItem2_Click(object sender, EventArgs e)
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

        private void ComboBoxLocationPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLocationPresets.SelectedItem is FarmLocation fLocation)
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

        private async void ButtonExportConfig_Click(object sender, EventArgs e)
        {
            if(!SaveSettings())
            {
                return;
            }

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
                    MessageBox.Show("Config exported", "Info");
                }
            }
        }

        private async void ButtonImportConfig_Click(object sender, EventArgs e)
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

        private void ButtonDeviceRandom_Click(object sender, EventArgs e)
        {
            _manager.RandomDeviceId();

            textBoxDeviceId.Text = _manager.UserSettings.DeviceId;

            //UpdateDetails(_manager.UserSettings);
        }

        private void ButtonResetDefaults_Click(object sender, EventArgs e)
        {
            _manager.RestoreDeviceDefaults();

            UpdateDetails(_manager.UserSettings);
        }

        private void CheckBoxAutoRotateProxies_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxRemoveOnStop.Enabled = checkBoxAutoRotateProxies.Checked;
        }
    }
}
