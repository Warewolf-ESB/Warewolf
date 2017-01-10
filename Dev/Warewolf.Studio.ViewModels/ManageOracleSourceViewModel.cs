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
    public class ManageOracleSourceViewModel : DatabaseSourceViewModelBase
    {
        private readonly IOracleSource _oracleSource;
        public ManageOracleSourceViewModel(IAsyncWorker asyncWorker)
            : base(asyncWorker, "Oracle")
        {
        }

        public ManageOracleSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base(updateManager, requestServiceNameViewModel, aggregator, asyncWorker, "Oracle")
        {
        }

        public ManageOracleSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker)
            : base(updateManager, aggregator, dbSource, asyncWorker, "Oracle")
        {
            HeaderText = Resources.Languages.Core.OracleSourceNewHeaderLabel;
            Header = Resources.Languages.Core.OracleSourceNewHeaderLabel;
            VerifyArgument.IsNotNull("oracleSource", _oracleSource);
            _oracleSource = dbSource as IOracleSource;
        }

        #region Overrides of SourceBaseImpl<IDbSource>

        public override string Name { get; set; }

        public override void FromModel(IDbSource service)
        {
            var oracleSource = (IOracleSource)service;
            ResourceName = oracleSource.Name;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(oracleSource.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? oracleSource.ServerName;
            }
            AuthenticationType = service.AuthenticationType;
            UserName = oracleSource.UserName;
            Password = oracleSource.Password;
            Path = oracleSource.Path;
            TestConnection();
            DatabaseName = oracleSource.DbName;
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
            return new OracleSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _oracleSource?.Id ?? Guid.NewGuid()
            };
        }

        protected override IDbSource ToDbSource()
        {
            return _oracleSource == null ? new OracleSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _oracleSource?.Id ?? SelectedGuid
            } : new OracleSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = (Guid)_oracleSource?.Id
            };
        }

        protected override IDbSource ToSourceDefinition()
        {
            return new DbSourceDefinition
            {
                AuthenticationType = _oracleSource.AuthenticationType,
                DbName = _oracleSource.DbName,
                Id = _oracleSource.Id,
                Name = _oracleSource.Name,
                Password = _oracleSource.Password,
                Path = _oracleSource.Path,
                ServerName = _oracleSource.ServerName,
                UserName = _oracleSource.UserName,
                Type = enSourceType.Oracle
            };
        }

        #endregion
    }
}