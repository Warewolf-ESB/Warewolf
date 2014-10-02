
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
