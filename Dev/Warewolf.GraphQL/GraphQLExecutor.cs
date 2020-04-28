using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Warewolf.Storage.Interfaces;

namespace Warewolf.GraphQL
{
    public class GraphQLExecutor
    {
        private readonly IExecutionEnvironment _dataObjEnvironment;
        private ISchema _schema;

        public GraphQLExecutor(IExecutionEnvironment dataObjEnvironment)
        {
            _dataObjEnvironment = dataObjEnvironment;
            _schema = new Schema
            {
                Query = new Query(_dataObjEnvironment),
            };

        }


        public string Execute(string query)
        {
            return _schema.ExecuteAsync(new DocumentWriter(), _ =>
                                                              {
                                                                _.Query = query;
                                                                _.EnableMetrics = true;
                                                              }).Result;
        }
    }
}