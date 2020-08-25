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
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(assemblyLoader);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssemblyGac_GivenReturnsNull_ShouldNotLoadAssembly()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            var load = Assembly.GetExecutingAssembly();
            var cleanName = load.FullName;
            var dirtyname = MakeDirty(cleanName);
            mock.Setup(wrapper => wrapper.Load(It.IsAny<string>()));
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(dirtyname, "", out Assembly assembly);
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
            var load = Assembly.GetExecutingAssembly();
            var cleanName = load.FullName;
            var dirtyname = MakeDirty(cleanName);
            mock.Setup(wrapper => wrapper.Load(cleanName))
                .Callback<string>((a) =>
                    {
                        var load1 = Assembly.Load(a);
                        Assert.IsNotNull(load1);
                    });
            mock.Setup(wrapper => wrapper.Load("")).Throws(new Exception());
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(dirtyname, "", out Assembly assembly);
            //---------------Test Result -----------------------
            Assert.IsFalse(tryLoadAssembly);
            Assert.IsNull(assembly);
            mock.Verify(wrapper => wrapper.Load(cleanName));
        }

        static string MakeDirty(string name)
        {
            return "GAC:" + name + ", processorArchitecture=x86";
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssemblyGac_GivenDirtyPath_ShouldCleanPathAndLoadAssembly()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            
            var load = Assembly.GetExecutingAssembly();
            var cleanName = load.FullName;
            var dirtyname = MakeDirty(cleanName);

            mock.Setup(wrapper => wrapper.Load(cleanName))
               .Returns(load);
            mock.Setup(wrapper => wrapper.Load("")).Throws(new Exception());
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(dirtyname, "", out Assembly assembly);
            //---------------Test Result -----------------------
            Assert.IsTrue(tryLoadAssembly);
            Assert.IsNotNull(assembly);
            mock.Verify(wrapper => wrapper.Load(cleanName));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssemblyGac_GivenDirtyPath_ShouldLoadReferencedAssemblies()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAssemblyWrapper>();
            var load = Assembly.GetExecutingAssembly();
            var cleanName = load.FullName;
            var dirtyname = MakeDirty(cleanName);
            var assemblyNames = load.GetReferencedAssemblies();
            
            mock.Setup(wrapper => wrapper.GetReferencedAssemblies(load)).Returns(assemblyNames);
            mock.Setup(wrapper => wrapper.Load(cleanName))
               .Returns(load);
            mock.Setup(wrapper => wrapper.Load("")).Throws(new Exception());
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(dirtyname, "", out Assembly assembly);
            //---------------Test Result -----------------------
            Assert.IsTrue(tryLoadAssembly);
            Assert.IsNotNull(assembly);
            mock.Verify(wrapper => wrapper.Load(cleanName), Times.Exactly(1));
            mock.Verify(wrapper => wrapper.Load(It.IsAny<AssemblyName>()), Times.AtLeast(2));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssemblyGac_GivenLoadsCorreclty_ShouldAddAssembliesTo_loadedAssemblies()
        {
            //---------------Set up test pack-------------------
            var load = Assembly.GetExecutingAssembly();
            var cleanName = load.FullName;
            var dirtyname = MakeDirty(cleanName);
            var mock = new Mock<IAssemblyWrapper>();
            var assemblyNames = load.GetReferencedAssemblies();
            mock.Setup(wrapper => wrapper.Load(cleanName)).Returns(load);
            mock.Setup(wrapper => wrapper.Load("")).Throws(new Exception());
         
            mock.Setup(wrapper => wrapper.GetReferencedAssemblies(load)).Returns(assemblyNames);
            foreach (var assemblyName in assemblyNames)
            {
                mock.Setup(wrapper => wrapper.Load(assemblyName)).Returns(Assembly.Load(assemblyName));
            }
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            var fieldInfo = typeof(AssemblyLoader).GetField("_loadedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fieldInfo);
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var value = (List<string>)fieldInfo.GetValue(assemblyLoader);
            Assert.AreEqual(0, value.Count);
            assemblyLoader.TryLoadAssembly(dirtyname, "", out Assembly assembly);
            value = (List<string>)fieldInfo.GetValue(assemblyLoader);
            //---------------Test Result -----------------------
            Assert.IsTrue(value.Count > 2);
            mock.Verify(wrapper => wrapper.Load(It.IsAny<AssemblyName>()), Times.AtLeast(2));
            mock.Verify(wrapper => wrapper.GetReferencedAssemblies(load), Times.Once);
       
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
            mock.Setup(wrapper => wrapper.GetReferencedAssemblies(load)).Returns(assemblyNames);
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            var fieldInfo = typeof(AssemblyLoader).GetField("_loadedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(fieldInfo);
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var value = (List<string>)fieldInfo.GetValue(assemblyLoader);
            Assert.AreEqual(0, value.Count);
            assemblyLoader.TryLoadAssembly(location, type.FullName, out Assembly assembly);
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
            var load = Assembly.GetExecutingAssembly();
            var cleanName = load.FullName;
            var dirtyname = MakeDirty(cleanName);
            mock.Setup(wrapper => wrapper.Load(cleanName)).Throws(new BadImageFormatException());
            mock.Setup(wrapper => wrapper.Load("")).Throws(new BadImageFormatException());
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            assemblyLoader.TryLoadAssembly(dirtyname, "", out Assembly assembly);
            //---------------Test Result -----------------------

        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TryLoadAssembly_GivenReturnsNull_ShouldNotLoadAssembly()
        {
            //---------------Set up test pack-------------------
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var human = new Human();
            var location = human.GetType().Assembly.Location;
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(location, "", out Assembly assembly);
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
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            assemblyLoader.TryLoadAssembly(location, "", out Assembly assembly);
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
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(location, "", out Assembly assembly);
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
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            var tryLoadAssembly = assemblyLoader.TryLoadAssembly(location, type.FullName, out Assembly assembly);
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
            var assemblyLoader = new Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin.AssemblyLoader(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(assemblyLoader);
            //---------------Execute Test ----------------------
            assemblyLoader.TryLoadAssembly(location, type.FullName, out Assembly assembly);
            //---------------Test Result -----------------------

        }
    }
}
