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
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_ValidateUnzipSourceDestinationFileOperation_GivenPathNotFile_ShouldThrowValidExc()
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
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, ex.InnerException.Message);
                src.Setup(point => point.IOPath.Path).Returns(srcPath);
                dst.Setup(point => point.IOPath.Path).Returns("");
                try
                {
                    driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
                }
                catch (Exception ex1)
                {
                    Assert.AreEqual(ErrorResource.DestinationMustBeADirectory, ex1.InnerException.Message);
                    dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
                    src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

                    try
                    {
                        driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
                    }
                    catch (Exception ex2)
                    {
                        Assert.AreEqual(ErrorResource.SourceMustBeAFile, ex2.InnerException.Message);
                        src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
                        dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);

                        try
                        {
                            driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
                        }
                        catch (Exception ex3)
                        {
                            Assert.AreEqual(ErrorResource.DestinationDirectoryExist, ex3.InnerException.Message);
                            args = new Dev2UnZipOperationTO("pa", true);

                            var invoke = driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
                            Assert.AreEqual(performAfterValidation.Invoke(), invoke.ToString());

                        }

                    }
                }
            }


            //---------------Test Result -----------------------
        }
    }
}
