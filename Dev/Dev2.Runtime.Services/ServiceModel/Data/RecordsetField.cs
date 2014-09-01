using Dev2.Common.Interfaces.Core.Graph;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class RecordsetField
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string RecordsetAlias { get; set; }

        /// <summary>
        /// This property exists so that when an instance comes back from the website the IPaths can be reconstructed.
        /// </summary>
        /// <value>
        /// The IPath which this field represents.
        /// </value>
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public IPath Path { get; set; }
    }
}