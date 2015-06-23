namespace Warewolf.Studio.AntiCorruptionLayer.ViewModels
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