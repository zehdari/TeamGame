namespace ECS.Resources;
public class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Handles [x, y] format
            reader.Read();
            float x = reader.GetSingle();
            reader.Read();
            float y = reader.GetSingle();
            reader.Read(); // EndArray
            return new Vector2(x, y);
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Handles { "X": x, "Y": y } format
            float x = 0, y = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                string propertyName = reader.GetString();
                reader.Read();

                if (propertyName == MAGIC.JSON_PARSING.X)
                    x = reader.GetSingle();
                else if (propertyName == MAGIC.JSON_PARSING.Y)
                    y = reader.GetSingle();
            }
            return new Vector2(x, y);
        }

        throw new JsonException("Invalid format for Vector2.");
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(MAGIC.JSON_PARSING.X, value.X);
        writer.WriteNumber(MAGIC.JSON_PARSING.Y, value.Y);
        writer.WriteEndObject();
    }
}
