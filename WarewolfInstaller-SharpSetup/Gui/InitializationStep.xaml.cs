using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using SharpSetup.Base;
using SharpSetup.UI.Wpf.Forms.Modern;

namespace Gui
{
    /// <summary>
    /// Interaction logic for InitializationStep.xaml
    /// </summary>
    public partial class InitializationStep : ModernInfoStep
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
