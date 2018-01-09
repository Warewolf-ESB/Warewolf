namespace Dev2.Services.Sql
{
    public interface IConnectionBuilder
    {
        ISqlConnection BuildConnection(string connectionString);
    }
}