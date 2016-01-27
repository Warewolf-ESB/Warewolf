using System;
using Dev2.DataList.Contract;
using Warewolf.Storage;

namespace Dev2.Runtime.ESB.Execution
{
    public interface IEsbExecutionContainer
    {
        Guid Execute(out ErrorResultTO errors, int update);

        IExecutionEnvironment Execute(IDSFDataObject inputs,IDev2Activity activity);

        String InstanceOutputDefinition { get; set; }
        String InstanceInputDefinition { get; set; }
    }
}