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
        Type _privateType;

        public PrivateObject(object privateObject)
        {
            _privateObject = privateObject;
            _privateType = null;
        }

        public PrivateObject(Type privateType)
        {
            _privateObject = null;
            _privateType = privateType;
        }
        
        public object Invoke(string memberName) => Invoke(memberName, false, new object[]{});
        public object Invoke(string memberName, params object[] inputParameters) => Invoke(memberName, false, inputParameters);
        public object InvokeStatic(string memberName, params object[] inputParameters) => Invoke(memberName, true, inputParameters);
        public object Invoke(string memberName, bool isStaticMember) => Invoke(memberName, isStaticMember, new object[]{});
        public object Invoke(string memberName, bool isStaticMember, params object[] inputParameters)
        {
            var getType = _privateObject == null ? _privateType : _privateObject.GetType();
            while (getType.GetMethod(memberName, (isStaticMember ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic) == null && getType.BaseType != null)
            {
                getType = getType.BaseType;
            }
            var getMethod = getType.GetMethod(memberName, (isStaticMember ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
            return getMethod?.Invoke(_privateObject, inputParameters);
        }
        
        public object GetFieldOrProperty(string fieldName) => GetFieldOrProperty(fieldName, false);
        public object GetFieldOrProperty(string fieldName, bool isStaticField)
        {
            var getType = _privateObject == null ? _privateType : _privateObject.GetType();
            while (getType.GetField(fieldName, (isStaticField ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic) == null && getType.BaseType != null)
            {
                getType = getType.BaseType;
            }
            var getField = getType.GetField(fieldName, (isStaticField ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
            return getField?.GetValue(_privateObject);
        }
        
        public object GetField(string fieldName) => GetField(fieldName, false);
        public object GetStaticField(string fieldName) => GetField(fieldName, true);
        public object GetField(string fieldName, bool isStaticField) => GetFieldOrProperty(fieldName, isStaticField);
        
        public void SetField(string fieldName, object setValue) => SetField(fieldName, setValue, false);
        public void SetField(string fieldName, object setValue, bool isStaticField)
        {
            var getType = _privateObject == null ? _privateType : _privateObject.GetType();
            while (getType.GetField(fieldName, (isStaticField ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic) == null && getType.BaseType != null)
            {
                getType = getType.BaseType;
            }
            var getField = getType.GetField(fieldName, (isStaticField ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
            getField?.SetValue(_privateObject, setValue);
        }
        
        public void SetFieldOrProperty(string fieldName, object setValue) => SetFieldOrProperty(fieldName, setValue, false);
        public void SetFieldOrProperty(string fieldName, object setValue, bool isStaticField) => SetField(fieldName, setValue, isStaticField);
        
        public object GetProperty(string propertyName) => GetProperty(propertyName, false);
        public object GetProperty(string propertyName, bool isStaticProperty)
        {
            var getType = _privateObject == null ? _privateType : _privateObject.GetType();
            while (getType.GetProperty(propertyName, (isStaticProperty ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic) == null && getType.BaseType != null)
            {
                getType = getType.BaseType;
            }
            var getProperty = getType.GetProperty(propertyName, (isStaticProperty ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
            return getProperty?.GetValue(_privateObject);
        }
        
        public void SetProperty(string propertyName, object setValue) => SetProperty(propertyName, setValue, false);
        public void SetProperty(string propertyName, object setValue, bool isStaticProperty)
        {
            var getType = _privateObject == null ? _privateType : _privateObject.GetType();
            while (getType.GetProperty(propertyName, (isStaticProperty ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic) == null && getType.BaseType != null)
            {
                getType = getType.BaseType;
            }
            var getProperty = getType.GetProperty(propertyName, (isStaticProperty ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
            getProperty?.SetValue(_privateObject, setValue);
        }
    }
}