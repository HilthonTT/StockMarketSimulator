using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SharedKernel;
using System.Reflection;

namespace Infrastructure.Outbox;

public sealed class DomainEventConverter(Assembly assembly) : JsonConverter<IDomainEvent>
{
    public override IDomainEvent? ReadJson(
        JsonReader reader,
        Type objectType,
        IDomainEvent? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);
        string? typeNameWithAssembly = jsonObject["$type"]?.ToString();

        if (string.IsNullOrWhiteSpace(typeNameWithAssembly))
        {
            throw new JsonSerializationException("Type information not found in JSON.");
        }

        string? typeName = typeNameWithAssembly.Contains(',')
             ? typeNameWithAssembly.Split(',')[0].Trim()
             : typeNameWithAssembly.Trim();

        Type? type = assembly.GetTypes().FirstOrDefault(t => t.FullName == typeName);

        if (type is null)
        {
            throw new JsonSerializationException($"Could not resolve type: {typeName}");
        }

        // Use a new serializer WITHOUT this converter to prevent recursion
        var safeSerializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.All
        };

        safeSerializer.Converters.Clear(); // Remove DomainEventConverter

        return (IDomainEvent)jsonObject.ToObject(type, safeSerializer)!;
    }

    public override void WriteJson(JsonWriter writer, IDomainEvent? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var jsonObject = new JObject();

        foreach (var property in value.GetType().GetProperties())
        {
            jsonObject[property.Name] = JToken.FromObject(property.GetValue(value)!, serializer);
        }

        jsonObject.WriteTo(writer);
    }
}
