#pragma warning disable
using System;
using System.Linq;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
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

        void InitializeViewModel()
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

        public override void FromModel(IDbSource source)
        {
            ResourceName = source.Name;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(source.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? source.ServerName;
            }
            AuthenticationType = source.AuthenticationType;
            Path = source.Path;
            TestConnection();
            DatabaseName = source.DbName;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion

        #region Overrides of DatabaseSourceViewModelBase

        protected override IDbSource ToNewDbSource() => new DbSourceDefinition
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

        protected override IDbSource ToDbSource() => DbSource == null ? new DbSourceDefinition
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

        protected override IDbSource ToSourceDefinition() => new DbSourceDefinition
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