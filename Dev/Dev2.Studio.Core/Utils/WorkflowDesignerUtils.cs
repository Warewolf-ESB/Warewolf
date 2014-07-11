using System;
using System.Collections.Generic;
using System.Linq;
using System.Parsing.Intellisense;
using System.Windows;
using Caliburn.Micro;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Unlimited.Applications.BusinessDesignStudio.Activities;

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


                if(nodes.Any() && !(intellisenseParser.EventLog.HasEventLogs))
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


            return result;
        }

        public static void CheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource)
        {
            if(Application.Current != null &&
                Application.Current.Dispatcher.CheckAccess()
                && Application.Current.MainWindow != null)
            {
                dynamic mvm = Application.Current.MainWindow.DataContext;
                if(mvm != null && mvm.ActiveItem != null)
                {
                    CheckIfRemoteWorkflowAndSetProperties(dsfActivity, resource, mvm.ActiveItem.Environment);
                }
            }
        }

        public static void CheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource, IEnvironmentModel contextEnv)
        {
            if(resource != null && resource.ResourceType == ResourceType.WorkflowService && contextEnv != null)
            {
                if(contextEnv.ID != resource.Environment.ID)
                {
                    dsfActivity.ServiceUri = resource.Environment.Connection.WebServerUri.AbsoluteUri;
                    dsfActivity.ServiceServer = resource.Environment.ID;
                }
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

            if(DataListSingleton.ActiveDataList != null && DataListSingleton.ActiveDataList.DataList != null)
            {
                foreach(var dataListItem in DataListSingleton.ActiveDataList.DataList)
                {
                    if(String.IsNullOrEmpty(dataListItem.Name))
                    {
                        continue;
                    }
                    if((dataListItem.Children.Count > 0))
                    {
                        if(partsToVerify.Count(part => part.Recordset == dataListItem.Name) == 0)
                        {
                            //19.09.2012: massimo.guerrera - Added in the description to creating the part
                            if(dataListItem.IsEditable)
                            {
                                // skip it if unused and exclude is on ;)
                                if(excludeUnusedItems && !dataListItem.IsUsed)
                                {
                                    continue;
                                }
                                missingWorkflowParts.Add(
                                    IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.Name,
                                                                                              String.Empty,
                                                                                              dataListItem.Description));
                                // ReSharper disable once LoopCanBeConvertedToQuery
                                foreach(var child in dataListItem.Children)
                                {
                                    if(!(String.IsNullOrEmpty(child.Name)))
                                    {
                                        //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                        if(dataListItem.IsEditable)
                                        {
                                            missingWorkflowParts.Add(
                                                IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                                    dataListItem.Name, child.Name, child.Description));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // ReSharper disable once LoopCanBeConvertedToQuery
                            foreach(var child in dataListItem.Children)
                                if(partsToVerify.Count(part => part.Field == child.Name && part.Recordset == child.Parent.Name) == 0)
                                {
                                    //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                    if(child.IsEditable)
                                    {
                                        // skip it if unused and exclude is on ;)
                                        if(excludeUnusedItems && !dataListItem.IsUsed)
                                        {
                                            continue;
                                        }

                                        missingWorkflowParts.Add(
                                            IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                                dataListItem.Name, child.Name, child.Description));
                                    }
                                }
                        }
                    }
                    else if(partsToVerify.Count(part => part.Field == dataListItem.Name) == 0)
                    {

                        if(dataListItem.IsEditable)
                        {
                            // skip it if unused and exclude is on ;)
                            if(excludeUnusedItems && !dataListItem.IsUsed)
                            {
                                continue;
                            }

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

        public static void EditResource(IResourceModel resource, IEventAggregator eventAggregator)
        {
            if(eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if(resource != null)
            {
                switch(resource.ResourceType)
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

        public static void ShowExampleWorkflow(string activityName, IEnvironmentModel environment, IPopupController popupController)
        {
            var resourceID = GetExampleID(activityName);
            var resource = environment.ResourceRepository
                      .FindSingle(r => r.ID.Equals(resourceID));

            if(resource == null)
            {
                if(popupController == null)
                {
                    var message =
                        string.Format(
                            StringResources.ExampleWorkflowNotFound,
                            GetExampleName(activityName));
                    MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    popupController.Buttons = MessageBoxButton.OK;
                    popupController.Description = string.Format(StringResources.ExampleWorkflowNotFound, resourceID);
                    popupController.Header = "Example Workflow Not Found";
                    popupController.ImageType = MessageBoxImage.Information;
                    popupController.Show();
                }
            }
            else
            {
                resource.ResourceType = ResourceType.WorkflowService;
                EditResource(resource, EventPublishers.Aggregator);
            }
        }

        private static Guid GetExampleID(string activityName)
        {
            var exampleIDString = ResolveExampleResource
                .ResourceManager
                .GetString(activityName);
            Guid exampleID;
            Guid.TryParse(exampleIDString, out exampleID);
            return exampleID;
        }

        private static string GetExampleName(string activityName)
        {
            return ResolveExampleResourceNames
                .ResourceManager
                .GetString(activityName);
        }
    }
}
