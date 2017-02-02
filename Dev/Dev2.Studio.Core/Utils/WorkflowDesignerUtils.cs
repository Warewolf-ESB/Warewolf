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
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Parsing.Intellisense;
using Dev2.Data.Exceptions;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;
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
    }
}



