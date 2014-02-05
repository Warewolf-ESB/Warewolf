using System.Collections.Generic;
using System.Windows;

namespace Gui
{
    /// <summary>
    /// Interaction logic for FinishStep.xaml
    /// </summary>
    public partial class FinishStep
    {
        public FinishStep(int stepNumber, List<string> listOfStepNames)
        {
            InitializeComponent();

            // swap text at end if not install mode ;)
            if(!InstallVariables.IsInstallMode)
            {
                cbStartStudio.Visibility = Visibility.Hidden;
                tbFinish.Visibility = Visibility.Visible;
            }
            else
            {
                // install mode ;)
                cbStartStudio.Visibility = Visibility.Visible;
                tbFinish.Visibility = Visibility.Hidden;

                // Force a studio start ;)
                InstallVariables.StartStudioOnExit = true;
                InstallVariables.ViewReadMe = true;
            }

            DataContext = new InfoStepDataContext(stepNumber, listOfStepNames);
        }

        void BtnExitWithStart(object sender, RoutedEventArgs e)
        {
            // swap the start flag ;)
            InstallVariables.StartStudioOnExit = !(InstallVariables.StartStudioOnExit);
        }

        void BtnExitWithReadme(object sender, RoutedEventArgs e)
        {
            // swap the read-me flag ;)
            InstallVariables.ViewReadMe = !(InstallVariables.ViewReadMe);
        }
    }
}
