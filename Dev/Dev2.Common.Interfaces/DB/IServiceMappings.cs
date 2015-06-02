using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DB
{
    public interface IServiceMappings
    {
        string MappingsHeader { get; }
        ICollection<IServiceInput> Inputs { get; }
        IList<IServiceOutputMapping> OutputMapping { get; set; }
        string RecordsetName { get; set; }
        bool IsInputsEmptyRows { get; set; }
        bool IsOutputMappingEmptyRows { get; set; }
    }
}