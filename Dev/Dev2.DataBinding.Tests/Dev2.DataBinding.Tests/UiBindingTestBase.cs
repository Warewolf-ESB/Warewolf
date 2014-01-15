using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Dev2.Studio;

namespace Dev2.DataBinding.Tests
{
    public class UiBindingTestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public UiBindingTestBase()
        {
            if(Application.Current == null)
            {
                var app = new App();
                app.InitializeComponent();
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            }
        }

        protected List<Binding> GetBindings(UIElement sqlBulkInsertDesigner)
        {
            var window = new Window();
            Grid content = new Grid();
            content.Children.Add(sqlBulkInsertDesigner);
            window.Content = content;
            window.Show();
            var bindingList = new List<Binding>();
            GetBindingsRecursive(sqlBulkInsertDesigner, bindingList);
            return bindingList;
        }

        void GetBindingsRecursive(DependencyObject dObj, List<Binding> bindingList)
        {
            bindingList.AddRange(DependencyObjectHelper.GetBindingObjects(dObj));

            int childrenCount = VisualTreeHelper.GetChildrenCount(dObj);
            if(childrenCount > 0)
            {
                for(int i = 0; i < childrenCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dObj, i);
                    GetBindingsRecursive(child, bindingList);
                }
            }
        }
    }
}
