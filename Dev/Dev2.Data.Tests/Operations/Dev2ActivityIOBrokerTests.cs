using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.PathOperations.Enums;
using Dev2.Data.Util;
using Dev2.PathOperations;
using Ionic.Zip;
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
            Util.Common common = new Util.Common();
            var value = common.IsNotFtpTypePath( pathMock.Object);
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
            Util.Common common = new Util.Common();
            var value = common.IsNotFtpTypePath(pathMock.Object);
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
            Util.Common common = new Util.Common();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var value = common.IsUncFileTypePath(pathMock.Object);
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
                Util.Common common = new Util.Common();
                common.ValidateEndPoint( endPoint.Object, dev2CrudOperationTO);
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
                Dev2.Data.Util.Common common = new Util.Common();
                common.ValidateEndPoint(endPoint.Object, dev2CrudOperationTO);
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
            var activityOperationsBroker = CreateBroker(file.Object, new Util.Common());
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
            var activityOperationsBroker = CreateBroker(file.Object, new Util.Common());
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
                Assert.AreEqual(ErrorResource.InvalidPath, ex.Message);
                var tempFileName = Path.GetTempFileName();
                dst.Setup(point => point.IOPath.Path).Returns(tempFileName);

                var success = privateObject.Invoke("Create", dst.Object, args, true);
                Assert.AreEqual("Success".ToUpper(), success.ToString().ToUpper());
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
            Util.Common common = new Util.Common();
            var path = common.CreateTmpDirectory();
            //---------------Test Result -----------------------
            Assert.IsNotNull(path);
            Assert.IsTrue(Directory.Exists(path));
            StringAssert.Contains(path, GlobalConstants.TempLocation);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AppendToTemp_GivenStreamAndTempString_ShouldNotThroException()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = CreateBroker();
            Util.Common  common = new Util.Common();
            const string dispatcherInvoke = "Dispatcher.Invoke";
            Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(dispatcherInvoke));
            const string ranString = "Rando.net";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                common.AppendToTemp( stream, ranString);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

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
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, ex.Message);
                src.Setup(point => point.IOPath.Path).Returns(srcPath);
                dst.Setup(point => point.IOPath.Path).Returns("");
                try
                {
                    privateObject.Invoke("ValidateUnzipSourceDestinationFileOperation", src.Object, dst.Object, args, performAfterValidation);
                }
                catch(Exception ex1)
                {
                    Assert.AreEqual(ErrorResource.DestinationMustBeADirectory, ex1.Message);
                    dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
                    src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

                    try
                    {
                        privateObject.Invoke("ValidateUnzipSourceDestinationFileOperation", src.Object, dst.Object, args, performAfterValidation);
                    }
                    catch(Exception ex2)
                    {
                        Assert.AreEqual(ErrorResource.SourceMustBeAFile, ex2.Message);
                        src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
                        dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);
                       
                        try
                        {
                            privateObject.Invoke("ValidateUnzipSourceDestinationFileOperation", src.Object, dst.Object, args, performAfterValidation);
                        }
                        catch(Exception ex3)
                        {
                            Assert.AreEqual(ErrorResource.DestinationDirectoryExist, ex3.Message);
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
                Assert.IsTrue(e.Message == "Invalid Path. Please ensure that the path provided is an absolute path, if you intended to access the local file system.");
                try
                {
                    path = Path.GetTempFileName();
                    privateObject.Invoke("DoFileTransfer", src.Object, dst.Object, args, dstPath.Object, p.Object, path, result);
                }
                catch (Exception e1)
                {
                    var error = $"Failed to authenticate with user [ userName ] for resource [ {path} ] ";
                    var actualError = e1.Message;
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
            var activityOperationsBroker = CreateBroker();
            
            var tempFileName = Path.GetTempFileName();
            var tempPath = Path.GetDirectoryName(tempFileName);
            PrivateObject privateObject = new PrivateObject(activityOperationsBroker);
            Dev2UnZipOperationTO operationTO = new Dev2UnZipOperationTO("Password", false);
            ZipFile file = new ZipFile();
            file.AddFile(tempFileName);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                Util.Common common = new Util.Common();
                file.Save(tempFileName);
                common.ExtractFile(operationTO, file, tempPath);
            }
            catch(Exception ex)
            {
                Assert.AreEqual("",ex.Message);
                
            }
            

            //---------------Test Result -----------------------
        }

    }
}
