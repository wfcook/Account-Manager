using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Helpers;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.Models;
using PokemonGoGUI.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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

        public MainForm()
        {
            InitializeComponent();
        }

        private void ShowDetails(IEnumerable<Manager> managers)
        {
            foreach(Manager manager in managers)
            {
                DetailsForm dForm = new DetailsForm(manager);
                dForm.Show();
            }
        }

        private void RefreshManager(Manager manager)
        {
            fastObjectListViewMain.RefreshObject(manager);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
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

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.OnLog -= manager_OnLog;
                manager.OnInventoryUpdate -= manager_OnInventoryUpdate;

                _managers.Remove(manager);
            }

            fastObjectListViewMain.SetObjects(_managers);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                AccountSettingsForm asForm = new AccountSettingsForm(manager);
                asForm.ShowDialog();
            }

            //fastObjectListViewMain.SetObjects(_managers);
        }

        private void fastObjectListViewMain_KeyDown(object sender, KeyEventArgs e)
        {
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

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int totalStarted = _managers.Count(x => x.IsRunning);

            bool confirmed = false;

            foreach(Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                if(totalStarted > 5 && !confirmed)
                {
                    DialogResult result = MessageBox.Show("Starting too many bots can result in a temp IP ban. Continue?", "Confirmation", MessageBoxButtons.YesNo);

                    if(result == DialogResult.Yes)
                    {
                        confirmed = true;
                    }
                    else
                    {
                        return;
                    }
                }

                manager.Start();

                ++totalStarted;
            }

            //fastObjectListViewMain.SetObjects(_managers);
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Manager manager in fastObjectListViewMain.SelectedObjects)
            {
                manager.Stop();
            }

            //fastObjectListViewMain.SetObjects(_managers);
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
                sfd.Title = "Save Accounts";
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
                    manager.UserSettings.MaxLevel = importModel.MaxLevel;

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

        private void exportAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = GetSaveFileName();

            if(String.IsNullOrEmpty(filename))
            {
                return;
            }

            try
            {
                IEnumerable<string> accounts = fastObjectListViewMain.SelectedObjects.Cast<Manager>().Select(x => String.Format("{0}:{1}", x.UserSettings.PtcUsername, x.UserSettings.PtcPassword));

                File.WriteAllLines(filename, accounts);

                MessageBox.Show(String.Format("Exported {0} accounts", accounts.Count()));
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format("Failed to export accounts. Ex: {0}", ex.Message));
            }
        }
    }
}
