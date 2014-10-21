
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
using System.Reflection;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    public class PluginSources : ExceptionManager
    {
        #region Get

        // POST: Service/PluginSources/Get
        public PluginSource Get(string resourceId, Guid workspaceId, Guid dataListId)
        {
            var result = new PluginSource { ResourceID = Guid.Empty, ResourceType = ResourceType.PluginSource };
            try
            {
                var xmlStr = Resources.ReadXml(workspaceId, resourceId);
                if(!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new PluginSource(xml);
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Save

        // POST: Service/PluginSources/Save
        public string Save(string args, Guid workspaceId, Guid dataListId)
        {
            var pluginSourceDetails = JsonConvert.DeserializeObject<PluginSource>(args);

            if(string.IsNullOrEmpty(pluginSourceDetails.AssemblyName))
            {
                //resolve AssemblyName from AssemblyLocation
                if(!string.IsNullOrEmpty(pluginSourceDetails.AssemblyLocation) && !pluginSourceDetails.AssemblyLocation.StartsWith(GlobalConstants.GACPrefix))
                {
                    //assembly location refers to a file, read the assembly name out of the dll file
                    pluginSourceDetails.AssemblyLocation = pluginSourceDetails.AssemblyLocation.EndsWith("\\") ?
                        pluginSourceDetails.AssemblyLocation.Remove(pluginSourceDetails.AssemblyLocation.Length - 1) : //remove trailing slashes if they exist
                        pluginSourceDetails.AssemblyLocation; //else do nothing
                    try
                    {
                        pluginSourceDetails.AssemblyName = AssemblyName.GetAssemblyName(pluginSourceDetails.AssemblyLocation).Name;
                    }
                    catch(Exception ex)
                    {
                        if(!string.IsNullOrEmpty(pluginSourceDetails.AssemblyLocation))
                        {
                            pluginSourceDetails.AssemblyName = pluginSourceDetails.AssemblyLocation.Substring(pluginSourceDetails.AssemblyLocation.LastIndexOf("\\", StringComparison.Ordinal) + 1, pluginSourceDetails.AssemblyLocation.IndexOf(".dll", StringComparison.Ordinal) - pluginSourceDetails.AssemblyLocation.LastIndexOf("\\", StringComparison.Ordinal) - 1);
                        }
                        Dev2Logger.Log.Error(ex);
                    }
                }
                else
                {
                    //assembly location refers to the GAC
                    string getName = null;
                    if(!string.IsNullOrEmpty(pluginSourceDetails.AssemblyLocation))
                    {
                        getName = pluginSourceDetails.AssemblyLocation.Substring(pluginSourceDetails.AssemblyLocation.IndexOf(':') + 1);
                    }
                    pluginSourceDetails.AssemblyName = getName;
                }
            }

            ResourceCatalog.Instance.SaveResource(workspaceId, pluginSourceDetails);
            if(workspaceId != GlobalConstants.ServerWorkspaceID)
            {
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, pluginSourceDetails);
            }

            return pluginSourceDetails.ToString();
        }

        #endregion

        #region ValidateAssemblyImageFormat

        // POST: Service/PluginSources/ValidateAssemblyImageFormat
        public string ValidateAssemblyImageFormat(string args, Guid workspaceId, Guid dataListId)
        {
            // ReSharper disable RedundantAssignment
            var toJson = @"{""validationresult"":""failure""}";
            // ReSharper restore RedundantAssignment

            var broker = new PluginBroker();

            string errorMsg;

            if(broker.ValidatePlugin(args, out errorMsg))
            {
                toJson = @"{""validationresult"":""success""}";
            }
            else
            {
                toJson = @"{""validationresult"":""" + errorMsg + @"""}";
            }

            return toJson;
        }

        #endregion
    }
}
