
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
using System.ComponentModel;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;
using Dev2.Models;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Workflow
{
    public class DsfActivityDropViewModel : SimpleBaseViewModel
    {
        #region Fields

        private RelayCommand _executeCommmand;
        private DelegateCommand _cancelComand;

        private IContextualResourceModel _selectedResource;

        #endregion Fields

        #region Ctor

        public DsfActivityDropViewModel(INavigationViewModel navigationViewModel, enDsfActivityType dsfActivityType)
        {
            NavigationViewModel = navigationViewModel;
            ActivityType = dsfActivityType;
            NavigationViewModel.PropertyChanged+=CheckIfSelectedItemChanged;
            Init();
            EventPublishers.Aggregator.Subscribe(this);
        }

        void CheckIfSelectedItemChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "SelectedItem")
            {
                OkCommand.RaiseCanExecuteChanged();
            }
        }

        void Init()
        {
            switch(ActivityType)
            {
                case enDsfActivityType.Workflow:
                    ImageSource = "Workflow-32";
                    Title = "Select A Workflow";
                    break;
                case enDsfActivityType.Service:
                    ImageSource = "ToolService-32";
                    Title = "Select A Service";
                    break;
                default:
                    ImageSource = "ExplorerWarewolfConnection-32";
                    Title = "Select A Resource";
                    break;
            }
        }

        #endregion Ctor

        #region Properties

        public string Title { get; private set; }

        public string ImageSource { get; private set; }

        public enDsfActivityType ActivityType { get; private set; }

        public INavigationViewModel NavigationViewModel { get; private set; }

        public string SelectedResourceName { get; set; }

        public IContextualResourceModel SelectedResourceModel
        {
            get
            {
                return _selectedResource;
            }
            set
            {
                _selectedResource = value;
                NotifyOfPropertyChange("SelectedResourceModel");
                CommandManager.InvalidateRequerySuggested();
            }
        }

        #endregion Properties

        #region Commands

        public RelayCommand OkCommand
        {
            get
            {
                return _executeCommmand ?? (_executeCommmand = new RelayCommand(param => Okay(), param => CanOkay));
            }
        }
        public bool CanOkay { get { return CanSelect(); } }


        private bool CanSelect()
        {
            bool isMatched = false;

            var explorerItemModel = NavigationViewModel.SelectedItem;

            if(explorerItemModel != null)
            {
                switch(ActivityType)
                {
                    case enDsfActivityType.Workflow:
                        isMatched = explorerItemModel.ResourceType == ResourceType.WorkflowService;
                        break;
                    case enDsfActivityType.Service:
                        isMatched = explorerItemModel.ResourceType == ResourceType.DbService ||
                                    explorerItemModel.ResourceType == ResourceType.PluginService ||
                                    explorerItemModel.ResourceType == ResourceType.WebService;
                        break;
                    case enDsfActivityType.All:
                        isMatched = explorerItemModel.ResourceType != ResourceType.Folder &&
                                    explorerItemModel.ResourceType != ResourceType.Server &&
                                    explorerItemModel.ResourceType != ResourceType.ServerSource;
                        break;
                    default:
                        isMatched = explorerItemModel.ResourceType != ResourceType.Folder &&
                                    explorerItemModel.ResourceType != ResourceType.WorkflowService &&
                                    explorerItemModel.ResourceType != ResourceType.DbService &&
                                    explorerItemModel.ResourceType != ResourceType.PluginService &&
                                    explorerItemModel.ResourceType != ResourceType.WebService;
                        break;
                }
            }

            return explorerItemModel != null && isMatched;
        }

        public ICommand CancelCommand
        {
            get
            {
                return _cancelComand ?? (_cancelComand = new DelegateCommand(param => Cancel()));
            }
        }

        #endregion Cammands

        #region Methods

        public Func<IEnvironmentRepository> GetEnvironmentRepository = () => EnvironmentRepository.Instance;

        /// <summary>
        /// Used for saving the data input by the user to the file system and pushing the data back at the workflow
        /// </summary>
        public void Okay()
        {
            var selectedItem = NavigationViewModel.SelectedItem;
            if(selectedItem == null)
            {
                return;
            }

            var environment = GetEnvironmentRepository().FindSingle(ev => ev.ID == selectedItem.EnvironmentId);

            if(environment == null)
            {
                return;
            }

            SelectedResourceModel = environment.ResourceRepository.FindSingleWithPayLoad(r => r.ID == selectedItem.ResourceId) as IContextualResourceModel;
            SelectedExplorerItemModel = selectedItem;
            if(SelectedResourceModel != null)
            {
                RequestClose(ViewModelDialogResults.Okay);
            }
        }

        public IExplorerItemModel SelectedExplorerItemModel { get; set; }

        /// <summary>
        /// Used for canceling the drop of t    he design surface
        /// </summary>
        public void Cancel()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        #endregion Methods

        #region Implementation of IDisposable

        protected override void OnDispose()
        {
            if(NavigationViewModel != null)
            {
                NavigationViewModel.Dispose();
                NavigationViewModel = null;
            }
            EventPublishers.Aggregator.Unsubscribe(this);

            base.OnDispose();
        }
        #endregion
    }
}
