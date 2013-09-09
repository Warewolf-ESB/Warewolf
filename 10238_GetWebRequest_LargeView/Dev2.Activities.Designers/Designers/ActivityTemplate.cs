using System.Collections.ObjectModel;
using System.Windows.Controls;
using Dev2.Activities.Adorners;

namespace Dev2.Activities.Designers
{
    public interface IHasActivityViewModelBase
    {
        IActivityViewModelBase ActivityViewModelBase { get; }
    }

    public abstract class ActivityTemplate : UserControl, IHasActivityViewModelBase
    {
        protected ActivityTemplate()
        {
            LeftButtons = new ObservableCollection<AdornerButton>();
            RightButtons = new ObservableCollection<AdornerButton>();
        }

        public ObservableCollection<AdornerButton> LeftButtons { get; set; }
        public ObservableCollection<AdornerButton> RightButtons { get; set; }

        public abstract IActivityViewModelBase ActivityViewModelBase { get; set; }
    }
}
