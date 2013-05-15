using System;
using Dev2.DynamicServices;
using System.Collections.Generic;

namespace Dev2.DynamicServices {
    public interface IDynamicServiceObject {
        enApprovalState ApprovalState { get; set; }
        string AuthorRoles { get; set; }
        string Comment { get; set; }
        string Category { get; set; }
        bool Compile();
        ICollection<string> CompilerErrors { get; set; }
        dynamic GetCompilerErrors();
        string HelpLink { get; set; }
        bool IsCompiled { get; }
        bool IsUserInRole(string userRoles, string resourceRoles);
        string Name { get; set; }
        enDynamicServiceObjectType ObjectType { get; set; }
        string ResourceDefinition { get; set; }
        int VersionNo { get; set; }
        void WriteCompileError(string traceMsg);
        void WriteCompileWarning(string traceMsg);
    }
}
