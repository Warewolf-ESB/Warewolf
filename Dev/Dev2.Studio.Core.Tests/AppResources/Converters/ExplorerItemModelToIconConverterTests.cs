
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
using System.Windows.Media.Imaging;
using Dev2.AppResources.Converters;
using Dev2.Common.Interfaces.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
    public class ExplorerItemModelToIconConverterTests
    {
        [TestInitialize]
        public void TestInit()
        {
            if(!UriParser.IsKnownScheme("pack"))
                // ReSharper disable ObjectCreationAsStatement
                new System.Windows.Application();
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfWorkflowService_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/Workflow-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.WorkflowService, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfDbService_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/DatabaseService-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.DbService, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfPluginService_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/PluginService-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.PluginService, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfWebService_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/WebService-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.WebService, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfDbSource_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/DatabaseSource-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.DbSource, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfPluginSource_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/PluginSource-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.PluginSource, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfWebSource_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/WebSource-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.WebSource, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfEmailSource_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/EmailSource-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.EmailSource, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfServerSource_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/ExplorerWarewolfConnection-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.ServerSource, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfServer_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/ExplorerWarewolfConnection-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.Server, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfFolder_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/ExplorerFolder-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.Folder, false }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfFolderExpanded_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/ExplorerFolderOpen-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.Folder, true }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfReservedService_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/Workflow-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.ReservedService, true }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfUnknown_CorrectBitmapImage()
        // ReSharper restore InconsistentNaming
        {
            Uri expected = new Uri("pack://application:,,,/Warewolf Studio;component/Images/Workflow-32.png");
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();

            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.Unknown, true }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            if(bitmapImage != null)
            {
                Assert.AreEqual(expected, bitmapImage.UriSource);
            }
            else
            {
                Assert.Fail("No BitmapImage was returned.");
            }
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfVersion_NoImageReturned()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();
            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.Version, true }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            Assert.IsNull(bitmapImage);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModelToIconConverter_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModelToIconConverter_Convert_WithResourceTypeOfMessageNoImageReturned()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var explorerItemModelToIconConverter = new ExplorerItemModelToIconConverter();
            //------------Execute Test---------------------------
            object convert = explorerItemModelToIconConverter.Convert(new object[] { ResourceType.Message, true }, null, null, null);
            BitmapImage bitmapImage = convert as BitmapImage;
            //------------Assert Results-------------------------
            Assert.IsNull(bitmapImage);
        }
    }
}
