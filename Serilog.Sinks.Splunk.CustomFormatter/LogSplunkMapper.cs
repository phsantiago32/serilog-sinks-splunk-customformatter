using Newtonsoft.Json;

namespace Serilog.Sinks.Splunk.CustomFormatter
{
    /// <summary>
    /// Oficial Data Contract Defined By Stone Team Splunk.
    /// </summary>
    [JsonObject]
    public class LogSplunkMapper
    {
        public LogSplunkMapper()
        {
            Event = new EventLogSplunK();
        }

        [JsonProperty(PropertyName = "time")]
        public long Time { get; set; }

        [JsonProperty(PropertyName = "host")]
        public string Host { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "sourcetype")]
        public string SourceType { get; set; }

        [JsonProperty(PropertyName = "index")]
        public string Index { get; set; }

        [JsonProperty(PropertyName = "event")]
        public EventLogSplunK Event { get; set; }
    }

    public class EventLogSplunK
    {
        [JsonProperty(PropertyName = "AdditionalData")]
        public object AdditionalData { get; set; }
        public string Message { get; set; }
        public string ProcessName { get; set; }
        public string ProductCompany { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public string Severity { get; set; }
    }
}
