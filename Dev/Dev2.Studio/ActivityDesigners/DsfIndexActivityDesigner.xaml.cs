using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfDateTimeActivityDesigner.xaml
    public partial class DsfIndexActivityDesigner
    {
        private bool _isRegistered = false;
        private string _mediatorKey = string.Empty;
        private ModelItem activity;

        public IList<string> IndexList { get; set; }

        public IList<string> DirectionList { get; set; }

        public DsfIndexActivityDesigner()
        {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            if (!_isRegistered)
            {
                // This is here because it might come in later
                //mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => Highlight(input as IDataListItemModel));
            }
            activity = newItem as ModelItem;

            IndexList = new List<string> { "First Occurrence", "Last Occurrence", "All Occurrences" };
            DirectionList = new List<string> { "Left to Right", "Right to Left" };
            cbxIndex.ItemsSource = IndexList;
            cbxDirection.ItemsSource = DirectionList;

            // 1203, CODE REVIEW, Null check needed
            ModelItem parent = activity.Parent;

            while (parent != null)
            {
                if (parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }
        }

        // This is here because it might come in later
        //private void Highlight(IDataListItemModel dataListItemViewModel)
        //{            
        //ObservableCollection<string> containingFields = new ObservableCollection<string>();
        //border.Visibility = Visibility.Hidden;

        //SetValuetxt.BorderBrush = Brushes.LightGray;
        //SetValuetxt.BorderThickness = new Thickness(1.0);
        //ToValuetxt.BorderBrush = Brushes.LightGray;
        //ToValuetxt.BorderThickness = new Thickness(1.0);

        //containingFields = DsfActivityDataListComparer.ContainsDataListItem(ModelItem, dataListItemViewModel);

        //if (containingFields.Count > 0) {
        //    foreach (string item in containingFields) {
        //        if (item.Equals("FieldName")) {
        //            SetValuetxt.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
        //            SetValuetxt.BorderThickness = new Thickness(2.0);
        //        }
        //        else if (item.Equals("FieldValue")) {
        //            ToValuetxt.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
        //            ToValuetxt.BorderThickness = new Thickness(2.0);
        //        }
        //        var bob = this.BorderBrush;


        //    }
        //}
        //}

        public void Dispose()
        {
            // This is here because it might come in later
            //Mediator.DeRegister(MediatorMessages.DataListItemSelected, _mediatorKey);
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfIndexActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void DsfIndexActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfIndexActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
