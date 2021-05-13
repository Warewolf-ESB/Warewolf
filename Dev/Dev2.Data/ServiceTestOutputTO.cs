/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Common.Interfaces;
using Newtonsoft.Json;

namespace Dev2.Data
{
    public class ServiceTestOutputTO : IServiceTestOutput
    {
        public string Variable { get; set; }
        public string Value { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        [DefaultValue("=")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string AssertOp { get; set; }
        public bool HasOptionsForValue { get; set; }
        public List<string> OptionsForValue { get; set; }
        public TestRunResult Result { get; set; }
        public void OnSearchTypeChanged() { }
    }
}