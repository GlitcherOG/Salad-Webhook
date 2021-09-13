using CefSharp;
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
        DataTable dt = new DataTable();
        public StorePage()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            dt.Columns.Add("Name");
            dt.Columns.Add("Cost");
            dt.Columns[1].DataType = System.Type.GetType("System.Decimal");
            dt.Columns.Add("Remaining");
            toolStripButton1.Enabled = false;
            toolStripLabel1.Text = Program.ProductTracking.Count.ToString() + " Items in Wishlist";
            //for (int i = 0; i < Program.ProductTracking.Count; i++)
            //{
            //    toolStripDropDownButton1.DropDownItems.Add(Program.ProductTracking[i].name);
            //}
            chromiumWebBrowser1.Load("https://app.salad.io");
            chromiumWebBrowser1.MenuHandler = new ConextMenuHandler();
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
            dt.Clear();
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
                DataRow _ravi = dt.NewRow();
                _ravi["Name"] = Program.ProductTracking[i].name;
                _ravi["Cost"] = Program.ProductTracking[i].price;
                _ravi["Remaining"] = Program.ProductTracking[i].quantity;
                dt.Rows.Add(_ravi);
            }
            this.Invoke((MethodInvoker)delegate
            {
                dataGridView1.DataSource = dt;
            });
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
                string[] temp = e.ClickedItem.Text.Split('(');
                if (temp[0] == Program.ProductTracking[i].name)
                {
                    chromiumWebBrowser1.Load("https://app.salad.io/rewards/" + Program.ProductTracking[i].id);
                    dataGridView1.Visible = false;
                }
            }
        }

        private void toolStripDropDownButton1_DoubleClick(object sender, EventArgs e)
        {
            dataGridView1.Visible = !dataGridView1.Visible;
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            for (int i = 0; i < Program.ProductTracking.Count; i++)
            {
                if(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString()== Program.ProductTracking[i].name)
                {
                    chromiumWebBrowser1.Load("https://app.salad.io/rewards/" + Program.ProductTracking[i].id);
                }
            }
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }
    }

    public class ConextMenuHandler : IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
            model.AddItem(CefMenuCommand.Find, "Add To Wishlist");
            model.SetEnabledAt(0, !string.IsNullOrEmpty(parameters.LinkUrl)&&parameters.LinkUrl.Contains("https://app.salad.io/rewards/"));
        }

        public bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            if (commandId == CefMenuCommand.Find)
            {
                string Address = "https://app-api.salad.io/api/v1/rewards/" + parameters.LinkUrl.Substring(29);
                Program.AddProduct(Address);
                return true;
            }
            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
        }

        public bool RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}
