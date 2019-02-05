/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Reflection;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    public class AssemblyWrapper : IAssemblyWrapper
    {
        public Assembly Load(string assemblyString) => Assembly.Load(assemblyString);

        public Assembly LoadFrom(string assemblyString) => Assembly.LoadFrom(assemblyString);

        public Assembly Load(AssemblyName toLoad) => Assembly.Load(toLoad);

        public Assembly UnsafeLoadFrom(string assemblyLocation) => Assembly.UnsafeLoadFrom(assemblyLocation);

        public Assembly GetAssembly(Type getType) => Assembly.GetAssembly(getType);

        public AssemblyName[] GetReferencedAssemblies(Assembly asm) => asm.GetReferencedAssemblies();
    }
}
