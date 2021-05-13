/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using Dev2.Common.ExtMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;


namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for DeleteResourceDialog.xaml
    /// </summary>
    public partial class DeleteResourceDialog
    {
        bool _openDependencyGraph;

        public bool OpenDependencyGraph => _openDependencyGraph;

        public DeleteResourceDialog(IContextualResourceModel model)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Title = String.Format(StringResources.DialogTitle_HasDependencies, model.ResourceType.GetDescription());
            tbDisplay.Text = String.Format(StringResources.DialogBody_HasDependencies, model.ResourceName,
                                                    model.ResourceType.GetDescription());
        }

        public DeleteResourceDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            tbDisplay.Text = message;
        }

        void Button3Click(object sender, RoutedEventArgs e)
        {
            _openDependencyGraph = true;
            DialogResult = false;
        }

        void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
