using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.TO;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Interfaces;

namespace Dev2.Studio.Core.Wizards
{
    [Export(typeof(IWizardEngine))]
    public class WizardEngine : IWizardEngine
    {
        #region Fields

        private const string _activitySpecificSettingsWizardPrefix = "";
        private const string _activitySpecificSettingsWizardSuffix = ".wiz";

        private const string _systemWizardPrefix = "Dev2";
        private const string _systemWizardSuffix = "Wizard";

        private const string _resourceTypeForWorkflows = "WorkflowService";
        private const string _resourceTypeForService = "Service";

        private IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        readonly IEventAggregator _eventPublisher;

        #endregion Fields

        #region Ctor

        public WizardEngine()
            : this(EventPublishers.Aggregator)
        {
        }

        public WizardEngine(IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
        }

        #endregion Ctor

        #region Properties

        [Import]
        public IServiceLocator ServiceLocator { get; set; }

        [Import]
        public IPopupController Popup { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets invocation details for an activity's wizard.
        /// </summary>
        /// <param name="activity">The model item representing the activity.</param>
        /// <param name="hostResource">The resource which hosts the acticity.</param>
        /// <exception cref="System.Exception">The ServiceLocator is null, please ensure that MEF imports are satisfied on this instance of the WizardEngine.</exception>
        public WizardInvocationTO GetActivityWizardInvocationTO(ModelItem activity, IContextualResourceModel hostResource, IDataListCompiler dataListCompiler = null)
        {
            //
            // Check if ServiceLocator is null
            //
            if(ServiceLocator == null)
            {
                throw new Exception("The ServiceLocator is null, please ensure that MEF imports are satisfied on this instance of the WizardEngine.");
            }

            //
            // Create the wizard ivocation TO
            //
            WizardInvocationTO result = new WizardInvocationTO();
            result.ExecutionStatusCallbackID = Guid.NewGuid();

            //
            // Create TransferDatalist and get it's ID
            //
            IBinaryDataList wizardDataList = GetWizardData(activity);

            IList<IBinaryDataListEntry> entries = wizardDataList.FetchAllEntries();

            foreach(IBinaryDataListEntry entry in entries)
            {
                if(entry.IsRecordset)
                {
                    IIndexIterator indexIterator = entry.FetchRecordsetIndexes();
                    while(indexIterator.HasMore())
                    {
                        int index = indexIterator.FetchNextIndex();

                        string errorString;
                        var record = entry.FetchRecordAt(index, out errorString);
                        foreach(var col in record)
                        {
                            col.HtmlEncodeRegionBrackets();
                        }
                    }
                }
                else
                {
                    IBinaryDataListItem scalar = entry.FetchScalar();
                    scalar.HtmlEncodeRegionBrackets();
                }
            }



            if(dataListCompiler == null)
            {
                dataListCompiler = CreateDatalistCompiler(hostResource);
            }

            if(dataListCompiler == null)
            {
                throw new Exception("Couldn't connect to the datalist server, please ensure you are connected and try again.");
            }

            ErrorResultTO errors;
            result.TransferDatalistID = dataListCompiler.PushBinaryDataList(wizardDataList.UID, wizardDataList, out errors);
            //dataListCompiler.UpsertSystemTag(result.TransferDatalistID, enSystemTag.SubstituteTokens, "true", out errors);

            //
            // Get the wizard endpoint
            //
            if(activity.ItemType == typeof(DsfActivity))
            {
                string activityName = activity.Properties["ServiceName"].ComputedValue.ToString();

                result.WizardTitle = "Setup: " + activityName;

                //
                // Get resource that the DsfActivity represents
                //
                IContextualResourceModel resource = hostResource.Environment.ResourceRepository.FindSingle(r => r.ResourceName == activityName) as IContextualResourceModel;
                if(resource != null)
                {
                    //
                    // Get endpoint
                    //                    
                    DataListMergeOpsTO dataListMergeOpsTO = new DataListMergeOpsTO(result.TransferDatalistID);
                    result.Endpoint = ServiceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.ResourceActivityWizardServiceKey, new Tuple<IContextualResourceModel, DataListMergeOpsTO, Guid>(resource, dataListMergeOpsTO, result.ExecutionStatusCallbackID));
                }
            }
            else
            {
                IEnvironmentModel environment = hostResource.Environment;
                string activityName = activity.ItemType.Name;

                result.WizardTitle = "Setup: " + activityName;

                DataListMergeOpsTO dataListMergeOpsTO = new DataListMergeOpsTO(result.TransferDatalistID);
                result.Endpoint = ServiceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.CodedActivityWizardServiceKey, new Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>(activityName, environment, dataListMergeOpsTO, result.ExecutionStatusCallbackID));
            }

            //
            // Create the appropriate callback handler for the given activity
            //


            IActivitySettingsWizardCallbackHandler callbackHandler = GetActivitySpecificSettingsWizardCallbackHandler(activity.ItemType);

            //Add DataListCompiler
            callbackHandler.CreateCompiler = new Func<IDataListCompiler>(() => CreateDatalistCompiler(hostResource));
            callbackHandler.Activity = activity;
            callbackHandler.DatalistID = result.TransferDatalistID;

            result.CallbackHandler = callbackHandler;

            return result;
        }

        /// <summary>
        /// Gets invocation details for an activity's settings wizard.
        /// </summary>
        /// <param name="activity">The model item representing the activity.</param>
        /// <param name="hostResource">The resource which hosts the acticity.</param>
        /// <exception cref="System.Exception">The ServiceLocator is null, please ensure that MEF imports are satisfied on this instance of the WizardEngine.</exception>
        public WizardInvocationTO GetActivitySettingsWizardInvocationTO(ModelItem activity, IContextualResourceModel hostResource, IDataListCompiler dataListCompiler = null)
        {
            //
            // Check if ServiceLocator is null
            //
            if(ServiceLocator == null)
            {
                throw new Exception("The ServiceLocator is null, please ensure that MEF imports are satisfied on this instance of the WizardEngine.");
            }

            //
            // Create the wizard ivocation TO
            //
            WizardInvocationTO result = new WizardInvocationTO();
            result.ExecutionStatusCallbackID = Guid.NewGuid();
            result.WizardTitle = "Settings";
            //
            // Create Datalist and get it's ID
            //

            IBinaryDataList SettingsDataList = GetGeneralSettingData(activity);
            //TODO - Decide on how to get general settings from an activity
            //activity.
            //IDataListCompiler dataListCompiler = CreateDatalistCompiler(hostResource);

            //
            // Get the wizard endpoint
            //
            IEnvironmentModel environment = hostResource.Environment;
            DataListMergeOpsTO dataListMergeOpsTO = new DataListMergeOpsTO(result.TransferDatalistID);
            result.Endpoint = ServiceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.ActivitySettingsWizardServiceKey, new Tuple<IEnvironmentModel, DataListMergeOpsTO, Guid>(environment, dataListMergeOpsTO, result.ExecutionStatusCallbackID));

            //
            // Create the callback handler
            //
            IActivityGeneralSettingsWizardCallbackHandler callbackHandler;

            if(!ImportService.TryGetExportValue<IActivityGeneralSettingsWizardCallbackHandler>(out callbackHandler))
            {
                throw new Exception("No wizard callback handler found for activity settings.");
            }

            callbackHandler.CreateCompiler = new Func<IDataListCompiler>(() => CreateDatalistCompiler(hostResource));
            callbackHandler.Activity = activity;
            callbackHandler.DatalistID = result.TransferDatalistID;

            result.CallbackHandler = callbackHandler;

            return result;
        }

        /// <summary>
        /// Determines whether the specified activity has a wizard.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>
        ///   <c>true</c> if the specified activity has wizard; otherwise, <c>false</c>.
        /// </returns>
        public bool HasWizard(ModelItem activity, IEnvironmentModel environmentModel)
        {
            if(activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            if(environmentModel == null)
            {
                throw new ArgumentNullException("IEnvironmentModel");
            }

            bool result = false;

            //
            // Get the wizard endpoint
            //
            string wizardName = String.Empty;
            bool isDsfActivity = (activity.ItemType == typeof(DsfActivity) || activity.ItemType.BaseType == typeof(DsfActivity));
            if(isDsfActivity)
            {
                var serviceNameProperty = activity.Properties["ServiceName"];
                if(serviceNameProperty != null && serviceNameProperty.ComputedValue != null)
                {
                    wizardName = GetResourceWizardName(serviceNameProperty.ComputedValue.ToString());
                }
            }
            else
            {
                wizardName = GetCodedActivityWizardName(activity.ItemType.Name);
            }

            if(String.IsNullOrWhiteSpace(wizardName))
                return false;

            var resource = environmentModel.ResourceRepository
                .FindSingle(r => r.ResourceName == wizardName) as IContextualResourceModel;

            if(resource != null)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Gets the parent for the give wizard, if the wizard is not a resource wizzard an exception is thrown, if no parent exists null is returned.
        /// </summary>
        /// <param name="wizardResource">The wizard resource.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Can't get a parent for a resource that is not a wizard. The attempt was made on ' + wizardResource.ResourceName + '.</exception>
        public IContextualResourceModel GetParent(IContextualResourceModel wizardResource)
        {
            if(wizardResource == null)
            {
                return null;
            }

            if(!IsResourceWizard(wizardResource))
            {
                throw new Exception("Can't get a parent for a resource that is not a wizard. The attempt was made on '" + wizardResource.ResourceName + "'.");
            }

            string parentName = GetWizardParentName(wizardResource.ResourceName);
            IContextualResourceModel parentResource = wizardResource.Environment.ResourceRepository.FindSingle(r => r.ResourceName == parentName) as IContextualResourceModel;

            return parentResource;
        }

        /// <summary>
        /// Gets the wizard for the give resource, if the resource is a wizzard an exception is thrown, if no wizard exists null is returned.
        /// </summary>
        /// <param name="parentResource">The parent resource.</param>
        public IContextualResourceModel GetWizard(IContextualResourceModel parentResource)
        {
            if(parentResource == null)
            {
                return null;
            }

            if(IsResourceWizard(parentResource))
            {
                throw new Exception("Can't get a wizard for a resource that is a wizard. The attempt was made on '" + parentResource.ResourceName + "'.");
            }

            string wizardName = GetResourceWizardName(parentResource.ResourceName);
            IContextualResourceModel wizardResource = parentResource.Environment.ResourceRepository.FindSingle(r => r.ResourceName == wizardName) as IContextualResourceModel;

            return wizardResource;
        }

        /// <summary>
        /// Determines whether the specified resource is a system or resource wizard.
        /// </summary>
        /// <param name="resource">The Resource.</param>
        /// <returns>
        ///   <c>true</c> if the specified resource is system or resource wizard; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWizard(IContextualResourceModel resource)
        {
            if(resource == null)
            {
                return false;
            }

            return (resource.ResourceName.StartsWith(_activitySpecificSettingsWizardPrefix) && resource.ResourceName.EndsWith(_activitySpecificSettingsWizardSuffix)) ||
                (resource.ResourceName.StartsWith(_systemWizardPrefix) && resource.ResourceName.EndsWith(_systemWizardSuffix));
        }

        /// <summary>
        /// Determines whether the specified resource is a resource wizard.
        /// </summary>
        /// <param name="resource">The Resource.</param>
        /// <returns>
        ///   <c>true</c> if the specified resource is a resource wizard; otherwise, <c>false</c>.
        /// </returns>
        public bool IsResourceWizard(IContextualResourceModel resource)
        {
            if(resource == null)
            {
                return false;
            }

            return (resource.ResourceName.StartsWith(_activitySpecificSettingsWizardPrefix) && resource.ResourceName.EndsWith(_activitySpecificSettingsWizardSuffix));
        }

        /// <summary>
        /// Determines whether the specified resource is as system wizard.
        /// </summary>
        /// <param name="resource">The Resource.</param>
        /// <returns>
        ///   <c>true</c> if the specified resource is a system wizard; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSystemWizard(IContextualResourceModel resource)
        {
            if(resource == null)
            {
                return false;
            }

            return (resource.ResourceName.StartsWith(_systemWizardPrefix) && resource.ResourceName.EndsWith(_systemWizardSuffix));
        }

        /// <summary>
        /// Command to create a wizard.
        /// </summary>
        /// <param name="parentResource">The parent resource.</param>
        public void CreateResourceWizard(IContextualResourceModel parentResource)
        {
            IContextualResourceModel resource = null;
            string wizardDataListString = GetDataListForWizard(parentResource);

            resource = ResourceModelFactory.CreateResourceModel(parentResource.Environment, _resourceTypeForWorkflows, GetResourceWizardName(parentResource.ResourceName), GetResourceWizardName(parentResource.ResourceName));

            if(resource != null)
            {
                resource.Category = string.Empty;
                resource.DataList = wizardDataListString;
                _eventPublisher.Publish(new AddWorkSurfaceMessage(resource));
                _eventPublisher.Publish(new SaveResourceMessage(resource, false));
                _eventPublisher.Publish(new UpdateResourceMessage(resource));
            }
        }

        /// <summary>
        /// Command to edit a wizard.
        /// </summary>
        /// <param name="parentResource">The parent resource.</param>
        public void EditResourceWizard(IContextualResourceModel parentResource)
        {
            IContextualResourceModel wizardResource = GetWizard(parentResource);
            if(wizardResource != null)
            {
                EditWizard(wizardResource, parentResource);
            }
        }

        /// <summary>
        /// Edits the wizard.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void EditWizard(IContextualResourceModel resource)
        {
            IContextualResourceModel parent = GetParent(resource);
            EditWizard(resource, parent);
        }

        /// <summary>
        /// Edits the wizard.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="parent">The parent.</param>
        /// <exception cref="System.Exception">The resource you are trying to edit is not a wizard.</exception>
        public void EditWizard(IContextualResourceModel resource, IContextualResourceModel parent)
        {
            if(resource != null)
            {
                if(IsResourceWizard(resource))
                {
                    string parentDl = parent.DataList;
                    if(parent.ResourceType == ResourceType.Service)
                    {
                        parentDl = _compiler.GetWizardDataListForService(parent.ServiceDefinition);
                    }
                    else if(parent.ResourceType == ResourceType.WorkflowService)
                    {
                        parentDl = _compiler.GetWizardDataListForWorkflow(parent.DataList);
                    }
                    IList<string> addedList = new List<string>();
                    IList<string> removedList = new List<string>();
                    resource.DataList = MergeWizardDataListsAndReturnDiffs(resource.DataList, parentDl, out addedList, out removedList);

                    _eventPublisher.Publish(new AddWorkSurfaceMessage(resource));
                    _eventPublisher.Publish(new SaveResourceMessage(resource, false));

                    string differencesString = Dev2MessageFactory.CreateStringFromListWithLabel("Added", addedList);
                    differencesString += Dev2MessageFactory.CreateStringFromListWithLabel("Removed", removedList);

                    if(Popup != null && !string.IsNullOrEmpty(differencesString))
                    {
                        Popup.Header = "Wizard Data List Notification";
                        Popup.Description = string.Concat("The following items have changed on the Data List of ", parent.ResourceName, ": ", Environment.NewLine, differencesString);
                        Popup.ImageType = MessageBoxImage.Information;
                        Popup.Buttons = MessageBoxButton.OK;
                        Popup.Show();
                    }

                    //if (addedList.Count > 0 || removedList.Count > 0)
                    //{
                    //    string message = string.Concat("The following items have changed on the Data List of ", parent.ResourceName, ": ");
                    //    DataListChangeNotificationViewModel dataListChangeNotifcationViewModel = new DataListChangeNotificationViewModel(message, addedList, removedList);

                    //    WindowNavigationBehavior.ShowDialog(dataListChangeNotifcationViewModel);
                    //}

                }
                else if(IsSystemWizard(resource))
                {
                    _eventPublisher.Publish(new AddWorkSurfaceMessage(resource));
                }
                else
                {
                    throw new Exception("The resource you are trying to edit is not a wizard.");
                }
            }
        }

        #endregion Methods

        #region Static Methods

        /// <summary>
        /// Gets the wizard name for a resource
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        public static string GetResourceWizardName(string resourceName)
        {
            return string.Concat(_activitySpecificSettingsWizardPrefix, resourceName, _activitySpecificSettingsWizardSuffix);
        }

        /// <summary>
        /// Gets the wizard name for a coded activity
        /// </summary>
        /// <param name="activityName">Name of the coded activity.</param>
        public static string GetCodedActivityWizardName(string activityName)
        {
            return string.Concat(_systemWizardPrefix, activityName, _systemWizardSuffix);
        }

        #endregion Static Methods

        #region Private Methods

        /// <summary>
        /// Merges the wizard data lists and return diffs.
        /// </summary>
        /// <param name="wizDl">The wizard datalist.</param>
        /// <param name="parentDl">The parent datalist.</param>
        /// <param name="addedList">The added list.</param>
        /// <param name="removedList">The removed list.</param>
        /// <returns></returns>
        private string MergeWizardDataListsAndReturnDiffs(string wizDl, string parentDl, out IList<string> addedList, out IList<string> removedList)
        {
            string result = string.Empty;
            ErrorResultTO errors = new ErrorResultTO();
            string fixedWizDl = DataListUtil.ExtractFixedDataList(wizDl);
            WizardDataListMergeTO wizTO = _compiler.MergeFixedWizardDataList(fixedWizDl, parentDl);

            addedList = CreateStringListFromBinaryEntries(wizTO.AddedRegions);
            removedList = CreateStringListFromBinaryEntries(wizTO.RemovedRegions);

            string remainingWizDataList = DataListUtil.ExtractEditableDataList(wizDl);

            if(!string.IsNullOrEmpty(remainingWizDataList))
            {
                string fixedDl = wizTO.IntersectedDataList;
                //Move datalspit tag into Dev2.common
                string finalDl = fixedDl.Insert(fixedDl.IndexOf("</DataList>"), remainingWizDataList);

                result = finalDl;
            }
            else
            {
                result = wizTO.IntersectedDataList;
            }
            return result;
        }

        /// <summary>
        /// Creates a string list from binary entries.
        /// </summary>
        /// <param name="listOfEntries">The list of entries.</param>
        /// <returns></returns>
        private IList<string> CreateStringListFromBinaryEntries(IList<IBinaryDataListEntry> listOfEntries)
        {
            IList<string> result = new List<string>();
            if(listOfEntries != null)
            {
                foreach(IBinaryDataListEntry entry in listOfEntries)
                {
                    if(!entry.IsRecordset)
                    {
                        result.Add(entry.FetchScalar().FieldName);
                    }
                    else
                    {
                        foreach(Dev2Column col in entry.Columns)
                        {
                            result.Add(DataListUtil.CreateRecordsetDisplayValue(entry.Namespace, col.ColumnName, string.Empty));
                        }
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Gets the data list for a wizard.
        /// </summary>
        /// <param name="parentResource">The parent resource.</param>
        /// <returns>DataList string</returns>
        private string GetDataListForWizard(IContextualResourceModel parentResource)
        {
            string result = string.Empty;

            if(parentResource.ResourceType == ResourceType.WorkflowService)
            {
                result = _compiler.GetWizardDataListForWorkflow(parentResource.DataList);
            }
            else if(parentResource.ResourceType == ResourceType.Service)
            {
                result = _compiler.GetWizardDataListForService(parentResource.ServiceDefinition);
            }
            result = DataListUtil.MakeDataListFixed(result);

            return result;
        }

        /// <summary>
        /// Gets the parent name for a wizard
        /// </summary>
        /// <param name="resourceName">Name of the wizard.</param>
        private string GetWizardParentName(string wizardName)
        {
            string result = wizardName;
            if(!string.IsNullOrEmpty(_activitySpecificSettingsWizardPrefix))
            {
                result = result.Replace(_activitySpecificSettingsWizardPrefix, "");
            }
            if(!string.IsNullOrEmpty(_activitySpecificSettingsWizardSuffix))
            {
                result = result.Replace(_activitySpecificSettingsWizardSuffix, "");
            }
            return result;
        }

        /// <summary>
        /// Creates the datalist compiler.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        private IDataListCompiler CreateDatalistCompiler(IContextualResourceModel resource)
        {
            IDataListCompiler compiler = null;
            if(resource == null || resource.Environment == null)
            {
                return compiler;
            }

            if(!resource.Environment.IsConnected)
            {
                resource.Environment.Connect();
            }

            if(resource.Environment.IsConnected)
            {
                compiler = DataListFactory.CreateDataListCompiler(resource.Environment.DataListChannel);
            }

            return compiler;
        }

        /// <summary>
        /// Gets the activity specific callback handler.
        /// </summary>
        /// <param name="activityType">Type of the activity.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No wizard callback handler found for activity type ' + activityType.ToString() + '.</exception>
        private IActivitySettingsWizardCallbackHandler GetActivitySpecificSettingsWizardCallbackHandler(Type activityType)
        {
            IActivitySettingsWizardCallbackHandler handler;

            Type contract = typeof(IActivitySpecificSettingsWizardCallbackHandler<>).MakeGenericType(new Type[] { activityType });
            MethodInfo mi = typeof(ImportService).GetMethods().Where(m => m.IsGenericMethod && m.Name == "GetExportValue" && m.GetParameters().Length == 0).First().MakeGenericMethod(contract);

            try
            {
                handler = mi.Invoke(null, null) as IActivitySettingsWizardCallbackHandler;
            }
            catch(Exception e)
            {
                throw new Exception("No wizard callback handler found for activity type '" + activityType.ToString() + "'.", e);
            }

            return handler;
        }

        /// <summary>
        /// Gets the settings data.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        private IBinaryDataList GetGeneralSettingData(ModelItem activity)
        {
            IWizardEditable wizardEditable = activity.GetCurrentValue() as IWizardEditable;
            if(wizardEditable != null)
            {
                return wizardEditable.GetGeneralSettingData();
            }

            return null;
        }

        /// <summary>
        /// Gets the wizard data.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        private IBinaryDataList GetWizardData(ModelItem activity)
        {
            IWizardEditable wizardEditable = activity.GetCurrentValue() as IWizardEditable;
            if(wizardEditable != null)
            {
                return wizardEditable.GetWizardData();
            }
            return null;
        }

        /// <summary>
        /// Gets the inputs for an activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        private IBinaryDataList GetInputs(ModelItem activity)
        {
            IWizardEditable wizardEditable = activity.GetCurrentValue() as IWizardEditable;
            if(wizardEditable != null)
            {
                return wizardEditable.GetInputs();
            }
            return null;
        }

        /// <summary>
        /// Gets the outputs for an activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        private IBinaryDataList GetOutputs(ModelItem activity)
        {
            IWizardEditable wizardEditable = activity.GetCurrentValue() as IWizardEditable;
            if(wizardEditable != null)
            {
                return wizardEditable.GetOutputs();
            }
            return null;
        }

        #endregion Private Methods
    }


}
