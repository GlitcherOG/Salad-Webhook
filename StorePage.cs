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
            toolStripLabel1.Text = Program.ProductTracking.Count.ToString() + " Items in Wishlist";
            for (int i = 0; i < Program.ProductTracking.Count; i++)
            {
                toolStripDropDownButton1.DropDownItems.Add(Program.ProductTracking[i].name);
            }
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
            if (toolStripButton1.Text == "Add To Wishlist")
            {
                toolStripButton1.Text = "Remove From Wishlist";
            }
            else
            {
                toolStripButton1.Text = "Add To Wishlist";
            }
            string uri = "https://app-api.salad.io/api/v1/rewards/";
            string temp = chromiumWebBrowser1.Address.Substring(29);
            Program.AddProduct(uri + temp);
        }

        public void UpdateButton()
        {
            toolStripLabel1.Text = Program.ProductTracking.Count.ToString() + " Items in Wishlist";
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
                            toolStripButton1.Text = "Remove From Wishlist";
                        });
                    }
                }
                if (!test)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        toolStripButton1.Text = "Add To Wishlist";
                    });
                }
            }
            this.Invoke((MethodInvoker)delegate
            {
                    toolStripDropDownButton1.DropDownItems.Clear();
            });
            for (int i = 0; i < Program.ProductTracking.Count; i++)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripDropDownButton1.DropDownItems.Add(Program.ProductTracking[i].name);
                });
            }
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Program.ProductTracking = new List<GameData>();
            Program.WishlistCheck = new List<bool>();
            Program.AmmountCheck = new List<string>();
            Program.ProductDataSaving.Save();
            UpdateButton();
        }

        private void toolStripDropDownButton1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            for (int i = 0; i < Program.ProductTracking.Count; i++)
            {
                if(e.ClickedItem.Text== Program.ProductTracking[i].name)
                {
                    chromiumWebBrowser1.Load("https://app.salad.io/rewards/"+ Program.ProductTracking[i].id);
                }
            }
        }
    }
}
