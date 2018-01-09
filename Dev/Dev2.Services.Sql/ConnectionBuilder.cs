namespace Dev2.Services.Sql
{
    public class ConnectionBuilder : IConnectionBuilder
    {
        public ISqlConnection BuildConnection(string connectionString)
        {
            return new SqlConnectionWrapper(connectionString);
        }
    }
}