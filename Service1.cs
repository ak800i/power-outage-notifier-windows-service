using HtmlAgilityPack;
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

        public static readonly List<UserData> userData = UserDataReader.ReadUserData();

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
                    CheckAndNotifyPowerOutage();
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

        public static void CheckAndNotifyPowerOutage()
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

                                SendMessageAsync(user.ChatId, $"Power outage will occurr in {daysLeftUntilOutage} days in {user.DistrictName}, {user.StreetName}.")
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
