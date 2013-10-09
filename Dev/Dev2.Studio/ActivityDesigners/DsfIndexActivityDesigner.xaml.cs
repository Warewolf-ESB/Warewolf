using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfDateTimeActivityDesigner.xaml
    public partial class DsfIndexActivityDesigner
    {
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

        public void Dispose()
        {
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfIndexActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivityHelper.HandleMouseDoubleClick(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            ActivityHelper.HandleDragEnter(e);
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
