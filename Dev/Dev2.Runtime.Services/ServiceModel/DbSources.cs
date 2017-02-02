/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
                    var broker = CreateDatabaseBroker();
                    result.DatabaseList = broker.GetDatabases(dbSourceDetails);
                    break;
                case enSourceType.MySqlDatabase:
                    var mybroker = CreateMySqlDatabaseBroker();
                    result.DatabaseList = mybroker.GetDatabases(dbSourceDetails);
                    break;
                case enSourceType.Oracle:
                    var obroker = CreateOracleDatabaseBroker();
                    result.DatabaseList = obroker.GetDatabases(dbSourceDetails);
                    break;
                case enSourceType.ODBC:
                    var odbcbroker = CreateODBCDatabaseBroker();
                    result.DatabaseList = odbcbroker.GetDatabases(dbSourceDetails);
                    break;
                case enSourceType.PostgreSQL:
                    var postgreBroker = CreatePostgreDatabaseBroker();
                    result.DatabaseList = postgreBroker.GetDatabases(dbSourceDetails);
                    break;
                default:
                    result.IsValid = false;
                    break;
            }
            return result;
        }

        #endregion

        protected SqlDatabaseBroker CreateDatabaseBroker()
        {
            return new SqlDatabaseBroker();
        }
        protected MySqlDatabaseBroker CreateMySqlDatabaseBroker()
        {
            return new MySqlDatabaseBroker();
        }
        protected OracleDatabaseBroker CreateOracleDatabaseBroker()
        {
            return new OracleDatabaseBroker();
        }
        protected ODBCDatabaseBroker CreateODBCDatabaseBroker()
        {
            return new ODBCDatabaseBroker();
        }

        protected PostgreSqlDataBaseBroker CreatePostgreDatabaseBroker()
        {
            return new PostgreSqlDataBaseBroker();
        }
    }
}
