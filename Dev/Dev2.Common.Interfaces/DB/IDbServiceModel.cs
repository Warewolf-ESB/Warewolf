using System.Collections.Generic;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Dev2.Common.Interfaces.DB
{
    public interface IDbServiceModel {
        ICollection<IDbSource> RetrieveSources();

        void CreateNewSource();

        void EditSource(IManageDatabaseSourceViewModel selectedSource);
    }
}