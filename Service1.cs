using System;
using System.Collections.Generic;
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
        readonly long telegramChatIdAjanko = 67903798;
        readonly string streetName = "ПАРИСКЕ КОМУНЕ";

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
                    CheckAndNotifyPowerOutage(streetName);
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
        /// <param name="streetName">Name of the street for which to check the outage</param>
        private void CheckAndNotifyPowerOutage(string streetName)
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
                    webClient.DownloadString("http://www.epsdistribucija.rs/planirana-iskljucenja-beograd/Dan_3_Iskljucenja.htm")
                };

                foreach (string webpage in webpages)
                {
                    if (webpage.Contains(streetName))
                    {
                        int daysLeftUntilOutage = webpages.IndexOf(webpage);

                        SendMessageAsync(telegramChatIdAjanko, $"Power outage in {daysLeftUntilOutage} days.")
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
    }
}
