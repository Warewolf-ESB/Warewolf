using System.ComponentModel;

namespace Dev2.Common.Interfaces.Infrastructure.SharedModels
{
    /// <summary>
    /// Compile time message types
    /// </summary>
    public enum CompileMessageType
    {
        [Description("Mapping out of date")]
        MappingChange,

        [Description("Resource has been deleted")]
        ResourceDeleted,

        [Description("Resource has been saved")]
        ResourceSaved,

        [Description("IsRequired mapping has changed")]
        MappingIsRequiredChanged
    }
}
