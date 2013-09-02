using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Dev2.Activities.Adorners;

namespace Dev2.Activities.Designers
{
    public abstract class ActivityTemplate : UserControl
    {
        protected ActivityTemplate()
        {
            LeftButtons = new ObservableCollection<AdornerButton>();
            RightButtons = new ObservableCollection<AdornerButton>();
        }

        public ObservableCollection<AdornerButton> LeftButtons { get; set; }

        public ObservableCollection<AdornerButton> RightButtons { get; set; }
    }
}