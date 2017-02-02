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
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Dialogs;
using Warewolf.Studio.Core;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.Workflow
{
    /// <summary>
    /// Interaction logic for DsfActivityDropWindow.xaml
    /// </summary>
    public partial class DsfActivityDropWindow : IDialog
    {
        readonly Grid _blackoutGrid = new Grid();
        public DsfActivityDropWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
        }

        void DsfActivityDropWindow_OnClosing(object sender, CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        private void DsfActivityDropWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void DsfActivityDropWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                mainViewModel?.ResetMainView();
            }
        }
    }
}
