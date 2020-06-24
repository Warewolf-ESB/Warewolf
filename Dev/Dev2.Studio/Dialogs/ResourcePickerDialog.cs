#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces.Threading;
using Dev2.ConnectionHelpers;
using Dev2.Services.Events;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views.Workflow;
using Dev2.Threading;
using Warewolf.Studio.ViewModels;

namespace Dev2.Dialogs
{
    // PBI 10652 - 2013.11.04 - TWR - Refactored from WorkflowDesignerViewModel.ViewPreviewDrop to enable re-use!

    public class ResourcePickerDialog : IResourcePickerDialog
    {
        readonly enDsfActivityType _activityType;

        public IExplorerViewModel SingleEnvironmentExplorerViewModel{get; private set;}
        IServer _server;
        IExplorerTreeItem _selectedResource;

        /// <summary>
        /// Creates a picker suitable for dropping from the toolbox.
        /// </summary>
        /// //todo:fix ctor for testing
        public ResourcePickerDialog(enDsfActivityType activityType)
            : this(activityType, null, EventPublishers.Aggregator, new AsyncWorker(), ConnectControlSingleton.Instance)
        {
        }

        /// <summary>
        /// Creates a picker suitable for picking from the given environment.
        /// </summary>
        public ResourcePickerDialog(enDsfActivityType activityType, IEnvironmentViewModel source)
            : this(activityType, source, EventPublishers.Aggregator, new AsyncWorker(), ConnectControlSingleton.Instance)
        {
        }

        public ResourcePickerDialog(enDsfActivityType activityType, IEnvironmentViewModel environmentViewModel, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IConnectControlSingleton connectControlSingleton)
        {
            VerifyArgument.IsNotNull("environmentRepository", environmentViewModel);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("connectControlSingleton", connectControlSingleton);

            SingleEnvironmentExplorerViewModel = new SingleEnvironmentExplorerViewModel(environmentViewModel, Guid.Empty, true);
            SingleEnvironmentExplorerViewModel.SelectedItemChanged += (sender, item) => { SelectedResource = item; };
            _activityType = activityType;
        }

        public static Task<IResourcePickerDialog> CreateAsync(enDsfActivityType activityType, IEnvironmentViewModel source)
        {
            var ret = new ResourcePickerDialog(activityType, source, EventPublishers.Aggregator, new AsyncWorker(), ConnectControlSingleton.Instance);
            return ret.InitializeAsync(source);
        }

        protected  async Task<IResourcePickerDialog> InitializeAsync(IEnvironmentViewModel environmentViewModel)
        {
            environmentViewModel.Connect();

            await environmentViewModel.LoadDialogAsync("");
            switch (_activityType)
            {
                case enDsfActivityType.Workflow:
                case enDsfActivityType.Service:
                    environmentViewModel.Filter(a => a.IsFolder || a.IsService);
                    break;
                case enDsfActivityType.Source:
                    environmentViewModel.Filter(a => a.IsFolder || a.IsSource);
                    break;
                case enDsfActivityType.All:
                    break;
                default:
                    break;
            }
            environmentViewModel.SelectAction = a => SelectedResource = a;
            return this;
        }

        public IExplorerTreeItem SelectedResource
        {
            get
            {
                return _selectedResource;
            }
            set
            {
                _selectedResource = value;
            }
        }

        public bool ShowDialog() => ShowDialog(null);
        public bool ShowDialog(IServer server)
        {
            _server = server;
            return ShowDialog(out DsfActivityDropViewModel dropViewModel);
        }

        public void SelectResource(Guid id)
        {
           SingleEnvironmentExplorerViewModel.SelectItem(id);
        }

        public bool ShowDialog(out DsfActivityDropViewModel dropViewModel)
        {
            dropViewModel = new DsfActivityDropViewModel(SingleEnvironmentExplorerViewModel, _activityType);
            try
            {
                var selected = SelectedResource;
                if (SelectedResource != null && selected != null)
                {
                    dropViewModel.SelectedResourceModel =
                        _server.ResourceRepository.FindSingle(c => c.ID == selected.ResourceId, true) as
                            IContextualResourceModel;
                }

                var dropWindow = CreateDialog(dropViewModel);
                dropWindow.ShowDialog();
                if (dropViewModel.DialogResult == ViewModelDialogResults.Okay)
                {
                    var model = dropViewModel;
                    SelectedResource = model.SelectedExplorerItemModel;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex.Message, GlobalConstants.WarewolfError);
            }
            SelectedResource = null;
            return false;
        }

        protected virtual IDialog CreateDialog(DsfActivityDropViewModel dataContext) => new DsfActivityDropWindow { DataContext = dataContext };

        public static enDsfActivityType DetermineDropActivityType(string typeName)
        {
            VerifyArgument.IsNotNull("typeName", typeName);

            if(typeName.Contains(GlobalConstants.ResourcePickerWorkflowString))
            {
                return enDsfActivityType.Workflow;
            }

            return enDsfActivityType.All;
        }
    }
}
