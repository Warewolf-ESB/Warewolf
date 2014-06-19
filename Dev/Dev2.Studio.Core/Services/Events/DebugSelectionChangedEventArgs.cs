using Dev2.Diagnostics.Debug;

namespace Dev2.Services.Events
{
    public class DebugSelectionChangedEventArgs
    {
        public IDebugState DebugState { get; set; }
        public ActivitySelectionType SelectionType { get; set; }
    }

    public enum ActivitySelectionType
    {
        None,
        Single,
        Add,
        Remove
    }
}
