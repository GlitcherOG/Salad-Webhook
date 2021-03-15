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

namespace WindowsFormsApp1
{
    static class Program
    {
        //Web Adresses
        //https://github.com/ravibpatel/AutoUpdater.NET
        //https://app-api.salad.io/api/v1/profile/xp
        //https://app-api.salad.io/api/v2/reports/30-day-earning-history
        //https://app-api.salad.io/api/v1/profile/
        //https://app-api.salad.io/api/v1/account/
        //https://app-api.salad.io/api/v1/reward-vault
        //https://app-api.salad.io/api/v1/notification-banner
        //https://app-api.salad.io/api/v1/profile/referral-code
        //https://app-api.salad.io/api/v1/profile/selected-reward
        //https://app-api.salad.io/api/v1/profile/referrals
        //https://app-api.salad.io/api/v2/storefront
        //https://app-api.salad.io/api/v1/rewards/
        //https://app-api.salad.io/login
        //https://app-api.salad.io/logout
        //Xp Works in a ratio of 1:3:7:13:23:38. Double current and add previous one

        static CefSharp.OffScreen.ChromiumWebBrowser chromiumWebBrowser1;
        static CefSharp.OffScreen.ChromiumWebBrowser chromiumWebBrowser2;
        static NoficationIcon Icon;
        public static List<GameData> ProductTracking = new List<GameData>();
        public static List<GameData> StoreProducts = new List<GameData>();
        public static SettingsSaveLoad Saving = new SettingsSaveLoad();
        static ProductDataSaveLoad ProductDataSaving = new ProductDataSaveLoad();
        public static int waittime = 15;
        public static bool postIfChange = true;
        public static bool postIfStoreChange = false;
        public static string Webhook = "";
        public static string username = "";
        static string Balance;
        static string OldBalance = "0";
        static string lifetimeBalance;
        static string lifetimeXP;
        static string ReferalCode;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var settings = new CefSettings();
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaladWebHook\\CefSharp\\Cache");
            Cef.Initialize(settings);
            LoadSavedData();
            Icon = new NoficationIcon();
            Application.Run(Icon);
        }
        public static void LoadSavedData()
        {
            Saving = SettingsSaveLoad.Load();
            ProductDataSaving = ProductDataSaveLoad.Load();
            if (Saving != null)
            {
                waittime = (int)Saving.waitTimeMin;
                postIfChange = Saving.onlyIfNewPost;
                Webhook = Saving.webhook;
                postIfStoreChange = Saving.postIfStoreChange;
            }
            else
            {
                Saving = new SettingsSaveLoad();
                LoginLogout loginform = new LoginLogout();
                loginform.Show();
                Settings Wsettings = new Settings();
                Wsettings.Show();
            }
            if(ProductDataSaving != null)
            {
                ProductTracking = ProductDataSaving.TrackedProducts;
                StoreProducts = ProductDataSaving.AllProducts;
            }
        }
        public static void LoadEarnings()
        {
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
                    //await Refresh();
                    await CheckData();
                }
                //await Task.Delay(10000);
                await CheckPrices();
            }
        }

        private static async Task AddProductA(string Address)
        {
            bool Test = false;
            for (int i = 0; i < ProductTracking.Count; i++)
            {
                if (Address == ProductTracking[i].id)
                {
                    Test = true;
                    ProductTracking.RemoveAt(i);
                }
            }
            if (!Test)
            {
                string uri = "https://app-api.salad.io/api/v1/rewards/";
                chromiumWebBrowser2.Load(uri + Address);
                await Task.Delay(3000);
                ProductTracking.Add(LoadGameData());
                ProductDataSaving.Save();
            }
        }

            public static void AddProduct(string Address)
            {
                Task.Run(() => AddProductA(Address));
            }

        private static async Task CheckPrices()
        {
            for (int i = 0; i < ProductTracking.Count; i++)
            {
                string uri = "https://app-api.salad.io/api/v1/rewards/";
                chromiumWebBrowser2.Load(uri + ProductTracking[i].id);
                await Task.Delay(3000);
                GameData data = LoadGameData();
                if(ProductTracking[i].price!=data.price)
                {
                    var client = new DiscordWebhookClient(Webhook);

                    var embed = new EmbedBuilder
                    {
                        Title = data.name,
                    };
                    float temp = float.Parse(ProductTracking[i].price) - float.Parse(data.price);
                    string tempbal;
                    if (temp > 0)
                    {
                        embed.Color = Color.Green;
                        tempbal = " ($+" + Math.Round(temp, 4).ToString() + ")";
                    }
                    else
                    {
                        embed.Color = Color.Red;
                        tempbal = " ($" + Math.Round(temp, 4).ToString() + ")";
                    }
                    embed.Description = data.description;
                    embed.ImageUrl = data.image;
                    embed.AddField("Price", data.price + " " + tempbal);
                    embed.Timestamp = DateTimeOffset.Now;
                    await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO Shop", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
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
            await Task.Delay(2000);
        }

        public static List<JsonDetails> LoadJson()
        {
            Task<string> task = Task.Run(() => chromiumWebBrowser1.GetSourceAsync());
            while (task == null)
            {
                task = Task.Run(() => chromiumWebBrowser1.GetSourceAsync());
            }
            //string temp = "{\"category\":\"gamingGiftcard\",\"checkoutTerms\":[\"US Blizzard Account\"],\"coverImage\":\" / api / v1 / reward - images / 9f04eca5 - e29a - 4890 - 85b3 - 4626096e169b\",\"description\":\"Get the latest in Blizzard items and games through Salad. USA Blizzard account only.\",\"developerName\":\"\",\"headline\":\"\",\"id\":\"0a600b4e - 42d5 - 4720 - a575 - 12b71c532586\",\"image\":\" / api / v1 / reward - images / 57542b2e - 3818 - 4aa9 - 9430 - 851e8dd8a02f\",\"images\":[],\"name\":\"Blizzard Gift Card - $20 \",\"platform\":\"unknown\",\"price\":21.0,\"publisherName\":\"\",\"tags\":[\"Gaming Gift Cards\"],\"videos\":[]}";
            string temp = task.Result.TrimStart("<html><head></head><body><pre style =\"word-wrap: break-word; white-space: pre-wrap;\">".ToCharArray());
            temp = temp.TrimEnd("</pre></body></html>".ToCharArray());
            List<JsonDetails> temp2 = new List<JsonDetails>();
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
                Icon.UpdateTooltip();
                await CheckData();
            }
            else
            {
                Icon.UpdateTooltip();
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
                    if (OldBalance == "0")
                    {
                        OldBalance = Balance;
                    }
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
                    string tempbal = "";
                    if (OldBalance != Balance)
                    {
                        float temp = float.Parse(Balance) - float.Parse(OldBalance);
                        OldBalance = Balance;
                        if (temp > 0)
                        {
                            embed.Color = Color.Green;
                            tempbal = " ($+" + Math.Round(temp, 4).ToString() + ")";
                        }
                        else
                        {
                            embed.Color = Color.Red;
                            tempbal = " ($" + Math.Round(temp, 4).ToString() + ")";
                        }
                    }
                    embed.Description = "Current Balance: $" + Balance + tempbal + Environment.NewLine + "Lifetime Earnings: $" + lifetimeBalance + Environment.NewLine + "Livetime XP: " + lifetimeXP;
                    embed.Footer = new EmbedFooterBuilder { Text = "Referal Code: " + ReferalCode };
                    await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
                }
                else
                {
                    MessageBox.Show("Please Check Settings and Paste in a Discord Webhook", "Error");
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
                if (i == JsonFile.Length)
                {
                    Dump2 = JsonFile[i].Replace("}", "");
                    Dump = Dump2.Split('"', ':');
                }
                else
                {
                    Dump = JsonFile[i].Split('"', ':');
                }
                for (int a = 0; a < Dump.Length; a++)
                {
                    if (Dump[a] != null && Dump[a] != "" && Dump[a] != " " && Dump[a] != "{" && Dump[a] != "}")
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

        public static GameData LoadGameData()
        {
            List<JsonDetails> temp = LoadJson();
            GameData data = new GameData();
            for (int i = 0; i < temp.Count; i++)
            {
                if (temp[i].Line1 == "id")
                {
                    data.id = temp[i].Line2;
                }
                if (temp[i].Line1 == "name")
                {
                    data.name = temp[i].Line2;
                }
                if (temp[i].Line1 == "price")
                {
                    data.price = temp[i].Line2;
                }
                if (temp[i].Line1 == "category")
                {
                    data.category = temp[i].Line2;
                }
                if (temp[i].Line1 == "image")
                {
                    data.image = temp[i].Line2;
                }
                if (temp[i].Line1 == "description")
                {
                    data.description = temp[i].Line2;
                }
            }
            return data;
        }
    }
}

public struct GameData
{
    public string id;
    public string name;
    public string price;
    public string description;
    public string category;
    public string image;
}
public struct JsonDetails
{
    public string Line1;
    public string Line2;
}
