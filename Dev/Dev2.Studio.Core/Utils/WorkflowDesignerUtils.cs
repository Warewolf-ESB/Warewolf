#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using Dev2.Data.Parsers;

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
                    var languageParser = new Dev2DataLanguageParser();

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



