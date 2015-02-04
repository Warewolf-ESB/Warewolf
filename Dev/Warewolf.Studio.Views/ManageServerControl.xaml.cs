using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageServerControl.xaml
    /// </summary>
    public partial class ManageServerControl
    {
        public ManageServerControl()
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
