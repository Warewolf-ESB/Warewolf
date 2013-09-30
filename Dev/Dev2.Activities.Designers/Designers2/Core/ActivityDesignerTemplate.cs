using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Dev2.Activities.Designers2.Core
{
    public class ActivityDesignerTemplate : UserControl
    {
        public ActivityDesignerTemplate()
        {
            LeftButtons = new ObservableCollection<Button>();
            RightButtons = new ObservableCollection<Button>();
        }

        public ObservableCollection<Button> LeftButtons { get; set; }
        public ObservableCollection<Button> RightButtons { get; set; }
    }
}
