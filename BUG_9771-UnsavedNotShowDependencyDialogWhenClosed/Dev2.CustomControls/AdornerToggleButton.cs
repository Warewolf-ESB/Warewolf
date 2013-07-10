using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Dev2.CustomControls
{
    public class AdornerToggleButton : ToggleButton
    {
        protected AdornerLayer AdornerLayer { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DefaultStyleKey = ToolBar.ToggleButtonStyleKey;
        }

        protected override void OnClick()
        {
            base.OnClick();
            AdornerLayer = AdornerLayer.GetAdornerLayer(this);
        }
    }
}
