using System.Windows;

namespace Dev2.Activities.Designers2.Zip
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
        }

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }
    }
}