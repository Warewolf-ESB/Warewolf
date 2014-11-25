/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Data;

namespace Dev2.Common.Interfaces.Services.Sql
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