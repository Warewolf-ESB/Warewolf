﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Data.SystemTemplates.Models
{
    /// <summary>
    /// A model for the Switch on the workflow designer
    /// </summary>
    public class Dev2Switch : IDev2DataModel
    {

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [JsonIgnore]
        public string Version { get { return "1.0.0"; } }

        /// <summary>
        /// Gets the name of the model.
        /// </summary>
        /// <value>
        /// The name of the model.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public Dev2ModelType ModelName { get { return Dev2ModelType.Dev2Switch; } }


        /// <summary>
        /// Gets or sets the switch variable.
        /// </summary>
        /// <value>
        /// The switch variable.
        /// </value>
        public string SwitchVariable { get; set; }

        /// <summary>
        /// To the web model.
        /// </summary>
        /// <returns></returns>
        public string ToWebModel()
        {
            string result = JsonConvert.SerializeObject(this);

            return result;
        }
        
        public string GenerateUserFriendlyModel()
        {
            return "on " + SwitchVariable;
        }
    }
}
