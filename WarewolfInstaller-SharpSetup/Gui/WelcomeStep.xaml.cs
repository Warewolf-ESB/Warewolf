using System.Collections.Generic;
using System.Security.Principal;
using System.Windows;
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
            CheckForElevatedPriveledges();

            //lblMode.Text = Properties.Resources.ResourceManager.GetString("WelcomeStepGreeting" + mode) ?? lblMode.Text;
            DataContext = new InfoStepDataContext(stepNumber, listOfStepNames);
        }

        // ReSharper disable InconsistentNaming
        private void WelcomeStep_MoveNext(object sender, ChangeStepRoutedEventArgs e)
        // ReSharper restore InconsistentNaming
        {
            Wizard.LifecycleAction(LifecycleActionType.ModeSelected, _mode);
        }

        void CheckForElevatedPriveledges()
        {
            if(!IsElevated())
            {
                CanGoNext = false;
                MessageBox.Show("You do not have sufficient access privileges to install Warewolf. In order to install warewolf, you require administrator priveledges.  Please contact your system administrator.");
            }
        }

        static bool IsElevated()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            if(currentIdentity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(currentIdentity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }
    }
}
