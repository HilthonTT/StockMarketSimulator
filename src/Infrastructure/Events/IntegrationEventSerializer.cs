using System.Collections.Concurrent;
using System.Text;
using Application.Abstractions.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Infrastructure.Events;

/// <summary>
/// Serializes integration events for the message broker.
/// </summary>
/// <remarks>
/// The concrete event type travels in a <c>$type</c> property on the root object only
/// (<see cref="TypeNameHandling.Auto"/>). On the way back in, the <see cref="IntegrationEventBinder"/>
/// refuses to instantiate anything that is not an <see cref="IIntegrationEvent"/>; resolving arbitrary
/// <c>$type</c> values from a queue is a well-known remote-code-execution vector.
/// </remarks>
internal static class IntegrationEventSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        SerializationBinder = new IntegrationEventBinder()
    };

    public static byte[] Serialize(IIntegrationEvent integrationEvent) =>
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(integrationEvent, typeof(IIntegrationEvent), Settings));

    public static IIntegrationEvent? Deserialize(ReadOnlySpan<byte> body) =>
        JsonConvert.DeserializeObject<IIntegrationEvent>(Encoding.UTF8.GetString(body), Settings);

    private sealed class IntegrationEventBinder : ISerializationBinder
    {
        private static readonly ConcurrentDictionary<(string? Assembly, string Type), Type> Cache = new();
        private readonly DefaultSerializationBinder _inner = new();

        public Type BindToType(string? assemblyName, string typeName) =>
            Cache.GetOrAdd((assemblyName, typeName), key =>
            {
                Type type = _inner.BindToType(key.Assembly, key.Type);

                return typeof(IIntegrationEvent).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false }
                    ? type
                    : throw new JsonSerializationException(
                        $"Type '{key.Type}' is not an allowed integration event type.");
            });

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName) =>
            _inner.BindToName(serializedType, out assemblyName, out typeName);
    }
}
