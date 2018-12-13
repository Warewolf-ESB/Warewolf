using System;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;

namespace Dev2.Runtime
{
    public interface IEsbExecutionContainer
    {
        Guid Execute(out ErrorResultTO errors, int update);

        IDSFDataObject Execute(IDSFDataObject inputs,IDev2Activity activity);

        String InstanceOutputDefinition { get; set; }
        String InstanceInputDefinition { get; set; }
        IDSFDataObject GetDataObject();
    }

    public interface IResumableExecutionContainer : IEsbExecutionContainer { }
    public interface IResumableExecutionContainerFactory
    {
        IResumableExecutionContainer New(Guid startActivityId, ServiceAction sa, DsfDataObject dataObject);
    }
}