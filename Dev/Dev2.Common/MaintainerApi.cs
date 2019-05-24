#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Common
{
    public class MaintainerApi
    {
        [Newtonsoft.Json.JsonProperty("FN")]
        public string Fn { get; set; }

        public string Email { get; set; }
        public string Url { get; set; }

        [Newtonsoft.Json.JsonProperty("org")]
        public string Org { get; set; }

        public string Adr { get; set; }
        public string Tel { get; set; }

        [Newtonsoft.Json.JsonProperty("X-Twitter")]
        public string XTwitter { get; set; }

        [Newtonsoft.Json.JsonProperty("X-Github")]
        public string XGithub { get; set; }

        [Newtonsoft.Json.JsonProperty("photo")]
        public string Photo { get; set; }

        [Newtonsoft.Json.JsonProperty("vCard")]
        public string VCard { get; set; }
    }
}