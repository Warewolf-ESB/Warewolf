
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common;
using Dev2.ConnectionHelpers;
using Dev2.Models;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views.Workflow;
using Dev2.Threading;

namespace Dev2.Dialogs
{
    // PBI 10652 - 2013.11.04 - TWR - Refactored from WorkflowDesignerViewModel.ViewPreviewDrop to enable re-use!

    public class ResourcePickerDialog : IResourcePickerDialog
    {
        readonly enDsfActivityType _activityType;

        readonly INavigationViewModel _navigationViewModel;
        IEnvironmentModel _environmentModel;
        IStudioResourceRepository _studio;


        /// <summary>
        /// Creates a picker suitable for dropping from the toolbox.
        /// </summary>
        public ResourcePickerDialog(enDsfActivityType activityType)
            : this(activityType, EnvironmentRepository.Instance, EventPublishers.Aggregator, new AsyncWorker(), true, StudioResourceRepository.Instance, ConnectControlSingleton.Instance)
        {
        }

        /// <summary>
        /// Creates a picker suitable for picking from the given environment.
        /// </summary>
        public ResourcePickerDialog(enDsfActivityType activityType, IEnvironmentModel source)
            : this(activityType, EnvironmentRepository.Create(source), EventPublishers.Aggregator, new AsyncWorker(), false, StudioResourceRepository.Instance, ConnectControlSingleton.Instance)
        {
        }

        public ResourcePickerDialog(enDsfActivityType activityType, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, bool isFromDrop, IStudioResourceRepository studioResourceRepository, IConnectControlSingleton connectControlSingleton)
        {
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("connectControlSingleton", connectControlSingleton);
            _studio = studioResourceRepository;
            _navigationViewModel = new NavigationViewModel(eventPublisher, asyncWorker, null, environmentRepository, studioResourceRepository,connectControlSingleton, () => {}, isFromDrop, activityType);
            _activityType = activityType;
        }

        public IResourceModel SelectedResource { get; set; }

        public bool ShowDialog(IEnvironmentModel environmentModel = null)
        {
            DsfActivityDropViewModel dropViewModel;
            _environmentModel = environmentModel;
            return ShowDialog(out dropViewModel);
        }

        bool ShowDialog(out DsfActivityDropViewModel dropViewModel)
        {
            if(_environmentModel != null)
            {
                _navigationViewModel.FilterEnvironment = _environmentModel;
                _navigationViewModel.Filter(model => model.EnvironmentId == _environmentModel.ID);
            }
            else
            {

                _navigationViewModel.Filter(null,false,false);
                var explorerItemModels = _navigationViewModel.ExplorerItemModels;
                if(explorerItemModels != null)
                {
                    foreach(ExplorerItemModel explorerItemModel in explorerItemModels)
                    {
                        if(explorerItemModel != null)
                        {
                            explorerItemModel.IsResourcePickerExpanded = true;
                        }
                    }
                }
            }
            dropViewModel = new DsfActivityDropViewModel(_navigationViewModel, _activityType);
            var contextualResourceModel = SelectedResource as IContextualResourceModel;
            if(SelectedResource != null && contextualResourceModel != null)
            {
                dropViewModel.SelectedResourceModel = contextualResourceModel;
                _navigationViewModel.BringItemIntoView(contextualResourceModel);
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
