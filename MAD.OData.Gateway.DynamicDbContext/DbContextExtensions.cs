using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace MAD.OData.Gateway.DynamicDbContext
{
    public static class DynamicContextExtensions
    {
        public static IQueryable Query(this DbContext context, string entitySetName)
        {
            var entitySetSplit = entitySetName.Split(".");
            var schema = entitySetSplit[0];
            var name = entitySetSplit[1];
            var entityTypes = context.Model.GetEntityTypes();

            foreach (var et in entityTypes)
            {
                var etSchema = et.GetDefaultSchema();
                var etTableName = et.GetDefaultTableName();

                if (etSchema != schema)
                    continue;

                if (etTableName != name)
                    continue;

                return (IQueryable)SetMethod.MakeGenericMethod(et.ClrType).Invoke(context, null);
            }

            throw new Exception("Entity not found");
        }

        static readonly MethodInfo SetMethod =
            typeof(DbContext).GetMethod(nameof(DbContext.Set), 1, new Type[] {  }) ??
            throw new Exception($"Type not found: DbContext.Set");
    }
}
