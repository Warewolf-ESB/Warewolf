using System.Collections.Generic;
using System.Text;

namespace Dev2.Common.Interfaces.Core.DynamicServices {

    public interface IDynamicServiceObject {
        string Comment { get; set; }
        string Category { get; set; }
        bool Compile();
        ICollection<string> CompilerErrors { get; set; }
        string HelpLink { get; set; }
        bool IsCompiled { get; }
        string Name { get; set; }
        enDynamicServiceObjectType ObjectType { get; set; }
        StringBuilder ResourceDefinition { get; set; }
        int VersionNo { get; set; }
        void WriteCompileError(string traceMsg);
        void WriteCompileWarning(string traceMsg);
    }
}
