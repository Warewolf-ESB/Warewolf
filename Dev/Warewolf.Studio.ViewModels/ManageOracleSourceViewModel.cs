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
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class ManageOracleSourceViewModel : DatabaseSourceViewModelBase
    {
        public ManageOracleSourceViewModel(IAsyncWorker asyncWorker)
            : base(asyncWorker, "Oracle")
        {
        }

        public ManageOracleSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base(updateManager, requestServiceNameViewModel, aggregator, asyncWorker, "Oracle")
        {
            HeaderText = Resources.Languages.Core.OracleSourceNewHeaderLabel;
            Header = Resources.Languages.Core.OracleSourceNewHeaderLabel;
            InitializeViewModel();
        }

        public ManageOracleSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker)
            : base(updateManager, aggregator, dbSource, asyncWorker, "Oracle")
        {
            VerifyArgument.IsNotNull("oracleSource", _oracleSource);
            _oracleSource = dbSource as IOracleSource;
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            CanSelectWindows = false;
            AuthenticationType = AuthenticationType.User;
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
            return new OracleSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = DbSource?.Id ?? Guid.NewGuid()
            };
        }

        #region Overrides of DatabaseSourceViewModelBase

        public override IDbSource ToModel()
        {
            if (Item == null)
            {
                Item = ToDbSource();
                return Item;
            }

            return new OracleSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = Item.Id
            };
        }

        #endregion

        protected override IDbSource ToDbSource()
        {
            return DbSource == null ? new OracleSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = DbSource?.Id ?? SelectedGuid
            } : new OracleSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = (Guid)DbSource?.Id
            };
        }

        protected override IDbSource ToSourceDefinition()
        {
            return new OracleSourceDefinition
            {
                AuthenticationType = DbSource.AuthenticationType,
                DbName = DbSource.DbName,
                Id = DbSource.Id,
                Name = DbSource.Name,
                Password = ((IOracleSource)DbSource).Password,
                Path = DbSource.Path,
                ServerName = DbSource.ServerName,
                UserName = ((IOracleSource)DbSource).UserName,
                Type = enSourceType.Oracle
            };
        }

        #endregion
    }
}