using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace Dev2.Integration.TestWeb
{
    public class GetCountries : AbstractHttpHandler
    {
        protected override string GetResponse(HttpContext context, string extension)
        {
            var prefix = context.Request.Params["prefix"];
            var paramPrefix = new SqlParameter("Prefix", SqlDbType.VarChar, 50) { Value = string.IsNullOrEmpty(prefix) ? "a" : prefix };

            var dt = FetchDataTable(CommandType.StoredProcedure, "Pr_CitiesGetCountries", paramPrefix);
            switch(extension.ToLower())
            {
                case "json":
                    return JsonConvert.SerializeObject(dt);

                default:
                    using(var writer = new StringWriter())
                    {
                        dt.WriteXml(writer);
                        return writer.ToString();
                    }
            }
        }

        static DataTable FetchDataTable(CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            using(var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Dev2TestingDB"].ConnectionString))
            {
                using(var command = new SqlCommand(commandText, connection) { CommandType = commandType })
                {
                    if(parameters != null && parameters.Length > 0)
                    {
                        foreach(var parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    connection.Open();
                    using(var reader = command.ExecuteReader((CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo)))
                    {
                        var table = new DataTable(commandText);
                        table.Load(reader, LoadOption.OverwriteChanges);
                        return table;
                    }
                }
            }
        }
    }
}