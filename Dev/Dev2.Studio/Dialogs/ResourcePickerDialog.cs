using System;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views.Workflow;
using Dev2.Threading;

namespace Dev2.Dialogs
{
    // PBI 10652 - 2013.11.04 - TWR - Refactored from WorkflowDesignerViewModel.ViewPreviewDrop to enable re-use!

    public class ResourcePickerDialog : IResourcePickerDialog
    {
        readonly enDsfActivityType _activityType;
        readonly ExplorerViewModel _explorerViewModel;

        /// <summary>
        /// Creates a picker suitable for dropping from the toolbox.
        /// </summary>
        public ResourcePickerDialog(enDsfActivityType activityType)
            : this(activityType, EnvironmentRepository.Instance, EventPublishers.Aggregator, new AsyncWorker(), true)
        {
        }

        /// <summary>
        /// Creates a picker suitable for picking from the given environment.
        /// </summary>
        public ResourcePickerDialog(enDsfActivityType activityType, IEnvironmentModel source)
            : this(activityType, EnvironmentRepository.Create(source), EventPublishers.Aggregator, new AsyncWorker(), false)
        {
        }

        public ResourcePickerDialog(enDsfActivityType activityType, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, bool isFromDrop)
        {
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);

            _activityType = activityType;
            _explorerViewModel = new ExplorerViewModel(eventPublisher, asyncWorker, environmentRepository, isFromDrop, activityType);

            asyncWorker.Start(() => { }, () => _explorerViewModel.LoadEnvironments());
        }

        public IResourceModel SelectedResource { get; set; }

        public bool ShowDialog()
        {
            DsfActivityDropViewModel dropViewModel;
            return ShowDialog(out dropViewModel);
        }

        bool ShowDialog(out DsfActivityDropViewModel dropViewModel)
        {
            dropViewModel = new DsfActivityDropViewModel(_explorerViewModel, _activityType);
            var contextualResourceModel = SelectedResource as IContextualResourceModel;
            if(SelectedResource != null && contextualResourceModel != null)
            {
                dropViewModel.SelectedResourceModel = contextualResourceModel;
                if(_explorerViewModel.NavigationViewModel != null)
                {
                    _explorerViewModel.NavigationViewModel.BringItemIntoView(contextualResourceModel);
                }
            }
            var dropWindow = CreateDialog(dropViewModel);
            dropWindow.ShowDialog();
            if(dropViewModel.DialogResult == ViewModelDialogResults.Okay)
            {
                SelectedResource = dropViewModel.SelectedResourceModel;
                return true;
            }
            SelectedResource = null;
            return false;
        }

        protected virtual IDialog CreateDialog(DsfActivityDropViewModel dataContext)
        {
            return new DsfActivityDropWindow { DataContext = dataContext };
        }

        public static enDsfActivityType DetermineDropActivityType(string typeName)
        {
            VerifyArgument.IsNotNull("typeName", typeName);

            if(typeName.Contains(GlobalConstants.ResourcePickerWorkflowString))
            {
                return enDsfActivityType.Workflow;
            }

            if(typeName.Contains(GlobalConstants.ResourcePickerServiceString))
            {
                return enDsfActivityType.Service;
            }

            return enDsfActivityType.All;
        }

        public static bool ShowDropDialog<T>(ref T picker, string typeName, out DsfActivityDropViewModel dropViewModel)
            where T : ResourcePickerDialog
        {
            var activityType = DetermineDropActivityType(typeName);
            if(activityType != enDsfActivityType.All)
            {
                if(picker == null)
                {
                    picker = (T)Activator.CreateInstance(typeof(T), activityType);
                }
                return picker.ShowDialog(out dropViewModel);
            }
            dropViewModel = null;
            return false;
        }
    }
}