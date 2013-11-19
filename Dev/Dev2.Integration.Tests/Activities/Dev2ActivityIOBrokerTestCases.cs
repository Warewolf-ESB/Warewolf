using System;
using System.IO;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.UnitTest.Framework.PathOperationTests;

namespace Dev2.Integration.Tests.Activities
{
    [TestClass]
    public class Dev2ActivityIOBrokerTestCases
    {
        #region Copy Unit Tests

        #region Copy Unit Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
                 (sourceEndPoint, destinationEndPoint) =>
                 {
                     var CopyTo = new Dev2CRUDOperationTO(overWrite);
                     return ActivityIOFactory.CreateOperationsBroker()
                                             .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
                 });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = true;

            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var CopyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var CopyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var CopyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
               (sourceEndPoint, destinationEndPoint) =>
               {
                   var CopyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunsFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunUncToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Copy")]
        public void Dev2ActivityIOBroker_Copy_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = true;
            RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
            {
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
            });
        }

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;
        //    const bool isRemoteDestination = true;
        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_RemoteToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;
        //    const bool isRemoteDestination = false;
        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunTestCase(isRemoteSource, isRemoteDestination, sourceFileName, destinationFile, overWrite, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_CreateEndPoint")]
        //public void Dev2ActivityIOBroker_CreateEndPoint_DestinationIsDirectoryEndsWithASlash_ReturnStatusIsSuccessful()
        //{
        //    //------------Setup for test--------------------------
        //    string destinationDirectory = Path.GetTempPath() + Guid.NewGuid();
        //    destinationDirectory = destinationDirectory + "\\";
        //    IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(destinationDirectory, string.Empty, null, true));
        //    var crudOps = new Dev2CRUDOperationTO(false);
        //    //------------Execute Test---------------------------
        //    var result = ActivityIOFactory.CreateOperationsBroker().CreateEndPoint(dstEndPoint, crudOps, false);
        //    //------------Assert Results-------------------------
        //    Assert.AreEqual("Success", result);
        //    Assert.IsTrue(Directory.Exists(destinationDirectory));
        //}

        #endregion
        #endregion

        #region Move Unit Tests
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2ActivityIOBroker_Move")]
        public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
        public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        {
            const bool overWrite = false;
            const string sourceFileName = "source.txt";
            const string destinationFile = "source.txt";
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = false;
            const bool createDestinationDirectory = false;
            const bool isDestinationADirectory = false;

            RunFtpToFtpTestCase( sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunLocalToLocalTestCase( sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool isSourceADirectory = true;
            const bool createDestinationDirectory = true;
            const bool isDestinationADirectory = true;

            RunFtpToFtpTestCase( sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase( sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = false;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToLocalTestCase(  sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = false;
            const bool isSourceADirectory = true;
            const bool isDestinationADirectory = true;
            RunFtpToFtpTestCase( sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
            const bool createSourceDirectory = true;
            const bool createDestinationDirectory = true;
            const bool isSourceADirectory = false;
            const bool isDestinationADirectory = false;
            RunFtpToFtpTestCase( sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = true;
            const bool isRemoteDestination = true;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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
            const bool isRemoteSource = false;
            const bool isRemoteDestination = false;
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

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;
        //    const bool isRemoteDestination = true;
        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_RemoteToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;
        //    const bool isRemoteDestination = false;
        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunTestCase(isRemoteSource, isRemoteDestination, sourceFileName, destinationFile, overWrite, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_CreateEndPoint")]
        //public void Dev2ActivityIOBroker_CreateEndPoint_DestinationIsDirectoryEndsWithASlash_ReturnStatusIsSuccessful()
        //{
        //    //------------Setup for test--------------------------
        //    string destinationDirectory = Path.GetTempPath() + Guid.NewGuid();
        //    destinationDirectory = destinationDirectory + "\\";
        //    IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(destinationDirectory, string.Empty, null, true));
        //    var crudOps = new Dev2CRUDOperationTO(false);
        //    //------------Execute Test---------------------------
        //    var result = ActivityIOFactory.CreateOperationsBroker().CreateEndPoint(dstEndPoint, crudOps, false);
        //    //------------Assert Results-------------------------
        //    Assert.AreEqual("Success", result);
        //    Assert.IsTrue(Directory.Exists(destinationDirectory));
        //}

        #endregion

        #region Test Cases

        private void RunLocalToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                             bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                             Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateLocalEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory,
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                Assert.IsTrue(Directory.Exists(destination.FilePath));
                var filename = destination.FilePath + @"\" + sourceFileName;
                var contents = File.ReadAllText(filename);
                Assert.IsTrue(contents.Contains(sourceData));
            }
            else
            {
                Assert.IsTrue(File.Exists(destination.FilePath));
                var contents = File.ReadAllText(destination.FilePath);
                Assert.IsTrue(contents.Contains(sourceData));
            }
        }

        private void RunFtpToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                          bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                          Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory,
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                Assert.IsTrue(Directory.Exists(destination.FilePath));
                var filename = destination.FilePath + @"\" + sourceFileName;
                var contents = File.ReadAllText(filename);
                Assert.IsTrue(contents.Contains(sourceData));
            }
            else
            {
                Assert.IsTrue(File.Exists(destination.FilePath));
                var contents = File.ReadAllText(destination.FilePath);
                Assert.IsTrue(contents.Contains(sourceData));
            }
        }

        private void RunsFtpToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                        bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                        Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory,
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                Assert.IsTrue(Directory.Exists(destination.FilePath));
                var filename = destination.FilePath + @"\" + sourceFileName;
                var contents = File.ReadAllText(filename);
                Assert.IsTrue(contents.Contains(sourceData));
            }
            else
            {
                Assert.IsTrue(File.Exists(destination.FilePath));
                var contents = File.ReadAllText(destination.FilePath);
                Assert.IsTrue(contents.Contains(sourceData));
            }
        }

        private void RunUncToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                     bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                     Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateUncEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory,
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                Assert.IsTrue(Directory.Exists(destination.FilePath));
                var filename = destination.FilePath + @"\" + sourceFileName;
                var contents = File.ReadAllText(filename);
                Assert.IsTrue(contents.Contains(sourceData));
            }
            else
            {
                Assert.IsTrue(File.Exists(destination.FilePath));
                var contents = File.ReadAllText(destination.FilePath);
                Assert.IsTrue(contents.Contains(sourceData));
            }
        }

        private void RunLocalToFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                           bool isSourceADirectory, bool createDestinationDirectory,
                                           bool isDestinationADirectory,
                                           Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                               actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateLocalEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
                                                             ParserStrings.PathOperations_Correct_Username,
                                                             ParserStrings.PathOperations_Correct_Password);
            Assert.IsTrue(response.Contains(sourceData));
            PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_Correct_Username,
                                         ParserStrings.PathOperations_Correct_Password, false);
        }

        private void RunFtpToFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                        bool isSourceADirectory, bool createDestinationDirectory,
                                        bool isDestinationADirectory,
                                        Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                            actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
                                                             ParserStrings.PathOperations_Correct_Username,
                                                             ParserStrings.PathOperations_Correct_Password);
            Assert.IsTrue(response.Contains(sourceData));
            PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_Correct_Username,
                                         ParserStrings.PathOperations_Correct_Password, false);
        }

        private void RunsFtpToFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                  bool isSourceADirectory, bool createDestinationDirectory,
                                  bool isDestinationADirectory,
                                  Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                      actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
                                                             ParserStrings.PathOperations_Correct_Username,
                                                             ParserStrings.PathOperations_Correct_Password);
            Assert.IsTrue(response.Contains(sourceData));
            PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_Correct_Username,
                                         ParserStrings.PathOperations_Correct_Password, false);
        }

        private void RunUncToFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                             bool isSourceADirectory, bool createDestinationDirectory,
                             bool isDestinationADirectory,
                             Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                 actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateUncEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
                                                             ParserStrings.PathOperations_Correct_Username,
                                                             ParserStrings.PathOperations_Correct_Password);
            Assert.IsTrue(response.Contains(sourceData));
            PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_Correct_Username,
                                         ParserStrings.PathOperations_Correct_Password, false);
        }

        private void RunLocalTosFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                          bool isSourceADirectory, bool createDestinationDirectory,
                          bool isDestinationADirectory,
                          Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                              actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateLocalEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreatesFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
                                                             ParserStrings.PathOperations_SFTP_Username,
                                                             ParserStrings.PathOperations_SFTP_Password);
            Assert.IsTrue(response.Contains(sourceData));
            PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_SFTP_Username,
                                         ParserStrings.PathOperations_SFTP_Password, false);
        }

        private void RunFtpTosFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                         bool isSourceADirectory, bool createDestinationDirectory,
                         bool isDestinationADirectory,
                         Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                             actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreatesFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
                                                             ParserStrings.PathOperations_SFTP_Username,
                                                             ParserStrings.PathOperations_SFTP_Password);
            Assert.IsTrue(response.Contains(sourceData));
            PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_SFTP_Username,
                                         ParserStrings.PathOperations_SFTP_Password, false);
        }

        private void RunsFtpTosFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                    bool isSourceADirectory, bool createDestinationDirectory,
                    bool isDestinationADirectory,
                    Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                        actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreatesFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
                                                             ParserStrings.PathOperations_SFTP_Username,
                                                             ParserStrings.PathOperations_SFTP_Password);
            Assert.IsTrue(response.Contains(sourceData));
            PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_SFTP_Username,
                                         ParserStrings.PathOperations_SFTP_Password, false);
        }

        private void RunUncTosFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                 bool isSourceADirectory, bool createDestinationDirectory,
                 bool isDestinationADirectory,
                 Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                     actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateUncEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
                                                             ParserStrings.PathOperations_SFTP_Username,
                                                             ParserStrings.PathOperations_SFTP_Password);
            Assert.IsTrue(response.Contains(sourceData));
            PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_SFTP_Username,
                                         ParserStrings.PathOperations_SFTP_Password, false);
        }

        private void RunLocalToUncTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                           bool isSourceADirectory, bool createDestinationDirectory,
                                           bool isDestinationADirectory,
                                           Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                                               actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateLocalEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory,
                                                    isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + @"\" + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            //string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
            //                                                 ParserStrings.PathOperations_Correct_Username,
            //                                                 ParserStrings.PathOperations_Correct_Password);

            var response = PathIOTestingUtils.ReadUNCFile(remoteFilePath);
            Assert.IsTrue(response.Contains(sourceData));
            File.Delete(remoteFilePath);
        }

        private void RunFtpToUncTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
             bool isSourceADirectory, bool createDestinationDirectory,
             bool isDestinationADirectory,
             Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                 actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + @"\" + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            //string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
            //                                                 ParserStrings.PathOperations_Correct_Username,
            //                                                 ParserStrings.PathOperations_Correct_Password);
            var response = PathIOTestingUtils.ReadUNCFile(remoteFilePath);
            Assert.IsTrue(response.Contains(sourceData));
            //PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_Correct_Username,
            //                             ParserStrings.PathOperations_Correct_Password, false);
        }

        private void RunsFtpToUncTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
          bool isSourceADirectory, bool createDestinationDirectory,
          bool isDestinationADirectory,
          Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
              actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            //string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
            //                                                 ParserStrings.PathOperations_Correct_Username,
            //                                                 ParserStrings.PathOperations_Correct_Password);
            var response = PathIOTestingUtils.ReadUNCFile(remoteFilePath);
            Assert.IsTrue(response.Contains(sourceData));
            //PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_Correct_Username,
            //                             ParserStrings.PathOperations_Correct_Password, false);
        }

        private void RunUncToUncTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
        bool isSourceADirectory, bool createDestinationDirectory,
        bool isDestinationADirectory,
        Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
            actionToPerform)
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateUncEndPoint(sourceFileName, sourceData, createSourceDirectory, isSourceADirectory);
            dynamic destination = CreateUncEndPoint(destinationFile, destinationData, createDestinationDirectory, isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);


            string remoteFilePath = "";

            if (isDestinationADirectory && string.IsNullOrEmpty(destinationFile))
            {
                remoteFilePath = destination.FilePath + sourceFileName;
            }
            else
            {
                remoteFilePath = destination.FilePath;
            }
            //string response = PathIOTestingUtils.ReadFileFTP(remoteFilePath,
            //                                                 ParserStrings.PathOperations_Correct_Username,
            //                                                 ParserStrings.PathOperations_Correct_Password);
            var response = PathIOTestingUtils.ReadUNCFile(remoteFilePath);
            Assert.IsTrue(response.Contains(sourceData));
            //PathIOTestingUtils.DeleteFTP(remoteFilePath, ParserStrings.PathOperations_Correct_Username,
            //                             ParserStrings.PathOperations_Correct_Password, false);
        }

        private dynamic CreateLocalEndPoint(string file, string data, bool createDirectory, bool isDirectory = false)
        {
            IActivityIOOperationsEndPoint dstEndPoint = null;
            string fileWithPath = string.Empty;

            string directory = Path.GetTempPath() + Guid.NewGuid();
            fileWithPath = Path.Combine(directory, file);

            if (createDirectory)
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(fileWithPath, data);
            }

            dstEndPoint =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    isDirectory ? directory : fileWithPath, string.Empty, null,
                    true));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        private dynamic CreateFtpEndPoint(string file, string data, bool createDirectory, bool isDirectory = false)
        {
            IActivityIOOperationsEndPoint dstEndPoint = null;
            string fileWithPath = string.Empty;

            string ftpSite = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/";
            fileWithPath = PathIOTestingUtils.CreateFileFTP(ftpSite, ParserStrings.PathOperations_Correct_Username,
                                                            ParserStrings.PathOperations_Correct_Password, false, file,
                                                            data, createDirectory);
            string path = (isDirectory && !string.IsNullOrEmpty(file)) ? fileWithPath.Replace(file, "") : fileWithPath;
            dstEndPoint =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    path, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        private dynamic CreatesFtpEndPoint(string file, string data, bool createDirectory, bool isDirectory = false)
        {
            IActivityIOOperationsEndPoint dstEndPoint = null;
            string fileWithPath = string.Empty;

            string ftpSite = ParserStrings.PathOperations_SFTP_Path + "/";
            fileWithPath = PathIOTestingUtils.CreateFilesFTP(ftpSite, ParserStrings.PathOperations_SFTP_Username,
                                                            ParserStrings.PathOperations_SFTP_Password, true, file,
                                                            data, createDirectory);
            string path = (isDirectory && !string.IsNullOrEmpty(file)) ? fileWithPath.Replace(file, "") : fileWithPath;
            dstEndPoint =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    path, ParserStrings.PathOperations_SFTP_Username, ParserStrings.PathOperations_SFTP_Password));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        private dynamic CreateUncEndPoint(string file, string data, bool createDirectory, bool isDirectory = false)
        {
            IActivityIOOperationsEndPoint dstEndPoint = null;
            string fileWithPath = string.Empty;

            string directory = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            fileWithPath = Path.Combine(directory, file);

            if (createDirectory)
            {
                PathIOTestingUtils.CreateAuthedUNCPath(directory,true);
                PathIOTestingUtils.CreateAuthedUNCPath(fileWithPath,data);
            }

            dstEndPoint =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    isDirectory ? directory : fileWithPath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        #endregion 
    }
}
