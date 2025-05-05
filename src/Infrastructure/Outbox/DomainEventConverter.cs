using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SharedKernel;
using System.Reflection;
using System.Collections.Concurrent;

namespace Infrastructure.Outbox;

/// <summary>
/// Custom JSON converter to handle serialization and deserialization of <see cref="IDomainEvent"/> types.
/// </summary>
/// <param name="assembly">The assembly where domain event types are defined.</param>
public sealed class DomainEventConverter(Assembly assembly) : JsonConverter<IDomainEvent>
{
    private static readonly ConcurrentDictionary<string, Type?> TypeCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    /// <inheritdoc />
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

        Type? type = GetOrAddMessageType(typeName)
            ?? throw new JsonSerializationException($"Could not resolve type: {typeName}");

        // Use a new serializer WITHOUT this converter to prevent recursion
        var safeSerializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.All
        };

        safeSerializer.Converters.Clear(); // Remove DomainEventConverter

        return (IDomainEvent)jsonObject.ToObject(type, safeSerializer)!;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, IDomainEvent? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        Type type = value.GetType();
        var jsonObject = new JObject();

        foreach (PropertyInfo property in GetCachedProperties(type))
        {
            object? propertyValue = property.GetValue(value);
            if (propertyValue is not null)
            {
                jsonObject[property.Name] = JToken.FromObject(propertyValue, serializer);
            }
        }

        jsonObject.WriteTo(writer);
    }

    /// <summary>
    /// Gets the domain event type from the cache or resolves it using the provided assembly.
    /// </summary>
    /// <param name="typeName">The fully qualified name of the type.</param>
    /// <returns>The resolved <see cref="Type"/> or null if not found.</returns>
    private Type? GetOrAddMessageType(string typeName)
    {
        return TypeCache.GetOrAdd(typeName, assembly.GetType);
    }

    /// <summary>
    /// Gets the public readable properties of a given type, using caching for performance.
    /// </summary>
    /// <param name="type">The type to reflect.</param>
    /// <returns>An array of <see cref="PropertyInfo"/> objects.</returns>
    private static PropertyInfo[] GetCachedProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t =>
            [.. t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetMethod?.IsStatic == false)]);
    }
}
