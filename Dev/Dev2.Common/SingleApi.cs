/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Common
{
    public class SingleApi
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        [Newtonsoft.Json.JsonProperty("humanUrl")]
        public string HumanUrl { get; set; }

        [Newtonsoft.Json.JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }

        public string Version { get; set; }
        public List<string> Tags { get; set; }

        [Newtonsoft.Json.JsonProperty("properties")]
        public List<PropertyApi> Properties { get; set; }

        [Newtonsoft.Json.JsonProperty("contact")]
        public List<MaintainerApi> Contact { get; set; }
    }
}