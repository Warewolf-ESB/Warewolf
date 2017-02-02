namespace Dev2.Studio.Core
{
    public class DebugStringTreeViewItemViewModel : DebugTreeViewItemViewModel<string>
    {
        public DebugStringTreeViewItemViewModel()
        {
            IsExpanded = false;
        }

        protected override void Initialize(string content)
        {
        }
    }
}