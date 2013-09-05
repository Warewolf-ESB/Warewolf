using System.Collections.Generic;

namespace Dev2.Activities.Designers
{
    public interface IActivityCollectionViewModel : IActivityViewModelBase
    {
        void AddListToCollection(IEnumerable<string> listToAdd, bool overwrite);
    }
}