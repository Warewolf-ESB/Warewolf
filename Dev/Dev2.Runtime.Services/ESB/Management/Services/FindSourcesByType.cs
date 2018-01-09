/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindSourcesByType : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                string type = null;
                values.TryGetValue("Type", out StringBuilder tmp);
                if (tmp != null)
                {
                    type = tmp.ToString();
                }

                if (string.IsNullOrEmpty(type))
                {
                    
                    throw new ArgumentNullException("type");
                    
                }
                Dev2Logger.Info("Find Sources By Type. " + type, GlobalConstants.WarewolfInfo);
                if (Enum.TryParse(type, true, out enSourceType sourceType))
                {
                    var result = ResourceCatalog.Instance.GetModels(theWorkspace.ID, sourceType);
                    if (result != null)
                    {
                        var serializer = new Dev2JsonSerializer();
                        return serializer.SerializeToBuilder(result);
                    }
                }
                return new StringBuilder();
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FindSourcesByType";
    }
}
