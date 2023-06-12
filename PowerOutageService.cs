using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PowerOutageNotifier
{
    public partial class PowerOutageService : ServiceBase
    {
        private static readonly string telegramBotToken = ConfigReader.ReadBotToken();

        private static readonly TelegramBotClient botClient = new TelegramBotClient(telegramBotToken);

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

        private static readonly List<string> waterUnplannedOutageUrls = new List<string>
        {
            "https://www.bvk.rs/kvarovi-na-mrezi/",
        };

        public PowerOutageService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogAsync($"Service running on {Environment.MachineName}").GetAwaiter().GetResult();
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Task.Run(CheckAndNotifyPowerOutage);
                        Task.Run(CheckAndNotifyWaterOutage);
                        Task.Run(CheckAndNotifyUnplannedWaterOutage);
                        Task.Run(CheckAndNotifyParkingTickets);
                        Thread.Sleep(TimeSpan.FromHours(1));
                    }
                    catch (Exception ex)
                    {
                        // just continue
                        LogAsync($"Exception {ex}").GetAwaiter().GetResult();
                    }
                }
            });
        }

        protected override void OnStop()
        {
            LogAsync($"Service stopping on {Environment.MachineName}").GetAwaiter().GetResult();
        }

        private static async Task LogAsync(string message)
        {
            await botClient.SendTextMessageAsync(userDataList.First().ChatId, message);
        }

        private static async Task SendMessageAsync(long chatId, string message)
        {
            await botClient.SendTextMessageAsync(chatId, message);
        }

        private static async Task RecieveMessageAsync()
        {
            User me = await botClient.GetMeAsync();
            Console.WriteLine($"{me.Username} started");

            // start listening for incoming messages
            while (true)
            {
                //get incoming messages
                var updates = await botClient.GetUpdatesAsync();
                foreach (var update in updates)
                {
                    // send response to incoming message
                    Console.WriteLine($"{update}");
                    await LogAsync(update.ToString());
                }
            }
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
                            if (district == user.DistrictName
                                && streets.IndexOf(user.StreetName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                string streetWithNumber = streets.Substring(streets.IndexOf(user.StreetName, StringComparison.OrdinalIgnoreCase));
                                streetWithNumber = streetWithNumber.Substring(0, streets.IndexOf(','));

                                Console.WriteLine($"Power outage detected. {user.FriendlyName}, {user.DistrictName}, {streetWithNumber}, {user.ChatId}");

                                int daysLeftUntilOutage = powerOutageUrls.IndexOf(url);

                                SendMessageAsync(user.ChatId, $"Power outage will occur in {daysLeftUntilOutage} days in {user.DistrictName}, {streetWithNumber}.")
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
                            if (nodeText.IndexOf(user.DistrictName, StringComparison.OrdinalIgnoreCase) >= 0
                                && nodeText.IndexOf(declinationRoot, StringComparison.OrdinalIgnoreCase) >= 0)
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
            foreach (string url in waterUnplannedOutageUrls)
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
                                    // Example: Check for the string "example" in each li element (case-insensitive)
                                    if (text.IndexOf(user.DistrictName, StringComparison.OrdinalIgnoreCase) >= 0
                                        && text.IndexOf(user.StreetName, StringComparison.OrdinalIgnoreCase) >= 0)
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

        public static void CheckAndNotifyParkingTickets()
        {
            string licensePlate = "BG677CJ";
            string url = "https://www.parking-servis.co.rs/lat/edpk";
            string searchKeyword = "NEMA EVIDENTIRANE ELEKTRONSKE";

            // Set up ChromeDriver
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless"); // Run in headless mode (without opening a browser window)
            using (IWebDriver driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl(url);

                // Find the input field and enter the license plate
                IWebElement inputElement = driver.FindElement(By.CssSelector("input[name='fine']"));
                inputElement.Clear();
                inputElement.SendKeys(licensePlate);

                // Find and click the submit button
                IWebElement submitButton = driver.FindElement(By.CssSelector("button[type='submit']"));
                submitButton.Click();

                // Wait for the presence of the result message
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                By resultLocator = By.CssSelector("div.entry-text.no-edpk-message");
                IWebElement resultElement = wait.Until(ExpectedConditions.ElementIsVisible(resultLocator));

                // Check if the result element contains the keyword
                if (resultElement.Text.Contains(searchKeyword))
                {
                    Console.WriteLine($"The keyword '{searchKeyword}' was found on the website.");
                }
                else
                {
                    SendMessageAsync(
                        userDataList.Where(user => user.FriendlyName.Contains("Ajanko")).First().ChatId,
                        $"There is a parking fine at {url}")
                        .GetAwaiter().GetResult();

                    SendMessageAsync(
                        userDataList.Where(user => user.FriendlyName.Contains("Ajanko")).First().ChatId,
                        licensePlate)
                        .GetAwaiter().GetResult();
                }
            }
        }
    }
}
