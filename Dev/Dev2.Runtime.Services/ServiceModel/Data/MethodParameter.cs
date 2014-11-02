/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;

namespace Dev2.Runtime.ServiceModel.Data
{
    /// <summary>
    ///     A Method Parameter
    /// </summary>
    [Serializable]
    public class MethodParameter
    {
        public string Name { get; set; }
        public bool EmptyToNull { get; set; }
        public bool IsRequired { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public Type Type { get; set; }

        public string TypeName
        {
            get { return Type != null ? Type.FullName : ""; }
        }
    }
}