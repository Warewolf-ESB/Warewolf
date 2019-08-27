/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindResourcesByType : DefaultEsbManagementEndpoint
    {
        private readonly Lazy<IResourceCatalog> _resourceCatalog;

        public FindResourcesByType()
            : this(new Lazy<IResourceCatalog>(() => ResourceCatalog.Instance))
        {
        }
        public FindResourcesByType(Lazy<IResourceCatalog> resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                string typeName = null;
                values.TryGetValue("Type", out StringBuilder tmp);
                if (tmp != null)
                {
                    typeName = tmp.ToString();
                }

                if (string.IsNullOrEmpty(typeName))
                {

                    throw new ArgumentNullException("type");

                }
                Dev2Logger.Info("Find Resources By Type. " + typeName, GlobalConstants.WarewolfInfo);

                var result = _resourceCatalog.Value.FindByType(typeName);
                if (result != null)
                {
                    var serializer = new Dev2JsonSerializer();
                    return serializer.SerializeToBuilder(result);
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

        public override string HandlesType() => nameof(FindResourcesByType);
    }
}
