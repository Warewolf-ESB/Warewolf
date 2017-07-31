using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;
using Ionic.Zip;
using Ionic.Zlib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;

// ReSharper disable InconsistentNaming

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    public class Dev2ActivityIOBrokerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateInstance_GivenThrowsNoExpetion_ShouldBeIActivityOperationsBroker()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                var activityOperationsBroker = CreateBroker();
                Assert.IsInstanceOfType(activityOperationsBroker, typeof(IActivityOperationsBroker));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Get_GivenPath_ShouldReturnFileEncodingContents()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var fileMock = new Mock<IActivityIOOperationsEndPoint>();
            fileMock.Setup(point => point.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(new ByteBuffer(Encoding.ASCII.GetBytes("")));
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var stringEncodingContents = activityOperationsBroker.Get(fileMock.Object, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(stringEncodingContents);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Get_GivenPath_ShouldReturnFileDecodedContents()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var fileMock = new Mock<IActivityIOOperationsEndPoint>();

            const string iAmGood = "I am good";
            fileMock.Setup(point => point.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(new ByteBuffer(Encoding.ASCII.GetBytes(iAmGood)));
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var stringEncodingContents = activityOperationsBroker.Get(fileMock.Object, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(stringEncodingContents);
            Assert.AreEqual(iAmGood, stringEncodingContents);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsBase64_GivenStartsWithBase64_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            PrivateObject obj = new PrivateObject(activityOperationsBroker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var invoke = obj.Invoke("IsBase64", "Content-Type:BASE64SomeJunkdata");
            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            PrivateObject obj = new PrivateObject(activityOperationsBroker);
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var pathReturned = obj.Invoke("GetFileNameFromEndPoint", mockEndpoint.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName_Overload()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            PrivateObject obj = new PrivateObject(activityOperationsBroker);
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var pathReturned = obj.Invoke("GetFileNameFromEndPoint", mockEndpoint.Object, mockActIo.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ListDirectory_GivenFilesAndFolders_ShouldReturnEmptyList()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var activityIOPaths = activityOperationsBroker.ListDirectory(endPoint.Object, ReadTypes.FilesAndFolders);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, activityIOPaths.Count);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ListDirectory_GivenFiles_ShouldReturnEmptyList()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFilesInDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var activityIOPaths = activityOperationsBroker.ListDirectory(endPoint.Object, ReadTypes.Files);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, activityIOPaths.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ListDirectory_GivenFolders_ShouldReturnEmptyList()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFoldersInDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var activityIOPaths = activityOperationsBroker.ListDirectory(endPoint.Object, ReadTypes.Folders);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, activityIOPaths.Count);
        }

        private static IActivityOperationsBroker CreateBroker()
        {
            return ActivityIOFactory.CreateOperationsBroker();
        }

        private static IActivityOperationsBroker CreateBroker(IFile file, ICommon common)
        {
            return ActivityIOFactory.CreateOperationsBroker(file, common);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNotFtpTypePath_GivenFileSysPath_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            const string txt = "C:\\Home\\a.txt";
            var pathMock = new Mock<IActivityIOPath>();
            pathMock.Setup(path => path.Path).Returns(txt);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Util.CommonDataUtils commonDataUtils = new Util.CommonDataUtils();
            var value = commonDataUtils.IsNotFtpTypePath( pathMock.Object);
            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(value.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsNotFtpTypePath_GivenFtpPath_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            const string txt = "ftp://Home//a.txt";
            var pathMock = new Mock<IActivityIOPath>();
            pathMock.Setup(path => path.Path).Returns(txt);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Util.CommonDataUtils commonDataUtils = new Util.CommonDataUtils();
            var value = commonDataUtils.IsNotFtpTypePath(pathMock.Object);
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(value.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsUncFileTypePath_GivenUNCPath_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            const string txt = "\\\\Home\\a.txt";
            var pathMock = new Mock<IActivityIOPath>();
            pathMock.Setup(path => path.Path).Returns(txt);
            Util.CommonDataUtils commonDataUtils = new Util.CommonDataUtils();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var value = commonDataUtils.IsUncFileTypePath(pathMock.Object);
            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(value.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateDirectory_GivenValidInterfaces_ShouldCallsCreateDirectoryCorrectly()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            endPoint.Setup(point => point.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var invoke = privateObject.Invoke("CreateDirectory", endPoint.Object, dev2CrudOperationTO);
            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(invoke.ToString()));
            endPoint.Verify(point => point.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ValidateEndPoint_GivenEmptyPath_ShouldThrowValidExc()
        {
            //---------------Set up test pack-------------------
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            endPoint.Setup(point => point.IOPath.Path).Returns("");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                Util.CommonDataUtils commonDataUtils = new Util.CommonDataUtils();
                commonDataUtils.ValidateEndPoint( endPoint.Object, dev2CrudOperationTO);
            }
            catch (Exception e)
            {
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, e.Message);
            }
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ValidateEndPoint_GivenPathAndOverwriteFalse_ShouldThrowValidExc()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(false);
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            endPoint.Setup(point => point.IOPath.Path).Returns("somepath");
            endPoint.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                Dev2.Data.Util.CommonDataUtils commonDataUtils = new Util.CommonDataUtils();
                commonDataUtils.ValidateEndPoint(endPoint.Object, dev2CrudOperationTO);
            }
            catch (Exception e)
            {
                Assert.AreEqual(ErrorResource.DestinationDirectoryExist, e.Message);
            }
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RemoveTmpFile_GivenEmptyFile_ShouldThrowAndLogException()
        {
            //---------------Set up test pack-------------------
            var file = new Mock<IFile>();
            var activityOperationsBroker = CreateBroker(file.Object, new Util.CommonDataUtils());
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("RemoveTmpFile", string.Empty);
            }
            catch (Exception ex)
            {
                //---------------Test Result -----------------------
                Assert.AreEqual("", ex.Message);
            }
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RemoveTmpFile_GivenFileFile_ShouldDeleteFile()
        {
            //---------------Set up test pack-------------------
            var file = new Mock<IFile>();
            file.Setup(file1 => file1.Delete(It.IsAny<string>()));
            var activityOperationsBroker = CreateBroker(file.Object, new Util.CommonDataUtils());
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            privateObject.Invoke("RemoveTmpFile", "SomePath");
            //---------------Test Result -----------------------
            file.Verify(file1 => file1.Delete(It.IsAny<string>()));

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Create_GivenDestination_ShouldCreateFileCorrectly()
        {
            //Create(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile)
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var dstPath = new Mock<IActivityIOPath>();
            var args = new Dev2CRUDOperationTO(true);
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(point => point.IOPath.Username).Returns("userName");
            dst.Setup(point => point.IOPath.Password).Returns("Password");
            dst.Setup(point => point.PathSeperator()).Returns(",");
            dst.Setup(point => point.IOPath.Path).Returns("path");
            dst.Setup(point => point.PathExist(dstPath.Object)).Returns(true);
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("Create", dst.Object, args, true);
            }
            catch (Exception ex)
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(ErrorResource.InvalidPath, ex.InnerException.Message);
                var tempFileName = Path.GetTempFileName();
                dst.Setup(point => point.IOPath.Path).Returns(tempFileName);

                var success = privateObject.Invoke("Create", dst.Object, args, true);
                Assert.AreEqual("Success".ToUpper(), success.ToString().ToUpper());
                File.Delete(tempFileName);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTmpDirectory_GivenObject_ShouldCreateFolderInTheCorrectLocation()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Util.CommonDataUtils commonDataUtils = new Util.CommonDataUtils();
            var path = commonDataUtils.CreateTmpDirectory();
            //---------------Test Result -----------------------
            Assert.IsNotNull(path);
            Assert.IsTrue(Directory.Exists(path));
            StringAssert.Contains(path, GlobalConstants.TempLocation);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExtractZipCompressionLevel_GivenLevel_ShouldCorreclty()
        {
            //---------------Set up test pack-------------------
            Util.CommonDataUtils commonDataUtils = new Util.CommonDataUtils();
            var level = commonDataUtils.ExtractZipCompressionLevel("Test");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(level);
            Assert.AreEqual(CompressionLevel.Default, level);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AppendToTemp_GivenStreamAndTempString_ShouldNotThroException()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            Util.CommonDataUtils  commonDataUtils = new Util.CommonDataUtils();
            const string dispatcherInvoke = "Dispatcher.Invoke";
            Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(dispatcherInvoke));
            const string ranString = "Rando.net";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            commonDataUtils.AppendToTemp( stream, ranString);

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Delete_GivenDeleteIsTrue_ShouldReturnResulOk()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(point => point.Delete(It.IsAny<IActivityIOPath>())).Returns(true);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var delete = activityOperationsBroker.Delete(dst.Object);

            //---------------Test Result -----------------------
            Assert.AreEqual("Success", delete);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Delete_GivenDeleteIsFalse_ShouldReturnResulBad()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(point => point.Delete(It.IsAny<IActivityIOPath>())).Returns(false);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var delete = activityOperationsBroker.Delete(dst.Object);

            //---------------Test Result -----------------------
            Assert.AreEqual("Failure", delete);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ValidateUnzipSourceDestinationFileOperation_GivenPathNotFile_ShouldThrowValidExc()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
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
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("ValidateUnzipSourceDestinationFileOperation", src.Object, dst.Object, args, performAfterValidation);
            }
            catch(Exception ex)
            {
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, ex.InnerException.Message);
                src.Setup(point => point.IOPath.Path).Returns(srcPath);
                dst.Setup(point => point.IOPath.Path).Returns("");
                try
                {
                    privateObject.Invoke("ValidateUnzipSourceDestinationFileOperation", src.Object, dst.Object, args, performAfterValidation);
                }
                catch(Exception ex1)
                {
                    Assert.AreEqual(ErrorResource.DestinationMustBeADirectory, ex1.InnerException.Message);
                    dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
                    src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

                    try
                    {
                        privateObject.Invoke("ValidateUnzipSourceDestinationFileOperation", src.Object, dst.Object, args, performAfterValidation);
                    }
                    catch(Exception ex2)
                    {
                        Assert.AreEqual(ErrorResource.SourceMustBeAFile, ex2.InnerException.Message);
                        src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
                        dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);
                       
                        try
                        {
                            privateObject.Invoke("ValidateUnzipSourceDestinationFileOperation", src.Object, dst.Object, args, performAfterValidation);
                        }
                        catch(Exception ex3)
                        {
                            Assert.AreEqual(ErrorResource.DestinationDirectoryExist, ex3.InnerException.Message);
                            args = new Dev2UnZipOperationTO("pa", true);
                            
                                var invoke = privateObject.Invoke("ValidateUnzipSourceDestinationFileOperation", src.Object, dst.Object, args, performAfterValidation);
                                Assert.AreEqual(performAfterValidation.Invoke(), invoke.ToString());
                            
                        }

                    }
                }
            }
      

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DoFileTransfer_GivenValidArgs_ShouldtransferCorrectly()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var dstPath = new Mock<IActivityIOPath>();
            var p = new Mock<IActivityIOPath>();
            var args = new Dev2CRUDOperationTO(true);
            var src = new Mock<IActivityIOOperationsEndPoint>();
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(point => point.IOPath.Username).Returns("userName");
            dst.Setup(point => point.IOPath.Password).Returns("Password");
            dst.Setup(point => point.IOPath.PrivateKeyFile).Returns("PKFile");
            dst.Setup(point => point.PathExist(dstPath.Object)).Returns(true);
            src.Setup(point => point.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(new MemoryStream());
            src.Setup(point => point.IOPath.PathType).Returns(enActivityIOPathType.FileSystem);
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            var path = "";
            bool result = false;
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                var invoke = privateObject.Invoke("DoFileTransfer", src.Object, dst.Object, args, dstPath.Object, p.Object, path, result);
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
                Assert.IsTrue(e.InnerException.Message == "Invalid Path. Please ensure that the path provided is an absolute path, if you intended to access the local file system.");
                try
                {
                    path = Path.GetTempFileName();
                    privateObject.Invoke("DoFileTransfer", src.Object, dst.Object, args, dstPath.Object, p.Object, path, result);

                    File.Delete(path);

                }
                catch (Exception e1)
                {
                    var error = $"Failed to authenticate with user [ userName ] for resource [ {path} ] ";
                    var actualError = e1.InnerException.Message;
                    //---------------Test Result -----------------------
                    Assert.AreEqual(actualError, error);

                }
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Rename_GivenSoureAndDestinationDifferentPathType_ShouldThrowExc()
        {
            //Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,Dev2CRUDOperationTO args)
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var args = new Dev2CRUDOperationTO(true);
            var src = new Mock<IActivityIOOperationsEndPoint>();
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                activityOperationsBroker.Rename(src.Object, dst.Object, args);
            }
            catch (Exception exc)
            {
                Assert.AreEqual(ErrorResource.SourceAndDestinationNOTFilesOrDirectory, exc.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Rename_GivenSoureAndDestinationSamePathTypePathExistsOverwriteFalse_ShouldThrowException()
        {
            //Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,Dev2CRUDOperationTO args)
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var args = new Dev2CRUDOperationTO(false);
            var src = new Mock<IActivityIOOperationsEndPoint>();
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                activityOperationsBroker.Rename(src.Object, dst.Object, args);
            }
            catch (Exception exc)
            {
                Assert.AreEqual(ErrorResource.DestinationDirectoryExist, exc.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Rename_GivenSoureAndDestinationSamePathTypePathExistsOverwriteTrue_ShouldDeleteDestFile()
        {
            //Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,Dev2CRUDOperationTO args)
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            var args = new Dev2CRUDOperationTO(true);
            var src = new Mock<IActivityIOOperationsEndPoint>();
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);
            dst.Setup(point => point.Delete(It.IsAny<IActivityIOPath>())).Returns(true);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            try
            {
                activityOperationsBroker.Rename(src.Object, dst.Object, args);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                //---------------Test Result -----------------------
                dst.Verify(point => point.Delete(It.IsAny<IActivityIOPath>()));
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExtractFile_GivenGivenValidArgs_ShouldNotThrowExc()
        {
            //---------------Set up test pack-------------------
            var tempFileName = Path.GetTempFileName();
            var tempPath = Path.GetDirectoryName(tempFileName);
            Dev2UnZipOperationTO operationTO = new Dev2UnZipOperationTO("Password", false);
            ZipFile file = new ZipFile();
            file.AddFile(tempFileName);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                Util.CommonDataUtils commonDataUtils = new Util.CommonDataUtils();
                file.Save(tempFileName);
                commonDataUtils.ExtractFile(operationTO, file, tempPath);
            }
            catch(Exception ex)
            {
                Assert.AreEqual("",ex.Message);
                
            }
            finally
            {
                File.Delete(tempFileName);
            }

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MoveTmpFileToDestination_GiventmpFile_ShouldReturnSucces()
        {
            //---------------Set up test pack-------------------
            var tempFileName = Path.GetTempFileName();
            var commonMock = new Mock<ICommon>();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(file => file.ReadAllBytes(It.IsAny<string>())).Returns(Encoding.ASCII.GetBytes("Hello world"));
            var activityOperationsBroker = CreateBroker(fileMock.Object, commonMock.Object);
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(false);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            dst.Setup(point => point.IOPath.Path).Returns(tempFileName);
            dst.Setup(point => point.Put(It.IsAny<Stream>(), It.IsAny<IActivityIOPath>(), It.IsAny<Dev2CRUDOperationTO>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(1);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            const string result = "";
            try
            {
                privateObject.SetField("_filesToDelete", new List<string>());
                privateObject.Invoke("MoveTmpFileToDestination", dst.Object, tempFileName, result);
                
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                File.Delete(tempFileName);
            }

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void EnsureFilesDontExists_GivenPathExistsAndPasthIsFile_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var tempFileName = Path.GetTempFileName();
            var tempSrcName = Path.GetTempFileName();
            var commonMock = new Mock<ICommon>();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(file => file.ReadAllBytes(It.IsAny<string>())).Returns(Encoding.ASCII.GetBytes("Hello world"));
            var activityOperationsBroker = CreateBroker(fileMock.Object, commonMock.Object);
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            var src = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            src.Setup(point => point.PathSeperator()).Returns(",");
            dst.Setup(point => point.IOPath.Path).Returns(tempFileName);
            src.Setup(point => point.IOPath.Path).Returns(tempSrcName);
            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("EnsureFilesDontExists", src.Object, dst.Object);
            }
            catch(Exception e)
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(ErrorResource.FileWithSameNameExist, e.InnerException.Message);
                dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
                dst.Setup(point => point.ListDirectory(It.IsAny<IActivityIOPath>())).Returns(new List<IActivityIOPath>());
                try
                {
                    privateObject.Invoke("EnsureFilesDontExists", src.Object, dst.Object);
                }
                catch(Exception ex)
                {
                   Assert.Fail(ex.InnerException.Message);
                }
               

            }
            finally
            {
                File.Delete(tempFileName);
                File.Delete(tempSrcName);
            }
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void PutRaw_GivenAppendFromBottom_RemovesTempFile()
        {
            //------------Setup for test-------------------------
            var commonMock = new Mock<ICommon>();
            var fileMock = new Mock<IFile>();
            var ioPathMock = new Mock<IActivityIOPath>();
            var putRawOperationMock = new Mock<IDev2PutRawOperationTO>();
            fileMock.Setup(file => file.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()));
            fileMock.Setup(file => file.Exists(It.IsAny<string>())).Returns(true);
            var activityOperationsBroker = CreateBroker(fileMock.Object, commonMock.Object);
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            var activityIOOperationsEndPointMock = new Mock<IActivityIOOperationsEndPoint>();
            activityIOOperationsEndPointMock.Setup(point => point.IOPath).Returns(ioPathMock.Object);
            activityIOOperationsEndPointMock.Setup(point =>point.Get(ioPathMock.Object, It.IsAny<List<string>>())).Returns(new MemoryStream());
            putRawOperationMock.Setup(to => to.WriteType).Returns(WriteType.AppendBottom);
            //------------Execute Test---------------------------
            var args = new object[] { activityIOOperationsEndPointMock.Object, putRawOperationMock.Object };
            privateObject.Invoke("PutRaw", args);
            //------------Assert Results-------------------------
            fileMock.Verify(file => file.Delete(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
