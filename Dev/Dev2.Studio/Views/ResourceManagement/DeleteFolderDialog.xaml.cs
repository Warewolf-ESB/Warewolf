
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for DeleteResourceDialog.xaml
    /// </summary>
    public partial class DeleteFolderDialog
    {
        public DeleteFolderDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Title = String.Format(Dev2.Resources.Languages.Core.DialogTitle_FolderHasDependencies);
            tbDisplay.Text = String.Format(Dev2.Resources.Languages.Core.DialogBody_FolderContentsHaveDependencies);
        }

        public DeleteFolderDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            tbDisplay.Text = message;
        }
    }
}
