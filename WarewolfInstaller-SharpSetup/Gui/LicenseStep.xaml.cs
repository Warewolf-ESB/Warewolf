using System.IO;
using System.Text;
using System.Windows;
using SharpSetup.UI.Wpf.Forms.Modern;

namespace Gui
{
    /// <summary>
    /// Interaction logic for LicenseStep.xaml
    /// </summary>
    public partial class LicenseStep : ModernActionStep
    {
        public LicenseStep()
        {
            InitializeComponent();
            rtbLicense.Selection.Load(new MemoryStream(ASCIIEncoding.Default.GetBytes(Properties.Resources.LicenseStepRtf)), DataFormats.Rtf);
            Loaded += (o, e) => { cbAccept.Focus(); };
        }
    }
}
