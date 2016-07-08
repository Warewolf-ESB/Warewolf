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
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Parsing.Intellisense;
using Dev2.Data.Exceptions;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Utils
{
    /// <summary>
    /// Utilities for the workflow designer view model
    /// </summary>
    public class WorkflowDesignerUtils
    {
        /// <summary>
        /// Is the list.
        /// </summary>
        /// <param name="activityField">The activity field.</param>
        /// <returns></returns>
        public IList<string> FormatDsfActivityField(string activityField)
        {
            //2013.06.10: Ashley Lewis for bug 9306 - handle the case of miss-matched region braces

            IList<string> result = new List<string>();

            var regions = DataListCleaningUtils.SplitIntoRegionsForFindMissing(activityField);
            foreach(var region in regions)
            {
                // Sashen: 09-10-2012 : Using the new parser
                var intellisenseParser = new SyntaxTreeBuilder();

                Node[] nodes = intellisenseParser.Build(region);

                // No point in continuing ;)
                if(nodes == null)
                {
                    return result;
                }

                if(intellisenseParser.EventLog.HasEventLogs)
                {
                    IDev2StudioDataLanguageParser languageParser = DataListFactory.CreateStudioLanguageParser();

                    try
                    {
                        result = languageParser.ParseForActivityDataItems(region);
                    }
                    catch(Dev2DataLanguageParseError)
                    {
                        return new List<string>();
                    }
                    catch(NullReferenceException)
                    {
                        return new List<string>();
                    }

                }
                var allNodes = new List<Node>();


                if(nodes.Any() && !intellisenseParser.EventLog.HasEventLogs)
                {
                    nodes[0].CollectNodes(allNodes);

                    // ReSharper disable once ForCanBeConvertedToForeach
                    for(int i = 0; i < allNodes.Count; i++)
                    {
                        if(allNodes[i] is DatalistRecordSetNode)
                        {
                            var refNode = allNodes[i] as DatalistRecordSetNode;
                            string nodeName = refNode.GetRepresentationForEvaluation();
                            nodeName = nodeName.Substring(2, nodeName.Length - 4);
                            result.Add(nodeName);
                        }
                        else if(allNodes[i] is DatalistReferenceNode)
                        {
                            var refNode = allNodes[i] as DatalistReferenceNode;
                            string nodeName = refNode.GetRepresentationForEvaluation();
                            nodeName = nodeName.Substring(2, nodeName.Length - 4);
                            result.Add(nodeName);
                        }

                    }
                }
            }
            try
            {
                FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(activityField);
            }
            catch(Exception)
            {
                return result.Where(lang => activityField.Contains("[[" + lang + "]]")).ToList();
            }

            return result;
        }


        protected static IEnvironmentModel ActiveEnvironment { get; set; }

        public static void CheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource, IEnvironmentModel contextEnv)
        {
            
            if(resource != null && resource.ResourceType == ResourceType.WorkflowService && contextEnv != null)
            {
                if(contextEnv.ID != resource.Environment.ID)
                {
                    dsfActivity.ServiceUri = resource.Environment.Connection.WebServerUri.AbsoluteUri;
                    dsfActivity.ServiceServer = resource.Environment.ID;
                  
                }
                dsfActivity.FriendlySourceName = new InArgument<string>(resource.Environment.Connection.WebServerUri.Host);
            }

        }

        /// <summary>
        /// Finds the missing workflow data regions.
        /// </summary>
        /// <param name="partsToVerify">The parts to verify.</param>
        /// <param name="excludeUnusedItems"></param>
        /// <returns></returns>
        public List<IDataListVerifyPart> MissingWorkflowItems(IList<IDataListVerifyPart> partsToVerify, bool excludeUnusedItems = false)
        {
            var missingWorkflowParts = new List<IDataListVerifyPart>();

            if(DataListSingleton.ActiveDataList != null)
            {
                if (DataListSingleton.ActiveDataList.ScalarCollection != null)
                {
                    foreach (var dataListItem in DataListSingleton.ActiveDataList.ScalarCollection)
                    {
                        if (String.IsNullOrEmpty(dataListItem.DisplayName))
                        {
                            continue;
                        }
                        if (partsToVerify.Count(part => part.Field == dataListItem.DisplayName) == 0)
                        {

                            if (dataListItem.IsEditable)
                            {
                                // skip it if unused and exclude is on ;)
                                if (excludeUnusedItems && !dataListItem.IsUsed)
                                {
                                    continue;
                                }

                                //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                missingWorkflowParts.Add(
                                    IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.DisplayName,
                                                                                           dataListItem.Description));
                            }

                        }
                    }
                }

                if (DataListSingleton.ActiveDataList.RecsetCollection != null)
                {
                    foreach (var dataListItem in DataListSingleton.ActiveDataList.RecsetCollection)
                    {
                        if (String.IsNullOrEmpty(dataListItem.DisplayName))
                        {
                            continue;
                        }
                        if (dataListItem.Children.Count > 0)
                        {
                            if (partsToVerify.Count(part => part.Recordset == dataListItem.DisplayName) == 0)
                            {
                                //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                if (dataListItem.IsEditable)
                                {
                                    // skip it if unused and exclude is on ;)
                                    if (excludeUnusedItems && !dataListItem.IsUsed)
                                    {
                                        continue;
                                    }
                                    missingWorkflowParts.Add(
                                        IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.DisplayName,
                                                                                                  String.Empty,
                                                                                                  dataListItem.Description));
                                    // ReSharper disable once LoopCanBeConvertedToQuery
                                    foreach (var child in dataListItem.Children)
                                    {
                                        if (!String.IsNullOrEmpty(child.DisplayName))
                                        {
                                            //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                            if (dataListItem.IsEditable)
                                            {
                                                missingWorkflowParts.Add(
                                                    IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                                        dataListItem.DisplayName, child.DisplayName, child.Description));
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // ReSharper disable once LoopCanBeConvertedToQuery
                                foreach (var child in dataListItem.Children)
                                    if (partsToVerify.Count(part => part.Field == child.DisplayName && part.Recordset == child.Parent.DisplayName) == 0)
                                    {
                                        //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                        if (child.IsEditable)
                                        {
                                            // skip it if unused and exclude is on ;)
                                            if (excludeUnusedItems && !dataListItem.IsUsed)
                                            {
                                                continue;
                                            }

                                            missingWorkflowParts.Add(
                                                IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                                    dataListItem.DisplayName, child.DisplayName, child.Description));
                                        }
                                    }
                            }
                        }
                        else if (partsToVerify.Count(part => part.Field == dataListItem.DisplayName) == 0)
                        {

                            if (dataListItem.IsEditable)
                            {
                                // skip it if unused and exclude is on ;)
                                if (excludeUnusedItems && !dataListItem.IsUsed)
                                {
                                    continue;
                                }

                                //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                missingWorkflowParts.Add(
                                    IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.DisplayName,
                                                                                           dataListItem.Description));
                            }

                        }
                    }
                }
            }

            return missingWorkflowParts;
        }
        
    }
}
