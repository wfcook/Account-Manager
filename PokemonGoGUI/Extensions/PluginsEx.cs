using PokemonGoGUI.GoManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGoGUI.Extensions
{
    internal interface IPlugin
    {
        string PluginName { get; }
        List<IPlugin> Plugins { get; set; }
        List<Manager> Managers { get; set; }
        Task Load(Manager managers);
        Task Load(List<Manager> managers);
    }

    internal class ExportModel
    {
        public List<IPlugin> Plugins { get; set; }
        public List<Manager> Managers { get; set; }
    }

    public class PluginsEx
    {
        private ExportModel ExportModel { get; set; }

        public async Task LoadPlugins()
        {
            this.ExportModel = new ExportModel();

            await Task.Run(delegate
            {
                this.ExportModel.Plugins = new List<IPlugin>();
                if (Directory.Exists("Plugins"))
                {
                    string[] files = Directory.GetFiles("Plugins", "*.dll");
                    int num = files.Length;
                    if (num != 0)
                    {
                        int num2 = 25 / num;
                        if (num2 == 0)
                        {
                            num2 = 1;
                        }
                        HashSet<string> hashSet = new HashSet<string>();
                        int num3 = 0;
                        while (true)
                        {
                            if (num3 >= num)
                            {
                                break;
                            }
                            string arg = files[num3].Split('\\').Last();
                            try
                            {
                                if (num3 < 25)
                                {
                                    this.UpdateStatus($"Loading plugin {arg}", num3 * num2 + 75);
                                }
                                byte[] rawAssembly = File.ReadAllBytes(files[num3]);
                                Assembly assembly = Assembly.Load(rawAssembly);
                                Type[] types = assembly.GetTypes();
                                foreach (Type type in types)
                                {
                                    if (!type.IsInterface && !type.IsAbstract)
                                    {
                                        type.GetInterface("IPlugin");
                                        if (type.BaseType.Name == "IPlugin")
                                        {
                                            IPlugin plugin2 = (IPlugin)Activator.CreateInstance(type);
                                            if (hashSet.Add(plugin2.PluginName))
                                            {
                                                this.ExportModel.Plugins.Add(plugin2);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Exception ex2 = ex;
                                if (ex2.InnerException != null)
                                {
                                    ex2 = ex2.InnerException;
                                }
                                MessageBox.Show($"Failed to load plugin {arg}. Ex: {ex2.Message}", "Plugin error");
                            }
                            num3++;
                        }
                    }
                }
            });
            this.UpdateStatus("Loading plugin data ...", 95);
            if (this.ExportModel.Managers == null)
            {
                this.ExportModel.Managers = new List<Manager>();
            }
            foreach (IPlugin plugin3 in this.ExportModel.Plugins)
            {
                await plugin3.Load(this.ExportModel.Managers);
            }
        }

        private void UpdateStatus(string v1, int v2)
        {
           //Logger infos here... not released yet.
        }
    }
}
