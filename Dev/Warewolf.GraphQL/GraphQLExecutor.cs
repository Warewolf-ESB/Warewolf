using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Warewolf.Storage.Interfaces;

namespace Warewolf.GraphQL
{
    public class GraphQLExecutor
    {
        private readonly IExecutionEnvironment _dataObjEnvironment;
        private readonly string _dataList;
        private ISchema _schema;

        public GraphQLExecutor(IExecutionEnvironment dataObjEnvironment, string dataList)
        {
            _dataObjEnvironment = dataObjEnvironment;
            _dataList = dataList;
            _schema = new Schema
            {
                Query = new Query(_dataObjEnvironment)
            };

        }


        public string Execute(string query)
        {
            return _schema.ExecuteAsync(new DocumentWriter(), _ => { _.Query = query; }).Result;
        }
    }
}