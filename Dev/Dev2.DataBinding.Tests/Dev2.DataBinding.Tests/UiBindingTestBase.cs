using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Dev2.Studio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.DataBinding.Tests
{
    [TestClass]
    public class UiBindingTestBase
    {
        Window _window;

        [TestInitialize]
        public void TestSetUp()
        {
            if(Application.Current == null)
            {
                var app = new App();
                app.InitializeComponent();
            }
            if(SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            }
        }

        [ClassCleanup]
        public void TestClassCleanUp()
        {
            if(_window != null)
            {
                _window.Dispatcher.InvokeShutdown();
                _window.Close();
            }
            System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            if(Application.Current != null)
            {
                Application.Current.Shutdown();
            }
        }
        /// <summary>
        /// Gets the bindings.
        /// </summary>
        /// <param name="sqlBulkInsertDesigner">The SQL bulk insert designer.</param>
        /// <returns></returns>
        protected List<Binding> GetBindings(UIElement sqlBulkInsertDesigner)
        {
            var bindingList = new List<Binding>();
            _window = new Window();
            Grid content = new Grid();
            content.Children.Add(sqlBulkInsertDesigner);
            _window.Content = content;
            _window.Show();
            GetBindingsRecursive(sqlBulkInsertDesigner, bindingList);
            return bindingList;
        }

        /// <summary>
        /// Gets the bindings recursive.
        /// </summary>
        /// <param name="dObj">The command object.</param>
        /// <param name="bindingList">The binding list.</param>
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
