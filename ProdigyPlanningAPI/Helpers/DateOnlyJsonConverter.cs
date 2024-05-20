using Newtonsoft.Json;
using System.Globalization;

namespace ProdigyPlanningAPI.Helpers
{
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string DateFormat = "dd/MM/yyyy";

        public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return DateOnly.ParseExact((string)reader.Value, DateFormat, CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
        }
    }
}
