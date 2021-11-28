// 
// /*
// *  Warewolf - Once bitten, there's no going back
// *  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
// *  Licensed under GNU Affero General Public License 3.0 or later.
// *  Some rights reserved.
// *  Visit our website for more information <http://warewolf.io/>
// *  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
// *  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
// */

using System;
using System.Reflection;

namespace Warewolf.Testing
{
    public class PrivateType
    {
        Type _privateType;

        public PrivateType(Type privateType)
        {
            _privateType = privateType ?? throw new ArgumentNullException(nameof(privateType));
        }
        
        public object Invoke(string memberName, bool isStaticMember, params object[] inputParameters)
        {
            // ReSharper disable PossibleNullReferenceException
            while (_privateType.GetMethod(memberName, (isStaticMember ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic) == null && _privateType.BaseType != null)
            // ReSharper restore PossibleNullReferenceException
            {
                _privateType = _privateType.BaseType;
            }
            var getMethod = _privateType.GetMethod(memberName, (isStaticMember ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
            return getMethod?.Invoke(_privateType, inputParameters);
        }
    }
}