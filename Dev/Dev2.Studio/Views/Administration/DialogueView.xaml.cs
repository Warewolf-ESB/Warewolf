using System.Windows;
using System.Windows.Input;

namespace Dev2.Studio.Views.Administration
{
    /// <summary>
    ///     Interaction logic for Dev2Dialogue.xaml
    /// </summary>
    public partial class DialogueView
    {
        public DialogueView()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Hello");
        }

    }
}