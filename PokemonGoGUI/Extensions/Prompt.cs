using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PokemonGoGUI.Extensions
{
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 150;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            Label textLabel = new Label() { AutoSize = true, Location = new Point(13, 13), Text = text };
            TextBox textBox = new TextBox() { Location = new Point(16, 41), Size = new Size(186, 20) };
            textBox.Text = defaultValue;


            Button confirmation = new Button() { Text = "OK", DialogResult = DialogResult.OK, Size = new Size(75, 23), Location = new Point(15, 67) };
            Button cancel = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Size = new Size(75, 23), Location = new Point(127, 67) };

            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { prompt.Close(); };

            prompt.AcceptButton = confirmation;
            prompt.AutoScaleDimensions = new SizeF(6F, 13F);
            prompt.AutoScaleMode = AutoScaleMode.Font;
            prompt.ClientSize = new Size(214, 100);
            prompt.Text = caption;

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                return textBox.Text;
            }

            return String.Empty;
        }
    }

    public static class AutoCompletePrompt
    {
        public static string ShowDialog(string text, string caption, IEnumerable<string> autoCompleteValues, string defaultValue = "")
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 150;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            Label textLabel = new Label() { AutoSize = true, Location = new Point(13, 13), Text = text };
            TextBox textBox = new TextBox() { Location = new Point(16, 41), Size = new Size(186, 20) };
            textBox.Text = defaultValue;
            textBox.AutoCompleteCustomSource.AddRange(autoCompleteValues.ToArray());
            textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            Button confirmation = new Button() { Text = "OK", DialogResult = DialogResult.OK, Size = new Size(75, 23), Location = new Point(15, 67) };
            Button cancel = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Size = new Size(75, 23), Location = new Point(127, 67) };

            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { prompt.Close(); };

            prompt.AcceptButton = confirmation;
            prompt.AutoScaleDimensions = new SizeF(6F, 13F);
            prompt.AutoScaleMode = AutoScaleMode.Font;
            prompt.ClientSize = new Size(214, 100);
            prompt.Text = caption;

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                return textBox.Text;
            }

            return String.Empty;
        }
    }
}
