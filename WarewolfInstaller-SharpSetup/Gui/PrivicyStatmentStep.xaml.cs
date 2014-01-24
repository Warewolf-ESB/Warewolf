using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace Gui
{
    /// <summary>
    /// Interaction logic for LicenseStep.xaml
    /// </summary>
    public partial class PrivacyStatmentStep
    {
        public PrivacyStatmentStep(int stepNumber, List<string> listOfStepNames)
        {
            InitializeComponent();
            rtbStatement.Selection.Load(new MemoryStream(Encoding.Default.GetBytes(Properties.Resources.WAREWOLF_EULA_EN)), DataFormats.Rtf);
            Loaded += (o, e) => cbAccept.Focus();
            DataContext = new InfoStepDataContext(stepNumber, listOfStepNames);
        }
    }
}
