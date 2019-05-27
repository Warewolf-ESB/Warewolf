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
using Dev2.Common.Common;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class ComPluginService : Service
    {
        public RecordsetList Recordsets { get; set; }        
        public string Namespace { get; set; }
        public string SerializedResult { get; set; }

        #region CTOR

        public ComPluginService()
        {
            ResourceID = Guid.Empty;
            ResourceType = "ComPluginService";
            Source = new ComPluginSource();
            Recordsets = new RecordsetList();
            Method = new ServiceMethod();
        }

        public ComPluginService(XElement xml)
            : base(xml)
        {
            ResourceType = "ComPluginService";
            var action = xml.Descendants("Action").FirstOrDefault();
            Namespace = action.AttributeSafe("Namespace");
            Source = CreateSource<ComPluginSource>(action);
            Method = CreateInputsMethod(action);
            Recordsets = CreateOutputsRecordsetList(action);
        }

        #endregion
    }
}