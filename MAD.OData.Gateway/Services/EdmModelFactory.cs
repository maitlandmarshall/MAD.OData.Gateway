using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using MAD.OData.Gateway.DynamicDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Data.SqlClient;

namespace MAD.OData.Gateway.Services
{
    public class EdmModelFactory
    {
        private readonly DynamicDbContextFactory dynamicDbContextFactory;
        private readonly string connectionString;

        public EdmModelFactory(DynamicDbContextFactory dynamicDbContextFactory, string connectionString)
        {
            this.dynamicDbContextFactory = dynamicDbContextFactory;
            this.connectionString = connectionString;
        }

        public async Task<IEdmModel> Create()
        {
            using var db = this.dynamicDbContextFactory.CreateDbContext(this.connectionString);
            var connection = new SqlConnection(this.connectionString);
            var reader = new DatabaseReader(connection);
            var tables = reader.TablesQuickView();

            var edmModel = new EdmModel();
            var container = new EdmEntityContainer("MAD.OData.Gateway", "EntityContainer");
            edmModel.AddElement(container);

            foreach (var t in tables)
            {
                if (t.Name == "__EFMigrationsHistory")
                    continue;

                var entityType = this.GetEntityType(db, t);
                var tableType = new EdmEntityType(entityType.ClrType.Namespace, entityType.ClrType.Name);
                
                foreach (var col in t.Columns)
                {
                    var prop = tableType.AddStructuralProperty(col.Name, this.GetEdmPrimitiveTypeKind(col), col.IsPrimaryKey == false);

                    if (col.IsPrimaryKey)
                    {
                        tableType.AddKeys(prop);
                    }
                }

                edmModel.AddElement(tableType);
                var es = container.AddEntitySet($"{t.SchemaOwner}.{t.Name}", tableType);
            }

            return edmModel;
        }

        private IEntityType GetEntityType(DbContext context, DatabaseTable t)
        {
            var entityTypes = context.Model.GetEntityTypes();

            foreach (var et in entityTypes)
            {
                if (et.GetSchema() != t.SchemaOwner)
                    continue;

                if (et.GetTableName() != t.Name)
                    continue;

                return et;
            }

            return null;
        }

        private EdmPrimitiveTypeKind GetEdmPrimitiveTypeKind(DatabaseColumn column)
        {
            var type = column.NetDataType();

            if (type == typeof(string))
                return EdmPrimitiveTypeKind.String;

            else if (type == typeof(short))
                return EdmPrimitiveTypeKind.Int16;

            else if (type == typeof(int))
                return EdmPrimitiveTypeKind.Int32;

            else if (type == typeof(long))
                return EdmPrimitiveTypeKind.Int64;

            else if (type == typeof(bool))
                return EdmPrimitiveTypeKind.Boolean;

            else if (type == typeof(Guid))
                return EdmPrimitiveTypeKind.Guid;

            else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                if (column.DbDataType == "date")
                {
                    return EdmPrimitiveTypeKind.Date;
                }
                else
                {
                    return EdmPrimitiveTypeKind.DateTimeOffset;
                }
            }
            else if (type == typeof(TimeSpan))
                return EdmPrimitiveTypeKind.Duration;

            else if (type == typeof(decimal))
                return EdmPrimitiveTypeKind.Decimal;

            else if (type == typeof(byte) || type == typeof(sbyte))
                return EdmPrimitiveTypeKind.Byte;

            else if (type == typeof(byte[]))
                return EdmPrimitiveTypeKind.Binary;

            else if (type == typeof(double))
                return EdmPrimitiveTypeKind.Double;

            else if (type == typeof(float))
                return EdmPrimitiveTypeKind.Single;
            else
                throw new NotImplementedException();
        }
    }
}
