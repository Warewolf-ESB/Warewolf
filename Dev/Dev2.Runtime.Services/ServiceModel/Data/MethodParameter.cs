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
    /// <summary>
    /// A Method Parameter
    /// </summary>
    [Serializable]
    public class MethodParameter : IMethodParameter
    {
        public string Name { get; set; }
        public bool EmptyToNull { get; set; }
        public bool IsRequired { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public string TypeName
        {
            get;
            set;
        }

        public bool IsObject { get; set; }
        public string Dev2ReturnType { get; set; }
        public string ShortTypeName { get; set; }
    }
}
