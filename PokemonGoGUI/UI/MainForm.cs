using Newtonsoft.Json;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Helpers;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using PokemonGoGUI.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGoGUI
{
    public partial class MainForm : Form
    {
        List<Manager> _managers = new List<Manager>();

        private readonly string _saveFile = "data";
        private const string _versionNumber = "1.2.2";

        public MainForm()
        {
            InitializeComponent();

            fastObjectListViewMain.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewMain.ForeColor = Color.LightGray;
            //fastObjectListViewMain.AlwaysGroupByColumn = olvColumnGroup;

            Text = "GoManager - v" + _versionNumber;

        }

        private void ShowDetails(IEnumerable<Manager> managers)
        {
            foreach(Manager manager in managers)
            {
                DetailsForm dForm = new DetailsForm(manager);
                dForm.Show();
            }
        }

        private void fastObjectListViewMain_DoubleClick(object sender, EventArgs e)
        {
            ShowDetails(fastObjectListViewMain.SelectedObjects.Cast<Manager>());
        }

        private void RefreshManager(Manager manager)
        {
            fastObjectListViewMain.RefreshObject(manager);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            await LoadSettings();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_managers.Any(x => x.IsRunning))
            {
                MessageBox.Show("Please stop bots before closing");

                e.Cancel = true;
            }

            SaveSettings();
        }

        private async Task LoadSettings()
        {
            string jsonFile = _saveFile + ".json";
            string bsonFile = _saveFile + ".bson";

            try
            {
                bool jsonFileExists = File.Exists(jsonFile);
                bool bsonFileExists = File.Exists(bsonFile);

                if(!jsonFileExists && !bsonFileExists)
                {
                    return;
                }

                List<Manager> tempManagers = new List<Manager>();

                if (bsonFileExists)
                {
                    byte[] data = await Task.Run(() => File.ReadAllBytes(bsonFile));
                    tempManagers = Serializer.FromBson<List<Manager>>(data);
                }
                else
                {
                    string data = await Task.Run(() => File.ReadAllText(jsonFile));
                    tempManagers = Serializer.FromJson<List<Manager>>(data);
                }

                if(tempManagers == null)
                {
                    MessageBox.Show("Failed to load settings");
                    return;
                }

                foreach(Manager manager in tempManagers)
                {
                    manager.OnLog += manager_OnLog;
                    manager.OnInventoryUpdate += manager_OnInventoryUpdate;

                    //Patch for version upgrade
                    if(String.IsNullOrEmpty(manager.UserSettings.DeviceId))
                    {
                        //Load some
                        manager.UserSettings.LoadDeviceSettings();
                    }

                    _managers.Add(manager);
                }
            }
            catch
            {
                MessageBox.Show("Failed to load settings");
                //Failed to load settings
            }

            fastObjectListViewMain.SetObjects(_managers);
        }

        private void SaveSettings()
        {
            try
            {
                string data = Serializer.ToJson(_managers);

                File.WriteAllText(_saveFile + ".json", data);
            }
            catch
            {
                //Failed to save
            }
        }

        private void manager_OnInventoryUpdate(object sender, EventArgs e)
        {
            Manager manager = sender as Manager;

            if(manager == null)
            {
                return;
            }

            //RefreshManager(manager);
        }

        private void manager_OnLog(object sender, LoggerEventArgs e)
        {
            Manager manager = sender as Manager;

            if (manager == null)
            {
                return;
            }

            //RefreshManager(manager);
        }

        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Manager manager = new Manager();

            AccountSettingsForm asForm = new AccountSettingsForm(manager);
            
            if(asForm.ShowDialog() == DialogResult.OK)
            {
                AddManager(manager);
            }

            fastObjectListViewMain.SetObjects(_managers);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int totalAccounts = fastObjectListViewMain.SelectedObjects.Count;

            if(totalAccounts == 0)
            {
                return;
            }

            DialogResult dResult = MessageBox.Show(String.Format("Delete {0} accounts?", totalAccounts), "Are you sure?", MessageBoxButtons.YesNoCancel);

            if(dResult != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            bool messageShown = false;

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                if(manager.IsRunning && !messageShown)
                {
                    messageShown = true;

                    MessageBox.Show("Only accounts that are not running will be deleted");
                }

                if (!manager.IsRunning)
                {
                    manager.OnLog -= manager_OnLog;
                    manager.OnInventoryUpdate -= manager_OnInventoryUpdate;

                    _managers.Remove(manager);
                }
            }

            fastObjectListViewMain.SetObjects(_managers);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = fastObjectListViewMain.SelectedObjects.Count;
            if(count > 1)
            {
                DialogResult result = MessageBox.Show(String.Format("Are you sure you want to open {0} edit forms?", count), "Confirmation", MessageBoxButtons.YesNo);

                if(result != DialogResult.Yes)
                {
                    return;
                }
            }

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                AccountSettingsForm asForm = new AccountSettingsForm(manager);
                asForm.ShowDialog();
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void fastObjectListViewMain_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Control && e.Alt && e.KeyCode == Keys.U)
            {
                DialogResult result = MessageBox.Show("Show developer tools?", "Confirmation", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    devToolsToolStripMenuItem.Visible = true;
                }
            }

            if(e.KeyCode != Keys.Enter)
            {
                return;
            }

            ShowDetails(fastObjectListViewMain.SelectedObjects.Cast<Manager>());

            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void viewDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowDetails(fastObjectListViewMain.SelectedObjects.Cast<Manager>());
        }

        private async void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = false;

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.Start();

                await Task.Delay(200);
            }

            startToolStripMenuItem.Enabled = true;

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.Stop();
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private List<string> ImportAccounts()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Open account file";
                ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return File.ReadAllLines(ofd.FileName).ToList();
                }
            }

            return new List<string>();
        }

        private string ImportConfig()
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Open config file";
                ofd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName;
                }
            }

            return String.Empty;
        }

        private void AddManager(Manager manager)
        {
            manager.OnLog += manager_OnLog;
            manager.OnInventoryUpdate += manager_OnInventoryUpdate;

            _managers.Add(manager);
        }

        private string GetSaveFileName()
        {
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Save File";
                sfd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    return sfd.FileName;
                }

                return String.Empty;
            }
        }

        private async void wConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tSMI = sender as ToolStripMenuItem;
            bool useConfig = false;

            if(tSMI == null || !Boolean.TryParse(tSMI.Tag.ToString(), out useConfig))
            {
                return;
            }

            try
            {
                List<string> accounts = ImportAccounts();
                string configFile = String.Empty;

                if (useConfig)
                {
                    configFile = ImportConfig();
                }

                HashSet<Manager> tempManagers = new HashSet<Manager>(_managers);

                if(useConfig && String.IsNullOrEmpty(configFile))
                {
                    return;
                }

                int totalSuccess = 0;
                int total = accounts.Count;

                foreach(string account in accounts)
                {
                    string[] parts = account.Split(':');

                    /*
                     * User:Pass = 2
                     * User:Pass:MaxLevel = 3
                     * User:Pass:IP:Port = 4
                     * User:Pass:IP:Port:MaxLevel = 5
                     * User:Pass:IP:Port:pUsername:pPassword = 6
                     * User:Pass:IP:Port:pUsername:pPassword:MaxLevel = 7
                     */
                    if (parts.Length < 2 || parts.Length > 7)
                    {
                        continue;
                    }

                    AccountImport importModel = new AccountImport();

                    if(!importModel.ParseAccount(account))
                    {
                        continue;
                    }

                    Manager manager = new Manager();

                    if (useConfig)
                    {
                        MethodResult result = await manager.ImportConfigFromFile(configFile);

                        if (!result.Success)
                        {
                            MessageBox.Show("Failed to import configuration file");

                            return;
                        }
                    }

                    manager.UserSettings.AccountName = importModel.Username;
                    manager.UserSettings.PtcUsername = importModel.Username;
                    manager.UserSettings.PtcPassword = importModel.Password;
                    manager.UserSettings.ProxyIP = importModel.Address;
                    manager.UserSettings.ProxyPort = importModel.Port;
                    manager.UserSettings.ProxyUsername = importModel.ProxyUsername;
                    manager.UserSettings.ProxyPassword = importModel.ProxyPassword;

                    if (parts.Length % 2 == 1)
                    {
                        manager.UserSettings.MaxLevel = importModel.MaxLevel;
                    }

                    if (tempManagers.Add(manager))
                    {
                        AddManager(manager);
                        ++totalSuccess;
                    }
                }

                fastObjectListViewMain.SetObjects(_managers);

                MessageBox.Show(String.Format("Successfully imported {0} out of {1} accounts", totalSuccess, total));
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format("Failed to import usernames. Ex: {0}", ex.Message));
            }
        }

        private void timerListViewUpdate_Tick(object sender, EventArgs e)
        {
            if(_managers.Count == 0)
            {
                return;
            }

            fastObjectListViewMain.RefreshObject(_managers[0]);
        }

        private void clearProxiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int totalAccounts = fastObjectListViewMain.SelectedObjects.Count;

            DialogResult result = MessageBox.Show(String.Format("Clear proxies from {0} accounts?", totalAccounts), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.ProxyIP = null;
                manager.UserSettings.ProxyPort = 0;
                manager.UserSettings.ProxyUsername = null;
                manager.UserSettings.ProxyPassword = null;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void enableColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableColorsToolStripMenuItem.Checked = !enableColorsToolStripMenuItem.Checked;
            bool isChecked = enableColorsToolStripMenuItem.Checked;

            if(isChecked)
            {
                fastObjectListViewMain.BackColor = Color.FromArgb(43, 43, 43);
                fastObjectListViewMain.ForeColor = Color.LightGray;

                fastObjectListViewMain.UseCellFormatEvents = true;
            }
            else
            {
                fastObjectListViewMain.BackColor = SystemColors.Window;
                fastObjectListViewMain.ForeColor = SystemColors.WindowText;

                fastObjectListViewMain.UseCellFormatEvents = false;
            }
        }


        private void showGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showGroupsToolStripMenuItem.Checked = !showGroupsToolStripMenuItem.Checked;
        }

        private void fastObjectListViewMain_FormatCell(object sender, BrightIdeasSoftware.FormatCellEventArgs e)
        {
            Manager manager = (Manager)e.Model;

            if(e.Column == olvColumnAccountState)
            {
                switch(manager.AccountState)
                {
                    case AccountState.AccountBan:
                        e.SubItem.ForeColor = Color.Red;
                        break;
                    case AccountState.PokemonBanTemp:
                        e.SubItem.ForeColor = Color.Yellow;
                        break;
                    case AccountState.PokestopBanTemp:
                        e.SubItem.ForeColor = Color.Yellow;
                        break;
                    case AccountState.PokemonBanAndPokestopBanTemp:
                        e.SubItem.ForeColor = Color.Yellow;
                        break;
                    case AccountState.Good:
                        e.SubItem.ForeColor = Color.Green;
                        break;
                }
            }
            else if(e.Column == olvColumnBotState)
            {
                switch(manager.State)
                {
                    case BotState.Running:
                        e.SubItem.ForeColor = Color.Green;
                        break;
                    case BotState.Starting:
                        e.SubItem.ForeColor = Color.LightGreen;
                        break;
                    case BotState.Stopping:
                        e.SubItem.ForeColor = Color.OrangeRed;
                        break;
                    case BotState.Stopped:
                        e.SubItem.ForeColor = Color.Red;
                        break;
                    case BotState.Paused:
                        e.SubItem.ForeColor = Color.MediumAquamarine;
                        break;
                    case BotState.Pausing:
                        e.SubItem.ForeColor = Color.MediumAquamarine;
                        break;
                }
            }
            else if (e.Column == olvColumnLastLogMessage)
            {
                Log log = manager.Logs.LastOrDefault();

                if(log == null)
                {
                    return;
                }

                e.SubItem.ForeColor = log.GetLogColor();
            }

        }

        private void garbageCollectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("This should not be called outside testing purposes. Continue?", "Confirmation", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                GC.Collect();
            }
        }

        private async void exportStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                await manager.ExportStats();
            }
        }

        private async void updateDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateDetailsToolStripMenuItem.Enabled = false;

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UpdateDetails();

                await Task.Delay(100);
            }

            updateDetailsToolStripMenuItem.Enabled = true;
        }

        private void importProxiesToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            int count = fastObjectListViewMain.SelectedObjects.Count;
            string fileName = String.Empty;
            int accountsPerProxy = 0;

            if(count == 0)
            {
                MessageBox.Show("Please select 1 or more accounts");
                return;
            }

            string pPerAccount = Prompt.ShowDialog("Accounts per proxy", "Accounts per proxy", "1");

            if(String.IsNullOrEmpty(pPerAccount))
            {
                return;
            }

            if (!Int32.TryParse(pPerAccount, out accountsPerProxy) || accountsPerProxy <= 0)
            {
                MessageBox.Show("Invalid input");

                return;
            }


            if (count == 0)
            {
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Open proxy file";
                ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fileName = ofd.FileName;
                }
            }

            if (String.IsNullOrEmpty(fileName))
            {
                return;
            }

            List<ProxyEx> proxies = new List<ProxyEx>();

            try
            {
                string[] tempProxies = File.ReadAllLines(fileName);
                ProxyEx tempProxyEx = null;

                foreach (string proxyEx in tempProxies)
                {
                    if (ProxyEx.TryParse(proxyEx, out tempProxyEx))
                    {
                        proxies.Add(tempProxyEx);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Failed to import proxy file. Ex: {0}", ex.Message));
                return;
            }

            if (proxies.Count == 0)
            {
                MessageBox.Show("No proxies found");
                return;
            }

            int proxyIndex = 0;
            int proxyUsage = 0;

            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                ++proxyUsage;

                if (proxyUsage > accountsPerProxy)
                {
                    ++proxyIndex;
                    proxyUsage = 1;

                    if (proxyIndex >= proxies.Count)
                    {
                        MessageBox.Show("Out of proxies");
                        return;
                    }
                }

                ProxyEx proxy = proxies[proxyIndex];

                manager.UserSettings.ProxyIP = proxy.Address;
                manager.UserSettings.ProxyPort = proxy.Port;
                manager.UserSettings.ProxyUsername = proxy.Username;
                manager.UserSettings.ProxyPassword = proxy.Password;
            }
        }

        private void exportAccountsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (fastObjectListViewMain.SelectedObjects.Count == 0)
            {
                return;
            }

            string filename = GetSaveFileName();

            if (String.IsNullOrEmpty(filename))
            {
                return;
            }

            try
            {
                IEnumerable<string> accounts = fastObjectListViewMain.SelectedObjects.Cast<Manager>().Select(x => String.Format("{0}:{1}", x.UserSettings.PtcUsername, x.UserSettings.PtcPassword));

                File.WriteAllLines(filename, accounts);

                MessageBox.Show(String.Format("Exported {0} accounts", accounts.Count()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Failed to export accounts. Ex: {0}", ex.Message));
            }
        }

        private void exportProxiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(fastObjectListViewMain.SelectedObjects.Count == 0)
            {
                return;
            }

            string filename = GetSaveFileName();

            if (String.IsNullOrEmpty(filename))
            {
                return;
            }

            try
            {
                IEnumerable<string> proxies = fastObjectListViewMain.SelectedObjects.Cast<Manager>().Select(x => x.Proxy.ToString());

                File.WriteAllLines(filename, proxies);

                MessageBox.Show(String.Format("Exported {0} proxies", proxies.Count()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Failed to export proxies. Ex: {0}", ex.Message));
            }
        }

        private void clearCountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.ClearStats();
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void logsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.ClearLog();
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void pauseUnPauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.TogglePause();
            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.Restart();
            }
        }

        private void logViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";
                
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    LogViewerForm lvForm = new LogViewerForm(ofd.FileName);

                    lvForm.ShowDialog();
                }
            }
        }

        #region Fast Settings

        private void setGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Group name", "Set group name", "Default");

            if(String.IsNullOrEmpty(data))
            {
                return;
            }

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.GroupName = data;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void setMaxLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Max Level:", "Set Max Level");

            if(String.IsNullOrEmpty(data))
            {
                return;
            }

            int level = 0;

            if(!Int32.TryParse(data, out level) || level < 0)
            {
                MessageBox.Show("Invalid value");
                return;
            }

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.MaxLevel = level;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void contextMenuStripAccounts_Opening(object sender, CancelEventArgs e)
        {
            Manager manager = fastObjectListViewMain.SelectedObjects.Cast<Manager>().FirstOrDefault();

            if(manager == null)
            {
                return;
            }

            enableTransferToolStripMenuItem.Checked = manager.UserSettings.TransferPokemon;
            enableEvolveToolStripMenuItem1.Checked = manager.UserSettings.EvolvePokemon;
            enableRecycleToolStripMenuItem4.Checked = manager.UserSettings.RecycleItems;
            enableIncubateEggsToolStripMenuItem5.Checked = manager.UserSettings.IncubateEggs;
            enableLuckyEggsToolStripMenuItem6.Checked = manager.UserSettings.UseLuckyEgg;
            enableSnipePokemonToolStripMenuItem3.Checked = manager.UserSettings.SnipePokemon;
            enableCatchPokemonToolStripMenuItem2.Checked = manager.UserSettings.CatchPokemon;
        }

        private void enableTransferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.TransferPokemon = !enableTransferToolStripMenuItem.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void enableEvolveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.EvolvePokemon = !enableEvolveToolStripMenuItem1.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void enableRecycleToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.RecycleItems = !enableRecycleToolStripMenuItem4.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void enableIncubateEggsToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.IncubateEggs = !enableIncubateEggsToolStripMenuItem5.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void enableLuckyEggsToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.UseLuckyEgg = !enableLuckyEggsToolStripMenuItem6.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void enableCatchPokemonToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.CatchPokemon = !enableCatchPokemonToolStripMenuItem2.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void enableSnipePokemonToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.SnipePokemon = !enableSnipePokemonToolStripMenuItem3.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void setRequiredPokemonToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string data = Prompt.ShowDialog("Evolvable pokemon required to evolve:", "Set Min Pokemon Before Evolve");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            int value = 0;

            if (!Int32.TryParse(data, out value) || value >= 500 || value < 0)
            {
                MessageBox.Show("Invalid value");
                return;
            }

            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.MinPokemonBeforeEvolve = value;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void setPokestopRateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Snipe after pokestops amount:", "Set Pokestop Rate");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            int value = 0;

            if (!Int32.TryParse(data, out value) || value >= 1000 || value < 0)
            {
                MessageBox.Show("Invalid value");
                return;
            }

            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.SnipeAfterPokestops = value;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void setMinBallsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Minimum balls required to snipe:", "Set Minimum Balls");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            int value = 0;

            if (!Int32.TryParse(data, out value) || value >= 1000 || value < 0)
            {
                MessageBox.Show("Invalid value");
                return;
            }

            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.MinBallsToSnipe = value;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void setMaxPokemonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Total pokemon per snipe:", "Set Maximum Pokemon To Snipe");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            int value = 0;

            if (!Int32.TryParse(data, out value) || value >= 500 || value < 0)
            {
                MessageBox.Show("Invalid value");
                return;
            }

            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.MaxPokemonPerSnipe = value;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void afterLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Snipe after level:", "Set Snipe After Level");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            int value = 0;

            if (!Int32.TryParse(data, out value) || value >= 40 || value < 0)
            {
                MessageBox.Show("Invalid value");
                return;
            }

            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.SnipeAfterLevel = value;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        #endregion

        private void exportJsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = String.Empty;
            List<AccountExportModel> exportModels = new List<AccountExportModel>();

            using(SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    fileName = sfd.FileName;
                }
                else
                {
                    return;
                }
            }

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                MethodResult<AccountExportModel> result = manager.GetAccountExport();

                if(!result.Success)
                {
                    continue;
                }

                exportModels.Add(result.Data);
            }

            try
            {
                string data = JsonConvert.SerializeObject(exportModels, Formatting.None);

                File.WriteAllText(fileName, data);

                MessageBox.Show(String.Format("Successfully exported {0} of {1} accounts", exportModels.Count, fastObjectListViewMain.SelectedObjects.Count));

            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format("Failed to save to file. Ex: {0}", ex.Message));
            }
        }

    }
}
