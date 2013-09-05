using System.Windows;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers;

namespace Dev2.Activities.QuickVariableInput
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

            Content = new QuickVariableInputView();            
        }

        public QuickVariableInputView QuickVariableInputView
        {
            get
            {
                return (QuickVariableInputView)Content;
            }
        }
    }
}
