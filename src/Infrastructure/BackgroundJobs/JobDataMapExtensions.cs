using System.Globalization;
using Quartz;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Typed accessors for <see cref="JobDataMap"/>.
/// </summary>
/// <remarks>
/// Quartz 4 only ships string/number/bool/<see cref="DateTimeOffset"/> accessors.
/// These extensions add the Guid, decimal and DateTime accessors the jobs need and tolerate values stored either as their native type or as strings
/// (which is how values come back from a persistent job store).
/// </remarks>
public static class JobDataMapExtensions
{
    public static Guid GetGuid(this JobDataMap map, string key) =>
        Get(map, key) switch
        {
            Guid guid => guid,
            string text => Guid.Parse(text, CultureInfo.InvariantCulture),
            object other => throw InvalidType(key, other, typeof(Guid)),
        };

    public static decimal GetDecimal(this JobDataMap map, string key) =>
        Convert.ToDecimal(Get(map, key), CultureInfo.InvariantCulture);

    public static DateTime GetDateTime(this JobDataMap map, string key) =>
        Get(map, key) switch
        {
            DateTime dateTime => dateTime,
            DateTimeOffset offset => offset.UtcDateTime,
            string text => DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            object other => throw InvalidType(key, other, typeof(DateTime)),
        };

    private static object Get(JobDataMap map, string key) =>
        map.TryGetValue(key, out object? value) && value is not null
            ? value
            : throw new KeyNotFoundException($"Job data key '{key}' is missing.");

    private static InvalidCastException InvalidType(string key, object value, Type expected) =>
        new($"Job data key '{key}' holds a {value.GetType().Name}, expected {expected.Name}.");
}
