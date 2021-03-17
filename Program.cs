using CefSharp;
using CefSharp.OffScreen;
using Discord;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

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
        //Xp Works in a ratio of 1:3:7:13:23:38. Add previous one and one before and add 3

        static CefSharp.OffScreen.ChromiumWebBrowser chromiumWebBrowser1;
        static NoficationIcon Icon;
        public static List<GameData> ProductTracking = new List<GameData>();
        public static List<GameData> StoreProducts = new List<GameData>();
        public static List<int> FruitXp = new List<int>();
        public static SettingsSaveLoad Saving = new SettingsSaveLoad();
        public static ProductDataSaveLoad ProductDataSaving = new ProductDataSaveLoad();
        static List<JsonDetails> Temp = new List<JsonDetails>();
        public static int waittime = 15;
        public static bool postIfChange = true;
        public static bool postIfStoreChange = false;
        public static string Webhook = "";
        public static string username = "";
        static string Balance = "0.000";
        static string OldBalance = "0";
        static string lifetimeBalance;
        static string lifetimeXP;
        static string ReferalCode;

        [STAThread]
        static void Main()
        {
            //GenerateFruit();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var settings = new CefSettings();
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaladWebHook\\CefSharp\\Cache");
            Cef.Initialize(settings);
            LoadSavedData();
            Icon = new NoficationIcon();
            Application.Run(Icon);
        }
        //public static void GenerateFruit()
        //{
        //    FruitXp.Add(60);
        //    FruitXp.Add((FruitXp[0] * 2) + FruitXp[0]);
        //    for (int i = 2; i < 19; i++)
        //    {
        //        FruitXp.Add((FruitXp[i - 1] + FruitXp[i - 2] + ());
        //    }
        //}
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
            if (ProductDataSaving != null)
            {
                ProductTracking = ProductDataSaving.TrackedProducts;
                StoreProducts = ProductDataSaving.AllProducts;
            }
            else
            {
                ProductDataSaving = new ProductDataSaveLoad();
            }
        }
        public static void LoadEarnings()
        {
            chromiumWebBrowser1 = new ChromiumWebBrowser();
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
                    await Task.Run(() => ProfileData());
                }
                else
                {
                    await Refresh();
                    await CheckData();
                }
                //await Task.Delay(30000);
                await CheckPrices();
                await CheckStore();
                //chromiumWebBrowser1 = new ChromiumWebBrowser();
                //chromiumWebBrowser2 = new ChromiumWebBrowser();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static async Task AddProductA(string Address)
        {
            bool Test = false;
            for (int i = 0; i < ProductTracking.Count; i++)
            {
                if (Address.TrimStart("https://app-api.salad.io/api/v1/rewards/".ToCharArray()) == ProductTracking[i].id)
                {
                    Test = true;
                    ProductTracking.RemoveAt(i);
                    NoficationIcon.storeform.UpdateButton();
                    ProductDataSaving.Save();
                }
            }
            if (!Test)
            {
                string temp2 = await LoadWebPage(Address);
                ProductTracking.Add(LoadGameData(temp2));
                //GameData temp = ProductTracking[0];
                //temp.price = "100";
                //ProductTracking[0] = temp;
                NoficationIcon.storeform.UpdateButton();
                ProductDataSaving.Save();
            }
        }

        public static void AddProduct(string Address)
        {
            Task.Run(() => AddProductA(Address));
        }

        private static async Task CheckStore()
        {
            if (postIfStoreChange)
            {
                string uri = "https://app-api.salad.io/api/v1/rewards/";
                string temp3 = LoadWebPage(uri, true).Result;
                temp3 = temp3.Replace("[{", "");
                temp3 = temp3.Replace("}]", "}");
                string[] temp2 = temp3.Split(new string[] { ",{" }, StringSplitOptions.None);
                List<GameData> Store = new List<GameData>();
                for (int i = 0; i < temp2.Length; i++)
                {
                    Store.Add(LoadGameData("{" + temp2[i]));
                }
                string StorePriceChanges = "";
                string StoreProductChanges = "";
                for (int i = 0; i < Store.Count; i++)
                {
                    bool test = false;
                    for (int a = 0; a < StoreProducts.Count; a++)
                    {
                        if (Store[i].id == StoreProducts[a].id)
                        {
                            test = true;
                            if (Store[i].price != StoreProducts[a].price)
                            {
                                float temp = float.Parse(Store[i].price) - float.Parse(StoreProducts[a].price);
                                string tempbal;
                                if (temp > 0)
                                {
                                    tempbal = " ($+" + Math.Round(temp, 4).ToString() + ")";
                                }
                                else
                                {
                                    tempbal = " ($" + Math.Round(temp, 4).ToString() + ")";
                                }
                                StorePriceChanges += Store[i].name + ": $" + Store[i].price + " ($" + Store[i].price + ")" + Environment.NewLine;
                            }
                            break;
                        }
                    }
                    if (!test)
                    {
                        StoreProductChanges += Store[i].name + ": $" + Store[i].price + "" + Environment.NewLine;
                    }
                }
                var client = new DiscordWebhookClient(Webhook);

                var embed = new EmbedBuilder
                {
                    Title = "Store Changes",
                };
                if (StorePriceChanges != "" || StoreProductChanges != "")
                {
                    if (StorePriceChanges != "" && StorePriceChanges.Length <= 2048)
                    {
                        if (StorePriceChanges.Length <= 2048)
                        {
                            embed.AddField("Price Changes", StorePriceChanges);
                        }
                        else
                        {
                            embed.AddField("Price Changes", Regex.Matches(StorePriceChanges, "\n").Count.ToString() + " Items have changed prices");
                        }
                    }
                    if (StoreProductChanges != "")
                    {
                        if (StoreProductChanges.Length <= 2048)
                        {
                            embed.AddField("Product Changes", StoreProductChanges);
                        }
                        else
                        {
                            embed.AddField("Product Changes", Regex.Matches(StoreProductChanges, "\n").Count.ToString() + " Items have been added");
                        }
                    }
                    embed.Timestamp = DateTimeOffset.Now;
                    await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO Shop", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
                    StoreProducts = Store;
                    ProductDataSaving.Save();
                }
            }
        }

        private static async Task CheckPrices()
        {
            if (ProductTracking.Count != 0)
            {
                for (int i = 0; i < ProductTracking.Count; i++)
                {
                    string temp2 = await LoadWebPage("https://app-api.salad.io/api/v1/rewards/" + ProductTracking[i].id);
                    GameData data = LoadGameData(temp2);
                    if (ProductTracking[i].price != data.price)
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
                            tempbal = " (-$" + Math.Round(temp, 4).ToString() + ")";
                        }
                        else
                        {
                            embed.Color = Color.Red;
                            tempbal = " (+$" + Math.Round(temp - temp + temp, 4).ToString() + ")";
                        }
                        embed.ImageUrl = "https://app-api.salad.io" + data.image;
                        embed.Description = "Price: $" + data.price + " " + tempbal;
                        embed.AddField("Description", data.description);
                        embed.Timestamp = DateTimeOffset.Now;
                        ProductTracking[i] = data;
                        embed.ThumbnailUrl = "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png";
                        await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
                    }
                }
                ProductDataSaving.Save();
            }
        }

        private static async Task Refresh()
        {
            string uri = "https://app.salad.io/earn/summary";
            chromiumWebBrowser1.Load(uri);
            await Task.Delay(1000);
            while (chromiumWebBrowser1.IsLoading)
            {
                await Task.Delay(100);
            }
            await Task.Delay(3000);
        }

        private static async Task<string> LoadWebPage(string uri, bool wait = false)
        {
            chromiumWebBrowser1.Load(uri);
            //if(wait)
            //{
            //    await Task.Delay(5000);
            //}
            await Task.Delay(1000);
            while (chromiumWebBrowser1.IsLoading)
            {
                await Task.Delay(100);
            }
            string temp;
            Task<string> task = chromiumWebBrowser1.GetSourceAsync();
            while (task == null && task.Result == "")
            {
                task = Task.Run(() => chromiumWebBrowser1.GetSourceAsync());
            }
            temp = task.Result.TrimStart("<html><head></head><body><pre style =\"word-wrap: break-word; white-space: pre-wrap;\">".ToCharArray());
            temp = temp.TrimEnd("</pre></body></html>".ToCharArray());
            return temp;
        }

        public static dynamic LoadJson(string Json = null)
        {
            string temp = Json;
            dynamic temp2 = new List<JsonDetails>();
            temp = temp.TrimStart("<html><head></head><body><pre style =\"word-wrap: break-word; white-space: pre-wrap;\">".ToCharArray());
            temp = temp.TrimEnd("</pre></body></html>".ToCharArray());
            temp2 = JValue.Parse(Json);
            return temp2;
        }

        private static async Task ProfileData()
        {
            await Refresh();
            string temp = await LoadWebPage("https://app-api.salad.io/api/v1/profile/");
            dynamic temp2 = LoadJson(temp);
            username = temp2.username;
            if (username != null && username != "")
            {
                temp = await LoadWebPage("https://app-api.salad.io/api/v1/profile/referral-code");
                temp2 = LoadJson(temp);
                ReferalCode = temp2.code;
                Icon.UpdateTooltip();
                await Task.Run(() => CheckData());
                //await CheckData();
            }
            else
            {
                Icon.UpdateTooltip();
            }
        }

        private static async Task CheckData()
        {
            string temp3 = await LoadWebPage("https://app-api.salad.io/api/v1/profile/balance");
            bool test = false;
            dynamic temp2 = LoadJson(temp3);
            string temp4 = temp2.currentBalance;
            if (Balance != temp4)
            {
                test = true;
            }
            Balance = temp4;
            if (OldBalance == "0")
            {
                OldBalance = Balance;
            }
            lifetimeBalance = temp2.lifetimeBalance;
            temp3 = await LoadWebPage("https://app-api.salad.io/api/v1/profile/xp");
            temp2 = LoadJson(temp3);
            lifetimeXP = temp2.lifetimeXp;
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
                    embed.Description = "Current Balance: $" + Balance.Substring(0,5) + tempbal + Environment.NewLine + "Lifetime Earnings: $" + lifetimeBalance.Substring(0, 5) + Environment.NewLine + "Livetime XP: " + lifetimeXP;
                    embed.Footer = new EmbedFooterBuilder { Text = "Referal Code: " + ReferalCode };
                    embed.ThumbnailUrl = "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png";
                    await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
                }
                else
                {
                    MessageBox.Show("Please Check Settings and Paste in a Discord Webhook", "Error");
                }
            }
        }
        //public static dynamic StripJson(string JsonFile)
        //{
        //    return JValue.Parse(JsonFile);
        //    List<JsonDetails> file = new List<JsonDetails>();
        //    string[] Dump;
        //    string Dump2;
        //    List<string> Dump3 = new List<string>();
        //    JsonDetails temp = new JsonDetails();
        //    Dump2 = JsonFile.Replace("{", "");
        //    Dump2 = Dump2.Replace("}", "");
        //    Dump = Dump2.Split(new string[] { ",\"" }, StringSplitOptions.None);
        //    string boo = "";
        //    for (int a = 0; a < Dump.Length; a++)
        //    {
        //        if (Dump[a].Contains(":"))
        //        {
        //            if (Dump[a].Contains("[") && !Dump[a].Contains("]"))
        //            {
        //                boo += Dump[a];
        //            }
        //            else
        //            {
        //                Dump3.Add(Dump[a]);
        //            }
        //        }
        //        else
        //        {
        //            boo += Dump[a];
        //            if (Dump[a].Contains("]"))
        //            {
        //                Dump3.Add(boo);
        //                boo = "";
        //            }
        //        }
        //    }
        //    for (int i = 0; i < Dump3.Count; i++)
        //    {
        //        string[] Lines = Dump3[i].Replace("\"", "").Split(':');
        //        temp.Line1 = Lines[0].ToLower();
        //        temp.Line2 = Lines[1].Replace("\n", Environment.NewLine);
        //        file.Add(temp);
        //    }
        //    return file;
        //}

        public static GameData LoadGameData(string Json)
        {
            dynamic temp = LoadJson(Json);
            string temp2 = temp.description;
            GameData data = new GameData();
            data.id = temp.id;
            data.name = temp.name;
            data.price = temp.price;
            data.image = temp.coverImage;
            data.description = temp2.Replace("<div>", "");
            data.description = data.description.Replace("<p>", "");
            data.description = data.description.Replace("<br />", "");
            data.description = data.description.Replace("</p>", "");
            data.description = data.description.Replace("</p>", "");
            data.description = data.description.Replace("<ul>", "");
            data.description = data.description.Replace("<li>", "");
            data.description = data.description.Replace("</li>", "");
            data.description = data.description.Replace("</ul>", "");
            data.description = data.description.Replace("</div>", "");
            return data;
        }
    }
}
[Serializable]
public struct GameData
{
    public string id;
    public string name;
    public string price;
    public string description;
    public string category;
    public string image;
}
[Serializable]
public struct JsonDetails
{
    public string Line1;
    public string Line2;
}
