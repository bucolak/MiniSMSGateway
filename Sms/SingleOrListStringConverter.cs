using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sms
{
    public class SingleOrListStringConverter : JsonConverter<List<string>>
    {
        public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                return string.IsNullOrWhiteSpace(value) ? new List<string>() : new List<string> { value};
            }
            else if(reader.TokenType == JsonTokenType.StartArray)
            {
                var list = new List<string>();
                while(reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    
                    if(reader.TokenType == JsonTokenType.String)
                    {
                        var value = reader.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                            list.Add(value);
                    }
                    
                }
                return list;
            }
            throw new JsonException("The `to` field must be a string or a string array.");    
        }
        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
