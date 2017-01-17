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
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Util;
using Dev2.Workspaces;
using Dev2.Common.Utils;
using System.Text.RegularExpressions;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;
// ReSharper disable NonLocalizedString
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Fetch a service body definition
    /// </summary>
    public class FetchResourceDefinition : IEsbManagementEndpoint
    {
        const string PayloadStart = @"<XamlDefinition>";
        const string PayloadEnd = @"</XamlDefinition>";
        const string AltPayloadStart = @"<Actions>";
        const string AltPayloadEnd = @"</Actions>";

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            StringBuilder tmp;
            requestArgs.TryGetValue("ResourceID", out tmp);
            if (tmp != null)
            {
                Guid resourceId;
                if (Guid.TryParse(tmp.ToString(), out resourceId))
                {
                    return resourceId;
                }
            }

            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.View;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                var res = new ExecuteMessage { HasError = false };
                string serviceId = null;
                bool prepairForDeployment = false;
                StringBuilder tmp;
                values.TryGetValue(@"ResourceID", out tmp);

                if (tmp != null)
                {
                    serviceId = tmp.ToString();
                }

                values.TryGetValue(@"PrepairForDeployment", out tmp);

                if (tmp != null)
                {
                    prepairForDeployment = bool.Parse(tmp.ToString());
                }

                Guid resourceId;
                Guid.TryParse(serviceId, out resourceId);

                Dev2Logger.Info($"Fetch Resource definition. ResourceId: {resourceId}");
                return GetResourceDefinition(theWorkspace, serializer, res, prepairForDeployment, resourceId);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err);
                throw;
            }
        }

        public StringBuilder GetResourceDefinition(IWorkspace theWorkspace, Dev2JsonSerializer serializer, ExecuteMessage res, bool prepairForDeployment, Guid resourceId)
        {
            try
            {

                var result = ResourceCatalog.Instance.GetResourceContents(theWorkspace.ID, resourceId);
                if (!result.IsNullOrEmpty())
                {
                    var tempResource = new Resource(result.ToXElement());
                    var resource = tempResource;

                    if (resource.ResourceType == @"DbSource")
                    {
                        res.Message.Append(result);
                    }
                    else
                    {
                        DoWorkflowServiceMessage(result, res);
                    }
                }
            }
            catch (ServiceNotAuthorizedException ex)
            {
                res.Message = ex.Message.ToStringBuilder();
                res.HasError = true;
                return serializer.SerializeToBuilder(res);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(string.Format(ErrorResource.ErrorGettingResourceDefinition, resourceId), e);
            }

            if (!res.Message.IsNullOrEmpty())
            {
                Dev2XamlCleaner dev2XamlCleaner = new Dev2XamlCleaner();
                res.Message = dev2XamlCleaner.StripNaughtyNamespaces(res.Message);
            }
            if (prepairForDeployment)
            {
                try
                {
                    res.Message = DecryptAllPasswords(res.Message);
                }
                catch (CryptographicException e)
                {
                    Dev2Logger.Error(@"Encryption had issues.", e);
                }
            }


            return serializer.SerializeToBuilder(res);
        }

        private static void DoWorkflowServiceMessage(StringBuilder result, ExecuteMessage res)
        {
            var startIdx = result.IndexOf(PayloadStart, 0, false);

            if(startIdx >= 0)
            {
                // remove beginning junk
                startIdx += PayloadStart.Length;
                result = result.Remove(0, startIdx);

                startIdx = result.IndexOf(PayloadEnd, 0, false);

                if(startIdx > 0)
                {
                    var len = result.Length - startIdx;
                    result = result.Remove(startIdx, len);

                    res.Message.Append(result.Unescape());
                }
            }
            else
            {
                // handle services ;)
                startIdx = result.IndexOf(AltPayloadStart, 0, false);
                if(startIdx >= 0)
                {
                    // remove begging junk
                    startIdx += AltPayloadStart.Length;
                    result = result.Remove(0, startIdx);

                    startIdx = result.IndexOf(AltPayloadEnd, 0, false);

                    if(startIdx > 0)
                    {
                        var len = result.Length - startIdx;
                        result = result.Remove(startIdx, len);

                        res.Message.Append(result.Unescape());
                    }
                }
                else
                {
                    // send the entire thing ;)
                    res.Message.Append(result);
                }
            }
        }

        public StringBuilder DecryptAllPasswords(StringBuilder stringBuilder)
        {
            Dictionary<string, StringTransform> replacements = new Dictionary<string, StringTransform>
                                                               {
                                                                   {
                                                                       "Source", new StringTransform
                                                                                 {
                                                                                     SearchRegex = new Regex(@"<Source ID=""[a-fA-F0-9\-]+"" .*ConnectionString=""([^""]+)"" .*>"),
                                                                                     GroupNumbers = new[] { 1 },
                                                                                     TransformFunction = DpapiWrapper.DecryptIfEncrypted
                                                                                 }
                                                                   },
                                                                   {
                                                                       "DsfAbstractFileActivity", new StringTransform
                                                                                                  {
                                                                                                      SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(DsfFileWrite|DsfFileRead|DsfFolderRead|DsfPathCopy|DsfPathCreate|DsfPathDelete|DsfPathMove|DsfPathRename|DsfZip|DsfUnzip) .*?Password=""([^""]+)"" .*?&gt;"),
                                                                                                      GroupNumbers = new[] { 3 },
                                                                                                      TransformFunction = DpapiWrapper.DecryptIfEncrypted
                                                                                                  }
                                                                   },
                                                                   {
                                                                       "DsfAbstractMultipleFilesActivity", new StringTransform
                                                                                                           {
                                                                                                               SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(DsfPathCopy|DsfPathMove|DsfPathRename|DsfZip|DsfUnzip) .*?DestinationPassword=""([^""]+)"" .*?&gt;"),
                                                                                                               GroupNumbers = new[] { 3 },
                                                                                                               TransformFunction = DpapiWrapper.DecryptIfEncrypted
                                                                                                           }
                                                                   },
                                                                   {
                                                                       "Zip", new StringTransform
                                                                              {
                                                                                  SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(DsfZip|DsfUnzip) .*?ArchivePassword=""([^""]+)"" .*?&gt;"),
                                                                                  GroupNumbers = new[] { 3 },
                                                                                  TransformFunction = DpapiWrapper.DecryptIfEncrypted
                                                                              }
                                                                   }
                                                               };
            string xml = stringBuilder.ToString();
            StringBuilder output = new StringBuilder();

            xml = StringTransform.TransformAllMatches(xml, replacements.Values.ToList());
            output.Append(xml);
            return output;
        }

        public DynamicService CreateServiceEntry()
        {
            var serviceAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var serviceEntry = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            serviceEntry.Actions.Add(serviceAction);

            return serviceEntry;
        }

        public string HandlesType()
        {
            return @"FetchResourceDefinitionService";
        }

    }
}
