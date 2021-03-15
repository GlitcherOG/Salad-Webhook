using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class StorePage : Form
    {
        public StorePage()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            toolStripButton1.Enabled = false;
            chromiumWebBrowser1.Load("https://app.salad.io");
        }

        private void chromiumWebBrowser1_AddressChanged(object sender, CefSharp.AddressChangedEventArgs e)
        {
            if (chromiumWebBrowser1.Address.Contains("https://app.salad.io/rewards/"))
            {
                bool test = false;
                for (int i = 0; i < Program.ProductTracking.Count; i++)
                {
                    if (chromiumWebBrowser1.Address.TrimStart("https://app.salad.io/rewards/".ToCharArray()) == Program.ProductTracking[i].id)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            toolStripButton1.Text = "Stop Tracking Price";
                        });
                    }
                }
                if (!test)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        toolStripButton1.Text = "Track Price";
                    });
                }
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripButton1.Enabled = true;
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripButton1.Enabled = false;
                });
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string uri = "https://app-api.salad.io/api/v1/rewards/";
            Program.AddProduct(uri + chromiumWebBrowser1.Address.TrimStart("https://app.salad.io/rewards/".ToCharArray()));
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Program.ProductTracking = new List<GameData>();
        }
    }
}
