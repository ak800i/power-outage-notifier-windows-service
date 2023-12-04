using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace PowerOutageNotifier
{
    public class ConfigReader
    {
        /// <summary>
        /// Example file structure:
        /// 
        /// Friendly Name,Chat ID,District Name,Street Name
        /// PositiveTest,123456,Палилула,САВЕ МРКАЉА
        /// </summary>
        readonly static private string csvFilePath = @"E:\Code\power-outage-notifier\userdata.csv";
        readonly static private string botTokenFilePath = @"E:\Code\power-outage-notifier\bot-token.txt";

        public static List<UserData> ReadUserData()
        {
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                List<UserData> userDataList = csv.GetRecords<UserData>().ToList();

                // Use the userDataList as needed
                return userDataList;
            }
        }

        /// <summary>
        /// Example file structure:
        /// 
        /// 123456:AAAAAAA
        /// </summary>
        public static string ReadBotToken() =>
            File.ReadAllText(botTokenFilePath);
    }
}
