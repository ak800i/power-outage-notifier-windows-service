﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace PowerOutageNotifier
{
    public partial class Service1 : ServiceBase
    {
        private static readonly string telegramBotToken = "6101873824:AAERozc6BX-Im46S5-fc_SPl9qzAmrNBRUA";

        private static readonly List<UserData> userData = new List<UserData>()
            {
                new UserData()
                {
                    FriendlyName = "Ajanko",
                    ChatId = 67903798,
                    DistrictName = "Нови Београд",
                    StreetName = "ПАРИСКЕ КОМУНЕ",
                }
            };

        // URLs of the web page to scrape
        private static readonly List<string> urls = new List<string>
        {
            "http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_0_Iskljucenja.htm",
            "http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_1_Iskljucenja.htm",
            "http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_2_Iskljucenja.htm",
            "http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_3_Iskljucenja.htm",
        };

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            while (true)
            {
                try
                {
                    CheckAndNotifyPowerOutageV2();
                    Thread.Sleep(TimeSpan.FromHours(1));
                }
                catch (Exception ex)
                {
                        // just continue
                }
            }
        }

        protected override void OnStop()
        {
        }

        public static async Task SendMessageAsync(long chatId, string message)
        {
            TelegramBotClient botClient = new TelegramBotClient(telegramBotToken);
            await botClient.SendTextMessageAsync(chatId, message);
        }

        /// <summary>
        /// Checks for power outage and sends a notification.
        /// </summary>
        private void CheckAndNotifyPowerOutage()
        {
            using (System.Net.WebClient webClient = new System.Net.WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                // The old and correct website
                List<string> webpages = new List<string>
                {
                    webClient.DownloadString("http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_0_Iskljucenja.htm"),
                    webClient.DownloadString("http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_1_Iskljucenja.htm"),
                    webClient.DownloadString("http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_2_Iskljucenja.htm"),
                    webClient.DownloadString("http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_3_Iskljucenja.htm"),
                };

                foreach (string webpage in webpages)
                {
                    if (webpage.Contains(userData.First().StreetName))
                    {
                        int daysLeftUntilOutage = webpages.IndexOf(webpage);

                        SendMessageAsync(userData.First().ChatId, $"Power outage in {daysLeftUntilOutage} days.")
                            .GetAwaiter().GetResult();
                    }
                }

                // The new website which always lacks data
                /*
                string newWebsite = webClient.DownloadString("http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_0_Iskljucenja.htm");
                if (newWebsite.Contains(streetName))
                {
                    SendMessageAsync(telegramChatIdAjanko, $"Power outage in UNDEFINED days.")
                        .GetAwaiter().GetResult();
                }
                */
            }
        }

        private static void CheckAndNotifyPowerOutageV2()
        {
            foreach (string url in urls)
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

                        foreach (var user in userData)
                        {
                            // Check if the street name occurs in the same row as the correct district name
                            if (district == user.DistrictName && streets.Contains(user.StreetName))
                            {
                                Console.WriteLine($"Outage detected. {user.FriendlyName}, {user.DistrictName}, {user.StreetName}, {user.ChatId}");

                                int daysLeftUntilOutage = urls.IndexOf(url);

                                SendMessageAsync(user.ChatId, $"Power outage in {daysLeftUntilOutage} days.")
                                    .GetAwaiter().GetResult();

                                break; // You can exit the loop if the match is found
                            }
                        }
                    }
                }
            }
        }
    }
}
