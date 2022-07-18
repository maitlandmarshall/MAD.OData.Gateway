using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Data.SqlClient;

namespace MAD.OData.Gateway.Services
{
    public class EdmModelFactory
    {
        private readonly string connectionString;

        public EdmModelFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<IEdmModel> Create()
        {
            var connection = new SqlConnection(this.connectionString);
            var reader = new DatabaseReader(connection);
            var tables = reader.TablesQuickView();

            var edmModel = new EdmModel();
            var container = new EdmEntityContainer("MAD.OData.Gateway", "EntityContainer");
            edmModel.AddElement(container);

            foreach (var t in tables)
            {
                var tableType = new EdmEntityType(t.SchemaOwner, t.Name);
                
                foreach (var col in t.Columns)
                {
                    var prop = tableType.AddStructuralProperty(col.Name, this.GetEdmPrimitiveTypeKind(col));

                    if (col.IsPrimaryKey)
                    {
                        tableType.AddKeys(prop);
                    }
                }

                edmModel.AddElement(tableType);
                container.AddEntitySet($"{t.SchemaOwner}.{t.Name}", tableType);
            }

            return edmModel;
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
                return EdmPrimitiveTypeKind.DateTimeOffset;

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
