using Dev2.Data.TO;
using Dev2.Interfaces;
using Warewolf.Storage;

namespace Dev2.Runtime.ESB.Control
{
    public interface IEnvironmentOutputMappingManager
    {
        IExecutionEnvironment UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors);
    }
}