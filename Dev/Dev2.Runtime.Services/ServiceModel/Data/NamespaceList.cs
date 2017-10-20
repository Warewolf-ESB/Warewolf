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
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class NamespaceItem : INamespaceItem
    {
        public string AssemblyLocation { get; set; }
        public string AssemblyName { get; set; }
        public string FullName { get; set; }
        public string MethodName { get; set; }
        public string JsonObject { get; set; }
    }
}
