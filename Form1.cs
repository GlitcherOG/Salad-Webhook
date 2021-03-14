using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaladWebhook
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string uri;
            uri = "https://app.salad.io/earn/summary";
            chromiumWebBrowser1.Load(uri);
        }
    }
}
