using System.Windows;

namespace Dev2.Activities.Designers2.Core.QuickVariableInput
{
    public partial class QuickVariableInputView
    {
        public QuickVariableInputView()
        {
            InitializeComponent();
        }

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }
    }
}
