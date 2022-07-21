using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;
using System.Reflection;

namespace MAD.OData.Gateway.DynamicDbContext
{
    public static class DynamicContextExtensions
    {
        public static IQueryable Query(this DbContext context, string entitySetName)
        {
            var et = context.GetEntityType(entitySetName);

            if (et is null)
                throw new Exception("Entity not found");

            return (IQueryable)SetMethod.MakeGenericMethod(et.ClrType).Invoke(context, null);
        }

        public static IEntityType GetEntityType(this DbContext context, string entitySetName)
        {
            var entityTypes = context.Model.GetEntityTypes();

            foreach (var et in entityTypes)
            {
                if (et.GetEntitySetName() == entitySetName)
                    return et;
            }

            return null;
        }

        static readonly MethodInfo SetMethod =
            typeof(DbContext).GetMethod(nameof(DbContext.Set), 1, new Type[] { }) ??
            throw new Exception($"Type not found: DbContext.Set");
    }
}
