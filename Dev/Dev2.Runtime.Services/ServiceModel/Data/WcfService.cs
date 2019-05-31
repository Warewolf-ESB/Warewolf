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
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core.DynamicServices;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class WcfService : Service
    {
        public RecordsetList Recordsets { get; set; }

        #region CTOR

        public WcfService()
        {
            ResourceID = Guid.Empty;
            ResourceType = "PluginService";
            Source = new WcfSource();
            Recordsets = new RecordsetList();
            Method = new ServiceMethod();
        }

        public WcfService(XElement xml)
            : base(xml)
        {
            ResourceType = "PluginService";
            var action = xml.Descendants("Action").FirstOrDefault();
            if (action == null)
            {
                
                if (xml.HasAttributes && xml.Attribute("Type").Value == "WcfService")
                {
                    action = xml;
                }
                else
                {
                    return;
                }
            }

            Source = CreateSource<WcfService>(action);
            Method = CreateInputsMethod(action);
            Recordsets = CreateOutputsRecordsetList(action);
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = CreateXml(enActionType.Plugin, Source, Recordsets);
            return result;
        }

        #endregion
    }
}
