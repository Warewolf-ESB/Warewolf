using System.Windows;
using System.IO;
using SharpSetup.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for InitializationStep.xaml
    /// </summary>
    public partial class InitializationStep
    {
        public InitializationStep()
        {
            InitializeComponent();
        }

        private void InitializationStep_Entered(object sender, RoutedEventArgs e)
        {
            var mainMsiFile = Properties.Resources.MainMsiFile;
            if (File.Exists(PublicResources.SerializedStateFile))
                MsiConnection.Instance.OpenFromFile(PublicResources.SerializedStateFile);
            else if (File.Exists(mainMsiFile))
                MsiConnection.Instance.Open(mainMsiFile, true);
            else
                MsiConnection.Instance.Open(SetupHelper.GetProductGuidFromPath(), true);
            Wizard.LifecycleAction(LifecycleActionType.ConnectionOpened);
            Wizard.NextStep();
        }
    }
}
