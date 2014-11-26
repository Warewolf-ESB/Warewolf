
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
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Reflection;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find registred assemblies
    /// </summary>
    public class RegisteredAssembly : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            SortedSet<string> gacList = new SortedSet<string>();

            StringBuilder result = new StringBuilder();
            Dev2Logger.Log.Info("Registered Assembly");
            try
            {
                IAssemblyName assemblyName;
                IAssemblyEnum assemblyEnum = GAC.CreateGACEnum();
                string json = "[";
                while (GAC.GetNextAssembly(assemblyEnum, out assemblyName) == 0)
                {
                    try
                    {
                        gacList.Add(GAC.GetDisplayName(assemblyName, ASM_DISPLAY_FLAGS.VERSION | ASM_DISPLAY_FLAGS.CULTURE | ASM_DISPLAY_FLAGS.PUBLIC_KEY_TOKEN));
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Log.Error(e.Message);
                    }
                }

                // now process each sorted entry
                foreach (string entry in gacList)
                {
                    json += @"{""AssemblyName"":""" + entry + @"""}";
                    json += ",";
                }

                json += "]";
                json = json.Replace(",]", "]"); //remove the last comma in the string in order to have valid json
                result.Append("<JSON>");
                result.Append(json);
                result.Append("</JSON>");
            }
            catch (Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                result.Append(ex.Message);
            }
            
            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService registeredAssemblyService = new DynamicService
                {
                    Name = HandlesType(),
                    DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
                };

            ServiceAction registeredAssemblyAction = new ServiceAction
                {
                    Name = HandlesType(),
                    SourceMethod = HandlesType(),
                    ActionType = enActionType.InvokeManagementDynamicService
                };

            registeredAssemblyService.Actions.Add(registeredAssemblyAction);

            return registeredAssemblyService;
        }

        public string HandlesType()
        {
            return "RegisteredAssemblyService";
        }
    }
}
