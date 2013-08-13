using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Activities.Presentation.Model;
using Caliburn.Micro;
using Dev2.Activities.Utils;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Data.Enums;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfForEachActivityDesigner : IDisposable, IHandle<DataListItemSelectedMessage>
    {
        public DsfForEachActivityDesigner()
        {
            InitializeComponent();
            EventAggregator = ImportService.GetExportValue<IEventAggregator>();
            EventAggregator.Subscribe(this);
            this.DropPoint.PreviewDrop += DropPointOnDragEnter;
            this.DropPoint.PreviewDragOver += DropPointOnDragEnter;
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

        protected IEventAggregator EventAggregator { get; set; }
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

     
        private void Highlight(IDataListItemModel dataListItemViewModel) {
            //List<string> containingFields = new List<string>();

            //ForEverytxt.BorderBrush = Brushes.LightGray;
            //ForEverytxt.BorderThickness = new Thickness(1.0);

            //containingFields = DsfActivityDataListComparer.ContainsDataListItem(ModelItem, dataListItemViewModel);

            //if (containingFields.Count > 0) {
            //    foreach (string item in containingFields) {
            //        if (item.Equals("foreachElementName")) {
            //            ForEverytxt.BorderBrush = System.Windows.Media.Brushes.Aqua;
            //            ForEverytxt.BorderThickness = new Thickness(2.0);
            //        }
            //    }
            //}
                    }
        public void Handle(DataListItemSelectedMessage message)
        {
            //Highlight(message.DataListItemModel);
        }

        public void Dispose()
        {
           EventAggregator.Unsubscribe(this);
        }

        private void ForEverytxt_LostFocus(object sender, RoutedEventArgs e)
        {
            //var textBox = sender as TextBox;
            //char[] token = { ']' };
            //string[] tokens = textBox.Text.Split(token);
            //if (!String.IsNullOrEmpty(textBox.Text)) {
            //    if (textBox.Text.EndsWith("]]") && tokens.Count() == 3) {
            //        if (!textBox.Text.Contains("()")) {
            //            textBox.Text = textBox.Text.Insert(textBox.Text.IndexOf("]"), "()");
            //        }
            //    }
                //else if (textBox.Text.EndsWith("]]") && tokens.Count() > 3) {
                // we have a recursive evaluation happening, only scalars or recordset().field are allowed

                //}
            //}
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
            e.Handled = true;
        }
        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            // 2013.07.29: Ashley Lewis for bug 9949 - workaround for Automatic-drill-down
        }
    }
}
