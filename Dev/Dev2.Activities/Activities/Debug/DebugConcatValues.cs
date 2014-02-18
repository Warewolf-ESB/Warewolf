
namespace Dev2.Activities.Debug
{
    public class DebugConcatValues
    {
        readonly DebugOutputBase[] _debugOutputBase;

        public DebugConcatValues(params DebugOutputBase[] debugOutputBase)
        {
            _debugOutputBase = debugOutputBase;
        }
    }
}
