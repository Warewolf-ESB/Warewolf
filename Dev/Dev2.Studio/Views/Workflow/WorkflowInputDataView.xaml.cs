using System.Timers;
using Dev2.DataList.Contract;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.UI;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Dev2.Studio.Views.Workflow
{
    /// <summary>
    /// Interaction logic for WorkflowInputDataWindow.xaml
    /// </summary>
    public partial class WorkflowInputDataView
    {
        public WorkflowInputDataView()
        {
            InitializeComponent();
            SetUpTextEditor();
        }

        private TextEditor _editor;
        private AbstractFoldingStrategy _foldingStrategy;
        private FoldingManager _foldingManager;
        Timer _foldingUpdateTimer;

        private void SetUpTextEditor()
        {
            _editor = new TextEditor();
            _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            _editor.ShowLineNumbers = true;
            _editor.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            _editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;            

            _foldingStrategy = new XmlFoldingStrategy();
            _foldingManager = FoldingManager.Install(_editor.TextArea);
            _editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();

            _foldingUpdateTimer = new Timer();
            _foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2).TotalSeconds;
            _foldingUpdateTimer.Elapsed += OnFoldingUpdateTimerOnTick;
            _foldingUpdateTimer.Start();
        }

        void OnFoldingUpdateTimerOnTick(object sender, EventArgs e)
        {
            if(_foldingStrategy != null && _foldingManager != null)
            {
                Application.Current.Dispatcher.Invoke(() => _foldingStrategy.UpdateFoldings(_foldingManager, _editor.Document));

            }
        }

        private void ShowDataInOutputWindow(string input)
        {
            _editor.Text = input;
            XmlOutput.Content = _editor;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = e.OriginalSource as TextBox;
            var dli = tb.DataContext as IDataListItem;
            var vm = DataContext as WorkflowInputDataViewModel;
            if(vm != null)
            {
                vm.AddRow(dli);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tabCtrl = e.Source as TabControl;
                var tabItem = tabCtrl.SelectedItem as TabItem;
                var vm = DataContext as WorkflowInputDataViewModel;
                if (vm != null)
                {
                    if (tabItem != null && tabItem.Header.ToString() == "XML")
                    {
                        vm.SetXMLData();
                        ShowDataInOutputWindow(vm.XmlData);
                    }
                    else
                    {
                        vm.XmlData = _editor.Text;
                        vm.SetWorkflowInputData();
                    }
                }
            }
        }

        private void MenuItem_AddRow(object sender, RoutedEventArgs e)
        {
            int indexToSelect = 1;
            var vm = DataContext as WorkflowInputDataViewModel;

            if (vm != null && (vm.AddBlankRow(DataListInputs.SelectedItem as IDataListItem, out indexToSelect)))
            {
                DataListInputs.SelectedIndex = indexToSelect;
                Dispatcher.BeginInvoke(new Action(FocusOnAddition), DispatcherPriority.ApplicationIdle);
            } 
        }

        private void FocusOnAddition()
        {
            var row = GetSelectedRow(DataListInputs);
            if (row != null)
            {
                var intelbox = FindByName("txtValue", row) as IntellisenseTextBox;
                if (intelbox != null)
                {
                    intelbox.Focus();
                }
            }
        }

        private void MenuItem_DeleteRow(object sender, RoutedEventArgs e)
        {
            int indexToSelect = 1;
            var vm = DataContext as WorkflowInputDataViewModel;
            if (vm != null && (vm.RemoveRow(DataListInputs.SelectedItem as IDataListItem, out indexToSelect)))
            {
                DataListInputs.SelectedIndex = indexToSelect;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = e.OriginalSource as TextBox;
            if(tb != null)
            {
                tb.SelectAll();
            }
        }

        private void IntellisenseTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int indexToSelect = 1;
            if ((e.Key == Key.Enter || e.Key == Key.Return) && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                var vm = DataContext as WorkflowInputDataViewModel;
                if (vm != null && (vm.AddBlankRow(DataListInputs.SelectedItem as IDataListItem,out indexToSelect)))
                {
                    DataListInputs.SelectedIndex = indexToSelect;
                    Dispatcher.BeginInvoke(new Action(FocusOnAddition), DispatcherPriority.ApplicationIdle);
                }                                                
                e.Handled = true;
            }
            else if (e.Key == Key.Delete && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                var vm = DataContext as WorkflowInputDataViewModel;
                if (vm != null && (vm.RemoveRow(DataListInputs.SelectedItem as IDataListItem, out indexToSelect)))
                {
                    DataListInputs.SelectedIndex = indexToSelect;
                }
                
                e.Handled = true;
            }
        }        

        private void DataListInputs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {                      
            var row = GetSelectedRow(DataListInputs);
            if (row != null)
            {
                var intelbox = FindByName("txtValue", row) as IntellisenseTextBox;
                if (intelbox != null)
                {
                    intelbox.Focus();
                }
            }
        }

        private FrameworkElement FindByName(string name, FrameworkElement root)
        {
            if (root != null)
            {
                var tree = new Stack<FrameworkElement>();
                tree.Push(root);
                while (tree.Count > 0)
                {
                    FrameworkElement current = tree.Pop();
                    if (current.Name == name)
                        return current;

                    int count = VisualTreeHelper.GetChildrenCount(current);
                    for (int SupplierCounter = 0; SupplierCounter < count; ++SupplierCounter)
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(current, SupplierCounter);
                        if (child is FrameworkElement)
                            tree.Push((FrameworkElement)child);
                    }
                }
            }
            return null;
        }

        public static DataGridRow GetSelectedRow(DataGrid grid)
        {
            var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
            return row;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tabItem = TabItems.SelectedItem as TabItem;
            var vm = DataContext as WorkflowInputDataViewModel;
            if (vm != null)
            {
                if (tabItem != null && tabItem.Header.ToString() == "XML")
                {
                    vm.XmlData = _editor.Text;
                    vm.SetWorkflowInputData();
                }             
            }
            _foldingUpdateTimer.Elapsed -= OnFoldingUpdateTimerOnTick;
            _foldingUpdateTimer.Stop();
            _foldingUpdateTimer = null;
        }
    }
}
