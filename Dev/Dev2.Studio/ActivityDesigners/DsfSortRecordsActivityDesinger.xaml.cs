using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Activities.Presentation.Model;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public partial class DsfSortRecordsActivityDesigner : IDisposable {
        public DsfSortRecordsActivityDesigner() {
            InitializeComponent();
            Ordercbx.Items.Add("Forward");
            Ordercbx.Items.Add("Backwards");            
        }


        protected override void OnModelItemChanged(object newItem) {
            base.OnModelItemChanged(newItem);

            ModelItem item = newItem as ModelItem;


            ModelItem parent = item.Parent;

            while (parent != null)
            {
                if (parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }       
        }

        public void Dispose() {            
        }

        private void CbxLoad(object sender, RoutedEventArgs e)
        {
            if(sender is ComboBox)
            {
                var cbx = sender as ComboBox;
                dynamic temp = cbx.DataContext;
                string selectedVal = temp.ModelItem.SelectedSort;
                //2013.02.08: Ashley Lewis - Bug 8725, Task 8734 - Prevent blank sort order
                cbx.SelectedValue = !string.IsNullOrEmpty(selectedVal) ? selectedVal : "Forward";
            }
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfSortRecordsActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivityHelper.HandleMouseDoubleClick(e);
        }

        void DsfSortRecordsActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfSortRecordsActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
