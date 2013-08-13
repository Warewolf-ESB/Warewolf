using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dev2.Interfaces;
using Dev2.UI;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.DsfMultiAssign
{
    public class DsfMultiAssignActivityViewModel : ActivityCollectionViewModelBase<ActivityDTO>
    {
        public DsfMultiAssignActivityViewModel(ModelItem modelItem) : base(modelItem)
        {
        }

        protected override string CollectionName
        {
            get
            {
                return "FieldsCollection";
            }
        }                
    }
}
