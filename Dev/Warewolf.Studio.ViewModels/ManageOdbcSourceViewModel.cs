using System;
using System.Linq;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Database;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class ManageOdbcSourceViewModel : DatabaseSourceViewModelBase
    {
        private readonly IOdbcSource _odbcSource;
        public ManageOdbcSourceViewModel(IAsyncWorker asyncWorker)
            : base(asyncWorker, "ODBC")
        {
        }

        public ManageOdbcSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base(updateManager, requestServiceNameViewModel, aggregator, asyncWorker, "ODBC")
        {
        }

        public ManageOdbcSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker)
            : base(updateManager, aggregator, dbSource, asyncWorker, "ODBC")
        {
            VerifyArgument.IsNotNull("odbcSource", _odbcSource);
            HeaderText = Resources.Languages.Core.OracleSourceNewHeaderLabel;
            Header = Resources.Languages.Core.OracleSourceNewHeaderLabel;
            _odbcSource = dbSource as IOdbcSource;
        }

        #region Overrides of SourceBaseImpl<IDbSource>

        public override string Name { get; set; }

        public override void FromModel(IDbSource service)
        {
            var odbcSource = (IOdbcSource)service;
            ResourceName = odbcSource.Name;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(odbcSource.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? odbcSource.ServerName;
            }
            AuthenticationType = service.AuthenticationType;
            Path = odbcSource.Path;
            TestConnection();
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
            return new OdbcSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.ODBC,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _odbcSource?.Id ?? Guid.NewGuid()
            };
        }

        protected override IDbSource ToDbSource()
        {
            return _odbcSource == null ? new OdbcSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Type = enSourceType.ODBC,
                Path = Path,
                Name = ResourceName,
                Id = _odbcSource?.Id ?? SelectedGuid
            } : new OdbcSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Type = enSourceType.ODBC,
                Path = Path,
                Name = ResourceName,
                Id = (Guid)_odbcSource?.Id
            };
        }

        protected override IDbSource ToSourceDefinition()
        {
            return new DbSourceDefinition
            {
                AuthenticationType = _odbcSource.AuthenticationType,
                Id = _odbcSource.Id,
                Name = _odbcSource.Name,
                Path = _odbcSource.Path,
                ServerName = _odbcSource.ServerName,
                Type = enSourceType.ODBC
            };
        }

        #endregion
    }
}