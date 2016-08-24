using System;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class DuplicateResourceViewModel : BindableBase, IDuplicateResourceViewModel
    {
        private ICreateDuplicateResourceView _createDuplicateResourceView;

        private string _newResourceName;
        public string NewResourceName
        {
            get
            {
                return _newResourceName;
            }
            private set
            {
                _newResourceName = value;
                OnPropertyChanged(() => NewResourceName);
            }
        }

        private bool _fixReferences;
        private Guid _resourceId;
        public bool FixReferences
        {
            get
            {
                return _fixReferences;
            }
            private set
            {
                _fixReferences = value;
                OnPropertyChanged(() => FixReferences);
            }
        }
        public ICommand CancelCommand { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public ICommand CreateCommand { get; private set; }


        public DuplicateResourceViewModel(ICreateDuplicateResourceView createDuplicateResourceView, Guid resourceId)
        {
            _createDuplicateResourceView = createDuplicateResourceView;
            _resourceId = resourceId;
            CancelCommand = new DelegateCommand(CancelAndClose);
            CreateCommand = new DelegateCommand(CreateDuplicates);
        }


        private void CreateDuplicates(object obj)
        {
            var comsController = new CommunicationController { ServiceName = "DuplicateResourceService" };

            comsController.AddPayloadArgument("ResourceID", _resourceId.ToString());
            comsController.AddPayloadArgument("NewResourceName", NewResourceName);
            comsController.AddPayloadArgument("FixRefs", FixReferences.ToString());
            var environmentConnection = EnvironmentRepository.Instance.ActiveEnvironment.Connection;
            // ReSharper disable once UnusedVariable
            var executeCommand = comsController.ExecuteCommand<ExecuteMessage>(environmentConnection, GlobalConstants.ServerWorkspaceID);
            CancelAndClose(null);
        }

        private void CancelAndClose(object obj)
        {
            _createDuplicateResourceView.CloseView();
        }

        public void ShowDialog()
        {
            _createDuplicateResourceView.DataContext = this;
            _createDuplicateResourceView.ShowView();
        }
    }
}
