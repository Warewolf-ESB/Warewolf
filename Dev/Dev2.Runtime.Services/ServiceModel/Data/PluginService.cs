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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class PluginService : Service
    {
        // BUG 9500 - 2013.05.31 - TWR : removed Recordset property
        public RecordsetList Recordsets { get; set; }

        // BUG 9500 - 2013.05.31 - TWR : added
        public string Namespace { get; set; }
        public string SerializedResult { get; set; }
        public ServiceConstructor Constructor { get; set; }
        public List<IDev2MethodInfo> MethodsToRun { get; set; }
        #region CTOR

        public PluginService()
        {
            ResourceID = Guid.Empty;
            ResourceType = "PluginService";
            Source = new PluginSource();
            Recordsets = new RecordsetList();
            MethodsToRun = new List<IDev2MethodInfo>();
            Constructor = new ServiceConstructor();
            Method = new ServiceMethod();
        }

        public PluginService(XElement xml)
            : base(xml)
        {
            ResourceType = "PluginService";
            var action = xml.Descendants("Action").FirstOrDefault(); ;
            if(action == null)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (xml.HasAttributes && xml.Attribute("Type").Value == "Plugin")
                {
                    action = xml;
                }
                else
                {
                    return;
                }
            }

            // BUG 9500 - 2013.05.31 - TWR : added
            Namespace = action.AttributeSafe("Namespace");

            // Handle old service this is not set
            // We also need to redo wizards to correctly return defaults and mappings ;)
            if(string.IsNullOrEmpty(Namespace))
            {
                var mySource = action.AttributeSafe("SourceName");
                if (!string.IsNullOrEmpty(mySource))
                {
                    // Now look up the old source and fetch namespace ;)
                    var services = ResourceCatalog.Instance.GetDynamicObjects<Source>(GlobalConstants.ServerWorkspaceID, mySource);

                    var tmp = services.FirstOrDefault();

                    if (tmp != null)
                    {
                        Namespace = tmp.AssemblyName;
                    }
                }
            }

            Source = CreateSource<PluginSource>(action);
            Method = CreateInputsMethod(action);
            Recordsets = CreateOutputsRecordsetList(action);
        }

        #endregion

        #region ToXml

        // BUG 9500 - 2013.05.31 - TWR : refactored
        public override XElement ToXml()
        {
            var result = CreateXml(enActionType.Plugin, Source, Recordsets,
                new XAttribute("Namespace", Namespace ?? string.Empty)
                );
            return result;
        }

        #endregion
    }
}
