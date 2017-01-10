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
    public class ManagePostgreSqlSourceViewModel : DatabaseSourceViewModelBase
    {
        private readonly IPostgreSource _postgreSource;
        public ManagePostgreSqlSourceViewModel(IAsyncWorker asyncWorker)
            : base(asyncWorker, "PostgreSQL")
        {
        }

        public ManagePostgreSqlSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base(updateManager, requestServiceNameViewModel, aggregator, asyncWorker, "PostgreSQL")
        {
            HeaderText = Resources.Languages.Core.PostgreSqlSourceNewHeaderLabel;
            Header = Resources.Languages.Core.PostgreSqlSourceNewHeaderLabel;
            CanSelectServer = true;
            CanSelectUser = true;
            CanSelectWindows = false;
            AuthenticationType = AuthenticationType.User;
        }

        public ManagePostgreSqlSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker)
            : base(updateManager, aggregator, dbSource, asyncWorker, "PostgreSQL")
        {
            VerifyArgument.IsNotNull("postgreSource", _postgreSource);
            HeaderText = Resources.Languages.Core.PostgreSqlSourceNewHeaderLabel;
            Header = Resources.Languages.Core.PostgreSqlSourceNewHeaderLabel;
            _postgreSource = dbSource as IPostgreSource;
            CanSelectServer = true;
            CanSelectUser = true;
            CanSelectWindows = false;
            AuthenticationType = AuthenticationType.User;
        }

        #region Overrides of SourceBaseImpl<IDbSource>

        public override string Name { get; set; }

        public override void FromModel(IDbSource service)
        {
            var postgreSource = (IPostgreSource)service;
            ResourceName = postgreSource.Name;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(postgreSource.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? postgreSource.ServerName;
            }
            AuthenticationType = service.AuthenticationType;
            UserName = postgreSource.UserName;
            Password = postgreSource.Password;
            Path = postgreSource.Path;
            TestConnection();
            DatabaseName = postgreSource.DbName;
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
            return new PostgreSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.PostgreSQL,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _postgreSource?.Id ?? Guid.NewGuid()
            };
        }

        protected override IDbSource ToDbSource()
        {
            return _postgreSource == null ? new PostgreSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.PostgreSQL,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _postgreSource?.Id ?? SelectedGuid
            } : new PostgreSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.PostgreSQL,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = (Guid)_postgreSource?.Id
            };
        }

        protected override IDbSource ToSourceDefinition()
        {
            return new PostgreSourceDefinition
            {
                AuthenticationType = _postgreSource.AuthenticationType,
                DbName = _postgreSource.DbName,
                Id = _postgreSource.Id,
                Name = _postgreSource.Name,
                Password = _postgreSource.Password,
                Path = _postgreSource.Path,
                ServerName = _postgreSource.ServerName,
                UserName = _postgreSource.UserName,
                Type = enSourceType.PostgreSQL
            };
        }

        #endregion
    }
}