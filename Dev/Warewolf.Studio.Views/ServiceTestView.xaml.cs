using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Dev2.UI;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ServiceTestView.xaml
    /// </summary>
    public partial class ServiceTestView : IView
    {
        public ServiceTestView()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)TestsListbox.Items).CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var item = e.NewItems[0];
                // scroll the new item into view
                TestsListbox.SelectedItem = item;
                TestsListbox.ScrollIntoView(item);
            }
        }

        private void TestsListbox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var viewModel = listBox.DataContext as IServiceTestViewModel;

                var frameworkElement = e.OriginalSource as FrameworkElement;
                if (frameworkElement != null)
                {
                    if (viewModel != null)
                    {
                        viewModel.SelectedServiceTest = null;
                        var model = frameworkElement.DataContext as IServiceTestModel;

                        if (model != null)
                        {
                            viewModel.SelectedServiceTest = model;
                            if (viewModel.SelectedServiceTest != null)
                            {
                                viewModel.SelectedServiceTest.IsTestSelected = true;
                            }
                        }
                    }
                }
            }
        }

        private void TxtValue_OnTextChanged(object sender, RoutedEventArgs e)
        {
            var textBox = sender as IntellisenseTextBox;
            if(textBox != null)
            {
                RefreshCommands(e);
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var textBox = sender as CheckBox;
            if (textBox != null)
            {
                RefreshCommands(e);
            }
        }

        private void RefreshCommands(RoutedEventArgs e)
        {
            var serviceTestViewModel = DataContext as IServiceTestViewModel;
            serviceTestViewModel?.RefreshCommands();
            e.Handled = true;
        }

        private void SelectedTestCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                var item = cb.DataContext;
                TestsListbox.SelectedItem = item;
            }
        }

        private void SelectedTestRunTestButton_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                var item = btn.DataContext;
                TestsListbox.SelectedItem = item;
            }
        }
    }
}
