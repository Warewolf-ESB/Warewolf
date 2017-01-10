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
    public class ManageMySqlSourceViewModel : DatabaseSourceViewModelBase
    {
        private readonly IMySqlSource _mySqlSource;
        public ManageMySqlSourceViewModel(IAsyncWorker asyncWorker)
            : base(asyncWorker, "MySqlDatabase")
        {
        }

        public ManageMySqlSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base(updateManager, requestServiceNameViewModel, aggregator, asyncWorker, "MySqlDatabase")
        {
        }

        public ManageMySqlSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker)
            : base(updateManager, aggregator, dbSource, asyncWorker, "MySqlDatabase")
        {
            VerifyArgument.IsNotNull("mySqlSource", _mySqlSource);
            HeaderText = Resources.Languages.Core.MySqlSourceNewHeaderLabel;
            Header = Resources.Languages.Core.MySqlSourceNewHeaderLabel;
            _mySqlSource = dbSource as IMySqlSource;
        }

        #region Overrides of SourceBaseImpl<IDbSource>

        public override string Name { get; set; }

        public override void FromModel(IDbSource service)
        {
            var mySqlSource = (IMySqlSource)service;
            ResourceName = mySqlSource.Name;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(mySqlSource.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? mySqlSource.ServerName;
            }
            AuthenticationType = service.AuthenticationType;
            UserName = mySqlSource.UserName;
            Password = mySqlSource.Password;
            Path = mySqlSource.Path;
            TestConnection();
            DatabaseName = mySqlSource.DbName;
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
            return new MySqlSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.MySqlDatabase,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _mySqlSource?.Id ?? Guid.NewGuid()
            };
        }

        protected override IDbSource ToDbSource()
        {
            return _mySqlSource == null ? new MySqlSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.MySqlDatabase,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _mySqlSource?.Id ?? SelectedGuid
            } : new MySqlSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.MySqlDatabase,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = (Guid)_mySqlSource?.Id
            };
        }

        protected override IDbSource ToSourceDefinition()
        {
            return new DbSourceDefinition
            {
                AuthenticationType = _mySqlSource.AuthenticationType,
                DbName = _mySqlSource.DbName,
                Id = _mySqlSource.Id,
                Name = _mySqlSource.Name,
                Password = _mySqlSource.Password,
                Path = _mySqlSource.Path,
                ServerName = _mySqlSource.ServerName,
                UserName = _mySqlSource.UserName,
                Type = enSourceType.MySqlDatabase
            };
        }

        #endregion
    }
}