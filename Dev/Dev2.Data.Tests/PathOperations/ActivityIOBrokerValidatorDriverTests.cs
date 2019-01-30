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
using System.IO;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class ActivityIOBrokerValidatorDriverTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateUnzipSourceDestinationFileOperation_InvalidSource()
        {
            //---------------Set up test pack-------------------
            var driver = new ActivityIOBrokerValidatorDriver();

            var srcPath = Path.GetTempPath();
            var dstPath = Path.GetTempPath();
            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.IOPath.Path).Returns("");
            src.Setup(point => point.PathSeperator()).Returns(",");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.SetupProperty(point => point.IOPath.Path);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            var args = new Dev2UnZipOperationTO("password", false);
            Func<string> performAfterValidation = () => "Success";
            var privateObject = driver;

            var hadException = false;
            try
            {
                driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
            }
            catch (Exception ex)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, ex.Message);
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateUnzipSourceDestinationFileOperation_DestinationNotADirectory()
        {
            //---------------Set up test pack-------------------
            var driver = new ActivityIOBrokerValidatorDriver();

            var srcPath = Path.GetTempPath();
            var dstPath = Path.GetTempPath();
            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.IOPath.Path).Returns("");
            src.Setup(point => point.PathSeperator()).Returns(",");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.SetupProperty(point => point.IOPath.Path);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            var args = new Dev2UnZipOperationTO("password", false);
            Func<string> performAfterValidation = () => "Success";
            var privateObject = driver;

            src.Setup(point => point.IOPath.Path).Returns(srcPath);
            dst.Setup(point => point.IOPath.Path).Returns("");

            var hadException = false;
            try
            {
                driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
            }
            catch (Exception ex1)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.DestinationMustBeADirectory, ex1.Message);
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateUnzipSourceDestinationFileOperation_SourceNotAFile()
        {
            //---------------Set up test pack-------------------
            var driver = new ActivityIOBrokerValidatorDriver();

            var srcPath = Path.GetTempPath();
            var dstPath = Path.GetTempPath();
            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.IOPath.Path).Returns("");
            src.Setup(point => point.PathSeperator()).Returns(",");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.SetupProperty(point => point.IOPath.Path);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            var args = new Dev2UnZipOperationTO("password", false);
            Func<string> performAfterValidation = () => "Success";
            var privateObject = driver;

            src.Setup(point => point.IOPath.Path).Returns(srcPath);
            dst.Setup(point => point.IOPath.Path).Returns("");

            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

            var hadException = false;
            try
            {
                driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
            }
            catch (Exception ex2)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.SourceMustBeAFile, ex2.Message);
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateUnzipSourceDestinationFileOperation_DestinationDirectoryExists()
        {
            //---------------Set up test pack-------------------
            var driver = new ActivityIOBrokerValidatorDriver();

            var srcPath = Path.GetTempPath();
            var dstPath = Path.GetTempPath();
            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.IOPath.Path).Returns("");
            src.Setup(point => point.PathSeperator()).Returns(",");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.SetupProperty(point => point.IOPath.Path);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            var args = new Dev2UnZipOperationTO("password", false);
            Func<string> performAfterValidation = () => "Success";
            var privateObject = driver;

            src.Setup(point => point.IOPath.Path).Returns(srcPath);
            dst.Setup(point => point.IOPath.Path).Returns("");

            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

            
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);

            var hadException = false;
            try
            {
                driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
            }
            catch (Exception ex3)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.DestinationDirectoryExist, ex3.Message);
                args = new Dev2UnZipOperationTO("pa", true);

                var invoke = driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
                Assert.AreEqual(performAfterValidation.Invoke(), invoke.ToString());
            }
            Assert.IsTrue(hadException);
        }
    }
}
