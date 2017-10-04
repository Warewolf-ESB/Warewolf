
using System.Windows;
using System.Windows.Input;
using Dev2.UI;

namespace Dev2.Activities.Designers2.Switch
{
    /// <summary>
    /// Interaction logic for ConfigureSwitchArm.xaml
    /// </summary>
    public partial class ConfigureSwitchArm
    {
        public ConfigureSwitchArm()
        {
            InitializeComponent();
        }

        private void SwitchArmCaseTextbox_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is IntellisenseTextBox textBox)
            {
                Keyboard.Focus(textBox.TextBox);
                textBox.TextBox.SelectAll();
            }
        }
    }
}
