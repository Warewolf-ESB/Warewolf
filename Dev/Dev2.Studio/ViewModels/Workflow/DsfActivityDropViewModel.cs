/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.WorkSurface;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Workflow
{
    public class DsfActivityDropViewModel : SimpleBaseViewModel
    {

        public IExplorerViewModel SingleEnvironmentExplorerViewModel { get; private set; }
        #region Fields

        private RelayCommand _executeCommmand;
        private DelegateCommand _cancelComand;

        private IContextualResourceModel _selectedResource;

        #endregion Fields

        #region Ctor

        public DsfActivityDropViewModel(IExplorerViewModel explorerViewModel, enDsfActivityType dsfActivityType)
        {
            SingleEnvironmentExplorerViewModel = explorerViewModel;
            SingleEnvironmentExplorerViewModel.SelectedItemChanged += SingleEnvironmentExplorerViewModel_SelectedItemChanged;
            ActivityType = dsfActivityType;

            Init();
            EventPublishers.Aggregator.Subscribe(this);
        }

        void SingleEnvironmentExplorerViewModel_SelectedItemChanged(object sender, IExplorerTreeItem e)
        {
            SelectedExplorerItemModel = e;
            OkCommand.RaiseCanExecuteChanged();
        }

        void Init()
        {
            switch(ActivityType)
            {
                case enDsfActivityType.Workflow:
                    ImageSource = "Workflow-32";
                    Title = "Select A Service";
                    break;
                case enDsfActivityType.Service:
                    ImageSource = "ToolService-32";
                    Title = "Select A Data Connector";
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
                OkCommand.RaiseCanExecuteChanged();
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
        public bool CanOkay => CanSelect();


        private bool CanSelect()
        {
            bool isMatched = false;

            var explorerItemModel = SingleEnvironmentExplorerViewModel.SelectedItem;

            if(explorerItemModel != null)
            {
                isMatched = explorerItemModel.IsService;
            }

            var explorerViewModelBase = ((Warewolf.Studio.ViewModels.ExplorerViewModelBase)SingleEnvironmentExplorerViewModel);
            var conductorBaseWithActiveItem = (Caliburn.Micro.ConductorBaseWithActiveItem<WorkSurfaceContextViewModel>)
                ((Warewolf.Studio.ViewModels.ExplorerItemViewModel)explorerViewModelBase?.SelectedItem)?.ShellViewModel;
            var workSurfaceContextViewModel = conductorBaseWithActiveItem?.ActiveItem;
            var contextualResourceModel = workSurfaceContextViewModel?.ContextualResourceModel;
            var guid = contextualResourceModel?.ID;
            if(explorerItemModel != null && explorerItemModel.ResourceId == guid)
            {
                return false;
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
            var selectedItem = SingleEnvironmentExplorerViewModel.SelectedItem;



            if(selectedItem == null)
            {
                return;
            }

            var environment = GetEnvironmentRepository().FindSingle(ev => ev.ID == selectedItem.Server.EnvironmentID);

            if(environment == null)
            {
                return;
            }

           // SelectedResourceModel = environment.ResourceRepository.FindSingleWithPayLoad(r => r.ID == selectedItem.ResourceId) as IContextualResourceModel;
            SelectedExplorerItemModel = selectedItem;
            if (SelectedExplorerItemModel != null)
            {
                RequestClose(ViewModelDialogResults.Okay);
            }
        }

        internal IExplorerTreeItem SelectedExplorerItemModel { get; private set; }

        /// <summary>
        /// Used for canceling the drop of t    he design surface
        /// </summary>
        void Cancel()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        #endregion Methods

        #region Implementation of IDisposable

        protected override void OnDispose()
        {
         
            EventPublishers.Aggregator.Unsubscribe(this);

            base.OnDispose();
        }
        #endregion
    }
}
