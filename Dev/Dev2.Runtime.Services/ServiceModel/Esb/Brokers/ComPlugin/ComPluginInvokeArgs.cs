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
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin
{
    /// <summary>
    /// Args to pass into the plugin ;)
    /// </summary>
    [Serializable]
    public class ComPluginInvokeArgs
    {
        public bool Is32Bit { get; set; }
        public string ClsId { get; set; }
        public string AssemblyName { get; set; }
        public string Fullname { get; set; }
        public string Method { get; set; }
        public List<MethodParameter> Parameters { get; set; }
        public IOutputFormatter OutputFormatter { get; set; }

    }
}
