using Dev2.Common.Enums;

namespace Dev2.Common
{
    public interface IScriptingContext : ISpookyLoadable<enScriptType>
    {
        string Execute(string scriptValue);
    }
}
