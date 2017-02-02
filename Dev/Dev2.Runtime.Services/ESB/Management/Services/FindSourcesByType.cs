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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find resources by type
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FindSourcesByType : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                string type = null;
                StringBuilder tmp;
                values.TryGetValue("Type", out tmp);
                if (tmp != null)
                {
                    type = tmp.ToString();
                }

                if (string.IsNullOrEmpty(type))
                {
                    // ReSharper disable NotResolvedInText
                    throw new ArgumentNullException("type");
                    // ReSharper restore NotResolvedInText
                }
                Dev2Logger.Info("Find Sources By Type. " + type);
                enSourceType sourceType;
                if (Enum.TryParse(type, true, out sourceType))
                {
                    var result = ResourceCatalog.Instance.GetModels(theWorkspace.ID, sourceType);
                    if (result != null)
                    {
                        Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                        return serializer.SerializeToBuilder(result);
                    }
                }
                return new StringBuilder();
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var findSourcesByTypeAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            var findSourcesByTypeService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            findSourcesByTypeService.Actions.Add(findSourcesByTypeAction);

            return findSourcesByTypeService;
        }

        public string HandlesType()
        {
            return "FindSourcesByType";
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }
}
