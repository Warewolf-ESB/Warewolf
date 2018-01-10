/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using Dev2.Common.ExtMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;


namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for DeleteResourceDialog.xaml
    /// </summary>
    public partial class RenameResourceDialog
    {

        readonly bool _openDependencyGraph = false;

        public bool OpenDependencyGraph => _openDependencyGraph;

        public RenameResourceDialog(IContextualResourceModel model, string newName, Window owner)
        {
            InitializeComponent();
            Owner = owner ?? Application.Current.MainWindow;
            Title = string.Format(StringResources.DialogTitle_HasDuplicateName, newName);
            tbDisplay.Text = string.Format(StringResources.DialogBody_HasDuplicateName,
                                model.ResourceType.GetDescription(), model.ResourceName);
        }

        public RenameResourceDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            tbDisplay.Text = message;
        }

        void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
