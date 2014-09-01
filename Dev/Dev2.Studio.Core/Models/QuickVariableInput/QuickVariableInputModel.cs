using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models.QuickVariableInput
{
    public class QuickVariableInputModel
    {

        private readonly ICollectionActivity _activity;
        private readonly ModelItem _modelItem;

        public QuickVariableInputModel(ModelItem modelItem, ICollectionActivity activity)
        {
            _modelItem = modelItem;
            _activity = activity;
        }

        public int GetCollectionCount()
        {
            return _activity.GetCollectionCount();
        }

        public void AddListToCollection(IList<string> listToAdd, bool overwrite)
        {
            _activity.AddListToCollection(listToAdd, overwrite, _modelItem);
        }
    }
}
