/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using Dev2.Providers.Errors;

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers2.Core.Errors
{
    public partial class ErrorsAdorner
    {
        int _errorsCounter;

        public ErrorsAdorner(FrameworkElement adornedElement)
            : base(adornedElement)
        {
            InitializeComponent();
        }

        void ErrorTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            textBlock?.SetValue(AutomationProperties.AutomationIdProperty, "UI_Error" + _errorsCounter++ + "_AutoID");
        }

        void ErrorsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            _errorsCounter = 0;
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            var actionableErrorInfo = hyperlink?.DataContext as ActionableErrorInfo;
            if (actionableErrorInfo != null)
            {
                Clipboard.SetText(actionableErrorInfo.Message);
            }
        }
    }
}
