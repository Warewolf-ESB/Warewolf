using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace Dev2.Activities.Adorners
{
    public class OverlayButtonsBar
    {
        public OverlayButtonsBar()
        {
            LeftButtons = new ObservableCollection<Button>();
            RightButtons = new ObservableCollection<Button>();
        }

        public bool IsDoneButtonVisible { get; set; }

        public ObservableCollection<Button> LeftButtons { get; set; }

        public ObservableCollection<Button> RightButtons { get; set; }
    }
}