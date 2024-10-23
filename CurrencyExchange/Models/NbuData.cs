using Newtonsoft.Json;

namespace CurrencyExchange.Models
{
    ///This is a class that held to parse date that we get from NBU in not standart for Newtonsoft format
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string _dateFormat = "dd.MM.yyyy";

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dateString = (string)reader.Value;
            return DateTime.ParseExact(dateString, _dateFormat, System.Globalization.CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(_dateFormat));
        }
    }
    public class NbuData
    {
        public int Id { get; set; }
        public int r030 { get; set; }

        public string txt { get; set; }

        public float rate { get; set; }

        public string cc { get; set; }
        //additional field marker that show newton how to work with this field
        [JsonConverter(typeof(CustomDateTimeConverter))] 
        public DateTime exchangedate { get; set; }
    }
}
