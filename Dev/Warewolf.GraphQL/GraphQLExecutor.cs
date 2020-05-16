#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Warewolf.Storage.Interfaces;

namespace Warewolf.GraphQL
{
    public class GraphQLExecutor: IGraphQLExecutor
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