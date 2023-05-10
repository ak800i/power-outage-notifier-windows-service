﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace PowerOutageNotifier
{
    public class UserDataReader
    {
        /// <summary>
        /// Example file structure:
        /// 
        /// Friendly Name,Chat ID,District Name,Street Name
        /// PositiveTest,123456,Палилула,САВЕ МРКАЉА
        /// </summary>
        readonly static private string csvFilePath = @"C:\userdata.csv";

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

        public static void Write()
        {
            using (var writer = new StreamWriter(csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(Service1.userData);
            }
        }
    }
}