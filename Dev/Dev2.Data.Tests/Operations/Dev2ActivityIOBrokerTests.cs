using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;
using Ionic.Zip;
using Ionic.Zlib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;



namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    public class Dev2ActivityIOBrokerTests_old
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_CreateInstance_GivenThrowsNoExpetion_ShouldBeIActivityOperationsBroker()
        {
            var activityOperationsBroker = CreateBroker();
            Assert.IsInstanceOfType(activityOperationsBroker, typeof(IActivityOperationsBroker));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_Get_GivenPath_ShouldReturnFileEncodingContents()
        {
            var activityOperationsBroker = CreateBroker();
            var fileMock = new Mock<IActivityIOOperationsEndPoint>();
            fileMock.Setup(point => point.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(new ByteBuffer(Encoding.ASCII.GetBytes("")));

            var stringEncodingContents = activityOperationsBroker.Get(fileMock.Object, true);
            Assert.IsNotNull(stringEncodingContents);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_Get_GivenPath_ShouldReturnFileDecodedContents()
        {
            var activityOperationsBroker = CreateBroker();
            var fileMock = new Mock<IActivityIOOperationsEndPoint>();

            const string iAmGood = "I am good";
            fileMock.Setup(point => point.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(new ByteBuffer(Encoding.ASCII.GetBytes(iAmGood)));

            var stringEncodingContents = activityOperationsBroker.Get(fileMock.Object, true);

            Assert.IsNotNull(stringEncodingContents);
            Assert.AreEqual(iAmGood, stringEncodingContents);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_IsBase64_GivenStartsWithBase64_ShouldReturnTrue()
        {
            var activityOperationsBroker = CreateBroker();
            var obj = new PrivateObject(activityOperationsBroker);

            var invoke = obj.Invoke("IsBase64", "Content-Type:BASE64SomeJunkdata");

            Assert.IsTrue(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName()
        {
            var activityOperationsBroker = CreateBroker();
            var obj = new PrivateType(activityOperationsBroker.GetType());
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);

            var pathReturned = obj.InvokeStatic("GetFileNameFromEndPoint", mockEndpoint.Object);

            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName_Overload()
        {
            var activityOperationsBroker = CreateBroker();
            var prType = new PrivateType(activityOperationsBroker.GetType());
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);

            var args = new object[]
            {
                mockEndpoint.Object, mockActIo.Object
            };
            var pathReturned = prType.InvokeStatic("GetFileNameFromEndPoint", args);

            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_ListDirectory_GivenFilesAndFolders_ShouldReturnEmptyList()
        {
            var activityOperationsBroker = CreateBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);

            var activityIOPaths = activityOperationsBroker.ListDirectory(endPoint.Object, ReadTypes.FilesAndFolders);

            Assert.AreEqual(0, activityIOPaths.Count);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_ListDirectory_GivenFiles_ShouldReturnEmptyList()
        {
            var activityOperationsBroker = CreateBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFilesInDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);

            var activityIOPaths = activityOperationsBroker.ListDirectory(endPoint.Object, ReadTypes.Files);

            Assert.AreEqual(0, activityIOPaths.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_ListDirectory_GivenFolders_ShouldReturnEmptyList()
        {
            var activityOperationsBroker = CreateBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFoldersInDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);

            var activityIOPaths = activityOperationsBroker.ListDirectory(endPoint.Object, ReadTypes.Folders);

            Assert.AreEqual(0, activityIOPaths.Count);
        }

        static IActivityOperationsBroker CreateBroker()
        {
            return ActivityIOFactory.CreateOperationsBroker();
        }

        static IActivityOperationsBroker CreateBroker(IFile file, ICommon common)
        {
            return ActivityIOFactory.CreateOperationsBroker(file, common);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_CreateDirectory_GivenValidInterfaces_ShouldCallsCreateDirectoryCorrectly()
        {
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            endPoint.Setup(o => o.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);

            var driver = new ActivityIOBrokerDriverBase();
            var result = driver.CreateDirectory(endPoint.Object, dev2CrudOperationTO);

            Assert.IsTrue(result);
            endPoint.Verify(o => o.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_ValidateEndPoint_GivenEmptyPath_ShouldThrowValidExc()
        {
            //---------------Set up test pack-------------------
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            endPoint.Setup(point => point.IOPath.Path).Returns("");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                var commonDataUtils = new Util.CommonDataUtils();
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
        public void Dev2ActivityIOBroker_ValidateEndPoint_GivenPathAndOverwriteFalse_ShouldThrowValidExc()
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
                var commonDataUtils = new Util.CommonDataUtils();
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
        public void Dev2ActivityIOBroker_RemoveTmpFile_GivenEmptyFile_ShouldThrowAndLogException()
        {
            //---------------Set up test pack-------------------
            var file = new Mock<IFile>();
            var activityOperationsBroker = CreateBroker(file.Object, new Util.CommonDataUtils());
            var privateObject = new PrivateObject(activityOperationsBroker);
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
        public void Dev2ActivityIOBroker_RemoveTmpFile_GivenFileFile_ShouldDeleteFile()
        {
            //---------------Set up test pack-------------------
            var file = new Mock<IFile>();
            file.Setup(file1 => file1.Delete(It.IsAny<string>()));
            var activityOperationsBroker = CreateBroker(file.Object, new Util.CommonDataUtils());
            var privateObject = new PrivateObject(activityOperationsBroker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            privateObject.Invoke("RemoveTmpFile", "SomePath");
            //---------------Test Result -----------------------
            file.Verify(file1 => file1.Delete(It.IsAny<string>()));

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_Create_GivenDestination_ShouldCreateFileCorrectly()
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
            var privateObject = new PrivateObject(activityOperationsBroker);
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
        public void Dev2ActivityIOBroker_Delete_GivenDeleteIsTrue_ShouldReturnResulOk()
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
        public void Dev2ActivityIOBroker_Delete_GivenDeleteIsFalse_ShouldReturnResulBad()
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
        public void Dev2ActivityIOBroker_ValidateUnzipSourceDestinationFileOperation_GivenPathNotFile_ShouldThrowValidExc()
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
            var privateObject = new PrivateObject(activityOperationsBroker);
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
        public void Dev2ActivityIOBroker_DoFileTransfer_GivenValidArgs_ShouldtransferCorrectly()
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
            var privateObject = new PrivateObject(activityOperationsBroker);
            var path = "";
            var result = false;
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
        public void Dev2ActivityIOBroker_Rename_GivenSoureAndDestinationDifferentPathType_ShouldThrowExc()
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
        public void Dev2ActivityIOBroker_Rename_GivenSoureAndDestinationSamePathTypePathExistsOverwriteFalse_ShouldThrowException()
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
        public void Dev2ActivityIOBroker_Rename_GivenSoureAndDestinationSamePathTypePathExistsOverwriteTrue_ShouldDeleteDestFile()
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
        public void Dev2ActivityIOBroker_ExtractFile_GivenGivenValidArgs_ShouldNotThrowExc()
        {
            //---------------Set up test pack-------------------
            var tempFileName = Path.GetTempFileName();
            var tempPath = Path.GetDirectoryName(tempFileName);
            var operationTO = new Dev2UnZipOperationTO("Password", false);
            var file = new ZipFile();
            file.AddFile(tempFileName);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                var commonDataUtils = new Util.CommonDataUtils();
                file.Save(tempFileName);
                commonDataUtils.ExtractFile(operationTO, new IonicZipFileWrapper(file), tempPath);
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
        public void Dev2ActivityIOBroker_MoveTmpFileToDestination_GiventmpFile_ShouldReturnSucces()
        {
            //---------------Set up test pack-------------------
            var tempFileName = Path.GetTempFileName();
            var commonMock = new Mock<ICommon>();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(file => file.ReadAllBytes(It.IsAny<string>())).Returns(Encoding.ASCII.GetBytes("Hello world"));
            var activityOperationsBroker = CreateBroker(fileMock.Object, commonMock.Object);
            var privateObject = new PrivateObject(activityOperationsBroker);
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
        public void Dev2ActivityIOBroker_EnsureFilesDontExists_GivenPathExistsAndPasthIsFile_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var tempFileName = Path.GetTempFileName();
            var tempSrcName = Path.GetTempFileName();
            var commonMock = new Mock<ICommon>();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(file => file.ReadAllBytes(It.IsAny<string>())).Returns(Encoding.ASCII.GetBytes("Hello world"));
            var activityOperationsBroker = CreateBroker(fileMock.Object, commonMock.Object);
            var privateObject = new PrivateObject(activityOperationsBroker);
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
    }
}
