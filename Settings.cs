using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
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

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
