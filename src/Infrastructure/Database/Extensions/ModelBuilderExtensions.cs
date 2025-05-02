using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database.Extensions;


/// <summary>
/// Provides extension methods for configuring the EF Core model,
/// including applying a UTC DateTime converter to properties.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// // A value converter that ensures DateTime values are marked as UTC when read from the database
    /// </summary>
    private static readonly ValueConverter<DateTime, DateTime> UtcValueConverter =
        new(outside => outside, inside => DateTime.SpecifyKind(inside, DateTimeKind.Utc));

    /// <summary>
    /// Applies a value converter to all DateTime properties in the model
    /// that include "Utc" in their name, ensuring they are treated as UTC.
    /// This helps prevent issues related to DateTimeKind being Unspecified
    /// when loading data from the database.
    /// </summary>
    /// <param name="modelBuilder">The EF Core ModelBuilder instance.</param>
    public static void ApplyUtcDateTimeConverter(this ModelBuilder modelBuilder)
    {
        modelBuilder.Model.GetEntityTypes()
            .ForEach(mutableEntityType => mutableEntityType
                .GetProperties()
                .Where(p => p.ClrType == typeof(DateTime) && p.Name.Contains("Utc", StringComparison.Ordinal))
                .ForEach(mutableProperty => mutableProperty.SetValueConverter(UtcValueConverter)));
    }
}
