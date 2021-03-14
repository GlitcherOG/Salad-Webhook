using CefSharp;
using CefSharp.OffScreen;
using Discord;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaladWebhook
{
    static class Program
    {
        //Web Adresses
        //https://app-api.salad.io/api/v1/profile/xp
        //https://app-api.salad.io/api/v2/reports/30-day-earning-history
        //https://app-api.salad.io/api/v1/profile/
        //https://app-api.salad.io/api/v1/account/
        //https://app-api.salad.io/api/v1/reward-vault
        //https://app-api.salad.io/api/v1/notification-banner
        //https://app-api.salad.io/api/v1/profile/referral-code
        //https://app-api.salad.io/api/v1/profile/selected-reward
        //https://app-api.salad.io/api/v1/profile/referrals

        static CefSharp.OffScreen.ChromiumWebBrowser chromiumWebBrowser1;
        static CefSharp.OffScreen.ChromiumWebBrowser chromiumWebBrowser2;
        public static SettingsSaveLoad Saving = new SettingsSaveLoad();
        public static int waittime = 1;
        public static bool postIfChange = true;
        public static string Webhook = "";
        static string username;
        static string Balance;
        static string lifetimeBalance;
        static string lifetimeXP;
        static string ReferalCode;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyApplicationContext());
        }


        public static void LoadEarnings()
        {
            Saving = SettingsSaveLoad.Load();
            if (Saving.webhook != "" && Saving.webhook != null)
            {
                waittime = (int)Saving.waitTimeMin;
                postIfChange = Saving.onlyIfNewPost;
                Webhook = Saving.webhook;
            }
            else
            {
                Form1 loginform = new Form1();
                loginform.Show();
                Settings Wsettings = new Settings();
                Wsettings.Show();
            }
            var settings = new CefSettings();
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaladWebHook\\CefSharp\\Cache");
            Cef.Initialize(settings);
            chromiumWebBrowser1 = new ChromiumWebBrowser();
            chromiumWebBrowser2 = new ChromiumWebBrowser();
            Task.Run(() => ProfileData());
            Task.Run(() => RefreshTimer());
        }

        private static async Task RefreshTimer()
        {
            while (true)
            {
                await Task.Delay(1000 * waittime * 60);
                if (username == null || username == "")
                {
                    await ProfileData().ConfigureAwait(false);
                }
                else
                {
                    await Refresh();
                    await CheckData().ConfigureAwait(false);
                }
            }
        }

        private static async Task Refresh()
        {
            string uri;
            uri = "https://app.salad.io/earn/summary";
            chromiumWebBrowser2.Load(uri);
            await Task.Delay(5000);
        }

        private static async Task LoadWebPage(string uri)
        {
            chromiumWebBrowser1.Load(uri);
            await Task.Delay(3000);
        }

        public static List<JsonDetails> LoadJson()
        {
            Task<string> task = Task.Run(() => chromiumWebBrowser1.GetSourceAsync());
            while (task == null)
            {
                task = Task.Run(() => chromiumWebBrowser1.GetSourceAsync());
            }
            string temp = task.Result.TrimStart("<html><head></head><body><pre style =\"word-wrap: break-word; white-space: pre-wrap;\">".ToCharArray());
            temp = temp.TrimEnd("}</pre></body></html>".ToCharArray());
            dynamic profile = temp;
            List<JsonDetails> temp2 = new List<JsonDetails>();
            //Need a custom Strip Program
            temp2 = StripJson(temp.Split(','));
            return temp2;
        }

        private static async Task ProfileData()
        {
            await Refresh();
            await LoadWebPage("https://app-api.salad.io/api/v1/profile/");
            List<JsonDetails> temp2 = LoadJson();
            for (int i = 0; i < temp2.Count; i++)
            {
                if (temp2[i].Line1.Contains("username"))
                {
                    username = temp2[i].Line2;
                }
            }
            if (username != null && username != "")
            {
                await LoadWebPage("https://app-api.salad.io/api/v1/profile/referral-code");
                temp2 = LoadJson();
                for (int i = 0; i < temp2.Count; i++)
                {
                    if (temp2[i].Line1.Contains("code"))
                    {
                        ReferalCode = temp2[i].Line2;
                    }
                }
                await CheckData();
            }
            else
            {
                MessageBox.Show("Please Login on the Salad Webpage.", "Error");
            }
        }

        private static async Task CheckData()
        {
            await LoadWebPage("https://app-api.salad.io/api/v1/profile/balance");
            bool test = false;
            List<JsonDetails> temp2 = LoadJson();
            for (int i = 0; i < temp2.Count; i++)
            {
                if (temp2[i].Line1.Contains("currentbalance"))
                {
                    if (Balance != temp2[i].Line2.Substring(0, temp2[i].Line2.IndexOf('.') + 4))
                    {
                        test = true;
                    }
                    Balance = temp2[i].Line2.Substring(0, temp2[i].Line2.IndexOf('.') + 4);
                }
                if (temp2[i].Line1.Contains("lifetimebalance"))
                {
                    lifetimeBalance = temp2[i].Line2.Substring(0, temp2[i].Line2.IndexOf('.') + 4);
                }
            }
            await LoadWebPage("https://app-api.salad.io/api/v1/profile/xp");
            temp2 = LoadJson();
            for (int i = 0; i < temp2.Count; i++)
            {
                if (temp2[i].Line1.Contains("lifetimexp"))
                {
                    lifetimeXP = temp2[i].Line2;
                }
            }
            if (!postIfChange || test && postIfChange)
            {
                if (Webhook != null && Webhook != "")
                {
                    var client = new DiscordWebhookClient(Webhook);

                    var embed = new EmbedBuilder
                    {
                        Title = username,
                    };
                    embed.Description = "Current Balance: $" + Balance + Environment.NewLine + "Lifetime Earnings: $" + lifetimeBalance + Environment.NewLine + "Livetime XP: " + lifetimeXP;
                    embed.Footer = new EmbedFooterBuilder { Text = "Referal Code: " + ReferalCode };
                    await client.SendMessageAsync(text: "", embeds: new[] { embed.Build() });
                }
                else
                {
                    MessageBox.Show("Please Check Settings and paste in a Discord Webhook", "Error");
                }
            }
        }
        public static List<JsonDetails> StripJson(string[] JsonFile)
        {
            List<JsonDetails> file = new List<JsonDetails>();
            for (int i = 0; i < JsonFile.Length; i++)
            {
                string[] Dump;
                string Dump2;
                List<string> Dump3 = new List<string>();
                JsonDetails temp = new JsonDetails();
                if (i == 0)
                {
                    Dump2 = JsonFile[i].Replace("{", "");
                    Dump = Dump2.Split('"', ':');
                }
                else
                {
                    Dump = JsonFile[i].Split('"', ':');
                }
                for (int a = 0; a < Dump.Length; a++)
                {
                    if (Dump[a] != null && Dump[a] != "" && Dump[a] != " ")
                    {
                        Dump3.Add(Dump[a]);
                    }
                }
                if (Dump3.Count >= 2)
                {
                    temp.Line1 = Dump3[0].ToLower().TrimEnd(' ').TrimStart(' ');
                    temp.Line2 = Dump3[1];
                    file.Add(temp);
                }
            }
            return file;
        }
        public struct JsonDetails
        {
            public string Line1;
            public string Line2;
        }

        public static bool GetLogin
        {
            get
            {
                string temp = System.IO.File.ReadAllText(@"C:\Users\User\AppData\Local\CefSharp\Cache\Cookies");
                if (temp.Contains("salad"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}

