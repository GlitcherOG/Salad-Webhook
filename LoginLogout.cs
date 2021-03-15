using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class LoginLogout : Form
    {
        public LoginLogout()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            WebBrowserPage.Load("https://app-api.salad.io/login");
        }

        private void WebBrowserPage_AddressChanged(object sender, CefSharp.AddressChangedEventArgs e)
        {
            if(WebBrowserPage.Address=="https://app-api.salad.io/login-complete")
            {
                Program.LoadEarnings();
                this.Invoke((MethodInvoker)delegate
                {
                    // close the form on the forms thread
                    this.Close();
                });
            }
        }
    }
}
