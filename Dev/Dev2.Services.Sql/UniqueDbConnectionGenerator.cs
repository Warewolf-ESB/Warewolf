/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Data.SqlClient;

namespace Dev2.Services.Sql
{
    public static class UniqueDbConnectionGenerator
    {
        public static SqlConnection GetConnection(string connectionString)
        {
            var conStrBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = 20,
                Pooling = true,
                MaxPoolSize = 100,
                MultipleActiveResultSets = true,                
            };
            conStrBuilder.ApplicationName = "Warewolf Service " + Guid.NewGuid().ToString();            
            var cString = conStrBuilder.ConnectionString;
            return new SqlConnection(cString);            
        }
    }
}
