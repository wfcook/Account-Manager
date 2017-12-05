using Newtonsoft.Json;
using POGOProtos.Enums;
using PokemonGoGUI.AccountScheduler;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using PokemonGoGUI.ProxyManager;
using PokemonGoGUI.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGoGUI
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private List<Manager> _managers = new List<Manager>();
        private ProxyHandler _proxyHandler = new ProxyHandler();
        private List<Scheduler> _schedulers = new List<Scheduler>();
        private bool _spf = false;
        private bool _showStartup = true;

        private readonly string _saveFile = "data";
        private string _versionNumber = $"{Application.ProductVersion} By --=FurtiF™=--";

        public MainForm()
        {
            InitializeComponent();

            fastObjectListViewMain.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewMain.ForeColor = Color.LightGray;

            fastObjectListViewProxies.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewProxies.ForeColor = Color.LightGray;

            fastObjectListViewScheduler.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewScheduler.ForeColor = Color.LightGray;

            //BackColor = Color.FromArgb(43, 43, 43);

            //tabPage1.BorderStyle = BorderStyle.None;
            //tabPage1.BackColor = Color.FromArgb(43, 43, 43);
            //fastOjectListViewMain.AlwaysGroupByColumn = olvColumnGroup;

            Text = "GoManager - " + _versionNumber;

            olvColumnProxyAuth.AspectGetter = delegate(object x)
            {
                GoProxy proxy = (GoProxy)x;

                if(String.IsNullOrEmpty(proxy.Username) || String.IsNullOrEmpty(proxy.Password))
                {
                    return String.Empty;
                }

                return String.Format("{0}:{1}", proxy.Username, proxy.Password);
            };

            olvColumnCurrentFails.AspectGetter = delegate(object x)
            {
                GoProxy proxy = (GoProxy)x;

                return String.Format("{0}/{1}", proxy.CurrentConcurrentFails, proxy.MaxConcurrentFails);
            };


            olvColumnUsageCount.AspectGetter = delegate(object x)
            {
                GoProxy proxy = (GoProxy)x;

                return String.Format("{0}/{1}", proxy.CurrentAccounts, proxy.MaxAccounts);
            };
        }

        private void ShowDetails(IEnumerable<Manager> managers)
        {
            int count = fastObjectListViewMain.SelectedObjects.Count;

            if (count > 1)
            {
                DialogResult result = MessageBox.Show(String.Format("Are you sure you want to open {0} detail forms?", count), "Confirmation", MessageBoxButtons.YesNo);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            foreach(Manager manager in managers)
            {
                if (manager.IsRunning)
                {
                    DetailsForm dForm = new DetailsForm(manager);
                    dForm.Show();
                }
            }
        }

        private void FastObjectListViewMain_DoubleClick(object sender, EventArgs e)
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

            await GoManager.VersionCheckState.Execute();

            await LoadSettings();

            if(_showStartup)
            {
                StartupForm startForm = new StartupForm();
                
                if(startForm.ShowDialog() == DialogResult.OK)
                {
                    _showStartup = startForm.ShowOnStartUp;
                }
            }

            UpdateStatusBar();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_managers.Any(x => x.IsRunning))
            {
                MessageBox.Show("Please stop bots before closing", "Information");

                e.Cancel = true;
            }

            SaveSettings();
        }

        private async Task<bool> LoadSettings()
        {
            string jsonFile = _saveFile + ".json";
            string gzipFile = _saveFile + ".json.gz";

            try
            {
                bool jsonFileExists = File.Exists(jsonFile);
                bool gzipFileExists = File.Exists(gzipFile);

                if(!jsonFileExists && !gzipFileExists)
                {
                    return false;
                }

                List<Manager> tempManagers = new List<Manager>();

                if (gzipFileExists)
                {
                    byte[] byteData = await Task.Run(() => File.ReadAllBytes(gzipFile));
                    string data = Compression.Unzip(byteData);

                    ProgramExportModel model = Serializer.FromJson<ProgramExportModel>(data);

                    _proxyHandler = model.ProxyHandler;
                    tempManagers = model.Managers;
                    _schedulers = model.Schedulers;
                    _spf = model.SPF;
                    _showStartup = model.ShowWelcomeMessage;

                }
                else
                {
                    string data = await Task.Run(() => File.ReadAllText(jsonFile));

                    tempManagers = Serializer.FromJson<List<Manager>>(data);
                }

                if(tempManagers == null)
                {
                    MessageBox.Show("Failed to load settings");
                    return true;
                }

                foreach(Manager manager in tempManagers)
                {
                    manager.AddSchedulerEvent();
                    manager.ProxyHandler = _proxyHandler;
                    manager.OnLog += Manager_OnLog;
                    manager.OnInventoryUpdate += Manager_OnInventoryUpdate;

                    //Patch for version upgrade
                    if(String.IsNullOrEmpty(manager.UserSettings.DeviceId))
                    {
                        //Load some
                        manager.UserSettings.LoadDeviceSettings();
                    }

                    if (manager.Tracker != null)
                    {
                        manager.Tracker.CalculatedTrackingHours();
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

            return true;
        }

        private void SaveSettings()
        {
            try
            {
                ProgramExportModel model = new ProgramExportModel
                {
                    Managers = _managers,
                    ProxyHandler = _proxyHandler,
                    Schedulers = _schedulers,
                    SPF = _spf,
                    ShowWelcomeMessage = _showStartup
                };

                string data = Serializer.ToJson(model);

                byte[] dataBytes = Compression.Zip(data);

                File.WriteAllBytes(_saveFile + ".json.gz", dataBytes);
            }
            catch
            {
                //Failed to save
            }
        }

        private void Manager_OnInventoryUpdate(object sender, EventArgs e)
        {
            //return;

            Manager manager = sender as Manager;

            if(manager == null)
            {
                return;
            }

            RefreshManager(manager);
        }

        private void Manager_OnLog(object sender, LoggerEventArgs e)
        {
            //return;

            Manager manager = sender as Manager;

            if (manager == null)
            {
                return;
            }

            RefreshManager(manager);
        }

        private void AddNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Manager manager = new Manager(_proxyHandler);

            AccountSettingsForm asForm = new AccountSettingsForm(manager);
            
            if(asForm.ShowDialog() == DialogResult.OK)
            {
                AddManager(manager);
            }

            fastObjectListViewMain.SetObjects(_managers);
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
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
                    manager.RemoveProxy();
                    manager.RemoveScheduler();

                    manager.OnLog -= Manager_OnLog;
                    manager.OnInventoryUpdate -= Manager_OnInventoryUpdate;

                    _managers.Remove(manager);
                }
            }

            fastObjectListViewMain.SetObjects(_managers);
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void FastObjectListViewMain_KeyDown(object sender, KeyEventArgs e)
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

        private void ViewDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowDetails(fastObjectListViewMain.SelectedObjects.Cast<Manager>());
        }

        private async void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = false;

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.SPF = _spf;
                manager.Start();

                await Task.Delay(200);
            }

            startToolStripMenuItem.Enabled = true;

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void StopToolStripMenuItem_Click(object sender, EventArgs e)
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
            manager.OnLog += Manager_OnLog;
            manager.OnInventoryUpdate += Manager_OnInventoryUpdate;

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

        private void UpdateStatusBar()
        {
            toolStripStatusLabelTotalAccounts.Text = _managers.Count.ToString();

            //Longer running
            int tempBanned = 0;
            int running = 0;
            int permBan = 0;

            List<Manager> tempManagers = new List<Manager>(_managers);

            foreach(Manager manager in tempManagers)
            {
                if(manager.IsRunning)
                {
                    ++running;
                }

                if(manager.AccountState == AccountState.PermAccountBan)
                {
                    ++permBan;
                }

                if(manager.AccountState == AccountState.PokemonBanAndPokestopBanTemp ||
                    manager.AccountState == AccountState.PokemonBanTemp ||
                    manager.AccountState == AccountState.PokestopBanTemp)
                {
                    ++tempBanned;
                }
            }

            toolStripStatusLabelAccountBanned.Text = permBan.ToString();
            toolStripStatusLabelTempBanned.Text = tempBanned.ToString();
            toolStripStatusLabelTotalRunning.Text = running.ToString();

            if(_proxyHandler.Proxies != null)
            {
                toolStripStatusLabelTotalProxies.Text = _proxyHandler.Proxies.Count.ToString();
                toolStripStatusLabelBannedProxies.Text = _proxyHandler.Proxies.Count(x => x.IsBanned).ToString();
            }
        }

        private async void WConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tSMI = sender as ToolStripMenuItem;

            if (tSMI == null || !Boolean.TryParse(tSMI.Tag.ToString(), out bool useConfig))
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

                    Manager manager = new Manager(_proxyHandler);

                    if (useConfig)
                    {
                        MethodResult result = await manager.ImportConfigFromFile(configFile);

                        if (!result.Success)
                        {
                            MessageBox.Show("Failed to import configuration file");

                            return;
                        }
                    }

                    manager.UserSettings.AccountName = importModel.Username.Trim();
                    manager.UserSettings.PtcUsername = importModel.Username.Trim();
                    manager.UserSettings.PtcPassword = importModel.Password.Trim();
                    manager.UserSettings.ProxyIP = importModel.Address;
                    manager.UserSettings.ProxyPort = importModel.Port;
                    manager.UserSettings.ProxyUsername = importModel.ProxyUsername;
                    manager.UserSettings.ProxyPassword = importModel.ProxyPassword;

                    if(importModel.Username.Contains("@"))
                    {
                        manager.UserSettings.AuthType = AuthType.Google;
                    }
                    else
                    {
                        manager.UserSettings.AuthType = AuthType.Ptc;
                    }

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

        private void TimerListViewUpdate_Tick(object sender, EventArgs e)
        {
            if(WindowState == FormWindowState.Minimized)
            {
                return;
            }

            if (tabControlProxies.SelectedTab == tabPageAccounts)
            {
                if (_managers.Count == 0)
                {
                    return;
                }

                fastObjectListViewMain.RefreshObject(_managers[0]);
            }
            else if(tabControlProxies.SelectedTab == tabPageProxies)
            {
                if(_proxyHandler.Proxies.Count == 0)
                {
                    return;
                }

                fastObjectListViewProxies.RefreshObject(_proxyHandler.Proxies.First());
            }
            else if (tabControlProxies.SelectedTab == tabPageScheduler)
            {
                if(_schedulers.Count == 0)
                {
                    return;
                }

                fastObjectListViewScheduler.RefreshObject(_schedulers[0]);
            }
        }

        private void ClearProxiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int totalAccounts = fastObjectListViewMain.SelectedObjects.Count;

            DialogResult result = MessageBox.Show(String.Format("Clear proxies from {0} accounts?", totalAccounts), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.RemoveProxy();

                manager.UserSettings.ProxyIP = null;
                manager.UserSettings.ProxyPort = 0;
                manager.UserSettings.ProxyUsername = null;
                manager.UserSettings.ProxyPassword = null;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableColorsToolStripMenuItem.Checked = !enableColorsToolStripMenuItem.Checked;
            bool isChecked = enableColorsToolStripMenuItem.Checked;

            if(isChecked)
            {
                new Scheduler();

                fastObjectListViewMain.BackColor = Color.FromArgb(43, 43, 43);
                fastObjectListViewMain.ForeColor = Color.LightGray;

                fastObjectListViewProxies.BackColor = Color.FromArgb(43, 43, 43);
                fastObjectListViewProxies.ForeColor = Color.LightGray;

                fastObjectListViewScheduler.BackColor = Color.FromArgb(43, 43, 43);
                fastObjectListViewScheduler.ForeColor = Color.LightGray;

                fastObjectListViewMain.UseCellFormatEvents = true;
            }
            else
            {
                fastObjectListViewMain.BackColor = SystemColors.Window;
                fastObjectListViewMain.ForeColor = SystemColors.WindowText;

                fastObjectListViewProxies.BackColor = SystemColors.Window;
                fastObjectListViewProxies.ForeColor = SystemColors.WindowText;


                fastObjectListViewScheduler.BackColor = SystemColors.Window;
                fastObjectListViewScheduler.ForeColor = SystemColors.WindowText;

                fastObjectListViewMain.UseCellFormatEvents = false;
                fastObjectListViewProxies.UseCellFormatEvents = false;
            }
        }

        private void ShowGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showGroupsToolStripMenuItem.Checked = !showGroupsToolStripMenuItem.Checked;
        }

        private void FastObjectListViewMain_FormatCell(object sender, BrightIdeasSoftware.FormatCellEventArgs e)
        {
            Manager manager = (Manager)e.Model;

            if(e.Column == olvColumnScheduler)
            {
                if(manager.AccountScheduler != null)
                {
                    e.SubItem.ForeColor = manager.AccountScheduler.NameColor;
                }
            }
            else if (e.Column == olvColumnExpPerHour)
            {
                if(manager.LuckyEggActive)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
            }
            else if(e.Column == olvColumnAccountState)
            {
                switch(manager.AccountState)
                {
                    case AccountState.PermAccountBan:
                        e.SubItem.ForeColor = Color.Red;
                        break;
                    case AccountState.NotVerified:
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
            else if (e.Column == olvColumnPokemonCaught)
            {
                if (manager.AccountScheduler == null || manager.AccountScheduler.PokemonLimiter.Option == SchedulerOption.Nothing)
                {
                    return;
                }

                if(manager.PokemonCaught >= manager.AccountScheduler.PokemonLimiter.Max)
                {
                    e.SubItem.ForeColor = Color.Red;
                }
                else if (manager.PokemonCaught <= manager.AccountScheduler.PokemonLimiter.Min)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }

            }
            else if (e.Column == olvColumnPokestopsFarmed)
            {
                if (manager.AccountScheduler == null || manager.AccountScheduler.PokeStoplimiter.Option == SchedulerOption.Nothing)
                {
                    return;
                }

                if (manager.PokestopsFarmed >= manager.AccountScheduler.PokeStoplimiter.Max)
                {
                    e.SubItem.ForeColor = Color.Red;
                }
                else if (manager.PokestopsFarmed <= manager.AccountScheduler.PokeStoplimiter.Min)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
            }

        }

        private void GarbageCollectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("This should not be called outside testing purposes. Continue?", "Confirmation", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                GC.Collect();
            }
        }

        private async void ExportStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10
            };

            IEnumerable<Manager> managers = fastObjectListViewMain.SelectedObjects.Cast<Manager>();

            await Task.Run(() =>
                {
                    Parallel.ForEach(managers, (manager) =>
                    {
                        manager.ExportStats().Wait();
                    });
                });
        }

        private async void UpdateDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateDetailsToolStripMenuItem.Enabled = false;

            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10
            };

            IEnumerable<Manager> selectedManager = fastObjectListViewMain.SelectedObjects.Cast<Manager>();

            await Task.Run(() =>
                {
                    Parallel.ForEach(selectedManager, options, (manager) =>
                    {
                        manager.UpdateDetails().Wait();
                    });
                });

            updateDetailsToolStripMenuItem.Enabled = true;
        }

        private void ImportProxiesToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            int count = fastObjectListViewMain.SelectedObjects.Count;
            string fileName = String.Empty;

            if (count == 0)
            {
                MessageBox.Show("Please select 1 or more accounts");
                return;
            }

            string pPerAccount = Prompt.ShowDialog("Accounts per proxy", "Accounts per proxy", "1");

            if(String.IsNullOrEmpty(pPerAccount))
            {
                return;
            }

            if (!Int32.TryParse(pPerAccount, out int accountsPerProxy) || accountsPerProxy <= 0)
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

        private void ExportAccountsToolStripMenuItem_Click_1(object sender, EventArgs e)
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

        private void ExportProxiesToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void ClearCountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("This will reset your last 23 hour count and is updated to accurately reflect your pokestops + pokemon counts.\n\nAre you sure you want to clear this?", "Confirmation", MessageBoxButtons.YesNo);

            if(result != DialogResult.Yes)
            {
                return;
            }

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.ClearStats();
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void LogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.ClearLog();
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void PauseUnPauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.TogglePause();
            }
        }

        private async void RestartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.Restart();

                await Task.Delay(100);
            }
        }

        #region Fast Settings

        private void ClaimLevelUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.ClaimLevelUpRewards = !claimLevelUpToolStripMenuItem.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableIPBanStopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.StopOnIPBan = !enableIPBanStopToolStripMenuItem.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableRotateProxiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.AutoRotateProxies = !enableRotateProxiesToolStripMenuItem.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void SetMaxRuntimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Max Runtime (hours)", "Set Max Runtime").Replace(",", ".");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            if (!Double.TryParse(data, NumberStyles.Any, CultureInfo.InvariantCulture, out double value) || value < 0)
            {
                MessageBox.Show("Invalid runtime value");
            }

            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.RunForHours = value;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void SetGroupToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void SetMaxLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Max Level:", "Set Max Level");

            if(String.IsNullOrEmpty(data))
            {
                return;
            }

            if (!Int32.TryParse(data, out int level) || level < 0)
            {
                MessageBox.Show("Invalid value");
                return;
            }

            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.MaxLevel = level;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void ContextMenuStripAccounts_Opening(object sender, CancelEventArgs e)
        {
            enableSpoofToolStripMenuItem.Checked = _spf;

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
            enableRotateProxiesToolStripMenuItem.Checked = manager.UserSettings.AutoRotateProxies;
            enableIPBanStopToolStripMenuItem.Checked = manager.UserSettings.StopOnIPBan;
            claimLevelUpToolStripMenuItem.Checked = manager.UserSettings.ClaimLevelUpRewards;

            //Remove all
            schedulerToolStripMenuItem.DropDownItems.Clear();

            //Add none
            ToolStripMenuItem noneSMI = new ToolStripMenuItem("None");
            noneSMI.Click += (o, s) =>
                {
                    foreach (Manager m in fastObjectListViewMain.SelectedObjects)
                    {
                        m.RemoveScheduler();
                    }
                };

            schedulerToolStripMenuItem.DropDownItems.Add(noneSMI);

            //Add all current schedulers
            if (_schedulers != null)
            {
                foreach (Scheduler scheduler in _schedulers)
                {
                    ToolStripMenuItem tSMI = new ToolStripMenuItem(scheduler.Name)
                    {
                        Tag = scheduler
                    };
                    tSMI.Click += Schedule_Click;

                    schedulerToolStripMenuItem.DropDownItems.Add(tSMI);
                }
            }
        }

        private void Schedule_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tSMI = sender as ToolStripMenuItem;
            
            if(tSMI == null)
            {
                return;
            }

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.AddScheduler((Scheduler)tSMI.Tag);
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableTransferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.TransferPokemon = !enableTransferToolStripMenuItem.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableEvolveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.EvolvePokemon = !enableEvolveToolStripMenuItem1.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableRecycleToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.RecycleItems = !enableRecycleToolStripMenuItem4.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableIncubateEggsToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.IncubateEggs = !enableIncubateEggsToolStripMenuItem5.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableLuckyEggsToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.UseLuckyEgg = !enableLuckyEggsToolStripMenuItem6.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableCatchPokemonToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.CatchPokemon = !enableCatchPokemonToolStripMenuItem2.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void EnableSnipePokemonToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.UserSettings.SnipePokemon = !enableSnipePokemonToolStripMenuItem3.Checked;
            }

            fastObjectListViewMain.RefreshSelectedObjects();
        }

        private void SetRequiredPokemonToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string data = Prompt.ShowDialog("Evolvable pokemon required to evolve:", "Set Min Pokemon Before Evolve");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            if (!Int32.TryParse(data, out int value) || value >= 500 || value < 0)
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

        private void SetPokestopRateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Snipe after pokestops amount:", "Set Pokestop Rate");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            if (!Int32.TryParse(data, out int value) || value >= 1000 || value < 0)
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

        private void SetMinBallsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Minimum balls required to snipe:", "Set Minimum Balls");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            if (!Int32.TryParse(data, out int value) || value >= 1000 || value < 0)
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

        private void SetMaxPokemonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Total pokemon per snipe:", "Set Maximum Pokemon To Snipe");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            if (!Int32.TryParse(data, out int value) || value >= 500 || value < 0)
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

        private void AfterLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Snipe after level:", "Set Snipe After Level");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            if (!Int32.TryParse(data, out int value) || value >= 40 || value < 0)
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

        private async void ExportJsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = String.Empty;
            List<AccountExportModel> exportModels = new List<AccountExportModel>();

            DialogResult dialogResult = MessageBox.Show("Update details before exporting?", "Update details", MessageBoxButtons.YesNoCancel);

            if(dialogResult == DialogResult.Cancel)
            {
                return;
            }

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

            IEnumerable<Manager> selectedManagers = fastObjectListViewMain.SelectedObjects.Cast<Manager>();

            await Task.Run(() =>
                {
                    ParallelOptions options = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 10
                    };

                    Parallel.ForEach(selectedManagers, options, (manager) =>
                    {
                        if (dialogResult == DialogResult.Yes)
                        {
                            manager.UpdateDetails().Wait();
                        }

                        MethodResult<AccountExportModel> result = manager.GetAccountExport();

                        if (!result.Success)
                        {
                            return;
                        }

                        exportModels.Add(result.Data);
                    });
                });

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

        private void ShowStatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(WindowState == FormWindowState.Minimized)
            {
                return;
            }

            bool showGroups = !statusStripStats.Visible;

            statusStripStats.Visible = showGroups;
            timerStatusBarUpdate.Enabled = showGroups;

            int scrollBarHeight = 38;

            if(showGroups)
            {
                UpdateStatusBar();
                fastObjectListViewMain.Height = this.Height - statusStripStats.Height - scrollBarHeight;
            }
            else
            {
                fastObjectListViewMain.Height = this.Height - scrollBarHeight;
            }
        }

        private void TimerStatusBarUpdate_Tick(object sender, EventArgs e)
        {
            UpdateStatusBar();
        }

        private void ImportConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string configFile = ImportConfig();

            if(String.IsNullOrEmpty(configFile))
            {
                return;
            }

            try
            {
                string data = File.ReadAllText(configFile);

                foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
                {
                    manager.ImportConfig(data);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format("Failed to import config file. Ex: {0}", ex.Message));
            }
        }

        private async void SnipeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> currentPokemon = new List<string>();

            foreach(PokemonId p in Enum.GetValues(typeof(PokemonId)))
            {
                if(p != PokemonId.Missingno)
                {
                    currentPokemon.Add(p.ToString());
                }
            }

            string pokemon = AutoCompletePrompt.ShowDialog("Pokemon to snipe", "Pokemon", currentPokemon);

            if(String.IsNullOrEmpty(pokemon))
            {
                return;
            }

            PokemonId pokemonToSnipe = PokemonId.Missingno;

            if(!Enum.TryParse<PokemonId>(pokemon, true, out pokemonToSnipe))
            {
                MessageBox.Show("Invalid pokemon");
                return;
            }

            string data = Prompt.ShowDialog("Location. Format = x.xxx, x.xxx", "Enter Location");

            if(String.IsNullOrEmpty(data))
            {
                return;
            }

            string[] parts = data.Split(',');

            if (!double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
            {
                MessageBox.Show("Invalid latitutde.");
                return;
            }

            if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
            {
                MessageBox.Show("Invalid longitude.");
                return;
            }

            snipePokemonToolStripMenuItem.Enabled = false;

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                //Snipe all at once
                await manager.ManualSnipe(lat, lon, pokemonToSnipe).ConfigureAwait(false);

                await Task.Delay(100);
            }

            snipePokemonToolStripMenuItem.Enabled = true;
        }


        #region Proxies

        private void TabControlProxies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabControlProxies.SelectedTab == tabPageProxies)
            {
                fastObjectListViewProxies.SetObjects(_proxyHandler.Proxies);
            }
            else if(tabControlProxies.SelectedTab == tabPageScheduler)
            {
                fastObjectListViewScheduler.SetObjects(_schedulers);
            }
        }

        private void ResetBanStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(GoProxy proxy in fastObjectListViewProxies.SelectedObjects)
            {
                _proxyHandler.MarkProxy(proxy, false);
            }

            fastObjectListViewProxies.RefreshSelectedObjects();
        }

        private void SingleProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Add proxy", "Proxy");

            if(String.IsNullOrEmpty(data))
            {
                return;
            }

            bool success = _proxyHandler.AddProxy(data);

            if(!success)
            {
                MessageBox.Show("Invalid proxy format");
                return;
            }

            fastObjectListViewProxies.SetObjects(_proxyHandler.Proxies);
        }

        private void FromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = String.Empty;
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

            try
            {
                string[] proxyData = File.ReadAllLines(fileName);

                int count = 0;

                foreach (string pData in proxyData)
                {
                    if(_proxyHandler.AddProxy(pData))
                    {
                        ++count;
                    }
                }

                fastObjectListViewProxies.SetObjects(_proxyHandler.Proxies);

                MessageBox.Show(String.Format("Imported {0} proxies", count), "Info");
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format("Failed to import proxy file. Ex: {0}", ex.Message), "Exception occured");
            }
        }

        private void DeleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int count = fastObjectListViewProxies.SelectedObjects.Count;

            if(count == 0)
            {
                return;
            }

            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to delete {0} proxies?", count), "Confirmation", MessageBoxButtons.YesNo);

            if(result != DialogResult.Yes)
            {
                return;
            }

            /*
            Dictionary<GoProxy, List<Manager>> pManagers = new Dictionary<GoProxy, List<Manager>>();

            foreach(Manager manager in _managers)
            {
                if(manager.CurrentProxy == null)
                {
                    continue;
                }

                if(pManagers.ContainsKey(manager.CurrentProxy))
                {
                    pManagers[manager.CurrentProxy].Add(manager);
                }
                else
                {
                    List<Manager> m = new List<Manager>();
                    m.Add(manager);

                    pManagers.Add(manager.CurrentProxy, m);
                }
            }*/

            bool messageShown = false;

            foreach(GoProxy proxy in fastObjectListViewProxies.SelectedObjects)
            {
                if(proxy.CurrentAccounts > 0 && !messageShown)
                {
                    messageShown = true;

                    MessageBox.Show("Only proxies with 0 accounts tied to them will be removed", "Information");
                }

                _proxyHandler.RemoveProxy(proxy);

                /*
                if(pManagers.ContainsKey(proxy))
                {
                    foreach(Manager manager in _managers)
                    {
                        manager.RemoveProxy();
                    }
                }*/
            }

            fastObjectListViewProxies.SetObjects(_proxyHandler.Proxies);
        }

        private void MaxConcurrentFailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Max concurrent fails", "Set fails", "3");

            if(String.IsNullOrEmpty(data))
            {
                return;
            }

            if (!Int32.TryParse(data, out int value) || value <= 0)
            {
                MessageBox.Show("Invalid value", "Warning");
                return;
            }

            foreach (GoProxy proxy in fastObjectListViewProxies.SelectedObjects)
            {
                proxy.MaxConcurrentFails = value;
            }

            fastObjectListViewProxies.RefreshSelectedObjects();
        }

        private void MaxAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Max Accounts", "Set Accounts", "3");

            if (String.IsNullOrEmpty(data))
            {
                return;
            }

            if (!Int32.TryParse(data, out int value) || value <= 0)
            {
                MessageBox.Show("Invalid value", "Warning");
                return;
            }

            foreach (GoProxy proxy in fastObjectListViewProxies.SelectedObjects)
            {
                proxy.MaxAccounts = value;
            }

            fastObjectListViewProxies.RefreshSelectedObjects();
        }

        private void FastObjectListViewProxies_FormatCell(object sender, BrightIdeasSoftware.FormatCellEventArgs e)
        {
            GoProxy proxy = (GoProxy)e.Model;

            if (e.Column == olvColumnCurrentFails)
            {
                if (proxy.CurrentConcurrentFails == 0)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (proxy.CurrentConcurrentFails > 0)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else if (proxy.CurrentConcurrentFails >= proxy.MaxConcurrentFails)
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
            else if (e.Column == olvColumnProxyBanned)
            {
                if (proxy.IsBanned)
                {
                    e.SubItem.ForeColor = Color.Red;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Green;
                }
            }
            else if (e.Column == olvColumnUsageCount)
            {
                if (proxy.CurrentAccounts == 0)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (proxy.CurrentAccounts <= proxy.MaxAccounts)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
        }

        #endregion

        #region Dev Tools

        private void EnableSpfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _spf = !_spf;

            foreach(Manager manager in _managers)
            {
                manager.UserSettings.SPF = _spf;
            }

            //enableSpoofToolStripMenuItem.Checked = _spf;
        }

        private void LargeAddressAwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(LargeAddressAware.IsLargeAware(Application.ExecutablePath).ToString());
        }

        private void LogViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LogViewerForm lvForm = new LogViewerForm(ofd.FileName);

                    lvForm.ShowDialog();
                }
            }
        }

        #endregion

        #region Scheduler

        private void DeleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            int count = fastObjectListViewScheduler.SelectedObjects.Count;

            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to remove {0} schedules?", count), "Confirmation", MessageBoxButtons.YesNo);

            if(result != DialogResult.Yes)
            {
                return;
            }

            //Deleting many at once will take awhile without this
            Dictionary<Scheduler, List<Manager>> managerSchedulers = new Dictionary<Scheduler, List<Manager>>();

            foreach(Manager manager in _managers)
            {
                if(manager.AccountScheduler == null)
                {
                    continue;
                }

                if(managerSchedulers.ContainsKey(manager.AccountScheduler))
                {
                    managerSchedulers[manager.AccountScheduler].Add(manager);
                }
                else
                {
                    List<Manager> m = new List<Manager>
                    {
                        manager
                    };

                    managerSchedulers.Add(manager.AccountScheduler, m);
                }
            }

            foreach (Scheduler scheduler in fastObjectListViewScheduler.SelectedObjects)
            {
                _schedulers.Remove(scheduler);
                
                //Should always be true. If not, bug.
                if(managerSchedulers.ContainsKey(scheduler))
                {
                    foreach(Manager manager in managerSchedulers[scheduler])
                    {
                        manager.RemoveScheduler();
                    }
                }
            }

            fastObjectListViewScheduler.SetObjects(_schedulers);
        }

        private void EnablelDisableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Scheduler scheduler in fastObjectListViewScheduler.SelectedObjects)
            {
                scheduler.Enabled = !scheduler.Enabled;
            }

            fastObjectListViewScheduler.RefreshSelectedObjects();
        }

        private void EditToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int count = fastObjectListViewScheduler.SelectedObjects.Count;

            if(count > 1)
            {
                DialogResult result = MessageBox.Show(String.Format("Are you sure you want to open up {0} edit forms?", count), "Confirmation", MessageBoxButtons.YesNo);

                if(result != DialogResult.Yes)
                {
                    return;
                }
            }

            foreach(Scheduler scheduler in fastObjectListViewScheduler.SelectedObjects)
            {
                SchedulerSettingForm schedulerForm = new SchedulerSettingForm(scheduler);

                schedulerForm.ShowDialog();
            }

            fastObjectListViewScheduler.RefreshSelectedObjects();
        }

        private void AddNewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Scheduler scheduler = new Scheduler();

            SchedulerSettingForm schedulerForm = new SchedulerSettingForm(scheduler);
            
            if(schedulerForm.ShowDialog() == DialogResult.OK)
            {
                _schedulers.Add(scheduler);
            }

            fastObjectListViewScheduler.SetObjects(_schedulers);
        }

        private void ManualCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Scheduler scheduler in fastObjectListViewScheduler.SelectedObjects)
            {
                scheduler.ForceCall();
            }
        }

        private void FastObjectListViewScheduler_FormatCell(object sender, BrightIdeasSoftware.FormatCellEventArgs e)
        {
            Scheduler scheduler = (Scheduler)e.Model;
            bool withinTime = scheduler.WithinTime();

            if(e.Column == olvColumnSchedulerName)
            {
                e.SubItem.ForeColor = scheduler.NameColor;
            }
            else if (e.Column == olvColumnSchedulerEnabled)
            {
                if(scheduler.Enabled)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
            else if (e.Column == olvColumnSchedulerStart || e.Column == olvColumnSchedulerEnd)
            {
                if(withinTime)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
        }

        #endregion
    }
}
