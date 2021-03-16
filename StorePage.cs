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
            toolStripLabel1.Text = "Tracking Prices For " + Program.ProductTracking.Count.ToString() +" Products";
            chromiumWebBrowser1.Load("https://app.salad.io");
        }

        private void chromiumWebBrowser1_AddressChanged(object sender, CefSharp.AddressChangedEventArgs e)
        {
            UpdateButton();
            if (chromiumWebBrowser1.Address.Contains("https://app.salad.io/rewards/"))
            {
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
            if (toolStripButton1.Text == "Track Price")
            {
                toolStripButton1.Text = "Stop Tracking Price";
            }
            else
            {
                toolStripButton1.Text = "Track Price";
            }
            string uri = "https://app-api.salad.io/api/v1/rewards/";
            string temp = chromiumWebBrowser1.Address.Substring(29);
            Program.AddProduct(uri + temp);
        }

        public void UpdateButton()
        {
            toolStripLabel1.Text = "Tracking Prices For " + Program.ProductTracking.Count.ToString() + " Products";
            if (chromiumWebBrowser1.Address.Contains("https://app.salad.io/rewards/"))
            {
                bool test = false;
                for (int i = 0; i < Program.ProductTracking.Count; i++)
                {
                    if (chromiumWebBrowser1.Address.TrimStart("https://app.salad.io/rewards/".ToCharArray()) == Program.ProductTracking[i].id)
                    {
                        test = true;
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
            }
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Program.ProductTracking = new List<GameData>();
            Program.ProductDataSaving.Save();
            UpdateButton();
        }
    }
}
