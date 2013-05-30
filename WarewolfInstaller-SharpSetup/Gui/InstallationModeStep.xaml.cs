using SharpSetup.Base;
using SharpSetup.UI.Wpf.Forms.Modern;

namespace Gui
{
    /// <summary>
    /// Interaction logic for InstallationModeStep.xaml
    /// </summary>
    public partial class InstallationModeStep : ModernInfoStep
    {
        public InstallationModeStep(InstallationModeCollection installationModes)
        {
            InitializeComponent();
            imsModes.ItemsSource = installationModes;
            imsModes.Loaded += (s, e) => { imsModes.Focus(); };
        }

        private void InstallationModeStep_MoveNext(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {
            Wizard.LifecycleAction(LifecycleActionType.ModeSelected, imsModes.SelectedItem);
        }
    }
}
