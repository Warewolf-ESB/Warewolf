using System.Collections.ObjectModel;
using System.Windows.Controls;
using Dev2.Activities.Designers2.Core.Controls;

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
        public Dev2DataGrid DataGrid { get; set; }
    }
}
