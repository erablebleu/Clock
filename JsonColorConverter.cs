using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace Clock;

internal class JsonColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => (Color)ColorConverter.ConvertFromString(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, Color c, JsonSerializerOptions options)
    {
        writer.WriteStringValue(c.ToString());
    }
}