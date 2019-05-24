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

using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common
{
    [ExcludeFromCodeCoverage]
    public class LogEntry
    {
        [JsonProperty("StartDateTime")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime StartDateTime { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("Url")]
        public string Url { get; set; }
        [JsonProperty("Result")]
        public string Result { get; set; }
        [JsonProperty("User")]
        public string User { get; set; }
        [JsonProperty("CompletedDateTime")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime CompletedDateTime { get; set; }
        [JsonProperty("ExecutionTime")]
        public string ExecutionTime { get; set; }
        [JsonProperty("ExecutionId")]
        public string ExecutionId { get; set; }
    }
}