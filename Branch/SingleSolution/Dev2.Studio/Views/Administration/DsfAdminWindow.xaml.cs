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
using System.Windows.Shapes;
using Unlimited.Framework;

namespace Unlimited.Applications.BusinessDesignStudio.Views {
    /// <summary>
    /// Interaction logic for DsfAdminWindow.xaml
    /// </summary>
    public partial class DsfAdminWindow : UserControl
    {
        public DsfAdminWindow() {
            InitializeComponent();
            
        }

        private void DocumentContent_Loaded(object sender, RoutedEventArgs e) {
            dynamic dataContext = this.DataContext;
            dataContext.Console = txtConsole;
            dataContext.CommandText = txtUserCommand;
            txtUserCommand.Focus();
        }

        
    }
}
