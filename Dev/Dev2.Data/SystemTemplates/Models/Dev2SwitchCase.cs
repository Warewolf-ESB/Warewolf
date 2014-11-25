
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.DataList.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Data.SystemTemplates.Models
{
    /// <summary>
    /// A model item for the Switch Case on the workflow designer
    /// </summary>
    public class Dev2SwitchCase : IDev2DataModel
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [JsonIgnore]
        public string Version
        {
            get { return "1.0.0"; }
        }

        /// <summary>
        /// Gets the name of the model.
        /// </summary>
        /// <value>
        /// The name of the model.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public Dev2ModelType ModelName
        {
            get { return Dev2ModelType.Dev2SwitchCase; }
        }

        /// <summary>
        /// Gets or sets the case value.
        /// </summary>
        /// <value>
        /// The case value.
        /// </value>
        public string CaseValue { get; set; }

        public string ToWebModel()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string GenerateUserFriendlyModel(Guid dlid, Dev2DecisionMode mode, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return "For " + CaseValue;
        }
    }
}
