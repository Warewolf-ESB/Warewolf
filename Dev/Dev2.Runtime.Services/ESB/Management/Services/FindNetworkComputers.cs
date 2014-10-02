
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find Computers service
    /// </summary>
    class FindNetworkComputers : IEsbManagementEndpoint
    {

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            StringBuilder result = new StringBuilder();
            Dev2Logger.Log.Info("Find Network Computers");
            string json = "[";
            try
            {
                var computers = GetComputerNames.ComputerNames;
                // DirectoryEntry with WinNT: was timing out, swapped to using a NetworkBrowser...
                json = computers.Cast<object>().Aggregate(json, (current, comp) => current + (@"{""ComputerName"":""" + comp + @"""},"));
                json += "]";
                json = json.Replace(",]", "]"); // remove last comma
                result.Append("<JSON>");
                result.Append(json);
                result.Append("</JSON>");
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                result.Append(ex.Message);
            }

            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findNetworkComputersService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };

            ServiceAction findNetworkComputersAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findNetworkComputersService.Actions.Add(findNetworkComputersAction);

            return findNetworkComputersService;
        }

        public string HandlesType()
        {
            return "FindNetworkComputersService";
        }
    }


    
}
