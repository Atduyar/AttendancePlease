using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configuration;

public static class BaseAuditableEntityConfiguration
{
    public static ModelBuilder ApplyBaseAuditableEntityConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IBaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IBaseAuditableEntity.CreatedAt))
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }

        return modelBuilder;
    }
}
