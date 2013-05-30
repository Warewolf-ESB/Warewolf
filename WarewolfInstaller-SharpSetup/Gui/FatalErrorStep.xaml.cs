using SharpSetup.UI.Wpf.Forms.Modern;

namespace Gui
{
    /// <summary>
    /// Interaction logic for FatalErrorStep.xaml
    /// </summary>
    public partial class FatalErrorStep : ModernInfoStep
    {
        public FatalErrorStep(string message)
        {
            InitializeComponent();
            lblMessage.Text = message;
        }
    }
}
