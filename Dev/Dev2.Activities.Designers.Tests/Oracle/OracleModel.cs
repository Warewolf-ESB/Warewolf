using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Core;

namespace Dev2.Activities.Designers.Tests.Oracle
{
    public class OracleModel : IDbServiceModel
    {
#pragma warning disable 649
        private IStudioUpdateManager _updateRepository;
#pragma warning restore 649
#pragma warning disable 169
        private IQueryManager _queryProxy;
#pragma warning restore 169

        public ObservableCollection<IDbSource> _sources = new ObservableCollection<IDbSource>
        {
            new DbSourceDefinition()
            {
                ServerName = "localServer",
                Type = enSourceType.MySqlDatabase,
                UserName = "johnny",
                Password = "bravo",
                AuthenticationType = AuthenticationType.Public,
                DbName = "",
                Name = "j_bravo",
                Path = "",
                Id = Guid.NewGuid()
            }
        };

        public ObservableCollection<IDbAction> _actions = new ObservableCollection<IDbAction>
        {
            new DbAction()
            {
                Name = "mob",
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") }
            }
        };

        public ObservableCollection<IDbAction> _refreshActions = new ObservableCollection<IDbAction>
        {
            new DbAction()
            {
                Name = "mob",
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") }
            },
            new DbAction()
            {
                Name = "arefreshOne",
                Inputs = new List<IServiceInput>() { new ServiceInput("[[b]]", "bsb") }
            }
        };

        public bool HasRecError { get; set; }

        #region Implementation of IDbServiceModel

        public ObservableCollection<IDbSource> RetrieveSources()
        {
            return Sources;
        }

        public ObservableCollection<IDbSource> Sources => _sources;

        public ICollection<IDbAction> GetActions(IDbSource source)
        {
            return Actions;
        }

        public ICollection<IDbAction> RefreshActions(IDbSource source)
        {
            return RefreshActionsList;
        }

        public ICollection<IDbAction> Actions => _actions;
        public ICollection<IDbAction> RefreshActionsList => _refreshActions;

        public void CreateNewSource(enSourceType type)
        {
        }
        public void EditSource(IDbSource selectedSource, enSourceType type)
        {
        }

        public DataTable TestService(IDatabaseService inputValues)
        {
            if (ThrowsTestError)
            {
                throw new Exception("bob");
            }

            if (HasRecError)
            {
                return null;
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("a");
            dt.Columns.Add("b");
            dt.Columns.Add("c");
            dt.TableName = "bob";
            return dt;

        }

        public IStudioUpdateManager UpdateRepository => _updateRepository;
        public bool ThrowsTestError { get; set; }

        #endregion
    }
}