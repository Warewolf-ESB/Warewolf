namespace Gui
{
    /// <summary>
    /// Interaction logic for FatalErrorStep.xaml
    /// </summary>
    public partial class FatalErrorStep
    {
        public FatalErrorStep(string message)
        {
            InitializeComponent();
            lblMessage.Text = message;
        }
    }
}
