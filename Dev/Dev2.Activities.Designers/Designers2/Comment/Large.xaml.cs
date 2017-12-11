using System.Windows;

namespace Dev2.Activities.Designers2.Comment
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
        }

        protected override IInputElement GetInitialFocusElement() => InitialFocusElement;
    }
}