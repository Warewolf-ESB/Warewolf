/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
    public class DbSources : ExceptionManager, IDbSources
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
				case enSourceType.SQLiteDatabase:
					var sqliteBroker = CreateSqliteDatabaseBroker();
					result.DatabaseList = sqliteBroker.GetDatabases(dbSourceDetails);
					break;
				case enSourceType.WebService:
                    break;
                case enSourceType.DynamicService:
                    break;
                case enSourceType.ManagementDynamicService:
                    break;
                case enSourceType.PluginSource:
                    break;
                case enSourceType.Unknown:
                    break;
                case enSourceType.Dev2Server:
                    break;
                case enSourceType.EmailSource:
                    break;
                case enSourceType.WebSource:
                    break;
                case enSourceType.OauthSource:
                    break;
                case enSourceType.SharepointServerSource:
                    break;
                case enSourceType.RabbitMQSource:
                    break;
                case enSourceType.ExchangeSource:
                    break;
                case enSourceType.WcfSource:
                    break;
                case enSourceType.ComPluginSource:
                    break;
                default:
                    result.IsValid = false;
                    break;
            }
            return result;
        }

        #endregion

        protected SqlDatabaseBroker CreateDatabaseBroker() => new SqlDatabaseBroker();

        protected MySqlDatabaseBroker CreateMySqlDatabaseBroker() => new MySqlDatabaseBroker();

        protected OracleDatabaseBroker CreateOracleDatabaseBroker() => new OracleDatabaseBroker();

        protected ODBCDatabaseBroker CreateODBCDatabaseBroker() => new ODBCDatabaseBroker();

        protected PostgreSqlDataBaseBroker CreatePostgreDatabaseBroker() => new PostgreSqlDataBaseBroker();

		protected SqliteDatabaseBroker CreateSqliteDatabaseBroker() => new SqliteDatabaseBroker();
	}
}
