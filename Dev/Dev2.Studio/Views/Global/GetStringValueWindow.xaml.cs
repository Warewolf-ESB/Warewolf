using System.Windows;

// ReSharper disable once CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio
{
    /// <summary>
    /// Interaction logic for ConfigureSwitchWindow.xaml
    /// </summary>
    public partial class GetStringDataWindow
    {
        public delegate void GotValueHandler(string stringData);
        public event GotValueHandler GotValueEvent;

        public GetStringDataWindow()
        {
            InitializeComponent();
            txtDataElementName.Focus();
        }

        public GetStringDataWindow(string windowTitle, string labelContent, string helpString, string val)
            : this()
        {
            Title = windowTitle;
            lblValue.Content = labelContent;
            txtHelp.Text = helpString;
            txtDataElementName.Text = val;
        }

        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            if(Validate())
            {
                Close();
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!Validate())
            {
                e.Cancel = true;
            }
        }

        private bool Validate()
        {
            if(string.IsNullOrEmpty(txtDataElementName.Text))
            {
                MessageBox.Show("A value is required", "Error", MessageBoxButton.OK);
                return false;
            }
            if(GotValueEvent != null)
            {
                GotValueEvent(txtDataElementName.Text);
            }
            return true;
        }

    }
}
