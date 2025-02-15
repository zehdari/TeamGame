namespace ECS.Resources;

public class ColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            byte[] colorValues = new byte[4];
            int index = 0;
            
            reader.Read(); // Move past StartArray '['
            // Read until we get to EndArray ']'
            while (reader.TokenType != JsonTokenType.EndArray && index < 4)
            {
                colorValues[index] = (byte)reader.GetInt32();
                index++;
                reader.Read();
            }
            
            return new Color(colorValues[0], colorValues[1], colorValues[2], colorValues[3]);
        }
        
        throw new JsonException("Expected color array in format [R, G, B, A]");
    }

    // Have to add a write for converters even if we aren't going to use it
    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.R);
        writer.WriteNumberValue(value.G);
        writer.WriteNumberValue(value.B);
        writer.WriteNumberValue(value.A);
        writer.WriteEndArray();
    }
}