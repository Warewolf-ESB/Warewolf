using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ServerUserControl.xaml
    /// </summary>
    public partial class ServerUserControl : UserControl
    {
        public ServerUserControl()
        {
            InitializeComponent();
        }

        // ReSharper disable InconsistentNaming
        void Preview_TextInput(object sender, TextCompositionEventArgs e)
            // ReSharper restore InconsistentNaming
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            e.Handled=  regex.IsMatch(e.Text);
        }
    }
}
