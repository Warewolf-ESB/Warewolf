using System;
using System.Activities.Presentation.Toolbox;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Infragistics.Calculations;
using Infragistics.DragDrop;
using Warewolf.Studio.ViewModels.ToolBox;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ToolboxView.xaml
    /// </summary>
    public partial class ToolboxView : IToolboxView,IToolboxService
    {
        public ToolboxView()
        {
            InitializeComponent();
        }

        #region Implementation of IWarewolfView

        #endregion

        private void DragSource_OnDragStart(object sender, DragDropStartEventArgs e)
        {
            if (e != null)
            {
            }
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
//            DragSource source = DragDropManager.GetDragSource(sender as DependencyObject);
//            if (source != null)
//            {
//                var grid = source.AssociatedObject as Grid;
//                if (grid != null)
//                {
//                    var dataContext = grid.DataContext;
//                    if (dataContext != null)
//                    {
//                        BindingOperations.SetBinding(source,
//                            DragSource.DataObjectProperty,
//                            new Binding
//                            {
//                                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
//                                Path = new PropertyPath("AssociatedObject.DataContext.ActivityType")
//                            });
//                    }
//                }
//
//
//            }
        }

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var dataContext = grid.DataContext as ToolDescriptorViewModel;
                if (dataContext != null)
                {
                    var toolType = dataContext.ActivityType.GetData(System.Activities.Presentation.DragDropHelper.WorkflowItemTypeNameFormat) as Type;
                    ToolboxItem itemWrapper = new ToolboxItem(toolType);
                    var dataObject = new System.Windows.Forms.DataObject();
                    dataObject.SetData(typeof(ToolboxItem), itemWrapper);
                    DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Move | DragDropEffects.Copy);
                }
            }
        }

        public void AddCreator(ToolboxItemCreatorCallback creator, string format)
        {
            throw new NotImplementedException();
        }

        public void AddCreator(ToolboxItemCreatorCallback creator, string format, IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public void AddLinkedToolboxItem(ToolboxItem toolboxItem, IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public void AddLinkedToolboxItem(ToolboxItem toolboxItem, string category, IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public void AddToolboxItem(ToolboxItem toolboxItem)
        {
            throw new NotImplementedException();
        }

        public void AddToolboxItem(ToolboxItem toolboxItem, string category)
        {
            throw new NotImplementedException();
        }

        public ToolboxItem DeserializeToolboxItem(object serializedObject)
        {
            throw new NotImplementedException();
        }

        public ToolboxItem DeserializeToolboxItem(object serializedObject, IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public ToolboxItem GetSelectedToolboxItem()
        {
            throw new NotImplementedException();
        }

        public ToolboxItem GetSelectedToolboxItem(IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public ToolboxItemCollection GetToolboxItems()
        {
            throw new NotImplementedException();
        }

        public ToolboxItemCollection GetToolboxItems(IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public ToolboxItemCollection GetToolboxItems(string category)
        {
            throw new NotImplementedException();
        }

        public ToolboxItemCollection GetToolboxItems(string category, IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public bool IsSupported(object serializedObject, IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public bool IsSupported(object serializedObject, ICollection filterAttributes)
        {
            throw new NotImplementedException();
        }

        public bool IsToolboxItem(object serializedObject)
        {
            throw new NotImplementedException();
        }

        public bool IsToolboxItem(object serializedObject, IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void RemoveCreator(string format)
        {
            throw new NotImplementedException();
        }

        public void RemoveCreator(string format, IDesignerHost host)
        {
            throw new NotImplementedException();
        }

        public void RemoveToolboxItem(ToolboxItem toolboxItem)
        {
            throw new NotImplementedException();
        }

        public void RemoveToolboxItem(ToolboxItem toolboxItem, string category)
        {
            throw new NotImplementedException();
        }

        public void SelectedToolboxItemUsed()
        {
            throw new NotImplementedException();
        }

        public object SerializeToolboxItem(ToolboxItem toolboxItem)
        {
            throw new NotImplementedException();
        }

        public bool SetCursor()
        {
            throw new NotImplementedException();
        }

        public void SetSelectedToolboxItem(ToolboxItem toolboxItem)
        {
            throw new NotImplementedException();
        }

        public CategoryNameCollection CategoryNames
        {
            get { throw new NotImplementedException(); }
        }

        public string SelectedCategory
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }

}