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

namespace Unlimited.Applications.BusinessDesignStudio.Views {
    /// <summary>
    /// Interaction logic for ChooseActivityWindow.xaml
    /// </summary>
    public partial class ChooseActivityWindow : Window {
        public ChooseActivityWindow() {
            InitializeComponent();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount >= 2) {
                dynamic dcontext = this.DataContext;
                if (dcontext != null) {
                    if(dcontext.OKCommand != null)
                        dcontext.OKCommand.Execute(null);
                }



            }
        }
    }
}
