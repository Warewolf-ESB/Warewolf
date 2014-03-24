using System.IO;
using System.Windows;
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
            if(File.Exists(mainMsiFile))
            {
                MsiConnection.Instance.Open(mainMsiFile, true);
            }
            else
            {
                MsiConnection.Instance.Open(SetupHelper.GetProductGuidFromPath(), true);
            }

            // if we want logging, here it is ;)
            if (!InstallVariables.RemoveLogFile)
            {
                // set log file location ;)
                var installLoc = MsiConnection.Instance.GetProperty("INSTALLLOCATION");
                var logFileName = Path.Combine(installLoc, "Warewolf_Install.log");
                MsiConnection.Instance.LogFile = logFileName;
            }

            Wizard.LifecycleAction(LifecycleActionType.ConnectionOpened);
            Wizard.NextStep();
            DataContext = new InfoStepDataContext();

            // Seems to be  install issues following the route below ;)
            //var mainMsiFile = Properties.Resources.MainMsiFile;
            //if(File.Exists(PublicResources.SerializedStateFile))
            //    MsiConnection.Instance.OpenFromFile(PublicResources.SerializedStateFile);
            //else if(File.Exists(mainMsiFile))
            //MsiConnection.Instance.Open(mainMsiFile, true);
            //else

            //MsiConnection.Instance.Open(SetupHelper.GetProductGuidFromPath(), true);
            //Wizard.LifecycleAction(LifecycleActionType.ConnectionOpened);
            //Wizard.NextStep();
            //DataContext = new InfoStepDataContext();
        }
    }
}
