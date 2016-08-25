using System;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class DuplicateResourceViewModel : BindableBase, IDuplicateResourceViewModel
    {
        private readonly ICommunicationController _controller;
        private readonly ICreateDuplicateResourceView _createDuplicateResourceView;

        private string _newResourceName;
        public string NewResourceName
        {
            get
            {
                return _newResourceName;
            }
            // ReSharper disable once UnusedMember.Local
            set
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
            // ReSharper disable once UnusedMember.Local
            set
            {
                _fixReferences = value;
                OnPropertyChanged(() => FixReferences);
            }
        }
        public ICommand CancelCommand { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public ICommand CreateCommand { get; private set; }


        public DuplicateResourceViewModel(ICreateDuplicateResourceView createDuplicateResourceView, Guid resourceId)
            : this(new CommunicationController { ServiceName = "DuplicateResourceService" })
        {
            _createDuplicateResourceView = createDuplicateResourceView;
            _resourceId = resourceId;

        }

        public DuplicateResourceViewModel(ICommunicationController controller)
        {
            _controller = controller;

            CancelCommand = new DelegateCommand(CancelAndClose);
            CreateCommand = new DelegateCommand(CreateDuplicates);
        }

        private void CreateDuplicates(object obj)
        {

            _controller.AddPayloadArgument("ResourceID", _resourceId.ToString());
            _controller.AddPayloadArgument("NewResourceName", NewResourceName);
            _controller.AddPayloadArgument("FixRefs", FixReferences.ToString());
            if(obj == null)
                obj = EnvironmentRepository.Instance.ActiveEnvironment;
            var environmentConnection = EnvironmentConnection(obj as IEnvironmentModel);
            // ReSharper disable once UnusedVariable
            var executeCommand = _controller.ExecuteCommand<ExecuteMessage>(environmentConnection, GlobalConstants.ServerWorkspaceID);
            CancelAndClose(null);
            EnvironmentRepository.Instance.Load();
        }

        private static IEnvironmentConnection EnvironmentConnection(IEnvironmentModel model) => model.Connection;

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
