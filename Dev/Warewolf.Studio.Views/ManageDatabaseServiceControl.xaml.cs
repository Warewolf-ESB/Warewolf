using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageDatabaseServiceControl.xaml
    /// </summary>
    public partial class ManageDatabaseServiceControl : IView, ICheckControlEnabledView
    {
        public ManageDatabaseServiceControl()
        {
            InitializeComponent();
            SourcesComboBox.Focus();
        }

        public void SelectDbSource(IDbSource dbSourceName)
        {
            try
            {
                SourcesComboBox.SelectedItem = dbSourceName;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
        }

        public void SelectDbAction(IDbAction actionName)
        {
            try
            {
                ActionsComboBox.SelectedItem = actionName;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch 
            {
                
            }
        }

        public void TestAction()
        {
            TestActionButton.Command.Execute(null);
        }

        public void Save()
        {
            SaveButton.Command.Execute(null);
        }

        public bool IsDataSourceFocused()
        {
            return SourcesComboBox.IsFocused;
        }

        public ItemCollection GetInputs()
        {
            BindingExpression be = InputsList.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return InputsList.Items;
        }

        public ItemCollection GetOutputs()
        {
            BindingExpression be = OutputsDataGrid.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return OutputsDataGrid.Items;
        }

        public ItemCollection GetInputMappings()
        {
            BindingExpression be = InputsMappingDataGrid.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return InputsMappingDataGrid.Items;
        }

        public ItemCollection GetOutputMappings()
        {
            BindingExpression be = OutputsMappingDataGrid.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return OutputsMappingDataGrid.Items;
        }

        public IDbSource GetSelectedDataSource()
        {
            return SourcesComboBox.SelectedItem as IDbSource;
        }

        public IDbAction GetSelectedAction()
        {
            return ActionsComboBox.SelectedItem as IDbAction;
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    return SaveButton.Command.CanExecute(null);
                case "Test":
                    return TestActionButton.Command.CanExecute(null);
                case "1 Data Source":
                    return SourcesComboBox.IsEnabled;
                case "2 Select Action":
                {
                    BindingExpression be = SelectAnActionGrid.GetBindingExpression(VisibilityProperty);
                    if (be != null)
                    {
                        be.UpdateTarget();
                    }
                    return SelectAnActionGrid.Visibility == Visibility.Visible;
                }
                case "3 Test Connector and Calculate Outputs":
                {
                    BindingExpression be = TestActionGrid.GetBindingExpression(VisibilityProperty);
                    if (be != null)
                    {
                        be.UpdateTarget();
                    }
                    return TestActionGrid.Visibility == Visibility.Visible;
                }
                case "4 Edit Default and Mapping Names":
                {
                    BindingExpression be = MappingsGrid.GetBindingExpression(VisibilityProperty);
                    if (be != null)
                    {
                        be.UpdateTarget();
                    }
                    return MappingsGrid.Visibility == Visibility.Visible;
                }
            }
            return false;
        }
    }
}
