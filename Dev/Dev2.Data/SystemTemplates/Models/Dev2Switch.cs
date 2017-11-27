/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.TO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Warewolf.Storage.Interfaces;

namespace Dev2.Data.SystemTemplates.Models
{
    public class Dev2Switch : IDev2DataModel, IDev2FlowModel
    {
        [JsonIgnore]
        public string Version => "1.0.0";
        [JsonConverter(typeof(StringEnumConverter))]
        public Dev2ModelType ModelName => Dev2ModelType.Dev2Switch;
        public string SwitchVariable { get; set; }
        public string SwitchExpression { get; set; }
        public string ToWebModel() => JsonConvert.SerializeObject(this);

        public string GenerateUserFriendlyModel(IExecutionEnvironment env, Dev2DecisionMode mode, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return "on " + SwitchVariable;
        }

        #region Implementation of IDev2FlowModel

        public string DisplayText { get; set; }

        #endregion
    }
}
