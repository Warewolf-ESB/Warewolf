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
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin
{
    /// <summary>
    /// Args to pass into the plugin ;)
    /// </summary>
    [DataContract]
    public class ComPluginInvokeArgs
    {
        [DataMember]
        public bool Is32Bit { get; set; }
        [DataMember]
        public string ClsId { get; set; }
        [DataMember]
        public string AssemblyName { get; set; }
        [DataMember]
        public string Fullname { get; set; }
        [DataMember]
        public string Method { get; set; }
        [DataMember]
        public List<MethodParameter> Parameters { get; set; }
        [DataMember]
        public IOutputFormatter OutputFormatter { get; set; }

    }
}
