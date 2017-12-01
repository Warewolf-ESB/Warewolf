using System.Windows;

namespace Dev2.Activities.Designers2.UniqueRecords
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
