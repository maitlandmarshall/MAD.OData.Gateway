using Castle.DynamicProxy.Internal;
using MAD.OData.Gateway.DynamicDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.OData.Edm;

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
            var entityTypes = db.Model.GetEntityTypes();

            var edmModel = new EdmModel();
            var container = new EdmEntityContainer("MAD.OData.Gateway", "EntityContainer");
            edmModel.AddElement(container);

            foreach (var entityType in entityTypes)
            {
                if (entityType.Name == "__EFMigrationsHistory")
                    continue;

                var tableType = new EdmEntityType(entityType.ClrType.Namespace, entityType.ClrType.Name);
                var columns = entityType.GetProperties();

                foreach (var col in columns)
                {
                    var isPk = col.IsPrimaryKey();
                    var prop = tableType.AddStructuralProperty(col.Name, this.GetEdmPrimitiveTypeKind(col), isPk == false);

                    if (isPk)
                    {
                        tableType.AddKeys(prop);
                    }
                }

                edmModel.AddElement(tableType);
                var es = container.AddEntitySet($"{entityType.GetDefaultSchema()}.{entityType.GetDefaultTableName()}", tableType);
            }

            return edmModel;
        }

        private EdmPrimitiveTypeKind GetEdmPrimitiveTypeKind(IProperty column)
        {
            var type = this.GetUnwrappedClrType(column.ClrType);

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
                var typeMapping = column.GetTypeMapping();

                if (typeMapping is SqlServerDateTimeTypeMapping dateMap)
                {
                    if (dateMap.DbType == System.Data.DbType.Date)
                        return EdmPrimitiveTypeKind.Date;
                    else
                        return EdmPrimitiveTypeKind.DateTimeOffset;
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

        private Type GetUnwrappedClrType(Type type)
        {
            if (type.IsNullableType())
            {
                return type.GenericTypeArguments[0];
            }
            else
            {
                return type;
            }
        }
    }
}
