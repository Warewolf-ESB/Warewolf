using SharpSetup.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for InstallationModeStep.xaml
    /// </summary>
    public partial class InstallationModeStep
    {
        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        public InstallationModeStep(InstallationModeCollection installationModes)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            InitializeComponent();
            imsModes.ItemsSource = installationModes;
            imsModes.Loaded += (s, e) => { imsModes.Focus(); };
            DataContext = new InfoStepDataContext();
        }

        private void InstallationModeStep_MoveNext(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {
            Wizard.LifecycleAction(LifecycleActionType.ModeSelected, imsModes.SelectedItem);
        }
    }
}
