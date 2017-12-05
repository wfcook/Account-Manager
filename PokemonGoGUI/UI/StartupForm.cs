using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PokemonGoGUI.UI
{
    public partial class StartupForm : Form
    {
        public bool ShowOnStartUp { get; set; }

        public StartupForm()
        {
            InitializeComponent();

            ShowOnStartUp = true;

            linkLabelDiscordChat.Links.Add(new LinkLabel.Link
            {
                LinkData = "https://discord.gg/4qn5gCf"
            });

            linkLabel1.Links.Add(new LinkLabel.Link
            {
                LinkData = "https://github.com/SL-x-TnT/GoManager"
            });
        }

        private void StartupForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ShowOnStartUp = checkBoxShowOnStartup.Checked;
        }

        private void linkLabelDiscordChat_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }
    }
}
