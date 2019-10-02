﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Warewolf.Common;
using Warewolf.Configuration;
using Warewolf.VirtualFileSystem;
using Warewolf.Web;

namespace Warewolf.Configuration
{
    [TestClass]
    public class ConfigSettingsBaseTests
    {
        private const int DefaultSomeInt = 123;
        private const string DefaultSomeString = "some string";

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ConfigSettingsBaseForTesting))]
        public void ConfigSettingsBase_SaveIfNotExists_ShouldCreateFileWithDefaults()
        {
            var path = "somepath.json";
            var mockFile = new Mock<IFileBase>();
            var mockDirectory = new Mock<IDirectoryBase>();

            var config = new ConfigSettingsBaseForTesting(path, mockFile.Object, mockDirectory.Object);

            config.SaveIfNotExists();

            var expectedData = JsonConvert.SerializeObject(new TestData { SomeInt = DefaultSomeInt, SomeString = DefaultSomeString });

            mockFile.Verify(o => o.Exists(path), Times.Exactly(2));
            mockDirectory.Verify(o => o.CreateIfNotExists(It.IsAny<string>()), Times.Once);
            mockFile.Verify(o => o.WriteAllText(path, expectedData), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ConfigSettingsBaseForTesting))]
        public void ConfigSettingsBase_Load_GivenNoConfigFile_ExpectDefaults()
        {
            var path = "somepath.json";
            var mockFile = new Mock<IFileBase>();
            mockFile.Setup(o => o.Exists(path)).Returns(false);
            var mockDirectory = new Mock<IDirectoryBase>();

            var config = new ConfigSettingsBaseForTesting(path, mockFile.Object, mockDirectory.Object);

            mockFile.Verify(o => o.Exists(path), Times.Once);
            mockFile.Verify(o => o.ReadAllText(path), Times.Never);

            Assert.AreEqual(DefaultSomeInt, config.SomeInt);
            Assert.AreEqual(DefaultSomeString, config.SomeString);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ConfigSettingsBaseForTesting))]
        [ExpectedException(typeof(JsonReaderException))]
        public void ConfigSettingsBase_Load_GivenInvalidConfigFile_ExpectException()
        {
            var path = "somepath.json";
            var mockFile = new Mock<IFileBase>();
            mockFile.Setup(o => o.Exists(path)).Returns(true);
            mockFile.Setup(o => o.ReadAllText(path)).Returns("some broken data");
            var mockDirectory = new Mock<IDirectoryBase>();

            var config = new ConfigSettingsBaseForTesting(path, mockFile.Object, mockDirectory.Object);

            var expectedData = JsonConvert.SerializeObject(new TestData { SomeInt = DefaultSomeInt, SomeString = DefaultSomeString });

            mockFile.Verify(o => o.Exists(path), Times.Once);

            Assert.AreEqual(DefaultSomeInt, config.SomeInt);
            Assert.AreEqual(DefaultSomeString, config.SomeString);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ConfigSettingsBaseForTesting))]
        public void ConfigSettingsBase_PropertyChanges_ShouldCreateFileWithChanges()
        {
            var path = "somepath.json";
            var mockFile = new Mock<IFileBase>();
            var mockDirectory = new Mock<IDirectoryBase>();

            var config = new ConfigSettingsBaseForTesting(path, mockFile.Object, mockDirectory.Object);

            config.SaveIfNotExists();

            var expectedData = JsonConvert.SerializeObject(new TestData { SomeInt = DefaultSomeInt, SomeString = DefaultSomeString });

            mockFile.Verify(o => o.Exists(path), Times.Exactly(2));
            mockFile.Verify(o => o.WriteAllText(path, expectedData), Times.Once);

            config.SomeString = "other string";
            config.SomeInt = 4321;

            var expectedData2 = JsonConvert.SerializeObject(new TestData { SomeInt = DefaultSomeInt, SomeString = "other string" });
            mockFile.Verify(o => o.WriteAllText(path, expectedData2), Times.Once);
            Assert.AreEqual("other string", config.SomeString);

            var expectedData3 = JsonConvert.SerializeObject(new TestData { SomeInt = 4321, SomeString = "other string" });
            mockFile.Verify(o => o.WriteAllText(path, expectedData3), Times.Once);
            Assert.AreEqual(4321, config.SomeInt);
        }

        class TestData
        {
            public int SomeInt { get; set; } = DefaultSomeInt;
            public string SomeString { get; set; } = DefaultSomeString;
        }

        /// <summary>
        /// This class is for tests above but it also shows the pattern for how to make a
        /// self saving property on your config.
        /// </summary>
        class ConfigSettingsBaseForTesting : ConfigSettingsBase<TestData>
        {
            public ConfigSettingsBaseForTesting(string settingsPath, IFileBase file, IDirectoryBase directoryWrapper) : base(settingsPath, file, directoryWrapper)
            {
            }

            public int SomeInt {
                get => _settings.SomeInt;
                set {
                    _settings.SomeInt = value;
                    Save();
                }
            }
            public string SomeString {
                get => _settings.SomeString;
                set
                {
                    _settings.SomeString = value;
                    Save();
                }
            }
        }
    }
}
