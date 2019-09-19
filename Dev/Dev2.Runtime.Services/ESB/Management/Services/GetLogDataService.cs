#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Warewolf.Driver.Serilog;
using Warewolf.Auditing;
using Warewolf.Logging;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetLogDataService : LogDataServiceBase, IEsbManagementEndpoint
    {
        private readonly ISeriLogConfig _seriLogSQLiteConfig;
        public GetLogDataService()
        {
            _seriLogSQLiteConfig = new SeriLogSQLiteConfig();
        }

        public GetLogDataService(ISeriLogConfig seriLogSQLiteConfig)
        {
            _seriLogSQLiteConfig = seriLogSQLiteConfig;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var resourceId = GetValue<string>("ResourceID", values);
            SeriLoggerSource seriLoggerSource = null;

            //get resource using resourceId to get connectionString and tableName
            if (resourceId == GlobalConstants.DefaultLoggingSourceId)
            {
                seriLoggerSource = new SeriLoggerSource();
            }

            Dev2Logger.Info("Get Log Data Service", GlobalConstants.WarewolfInfo);
            var serializer = new Dev2JsonSerializer();
            try
            {
                var logg = new AuditQueryable(seriLoggerSource.ConnectionString, seriLoggerSource.TableName);
                var audits = logg.QueryLogData(values);
                LogDataCache.CurrentResults = audits;
                return serializer.SerializeToBuilder(audits);
            }
            catch (Exception e)
            {
                Dev2Logger.Info("Get Log Data ServiceError", e, GlobalConstants.WarewolfInfo);
            }
            return serializer.SerializeToBuilder("");
        }

        T GetValue<T>(string key, Dictionary<string, StringBuilder> values)
        {
            var toReturn = default(T);
            if (values.TryGetValue(key, out StringBuilder value))
            {
                var item = value.ToString();
                return (T)Convert.ChangeType(item, typeof(T));
            }
            return toReturn;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "GetLogDataService";
    }

    public static class LogDataCache
    {
        public static IEnumerable<dynamic> CurrentResults { get; set; }
    }
}