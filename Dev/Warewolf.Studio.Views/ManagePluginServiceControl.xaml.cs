using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManagePluginServiceControl.xaml
    /// </summary>
    public partial class ManagePluginServiceControl : IView, ICheckControlEnabledView
    {
        public ManagePluginServiceControl()
        {
            InitializeComponent();
            SourcesComboBox.Focus();
        }

        public bool IsSelectSourceFocused()
        {
            return SourcesComboBox.IsFocused;
        }
        public void SelectPluginSource(IPluginSource pluginSource)
        {
            try
            {
                SourcesComboBox.SelectedItem = pluginSource;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
        }
        public void SelectNamespace(INamespaceItem namespaceItem)
        {
            try
            {
                NamespaceComboBox.SelectedItem = namespaceItem;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
        }
        public void SelectAction(IPluginAction pluginAction)
        {
            try
            {
                ActionsComboBox.SelectedItem = pluginAction;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
        }

        public void TestAction()
        {
            TestButton.Command.Execute(null);
        }

        public void EditAction()
        {
            EditButton.Command.Execute(null);
        }

        public void NewAction()
        {
            NewButton.Command.Execute(null);
        }

        public void RefreshAction()
        {
            RefreshButton.Command.Execute(null);
        }

        public void Save()
        {
            var viewModel = DataContext as ManagePluginServiceViewModel;
            if (viewModel != null)
            {
                viewModel.SaveCommand.Execute(null);
            }
        }

        public IPluginSource GetSelectedPluginSource()
        {
            var selectedSource = SourcesComboBox.SelectedItem as IPluginSource;
            
            return selectedSource;
        }

        public IPluginAction GetSelectedActionSource()
        {
            var selectedAction = ActionsComboBox.SelectedItem as IPluginAction; ;
            
            return selectedAction;
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    var viewModel = DataContext as ManagePluginServiceViewModel;
                    return viewModel != null && viewModel.SaveCommand.CanExecute(null);
                case "Test":
                    return TestButton.Command.CanExecute(null);
                case "1 Select a Source":
                    return SourcesComboBox.IsEnabled;
                case "2 Select a Namespace":
                    {
                        BindingExpression be = NamespaceGrid.GetBindingExpression(VisibilityProperty);
                        if (be != null)
                        {
                            be.UpdateTarget();
                        }
                        return NamespaceGrid.Visibility == Visibility.Visible;
                    }
                case "3 Select an Action":
                    {
                        BindingExpression be = ActionGrid.GetBindingExpression(VisibilityProperty);
                        if (be != null)
                        {
                            be.UpdateTarget();
                        }
                        return ActionGrid.Visibility == Visibility.Visible;
                    }
                case "4 Provide Test Values":
                    {
                        BindingExpression be = TestGrid.GetBindingExpression(VisibilityProperty);
                        if (be != null)
                        {
                            be.UpdateTarget();
                        }
                        return TestGrid.Visibility == Visibility.Visible;
                    }
                case "5 Default and Mapping":
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

        public ItemCollection GetInputs()
        {
            BindingExpression be = InputGrid.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return InputGrid.ItemsSource as ItemCollection;
        }

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        public string[] GetActionNames()
        {
           return ActionsComboBox.Items.Select(a => ((IPluginAction)a.Data).FullName).ToArray();
        }
    }
}
