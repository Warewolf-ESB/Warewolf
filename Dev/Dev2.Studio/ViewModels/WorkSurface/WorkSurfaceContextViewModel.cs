using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Session;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Diagnostics;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.InterfaceImplementors.WizardResourceKeys;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Webs;
using Unlimited.Framework;
using Action = System.Action;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;

namespace Dev2.Studio.ViewModels.WorkSurface
{
    /// <summary>
    /// Class used as unified context across the studio - coordination across different regions
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceContextViewModel : BaseViewModel,
                                 IHandle<SaveResourceMessage>, IHandle<DebugResourceMessage>,
                                 IHandle<ExecuteResourceMessage>
    {
        #region private fields

        private IDataListViewModel _dataListViewModel;
        private IWorkSurfaceViewModel _workSurfaceViewModel;
        private DebugOutputViewModel _debugOutputViewModel;
        private IWindowManager _windowManager;
        private IContextualResourceModel _contextualResourceModel;
        private IFrameworkSecurityContext _securityContext;
        private IEventAggregator _eventAggregator;
        private IWorkspaceItemRepository _workspaceItemRepository;
        #endregion private fields

        #region public properties
        public WorkSurfaceKey WorkSurfaceKey { get; private set; }
        public DebugWriter DebugWriter { get; set; }

        public DebugOutputViewModel DebugOutputViewModel
        {
            get
            {
                return _debugOutputViewModel;
            }
            set
            {
                _debugOutputViewModel = value;
                NotifyOfPropertyChange(() => DebugOutputViewModel);
            }
        } 

        public bool DeleteRequested { get; set; }

        public IDataListViewModel DataListViewModel
        {
            get
            {
                return _dataListViewModel;
            }
            set
            {
                if (_dataListViewModel == value) return;

                _dataListViewModel = value;
                NotifyOfPropertyChange(() => DataListViewModel);
                if (DataListViewModel != null)
                    DataListViewModel.ConductWith(this);
            }
        }

        public IWorkSurfaceViewModel WorkSurfaceViewModel
        {
            get
            {
                return _workSurfaceViewModel;
            }
            set
            {
                if (_workSurfaceViewModel == value) return;

                _workSurfaceViewModel = value;
                NotifyOfPropertyChange(() => WorkSurfaceViewModel);

                var isWorkFlowDesigner = _workSurfaceViewModel is WorkflowDesignerViewModel;
                if (isWorkFlowDesigner)
                {
                    var workFlowDesignerViewModel = (WorkflowDesignerViewModel) _workSurfaceViewModel;
                    _contextualResourceModel = workFlowDesignerViewModel.ResourceModel;
                }

                if (WorkSurfaceViewModel != null)
                    WorkSurfaceViewModel.ConductWith(this);
            }
        }

        #endregion public properties

        public WorkSurfaceContextViewModel(WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
        {
            WorkSurfaceKey = workSurfaceKey;
            WorkSurfaceViewModel = workSurfaceViewModel;

             ImportService.TryGetExportValue(out _windowManager);
             ImportService.TryGetExportValue(out _securityContext);
             ImportService.TryGetExportValue(out _eventAggregator);
             ImportService.TryGetExportValue(out _workspaceItemRepository);

            if (_eventAggregator != null)
                _eventAggregator.Subscribe(this);

            if (workSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow)
                DebugOutputViewModel = new DebugOutputViewModel(workSurfaceKey);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            DataListSingleton.SetDataList(_dataListViewModel);

            var workflowDesignerViewModel = WorkSurfaceViewModel as WorkflowDesignerViewModel;
            if (workflowDesignerViewModel != null)
                workflowDesignerViewModel.AddMissingWithNoPopUpAndFindUnusedDataListItems();
        }

        public void DisplayDebugOutput(object message)
        {
            DebugOutputViewModel.Append(message);
        }

        public void GetServiceInputDataFromUser(IServiceDebugInfoModel input)
        {
            EventAggregator.Publish(new AddMissingAndFindUnusedDataListItemsMessage(_contextualResourceModel));
            var inputDataViewModel = new WorkflowInputDataViewModel(input, DebugWriter);
            _windowManager.ShowDialog(inputDataViewModel);
        }

        public void Debug(IContextualResourceModel resourceModel, bool isDebug)
        {
            if (resourceModel == null || resourceModel.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }

            Save(resourceModel,true);
            var mode = isDebug ? DebugMode.DebugInteractive : DebugMode.Run;

            IServiceDebugInfoModel debugInfoModel =
                ServiceDebugInfoModelFactory.CreateServiceDebugInfoModel(resourceModel, string.Empty, 0, mode);

            GetServiceInputDataFromUser(debugInfoModel);
        }

        public void Build()
        {
            Build(_contextualResourceModel, false);
        }

        private void Build(IContextualResourceModel model, bool deploy = true)
        {
            if (model == null || model.Environment == null || !model.Environment.IsConnected)
            {
                return;
            }

            // Clear output
            EventAggregator.Publish(new DebugWriterWriteMessage(string.Empty));

            var sb = new StringBuilder();
            dynamic buildRequest = new UnlimitedObject();

            if (!deploy)
            {
                buildRequest.Service = "CompileResourceService";
            }
            else
            {
                buildRequest.Service = "AddResourceService";
                buildRequest.Roles = String.Join(",", _securityContext.Roles);
            }

            sb.AppendLine(String.Format("<Build StartDate=\"{0}\">", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

            if (model.ResourceType == ResourceType.WorkflowService)
            {
                sb.AppendLine(String.Format("<Build WorkflowName=\"{0}\" />", model.ResourceName));
            }
            else if (model.ResourceType == ResourceType.Service)
            {
                sb.AppendLine(String.Format("<Build Service=\"{0}\" />", model.ResourceName));
            }
            else if (model.ResourceType == ResourceType.Source)
            {
                sb.AppendLine(String.Format("<Build Source=\"{0}\" />", model.ResourceName));
            }

            buildRequest.ResourceXml = model.ToServiceDefinition();

            Guid workspaceID = ((IStudioClientContext)model.Environment.DsfChannel).WorkspaceID;

            string result =
                model.Environment.DsfChannel.
                      ExecuteCommand(buildRequest.XmlString, workspaceID, GlobalConstants.NullDataListID) ??
                string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, buildRequest.Service);

            sb.AppendLine(result);
            sb.AppendLine(String.Format("</Build>"));
            DisplayDebugOutput(sb.ToString());
        }

        public void ViewInBrowser()
        {
            EventAggregator.Publish(new AddMissingAndFindUnusedDataListItemsMessage(_contextualResourceModel));

            Save(_contextualResourceModel,true);

            if (_contextualResourceModel == null || _contextualResourceModel.Environment == null ||
                _contextualResourceModel.Environment.Connection == null) return;

            Process.Start(StudioToWizardBridge.GetWorkflowUrl(_contextualResourceModel).AbsoluteUri);
        }

        public void ShowSaveDialog(IContextualResourceModel resourceModel)
        {
            RootWebSite.ShowNewWorkflowSaveDialog(resourceModel);
        }

        public void Save(bool isLocalSave = false)
        {
            Save(_contextualResourceModel, isLocalSave);
        }

        private void Save(IContextualResourceModel resource, bool isLocalSave)
        {
            if (resource == null)
            {
                return;
            }          


            if (resource.IsNewWorkflow && !isLocalSave)
            {
                ShowSaveDialog(resource);
                return;
            }

            var vm = WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            if (vm != null)
            {
                vm.BindToModel();
            }

            if (!isLocalSave)
            {
                Build(resource);
            }

            var resourceToUpdate = resource.Environment.ResourceRepository.FindSingle(
                c => c.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase));
            if (resourceToUpdate != null)
            {
                resourceToUpdate.Update(resource);
            }

            var result = _workspaceItemRepository.UpdateWorkspaceItem(resource);

            if(!isLocalSave)
            {
                DisplaySaveResult(result);
            }

            resource.Environment.ResourceRepository.Save(resource);
            EventAggregator.Publish(new UpdateDeployMessage());
        }

        private void DisplaySaveResult(string result)
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("<Save StartDate=\"{0}\">",
                                        DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            sb.AppendLine(result);
            sb.AppendLine(String.Format("</Save>"));

            DisplayDebugOutput(sb.ToString());
        }

        public void Handle(DebugResourceMessage message)
        {
            Debug(message.Resource, true);
        }

        public void Handle(ExecuteResourceMessage message)
        {
            Debug(message.Resource, false);
        }

        public void Handle(SaveResourceMessage message)
        {
            if (_contextualResourceModel == message.Resource)
                Save(message.Resource, message.IsLocalSave);
        }

    }
}
