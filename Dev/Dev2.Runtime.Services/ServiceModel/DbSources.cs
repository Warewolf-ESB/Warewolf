
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;

namespace Dev2.Runtime.ServiceModel
{
    public class DbSources : ExceptionManager
    {
        
        #region Get database list

        public virtual DatabaseValidationResult DoDatabaseValidation(DbSource dbSourceDetails)
        {
            var result = new DatabaseValidationResult();

            switch (dbSourceDetails.ServerType)
            {
                case enSourceType.SqlDatabase:
                    var broker = CreateDatabaseBroker(dbSourceDetails.ServerType);
                    result.DatabaseList = broker.GetDatabases(dbSourceDetails);
                    break;
                case enSourceType.MySqlDatabase:
                    var mybroker = CreateMySqlDatabaseBroker(dbSourceDetails.ServerType);
                    result.DatabaseList = mybroker.GetDatabases(dbSourceDetails);
                    break;
                case enSourceType.Oracle:
                    var obroker = CreateOracleDatabaseBroker(dbSourceDetails.ServerType);
                    result.DatabaseList = obroker.GetDatabases(dbSourceDetails);
                    break;
                case enSourceType.ODBC:
                    var odbcbroker = CreateODBCDatabaseBroker(dbSourceDetails.ServerType);
                    result.DatabaseList = odbcbroker.GetDatabases(dbSourceDetails);
                    break;
                case enSourceType.PostgreSql:
                    var postgreBroker = CreatePostgreDatabaseBroker(dbSourceDetails.ServerType);
                    result.DatabaseList = postgreBroker.GetDatabases(dbSourceDetails);
                    break;
                default:
                    result.IsValid = false;
                    break;
            }
            return result;
        }

        #endregion

        protected virtual SqlDatabaseBroker CreateDatabaseBroker(enSourceType type)
        {
            return new SqlDatabaseBroker();
        }
        protected virtual MySqlDatabaseBroker CreateMySqlDatabaseBroker(enSourceType type)
        {
            return new MySqlDatabaseBroker();
        }
        protected virtual OracleDatabaseBroker CreateOracleDatabaseBroker(enSourceType type)
        {
            return new OracleDatabaseBroker();
        }
        protected virtual ODBCDatabaseBroker CreateODBCDatabaseBroker(enSourceType type)
        {
            return new ODBCDatabaseBroker();
        }

        protected virtual PostgreSqlDataBaseBroker CreatePostgreDatabaseBroker(enSourceType type)
        {
            return new PostgreSqlDataBaseBroker();
        }
    }
}
