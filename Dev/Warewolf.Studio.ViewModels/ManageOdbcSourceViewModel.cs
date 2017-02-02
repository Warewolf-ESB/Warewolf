using System;
using System.Linq;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class ManageOdbcSourceViewModel : DatabaseSourceViewModelBase
    {
        public ManageOdbcSourceViewModel(IAsyncWorker asyncWorker)
            : base(asyncWorker, "ODBC")
        {
        }

        public ManageOdbcSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base(updateManager, requestServiceNameViewModel, aggregator, asyncWorker, "ODBC")
        {
            HeaderText = Resources.Languages.Core.OdbcSourceNewHeaderLabel;
            Header = Resources.Languages.Core.OdbcSourceNewHeaderLabel;
            InitializeViewModel();
        }

        public ManageOdbcSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker)
            : base(updateManager, aggregator, dbSource, asyncWorker, "ODBC")
        {
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            CanSelectServer = false;
            CanSelectUser = false;
            EmptyServerName = "Localhost";
            AuthenticationType = AuthenticationType.Windows;
        }

        #region Overrides of SourceBaseImpl<IDbSource>

        public override string Name
        {
            get
            {
                return ResourceName;
            }
            set
            {
                ResourceName = value;
            }
        }

        public override void FromModel(IDbSource service)
        {
            ResourceName = service.Name;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(service.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? service.ServerName;
            }
            AuthenticationType = service.AuthenticationType;
            Path = service.Path;
            TestConnection();
            DatabaseName = service.DbName;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion

        #region Overrides of DatabaseSourceViewModelBase

        protected override IDbSource ToNewDbSource()
        {
            return new DbSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = EmptyServerName,
                Password = Password,
                UserName = UserName,
                Type = enSourceType.ODBC,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = DbSource?.Id ?? Guid.NewGuid()
            };
        }

        protected override IDbSource ToDbSource()
        {
            return DbSource == null ? new DbSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = EmptyServerName,
                Password = Password,
                UserName = UserName,
                Type = enSourceType.ODBC,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = DbSource?.Id ?? SelectedGuid
            } : new DbSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = EmptyServerName,
                Password = Password,
                UserName = UserName,
                Type = enSourceType.ODBC,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = (Guid)DbSource?.Id
            };
        }

        protected override IDbSource ToSourceDefinition()
        {
            return new DbSourceDefinition
            {
                AuthenticationType = DbSource.AuthenticationType,
                DbName = DbSource.DbName,
                Id = DbSource.Id,
                Name = DbSource.Name,
                Password = DbSource.Password,
                Path = DbSource.Path,
                ServerName = string.IsNullOrWhiteSpace(EmptyServerName) ? "Localhost" : EmptyServerName,
                UserName = DbSource.UserName,
                Type = enSourceType.ODBC
            };
        }

        public override IDbSource ToModel()
        {
            if (Item == null)
            {
                Item = ToDbSource();
                return Item;
            }

            return new DbSourceDefinition()
            {
                AuthenticationType = AuthenticationType,
                ServerName = EmptyServerName,
                Password = Password,
                UserName = UserName,
                Type = enSourceType.ODBC,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = Item.Id
            };
        }

        #endregion
    }
}