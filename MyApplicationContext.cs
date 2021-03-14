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

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            //Here, you can do stuff if the tray icon is doubleclicked
            TrayIcon.ShowBalloonTip(10000);
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
}