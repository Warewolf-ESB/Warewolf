/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Studio.Core.Interfaces;

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
        Grid _blackoutGrid;
        private bool _openDependencyGraph;

        public bool OpenDependencyGraph { get { return _openDependencyGraph; } }

        public ResourceChangedDialog(IContextualResourceModel model, int numOfDependances)
        {
            InitializeComponent();
            ShowView();
            Owner = Application.Current.MainWindow;
            if(numOfDependances <= 1)
            {
                tbDisplay.Text = String.Format("{0} is used by another workflow. That instance needs to be updated.", model.ResourceName);
                button3.Content = "Open Affected Workflow";
                button3.SetValue(AutomationProperties.AutomationIdProperty, "UI_ShowAffectedWorkflowsButton_AutoID");
            }
            else
            {
                tbDisplay.Text = String.Format("{0} is used in {1} instances. Those instances need to be updated.", model.ResourceName, numOfDependances);
                button3.Content = "Show Affected Workflows";
                button3.SetValue(AutomationProperties.AutomationIdProperty, "UI_ShowAffectedWorkflowsButton_AutoID");
            }
        }

        private void Button3Click(object sender, RoutedEventArgs e)
        {
            _openDependencyGraph = true;
            DialogResult = false;
        }

        public void ShowView()
        {
            var effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            var content = Application.Current.MainWindow.Content as Grid;
            _blackoutGrid = new Grid
            {
                Background = new SolidColorBrush(Colors.DarkGray),
                Opacity = 0.5
            };
            if (content != null)
            {
                content.Children.Add(_blackoutGrid);
            }
            Application.Current.MainWindow.Effect = effect;
        }

        void RemoveBlackOutEffect()
        {
            Application.Current.MainWindow.Effect = null;
            var content = Application.Current.MainWindow.Content as Grid;
            if (content != null)
            {
                content.Children.Remove(_blackoutGrid);
            }
        }

        void ResourceChangedDialog_OnClosing(object sender, CancelEventArgs e)
        {
            RemoveBlackOutEffect();
        }
    }
}
