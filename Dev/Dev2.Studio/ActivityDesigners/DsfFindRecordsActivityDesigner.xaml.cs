using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.DataList;
using Dev2.Utilities;

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

            foreach(string item in e.AddedItems)
            {
                if(item == "Doesn't Contains" || item == "Contains" || item == "=" || item == "<> (Not Equal)" || item == "Ends With" || item == "Starts With" || item == "Is Regex" || item == ">" || item == "<" || item == "<=" || item == ">=")
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
            ActivityHelper.HandleMouseDoubleClick(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            ActivityHelper.HandleDragEnter(e);
        }

        void DsfFindRecordsActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if(uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfFindRecordsActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if(uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
