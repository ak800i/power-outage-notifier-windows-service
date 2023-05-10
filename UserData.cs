using CsvHelper.Configuration.Attributes;

namespace PowerOutageNotifier
{
    public class UserData
    {
        [Name("Friendly Name")]
        public string FriendlyName { get; set; }

        [Name("Chat ID")]
        public long ChatId { get; set; }

        [Name("District Name")]
        public string DistrictName { get; set; }

        [Name("Street Name")]
        public string StreetName { get; set; }
    }
}