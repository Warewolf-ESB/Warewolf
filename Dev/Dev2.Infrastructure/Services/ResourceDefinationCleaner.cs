﻿using System;
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
            var serializer = new Dev2JsonSerializer();
            var res = new ExecuteMessage();
            try
            {
                if (!contents.IsNullOrEmpty())
                {
                    var assembly = Assembly.Load("Dev2.Data");
                    var type = assembly.GetType("Dev2.Runtime.ServiceModel.Data.Resource");
                    var instance = Activator.CreateInstance(type, contents.ToXElement());
                    
                     var resource = (IResource)instance;
                    if (resource.ResourceType == @"DbSource")
                    {
                        res.Message.Append(contents);
                    }
                    else
                    {
                        DoWorkflowServiceMessage(contents, res);
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
                    Dev2Logger.Error(@"Encryption had issues.", e, GlobalConstants.WarewolfError);
                }
            }


            return serializer.SerializeToBuilder(res);
        }

        private static void DoWorkflowServiceMessage(StringBuilder result, IExecuteMessage res)
        {
            var workflowResult = result;
            var startIdx = workflowResult.IndexOf(GlobalConstants.PayloadStart, 0, false);
            
            if (startIdx >= 0)
            {
                startIdx += GlobalConstants.PayloadStart.Length;
                workflowResult = workflowResult.Remove(0, startIdx);

                startIdx = result.IndexOf(GlobalConstants.PayloadEnd, 0, false);

                if (startIdx > 0)
                {
                    var len = result.Length - startIdx;
                    workflowResult = workflowResult.Remove(startIdx, len);

                    res.Message.Append(workflowResult.Unescape());
                }
            }
            else
            {
                startIdx = result.IndexOf(GlobalConstants.AltPayloadStart, 0, false);
                if (startIdx >= 0)
                {
                    startIdx += GlobalConstants.AltPayloadStart.Length;
                    workflowResult = workflowResult.Remove(0, startIdx);

                    startIdx = result.IndexOf(GlobalConstants.AltPayloadEnd, 0, false);

                    if (startIdx > 0)
                    {
                        var len = result.Length - startIdx;
                        workflowResult = workflowResult.Remove(startIdx, len);

                        res.Message.Append(workflowResult.Unescape());
                    }
                }
                else
                {
                    res.Message.Append(workflowResult);
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
            var xml = stringBuilder.ToString();
            var output = new StringBuilder();

            xml = StringTransform.TransformAllMatches(xml, replacements.Values.ToList());
            output.Append(xml);
            return output;
        }
    }
}
