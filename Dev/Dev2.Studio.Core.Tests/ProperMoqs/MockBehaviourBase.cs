
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
using System.Reflection;
using Dev2.Core.Tests.Enums;

namespace Dev2.Core.Tests.ProperMoqs {
    internal abstract class MockBehaviourBase {

        #region Locals

        private enTestObjectBehaviourChangeType _behaviourType;

        #endregion Locals

        #region Some Freaky Property and Method Changing

        public object this[string name] {
            set {
                Type myType = GetType();
                if(_behaviourType == enTestObjectBehaviourChangeType.Property) {
                    PropertyInfo propertyInfo = myType.GetProperty(name);
                    propertyInfo.SetValue(this, value, null);
                }
                else {
                    MethodInfo methodInfo = myType.GetMethod(name);
                    var methodBody = methodInfo.GetMethodBody();
                    if(methodBody != null)
                    {
                        byte[] MethodBodyAsByteArray = methodBody.GetILAsByteArray();
                    }
                }
            }
        }

        #endregion Some Freaky Property and Method Changing

        internal virtual void ChangeReturnValue(string Name, enTestObjectBehaviourChangeType behaviourType, object ReturnValue) {
            _behaviourType = behaviourType;
            this[Name] = ReturnValue;
        }
    }
}
