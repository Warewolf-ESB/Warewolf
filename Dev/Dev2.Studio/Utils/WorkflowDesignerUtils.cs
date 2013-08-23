using System;
using System.Collections.Generic;
using System.Linq;
using System.Parsing.Intellisense;
using System.Text.RegularExpressions;
using Caliburn.Micro;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Studio.Utils
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
        public IList<String> FormatDsfActivityField(string activityField)
        {
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
                            return new List<String>();
                        }
                        catch(NullReferenceException)
                        {
                            return new List<String>();
                        }

                    }
                    var allNodes = new List<Node>();


                    if(nodes.Any() && !(intellisenseParser.EventLog.HasEventLogs))
                    {
                        nodes[0].CollectNodes(allNodes);

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

            return result;
        }

        public static void EditResource(IResourceModel resource,IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (resource != null)
            {
                switch (resource.ResourceType)
                {
                    case ResourceType.WorkflowService:
                        eventAggregator.Publish(new AddWorkSurfaceMessage(resource));
                        break;

                    case ResourceType.Service:
                        eventAggregator.Publish(new ShowEditResourceWizardMessage(resource));
                        break;
                    case ResourceType.Source:
                        eventAggregator.Publish(new ShowEditResourceWizardMessage(resource));
                        break;
                }
            }

    }
}
}
