using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class WebPage : Form
    {
        public WebPage()
        {
            InitializeComponent();
            WebBrowserPage.Load("https://app.salad.io/earn/summary");
        }
    }
}
