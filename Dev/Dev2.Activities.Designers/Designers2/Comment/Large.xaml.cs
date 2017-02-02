using System.Windows;

namespace Dev2.Activities.Designers2.Comment
{
    /// <summary>
    /// Interaction logic for Large.xaml
    /// </summary>
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
        }

        #region Overrides of ActivityDesignerTemplate

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }

        #endregion
    }
}
