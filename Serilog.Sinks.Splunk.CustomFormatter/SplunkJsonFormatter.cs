using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Serilog.Sinks.Splunk.CustomFormatter
{
    public class SplunkJsonFormatter : ITextFormatter
    {
        private static readonly JsonValueFormatter ValueFormatter = new JsonValueFormatter(typeTagName: "$type");
        private string _suffix;
        private SplunkLogSettings _splunkSettings { get; }

        /// <summary>
        /// Construct a <see cref="SplunkJsonFormatter"/>.
        /// </summary>
        public SplunkJsonFormatter(SplunkLogSettings splunkSettings)
        {
            this._splunkSettings = splunkSettings;
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null)
                throw new ArgumentNullException(nameof(logEvent));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            FillLogSplunkStone(logEvent, output);
        }

        private void FillLogSplunkStone(LogEvent logEvent, TextWriter output)
        {
            LogSplunkMapper logData = ArrangeLogData(logEvent);
            DefaultSuffixPropertiesSplunk(logData);
            FormatPropertiesSplunk(logEvent: logEvent, output: output, logData: logData);
        }

        private LogSplunkMapper ArrangeLogData(LogEvent logEvent)
        {
            var time = (long)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var index = _splunkSettings.Index;
            var sourceType = _splunkSettings.SourceType;
            var productCompany = _splunkSettings.ProductCompany;
            var productVersion = _splunkSettings.ProductVersion;
            var processName = _splunkSettings.ProcessName;
            var application = _splunkSettings.Application;
            var machine = Environment.MachineName;

            var logSplunkMapper = new LogSplunkMapper();
            var message = logEvent.RenderMessage(null);
            logSplunkMapper.Time = time;
            logSplunkMapper.Host = machine?.ToString();
            logSplunkMapper.Source = application?.ToString();
            logSplunkMapper.SourceType = sourceType?.ToString();
            logSplunkMapper.Index = index?.ToString();

            logSplunkMapper.Event.ProcessName = processName;
            logSplunkMapper.Event.ProductCompany = productCompany;
            logSplunkMapper.Event.ProductName = application;
            logSplunkMapper.Event.ProductVersion = productVersion;
            logSplunkMapper.Event.Severity = logEvent.Level.ToString();
            logSplunkMapper.Event.Message = message.Replace(@"""", "") ?? logSplunkMapper.Event.ProcessName;

            return logSplunkMapper;
        }

        private void DefaultSuffixPropertiesSplunk(LogSplunkMapper mapper)
        {
            var suffixWriter = new StringWriter();
            suffixWriter.Write("}"); // Terminates "event"

            if (!string.IsNullOrWhiteSpace(mapper.Source))
            {
                suffixWriter.Write(",\"source\":");
                JsonValueFormatter.WriteQuotedJsonString(mapper.Source, suffixWriter);
            }

            if (!string.IsNullOrWhiteSpace(mapper.SourceType))
            {
                suffixWriter.Write(",\"sourcetype\":");
                JsonValueFormatter.WriteQuotedJsonString(mapper.SourceType, suffixWriter);
            }

            if (!string.IsNullOrWhiteSpace(mapper.Host))
            {
                suffixWriter.Write(",\"host\":");
                JsonValueFormatter.WriteQuotedJsonString(mapper.Host, suffixWriter);
            }

            if (!string.IsNullOrWhiteSpace(mapper.Index))
            {
                suffixWriter.Write(",\"index\":");
                JsonValueFormatter.WriteQuotedJsonString(mapper.Index, suffixWriter);
            }

            suffixWriter.Write('}'); // Terminates the types of Splunk
            _suffix = suffixWriter.ToString();
        }

        public void FormatPropertiesSplunk(LogEvent logEvent, TextWriter output, LogSplunkMapper logData)
        {

            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.Write("{\"time\":\"");
            output.Write(logData.Time);
            output.Write("\",\"event\":{");

            if (!string.IsNullOrWhiteSpace(logData.Event.Message))
            {
                output.Write("\"Message\":");
                JsonValueFormatter.WriteQuotedJsonString(logData.Event.Message, output);
            }

            if (!string.IsNullOrWhiteSpace(logData.Event.ProcessName))
            {
                output.Write(",\"ProcessName\":");
                JsonValueFormatter.WriteQuotedJsonString(logData.Event.ProcessName, output);
            }

            if (!string.IsNullOrWhiteSpace(logData.Event.ProductCompany))
            {
                output.Write(",\"ProductCompany\":");
                JsonValueFormatter.WriteQuotedJsonString(logData.Event.ProductCompany, output);
            }

            if (!string.IsNullOrWhiteSpace(logData.Event.ProductName))
            {
                output.Write(",\"ProductName\":");
                JsonValueFormatter.WriteQuotedJsonString(logData.Event.ProductName, output);
            }

            if (!string.IsNullOrWhiteSpace(logData.Event.ProductVersion))
            {
                output.Write(",\"ProductVersion\":");
                JsonValueFormatter.WriteQuotedJsonString(logData.Event.ProductVersion, output);
            }

            if (!string.IsNullOrWhiteSpace(logData.Event.Severity))
            {
                output.Write(",\"Severity\":");
                JsonValueFormatter.WriteQuotedJsonString(logData.Event.Severity, output);
            }

            string[] aditionalDataToBeRemoved = { "SplunkIndex", "ProductCompany", "ProductVersion", "ProcessName" };
            var propertiesFiltered = logEvent.Properties.Where(p => !aditionalDataToBeRemoved.Contains(p.Key)).ToDictionary(d => d.Key, d => d.Value);

            WriteProperties(propertiesFiltered, output, logEvent.Exception);

            output.WriteLine(_suffix);
        }

        private void WriteProperties(Dictionary<string, LogEventPropertyValue> properties, TextWriter output, Exception exception)
        {
            output.Write(",\"AdditionalData\":{");

            var precedingDelimiter = "";

            if (properties.Count != 0)
            {
                foreach (var property in properties)
                {
                    output.Write(precedingDelimiter);
                    precedingDelimiter = ",";

                    JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                    output.Write(':');
                    ValueFormatter.Format(property.Value, output);
                }
            }

            WriterException(exception, precedingDelimiter, output);

            output.Write('}');
        }

        private void WriterException(Exception exception, string precedingDelimiter, TextWriter output)
        {
            if (exception != null)
            {
                output.Write(precedingDelimiter);

                output.Write("\"Exception\":{");

                JsonValueFormatter.WriteQuotedJsonString("Message", output);
                output.Write(':');
                JsonValueFormatter.WriteQuotedJsonString(exception.Message, output);

                output.Write(",");

                JsonValueFormatter.WriteQuotedJsonString("StackTrace", output);
                output.Write(':');
                JsonValueFormatter.WriteQuotedJsonString(exception.StackTrace, output);

                output.Write('}');
            }
        }
    }
}
