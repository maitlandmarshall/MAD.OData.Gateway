using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SqlClient;

namespace MAD.OData.Gateway.Services
{
    public class SqlKataFactory
    {
        private readonly IConfiguration configuration;

        public SqlKataFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public QueryFactory Create()
        {
            var sqlConnection = new SqlConnection(this.configuration.GetConnectionString("odata"));
            var compiler = new SqlServerCompiler();

            return new QueryFactory(sqlConnection, compiler);
        }
    }
}
