using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Extensions;
using Tu.Imports;
using Tu.Servers;
using Tu.Washing;

namespace Tu.Server.Tests
{
    public partial class WashingMachineTests
    {
        static readonly string ImportErrorsBody = ResourceFetcher.Fetch("Tu.Server.Tests", "ImportErrorsBody.html");

        [TestMethod]
        [TestCategory("WashingMachine_Import")]
        [Description("WashingMachine Import reads sent count from local file system.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ImportCsv_ReadsSentCountFile()
        {
            const int SentCount = 3;
            var runDate = new DateTime(2013, 8, 5);
            var expectedFileName = string.Format("{0:yyyyMMdd}.txt", runDate);

            var importProcessor = new Mock<IImportProcessor>();
            importProcessor.Setup(p => p.Errors).Returns(new DataTable());
            importProcessor.Setup(p => p.OutputData).Returns(new DataTable());
            importProcessor.Setup(p => p.Run(It.IsAny<string>())).Returns(new DataRow[0]);

            var sqlServer = new Mock<ISqlServer>();
            var emailServer = new Mock<IEmailServer>();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns("xxx");

            var readHitCount = 0;
            string actualFileName = null;
            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.ReadAllText(It.IsAny<string>()))
                           .Callback((string fileName) =>
                           {
                               if(readHitCount++ == 0)  // Only interested in first hit!
                               {
                                   actualFileName = fileName;
                               }
                           }).Returns(SentCount.ToString).Verifiable();

            var machine = new WashingMachine(importProcessor.Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, localFileServer.Object);
            var importResult = machine.Import(runDate);

            localFileServer.Verify(f => f.ReadAllText(It.IsAny<string>()));

            Assert.AreEqual(SentCount, importResult.SentCount);
            Assert.AreEqual(expectedFileName, actualFileName);
        }

        [TestMethod]
        [TestCategory("WashingMachine_Import")]
        [Description("WashingMachine Import dowloads csv from FTP machine.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ImportCsv_DownloadsFromFtpServer()
        {
            const string TestData = "Test Data";
            var runDate = new DateTime(2013, 8, 5);

            string actualFileName = null;
            var expectedFileName = string.Format("{0}/{1}", "Transunion/Auto Output", string.Format("{0:yyyyMMdd}-TUW.csv", runDate));

            var sqlServer = new Mock<ISqlServer>();
            var emailServer = new Mock<IEmailServer>();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Callback((string relativeUri) => { actualFileName = relativeUri; }).Returns(TestData).Verifiable();

            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(string.Empty);

            var importProcessor = new Mock<IImportProcessor>();
            importProcessor.Setup(p => p.Errors).Returns(new DataTable());
            importProcessor.Setup(p => p.OutputData).Returns(new DataTable());
            importProcessor.Setup(p => p.Run(It.IsAny<string>())).Returns(new DataRow[0]);

            var machine = new WashingMachine(importProcessor.Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, localFileServer.Object);
            machine.Import(runDate);

            ftpServer.Verify(e => e.Download(It.IsAny<string>()), "Csv was not downloaded from FTP sever.");

            Assert.AreEqual(expectedFileName, actualFileName, "Incorrect csv file name was downloaded.");
        }

        [TestMethod]
        [TestCategory("WashingMachine_Import")]
        [Description("WashingMachine Import validates csv.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ImportCsv_ValidatesFile()
        {
            const string TestData = "Test Data";
            var runDate = new DateTime(2013, 8, 5);

            var importProcessor = new Mock<IImportProcessor>();
            importProcessor.Setup(p => p.Errors).Returns(new DataTable());
            importProcessor.Setup(p => p.OutputData).Returns(new DataTable());
            importProcessor.Setup(p => p.Run(It.IsAny<string>())).Returns(new DataRow[0]).Verifiable();

            var sqlServer = new Mock<ISqlServer>();
            var emailServer = new Mock<IEmailServer>();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns(TestData);

            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(string.Empty);

            var machine = new WashingMachine(importProcessor.Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, localFileServer.Object);
            machine.Import(runDate);

            importProcessor.Verify(p => p.Run(It.IsAny<string>()));
        }

        [TestMethod]
        [TestCategory("WashingMachine_Import")]
        [Description("WashingMachine Import saves errors to errors server.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ImportCsv_SavesErrorsToErrorsServer()
        {
            const int ExpectedContentsCount = 3; // includes header row
            var runDate = new DateTime(2013, 8, 5);

            var errors = new DataTable();
            errors.Columns.Add("GovID", typeof(string));
            errors.Columns.Add("Reason", typeof(string));
            errors.Rows.Add("5412225703083", "Numeric values in Surname");
            errors.Rows.Add("7406285559086", "Forenames Is Not Uppercase");

            var importProcessor = new Mock<IImportProcessor>();
            importProcessor.Setup(p => p.Errors).Returns(errors);
            importProcessor.Setup(p => p.OutputData).Returns(new DataTable());
            importProcessor.Setup(p => p.Run(It.IsAny<string>())).Returns(new DataRow[0]);

            var sqlServer = new Mock<ISqlServer>();
            var emailServer = new Mock<IEmailServer>();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns("xxxx");

            string actualFilePath = null;
            string actualContent = null;

            var errorsServer = new Mock<IFileServer>();
            errorsServer.Setup(s => s.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback((string filePath, string content) =>
            {
                actualFilePath = filePath;
                actualContent = content;
            }).Verifiable();

            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(string.Empty);

            var machine = new WashingMachine(importProcessor.Object, sqlServer.Object, emailServer.Object, ftpServer.Object, errorsServer.Object, localFileServer.Object);
            machine.Import(runDate);

            errorsServer.Verify(s => s.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), "Errors file was not saved to errors server.");

            var fileName = string.Format("{0:yyyyMMdd}-TUW.csv", runDate);
            var errorsFilePath = string.Format(@"\\rsaklfsvrtfsbld\Benchmark_Errors\Trevor\{0}_Errors.csv", fileName.Replace(".csv", ""));
            Assert.AreEqual(errorsFilePath, actualFilePath, "Errors file was not saved to the correct file path on the errors server.");

            Assert.IsNotNull(actualContent, "Errors file was not created.");

            var contents = actualContent.ToLines();

            Assert.IsTrue(contents.Count >= 1, "Errors file must include a header row.");
            Assert.AreEqual("GovID|Reason", contents[0], "Errors file included wrong header row.");

            Assert.AreEqual(ExpectedContentsCount, contents.Count, "Errors file did not add errors to output.");

            for(var i = 1; i < contents.Count; i++)
            {
                var fields = contents[i].Split('|');
                Assert.AreEqual(2, fields.Length, "Errors file did not have correct number of fields in line " + i);
            }
        }

        [TestMethod]
        [TestCategory("WashingMachine_Import")]
        [Description("WashingMachine Import sends errors email.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ImportCsvWithErrorsAndZeroDiff_SendsNonHighlightedEmail()
        {
            const string TestData = "Test Data";
            var runDate = new DateTime(2013, 8, 5);

            var validRows = new List<DataRow>();

            var importData = new DataTable();
            importData.Columns.Add("IsValid", typeof(bool));
            var dc = importData.Columns.Add("TestField", typeof(string));
            validRows.Add(importData.Rows.Add(true, "row1"));
            validRows.Add(importData.Rows.Add(true, "row2"));
            validRows.Add(importData.Rows.Add(true, "row3"));
            validRows.Add(importData.Rows.Add(true, "row4"));

            var importProcessor = new Mock<IImportProcessor>();
            importProcessor.Setup(p => p.Columns).Returns(new List<DataColumn>(new[] { dc }));
            importProcessor.Setup(p => p.Errors).Returns(new DataTable());
            importProcessor.Setup(p => p.OutputData).Returns(importData);
            importProcessor.Setup(p => p.Run(It.IsAny<string>())).Returns(validRows.ToArray);

            var sqlServer = new Mock<ISqlServer>();
            sqlServer.Setup(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>(), It.IsAny<SqlParameter>())).Returns(new DataTable()).Verifiable();

            MailMessage message = null;

            var emailServer = new Mock<IEmailServer>();
            emailServer.Setup(e => e.Send(It.IsAny<MailMessage>())).Callback((MailMessage msg) => message = msg).Verifiable();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns(TestData);

            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(ImportErrorsBody);

            var machine = new WashingMachine(importProcessor.Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, localFileServer.Object);
            var importResult = machine.Import(runDate);

            Assert.AreEqual(4, importResult.ReturnedCount);
            Assert.AreEqual(4, importResult.ImportedCount);

            emailServer.Verify(e => e.Send(It.IsAny<MailMessage>()), "Errors email was not sent.");

            var expectedRecipients = new[] { "travis.frisinger@dev2.co.za", "trevor.williams-ros@dev2.co.za" };
            var actualRecipients = message.To.ToArray().Select(a => a.Address);
            Assert.IsTrue(expectedRecipients.SequenceEqual(actualRecipients));

            VerifyImportErrorsEmailBody(message, runDate, importResult);
        }

        [TestMethod]
        [TestCategory("WashingMachine_Import")]
        [Description("WashingMachine Import sends errors email.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ImportCsvWithErrorsAndNonZeroDiff_SendsHighlightedEmail()
        {
            const string TestData = "Test Data";
            var runDate = new DateTime(2013, 8, 5);

            var validRows = new List<DataRow>();
            var importData = new DataTable();
            importData.Columns.Add("IsValid", typeof(bool));
            var dc = importData.Columns.Add("TestField", typeof(string));
            validRows.Add(importData.Rows.Add(true, "row1"));
            validRows.Add(importData.Rows.Add(true, "row2"));
            importData.Rows.Add(false, "row3"); // 1 invalid row - for non-zero diff
            validRows.Add(importData.Rows.Add(true, "row4"));

            var importProcessor = new Mock<IImportProcessor>();
            importProcessor.Setup(p => p.Columns).Returns(new List<DataColumn>(new[] { dc }));
            importProcessor.Setup(p => p.Errors).Returns(new DataTable());
            importProcessor.Setup(p => p.OutputData).Returns(importData);
            importProcessor.Setup(p => p.Run(It.IsAny<string>())).Returns(validRows.ToArray);

            var sqlServer = new Mock<ISqlServer>();
            sqlServer.Setup(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>(), It.IsAny<SqlParameter>())).Returns(new DataTable()).Verifiable();

            MailMessage message = null;

            var emailServer = new Mock<IEmailServer>();
            emailServer.Setup(e => e.Send(It.IsAny<MailMessage>())).Callback((MailMessage msg) => message = msg).Verifiable();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns(TestData);

            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(ImportErrorsBody);

            var machine = new WashingMachine(importProcessor.Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, localFileServer.Object);
            var importResult = machine.Import(runDate);

            Assert.AreEqual(4, importResult.ReturnedCount);
            Assert.AreEqual(3, importResult.ImportedCount);

            emailServer.Verify(e => e.Send(It.IsAny<MailMessage>()), "Errors email was not sent.");

            var expectedRecipients = new[] { "travis.frisinger@dev2.co.za", "trevor.williams-ros@dev2.co.za" };
            var actualRecipients = message.To.ToArray().Select(a => a.Address);
            Assert.IsTrue(expectedRecipients.SequenceEqual(actualRecipients));

            VerifyImportErrorsEmailBody(message, runDate, importResult);
        }

        [TestMethod]
        [TestCategory("WashingMachine_Import")]
        [Description("WashingMachine Import saves valid rows to the database.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ImportCsv_SavesValidRowsToSqlServer()
        {
            var runDate = new DateTime(2013, 8, 5);

            var importData = new DataTable();
            importData.Columns.Add("IsValid", typeof(bool));
            var dc = importData.Columns.Add("TestField", typeof(string));
            importData.Rows.Add(true, "row1");
            importData.Rows.Add(true, null);
            importData.Rows.Add(true, DBNull.Value);
            importData.Rows.Add(true, "row4");

            var importProcessor = new Mock<IImportProcessor>();
            importProcessor.Setup(p => p.Columns).Returns(new List<DataColumn>(new[] { dc }));
            importProcessor.Setup(p => p.Errors).Returns(new DataTable());
            importProcessor.Setup(p => p.OutputData).Returns(new DataTable());
            importProcessor.Setup(p => p.Run(It.IsAny<string>())).Returns(importData.Select());

            string actualCommandText = null;
            SqlParameter actualCommandParameter = null;

            var sqlServer = new Mock<ISqlServer>();
            sqlServer.Setup(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>(), It.IsAny<SqlParameter>()))
                .Callback((string commandText, CommandType commandType, SqlParameter[] parameters) =>
                {
                    actualCommandText = commandText;
                    actualCommandParameter = parameters[0];
                }).Returns(new DataTable()).Verifiable();

            var emailServer = new Mock<IEmailServer>();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns("xxxx");

            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(string.Empty);

            var machine = new WashingMachine(importProcessor.Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, localFileServer.Object);
            machine.Import(runDate);

            sqlServer.Verify(s => s.FetchDataTable(It.IsAny<string>(), It.IsAny<CommandType>(), It.IsAny<SqlParameter>()), "Valid rows not saved to SQL Server.");

            Assert.AreEqual(WashingMachine.SaveCommandText, actualCommandText, "Valid rows not saved using correct stored procedure.");
            Assert.AreEqual(WashingMachine.SaveCommandParameterName, actualCommandParameter.ParameterName, "Valid rows not saved using correct stored procedure parameter.");

            var paramValue = actualCommandParameter.Value as SqlXml;

            Assert.IsNotNull(paramValue, "Valid rows not saved using XML parameter.");
            var xmlStr = paramValue.Value;

            var xml = XElement.Parse(xmlStr);
            var children = xml.Elements().ToList();
            Assert.AreEqual(4, children.Count, "SQL XML did not contain all valid rows.");

            var i = 1;
            foreach(var child in children)
            {
                var fields = child.Elements().ToList();
                Assert.AreEqual(1, fields.Count);
                Assert.AreEqual("TestField", fields[0].Name);
                switch(i)
                {
                    case 1:
                    case 4:
                        Assert.AreEqual("row" + i, fields[0].Value);
                        break;
                    case 2:
                    case 3:
                        Assert.AreEqual(string.Empty, fields[0].Value);
                        break;
                }
                i++;
            }
        }


        [TestMethod]
        [TestCategory("WashingMachine_Import")]
        [Description("WashingMachine Import moves FTP file to complete.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ImportCsv_MovesFtpFileToComplete()
        {
            var runDate = new DateTime(2013, 8, 5);

            var importProcessor = new Mock<IImportProcessor>();
            importProcessor.Setup(p => p.Errors).Returns(new DataTable());
            importProcessor.Setup(p => p.OutputData).Returns(new DataTable());
            importProcessor.Setup(p => p.Run(It.IsAny<string>())).Returns(new DataRow[0]);

            var sqlServer = new Mock<ISqlServer>();

            var emailServer = new Mock<IEmailServer>();

            string fromUri = null;
            string toUri = null;

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns("xxxx");
            ftpServer.Setup(e => e.Rename(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string fromRelativeUri, string toRelativeUri) =>
                {
                    fromUri = fromRelativeUri;
                    toUri = toRelativeUri;
                }).Returns(true).Verifiable();

            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(string.Empty);

            var machine = new WashingMachine(importProcessor.Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, localFileServer.Object);
            machine.Import(runDate);

            ftpServer.Verify(e => e.Rename(It.IsAny<string>(), It.IsAny<string>()), "FTP file not moved to complete.");

            var fileName = string.Format("{0:yyyyMMdd}-TUW.csv", runDate);

            var expectedFromUri = string.Format("{0}/{1}", "Transunion/Auto Output", fileName);
            var expectedToUri = string.Format("{0}/{1}", "Transunion/Auto Output/Complete", fileName);

            Assert.AreEqual(expectedFromUri, fromUri, "Incorrect FTP file moved.");
            Assert.AreEqual(expectedToUri, toUri, "FTP file moved to incorrect location.");
        }


        [TestMethod]
        [TestCategory("WashingMachine_Import")]
        [Description("WashingMachine Import reads sent count from local file system.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_ImportCsv_DeletesSentCountFile()
        {
            var runDate = new DateTime(2013, 8, 5);
            var expectedFileName = string.Format("{0:yyyyMMdd}.txt", runDate);

            var sqlServer = new Mock<ISqlServer>();
            var emailServer = new Mock<IEmailServer>();

            var ftpServer = new Mock<IFtpServer>();
            ftpServer.Setup(e => e.Download(It.IsAny<string>())).Returns("xxx");

            string actualFileName = null;
            var localFileServer = new Mock<IFileServer>();
            localFileServer.Setup(f => f.Delete(It.IsAny<string>()))
                           .Callback((string fileName) =>
                           {
                               actualFileName = fileName;
                           }).Verifiable();

            localFileServer.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("xxxxx");

            var importProcessor = new Mock<IImportProcessor>();
            importProcessor.Setup(p => p.Errors).Returns(new DataTable());
            importProcessor.Setup(p => p.OutputData).Returns(new DataTable());
            importProcessor.Setup(p => p.Run(It.IsAny<string>())).Returns(new DataRow[0]);

            var machine = new WashingMachine(importProcessor.Object, sqlServer.Object, emailServer.Object, ftpServer.Object, new Mock<IFileServer>().Object, localFileServer.Object);
            machine.Import(runDate);

            localFileServer.Verify(f => f.Delete(It.IsAny<string>()));

            Assert.AreEqual(expectedFileName, actualFileName);
        }

        static void VerifyImportErrorsEmailBody(MailMessage message, DateTime runDate, ImportResult importResult)
        {
            var fileName = string.Format("{0:yyyyMMdd}-TUW.csv", runDate);
            var errorsFilePath = string.Format(@"\\rsaklfsvrtfsbld\Benchmark_Errors\Trevor\{0}_Errors.csv", fileName.Replace(".csv", ""));

            var diffCount = importResult.DifferenceCount > 0 ? string.Format("<b style='color: red'>{0}</b>", importResult.DifferenceCount) : "0";

            var expectedBody = ImportErrorsBody
                .Replace("##filename##", errorsFilePath)
                .Replace("##sent##", importResult.SentCount.ToString(CultureInfo.InvariantCulture))
                .Replace("##returned##", importResult.ReturnedCount.ToString(CultureInfo.InvariantCulture))
                .Replace("##imported##", importResult.ImportedCount.ToString(CultureInfo.InvariantCulture))
                .Replace("##difference##", diffCount);

            //var idx = message.Body.IndexOf("<body>");
            //var actualBody = message.Body.Remove(0, idx);
            //idx = actualBody.IndexOf("</body>") + 7;
            //actualBody = actualBody.Remove(idx);

            var actualBody = message.Body;
            Assert.AreEqual(expectedBody, actualBody, "Errors Email not to sent with the correct body.");
        }

    }
}
