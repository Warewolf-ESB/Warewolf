#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Infragistics.Controls.Menus;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManagePluginSourceControl.xaml
    /// </summary>
    public partial class ManageComPluginSourceControl : IView, ICheckControlEnabledView
    {
        public ManageComPluginSourceControl()
        {
            InitializeComponent();

        }

        public string GetHeaderText()
        {
            var be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }


        void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue.Equals(true) && e.NewValue.Equals(false))
            {
                SearchTextBox.IsEnabled = true;
                RefreshButton.IsEnabled = true;
            }
        }

        public IDllListingModel SelectItem(string itemName)
        {
            
            var xamDataTreeNode = GetItem(itemName);
            if (xamDataTreeNode != null)
            {
                xamDataTreeNode.IsSelected = true;
                ExplorerTree.ActiveNode = xamDataTreeNode;
            }
            return xamDataTreeNode?.Data as IDllListingModel;
        }

        public bool IsItemVisible(string itemName)
        {
            var xamDataTreeNode = GetItem(itemName);
            return xamDataTreeNode != null;
        }

        XamDataTreeNode GetItem(string itemName)
        {
            var xamDataTreeNodes = TreeUtils.Descendants(ExplorerTree.Nodes.ToArray());
            return xamDataTreeNodes.FirstOrDefault(node =>
            {
                if (node.Data is IDllListingModel item && item.Name.ToLowerInvariant().Contains(itemName.ToLowerInvariant()))
                {
                    return true;
                }

                return false;
            });
        }

        public string GetAssemblyName()
        {
            var be = AssemblyNameTextBox.GetBindingExpression(TextBox.TextProperty);
            be?.UpdateTarget();
            return AssemblyNameTextBox.Text;
        }

        public void ClearFilter()
        {
            SearchTextBox.ClearSearchCommand.Execute(null);
        }

        public void PerformSave()
        {
            var viewModel = DataContext as ManageComPluginSourceViewModel;
            viewModel?.OkCommand.Execute(null);
        }

        public void SetAssemblyName(string assemblyName)
        {
            AssemblyNameTextBox.Text = assemblyName;
            var be = AssemblyNameTextBox.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateSource();
        }

        public IDllListingModel OpenItem(string itemNameToOpen)
        {
            var xamDataTreeNode = GetItem(itemNameToOpen);
            if (xamDataTreeNode != null)
            {
                xamDataTreeNode.IsExpanded = true;
            }
            return xamDataTreeNode?.Data as IDllListingModel;
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    var viewModel = DataContext as ManageComPluginSourceViewModel;
                    return viewModel != null && viewModel.OkCommand.CanExecute(null);
                case "Filter":
                    return SearchTextBox.IsEnabled;
                default:
                    break;
            }
            return false;
        }

        public void ExecuteRefresh()
        {
            RefreshButton.Command.Execute(null);
        }

        public void FilterItems()
        {
            var count = ExplorerTree.Nodes.Count;
        }



    }
}
