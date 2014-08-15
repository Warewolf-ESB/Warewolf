using System;
using System.Globalization;
using System.IO;
using System.Linq;
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

        // ReSharper disable InconsistentNaming
        private void InitializationStep_Entered(object sender, RoutedEventArgs e)
        // ReSharper restore InconsistentNaming
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
            //if(!InstallVariables.RemoveLogFile)
            //{
            // set log file location ;)
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDir = Path.Combine(appData, "Warewolf");

            // create directory to avoid issues ;)
            if(!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            var logFileName = Path.Combine(logDir, "Warewolf_Install.log");

            MsiConnection.Instance.LogFile = logFileName;
            //}

            Wizard.LifecycleAction(LifecycleActionType.ConnectionOpened);
            try
            {
                ContainsUnicodeCharacter(Environment.MachineName.ToLower(CultureInfo.InvariantCulture));
                Wizard.NextStep();
                DataContext = new InfoStepDataContext();
            }
            catch(Exception exception)
            {
                CanGoNext = false;
                MessageBox.Show("Installation cannot continue due to the following:" +
                    Environment.NewLine +
                    exception.Message);
                Wizard.Finish();
            }
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

        public void ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 128;

            var containsUnicodeCharacter = input.Any(c => c > MaxAnsiCode);
            if(containsUnicodeCharacter)
            {
                throw new InvalidDataException(string.Format("The Machine Name: {0} " +
                                                             "contains invalid characters. " +
                                                             "Warewolf only supports latin based character set.", input));
            }
        }
    }
}
