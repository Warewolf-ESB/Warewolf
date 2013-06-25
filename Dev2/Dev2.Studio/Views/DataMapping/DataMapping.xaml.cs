using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DataMapping : UserControl
    {
        public DataMapping()
        {
            InitializeComponent();
        }


        private void TextBox_LostFocus(dynamic sender, RoutedEventArgs e) {
            dynamic dataContext = DataContext;
            dataContext.InputLostFocusTextBox(sender.Text);
        }

        private void TextBox_LostFocus_1(dynamic sender, RoutedEventArgs e) {
            dynamic dataContext = DataContext;
            dataContext.OutputLostFocusTextBox(sender.Text);
        }

        private void InputTxt_GotFocus(dynamic sender, RoutedEventArgs e) {
            dynamic dataContext = DataContext; 
            dataContext.InputTextBoxGotFocus(sender.DataContext);            
        }

        private void OutputTxt_GotFocus(dynamic sender, RoutedEventArgs e) {
            dynamic dataContext = DataContext;
            dataContext.OutputTextBoxGotFocus(sender.DataContext);  
        }

        private void OutputTxt_KeyUp(dynamic sender, KeyEventArgs e) {
            dynamic dataContext = DataContext;
            dataContext.OutputLostFocusTextBox(sender.Text);
        }

        private void InputTxt_KeyUp(dynamic sender, KeyEventArgs e) {
            dynamic dataContext = DataContext;
            dataContext.InputLostFocusTextBox(sender.Text); 
        }
    }
}
