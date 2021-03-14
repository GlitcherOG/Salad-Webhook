using System;
using System.Drawing;
using System.Windows.Forms;

namespace SaladWebhook
{
    class MyApplicationContext : ApplicationContext
    {
        //Component declarations
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem LoginAndOut;
        private ToolStripMenuItem Settings;
        private ToolStripMenuItem Exit;
        public static Form1 loginform;
        public static Settings settings;

        public MyApplicationContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            TrayIcon.Visible = true;
            AutoClosingMessageBox.Show("Webhook has been started.", "Started", 5000);
        }

        private void InitializeComponent()
        {
            Program.LoadEarnings();
            TrayIcon = new NotifyIcon();

            //TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            //TrayIcon.BalloonTipText =
            //  "I noticed that you double-clicked me! What can I do for you?";
            //TrayIcon.BalloonTipTitle = "You called Master?";
            TrayIcon.Text = "Salad Webhook";


            //The icon is added to the project resources.
            //Here, I assume that the name of the file is 'TrayIcon.ico'
            TrayIcon.Icon = Properties.Resources.Icon1;

            ////Optional - handle doubleclicks on the icon:
            //TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            //Optional - Add a context menu to the TrayIcon:
            TrayIconContextMenu = new ContextMenuStrip();
            TrayIconContextMenu.SuspendLayout();

            Exit = new ToolStripMenuItem();
            this.Exit.Text = "Exit";
            this.Exit.Size = new Size(152, 22);
            this.Exit.Click += new EventHandler(this.Exit_Click);

            Settings = new ToolStripMenuItem();
            this.Settings.Name = "Settings";
            this.Settings.Size = new Size(152, 22);
            this.Settings.Text = "Settings";
            this.Settings.Click += new EventHandler(this.Settings_Click);

            LoginAndOut = new ToolStripMenuItem();
            this.LoginAndOut.Name = "Salad Webpage";
            this.LoginAndOut.Size = new Size(152, 22);
            this.LoginAndOut.Text = "Salad Webpage";
            this.LoginAndOut.Click += new EventHandler(this.Login_Click);

            // 
            // TrayIconContextMenu
            // 
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {
            this.LoginAndOut, Settings, Exit});
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            this.TrayIconContextMenu.Size = new Size(153, 70);

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            CefSharp.Cef.Shutdown();
            Application.Exit();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            settings = new Settings();
            settings.Show();
        }

        private void Login_Click(object sender, EventArgs e)
        {
            loginform = new Form1();
            loginform.Show();
        }
    }

    public class AutoClosingMessageBox
    {
        System.Threading.Timer _timeoutTimer;
        string _caption;
        AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            using (_timeoutTimer)
                MessageBox.Show(text, caption);
        }
        public static void Show(string text, string caption, int timeout)
        {
            new AutoClosingMessageBox(text, caption, timeout);
        }
        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
        const int WM_CLOSE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }
}