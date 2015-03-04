using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Security.RightsManagement;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Warewolf.Studio.ViewModels
{
    public class DbServiceModel : IDbServiceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;
        readonly string _serverName;

        #region Implementation of IDbServiceModel

        public DbServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
            _serverName = serverName;
        }

        public ICollection<IDbSource> RetrieveSources()
        {
            return new ObservableCollection<IDbSource> { new DbSourceDefinition { Name = "bob" }, new DbSourceDefinition() { Name = "dora" }, new DbSourceDefinition() { Name = "foo large" } };

        }

        public ICollection<IDbAction> GetActions()
        {
            return new ObservableCollection<IDbAction> {new DbAction {Name = "sp_bob",Inputs =  new List<IDbInput>(){new DbInput("Bob","Dave"),new DbInput("moo","cow")}}};
        }

        public void CreateNewSource()
        {
        }

        public void EditSource(IDbSource selectedSource)
        {
        }

        public  DataTable TestService(IList<IDbInput> inputValues)
        {
           var testResults = new DataTable("Results");
            testResults.Columns.Add(new DataColumn("Record Name"));
            testResults.Columns.Add(new DataColumn("Windows Group"));
            testResults.Columns.Add(new DataColumn("Response"));
            testResults.Columns.Add(new DataColumn("Bob"));
            testResults.Rows.Add(new object[] { "dbo_Save_person(1)", "asdasd", "dasdasd", "111" });
            testResults.Rows.Add(new object[] { "dbo_Save_person(2)", "qweqwe", "dasfghfgdasd", "111" });
            testResults.Rows.Add(new object[] { "dbo_Save_person(3)", "fghfhf", "fgh", "111" });
            return testResults;
        }

        public IEnumerable<IDbOutputMapping> GetDbOutputMappings(IDbAction action)
        {
            return new List<IDbOutputMapping> { new DbOutputMapping("bob", "The"), new DbOutputMapping("dora", "The"), new DbOutputMapping("Tree", "The") }; 
        }

        public void SaveService(IDatabaseService toModel)
        {
      
            _updateRepository.Save(toModel);
        }

        #endregion
    }
}