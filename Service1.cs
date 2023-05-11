using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace PowerOutageNotifier
{
    public partial class Service1 : ServiceBase
    {
        private static readonly string telegramBotToken = ConfigReader.ReadBotToken();

        public static readonly List<UserData> userDataList = ConfigReader.ReadUserData();

        // URLs of the web page to scrape
        private static readonly List<string> powerOutageUrls = new List<string>
        {
            "http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_0_Iskljucenja.htm",
            "http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_1_Iskljucenja.htm",
            "http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_2_Iskljucenja.htm",
            "http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_3_Iskljucenja.htm",
        };

        private static readonly List<string> waterOutageUrls = new List<string>
        {
            "https://www.bvk.rs/planirani-radovi/",
        };

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            SendMessageAsync(userDataList.First().ChatId, $"Service running on {Environment.MachineName}").GetAwaiter().GetResult();
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Task.Run(CheckAndNotifyPowerOutage);
                        Task.Run(CheckAndNotifyWaterOutage);
                        Thread.Sleep(TimeSpan.FromHours(1));
                    }
                    catch (Exception)
                    {
                        // just continue
                    }
                }
            });
        }

        protected override void OnStop()
        {
            SendMessageAsync(userDataList.First().ChatId, $"Service stoppingon {Environment.MachineName}").GetAwaiter().GetResult();
        }

        private static async Task SendMessageAsync(long chatId, string message)
        {
            TelegramBotClient botClient = new TelegramBotClient(telegramBotToken);
            await botClient.SendTextMessageAsync(chatId, message);
        }

        public static void CheckAndNotifyPowerOutage()
        {
            foreach (string url in powerOutageUrls)
            {
                // Create a new HtmlWeb instance
                HtmlWeb web = new HtmlWeb();

                // Load the HTML document from the specified URL
                HtmlDocument document = web.Load(url);

                // Find the table rows in the document
                HtmlNodeCollection rows = document.DocumentNode.SelectNodes("//table//tr");

                // Iterate through the rows
                foreach (HtmlNode row in rows)
                {
                    // Find the cells in the current row
                    HtmlNodeCollection cells = row.SelectNodes("td");

                    // Check if the row has the correct number of cells
                    if (cells != null && cells.Count >= 3)
                    {
                        // Get the district name from the first cell
                        string district = cells[0].InnerText.Trim();

                        // Get the street name from the second cell
                        string streets = cells[2].InnerText.Trim();

                        foreach (var user in userDataList)
                        {
                            // Check if the street name occurs in the same row as the correct district name
                            if (district == user.DistrictName && streets.Contains(user.StreetName))
                            {
                                Console.WriteLine($"Power outage detected. {user.FriendlyName}, {user.DistrictName}, {user.StreetName}, {user.ChatId}");

                                int daysLeftUntilOutage = powerOutageUrls.IndexOf(url);

                                SendMessageAsync(user.ChatId, $"Power outage will occurr in {daysLeftUntilOutage} days in {user.DistrictName}, {user.StreetName}.")
                                    .GetAwaiter().GetResult();
                            }
                        }
                    }
                }
            }
        }

        public static void CheckAndNotifyWaterOutage()
        {
            foreach (string url in waterOutageUrls)
            {
                // Create a new HtmlWeb instance
                HtmlWeb web = new HtmlWeb();

                // Load the HTML document from the specified URL
                HtmlDocument document = web.Load(url);

                HtmlNodeCollection workNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'toggle_content')]");
                if (workNodes != null)
                {
                    foreach (HtmlNode workNode in workNodes)
                    {
                        string nodeText = workNode.InnerText;

                        foreach (var user in userDataList)
                        {
                            string declinationRoot = user.StreetName.Substring(0, user.StreetName.Length - 2);

                            // Check if the street name occurs in the same entry as the correct district name
                            if (nodeText.Contains(user.DistrictName) && nodeText.Contains(declinationRoot))
                            {
                                Console.WriteLine($"Water outage detected. {user.FriendlyName}, {user.DistrictName}, {user.StreetName}, {user.ChatId}");

                                SendMessageAsync(user.ChatId, $"Water outage might occurr in {user.DistrictName}, {user.StreetName}.\n{nodeText}")
                                    .GetAwaiter().GetResult();
                            }
                        }
                    }
                }
            }
        }

        public static void CheckAndNotifyUnplannedWaterOutage()
        {
            foreach (string url in waterOutageUrls)
            {
                // Create a new HtmlWeb instance
                HtmlWeb web = new HtmlWeb();

                // Load the HTML document from the specified URL
                HtmlDocument document = web.Load(url);

                HtmlNodeCollection divElements = document.DocumentNode.SelectNodes("//div[@class='toggle_content invers-color ' and @itemprop='text']");
                if (divElements != null)
                {
                    foreach (HtmlNode divElement in divElements)
                    {
                        // Find the ul element within each div element
                        HtmlNode ulElement = divElement.SelectSingleNode(".//ul");

                        if (ulElement != null)
                        {
                            // Iterate through each li element within the ul element
                            foreach (HtmlNode liElement in ulElement.Descendants("li"))
                            {
                                // Check for string occurrences
                                string text = liElement.InnerText;

                                foreach (var user in userDataList)
                                {
                                    // Example: Check for the string "example" in each li element
                                    if (text.Contains(user.DistrictName) && text.Contains(user.StreetName))
                                    {
                                        Console.WriteLine($"Water outage detected. {user.FriendlyName}, {user.DistrictName}, {user.StreetName}, {user.ChatId}");

                                        SendMessageAsync(user.ChatId, $"Water outage might be happening in {user.DistrictName}, {user.StreetName}.\n{text}")
                                            .GetAwaiter().GetResult();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
