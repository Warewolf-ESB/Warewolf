using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Patterns;

namespace Dev2.Common.Interfaces.Scripting
{
    public interface IScriptingContext : ISpookyLoadable<enScriptType>
    {
        string Execute(string scriptValue);
    }
}
