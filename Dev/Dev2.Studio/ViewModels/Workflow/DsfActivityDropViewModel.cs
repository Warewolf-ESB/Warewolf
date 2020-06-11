#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.Interfaces;
using Warewolf.Studio.ViewModels;

namespace Dev2.Studio.ViewModels.Workflow
{
    public class DsfActivityDropViewModel : SimpleBaseViewModel
    {
        public IExplorerViewModel SingleEnvironmentExplorerViewModel { get; private set; }

        RelayCommand _executeCommmand;
        DelegateCommand _cancelComand;

        private readonly IServerRepository _serverRepository;

        IContextualResourceModel _selectedResource;

        public DsfActivityDropViewModel(IExplorerViewModel explorerViewModel, enDsfActivityType dsfActivityType)
            : this(explorerViewModel, dsfActivityType, ServerRepository.Instance)
        {
        }
        public DsfActivityDropViewModel(IExplorerViewModel explorerViewModel, enDsfActivityType dsfActivityType, IServerRepository serverRepository)
        {
            SingleEnvironmentExplorerViewModel = explorerViewModel;
            SingleEnvironmentExplorerViewModel.SelectedItemChanged += SingleEnvironmentExplorerViewModel_SelectedItemChanged;
            ActivityType = dsfActivityType;

            _serverRepository = serverRepository;

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
            if (ActivityType == enDsfActivityType.Workflow)
            {
                ImageSource = "Workflow-32";
                Title = "Select A Service";
            }
        }

        public string Title { get; private set; }

        public string ImageSource { get; private set; }

        public enDsfActivityType ActivityType { get; private set; }

        public IContextualResourceModel SelectedResourceModel
        {
            get => _selectedResource;
            set
            {
                _selectedResource = value;
                NotifyOfPropertyChange("SelectedResourceModel");
                OkCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand OkCommand => _executeCommmand ?? (_executeCommmand = new RelayCommand(param => Okay(), param => CanOkay));

        public bool CanOkay => CanSelect();

        bool CanSelect()
        {
            var isMatched = false;

            var explorerItemModel = SingleEnvironmentExplorerViewModel.SelectedItem;

            if (explorerItemModel is EnvironmentViewModel environmentViewModel)
            {
                isMatched |= environmentViewModel.IsSelected;
            }
            else if (explorerItemModel != null)
            {
                isMatched = explorerItemModel.IsService;
                isMatched |= explorerItemModel.IsFolder;
            }

            return explorerItemModel != null && isMatched;
        }

        public ICommand CancelCommand => _cancelComand ?? (_cancelComand = new DelegateCommand(param => Cancel()));


        public void Okay()
        {
            var selectedItem = SingleEnvironmentExplorerViewModel.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var environment = _serverRepository.FindSingle(ev => ev.EnvironmentID == selectedItem.Server.EnvironmentID);
            if (environment == null)
            {
                return;
            }
            SelectedExplorerItemModel = selectedItem;
            if (SelectedExplorerItemModel != null)
            {
                RequestClose(ViewModelDialogResults.Okay);
            }
        }

        internal IExplorerTreeItem SelectedExplorerItemModel { get; private set; }

        void Cancel()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        protected override void OnDispose()
        {
            EventPublishers.Aggregator.Unsubscribe(this);
            base.OnDispose();
        }
    }
}
