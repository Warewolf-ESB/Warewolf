using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Warewolf.Storage;

namespace Dev2.Runtime.ESB.Control
{
    public interface IEnvironmentOutputMappingManager
    {
        IExecutionEnvironment UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors);
    }
}