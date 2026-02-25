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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Utils;
using Dev2.Communication;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;

namespace Dev2
{
    public class ResourceDefinationCleaner : IResourceDefinationCleaner
    {
        public StringBuilder GetResourceDefinition( bool prepairForDeployment, Guid resourceId, StringBuilder contents)
        {
            Dev2Logger.Debug($"GetResourceDefinition called for resource: {resourceId}", GlobalConstants.WarewolfError);
            Dev2Logger.Debug($"PrepareForDeployment={prepairForDeployment}, contents null/empty={contents.IsNullOrEmpty()}", GlobalConstants.WarewolfError);
            var serializer = new Dev2JsonSerializer();
            //return serializer.SerializeToBuilder(this.GetRawResourceDefinition(prepairForDeployment, resourceId, contents));

            var res = new ExecuteMessage();
            try
            {
                if (!contents.IsNullOrEmpty())
                {
                    Dev2Logger.Debug("Attempting to load Dev2.Data assembly and construct Resource instance.", GlobalConstants.WarewolfError);
                    var assembly = Assembly.Load("Dev2.Data");
                    var type = assembly.GetType("Dev2.Runtime.ServiceModel.Data.Resource");
                    if (type == null)
                    {
                        Dev2Logger.Error($"Could not find type 'Dev2.Runtime.ServiceModel.Data.Resource' in assembly Dev2.Data for resource {resourceId}.", GlobalConstants.WarewolfError);
                    }
                    else
                    {
                        var instance = Activator.CreateInstance(type, contents.ToXElement());
                        var resource = (IResource)instance;
                        Dev2Logger.Info($"Loaded resource. Type: {resource.GetType().FullName}, ResourceType: {resource.ResourceType}", GlobalConstants.WarewolfError);
                        if (resource.ResourceType == @"DbSource")
                        {
                            Dev2Logger.Debug("Resource is DbSource - returning raw contents.", GlobalConstants.WarewolfError);
                            res.Message.Append(contents);
                        }
                        else
                        {
                            Dev2Logger.Debug("Processing workflow service message payload.", GlobalConstants.WarewolfError);
                            DoWorkflowServiceMessage(contents, res);
                            Dev2Logger.Debug($"After payload extraction, message length: {res.Message.Length}", GlobalConstants.WarewolfError);
                        }
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
                Dev2Logger.Error(string.Format(ErrorResource.ErrorGettingResourceDefinition, resourceId), e, GlobalConstants.WarewolfError);
            }

            if (!res.Message.IsNullOrEmpty())
            {
                var dev2XamlCleaner = new Dev2XamlCleaner();
                Dev2Logger.Debug("Stripping naughty namespaces from resource message.", GlobalConstants.WarewolfError);
                res.Message = dev2XamlCleaner.StripNaughtyNamespaces(res.Message);
                Dev2Logger.Debug($"After StripNaughtyNamespaces, length: {res.Message.Length}", GlobalConstants.WarewolfError);
            }
            if (prepairForDeployment)
            {
                try
                {
                    Dev2Logger.Debug("Decrypting all passwords in resource message (prepare for deployment).", GlobalConstants.WarewolfError);
                    res.Message = DecryptAllPasswords(res.Message);
                    Dev2Logger.Debug($"After DecryptAllPasswords, length: {res.Message.Length}", GlobalConstants.WarewolfError);
                }
                catch (CryptographicException e)
                {
                    Dev2Logger.Error(@"Encryption had issues.", e, GlobalConstants.WarewolfError);
                }
            }

            return serializer.SerializeToBuilder(res);
        }

        public IExecuteMessage GetRawResourceDefinition(bool prepairForDeployment, Guid resourceId, StringBuilder contents)
        {
            Dev2Logger.Debug($"GetRawResourceDefinition called for resource: {resourceId}", GlobalConstants.WarewolfError);
            Dev2Logger.Debug($"PrepareForDeployment={prepairForDeployment}, contents null/empty={contents.IsNullOrEmpty()}", GlobalConstants.WarewolfError);
            var result = new ExecuteMessage(); 
            try
            {
                if (!contents.IsNullOrEmpty())
                {
                    Dev2Logger.Debug("Attempting to load Dev2.Data assembly and construct Resource instance (raw).", GlobalConstants.WarewolfError);
                    var assembly = Assembly.Load("Dev2.Data");
                    var type = assembly.GetType("Dev2.Runtime.ServiceModel.Data.Resource");
                    if (type == null)
                    {
                        Dev2Logger.Error($"Could not find type 'Dev2.Runtime.ServiceModel.Data.Resource' in assembly Dev2.Data for resource {resourceId}.", GlobalConstants.WarewolfError);
                    }
                    else
                    {
                        var instance = Activator.CreateInstance(type, contents.ToXElement());
                        var resource = (IResource)instance;
                        Dev2Logger.Info($"Loaded resource (raw). Type: {resource.GetType().FullName}, ResourceType: {resource.ResourceType}", GlobalConstants.WarewolfError);
                        if (resource.ResourceType == @"DbSource")
                        {
                            Dev2Logger.Debug("Resource is DbSource - returning raw contents (raw).", GlobalConstants.WarewolfError);
                            result.Message.Append(contents);
                        }
                        else
                        {
                            Dev2Logger.Debug("Processing workflow service message payload (raw).", GlobalConstants.WarewolfError);
                            DoWorkflowServiceMessage(contents, result);
                            Dev2Logger.Debug($"After payload extraction (raw), message length: {result.Message.Length}", GlobalConstants.WarewolfError);
                        }
                    }
                }
            }
            catch (ServiceNotAuthorizedException ex)
            {
                result.Message = ex.Message.ToStringBuilder();
                result.HasError = true;
                return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(string.Format(ErrorResource.ErrorGettingResourceDefinition, resourceId), e, GlobalConstants.WarewolfError);
            }

            if (!result.Message.IsNullOrEmpty())
            {
                var dev2XamlCleaner = new Dev2XamlCleaner();
                result.Message = dev2XamlCleaner.StripNaughtyNamespaces(result.Message);
            }
            if (prepairForDeployment)
            {
                try
                {
                    result.Message = DecryptAllPasswords(result.Message);
                }
                catch (CryptographicException e)
                {
                    Dev2Logger.Error(@"Encryption had issues.", e, GlobalConstants.WarewolfError);
                }
            }

            return result;
        }

        private static void DoWorkflowServiceMessage(StringBuilder result, IExecuteMessage res)
        {
            var workflowResult = result;
            var startIdx = workflowResult.IndexOf(GlobalConstants.PayloadStart, 0, false);
            Dev2Logger.Debug($"DoWorkflowServiceMessage: looking for PayloadStart at index {startIdx}.", GlobalConstants.WarewolfError);

            if (startIdx >= 0)
            {
                startIdx += GlobalConstants.PayloadStart.Length;
                workflowResult = workflowResult.Remove(0, startIdx);

                startIdx = result.IndexOf(GlobalConstants.PayloadEnd, 0, false);
                Dev2Logger.Debug($"DoWorkflowServiceMessage: found PayloadEnd at index {startIdx}.", GlobalConstants.WarewolfError);

                if (startIdx > 0)
                {
                    var len = result.Length - startIdx;
                    workflowResult = workflowResult.Remove(startIdx, len);

                    res.Message.Append(workflowResult.Unescape());
                    Dev2Logger.Debug($"DoWorkflowServiceMessage: appended unescaped payload, length now {res.Message.Length}.", GlobalConstants.WarewolfError);
                }
            }
            else
            {
                startIdx = result.IndexOf(GlobalConstants.AltPayloadStart, 0, false);
                Dev2Logger.Debug($"DoWorkflowServiceMessage: looking for AltPayloadStart at index {startIdx}.", GlobalConstants.WarewolfError);
                if (startIdx >= 0)
                {
                    startIdx += GlobalConstants.AltPayloadStart.Length;
                    workflowResult = workflowResult.Remove(0, startIdx);

                    startIdx = result.IndexOf(GlobalConstants.AltPayloadEnd, 0, false);
                    Dev2Logger.Debug($"DoWorkflowServiceMessage: found AltPayloadEnd at index {startIdx}.", GlobalConstants.WarewolfError);

                    if (startIdx > 0)
                    {
                        var len = result.Length - startIdx;
                        workflowResult = workflowResult.Remove(startIdx, len);

                        res.Message.Append(workflowResult.Unescape());
                        Dev2Logger.Debug($"DoWorkflowServiceMessage: appended unescaped alt payload, length now {res.Message.Length}.", GlobalConstants.WarewolfError);
                    }
                }
                else
                {
                    res.Message.Append(workflowResult);
                    Dev2Logger.Debug($"DoWorkflowServiceMessage: no payload markers found, appended whole message, length now {res.Message.Length}.", GlobalConstants.WarewolfError);
                }
            }
        }

        public StringBuilder DecryptAllPasswords(StringBuilder stringBuilder)
        {
            var replacements = new Dictionary<string, StringTransform>
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
                                                                                                      SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(FileReadWithBase64|DsfFileWrite|DsfFileRead|DsfFolderRead|DsfPathCopy|DsfPathCreate|DsfPathDelete|DsfPathMove|DsfPathRename|DsfZip|DsfUnzip) .*?Password=""([^""]+)"" .*?&gt;"),
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
                                                                   },
                                                                   {
                                                                       "DsfSendEmailActivity", new StringTransform
                                                                              {
                                                                                  SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?DsfSendEmailActivity .*?Password=""([^""]+)"" .*?&gt;"),
                                                                                  GroupNumbers = new[] { 2 },
                                                                                  TransformFunction = DpapiWrapper.DecryptIfEncrypted
                                                                              }
                                                                   }
                                                               };
            var xml = stringBuilder.ToString();
            var output = new StringBuilder();

            xml = StringTransform.TransformAllMatches(xml, replacements.Values.ToList());
            output.Append(xml);
            return output;
        }
    }
}
