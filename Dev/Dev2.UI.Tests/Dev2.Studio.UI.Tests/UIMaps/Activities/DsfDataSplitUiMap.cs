using Dev2.Studio.UI.Tests.Enums;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfDataSplitUiMap : ToolsUiMapBase
    {
        public DsfDataSplitUiMap(bool createNewtab = true, bool dragOnTab = true)
            : base(createNewtab, 1500)
        {
            if(dragOnTab)
            {
                DragToolOntoDesigner(ToolType.DataSplit);
            }
        }

    }
}
