using System;
using System.Collections.Generic;
using System.Linq;
using System.Parsing.Intellisense;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;
using Dev2.Studio.Core;

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
            // Sashen: 09-10-2012 : Using the new parser
            var intellisenseParser = new SyntaxTreeBuilder();

            IList<string> result = new List<string>();
            Node[] nodes = intellisenseParser.Build(activityField);

            // No point in continuing ;)
            if (nodes == null)
            {
                return result;
            }

            if (intellisenseParser.EventLog.HasEventLogs)
            {
                IDev2StudioDataLanguageParser languageParser = DataListFactory.CreateStudioLanguageParser();

                try
                {
                    result = languageParser.ParseForActivityDataItems(activityField);
                }
                catch (Dev2DataLanguageParseError)
                {
                    return new List<String>();
                }
                catch (NullReferenceException)
                {
                    return new List<String>();
                }

            }
            var allNodes = new List<Node>();


            if (nodes.Any() && !(intellisenseParser.EventLog.HasEventLogs))
            {
                nodes[0].CollectNodes(allNodes);

                for(int i = 0; i < allNodes.Count; i++)
                {
                    if (allNodes[i] is DatalistRecordSetNode)
                    {
                        var refNode = allNodes[i] as DatalistRecordSetNode;
                        string nodeName = refNode.GetRepresentationForEvaluation();
                        nodeName = nodeName.Substring(2, nodeName.Length - 4);
                        result.Add(nodeName);
                    }
                    else if (allNodes[i] is DatalistReferenceNode)
                    {
                        var refNode = allNodes[i] as DatalistReferenceNode;
                        string nodeName = refNode.GetRepresentationForEvaluation();
                        nodeName = nodeName.Substring(2, nodeName.Length - 4);
                        result.Add(nodeName);
                    }
                    else if (allNodes[i] is DatalistRecordSetFieldNode)
                    {
                        var refNode = allNodes[i] as DatalistRecordSetFieldNode;
                        string nodeName = refNode.GetRepresentationForEvaluation();
                        nodeName = nodeName.Substring(2, nodeName.Length - 4);
                        result.Add(nodeName);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Finds the missing workflow data regions.
        /// </summary>
        /// <param name="partsToVerify">The parts to verify.</param>
        /// <returns></returns>
        public List<IDataListVerifyPart> MissingWorkflowItems(IList<IDataListVerifyPart> partsToVerify)
        {
            var missingWorkflowParts = new List<IDataListVerifyPart>();

            if (DataListSingleton.ActiveDataList != null && DataListSingleton.ActiveDataList.DataList != null)
                foreach (var dataListItem in DataListSingleton.ActiveDataList.DataList)
                {
                    if (String.IsNullOrEmpty(dataListItem.Name))
                    {
                        continue;
                    }
                    if ((dataListItem.Children.Count > 0))
                    {
                        if (partsToVerify.Count(part => part.Recordset == dataListItem.Name) == 0)
                        {
                            //19.09.2012: massimo.guerrera - Added in the description to creating the part
                            if (dataListItem.IsEditable)
                            {
                                missingWorkflowParts.Add(
                                    IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.Name,
                                                                                              String.Empty,
                                                                                              dataListItem.Description));
                                foreach (var child in dataListItem.Children)
                                    if (!(String.IsNullOrEmpty(child.Name)))
                                        //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                        if (dataListItem.IsEditable)
                                        {
                                            missingWorkflowParts.Add(
                                                IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                                    dataListItem.Name, child.Name, child.Description));
                                        }
                            }
                        }
                        else
                            foreach (var child in dataListItem.Children)
                                if (
                                    partsToVerify.Count(
                                        part => part.Field == child.Name && part.Recordset == child.Parent.Name) == 0)
                                {
                                    //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                    if (child.IsEditable)
                                    {
                                        missingWorkflowParts.Add(
                                            IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                                dataListItem.Name, child.Name, child.Description));
                                    }
                                }
                    }
                    else if (partsToVerify.Count(part => part.Field == dataListItem.Name) == 0)
                    {
                        {
                            if (dataListItem.IsEditable)
                            {
                                //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                missingWorkflowParts.Add(
                                    IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.Name,
                                                                                           dataListItem.Description));
                            }
                        }
                    }
                }

            return missingWorkflowParts;
        }



    }
}
