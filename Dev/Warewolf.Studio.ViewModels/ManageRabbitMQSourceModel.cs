using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.RabbitMQ;
using System.Collections.Generic;

namespace Warewolf.Studio.ViewModels
{
    // ReSharper disable once InconsistentNaming
    public class ManageRabbitMQSourceModel : IRabbitMQSourceModel
    {
        private readonly IStudioUpdateManager _updateManager;
        private readonly IQueryManager _queryManager;
        private readonly IShellViewModel _shellViewModel;

        public ManageRabbitMQSourceModel(IStudioUpdateManager updateManager, IQueryManager queryManager, IShellViewModel shellViewModel)
        {
            _updateManager = updateManager;
            _queryManager = queryManager;
            _shellViewModel = shellViewModel;
        }

        #region Implementation of IRabbitMQSourceModel

        public ICollection<IRabbitMQServiceSourceDefinition> RetrieveSources()
        {
            return new List<IRabbitMQServiceSourceDefinition>(_queryManager.FetchRabbitMQServiceSources());
        }

        public void CreateNewSource()
        {
            _shellViewModel.NewRabbitMQSource(string.Empty);
        }

        public void EditSource(IRabbitMQServiceSourceDefinition selectedSource)
        {
            _shellViewModel.EditResource(selectedSource);
        }

        public string TestSource(IRabbitMQServiceSourceDefinition source)
        {
            return _updateManager.TestConnection(source);
        }

        public void SaveSource(IRabbitMQServiceSourceDefinition source)
        {
            _updateManager.Save(source);
        }

        #endregion Implementation of IRabbitMQSourceModel
    }
}