using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookingOrchestrationApi
{
    /// <summary>
    /// Custom DateTime converter to handle ISO 8601 date format (YYYY-MM-DD)
    /// </summary>
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        private const string DateFormat = "yyyy-MM-dd";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrEmpty(stringValue))
                return DateTime.MinValue;
            
            // Try parsing as ISO 8601 date (YYYY-MM-DD)
            if (DateTime.TryParseExact(stringValue, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
            {
                return result;
            }

            // Fallback to general parsing
            if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var fallbackResult))
            {
                return fallbackResult;
            }
            
            throw new JsonException($"Unable to convert \"{stringValue}\" to DateTime. Expected format: {DateFormat}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
        }
    }
}
