/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class AssemblyWrapperTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AssemblyWrapper))]
        public void AssemblyWrapper_Load_Using_String()
        {
            var assemblyLoader = new AssemblyWrapper();
            var assembly = assemblyLoader.GetAssembly(typeof(AssemblyWrapper));
            var load = assemblyLoader.Load(assembly.GetName().ToString());

            Assert.AreEqual("Dev2.Common.dll", load.ManifestModule.Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AssemblyWrapper))]
        public void AssemblyWrapper_LoadFrom_Using_String()
        {
            var assemblyLoader = new AssemblyWrapper();
            var assembly = assemblyLoader.GetAssembly(typeof(AssemblyWrapper));
            var loadFrom = assemblyLoader.LoadFrom(assembly.Location);

            Assert.AreEqual("Dev2.Common.dll", loadFrom.ManifestModule.Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AssemblyWrapper))]
        public void AssemblyWrapper_Load_Using_Assembly()
        {
            var assemblyLoader = new AssemblyWrapper();
            var assembly = assemblyLoader.GetAssembly(typeof(AssemblyWrapper));
            var load = assemblyLoader.Load(assembly.GetName());

            Assert.AreEqual("Dev2.Common.dll", load.ManifestModule.Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AssemblyWrapper))]
        public void AssemblyWrapper_UnsafeLoadFrom_Using_String()
        {
            var assemblyLoader = new AssemblyWrapper();
            var assembly = assemblyLoader.GetAssembly(typeof(AssemblyWrapper));
            var unsafeLoadFrom = assemblyLoader.UnsafeLoadFrom(assembly.Location);

            Assert.AreEqual("Dev2.Common.dll", unsafeLoadFrom.ManifestModule.Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AssemblyWrapper))]
        public void AssemblyWrapper_GetReferencedAssemblies_Using_Assembly()
        {
            var assemblyLoader = new AssemblyWrapper();
            var assembly = assemblyLoader.GetAssembly(typeof(AssemblyWrapper));
            var referencedAssembliesload = assemblyLoader.GetReferencedAssemblies(assembly);

            Assert.IsNotNull(referencedAssembliesload);
        }
    }
}
