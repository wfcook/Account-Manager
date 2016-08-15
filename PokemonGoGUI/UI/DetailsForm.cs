using BrightIdeasSoftware;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Settings.Master;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Helpers;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.GoManager.Models;
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
    public partial class DetailsForm : Form
    {
        private Manager _manager; 
        private int _totalLogs = 0;

        public DetailsForm(Manager manager)
        {
            InitializeComponent();

            _manager = manager;

            fastObjectListViewLogs.PrimarySortColumn = olvColumnDate;
            fastObjectListViewLogs.PrimarySortOrder = SortOrder.Descending;
            fastObjectListViewLogs.ListFilter = new TailFilter(500);

            fastObjectListViewPokedex.PrimarySortColumn = olvColumnPokedexFriendlyName;
            fastObjectListViewPokedex.PrimarySortOrder = SortOrder.Ascending;

            fastObjectListViewLogs.BackColor = Color.FromArgb(43, 43, 43);

            fastObjectListViewPokedex.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewPokedex.ForeColor = Color.LightGray;

            fastObjectListViewPokemon.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewPokemon.ForeColor = Color.LightGray;

            fastObjectListViewInventory.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewInventory.ForeColor = Color.LightGray;

            fastObjectListViewEggs.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewEggs.ForeColor = Color.LightGray;

            fastObjectListViewCandy.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewCandy.ForeColor = Color.LightGray;

            #region Pokedex

            olvColumnPokedexFriendlyName.AspectGetter = delegate(object x)
            {
                PokedexEntry entry = (PokedexEntry)x;

                //ToString for sorting purposes
                return (int)entry.PokemonId;
            };

            olvColumnPokedexId.AspectGetter = delegate(object x)
            {
                PokedexEntry entry = (PokedexEntry)x;

                return entry.PokemonId.ToString();
            };

            olvColumnPokedexFriendlyName.AspectGetter = delegate(object x)
            {
                PokedexEntry entry = (PokedexEntry)x;

                //ToString for sorting purposes
                return (int)entry.PokemonId;
            };

            #endregion

            #region Pokemon

            olvColumnPokemonFavorite.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;

                if(pokemon.Favorite == 1)
                {
                    return true;
                }

                return false;
            };

            olvColumnPokemonRarity.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;
                PokemonSettings pokemonSettings = _manager.GetPokemonSetting(pokemon.PokemonId).Data;

                if (pokemonSettings == null)
                {
                    return PokemonRarity.Normal;
                }

                return pokemonSettings.Rarity;
            };

            olvColumnCandyToEvolve.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;
                PokemonSettings pokemonSettings = _manager.GetPokemonSetting(pokemon.PokemonId).Data;

                if(pokemonSettings == null)
                {
                    return -1;
                }

                return pokemonSettings.CandyToEvolve;
            };

            olvColumnPokemonCandy.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;

                if(_manager.PokemonCandy == null || _manager.PokemonCandy.Count == 0)
                {
                    return -1;
                }

                PokemonSettings settings = _manager.GetPokemonSetting(pokemon.PokemonId).Data;

                if(settings == null)
                {
                    return -1;
                }

                Candy family = _manager.PokemonCandy.FirstOrDefault(y => y.FamilyId == settings.FamilyId);

                if(family == null)
                {
                    return -1;
                }

                return family.Candy_;
            };

            olvColumnPokemonName.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;


                return pokemon.PokemonId.ToString();
            };

            olvColumnPrimaryMove.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;


                return ((PokemonMove)pokemon.Move1).ToString().Replace("Fast", "");
            };

            olvColumnSecondaryMove.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;

                return ((PokemonMove)pokemon.Move2).ToString();
            };

            olvColumnAttack.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;
                MethodResult<PokemonSettings> settings = _manager.GetPokemonSetting(pokemon.PokemonId);

                if(!settings.Success)
                {
                    return -1;
                }

                return pokemon.IndividualAttack;// +settings.Data.Stats.BaseAttack;
            };

            olvColumnDefense.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;
                MethodResult<PokemonSettings> settings = _manager.GetPokemonSetting(pokemon.PokemonId);

                if (!settings.Success)
                {
                    return -1;
                }

                return pokemon.IndividualDefense;// +settings.Data.Stats.BaseDefense;
            };

            olvColumnStamina.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;
                MethodResult<PokemonSettings> settings = _manager.GetPokemonSetting(pokemon.PokemonId);

                if (!settings.Success)
                {
                    return -1;
                }

                return pokemon.IndividualStamina;// +settings.Data.Stats.BaseStamina;
            };


            olvColumnPerfectPercent.AspectGetter = delegate(object x)
            {
                PokemonData pokemon = (PokemonData)x;
                MethodResult<double> settings = _manager.CalculateIVPerfection(pokemon);

                if (!settings.Success)
                {
                    return -1;
                }

                string sDouble = String.Format("{0:0.00}", settings.Data);

                return double.Parse(sDouble);// +settings.Data.Stats.BaseStamina;
            };

            #endregion

            #region Candy

            olvColumnCandyFamily.AspectGetter = delegate(object x)
            {
                Candy family = (Candy)x;

                return family.FamilyId.ToString().Replace("Family", "");
            };

            #endregion

            #region Inventory

            olvColumnInventoryItem.AspectGetter = delegate(object x)
            {
                ItemData item = (ItemData)x;

                return item.ItemId.ToString().Replace("Item", "");
            };

            #endregion
        }

        private async void DetailsForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _totalLogs = _manager.Logs.Count;

            if (_manager.LogHeaderSettings != null)
            {
                fastObjectListViewLogs.RestoreState(_manager.LogHeaderSettings);
            }

            List<LoggerTypes> values = new List<LoggerTypes>();

            foreach(LoggerTypes type in Enum.GetValues(typeof(LoggerTypes)))
            {
                if(type == LoggerTypes.LocationUpdate)
                {
                    continue;
                }

                values.Add(type);
            }

            olvColumnStatus.ValuesChosenForFiltering = values;
            fastObjectListViewLogs.UpdateColumnFiltering();

            Text = _manager.AccountName;

            _manager.OnInventoryUpdate += _manager_OnInventoryUpdate;
            _manager.OnLog += _manager_OnLog;

            DisplayDetails();
            UpdateListViews();

            if (_manager.State == BotState.Stopped)
            {
                await UpdateDetails();
            }

            DisplayDetails();
            UpdateListViews();
        }

        private void _manager_OnLog(object sender, LoggerEventArgs e)
        {
            //Update logs
            if (fastObjectListViewLogs.IsDisposed || fastObjectListViewLogs.Disposing)
            {
                return;
            }

            fastObjectListViewLogs.SetObjects(_manager.Logs);

            DisplayDetails();

            if (e.LogType != LoggerTypes.Debug && e.LogType != LoggerTypes.LocationUpdate)
            {
                Invoke(new MethodInvoker(() =>
                {
                    if (tabControlMain.SelectedTab == tabPageLogs)
                    {
                        _totalLogs = _manager.TotalLogs;
                    }
                    else
                    {
                        int newLogs = _manager.TotalLogs - _totalLogs;

                        if (newLogs > 0)
                        {
                            tabPageLogs.Text = String.Format("Logs ({0})", newLogs);
                        }
                    }
                }));
            }
        }

        private void DetailsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _manager.OnInventoryUpdate -= _manager_OnInventoryUpdate;
            _manager.OnLog -= _manager_OnLog;

            _manager.LogHeaderSettings = fastObjectListViewLogs.SaveState();
        }

        private void _manager_OnInventoryUpdate(object sender, EventArgs e)
        {
            DisplayDetails();
            UpdateListViews();
        }

        private async Task UpdateDetails()
        {
            buttonUpdateStats.Enabled = false;

            MethodResult result = await _manager.UpdateDetails();

            if (!result.Success)
            {
                MessageBox.Show("Failed to login");
            }


            buttonUpdateStats.Enabled = true;
        }

        private void DisplayDetails()
        {
            if(InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                    {
                        DisplayDetails();
                    }));

                return;
            }

            labelPlayerLevel.Text = _manager.Level.ToString();
            labelExp.Text = _manager.ExpRatio.ToString();
            labelRunningTime.Text = _manager.RunningTime;
            labelStardust.Text = _manager.TotalStardust.ToString();
            labelExpPerHour.Text = String.Format("{0:0}", _manager.ExpPerHour);
            labelExpGained.Text = _manager.ExpGained.ToString();
            labelDistanceWalked.Text = String.Format("{0:0.00}km", _manager.Stats.KmWalked);

            if (_manager.Stats != null)
            {
                labelPokemonCaught.Text = _manager.Stats.PokemonsCaptured.ToString();
                labelPokestopVisits.Text = _manager.Stats.PokeStopVisits.ToString();
                labelUniquePokemon.Text = _manager.Stats.UniquePokedexEntries.ToString();
            }

            if (_manager.Pokemon != null)
            {
                labelPokemonCount.Text = String.Format("{0}/{1}", _manager.Pokemon.Count + _manager.Eggs.Count, _manager.MaxPokemonStorage);
            }

            if (_manager.Items != null)
            {
                labelInventoryCount.Text = String.Format("{0}/{1}", _manager.Items.Sum(x => x.Count).ToString(), _manager.MaxItemStorage);
            }

            if(_manager.PlayerData != null)
            {
                labelPlayerUsername.Text = _manager.PlayerData.Username;
                labelPlayerTeam.Text = _manager.PlayerData.Team.ToString();
            }
        }

        private void UpdateListViews()
        {
            fastObjectListViewPokedex.SetObjects(_manager.Pokedex);
            //fastObjectListViewPokemon.SetObjects(_manager.Pokemon);
            fastObjectListViewCandy.SetObjects(_manager.PokemonCandy);
            fastObjectListViewInventory.SetObjects(_manager.Items);
            fastObjectListViewLogs.SetObjects(_manager.Logs);
            fastObjectListViewEggs.SetObjects(_manager.Eggs);
        }

        private async void buttonUpdateStats_Click(object sender, EventArgs e)
        {
            await UpdateDetails();
        }

        private void fastObjectListViewLogs_FormatRow(object sender, FormatRowEventArgs e)
        {
            Log log = e.Model as Log;

            if(log == null)
            {
                return;
            }

            e.Item.ForeColor = log.GetLogColor();
        }

        private void fastObjectListViewPokemon_FormatCell(object sender, FormatCellEventArgs e)
        {
            PokemonData pokemonData = (PokemonData)e.Model;

            if(e.Column == olvColumnPokemonCandy)
            {
                int candy = (int)olvColumnPokemonCandy.GetValue(pokemonData);
                int candyToEvolve = (int)olvColumnCandyToEvolve.GetValue(pokemonData);

                if (candyToEvolve > 0)
                {
                    if (candy >= candyToEvolve)
                    {
                        e.SubItem.ForeColor = Color.Green;
                    }
                    else
                    {
                        e.SubItem.ForeColor = Color.Red;
                    }
                }
            }
            else if (e.Column == olvColumnPerfectPercent)
            {
                double perfectPercent = Convert.ToDouble(olvColumnPerfectPercent.GetValue(pokemonData));

                if(perfectPercent >= 93)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (perfectPercent >= 86)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
            else if (e.Column == olvColumnAttack)
            {
                if(pokemonData.IndividualAttack >= 13)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if(pokemonData.IndividualAttack >= 11)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else if(pokemonData.IndividualAttack >= 9)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }

            }
            else if (e.Column == olvColumnDefense)
            {
                if (pokemonData.IndividualDefense >= 13)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (pokemonData.IndividualDefense >= 11)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else if (pokemonData.IndividualDefense >= 9)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
            else if (e.Column == olvColumnStamina)
            {
                if (pokemonData.IndividualStamina >= 13)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (pokemonData.IndividualStamina >= 11)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else if (pokemonData.IndividualStamina >= 9)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
        }

        private void upgradeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to upgrade {0} pokemon?", fastObjectListViewPokemon.SelectedObjects.Count), "Confirmation", MessageBoxButtons.YesNo);

            if(result != DialogResult.Yes)
            {
                return;
            }

            fastObjectListViewPokemon.SetObjects(_manager.Pokemon);
        }

        private async void transferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to transfer {0} pokemon?", fastObjectListViewPokemon.SelectedObjects.Count), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            contextMenuStripPokemonDetails.Enabled = false;

            MethodResult managerResult = await _manager.TransferPokemon(fastObjectListViewPokemon.SelectedObjects.Cast<PokemonData>());

            await UpdateDetails();

            contextMenuStripPokemonDetails.Enabled = true;

            fastObjectListViewPokemon.SetObjects(_manager.Pokemon);

            MessageBox.Show("Finished transferring pokemon");
        }

        private async void evolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to evolve {0} pokemon?", fastObjectListViewPokemon.SelectedObjects.Count), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            contextMenuStripPokemonDetails.Enabled = false;

            await _manager.EvolvePokemon(fastObjectListViewPokemon.SelectedObjects.Cast<PokemonData>());
            
            await UpdateDetails();

            contextMenuStripPokemonDetails.Enabled = true;

            fastObjectListViewPokemon.SetObjects(_manager.Pokemon);

            MessageBox.Show("Finished evolving pokemon");
        }

        private void contextMenuStripPokemonDetails_Opening(object sender, CancelEventArgs e)
        {
            /*
            if(_manager.IsRunning)
            {
                contextMenuStripPokemonDetails.Enabled = false;
            }
            else
            {
                contextMenuStripPokemonDetails.Enabled = true;
            }*/
        }

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageLogs)
            {
                _totalLogs = _manager.TotalLogs;

                tabPageLogs.Text = "Logs";
            }
            else if(tabControlMain.SelectedTab == tabPagePokemon)
            {
                fastObjectListViewPokemon.SetObjects(_manager.Pokemon);
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int total = fastObjectListViewLogs.SelectedObjects.Count;

            if(total == 0)
            {
                return;
            }

            string copiedMessage = String.Join(Environment.NewLine, fastObjectListViewLogs.SelectedObjects.Cast<Log>().Select(x => x.ToString()));

            Clipboard.SetText(copiedMessage);
        }

        private async void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    MethodResult result = await _manager.ExportLogs(sfd.FileName);

                    if(result.Success)
                    {
                        MessageBox.Show("Logs exported");
                    }
                }
            }
        }

        private async void setFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            favoriteToolStripMenuItem.Enabled = false;

            await _manager.FavoritePokemon(fastObjectListViewPokemon.SelectedObjects.Cast<PokemonData>(), true);
            await UpdateDetails();

            favoriteToolStripMenuItem.Enabled = true;

            fastObjectListViewPokemon.SetObjects(_manager.Pokemon);

            MessageBox.Show("Finished favoriting pokemon");

        }

        private async void setUnfavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            favoriteToolStripMenuItem.Enabled = false;

            await _manager.FavoritePokemon(fastObjectListViewPokemon.SelectedObjects.Cast<PokemonData>(), false);
            await UpdateDetails();

            favoriteToolStripMenuItem.Enabled = true;

            fastObjectListViewPokemon.SetObjects(_manager.Pokemon);

            MessageBox.Show("Finished unfavoriting pokemon");

        }

        private async void recycleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Amount to recycle", "Set recycle amount");
            int amount = 0;

            if(String.IsNullOrEmpty(data) || !Int32.TryParse(data, out amount) || amount <= 0)
            {
                return;
            }

            foreach(ItemData item in fastObjectListViewInventory.SelectedObjects)
            {
                int toDelete = amount;

                if(amount > item.Count)
                {
                    toDelete = item.Count;
                }

                await _manager.RecycleItem(item, toDelete);

                await Task.Delay(500);
            }

            await _manager.UpdateInventory();

            fastObjectListViewInventory.SetObjects(_manager.Items);

            MessageBox.Show("Finished recycling items");
        }
    }
}
