using System;
using System.Reflection;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Newtonsoft.Json;
using PluginSource = Dev2.Runtime.ServiceModel.Data.PluginSource;

namespace Dev2.Runtime.ServiceModel
{
    public class PluginSources : ExceptionManager
    {
        #region Get

        // POST: Service/PluginSources/Get
        public PluginSource Get(string resourceID, Guid workspaceID, Guid dataListID)
        {
            var result = new PluginSource { ResourceID = Guid.Empty, ResourceType = ResourceType.PluginSource };
            try
            {
                var xmlStr = Resources.ReadXml(workspaceID, ResourceType.PluginSource, resourceID);
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
        public string Save(string args, Guid workspaceID, Guid dataListID)
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
                        this.LogError(ex);
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

            ResourceCatalog.Instance.SaveResource(workspaceID, pluginSourceDetails);
            if(workspaceID != GlobalConstants.ServerWorkspaceID)
            {
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, pluginSourceDetails);
            }

            return pluginSourceDetails.ToString();
        }

        #endregion

        #region ValidateAssemblyImageFormat

        // POST: Service/PluginSources/ValidateAssemblyImageFormat
        public string ValidateAssemblyImageFormat(string args, Guid workspaceID, Guid dataListID)
        {
            var toJson = @"{""validationresult"":""failure""}";

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