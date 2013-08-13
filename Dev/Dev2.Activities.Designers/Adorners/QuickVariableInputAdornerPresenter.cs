using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Studio.Views.UserInterfaceBuilder;

namespace Dev2.Activities.Adorners
{
    public sealed class QuickVariableInputAdornerPresenter : OverlayAdornerPresenter
    {
        public QuickVariableInputAdornerPresenter()
        {
            ImageSourceUri =
                "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceQuickVariableInput-32.png";
            OverlayType = OverlayType.QuickVariableInput;
            ToolTip = "Open Quick Variable Input";
            ExpandedToolTip = "Close Quick Variable Input";
            Content = new DataGridQuickVariableInputView();
        }
    }
}
