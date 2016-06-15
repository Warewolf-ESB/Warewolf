/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Dev2.Integration.Tests.WebTester
{
    class AssemblyReference
    {
        private Assembly _assembly;
        readonly Dictionary<string, ParameterInfo[]> methodParamMap;

        private readonly string path = Path.GetFullPath(@".\plugins\");

        public AssemblyReference()
        {
            methodParamMap = new Dictionary<string, ParameterInfo[]>();
            _assembly = null;
        }
        public void GetAssemblyInfo(string AssemblyName)
        {
            GetAssembly(AssemblyName);
            GetMethods();
        }

        public void ExecuteMethod(string methodName, string[] parameters)
        {
            object obj = _assembly.CreateInstance(methodName, true);
            if(obj != null)
            {
                obj.GetType().GetProperties();
            }
        }

        public string[] GetParamsForMethod(string MethodName)
        {
            //string[] parameters = new string[methodParamMap.Values.Count(param => (methodParamMap[methodParamMap.Keys.Where(method => method.Name == MethodName)]))];

            //Need to query the Dictionary using a LINQ query
            // for the time being
            List<string> param = GetParametersForMethod(MethodName);
            return param.ToArray();
        }

        private List<string> GetParametersForMethod(string MethodName)
        {
            List<ParameterInfo[]> parameters = new List<ParameterInfo[]>();
            List<string> strings = new List<string>();
            parameters.Add(methodParamMap[MethodName]);
            return strings;
        }

        private void GetAssembly(string AssemblyName)
        {
            _assembly = null;
            string FullAssemblyPath = Path.Combine(path + AssemblyName);
            try
            {
                _assembly = Assembly.LoadFile(FullAssemblyPath);
            }
            catch(FileNotFoundException fnfex)
            {
                throw fnfex;
            }
        }

        private void GetMethods()
        {
            Type[] types = _assembly.GetTypes();

            foreach(Type type in types)
            {
                MethodInfo[] _methods = type.GetMethods();
                foreach(MethodInfo method in _methods)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    methodParamMap.Add(method.Name, parameters);
                }
            }
        }
    }
}
