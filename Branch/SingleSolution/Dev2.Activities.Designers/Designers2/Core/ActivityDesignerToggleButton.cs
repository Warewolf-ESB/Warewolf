using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dev2.Activities.Designers2.Core
{
    public class ActivityDesignerToggleButton : ToggleButton
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DefaultStyleKey = ToolBar.ToggleButtonStyleKey;
        }
    }
}
