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
    public class ManageSqlServerSourceViewModel : DatabaseSourceViewModelBase
    {
        private readonly ISqlServerSource _sqlServerSource;
        public ManageSqlServerSourceViewModel(IAsyncWorker asyncWorker)
            : base(asyncWorker, "SqlDatabase")
        {
        }

        public ManageSqlServerSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base(updateManager, requestServiceNameViewModel, aggregator, asyncWorker, "SqlDatabase")
        {
        }

        public ManageSqlServerSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker)
            : base(updateManager, aggregator, dbSource, asyncWorker, "SqlDatabase")
        {
            VerifyArgument.IsNotNull("sqlServerSource", _sqlServerSource);
            HeaderText = Resources.Languages.Core.SqlServerSourceServerNewHeaderLabel;
            Header = Resources.Languages.Core.SqlServerSourceServerNewHeaderLabel;
            _sqlServerSource = dbSource as ISqlServerSource;
        }

        #region Overrides of SourceBaseImpl<IDbSource>

        public override string Name { get; set; }

        public override void FromModel(IDbSource service)
        {
            var sqlServerSource = (ISqlServerSource)service;
            ResourceName = sqlServerSource.Name;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(sqlServerSource.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? sqlServerSource.ServerName;
            }
            AuthenticationType = service.AuthenticationType;
            UserName = sqlServerSource.UserName;
            Password = sqlServerSource.Password;
            Path = sqlServerSource.Path;
            TestConnection();
            DatabaseName = sqlServerSource.DbName;
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
            return new SqlServerSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.SqlDatabase,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _sqlServerSource?.Id ?? Guid.NewGuid()
            };
        }

        protected override IDbSource ToDbSource()
        {
            return _sqlServerSource == null ? new SqlServerSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.SqlDatabase,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _sqlServerSource?.Id ?? SelectedGuid
            } : new SqlServerSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.SqlDatabase,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = (Guid)_sqlServerSource?.Id
            };
        }

        protected override IDbSource ToSourceDefinition()
        {
            return new DbSourceDefinition
            {
                AuthenticationType = _sqlServerSource.AuthenticationType,
                DbName = _sqlServerSource.DbName,
                Id = _sqlServerSource.Id,
                Name = _sqlServerSource.Name,
                Password = _sqlServerSource.Password,
                Path = _sqlServerSource.Path,
                ServerName = _sqlServerSource.ServerName,
                UserName = _sqlServerSource.UserName,
                Type = enSourceType.SqlDatabase
            };
        }

        #endregion
    }
}