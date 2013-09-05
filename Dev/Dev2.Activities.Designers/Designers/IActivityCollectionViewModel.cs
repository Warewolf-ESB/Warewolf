using System.Collections.Generic;

namespace Dev2.Activities.Designers
{
    public interface IActivityCollectionViewModel
    {
        void AddListToCollection(IEnumerable<string> listToAdd, bool overwrite);
    }
}