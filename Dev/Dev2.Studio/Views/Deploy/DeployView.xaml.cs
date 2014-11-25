
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Windows;
using Dev2.Models;
using Dev2.Studio.ViewModels.Deploy;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.Deploy
{
    /// <summary>
    /// Interaction logic for DeployView.xaml
    /// </summary>
    public partial class DeployView
    {
        public DeployView()
        {
            InitializeComponent();
        }

        void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if(frameworkElement != null)
            {
                ExplorerItemModel rtvm = frameworkElement.DataContext as ExplorerItemModel;
                if(rtvm != null)
                {
                    DeployViewModel viewModel = DataContext as DeployViewModel;
                    if(viewModel != null)
                    {
                        viewModel.SelectDependencies(new List<IExplorerItemModel> { rtvm });
                    }
                }
            }
        }
    }
}
