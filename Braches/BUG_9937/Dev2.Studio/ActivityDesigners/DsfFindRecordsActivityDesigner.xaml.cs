
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.DataList;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;


namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfFindRecordsActivityDesigner.xaml
    public partial class DsfFindRecordsActivityDesigner
    {
        public IList<string> ItemList { get; set; }

        public DsfFindRecordsActivityDesigner()
        {
            InitializeComponent();
            ItemList = FindRecsetOptions.FindAll().Select(c => c.HandlesType()).ToList();
            cbxWhere.ItemsSource = ItemList.OrderBy(c => c);
        }


        private void cbxWhere_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            foreach (string item in e.AddedItems)
            {
                if (item == "Not Contains" || item == "Contains" || item == "Equal" || item == "Not Equal" || item == "Ends With" || item == "Starts With" || item == "Regex" || item == ">" || item == "<" || item == "<=" || item == ">=")
                {
                    txtMatch.IsEnabled = true;
                }
                else
                {
                    txtMatch.IsEnabled = false;
                    txtMatch.Text = string.Empty;
                }
            }
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfFindRecordsActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void DsfFindRecordsActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfFindRecordsActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
