using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class RecordsetField
    {
        public string Name { get; set; }
        public string Alias { get; set; }


        /// <summary>
        /// This property exists because this classes fields couldn't describe all the data in an IPath.
        /// Which meant when this class came back from the website IPaths couldn't be reconstructed.
        /// </summary>
        /// <value>
        /// The IPath which this field represents.
        /// </value>
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public IPath Path { get; set; }
    }
}