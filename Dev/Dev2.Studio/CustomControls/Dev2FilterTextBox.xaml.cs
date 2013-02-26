using System.Windows;
using System.Windows.Controls;

namespace Dev2.Studio.Custom_Dev2_Controls
{
    /// <summary>
    /// Interaction logic for Dev2FilterTextBox.xaml
    /// </summary>
    public partial class Dev2FilterTextBox : UserControl
    {
        public Dev2FilterTextBox()
        {
            InitializeComponent();
        }

        void ClearFilter_OnClick(object sender, RoutedEventArgs e)
        {
            Filtertxt.Clear();
            Filtertxt.Focus();
        }
    }
}
