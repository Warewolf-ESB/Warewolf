/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Studio.Core;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.ResourceManagement
{
    public interface IResourceChangedDialog
    {
        bool OpenDependencyGraph { get; }
        bool? ShowDialog();
    }

    /// <summary>
    /// Interaction logic for ResourceChangedDialog.xaml
    /// </summary>
    public partial class ResourceChangedDialog : IResourceChangedDialog
    {
        readonly Grid _blackoutGrid = new Grid();
        private bool _openDependencyGraph;

        public bool OpenDependencyGraph => _openDependencyGraph;

        public ResourceChangedDialog(IContextualResourceModel model, int numOfDependances)
        {
            InitializeComponent();
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            Owner = Application.Current.MainWindow;
            if(numOfDependances <= 1)
            {
                tbDisplay.Text = $"{model.ResourceName} is used by another workflow. That instance needs to be updated.";
                button3.Content = "Open Affected Workflow";
                button3.SetValue(AutomationProperties.AutomationIdProperty, "UI_ShowAffectedWorkflowsButton_AutoID");
            }
            else
            {
                tbDisplay.Text = $"{model.ResourceName} is used in {numOfDependances} instances. Those instances need to be updated.";
                button3.Content = "Show Affected Workflows";
                button3.SetValue(AutomationProperties.AutomationIdProperty, "UI_ShowAffectedWorkflowsButton_AutoID");
            }
        }

        private void Button3Click(object sender, RoutedEventArgs e)
        {
            _openDependencyGraph = true;
            DialogResult = false;
        }

        void ResourceChangedDialog_OnClosing(object sender, CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }
    }
}
