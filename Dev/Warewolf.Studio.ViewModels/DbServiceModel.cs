using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Warewolf.Studio.ViewModels
{
    public class DbServiceModel : IDbServiceModel
    {
        #region Implementation of IDbServiceModel

        public ICollection<IDbSource> RetrieveSources()
        {
            return new ObservableCollection<IDbSource> { new DbSourceDefinition() { Name = "bob" }, new DbSourceDefinition() { Name = "dora" }, new DbSourceDefinition() { Name = "foo large" } };

        }

        public void CreateNewSource()
        {
        }

        public void EditSource(IManageDatabaseSourceViewModel selectedSource)
        {
        }

        #endregion
    }
}