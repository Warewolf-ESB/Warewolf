
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DataMapping
    {
        public DataMapping()
        {
            InitializeComponent();
        }


        private void TextBoxLostFocus(dynamic sender, RoutedEventArgs e)
        {
            dynamic dataContext = DataContext;
            dataContext.InputLostFocusTextBox(sender.Text);
        }

        private void TextBoxLostFocus1(dynamic sender, RoutedEventArgs e)
        {
            dynamic dataContext = DataContext;
            dataContext.OutputLostFocusTextBox(sender.Text);
        }

        private void InputTxtGotFocus(dynamic sender, RoutedEventArgs e)
        {
            dynamic dataContext = DataContext;
            dataContext.InputTextBoxGotFocus(sender.DataContext);
        }

        private void OutputTxtGotFocus(dynamic sender, RoutedEventArgs e)
        {
            dynamic dataContext = DataContext;
            dataContext.OutputTextBoxGotFocus(sender.DataContext);
        }

        private void OutputTxtKeyUp(dynamic sender, KeyEventArgs e)
        {
            dynamic dataContext = DataContext;
            dataContext.OutputLostFocusTextBox(sender.Text);
        }

        private void InputTxtKeyUp(dynamic sender, KeyEventArgs e)
        {
            dynamic dataContext = DataContext;
            dataContext.InputLostFocusTextBox(sender.Text);
        }
    }
}
