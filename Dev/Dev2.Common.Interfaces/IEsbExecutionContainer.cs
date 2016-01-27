using System;
using Dev2.Common.Interfaces.Data.TO;

namespace Dev2.Common.Interfaces
{
    public interface IEsbExecutionContainer
    {
        String InstanceOutputDefinition { get; set; }
        String InstanceInputDefinition { get; set; }

        Guid Execute(out IErrorResultTO errors, int update);
    }
}