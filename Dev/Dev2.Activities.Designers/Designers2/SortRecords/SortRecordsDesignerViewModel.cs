using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.SortRecords
{
    public class SortRecordsDesignerViewModel : ActivityDesignerViewModel
    {
        public SortRecordsDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            SortOrderTypes = new List<string> { "Forward", "Backwards" };
            SelectedSelectedSort = string.IsNullOrEmpty(SelectedSort) ? SortOrderTypes[0] : SelectedSort;
        }

        public List<string> SortOrderTypes { get; private set; }

        public string SelectedSelectedSort { get { return (string)GetValue(SelectedSelectedSortProperty); } set { SetValue(SelectedSelectedSortProperty, value); } }

        public static readonly DependencyProperty SelectedSelectedSortProperty =
            DependencyProperty.Register("SelectedSelectedSort", typeof(string), typeof(SortRecordsDesignerViewModel), new PropertyMetadata(null, OnSelectedSelectedSortChanged));

        // DO NOT bind to these properties - these are here for convenience only!!!
       private  string SelectedSort { set { SetProperty(value);  }
            get { return  GetProperty<string>(); }
        }

        static void OnSelectedSelectedSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (SortRecordsDesignerViewModel)d;
            var value = e.NewValue as string;

            if(!string.IsNullOrWhiteSpace(value))
            {
                viewModel.SelectedSort = value;
            }
        }

        public override void Validate()
        {
        }
    }
}