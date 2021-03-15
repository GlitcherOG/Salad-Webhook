using System;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace WindowsFormsApp1
{
    public partial class Settings : Form
    {
        string file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\SaladWeb.lnk";
        public Settings()
        {
            InitializeComponent();
            numericUpDown1.Value = Program.waittime;
            checkBox1.Checked = Program.postIfChange;
            checkBox2.Checked = Program.postIfStoreChange;
            textBox1.Text = Program.Webhook;
            if (System.IO.File.Exists(file))
            {
                checkBox3.Checked = true;
            }
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            Program.waittime = (int)numericUpDown1.Value;
            Program.postIfChange = checkBox1.Checked;
            Program.Webhook = textBox1.Text;
            Program.postIfStoreChange = checkBox2.Checked;
            Program.Saving.Save();
            if (checkBox3.Checked)
            {
                WshShell wsh = new WshShell();
                IWshShortcut shortcut = wsh.CreateShortcut(file) as IWshRuntimeLibrary.IWshShortcut;
                shortcut.TargetPath = Application.ExecutablePath;
                shortcut.Save();
            }
            else
            {
                if (System.IO.File.Exists(file))
                {
                    System.IO.File.Delete(file);
                }
            }
            this.Close();
        }
    }
}
