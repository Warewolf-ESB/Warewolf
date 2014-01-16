using System.IO;
using System.Text;
using System.Windows;

namespace Gui
{
    /// <summary>
    /// Interaction logic for LicenseStep.xaml
    /// </summary>
    public partial class LicenseStep
    {
        public LicenseStep(int stepNumber, int totalSteps)
        {
            InitializeComponent();
            rtbLicense.Selection.Load(new MemoryStream(Encoding.Default.GetBytes(Properties.Resources.WAREWOLF_EULA_EN)), DataFormats.Rtf);
            Loaded += (o, e) => cbAccept.Focus();
            DataContext = new InfoStepDataContext(stepNumber, totalSteps);
        }
    }
}
