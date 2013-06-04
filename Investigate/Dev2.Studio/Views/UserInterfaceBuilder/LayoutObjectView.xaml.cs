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
using Unlimited.Framework;

namespace Unlimited.Applications.BusinessDesignStudio {
    /// <summary>
    /// Interaction logic for LayoutObjectView.xaml
    /// </summary>
    public partial class LayoutObjectView : UserControl {
        public LayoutObjectView() {
            InitializeComponent();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount >= 2) {
                if (DataContext != null) {
                    (DataContext as dynamic).EditCommand.Execute(null);
                }
            }
        }
    }
}
