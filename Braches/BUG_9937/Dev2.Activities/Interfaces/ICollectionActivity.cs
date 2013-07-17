using System.Activities.Presentation.Model;
using System.Collections.Generic;

namespace Dev2.Interfaces
{
    public interface ICollectionActivity
    {
        int GetCollectionCount();
        void AddListToCollection(IList<string> listToAdd, bool overwrite, ModelItem modelItem);
    }
}
