namespace ENVIRIO.Data
{
    public class SmtpSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string SenderEmail { get; set; } = "";
        public string SenderName { get; set; } = "ENVIRIO Monitoring";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
