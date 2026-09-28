using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleCity.Core.JsonConverters;

public sealed class PersonIdJsonConverter : JsonConverter<PersonId>
{
    public override PersonId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString() ?? throw new JsonException("Invalid PersonId value");
        return new PersonId(Guid.Parse(s));
    }

    public override void Write(Utf8JsonWriter writer, PersonId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.ToString("N"));
    }

    public override PersonId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString() ?? throw new JsonException("Invalid PersonId property name");
        return new PersonId(Guid.Parse(s));
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, PersonId value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.Value.ToString("N"));
    }
}

public sealed class HouseholdIdJsonConverter : JsonConverter<HouseholdId>
{
    public override HouseholdId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString() ?? throw new JsonException("Invalid HouseholdId value");
        return new HouseholdId(Guid.Parse(s));
    }

    public override void Write(Utf8JsonWriter writer, HouseholdId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.ToString("N"));
    }

    public override HouseholdId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString() ?? throw new JsonException("Invalid HouseholdId property name");
        return new HouseholdId(Guid.Parse(s));
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, HouseholdId value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.Value.ToString("N"));
    }
}
