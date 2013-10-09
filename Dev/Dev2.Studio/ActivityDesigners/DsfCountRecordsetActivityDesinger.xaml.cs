using System;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Activities.Utils;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfCountRecordsetActivityDesigner : IDisposable
    {

        public DsfCountRecordsetActivityDesigner()
        {
            InitializeComponent();
        }


        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            ModelItem item = newItem as ModelItem;

            if (item != null)
            {
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
        }

       
        public void Dispose()
        {
            
        }

        void Recordsettxt_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox != null)
            {
                var text = textBox.Text;

                if(!String.IsNullOrEmpty(text))
                {
                    var result = ActivityDesignerLanuageNotationConverter.ConvertToTopLevelRSNotation(text);
                    textBox.Text = result;
                }
            }
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfCountRecordsetActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivityHelper.HandleMouseDoubleClick(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            ActivityHelper.HandleDragEnter(e);
        }

        void DsfCountRecordsetActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfCountRecordsetActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
