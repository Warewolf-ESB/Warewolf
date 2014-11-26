
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
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.DurableInstancing;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.DynamicServices;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FileSystemInstanceStoreIoTests
    {
        const string TestFileName = "TestInstanceStore";
        const string TestMetaFileName = "TestInstanceStoreMeta";
        static string TestPath;

        #region ClassInitialize/Cleanup

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            //var currentDir = Path.Combine(testContext.TestDir, Guid.NewGuid().ToString());
            var currentDir = EnvironmentVariables.ApplicationPath;

            TestPath = Path.Combine(currentDir, "InstanceStore");
            Directory.CreateDirectory(TestPath);
            Directory.SetCurrentDirectory(currentDir);
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {
            if(Directory.Exists(TestPath))
            {
                DirectoryHelper.CleanUp(TestPath);
            }
        }

        #endregion

        #region LoadInstance

        [TestMethod]
        [ExpectedException(typeof(InstancePersistenceException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void LoadInstance_With_FileUsedByAnotherProcess_Expected_ThrowsInstancePersistenceException()
        // ReSharper restore InconsistentNaming
        {
            var instanceId = Guid.NewGuid();
            var path = Path.Combine(TestPath, instanceId + ".xml");
            var metaPath = Path.Combine(TestPath, instanceId + ".meta.xml");

            try
            {
                var xml = XmlResource.Fetch(TestFileName);
                xml.Save(path);
                xml = XmlResource.Fetch(TestMetaFileName);
                xml.Save(metaPath);

                // Force error: The process cannot access the file because it is being used by another process
                using(var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    var store = new FileSystemInstanceStoreIO();
                    IDictionary<XName, InstanceValue> instanceData;
                    IDictionary<XName, InstanceValue> instanceMetadata;

                    var result = store.LoadInstance(instanceId, out instanceData, out instanceMetadata);
                }
            }
            finally
            {
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
                if(File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
            }
        }

        #endregion

        #region SaveAllInstanceData

        [TestMethod]
        [ExpectedException(typeof(InstancePersistenceException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void SaveAllInstanceData_With_FileUsedByAnotherProcess_Expected_ThrowsInstancePersistenceException()
        // ReSharper restore InconsistentNaming
        {
            var instanceId = Guid.NewGuid();
            var path = Path.Combine(TestPath, instanceId + ".xml");

            try
            {
                var xml = XmlResource.Fetch(TestFileName);
                xml.Save(path);

                // Force error: The process cannot access the file because it is being used by another process
                using(var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    var store = new FileSystemInstanceStoreIO();
                    var result = store.SaveAllInstanceData(instanceId, new SaveWorkflowCommand());
                }
            }
            finally
            {
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        #endregion

        #region SaveAllInstanceMetaData

        [TestMethod]
        [ExpectedException(typeof(InstancePersistenceException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void SaveAllInstanceMetaData_With_FileUsedByAnotherProcess_Expected_ThrowsInstancePersistenceException()
        // ReSharper restore InconsistentNaming
        {
            var instanceId = Guid.NewGuid();
            var path = Path.Combine(TestPath, instanceId + ".meta.xml");

            try
            {
                var xml = XmlResource.Fetch(TestMetaFileName);
                xml.Save(path);

                // Force error: The process cannot access the file because it is being used by another process
                using(var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    var store = new FileSystemInstanceStoreIO();
                    store.SaveAllInstanceMetaData(instanceId, new SaveWorkflowCommand());
                }
            }
            finally
            {
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        #endregion

        #region SaveInstanceAssociation

        [TestMethod]
        [ExpectedException(typeof(InstancePersistenceException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void SaveInstanceAssociation_With_FileUsedByAnotherProcess_Expected_ThrowsInstancePersistenceException()
        // ReSharper restore InconsistentNaming
        {
            var instanceId = Guid.NewGuid();
            var instanceKey = Guid.NewGuid();

            var store = new FileSystemInstanceStoreIO();

            var path = store.GetSaveInstanceAssociationPath(instanceId, instanceKey);

            try
            {
                var xml = XmlResource.Fetch(TestFileName);
                xml.Save(path);

                // Force error: The process cannot access the file because it is being used by another process
                using(var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    store.SaveInstanceAssociation(instanceId, instanceKey, true);
                }
            }
            finally
            {
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        #endregion

        #region DeleteInstanceAssociation
        //SASHEN NAIDOO
        //Commented out because this test is not thread safe and needs to be changed
        //[TestMethod]
        //[ExpectedException(typeof(InstancePersistenceException))]
        //// ReSharper disable InconsistentNaming - Unit Tests
        //public void DeleteInstanceAssociation_With_FileUsedByAnotherProcess_Expected_ThrowsInstancePersistenceException()
        //// ReSharper restore InconsistentNaming
        //{
        //    var instanceId = Guid.NewGuid();
        //    var instanceKey = Guid.NewGuid();
        //    var path = Path.Combine(TestPath, string.Format("Key.{0}.{1}.xml", instanceId, instanceKey));

        //    try
        //    {
        //        var xml = XmlResource.Fetch(TestFileName);
        //        xml.Save(path);

        //        // Force error: The process cannot access the file because it is being used by another process
        //        using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
        //        {
        //            var store = new FileSystemInstanceStoreIO();
        //            store.DeleteInstanceAssociation(instanceKey);
        //        }
        //    }
        //    finally
        //    {
        //        if (File.Exists(path))
        //        {
        //            File.Delete(path);
        //        }
        //    }
        //}

        #endregion

        #region GetInstanceAssociation

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void GetInstanceAssociation_With_InvalidFileName_Expected_ReturnsEmptyGuid()
        // ReSharper restore InconsistentNaming
        {
            const string InvalidInstanceId = "99999";
            var instanceKey = Guid.NewGuid();
            var path = Path.Combine(TestPath, string.Format("Key.{0}.{1}.xml", instanceKey, InvalidInstanceId));

            try
            {
                var xml = XmlResource.Fetch(TestFileName);
                xml.Save(path);

                var store = new FileSystemInstanceStoreIO();
                var result = store.GetInstanceAssociation(instanceKey);
                Assert.AreEqual(Guid.Empty, result);
            }
            finally
            {
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        #endregion
    }
}
