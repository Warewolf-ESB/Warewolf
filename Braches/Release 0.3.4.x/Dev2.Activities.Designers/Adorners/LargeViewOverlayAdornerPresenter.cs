using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Activities.Adorners
{
    public sealed class LargeViewOverlayAdornerPresenter : OverlayAdornerPresenter
    {
        public LargeViewOverlayAdornerPresenter()
        {
            ImageSourceUri =
                "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceExpandMapping-32.png";
            ExpandedImageSourceUri =
                "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceCollapseMapping-32.png";
            OverlayType = OverlayType.LargeView;
            ToolTip = "Open Large View";
            ExpandedToolTip = "Close Large View";
        }
    }
}
