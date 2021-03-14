namespace WindowsFormsApp1
{
    partial class LoginLogout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginLogout));
            this.WebBrowserPage = new CefSharp.WinForms.ChromiumWebBrowser();
            this.SuspendLayout();
            // 
            // WebBrowserPage
            // 
            this.WebBrowserPage.ActivateBrowserOnCreation = false;
            this.WebBrowserPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WebBrowserPage.Location = new System.Drawing.Point(0, 0);
            this.WebBrowserPage.Name = "WebBrowserPage";
            this.WebBrowserPage.Size = new System.Drawing.Size(370, 513);
            this.WebBrowserPage.TabIndex = 0;
            this.WebBrowserPage.AddressChanged += new System.EventHandler<CefSharp.AddressChangedEventArgs>(this.WebBrowserPage_AddressChanged);
            // 
            // LoginLogout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 513);
            this.Controls.Add(this.WebBrowserPage);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginLogout";
            this.Text = "Webpage";
            this.ResumeLayout(false);

        }

        #endregion

        private CefSharp.WinForms.ChromiumWebBrowser WebBrowserPage;
    }
}

