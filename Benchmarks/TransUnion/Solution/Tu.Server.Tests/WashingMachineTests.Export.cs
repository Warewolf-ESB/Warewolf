using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Extensions;
using Tu.Imports;
using Tu.Servers;
using Tu.Washing;

namespace Tu.Server.Tests
{
    [TestClass]
    public partial class WashingMachineTests
    {
        [TestMethod]
        [TestCategory("WashingMachine_Export")]
        [Description("WashingMachine Export feches data dump from SQL server.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ExportDataDump_FetchesDataFromSqlServer()
        {
            var dataTable = CreateTestData();

            string actualCommandText = null;

            var sqlServer = new Mock<ISqlServer>();
            sqlServer.Setup(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>()))
                .Callback((string commandText, CommandType commandType, SqlParameter[] parameters) =>
                {
                    actualCommandText = commandText;
                })
                .Returns(dataTable);

            var emailServer = new Mock<IEmailServer>();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns("xxxx");

            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, new Mock<IFileServer>().Object);
            machine.Export();

            sqlServer.Verify(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>()), "DataDump not fetched from SQL Server.");

            Assert.AreEqual(WashingMachine.FetchDataCommandText, actualCommandText, "DataDump not fetched using correct stored procedure.");
        }

        [TestMethod]
        [TestCategory("WashingMachine_Export")]
        [Description("WashingMachine Export sends data dump email.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ExportDataDump_SendsEmail()
        {
            var dataTable = CreateTestData();

            // We expect all rows from original 2 data rows in data table 
            const int ExpectedContentsCount = 3; // includes header row

            var sqlServer = new Mock<ISqlServer>();
            sqlServer.Setup(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>())).Returns(dataTable);

            MailMessage message = null;
            var emailClientHitCount = 0;

            var emailServer = new Mock<IEmailServer>();
            emailServer.Setup(e => e.Send(It.IsAny<MailMessage>())).Callback((MailMessage msg) =>
            {
                if(emailClientHitCount++ == 0) // testing first email that gets sent
                {
                    message = msg;
                }
            }).Verifiable();

            var ftpServer = new Mock<IFtpServer>();
            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, new Mock<IFileServer>().Object);
            machine.Export();

            emailServer.Verify(e => e.Send(It.IsAny<MailMessage>()), "DataDumpEmail was not sent.");

            var expectedBody = string.Format("{0:yyyyMMdd}_QLINK_Fails_datadump.csv please is about to be processed from The Unlimited. Record count : {1}.", DateTime.Now, 2);
            Assert.AreEqual(expectedBody, message.Body, "DataDumpEmail not to sent with the correct body.");

            var expectedRecipients = new[] { "travis.frisinger@dev2.co.za", "trevor.williams-ros@dev2.co.za" };
            var actualRecipients = message.To.ToArray().Select(a => a.Address);
            Assert.IsTrue(expectedRecipients.SequenceEqual(actualRecipients));

            Assert.AreEqual(1, message.Attachments.Count, "DataDumpEmail attachment is missing.");

            var contents = ReadAttachment(message.Attachments[0]);

            Assert.IsTrue(contents.Count >= 1, "DataDumpEmail attachment must include a header row.");
            Assert.AreEqual("GovID|FirstNames|LastName|DateLastContactInfoCleaned", contents[0], "DataDumpEmail attachment included wrong header row.");
            Assert.AreEqual(ExpectedContentsCount, contents.Count, "DataDumpEmail attachment did not add all data table rows to output.");

            for(var i = 1; i < contents.Count; i++)
            {
                var fields = contents[i].Split('|');
                Assert.AreEqual(4, fields.Length, "DataDumpEmail did not add correct number of fields.");
            }
        }

        [TestMethod]
        [TestCategory("WashingMachine_Export")]
        [Description("WashingMachine Export uploads csv to FTP server.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ExportCsv_UploadsToFtpServer()
        {
            var dataTable = CreateTestData();

            var sqlServer = new Mock<ISqlServer>();
            sqlServer.Setup(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>())).Returns(dataTable);

            var emailServer = new Mock<IEmailServer>();

            string fileName = null;
            string ftpData = null;

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Upload(It.IsAny<string>(), It.IsAny<string>())).Callback((string relativeUri, string data) =>
            {
                fileName = relativeUri;
                ftpData = data;
            }).Returns(true).Verifiable();

            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, new Mock<IFileServer>().Object);
            machine.Export();

            ftpServer.Verify(e => e.Upload(It.IsAny<string>(), It.IsAny<string>()), "Csv was not uploaded to FTP sever.");

            Assert.IsNotNull(ftpData, "Csv content was not sent to FTP sever.");

            var contents = ftpData.ToLines();

            VerifyExportFile(fileName, contents, "Transunion/Auto Input");
        }

        [TestMethod]
        [TestCategory("WashingMachine_Export")]
        [Description("WashingMachine Export sends csv email.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ExportCsv_SendsEmail()
        {
            var dataTable = CreateTestData();

            var sqlServer = new Mock<ISqlServer>();
            sqlServer.Setup(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>())).Returns(dataTable);

            MailMessage message = null;
            var emailClientHitCount = 0;

            var emailServer = new Mock<IEmailServer>();
            emailServer.Setup(e => e.Send(It.IsAny<MailMessage>())).Callback((MailMessage msg) =>
            {
                if(emailClientHitCount++ == 1) // testing second email that gets sent
                {
                    message = msg;
                }
            }).Verifiable();

            var ftpServer = new Mock<IFtpServer>();

            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, new Mock<IFileServer>().Object);
            machine.Export();

            emailServer.Verify(e => e.Send(It.IsAny<MailMessage>()), Times.Exactly(2), "Request Notification was not sent.");

            var expectedBody = string.Format("{0:yyyyMMdd}_QLINK_Fails_dataclean.csv is ready for data cleaning from The Unlimited. Record count : {1}.", DateTime.Now, 1);

            Assert.AreEqual(expectedBody, message.Body, "Request Notification not to sent with the correct body.");
            Assert.AreEqual("datawash@dev2.co.za", message.From.Address, "Request Notification not to sent from correct address.");
            Assert.AreEqual("New File for Data Cleaning", message.Subject, "Request Notification not to sent with the correct subject.");

            var expectedRecipients = new[] { "travis.frisinger@dev2.co.za", "trevor.williams-ros@dev2.co.za" };
            var actualRecipients = message.To.ToArray().Select(a => a.Address);
            Assert.IsTrue(expectedRecipients.SequenceEqual(actualRecipients), "Request Notification not to sent to correct recipients");

            Assert.AreEqual(1, message.Attachments.Count, "Request Notification attachment is missing.");

            var attachment = message.Attachments[0];
            var contents = ReadAttachment(attachment);

            VerifyExportFile(attachment.ContentType.Name, contents);
        }

        [TestMethod]
        [TestCategory("WashingMachine_Export")]
        [Description("WashingMachine Export saves row count to local file system.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ExportCsv_SavesRowCountToTimestampedFile()
        {
            var dataTable = CreateTestData();

            // We expect one row to be filtered out from original 2 data rows for rule ... DateLastContactInfoCleaned > 90
            const int ExpectedContentsCount = 1; // excludes header row

            var expectedFileName = string.Format("{0:yyyyMMdd}.txt", DateTime.Now);

            var sqlServer = new Mock<ISqlServer>();
            sqlServer.Setup(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>())).Returns(dataTable);

            var emailServer = new Mock<IEmailServer>();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns("xxxx");

            string actualFileName = null;
            string actualFileContents = null;

            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string fileName, string contents) =>
                {
                    actualFileName = fileName;
                    actualFileContents = contents;
                }).Verifiable();

            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, localFileServer.Object);
            machine.Export();

            localFileServer.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));

            int actualCount;
            int.TryParse(actualFileContents, out actualCount);


            Assert.AreEqual(ExpectedContentsCount, actualCount);
            Assert.AreEqual(expectedFileName, actualFileName);
        }

        static void VerifyExportFile(string fileName, List<string> contents, string folderName = null)
        {
            // We expect one row to be filtered out from original 2 data rows for rule ... DateLastContactInfoCleaned > 90
            const int ExpectedContentsCount = 2; // includes header row

            Assert.IsNotNull(fileName, "File name was not sent to FTP sever.");

            var expectedFileName = string.Format("{0}{1:yyyyMMdd}_QLINK_Fails_dataclean.csv", folderName == null ? "" : folderName + "/", DateTime.Now);
            Assert.AreEqual(expectedFileName, fileName, "File name was incorrect.");

            Assert.IsTrue(contents.Count >= 1, "File must include a header row.");
            Assert.AreEqual("RSA ID|FirstName|LastName", contents[0], "File included wrong header row.");

            Assert.AreEqual(ExpectedContentsCount, contents.Count, "File did not add filtered data table rows to output.");

            for(var i = 1; i < contents.Count; i++)
            {
                var fields = contents[i].Split('|');
                Assert.AreEqual(3, fields.Length, "File did not have correct number of fields in line " + i);
            }
        }

        static List<string> ReadAttachment(AttachmentBase attachment)
        {
            var result = new List<string>();
            var stream = CopyStream(attachment.ContentStream);
            using(var reader = new StreamReader(stream))
            {
                string text;
                while((text = reader.ReadLine()) != null)
                {
                    result.Add(text);
                }
            }
            return result;
        }

        static Stream CopyStream(Stream inputStream)
        {
            const int ReadSize = 256;
            var buffer = new byte[ReadSize];
            var ms = new MemoryStream();

            var count = inputStream.Read(buffer, 0, ReadSize);
            while(count > 0)
            {
                ms.Write(buffer, 0, count);
                count = inputStream.Read(buffer, 0, ReadSize);
            }
            ms.Seek(0, SeekOrigin.Begin);
            inputStream.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        static DataTable CreateTestData()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("GovID", typeof(string)), 
                new DataColumn("FirstNames", typeof(string)), 
                new DataColumn("LastName", typeof(string)), 
                new DataColumn("DateLastContactInfoCleaned", typeof(int))
            });
            dt.Rows.Add("5412225703083", "Celani Raymond", "Majola", 528);
            dt.Rows.Add("7406285559086", "Vusi David", "Mahlatsi", -1);
            return dt;
        }

    }
}
