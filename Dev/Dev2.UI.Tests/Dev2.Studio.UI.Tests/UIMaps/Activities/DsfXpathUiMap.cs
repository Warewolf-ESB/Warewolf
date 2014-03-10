using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfXpathUiMap : ToolsUiMapBase
    {
        public DsfXpathUiMap(bool createNewtab = true, bool dragFindRecordsOntoNewTab = true)
            : base(createNewtab, 1500)
        {
            if(dragFindRecordsOntoNewTab)
            {
                DragToolOntoDesigner(ToolType.XPath);
            }
        }

        public void EnterTextIntoFieldsToSearch(string stringToEnter, ViewType viewType)
        {
            GetFieldsToSearchTextBoxControl(viewType).EnterText(stringToEnter);
        }

        public string GetTextFromFieldsToSearch(ViewType viewType)
        {
            return GetFieldsToSearchTextBoxControl(viewType).GetText();
        }

        public UITestControl GetFieldsToSearchTextBoxControl(ViewType viewType)
        {
            return GetControl("UI__SourceStringtxt_AutoID", GetView(viewType), ControlType.Edit); 
        }
    }
}
