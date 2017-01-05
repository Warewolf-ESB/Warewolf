/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Xml.Linq;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class PluginServicesMock : PluginServices
    {
        public new Service DeserializeService(string args)
        {
            return base.DeserializeService(args);
        }

        public new Service DeserializeService(XElement xml, string resourceType)
        {
            return base.DeserializeService(xml, resourceType);
        }

        public bool FetchRecordsetAddFields { get; set; }
        public int FetchRecordsetHitCount { get; set; }
        public override RecordsetList FetchRecordset(PluginService pluginService, bool addFields)
        {
            FetchRecordsetHitCount++;
            FetchRecordsetAddFields = addFields;
            return base.FetchRecordset(pluginService, addFields);
        }
    }
}
