using System;
using Dev2.DataList.Contract;

namespace Dev2.Runtime.ESB.Execution
{
    public interface IEsbExecutionContainer
    {
        Guid Execute(out ErrorResultTO errors, int update);

        IDSFDataObject Execute(IDSFDataObject inputs,IDev2Activity activity);

        String InstanceOutputDefinition { get; set; }
        String InstanceInputDefinition { get; set; }
        IDSFDataObject GetDataObject();
    }
}