using System.Text.Json;
using System.Text.Json.Serialization;

namespace DependencyTrackHelper.Api.Models;

internal sealed class DependencyTrackProjectDto
{
    public string Uuid { get; init; } = string.Empty;

    public string? Name { get; init; }

    public string? Version { get; init; }

    public bool Active { get; set; } = true;

    public bool IsLatest { get; set; }

    public string? Description { get; set; } = string.Empty;

    public string Classifier { get; init; } = string.Empty;

    public string CollectionLogic { get; init; } = string.Empty;

    [JsonConverter(typeof(TagsKeyValueListConverter))]
    public List<DependencyTrackTagKeyValue>? CollectionTag { get; init; } = null;


    [JsonConverter(typeof(TagsKeyValueListConverter))]
    public List<DependencyTrackTagKeyValue> Tags { get; init; } = [];

    [JsonConverter(typeof(UnixMillisecondsDateTimeOffsetConverter))]
    public DateTimeOffset? LastBomImport { get; init; }

    public DependencyTrackProjectReferenceDto? Parent { get; set; }
}

internal sealed class DependencyTrackProjectReferenceDto
{
    public string Uuid { get; init; } = string.Empty;
}

internal sealed class DependencyTrackTagKeyValue
{
    public string Key { get; init; } = string.Empty;

    public string? Value { get; init; }
}

internal sealed class DependencyTrackCreateProjectRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Version { get; init; }

    public bool Active { get; init; }

    public bool? IsLatest { get; init; }

    public string? Description { get; init; } = string.Empty;

    public string Classifier { get; init; } = string.Empty;

    public string CollectionLogic { get; init; } = string.Empty;

    public DependencyTrackProjectReferenceDto? Parent { get; init; }
}

internal sealed class DependencyTrackUpdateProjectRequest
{
    public string Uuid { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Version { get; init; }

    public bool Active { get; init; }

    public bool? IsLatest { get; init; }

    public string? Description { get; init; } = string.Empty;

    public DependencyTrackProjectReferenceDto? Parent { get; init; }
}

internal sealed class UnixMillisecondsDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var epochMilliseconds))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(epochMilliseconds);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (long.TryParse(value, out var parsedEpochMilliseconds))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(parsedEpochMilliseconds);
            }

            if (DateTimeOffset.TryParse(value, out var parsedDateTimeOffset))
            {
                return parsedDateTimeOffset;
            }
        }

        throw new JsonException("Unsupported DateTimeOffset JSON value.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteNumberValue(value.Value.ToUnixTimeMilliseconds());
    }
}

internal sealed class TagsKeyValueListConverter : JsonConverter<List<DependencyTrackTagKeyValue>>
{
    public override List<DependencyTrackTagKeyValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tags = new List<DependencyTrackTagKeyValue>();

        if (reader.TokenType == JsonTokenType.Null)
        {
            return tags;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                tags.Add(new DependencyTrackTagKeyValue
                {
                    Key = property.Name,
                    Value = property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString()
                        : property.Value.ToString()
                });
            }

            return tags;
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (!item.TryGetProperty("name", out var nameElement))
                {
                    continue;
                }

                var name = nameElement.GetString();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                string? value = null;
                if (item.TryGetProperty("value", out var valueElement))
                {
                    value = valueElement.ValueKind == JsonValueKind.String
                        ? valueElement.GetString()
                        : valueElement.ToString();
                }

                tags.Add(new DependencyTrackTagKeyValue
                {
                    Key = name,
                    Value = value
                });
            }

            return tags;
        }

        throw new JsonException("Unsupported tags JSON format.");
    }

    public override void Write(Utf8JsonWriter writer, List<DependencyTrackTagKeyValue> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            writer.WriteStartObject();
            writer.WriteString("name", item.Key);

            if (!string.IsNullOrWhiteSpace(item.Value))
            {
                writer.WriteString("value", item.Value);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
