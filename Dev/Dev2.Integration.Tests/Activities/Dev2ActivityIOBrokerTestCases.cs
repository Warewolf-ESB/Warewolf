using System;
using System.IO;
using System.Reflection;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Activities
{
    [TestClass]
    public class Dev2ActivityIOBrokerTestCases
    {
        private static string _zipFile = "";
        [ClassInitialize]
        public static void Initializer(TestContext context)
        {
            const string ResourceName = "Dev2.Integration.Tests.TestData.Test.zip";
            string fileName = Path.GetTempPath() + "\\9999.zip";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using(Stream stream = assembly.GetManifestResourceStream(ResourceName))
            {
                if(stream != null)
                {
                    byte[] buf = new byte[stream.Length];  
                    stream.Read(buf, 0, buf.Length);
                    File.WriteAllBytes(fileName, buf);
                }
            }

            _zipFile = fileName;
        }

        #region Delete Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Delete")]
        public void Dev2ActivityIOBroker_Delete_FileLocally_FileExists_FileIsDeleted()
        {
            const string fileName = "source.txt";
            const bool createSourceFile = true;
            const string sourceData = "some ";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, createSourceFile, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                        .Delete(source.EndPoint);
            Assert.AreEqual("Success", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Delete")]
        public void Dev2ActivityIOBroker_Delete_FileLocally_FileDoesExists_FileIsDeleted()
        {
            const string fileName = "source.txt";
            const bool createSourceFile = false;
            const string sourceData = "some ";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, createSourceFile, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                        .Delete(source.EndPoint);
            Assert.AreEqual("Failure", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Delete")]
        public void Dev2ActivityIOBroker_Delete_FileOnUnc_FileExists_FileIsDeleted()
        {
            const string fileName = "source.txt";
            const bool createSourceFile = true;
            const string sourceData = "some ";
            dynamic source = CreateUncEndPoint(fileName, sourceData, createSourceFile, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                        .Delete(source.EndPoint);
            Assert.AreEqual("Success", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Delete")]
        public void Dev2ActivityIOBroker_Delete_FileOnUnc_FileDoesExists_FileIsDeleted()
        {
            const string fileName = "source.txt";
            const bool createSourceFile = false;
            const string sourceData = "some ";
            dynamic source = CreateUncEndPoint(fileName, sourceData, createSourceFile, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                        .Delete(source.EndPoint);
            Assert.AreEqual("Failure", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Delete")]
        public void Dev2ActivityIOBroker_Delete_FileOnFtp_FileExists_FileIsDeleted()
        {
            const string fileName = "source.txt";
            const bool createSourceFile = true;
            const string sourceData = "some ";
            dynamic source = CreateFtpEndPoint(fileName, sourceData, createSourceFile, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                        .Delete(source.EndPoint);
            Assert.AreEqual("Success", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Delete")]
        public void Dev2ActivityIOBroker_Delete_FileOnsFtp_FileExists_FileIsDeleted()
        {
            const string fileName = "source.txt";
            const bool createSourceFile = true;
            const string sourceData = "some ";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, createSourceFile, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                        .Delete(source.EndPoint);
            Assert.AreEqual("Success", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Delete")]
        public void Dev2ActivityIOBroker_Delete_FileOnsFtp_FileDoesExists_FileIsDeleted()
        {
            const string fileName = "source.txt";
            const bool createSourceFile = true;
            const string sourceData = "some ";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, createSourceFile, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                        .Delete(source.EndPoint);
            Assert.AreEqual("Success", result);
        }
        #endregion

        #region Zip Unit Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
                 (sourceEndPoint, destinationEndPoint) =>
                 {
                     var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                     return ActivityIOFactory.CreateOperationsBroker()
                                             .Zip(sourceEndPoint, destinationEndPoint, zipTo);
                 });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToFile_FileHasArchivePassword_UnZipWillRequireSamePassword()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFilename = "destination.zip";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, destinationFilename, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
                 (sourceEndPoint, destinationEndPoint) =>
                 {
                     const string archivePassword = "GgRest74@#$";
                     var zipTo = new Dev2ZipOperationTO("Best Speed", archivePassword, "", overWrite);
                     var result = ActivityIOFactory.CreateOperationsBroker()
                                             .Zip(sourceEndPoint, destinationEndPoint, zipTo);

                     if(result == "Success")
                     {
                         var unzipTo = new Dev2UnZipOperationTO(archivePassword, overWrite);
                         var destination = CreateLocalEndPoint(sourceFileName, "", false, "", true);
                         result = ActivityIOFactory.CreateOperationsBroker()
                                                 .UnZip(destinationEndPoint, destination.EndPoint, unzipTo);
                     }
                     else
                     {
                         throw new Exception("Zip operation failed");
                     }
                     return result;
                 });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToFile_FileHasArchivePassword_UnZipWithoutPasswordThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFilename = "destination.zip";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, destinationFilename, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
                 (sourceEndPoint, destinationEndPoint) =>
                 {
                     const string archivePassword = "GgRest74@#$";
                     var zipTo = new Dev2ZipOperationTO("Best Speed", archivePassword, "", overWrite);
                     var result = ActivityIOFactory.CreateOperationsBroker()
                                             .Zip(sourceEndPoint, destinationEndPoint, zipTo);

                     if(result == "Success")
                     {
                         var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                         var destination = CreateLocalEndPoint(sourceFileName, "", false, "", true);
                         result = ActivityIOFactory.CreateOperationsBroker()
                                                 .UnZip(destinationEndPoint, destination.EndPoint, unzipTo);
                     }
                     else
                     {
                         throw new Exception("Zip operation failed");
                     }
                     return result;
                 });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Zip(sourceEndPoint, destinationEndPoint, zipTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Zip(sourceEndPoint, destinationEndPoint, zipTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Zip(sourceEndPoint, destinationEndPoint, zipTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Zip(sourceEndPoint, destinationEndPoint, zipTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_sFTPTosFTP_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder/EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_FtpTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_FtpToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_FtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_FtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_sFtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_sFtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_UncTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Zip_UncToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_DestinationIsBlank_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "achivepwd", "", overWrite);
                destinationEndPoint.IOPath.Path = "";
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Zip")]
        public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.zip";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var zipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Zip(sourceEndPoint, destinationEndPoint, zipTo);
            });
        }

        #endregion

        #region Rename Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var renameTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, destinationEndPoint, renameTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_SourceAndDesinationFilesAreTheSame_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var copyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, sourceEndPoint, copyTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToLocal_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder\\EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpToFtp_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder/EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_sFTPTosFTP_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder/EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_UncToUnc_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder\\EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var renameTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, destinationEndPoint, renameTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var renameTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, destinationEndPoint, renameTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_UncToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var renameTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, destinationEndPoint, renameTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_UncToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunsFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunUncToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_FtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_UncToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_sFtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_FtpToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_FtpTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_FtpToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_UncToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_UncToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_LocalToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_sFtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_UncToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_FtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_sFtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_sFtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_sFtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_UncToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_UncTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Rename_UncToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_sFtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Rename")]
        public void Dev2ActivityIOBroker_Rename_LocalToUnc_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var renameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, renameTo);
            });
        }

        #endregion

        #region Copy Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
                 (sourceEndPoint, destinationEndPoint) =>
                 {
                     var copyTo = new Dev2CRUDOperationTO(overWrite);
                     return ActivityIOFactory.CreateOperationsBroker()
                                             .Copy(sourceEndPoint, destinationEndPoint, copyTo);
                 });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var copyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Copy(sourceEndPoint, destinationEndPoint, copyTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_SourceAndDesinationFilesAreTheSame_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var copyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Copy(sourceEndPoint, sourceEndPoint, copyTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var copyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Copy(sourceEndPoint, destinationEndPoint, copyTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var copyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Copy(sourceEndPoint, destinationEndPoint, copyTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var copyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Copy(sourceEndPoint, destinationEndPoint, copyTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunsFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunUncToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder\\EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder/EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_sFTPTosFTP_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder/EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_UncToUnc_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder\\EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_FtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_sFtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_sFtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_UncTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Copy_UncToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        #endregion

        #region Move Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
                 (sourceEndPoint, destinationEndPoint) =>
                 {
                     var moveTo = new Dev2CRUDOperationTO(overWrite);
                     return ActivityIOFactory.CreateOperationsBroker()
                                             .Move(sourceEndPoint, destinationEndPoint, moveTo);
                 });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var moveTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Move(sourceEndPoint, destinationEndPoint, moveTo);
               });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_SourceAndDesinationFilesAreTheSame_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var copyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Move(sourceEndPoint, sourceEndPoint, copyTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder\\EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpToFtp_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder/EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_sFTPTosFTP_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder/EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_UncToUnc_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var copyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder\\EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, copyTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var moveTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Move(sourceEndPoint, destinationEndPoint, moveTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var moveTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Move(sourceEndPoint, destinationEndPoint, moveTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var moveTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Move(sourceEndPoint, destinationEndPoint, moveTo);
               });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunsFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunUncToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_UncToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_UncToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_UncToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_FtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_sFtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_sFtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_UncToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_UncTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Move_UncToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_UncToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var moveTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Move(sourceEndPoint, destinationEndPoint, moveTo);
            });
        }

        #endregion

        #region UnZip Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
                 (sourceEndPoint, destinationEndPoint) =>
                 {
                     var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                     return ActivityIOFactory.CreateOperationsBroker()
                                             .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
                 }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_LocalToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_LocalToUnc_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_FtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_sFtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_UncToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_FtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_sFtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_UncToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_LocalToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_LocalToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_LocalTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_LocalToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_FtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_sFtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_UncToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_FtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_FtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_FtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_sFtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_sFtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_sFtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_UncToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_UncTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_UncToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_LocalToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_LocalToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_LocalTosFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_LocalToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_FtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_sFtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_UncToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_FtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_sFtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_UncToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_FtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_sFtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        public void Dev2ActivityIOBroker_UnZip_UncToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_SourceIsADirectory_ThrowsException()
        {
            const bool overWrite = true;
            const string sourceFileName = "source.txt";
            const string destinationFileName = "source.txt";

            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_UnZip")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_UnZip_DestinationIsAFile_ThrowsException()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";


            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var unzipTo = new Dev2UnZipOperationTO("", overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .UnZip(sourceEndPoint, destinationEndPoint, unzipTo);
            }, _zipFile);
        }

        #endregion

        #region Create Tests

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileLocally_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = false;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory,
                 sourceEndPoint =>
                 {
                     var createTo = new Dev2CRUDOperationTO(overWrite);
                     return ActivityIOFactory.CreateOperationsBroker()
                                             .Create(sourceEndPoint, createTo, false);
                 });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnFtp_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = false;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileUnc_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = false;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnsFtp_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = false;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryLocally_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryOnFtp_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryOnsFtp_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryOnUnc_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryLocally_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryOnFtp_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryOnUnc_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryOnsFtp_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryLocally_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = true;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryOnFtp_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = true;
            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryOnUnc_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = true;
            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }



        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_DirectoryOnsFtp_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = true;
            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileLocally_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileLocally_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnFtp_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnsFtp_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnUnc_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileLocally_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnFtp_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnsFtp_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnUnc_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnFtp_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnsFtp_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToDirectory_FileOnUnc_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, false);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileLocally_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = false;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory,
                 sourceEndPoint =>
                 {
                     var createTo = new Dev2CRUDOperationTO(overWrite);
                     return ActivityIOFactory.CreateOperationsBroker()
                                             .Create(sourceEndPoint, createTo, true);
                 });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnFtp_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = false;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileUnc_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = false;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnsFtp_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = false;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryLocally_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryOnFtp_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryOnsFtp_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryOnUnc_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryLocally_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryOnFtp_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryOnUnc_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryOnsFtp_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryLocally_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = true;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryOnFtp_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = true;
            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryOnUnc_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = true;
            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }



        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_DirectoryOnsFtp_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = false;
            const bool isSourceADirectory = true;
            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToFile_FileLocally_OverwriteIsFalse_DestinationExists_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToFile_FileLocally_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnFtp_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnsFtp_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnUnc_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        {
            const bool overWrite = false;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileLocally_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunLocalTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnFtp_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnsFtp_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnUnc_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnFtp_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunFtpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnsFtp_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunsFftpTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Create")]
        public void Dev2ActivityIOBroker_Create_ToFile_FileOnUnc_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        {
            const bool overWrite = true;
            const string fileName = "source.txt";
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;

            RunUncTestCase(fileName, createSourceDirectory, isSourceADirectory, sourceEndPoint =>
            {
                var createTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Create(sourceEndPoint, createTo, true);
            });
        }


        #endregion

        #region Write Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileLocally_WriteTypeAppendBottom_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some ";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.AppendBottom, "data");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "some data");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnFtp_WriteTypeAppendBottom_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some ";
            dynamic source = CreateFtpEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.AppendBottom, "data");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "some data");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnsFtp_WriteTypeAppendBottom_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some ";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.AppendBottom, "data");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "some data");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnUnc_WriteTypeAppendBottom_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some ";
            dynamic source = CreateUncEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.AppendBottom, "data");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "some data");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileLocally_WriteTypeAppendTop_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.AppendTop, "data ");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data some");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnFtp_WriteTypeAppendTop_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateFtpEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.AppendTop, "data ");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data some");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnsFtp_WriteTypeAppendTop_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.AppendTop, "data ");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data some");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnUnc_WriteTypeAppendTop_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateUncEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.AppendTop, "data ");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data some");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileLocally_WriteTypeOverwrite_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.Overwrite, "data ");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data ");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnFtp_WriteTypeOverwrite_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateFtpEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.Overwrite, "data ");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data ");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnsFtp_WriteTypeOverwrite_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.Overwrite, "data ");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data ");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnUnc_WriteTypeOverwrite_DestinationExists_DataIsAppended()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateUncEndPoint(fileName, sourceData, writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.Overwrite, "data ");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data ");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileLocally_DestinationFileDoesNotExist_ResultIsSuccessful()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = false;
            dynamic source = CreateLocalEndPoint(fileName, "", writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.Overwrite, "data");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnUnc_DestinationFileDoesNotExist_ResultIsSuccessful()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = false;
            dynamic source = CreateUncEndPoint(fileName, "", writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.Overwrite, "data");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnFtp_DestinationFileDoesNotExist_ResultIsSuccessful()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = false;
            dynamic source = CreateFtpEndPoint(fileName, "", writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.Overwrite, "data");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_WriteFile")]
        public void Dev2ActivityIOBroker_WriteFile_FileOnSFtp_DestinationFileDoesNotExist_ResultIsSuccessful()
        {
            const string fileName = "source.txt";
            const bool writeFileSourceDirectory = false;
            dynamic source = CreatesFtpEndPoint(fileName, "", writeFileSourceDirectory, "");
            var writeTo = new Dev2PutRawOperationTO(WriteType.Overwrite, "data");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .PutRaw(source.EndPoint, writeTo);
            Assert.AreEqual("Success", result);
            var data = PathIOTestingUtils.ReadFile(source.EndPoint);
            Assert.AreEqual(data, "data");
        }

        #endregion

        #region Read Files Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFile")]
        public void Dev2ActivityIOBroker_ReadFile_FileLocally_DestinationExists_ReadsSuccessfully()
        {
            const string fileName = "source.txt";
            const bool readFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, readFileSourceDirectory, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .Get(source.EndPoint);
            Assert.AreEqual(sourceData, result);
            source.EndPoint.Delete(source.EndPoint.IOPath);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFile")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_ReadFile_FileLocally_DestinationDoesNotExists_ThrowsException()
        {
            const string fileName = "source.txt";
            const bool readFileSourceDirectory = false;
            const string sourceData = "some";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, readFileSourceDirectory, "");
            ActivityIOFactory.CreateOperationsBroker()
                                             .Get(source.EndPoint);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFile")]
        public void Dev2ActivityIOBroker_ReadFile_FileOnUnc_DestinationExists_ReadsSuccessfully()
        {
            const string fileName = "source.txt";
            const bool readFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateUncEndPoint(fileName, sourceData, readFileSourceDirectory, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .Get(source.EndPoint);
            Assert.AreEqual(sourceData, result);
            source.EndPoint.Delete(source.EndPoint.IOPath);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFile")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_ReadFile_FileOnUnc_DestinationDoesNotExists_ThrowsException()
        {
            const string fileName = "source.txt";
            const bool readFileSourceDirectory = false;
            const string sourceData = "some";
            dynamic source = CreateUncEndPoint(fileName, sourceData, readFileSourceDirectory, "");
            ActivityIOFactory.CreateOperationsBroker()
                                             .Get(source.EndPoint);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFile")]
        public void Dev2ActivityIOBroker_ReadFile_FileOnFtp_DestinationExists_ReadsSuccessfully()
        {
            const string fileName = "source.txt";
            const bool readFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateFtpEndPoint(fileName, sourceData, readFileSourceDirectory, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .Get(source.EndPoint);
            Assert.AreEqual(sourceData, result);
            source.EndPoint.Delete(source.EndPoint.IOPath);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFile")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_ReadFile_FileOnFtp_DestinationDoesNotExists_ThrowsException()
        {
            const string fileName = "source.txt";
            const bool readFileSourceDirectory = false;
            const string sourceData = "some";
            dynamic source = CreateFtpEndPoint(fileName, sourceData, readFileSourceDirectory, "");
            ActivityIOFactory.CreateOperationsBroker()
                                             .Get(source.EndPoint);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFile")]
        public void Dev2ActivityIOBroker_ReadFile_FileOnsFtp_DestinationExists_ReadsSuccessfully()
        {
            const string fileName = "source.txt";
            const bool readFileSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, readFileSourceDirectory, "");
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .Get(source.EndPoint);
            Assert.AreEqual(sourceData, result);
            source.EndPoint.Delete(source.EndPoint.IOPath);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFile")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_ReadFile_FileOnsFtp_DestinationDoesNotExists_ThrowsException()
        {
            const string fileName = "source.txt";
            const bool readFileSourceDirectory = false;
            const string sourceData = "some";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, readFileSourceDirectory, "");
            ActivityIOFactory.CreateOperationsBroker()
                                             .Get(source.EndPoint);
        }
        #endregion

        #region Read Files Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFolder")]
        public void Dev2ActivityIOBroker_ReadFolder_FileLocally_DestinationExists_ReadsSuccessfully()
        {
            const string fileName = "source.txt";
            const bool readFolderSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, readFolderSourceDirectory, "", true);
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .ListDirectory(source.EndPoint, ReadTypes.Files);
            Assert.AreEqual(result.Count, 1);
            source.EndPoint.Delete(source.EndPoint.IOPath);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFolder")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_ReadFolder_FileLocally_DestinationDoesNotExists_ThrowsException()
        {
            const string fileName = "source.txt";
            const bool readFolderSourceDirectory = false;
            const string sourceData = "some";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, readFolderSourceDirectory, "", true);
            ActivityIOFactory.CreateOperationsBroker()
                                             .ListDirectory(source.EndPoint, ReadTypes.Files);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFolder")]
        public void Dev2ActivityIOBroker_ReadFolder_FileOnUnc_DestinationExists_ReadsSuccessfully()
        {
            const string fileName = "source.txt";
            const bool readFolderSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateUncEndPoint(fileName, sourceData, readFolderSourceDirectory, "", true);
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .ListDirectory(source.EndPoint, ReadTypes.Files);
            Assert.AreEqual(result.Count, 1);
            source.EndPoint.Delete(source.EndPoint.IOPath);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFolder")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_ReadFolder_FileOnUnc_DestinationDoesNotExists_ThrowsException()
        {
            const string fileName = "source.txt";
            const bool readFolderSourceDirectory = false;
            const string sourceData = "some";
            dynamic source = CreateUncEndPoint(fileName, sourceData, readFolderSourceDirectory, "", true);
            ActivityIOFactory.CreateOperationsBroker()
                                             .ListDirectory(source.EndPoint, ReadTypes.Files);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFolder")]
        public void Dev2ActivityIOBroker_ReadFolder_FileOnFtp_DestinationExists_ReadsSuccessfully()
        {
            const string fileName = "source.txt";
            const bool readFolderSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreateFtpEndPoint(fileName, sourceData, readFolderSourceDirectory, "", true);
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .ListDirectory(source.EndPoint, ReadTypes.Files);
            Assert.AreEqual(result.Count, 1);
            source.EndPoint.Delete(source.EndPoint.IOPath);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFolder")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_ReadFolder_FileOnFtp_DestinationDoesNotExists_ThrowsException()
        {
            const string fileName = "source.txt";
            const bool readFolderSourceDirectory = false;
            const string sourceData = "some";
            dynamic source = CreateFtpEndPoint(fileName, sourceData, readFolderSourceDirectory, "", true);
            ActivityIOFactory.CreateOperationsBroker()
                                             .ListDirectory(source.EndPoint, ReadTypes.Files);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFolder")]
        public void Dev2ActivityIOBroker_ReadFolder_FileOnsFtp_DestinationExists_ReadsSuccessfully()
        {
            const string fileName = "source.txt";
            const bool readFolderSourceDirectory = true;
            const string sourceData = "some";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, readFolderSourceDirectory, "", true);
            var result = ActivityIOFactory.CreateOperationsBroker()
                                             .ListDirectory(source.EndPoint, ReadTypes.Files);
            Assert.AreEqual(result.Count, 1);
            source.EndPoint.Delete(source.EndPoint.IOPath);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_ReadFolder")]
        [ExpectedException(typeof(Exception))]
        public void Dev2ActivityIOBroker_ReadFolder_FileOnsFtp_DestinationDoesNotExists_ThrowsException()
        {
            const string fileName = "source.txt";
            const bool readFolderSourceDirectory = false;
            const string sourceData = "some";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, readFolderSourceDirectory, "", true);
            ActivityIOFactory.CreateOperationsBroker()
                                             .ListDirectory(source.EndPoint, ReadTypes.Files);
        }
        #endregion

        #region Test Cases

        private void RunLocalToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                             bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                             Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateLocalEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory, "",
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunLocalTestCase(string fileName, bool createSourceDirectory, bool isDirectory,
                                        Func<IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            dynamic source = CreateLocalEndPoint(fileName, sourceData, createSourceDirectory, testFileName, isDirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint);
            //------------Assert Results-------------------------
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunFtpTestCase(string fileName, bool createSourceDirectory, bool isDirectory,
                                       Func<IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            dynamic source = CreateFtpEndPoint(fileName, sourceData, createSourceDirectory, testFileName, isDirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint);
            //------------Assert Results-------------------------
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunsFftpTestCase(string fileName, bool createSourceDirectory, bool isDirectory,
                                      Func<IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            dynamic source = CreatesFtpEndPoint(fileName, sourceData, createSourceDirectory, testFileName, isDirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint);
            //------------Assert Results-------------------------
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunUncTestCase(string fileName, bool createSourceDirectory, bool isDirectory,
                                     Func<IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            dynamic source = CreateUncEndPoint(fileName, sourceData, createSourceDirectory, testFileName, isDirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint);
            //------------Assert Results-------------------------
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunFtpToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                          bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                          Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory, "",
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunsFtpToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                        bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                        Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory, "",
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunUncToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                     bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                     Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateUncEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory, "",
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunLocalToFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                           bool isSourceADirectory, bool createDestinationDirectory,
                                           bool isDestinationADirectory,
                                           Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                               actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateLocalEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName,
                                                 isSourceADirectory);
            dynamic destination = CreateFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, "",
                                                    isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunFtpToFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                        bool isSourceADirectory, bool createDestinationDirectory,
                                        bool isDestinationADirectory,
                                        Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                            actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2FTPProvider_Timeout")]
        public void Dev2FTPProvider_Timeout_ShouldTimeoutTimely()
        {
            //------------Setup for test--------------------------
            string ftpSite = ParserStrings.PathOperations_SFTP_Path + "/";
            IActivityIOPath pathFromString = ActivityIOFactory.CreatePathFromString(ftpSite, ParserStrings.PathOperations_SFTP_Username,
                                                                                            ParserStrings.PathOperations_SFTP_Password);
            IActivityIOOperationsEndPoint FTPPro =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(pathFromString);
            //------------Execute Test---------------------------
            var start = DateTime.Now;
            try
            {
                FTPPro.Get(pathFromString);
            }
            catch(Exception)
            {
                var end = DateTime.Now;
                //------------Assert Results-------------------------
                var timeToTimeout = end - start;
                var inRange = timeToTimeout.TotalSeconds <= 6;
                Assert.IsTrue(inRange, string.Format("Actual timeout: {0}", timeToTimeout.TotalSeconds));
            }

        }

        private void RunsFtpToFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                  bool isSourceADirectory, bool createDestinationDirectory,
                                  bool isDestinationADirectory,
                                  Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                      actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunUncToFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                             bool isSourceADirectory, bool createDestinationDirectory,
                             bool isDestinationADirectory,
                             Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                 actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateUncEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunLocalTosFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                          bool isSourceADirectory, bool createDestinationDirectory,
                          bool isDestinationADirectory,
                          Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                              actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateLocalEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreatesFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        static void CleanSource(dynamic source)
        {
            try
            {
                source.EndPoint.Delete(source.EndPoint.IOPath);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {

            }
        }

        static void CleanDestination(dynamic destination)
        {
            try
            {
                destination.EndPoint.Delete(destination.EndPoint.IOPath);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        private void RunFtpTosFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                         bool isSourceADirectory, bool createDestinationDirectory,
                         bool isDestinationADirectory,
                         Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                             actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreatesFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunsFtpTosFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                    bool isSourceADirectory, bool createDestinationDirectory,
                    bool isDestinationADirectory,
                    Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                        actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreatesFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunUncTosFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                 bool isSourceADirectory, bool createDestinationDirectory,
                 bool isDestinationADirectory,
                 Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                     actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateUncEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunLocalToUncTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                           bool isSourceADirectory, bool createDestinationDirectory,
                                           bool isDestinationADirectory,
                                           Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                               actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateLocalEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory, "",
                                                    isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunFtpToUncTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
             bool isSourceADirectory, bool createDestinationDirectory,
             bool isDestinationADirectory,
             Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                 actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunsFtpToUncTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
          bool isSourceADirectory, bool createDestinationDirectory,
          bool isDestinationADirectory,
          Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
              actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private void RunUncToUncTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
        bool isSourceADirectory, bool createDestinationDirectory,
        bool isDestinationADirectory,
        Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
            actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateUncEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            CleanDestination(destination);
            CleanSource(source);
            Assert.AreEqual("Success", result);

        }

        private dynamic CreateLocalEndPoint(string file, string data, bool createDirectory, string testFile, bool isDirectory = false)
        {
            string directory = Path.GetTempPath() + Guid.NewGuid();
            string fileWithPath = Path.Combine(directory, file);

            if(createDirectory)
            {
                Directory.CreateDirectory(directory);

                if(string.IsNullOrEmpty(testFile))
                {
                    File.WriteAllText(fileWithPath, data);
                }
                else
                {
                    File.Copy(testFile, fileWithPath);
                }
            }

            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    isDirectory ? directory : fileWithPath, string.Empty, null,
                    true));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        private dynamic CreateFtpEndPoint(string file, string data, bool createDirectory, string testFile, bool isDirectory = false)
        {
            string ftpSite = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/";
            string fileWithPath = PathIOTestingUtils.CreateFileFTP(ftpSite, ParserStrings.PathOperations_Correct_Username,
                                                            ParserStrings.PathOperations_Correct_Password, false, file,
                                                            data, createDirectory, testFile);
            string path = (isDirectory && !string.IsNullOrEmpty(file)) ? fileWithPath.Replace(file, "") : fileWithPath;
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    path, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        private dynamic CreatesFtpEndPoint(string file, string data, bool createDirectory, string testFile, bool isDirectory = false)
        {
            string ftpSite = ParserStrings.PathOperations_SFTP_Path + "/";
            string fileWithPath = PathIOTestingUtils.CreateFilesFTP(ftpSite, ParserStrings.PathOperations_SFTP_Username,
                                                            ParserStrings.PathOperations_SFTP_Password, true, file,
                                                            data, createDirectory, testFile);
            string path = (isDirectory && !string.IsNullOrEmpty(file)) ? fileWithPath.Replace(file, "") : fileWithPath;
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    path, ParserStrings.PathOperations_SFTP_Username, ParserStrings.PathOperations_SFTP_Password));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        private dynamic CreateUncEndPoint(string file, string data, bool createDirectory, string testFile, bool isDirectory = false)
        {
            string directory = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            string fileWithPath = Path.Combine(directory, file);

            if(createDirectory)
            {
                PathIOTestingUtils.CreateAuthedUNCPath(directory, true);
                PathIOTestingUtils.CreateAuthedUNCPath(fileWithPath, data, false, testFile);
            }

            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    isDirectory ? directory : fileWithPath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        #endregion
    }
}
