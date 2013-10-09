using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Activities.Presentation.Model;
using Caliburn.Micro;
using Dev2.Activities.Utils;
using Dev2.Common;
using Dev2.Data.Enums;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.Messages;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfForEachActivityDesigner : IDisposable, IHandle<DataListItemSelectedMessage>
    {
        public DsfForEachActivityDesigner()
        {
            InitializeComponent();
            EventPublishers.Aggregator.Subscribe(this);
            DropPoint.PreviewDrop += DropPointOnDragEnter;
            DropPoint.PreviewDragOver += DropPointOnDragEnter;
        }

        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;
            var formats = e.Data.GetFormats();
            if (!formats.Any()) return;
            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemFormat") >= 0);
            if (String.IsNullOrEmpty(modelItemString))
            {
                modelItemString = formats.FirstOrDefault(s => s.IndexOf("WorkflowItemTypeNameFormat") >= 0);
                if (String.IsNullOrEmpty(modelItemString)) return;
            }
            var objectData = e.Data.GetData(modelItemString);
            ForeachActivityDesignerUtils foreachActivityDesignerUtils = new ForeachActivityDesignerUtils();
            dropEnabled = foreachActivityDesignerUtils.ForeachDropPointOnDragEnter(objectData);
            if (!dropEnabled)
                {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }

            
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

        public void Handle(DataListItemSelectedMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
        }

        public void Dispose()
        {
            EventPublishers.Aggregator.Unsubscribe(this);
        }

        void CbxForEachType_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            if (cbx != null)
            {
                if (cbx.Items.Count == 0)
                {
                    cbx.ItemsSource = Dev2EnumConverter.ConvertEnumsTypeToStringList<enForEachType>();
                }
            }
        }

        void CbxForEachType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {          
            switch ((string)e.AddedItems[0])
            {
                case "* in Range":
                    txtFrom.Visibility = Visibility.Visible;
                    txtTo.Visibility = Visibility.Visible;
                    txtCSVIndexes.Visibility = Visibility.Hidden;
                    txtNumber.Visibility = Visibility.Hidden;
                    txtRecordset.Visibility = Visibility.Hidden;
                    break;

                case "* in CSV":
                    txtFrom.Visibility = Visibility.Hidden;
                    txtTo.Visibility = Visibility.Hidden;
                    txtCSVIndexes.Visibility = Visibility.Visible;
                    txtNumber.Visibility = Visibility.Hidden;
                    txtRecordset.Visibility = Visibility.Hidden;
                    break;

                case "* in Recordset":
                    txtFrom.Visibility = Visibility.Hidden;
                    txtTo.Visibility = Visibility.Hidden;
                    txtCSVIndexes.Visibility = Visibility.Visible;
                    txtNumber.Visibility = Visibility.Hidden;
                    txtRecordset.Visibility = Visibility.Visible;
                    break;

                default:
                    txtFrom.Visibility = Visibility.Hidden;
                    txtTo.Visibility = Visibility.Hidden;
                    txtCSVIndexes.Visibility = Visibility.Hidden;
                    txtNumber.Visibility = Visibility.Visible;
                    txtRecordset.Visibility = Visibility.Hidden;
                    break;
            }
        }

        void DsfForEachActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfForEachActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }

        void DsfForEachActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivityHelper.HandleMouseDoubleClick(e);
        }
        
        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
           ActivityHelper.HandleDragEnter(e);
        }
    }
}
