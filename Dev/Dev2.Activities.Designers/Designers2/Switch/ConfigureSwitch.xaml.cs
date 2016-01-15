using System;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Activities.Designers2.Switch
{
    /// <summary>
    /// Interaction logic for ConfigureSwitch.xaml
    /// </summary>
    public partial class ConfigureSwitch : IView
    {
        public ConfigureSwitch()
        {
            InitializeComponent();
        }

        public void CheckSwitchVariableState(string state)
        {
            if(VariabletoSwitchon.IsEnabled != (state.ToLower()=="enabled"))
            {
                throw new Exception("State is not"+state);
            }

        }

        public void CheckDisplayState(string state)
        {
            if (DisplayText.IsEnabled != (state.ToLower() == "enabled"))
            {
                throw new Exception("State is not" + state);
            }

        }

        public void SetVariableToSwitchOn(string state)
        {
            VariabletoSwitchon.Text = state;

        }

        public string GetDisplayName()
        {
            return DisplayText.Text;
        }
    }
}
