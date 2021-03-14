using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaladWebhook
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            numericUpDown1.Value = Program.waittime;
            checkBox1.Checked = Program.postIfChange;
            textBox1.Text = Program.Webhook;
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            Program.waittime = (int)numericUpDown1.Value;
            Program.postIfChange = checkBox1.Checked;
            Program.Webhook = textBox1.Text;
            Program.Saving.Save();
        }
    }
}
