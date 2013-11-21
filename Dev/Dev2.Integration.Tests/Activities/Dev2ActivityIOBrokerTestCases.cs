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
        private string _zipFile = "";
        [TestInitialize]
        public void Initializer()
        {
            _zipFile  = Path.Combine(Environment.CurrentDirectory, @"TestData\Test.zip");
        }

        //#region Zip Unit Tests
        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //         (sourceEndPoint, destinationEndPoint) =>
        //         {
        //             var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //             return ActivityIOFactory.CreateOperationsBroker()
        //                                     .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //         });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_SourceAndDesinationFilesAreTheSame_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Zip(sourceEndPoint, sourceEndPoint, ZipTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_sFTPTosFTP_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
        //                                                       "DeeperFolder/EvenDeeperFolder");
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_FtpTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_FtpToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_FtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_FtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_sFtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_sFtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_UncTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Zip_UncToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_sFtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_UncToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalTosFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_LocalToUnc_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Zip")]
        //public void Dev2ActivityIOBroker_Zip_FtpToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.zip";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var ZipTo = new Dev2ZipOperationTO("Best Speed", "", "", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Zip(sourceEndPoint, destinationEndPoint, ZipTo);
        //    });
        //}

        //#endregion

        #region Rename Unit Tests
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
                   var RenameTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                   var CopyTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, sourceEndPoint, CopyTo);
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
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder\\EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, CopyTo);
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
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder/EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, CopyTo);
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
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder/EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, CopyTo);
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
                var CopyTo = new Dev2CRUDOperationTO(overWrite);
                destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
                                                               "DeeperFolder\\EvenDeeperFolder");
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, CopyTo);
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
                   var RenameTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                   var RenameTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                   var RenameTo = new Dev2CRUDOperationTO(overWrite);
                   return ActivityIOFactory.CreateOperationsBroker()
                                           .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
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
                var RenameTo = new Dev2CRUDOperationTO(overWrite);
                return ActivityIOFactory.CreateOperationsBroker()
                                        .Rename(sourceEndPoint, destinationEndPoint, RenameTo);
            });
        }

        #endregion

        //#region Copy Unit Tests
        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //         (sourceEndPoint, destinationEndPoint) =>
        //         {
        //             var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //             return ActivityIOFactory.CreateOperationsBroker()
        //                                     .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //         });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_SourceAndDesinationFilesAreTheSame_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Copy(sourceEndPoint, sourceEndPoint, CopyTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
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
        //public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunUncToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
        //                                                       "DeeperFolder\\EvenDeeperFolder");
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
        //                                                       "DeeperFolder/EvenDeeperFolder");
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_sFTPTosFTP_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
        //                                                       "DeeperFolder/EvenDeeperFolder");
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_UncToUnc_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
        //                                                       "DeeperFolder\\EvenDeeperFolder");
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_sFtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

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
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
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
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_FtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_sFtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_sFtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_UncTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Copy_UncToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
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
        //public void Dev2ActivityIOBroker_Copy_sFtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_sFtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_sFtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_UncToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalTosFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_LocalToUnc_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Copy(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Copy")]
        //public void Dev2ActivityIOBroker_Copy_FtpToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

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


        ////[TestMethod]
        ////[Owner("Tshepo Ntlhokoa")]
        ////[TestCategory("Dev2ActivityIOBroker_CreateEndPoint")]
        ////public void Dev2ActivityIOBroker_CreateEndPoint_DestinationIsDirectoryEndsWithASlash_ReturnStatusIsSuccessful()
        ////{
        ////    //------------Setup for test--------------------------
        ////    string destinationDirectory = Path.GetTempPath() + Guid.NewGuid();
        ////    destinationDirectory = destinationDirectory + "\\";
        ////    IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(destinationDirectory, string.Empty, null, true));
        ////    var crudOps = new Dev2CRUDOperationTO(false);
        ////    //------------Execute Test---------------------------
        ////    var result = ActivityIOFactory.CreateOperationsBroker().CreateEndPoint(dstEndPoint, crudOps, false);
        ////    //------------Assert Results-------------------------
        ////    Assert.AreEqual("Success", result);
        ////    Assert.IsTrue(Directory.Exists(destinationDirectory));
        ////}

        //#endregion

        //#region Move Unit Tests
        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //         (sourceEndPoint, destinationEndPoint) =>
        //         {
        //             var moveTo = new Dev2CRUDOperationTO(overWrite);
        //             return ActivityIOFactory.CreateOperationsBroker()
        //                                     .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //         });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var moveTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //       });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_SourceAndDesinationFilesAreTheSame_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Move(sourceEndPoint, sourceEndPoint, CopyTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
        //                                                       "DeeperFolder\\EvenDeeperFolder");
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
        //                                                       "DeeperFolder/EvenDeeperFolder");
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_sFTPTosFTP_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
        //                                                       "DeeperFolder/EvenDeeperFolder");
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_UncToUnc_DestinationIsAChildOfSource_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var CopyTo = new Dev2CRUDOperationTO(overWrite);
        //        destinationEndPoint.IOPath.Path = Path.Combine(sourceEndPoint.IOPath.Path,
        //                                                       "DeeperFolder\\EvenDeeperFolder");
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, CopyTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var moveTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var moveTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToLocal_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //       (sourceEndPoint, destinationEndPoint) =>
        //       {
        //           var moveTo = new Dev2CRUDOperationTO(overWrite);
        //           return ActivityIOFactory.CreateOperationsBroker()
        //                                   .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //       });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
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
        //public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunUncToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isDestinationADirectory = true;

        //    RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_sFtpToFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToUnc_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpTosFtp_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToLocal_DirectoryToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_sFtpToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToLocal_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalTosFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToUnc_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}


        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_sFtpToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToFtp_DirectoryToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = false;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_UncToLocal_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpTosFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpToUnc_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

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
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_UncToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToFile_OverwriteIsFalse_DestinationExists_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_UncToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
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
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_FtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_sFtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_sFtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_UncToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_UncTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_Move_UncToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
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
        //public void Dev2ActivityIOBroker_Move_sFtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_sFtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_FtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_sFtpToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_UncToLocal_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalTosFtp_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_Move")]
        //public void Dev2ActivityIOBroker_Move_LocalToUnc_FileToFile_OverwriteIsTrue_DestinationExists_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = false;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var moveTo = new Dev2CRUDOperationTO(overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .Move(sourceEndPoint, destinationEndPoint, moveTo);
        //    });
        //}

        //#endregion

        //#region UnZip Unit Tests
        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_LocalToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory,
        //         (sourceEndPoint, destinationEndPoint) =>
        //         {
        //             var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //             return ActivityIOFactory.CreateOperationsBroker()
        //                                     .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //         }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_LocalToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        ////[TestMethod]
        ////[Owner("Tshepo Ntlhokoa")]
        ////[TestCategory("Dev2ActivityIOBroker_UnZip")]
        ////public void Dev2ActivityIOBroker_UnZip_LocalTosFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        ////{
        ////    const bool overWrite = false;
        ////    const string sourceFileName = "source.txt";


        ////    const bool createSourceDirectory = true;
        ////    const bool isSourceADirectory = false;
        ////    const bool createDestinationDirectory = false;
        ////    const bool isDestinationADirectory = true;

        ////    RunLocalTosFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        ////    {
        ////        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        ////        return ActivityIOFactory.CreateOperationsBroker()
        ////                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        ////    }, _zipFile);
        ////}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_LocalToUnc_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunLocalToUncTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_FtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_sFtpToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_UncToFtp_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunUncToFtpTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_FtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_sFtpToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunsFtpToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_UncToLocal_FileToDirectory_OverwriteIsFalse_DestinationDoesNotExist_ResultIsSuccessful()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const bool isRemoteSource = true;

        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = true;

        //    RunUncToLocalTestCase(sourceFileName, "", createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_LocalToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_LocalToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_LocalTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_LocalToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_FtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_sFtpToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_UncToLocal_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_FtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_FtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_FtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //[Ignore]
        //public void Dev2ActivityIOBroker_UnZip_sFtpToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_sFtpTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_sFtpToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_UncToFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_UncTosFtp_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_UncToUnc_FileToDirectory_OverwriteIsFalse_FileExistsOnDestination_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_LocalToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_LocalToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_LocalTosFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_LocalToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunLocalToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_FtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_sFtpToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_UncToLocal_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToLocalTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_FtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_sFtpToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_UncToFtp_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_FtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_sFtpToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunsFtpToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //public void Dev2ActivityIOBroker_UnZip_UncToUnc_FileToDirectory_OverwriteIsTrue_FileExistsOnDestination_ResultIsSuccessful()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool isDestinationADirectory = true;
        //    RunUncToUncTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_SourceIsADirectory_ThrowsException()
        //{
        //    const bool overWrite = true;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFileName = "source.txt";

        //    const bool createSourceDirectory = true;
        //    const bool createDestinationDirectory = true;
        //    const bool isSourceADirectory = true;
        //    const bool isDestinationADirectory = true;
        //    RunFtpToFtpTestCase(sourceFileName, destinationFileName, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //[TestMethod]
        //[Owner("Tshepo Ntlhokoa")]
        //[TestCategory("Dev2ActivityIOBroker_UnZip")]
        //[ExpectedException(typeof(Exception))]
        //public void Dev2ActivityIOBroker_UnZip_DestinationIsAFile_ThrowsException()
        //{
        //    const bool overWrite = false;
        //    const string sourceFileName = "source.txt";
        //    const string destinationFile = "source.txt";


        //    const bool createSourceDirectory = true;
        //    const bool isSourceADirectory = false;
        //    const bool createDestinationDirectory = false;
        //    const bool isDestinationADirectory = false;

        //    RunLocalTosFtpTestCase(sourceFileName, destinationFile, createSourceDirectory, isSourceADirectory, createDestinationDirectory, isDestinationADirectory, (sourceEndPoint, destinationEndPoint) =>
        //    {
        //        var UnZipTo = new Dev2UnZipOperationTO("", overWrite);
        //        return ActivityIOFactory.CreateOperationsBroker()
        //                                .UnZip(sourceEndPoint, destinationEndPoint, UnZipTo);
        //    }, _zipFile);
        //}

        //#endregion

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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
        }

        private void RunFtpToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                          bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                          Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreateFtpEndPoint(sourceFileName, sourceData, createSourceDirectory,testFileName, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory, "",
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
        }

        private void RunsFtpToLocalTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                                        bool isSourceADirectory, bool createDestinationDirectory, bool isDestinationADirectory,
                                        Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string> actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory, testFileName, isSourceADirectory);
            dynamic destination = CreateLocalEndPoint(destinationFile, destinationData, createDestinationDirectory,"",
                                                      isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
        }

        private void RunsFtpTosFtpTestCase(string sourceFileName, string destinationFile, bool createSourceDirectory,
                    bool isSourceADirectory, bool createDestinationDirectory,
                    bool isDestinationADirectory,
                    Func<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, string>
                        actionToPerform, string testFileName = "")
        {
            const string sourceData = "some source string data";
            const string destinationData = "some destination string data";
            dynamic source = CreatesFtpEndPoint(sourceFileName, sourceData, createSourceDirectory,testFileName, isSourceADirectory);
            dynamic destination = CreatesFtpEndPoint(destinationFile, destinationData, createDestinationDirectory, "", isDestinationADirectory);
            //------------Execute Test---------------------------

            var result = actionToPerform(source.EndPoint, destination.EndPoint);
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
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
            Assert.AreEqual("Success", result);
            destination.EndPoint.Delete(destination.EndPoint.IOPath);
        }

        private dynamic CreateLocalEndPoint(string file, string data, bool createDirectory,string testFile, bool isDirectory = false)
        {
            IActivityIOOperationsEndPoint dstEndPoint = null;
            string fileWithPath = string.Empty;

            string directory = Path.GetTempPath() + Guid.NewGuid();
            fileWithPath = Path.Combine(directory, file);

            if (createDirectory)
            {
                Directory.CreateDirectory(directory);

                if (string.IsNullOrEmpty(testFile))
                {
                    File.WriteAllText(fileWithPath, data);
                }
                else
                {
                    File.Copy(testFile, fileWithPath);
                }
            }

            dstEndPoint =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    isDirectory ? directory : fileWithPath, string.Empty, null,
                    true));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        private dynamic CreateFtpEndPoint(string file, string data, bool createDirectory,string testFile, bool isDirectory = false)
        {
            IActivityIOOperationsEndPoint dstEndPoint = null;
            string fileWithPath = string.Empty;

            string ftpSite = ParserStrings.PathOperations_FTP_Auth + "PUT_DATA/";
            fileWithPath = PathIOTestingUtils.CreateFileFTP(ftpSite, ParserStrings.PathOperations_Correct_Username,
                                                            ParserStrings.PathOperations_Correct_Password, false, file,
                                                            data, createDirectory, testFile);
            string path = (isDirectory && !string.IsNullOrEmpty(file)) ? fileWithPath.Replace(file, "") : fileWithPath;
            dstEndPoint =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    path, ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        private dynamic CreatesFtpEndPoint(string file, string data, bool createDirectory,string testFile, bool isDirectory = false)
        {
            IActivityIOOperationsEndPoint dstEndPoint = null;
            string fileWithPath = string.Empty;

            string ftpSite = ParserStrings.PathOperations_SFTP_Path + "/";
            fileWithPath = PathIOTestingUtils.CreateFilesFTP(ftpSite, ParserStrings.PathOperations_SFTP_Username,
                                                            ParserStrings.PathOperations_SFTP_Password, true, file,
                                                            data, createDirectory, testFile);
            string path = (isDirectory && !string.IsNullOrEmpty(file)) ? fileWithPath.Replace(file, "") : fileWithPath;
            dstEndPoint =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    path, ParserStrings.PathOperations_SFTP_Username, ParserStrings.PathOperations_SFTP_Password));

            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        private dynamic CreateUncEndPoint(string file, string data, bool createDirectory,string testFile, bool isDirectory = false)
        {
            IActivityIOOperationsEndPoint dstEndPoint = null;
            string fileWithPath = string.Empty;

            string directory = TestResource.PathOperations_UNC_Path_Secure + Guid.NewGuid();
            fileWithPath = Path.Combine(directory, file);

            if (createDirectory)
            {
                PathIOTestingUtils.CreateAuthedUNCPath(directory,true);
                PathIOTestingUtils.CreateAuthedUNCPath(fileWithPath, data, false, testFile);
            }

            dstEndPoint =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(
                    isDirectory ? directory : fileWithPath, "DEV2\\" + TestResource.PathOperations_Correct_Username, TestResource.PathOperations_Correct_Password));
            
            return new { EndPoint = dstEndPoint, FilePath = fileWithPath };
        }

        #endregion 
    }
}
