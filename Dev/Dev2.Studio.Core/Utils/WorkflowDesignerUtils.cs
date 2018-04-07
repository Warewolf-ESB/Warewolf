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
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Parsing.Intellisense;
using Dev2.Data.Exceptions;
using Dev2.DataList.Contract;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Dev2.Data.Interfaces;

namespace Dev2.Utils
{
    public class WorkflowDesignerUtils
    {
        public IList<string> FormatDsfActivityField(string activityField)
        {
            IList<string> result = new List<string>();

            var regions = DataListCleaningUtils.SplitIntoRegionsForFindMissing(activityField);
            foreach(var region in regions)
            {
                // Sashen: 09-10-2012 : Using the new parser
                var intellisenseParser = new SyntaxTreeBuilder();

                var nodes = intellisenseParser.Build(region);

                // No point in continuing ;)
                if (nodes == null)
                {
                    return result;
                }

                if (intellisenseParser.EventLog.HasEventLogs)
                {
                    var languageParser = DataListFactory.CreateStudioLanguageParser();

                    try
                    {
                        result = languageParser.ParseForActivityDataItems(region);
                    }
                    catch (Dev2DataLanguageParseError)
                    {
                        return new List<string>();
                    }
                    catch (NullReferenceException)
                    {
                        return new List<string>();
                    }

                }
                var allNodes = new List<Node>();
                result = FormatDsfActivityField(result, intellisenseParser, nodes, allNodes);
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

        static IList<string> FormatDsfActivityField(IList<string> result, SyntaxTreeBuilder intellisenseParser, Node[] nodes, List<Node> allNodes)
        {
            if (nodes.Any() && !intellisenseParser.EventLog.HasEventLogs)
            {
                nodes[0].CollectNodes(allNodes);

                for (int i = 0; i < allNodes.Count; i++)
                {
                    result = FormatDsfActivityField(result, allNodes, i);
                }
            }
            return result;
        }

        static IList<string> FormatDsfActivityField(IList<string> result, List<Node> allNodes, int i)
        {
            if (allNodes[i] is DatalistRecordSetNode)
            {
                var refNode = allNodes[i] as DatalistRecordSetNode;
                var nodeName = refNode.GetRepresentationForEvaluation();
                nodeName = nodeName.Substring(2, nodeName.Length - 4);
                result.Add(nodeName);
            }
            else
            {
                if (allNodes[i] is DatalistReferenceNode)
                {
                    var refNode = allNodes[i] as DatalistReferenceNode;
                    var nodeName = refNode.GetRepresentationForEvaluation();
                    nodeName = nodeName.Substring(2, nodeName.Length - 4);
                    result.Add(nodeName);
                }
            }
            return result;
        }

        protected static IServer ActiveEnvironment { get; set; }

        public static void CheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource, IServer contextEnv)
        {
            
            if(resource != null && resource.ResourceType == ResourceType.WorkflowService && contextEnv != null)
            {
                if(contextEnv.EnvironmentID != resource.Environment.EnvironmentID)
                {
                    dsfActivity.ServiceUri = resource.Environment.Connection.WebServerUri.AbsoluteUri;
                    dsfActivity.ServiceServer = resource.Environment.EnvironmentID;
                  
                }
                dsfActivity.FriendlySourceName = new InArgument<string>(resource.Environment.Connection.WebServerUri.Host);
            }

        }
    }
}



