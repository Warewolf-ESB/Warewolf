using System.Data;

namespace Dev2.Services.Sql
{
    public interface IDbFactory
    {
        IDbConnection CreateConnection(string connectionString);

        IDbCommand CreateCommand(IDbConnection connection, CommandType text, string format);

        DataTable GetSchema(IDbConnection connection, string collectionName);

        DataTable CreateTable(IDataReader reader, LoadOption overwriteChanges);

        DataSet FetchDataSet(IDbCommand command);
    }
}