
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
