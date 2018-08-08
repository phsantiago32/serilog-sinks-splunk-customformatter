namespace Serilog.Sinks.Splunk.CustomFormatter
{
    /// <summary>
    /// Splunk log settings
    /// </summary>
    public class SplunkLogSettings
    {
        public string ServerURL { get; set; }
        public string Token { get; set; }
        public string Index { get; set; }
        public string SourceType { get; set; }
        public string ProductCompany { get; set; }
        public string ProductVersion { get; set; }
        public string ProcessName { get; set; }
        public string Application { get; set; }
    }
}
