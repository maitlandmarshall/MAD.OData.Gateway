using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MAD.OData.Gateway.DynamicDbContext
{
    public static class EntityTypeExtensions
    {
        public static string GetEntitySetName(this IEntityType entityType)
        {
            var schema = entityType.GetDefaultSchema();
            var tableName = entityType.GetDefaultTableName();

            if (string.IsNullOrWhiteSpace(tableName))
                throw new InvalidEntityTypeException($"EntityType with name '{entityType.Name}' has a null or empty {nameof(tableName)}.");

            if (schema == "dbo")
                return tableName;
            else
                return $"{entityType.GetDefaultSchema()}.{entityType.GetDefaultTableName()}";
        }

    }
}
