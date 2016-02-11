using System;
using System.Windows;
using System.Windows.Input;
using Infragistics.Controls.Grids;

namespace Dev2.Activities.Designers2.Core
{
    /// <summary>
    /// Interaction logic for ManageServiceInputView.xaml
    /// </summary>
    public partial class ManageServiceInputView
    {
        public ManageServiceInputView()
        {
            InitializeComponent();
            DoneButton.IsEnabled = false;
            KeyDown += OnKeyDown; 
        }

        void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Escape)
            {
                RequestClose();
            }
        }

        public void ShowView()
        {
            Show();
        }

        public void RequestClose()
        {
            Close();
        }

        public void OutputDataGridResize()
        {
            if (OutputsDataGrid != null && OutputsDataGrid.Columns != null && OutputsDataGrid.Columns.Count > 10)
            {
                OutputsDataGrid.ColumnWidth = ColumnWidth.SizeToHeader;
            }
        }

        void TestActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            DoneButton.IsEnabled = true;
        }

        void ManageServiceInputView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
