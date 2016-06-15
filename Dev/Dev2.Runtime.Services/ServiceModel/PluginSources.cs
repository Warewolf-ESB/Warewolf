
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Xml.Linq;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel
{
    public class PluginSources : ExceptionManager
    {
        #region Get

        // POST: Service/PluginSources/Get
        public PluginSource Get(string resourceId, Guid workspaceId, Guid dataListId)
        {
            var result = new PluginSource { ResourceID = Guid.Empty, ResourceType = "PluginSource" };
            try
            {
                var xmlStr = Resources.ReadXml(workspaceId, "PluginSource", resourceId);
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

    }
}
