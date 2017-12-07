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
                LinkData = "https://discord.gg/rkm4xhX"
            });

            linkLabel1.Links.Add(new LinkLabel.Link
            {
                LinkData = "https://github.com/Furtif/GoManager"
            });
        }

        private void StartupForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ShowOnStartUp = checkBoxShowOnStartup.Checked;
        }

        private void LinkLabelDiscordChat_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SNATC29B4ZJD4");
        }
    }
}
