namespace StockWise.Helpers
{
    public class EmailSettings
    {
        public string From { get; set; }
        public string FromName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
