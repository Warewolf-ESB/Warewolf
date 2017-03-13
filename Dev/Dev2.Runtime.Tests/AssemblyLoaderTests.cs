using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestingDotnetDllCascading;

namespace Dev2.Tests.Runtime
{
    [TestClass]
    public class AssemblyLoaderTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_InitializesDefaults()
        {
            //---------------Set up test pack-------------------
            var assemblyLoader = new AssemblyLoader();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(assemblyLoader);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_Dependency_InitializesDefaults()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(assemblyLoader);
        }
        const string DirtyGacName = "GAC:Microsoft.SqlServer.GridControl, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91, processorArchitecture=x86";
        const string CleanGacName = "Microsoft.SqlServer.GridControl, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssemblyGac_GivenReturnsNull_ShouldNotLoadAssembly()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            mock.Setup(wrapper => wrapper.Load(It.IsAny<string>()));
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(DirtyGacName, "", out assembly);
            //---------------Test Result -----------------------
            Assert.IsFalse(tryLoadAssembly);
            Assert.IsNull(assembly);
            mock.Verify(wrapper => wrapper.Load(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssemblyGac_GivenDirtyPath_ShouldUseCleanPath()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            mock.Setup(wrapper => wrapper.Load(CleanGacName))
                .Callback<string>((a) =>
                    {
                        var load = Assembly.Load(a);
                        Assert.IsNotNull(load);
                    });
            mock.Setup(wrapper => wrapper.Load("")).Throws(new Exception());
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(DirtyGacName, "", out assembly);
            //---------------Test Result -----------------------
            Assert.IsFalse(tryLoadAssembly);
            Assert.IsNull(assembly);
            mock.Verify(wrapper => wrapper.Load(CleanGacName));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssemblyGac_GivenDirtyPath_ShouldCleanPathAndLoadAssembly()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            var currentDirectory = Directory.GetCurrentDirectory();
            var location = Path.Combine(currentDirectory, "Assembly", "Microsoft.SqlServer.GridControl.dll");
            var load = Assembly.LoadFrom(location);
            mock.Setup(wrapper => wrapper.Load(CleanGacName))
               .Returns(load);
            mock.Setup(wrapper => wrapper.Load("")).Throws(new Exception());
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(DirtyGacName, "", out assembly);
            //---------------Test Result -----------------------
            Assert.IsTrue(tryLoadAssembly);
            Assert.IsNotNull(assembly);
            mock.Verify(wrapper => wrapper.Load(CleanGacName));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssemblyGac_GivenDirtyPath_ShouldLoadReferencedAssemblies()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            var currentDirectory = Directory.GetCurrentDirectory();
            var location = Path.Combine(currentDirectory, "Assembly", "Microsoft.SqlServer.GridControl.dll");
            var load = Assembly.LoadFrom(location);
            var a1 = load.GetReferencedAssemblies().Single(name => name.FullName == "System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var a2 = load.GetReferencedAssemblies().Single(name => name.FullName == "System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var a3 = load.GetReferencedAssemblies().Single(name => name.FullName == "System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var a4 = load.GetReferencedAssemblies().Single(name => name.FullName == "Accessibility, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var a5 = load.GetReferencedAssemblies().Single(name => name.FullName == "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            AssemblyName[] assemblyNames = { a1, a2, a3, a4, a5 };
            mock.Setup(wrapper => wrapper.GetReferencedAssemblies(load)).Returns(assemblyNames);
            mock.Setup(wrapper => wrapper.Load(CleanGacName))
               .Returns(load);
            mock.Setup(wrapper => wrapper.Load("")).Throws(new Exception());
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(DirtyGacName, "", out assembly);
            //---------------Test Result -----------------------
            Assert.IsTrue(tryLoadAssembly);
            Assert.IsNotNull(assembly);
            mock.Verify(wrapper => wrapper.Load("Microsoft.SqlServer.GridControl, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"), Times.Exactly(1));
            mock.Verify(wrapper => wrapper.Load(It.IsAny<AssemblyName>()), Times.Exactly(5));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssemblyGac_GivenLoadsCorreclty_ShouldAddAssembliesTo_loadedAssemblies()
        {
            //---------------Set up test pack-------------------
            var currentDirectory = Directory.GetCurrentDirectory();
            var location = Path.Combine(currentDirectory, "Assembly", "Microsoft.SqlServer.GridControl.dll");
            var load = Assembly.LoadFrom(location);
            var a1 = load.GetReferencedAssemblies().Single(name => name.FullName == "System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var a2 = load.GetReferencedAssemblies().Single(name => name.FullName == "System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var a3 = load.GetReferencedAssemblies().Single(name => name.FullName == "System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var a4 = load.GetReferencedAssemblies().Single(name => name.FullName == "Accessibility, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var a5 = load.GetReferencedAssemblies().Single(name => name.FullName == "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            AssemblyName[] assemblyNames = { a1, a2, a3, a4, a5 };
            var mock = new Mock<IAssemblyWrapper>();

            mock.Setup(wrapper => wrapper.Load(CleanGacName)).Returns(load);
            mock.Setup(wrapper => wrapper.Load("")).Throws(new Exception());
            mock.Setup(wrapper => wrapper.Load(a1)).Returns(Assembly.Load(a1));
            mock.Setup(wrapper => wrapper.Load(a2)).Returns(Assembly.Load(a2));
            mock.Setup(wrapper => wrapper.Load(a3)).Returns(Assembly.Load(a3));
            mock.Setup(wrapper => wrapper.Load(a4)).Returns(Assembly.Load(a4));
            mock.Setup(wrapper => wrapper.Load(a5)).Returns(Assembly.Load(a5));
            mock.Setup(wrapper => wrapper.GetReferencedAssemblies(load)).Returns(assemblyNames);
            var assemblyLoader = new AssemblyLoader(mock.Object);
            var fieldInfo = typeof(AssemblyLoader).GetField("_loadedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fieldInfo);
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var value = (List<string>)fieldInfo.GetValue(assemblyLoader);
            Assert.AreEqual(0, value.Count);
            Assembly assembly;
            assemblyLoader.TryLoadAssembly(DirtyGacName, "", out assembly);
            value = (List<string>)fieldInfo.GetValue(assemblyLoader);
            //---------------Test Result -----------------------
            Assert.AreEqual(5, value.Count);
            mock.Verify(wrapper => wrapper.Load(It.IsAny<AssemblyName>()), Times.Exactly(5));
            mock.Verify(wrapper => wrapper.GetReferencedAssemblies(load), Times.Once);
            mock.Verify(wrapper => wrapper.Load(a1));
            mock.Verify(wrapper => wrapper.Load(a2));
            mock.Verify(wrapper => wrapper.Load(a3));
            mock.Verify(wrapper => wrapper.Load(a4));
            mock.Verify(wrapper => wrapper.Load(a5));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssembly_GivenLoadsCorreclty_ShouldAddAssembliesTo_loadedAssemblies()
        {
            //---------------Set up test pack-------------------
            var human = new Human();
            var type = human.GetType();
            var humanAssembly = type.Assembly;
            var location = humanAssembly.Location;
            Assert.IsNotNull(location);
            var load = Assembly.LoadFrom(location);

            var referencedAssemblies = load.GetReferencedAssemblies();

            var a1 = referencedAssemblies.Single(name => name.FullName == "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            AssemblyName[] assemblyNames = { a1 };
            var mock = new Mock<IAssemblyWrapper>();
            var path = Path.GetDirectoryName(location);
            var myLoad = Path.Combine(path, a1.Name + ".dll");


            mock.Setup(wrapper => wrapper.Load(location)).Returns(load);
            mock.Setup(wrapper => wrapper.LoadFrom(location)).Throws(new Exception());
            mock.Setup(wrapper => wrapper.LoadFrom(myLoad)).Throws(new Exception());
            mock.Setup(wrapper => wrapper.UnsafeLoadFrom(location)).Throws(new Exception());
            mock.Setup(wrapper => wrapper.GetAssembly(type)).Returns(load);
            mock.Setup(wrapper => wrapper.Load(a1)).Throws(new Exception());
            //mock.Setup(wrapper => wrapper.Load(a1)).Returns(Assembly.Load(a1));
            mock.Setup(wrapper => wrapper.GetReferencedAssemblies(load)).Returns(assemblyNames);
            var assemblyLoader = new AssemblyLoader(mock.Object);
            var fieldInfo = typeof(AssemblyLoader).GetField("_loadedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fieldInfo);
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var value = (List<string>)fieldInfo.GetValue(assemblyLoader);
            Assert.AreEqual(0, value.Count);
            Assembly assembly;
            assemblyLoader.TryLoadAssembly(location, type.FullName, out assembly);
            value = (List<string>)fieldInfo.GetValue(assemblyLoader);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, value.Count);
            mock.Verify(wrapper => wrapper.Load(It.IsAny<AssemblyName>()), Times.Exactly(1));
            mock.Verify(wrapper => wrapper.GetReferencedAssemblies(load), Times.Once);
            mock.Verify(wrapper => wrapper.Load(a1));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(BadImageFormatException))]
        public void TryLoadAssemblyGac_GivenThrowsBadFormat_Shouldrethrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            mock.Setup(wrapper => wrapper.Load(CleanGacName)).Throws(new BadImageFormatException());
            mock.Setup(wrapper => wrapper.Load("")).Throws(new BadImageFormatException());
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            assemblyLoader.TryLoadAssembly(DirtyGacName, "", out assembly);
            //---------------Test Result -----------------------

        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssembly_GivenReturnsNull_ShouldNotLoadAssembly()
        {
            //---------------Set up test pack-------------------
            var assemblyLoader = new AssemblyLoader();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            var human = new Human();
            var location = human.GetType().Assembly.Location;
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(location, "", out assembly);
            //---------------Test Result -----------------------
            Assert.IsTrue(tryLoadAssembly);
            Assert.IsNotNull(assembly);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(BadImageFormatException))]
        public void TryLoadAssembly_GivenThrowsBadFormat_Shouldrethrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            var human = new Human();
            var location = human.GetType().Assembly.Location;
            mock.Setup(wrapper => wrapper.LoadFrom(location)).Throws(new BadImageFormatException());
            mock.Setup(wrapper => wrapper.Load("")).Throws(new BadImageFormatException());
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            assemblyLoader.TryLoadAssembly(location, "", out assembly);
            //---------------Test Result -----------------------

        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssembly_GivenThrowsExceptionForPathDll_ShouldThrowLoadUnsafely()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            var human = new Human();
            var loadedAssembly = human.GetType().Assembly;
            var location = loadedAssembly.Location;
            mock.Setup(wrapper => wrapper.LoadFrom(location)).Throws(new Exception());
            mock.Setup(wrapper => wrapper.UnsafeLoadFrom(location)).Returns(loadedAssembly);
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(location, "", out assembly);
            //---------------Test Result -----------------------
            Assert.IsTrue(tryLoadAssembly);
            Assert.IsNotNull(assembly);
            mock.Verify(wrapper => wrapper.LoadFrom(location));
            mock.Verify(wrapper => wrapper.UnsafeLoadFrom(location));
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssembly_GivenUnsafeLoadFromThrows_ShouldGetAssmbly()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            var human = new Human();
            var type = human.GetType();
            var loadedAssembly = type.Assembly;
            var location = loadedAssembly.Location;
            mock.Setup(wrapper => wrapper.LoadFrom(location)).Throws(new Exception());
            mock.Setup(wrapper => wrapper.UnsafeLoadFrom(location)).Throws(new Exception());
            mock.Setup(wrapper => wrapper.GetAssembly(type)).Returns(loadedAssembly);
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(location, type.FullName, out assembly);
            //---------------Test Result -----------------------
            Assert.IsTrue(tryLoadAssembly);
            Assert.IsNotNull(assembly);
            mock.Verify(wrapper => wrapper.LoadFrom(location));
            mock.Verify(wrapper => wrapper.UnsafeLoadFrom(location));
            mock.Verify(wrapper => wrapper.GetAssembly(type));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(BadImageFormatException))]
        public void TryLoadAssembly_GivenUnsafeLoadFromThrowsBadFormat_ShouldRethrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            var human = new Human();
            var type = human.GetType();
            var loadedAssembly = type.Assembly;
            var location = loadedAssembly.Location;
            mock.Setup(wrapper => wrapper.LoadFrom(location)).Throws(new Exception());
            mock.Setup(wrapper => wrapper.UnsafeLoadFrom(location)).Throws(new Exception());
            mock.Setup(wrapper => wrapper.GetAssembly(type)).Throws(new BadImageFormatException());
            var assemblyLoader = new AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            Assembly assembly;
            assemblyLoader.TryLoadAssembly(location, type.FullName, out assembly);
            //---------------Test Result -----------------------

        }
    }
}
