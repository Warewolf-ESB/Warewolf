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
using Dev2.Workspaces;
using Dev2.Runtime.Auditing;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetLogDataService : LogDataServiceBase, IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2Logger.Info("Get Log Data Service", GlobalConstants.WarewolfInfo);
            var serializer = new Dev2JsonSerializer();
            try
            {
                var results = BuildTempObjects(values);
                LogDataCache.CurrentResults = results;
                return serializer.SerializeToBuilder(results);
            }
            catch (Exception e)
            {
                Dev2Logger.Info("Get Log Data ServiceError", e, GlobalConstants.WarewolfInfo);
            }
            return serializer.SerializeToBuilder("");
        }
              
        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "GetLogDataService";

       
    }

    public static class LogDataCache
    {
        public static IEnumerable<dynamic> CurrentResults { get; set; }
    }
}