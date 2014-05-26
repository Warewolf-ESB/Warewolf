using System.Collections.Generic;
using SharpSetup.Base;
using SharpSetup.UI.Wpf.Base;
using SharpSetup.UI.Wpf.Forms.Modern;

namespace Gui
{
    /// <summary>
    /// Interaction logic for WelcomeStep.xaml
    /// </summary>
// ReSharper disable RedundantExtendsListEntry
    public partial class WelcomeStep : ModernInfoStep
// ReSharper restore RedundantExtendsListEntry
    {

        readonly InstallationMode _mode;
        public WelcomeStep(InstallationMode mode, int stepNumber, List<string> listOfStepNames)
        {
            _mode = mode;
            InitializeComponent();
            //lblMode.Text = Properties.Resources.ResourceManager.GetString("WelcomeStepGreeting" + mode) ?? lblMode.Text;
            DataContext = new InfoStepDataContext(stepNumber, listOfStepNames);
        }

// ReSharper disable InconsistentNaming
        private void WelcomeStep_MoveNext(object sender, ChangeStepRoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            Wizard.LifecycleAction(LifecycleActionType.ModeSelected, _mode);
        }
    }
}
