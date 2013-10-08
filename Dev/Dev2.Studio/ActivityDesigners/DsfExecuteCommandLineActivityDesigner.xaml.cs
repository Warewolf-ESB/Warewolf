using System;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfExecuteCommandLineActivityDesigner : IDisposable
    {
        public DsfExecuteCommandLineActivityDesigner()
        {
            InitializeComponent();
        }


        protected override void OnModelItemChanged(object newItem)
        {
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
        
        public void Dispose()
        {
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfExecuteCommandLineActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivityHelper.HandleMouseDoubleClick(e);
        }

        void DsfExecuteCommandLineActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfExecuteCommandLineActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
