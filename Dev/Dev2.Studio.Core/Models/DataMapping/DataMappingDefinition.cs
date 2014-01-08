using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models
{
    public class DataMappingDefinition : IDataMappingOutputs
    {
        public DataMappingDefinition(string name, string mapsTo)
        {
            Name = name;
            MapsTo = mapsTo;
        }

        public string Name { get; set; }

        public string MapsTo { get; set; }
    }
}
