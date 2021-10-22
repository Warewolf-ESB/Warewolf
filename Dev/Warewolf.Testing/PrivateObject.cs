/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Reflection;

namespace Warewolf.Testing
{
    public class PrivateObject
    {
        Object _privateObject;

        public PrivateObject(object privateObject)
        {
            _privateObject = privateObject;
        }
        
        public object Invoke(string memberName, params object[] inputParameters)
        {
            var getType = _privateObject.GetType();
            var getMethod = getType.GetMethod(memberName, BindingFlags.Instance | BindingFlags.NonPublic);
            return getMethod?.Invoke(memberName, inputParameters);
        }
        
        public object GetFieldOrProperty(string fieldName) => GetFieldOrProperty(fieldName, false);
        public object GetFieldOrProperty(string fieldName, bool isStaticField)
        {
            var getType = _privateObject.GetType();
            var getField = getType.GetField(fieldName, (isStaticField ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
            return getField?.GetValue(_privateObject);
        }
        
        public void SetField(string fieldName, object setValue) => SetField(fieldName, setValue, false);
        public void SetField(string fieldName, object setValue, bool isStaticField)
        {
            var getType = _privateObject.GetType();
            var getField = getType.GetField(fieldName, (isStaticField ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
            getField?.SetValue(_privateObject, setValue);
        }
        
        public object GetProperty(string propertyName)
        {
            var getType = _privateObject.GetType();
            var getProperty = getType.GetProperty(propertyName);
            return getProperty?.GetValue(_privateObject);
        }
        
        public void SetProperty(string propertyName, object setValue)
        {
            var getType = _privateObject.GetType();
            var getProperty = getType.GetProperty(propertyName);
            getProperty?.SetValue(_privateObject, setValue);
        }
    }
}