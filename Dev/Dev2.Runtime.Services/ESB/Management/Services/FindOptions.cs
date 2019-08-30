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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Warewolf.Options;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindOptions : DefaultEsbManagementEndpoint
    {
        private IResourceCatalog _resourceCatalog;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                if (values == null)
                {
                    throw new InvalidDataContractException(ErrorResource.NoParameter);
                }
                string selectedSourceId = null;
                values.TryGetValue("SelectedSourceId", out StringBuilder tmp);
                if (tmp != null)
                {
                    selectedSourceId = tmp.ToString();
                }

                if (string.IsNullOrEmpty(selectedSourceId))
                {
                    throw new ArgumentNullException("SelectedSourceId");
                }
                Dev2Logger.Info("Find Options. " + selectedSourceId, GlobalConstants.WarewolfInfo);

                var source = ResourceCatalog.GetResource(theWorkspace.ID, Guid.Parse(selectedSourceId));
                var result = new List<IOption>();
                if (source.ResourceType == enSourceType.RabbitMQSource.ToString())
                {
                    var options = OptionConvertor.Convert(new RabbitMqOptions());
                    result.AddRange(options);
                }
                
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

        public IResourceCatalog ResourceCatalog
        {
            get
            {
                return _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalog = value;
            }
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => nameof(FindOptions);
    }
}
