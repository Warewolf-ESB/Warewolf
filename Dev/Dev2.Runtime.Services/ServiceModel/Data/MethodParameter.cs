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
using System.Runtime.Serialization;
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    /// <summary>
    /// A Method Parameter
    /// </summary>
    [DataContract]
    public class MethodParameter : IMethodParameter
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool EmptyToNull { get; set; }

        [DataMember]
        public bool IsRequired { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string DefaultValue { get; set; }

        [DataMember]
        public string TypeName
        {
            get;
            set;
        }

        [DataMember]
        public bool IsObject { get; set; }

        [DataMember]
        public string Dev2ReturnType { get; set; }

        [DataMember]
        public string ShortTypeName { get; set; }
    }
}
