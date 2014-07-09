using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Net.Mail;
using System.Net.Mime;
using System.Xml.Linq;
using Tu.Extensions;
using Tu.Imports;
using Tu.Servers;

namespace Tu.Washing
{
    public class WashingMachine : DisposableObject, IWashingMachine
    {
        public const string FetchDataCommandText = "[dbo].[proc_FailureDataForCleaning_Get]";
        public const CommandType FetchDataCommandType = CommandType.StoredProcedure;

        public const string SaveCommandText = "[Prospect].[proc_CustomerStage_Save]";
        public const CommandType SaveCommandType = CommandType.StoredProcedure;
        public const string SaveCommandParameterName = "@ProspectData";
        public const SqlDbType SaveCommandParameterType = SqlDbType.Xml;

        readonly IImportProcessor _importProcessor;
        readonly ISqlServer _sqlServer;
        readonly IEmailServer _emailServer;
        readonly IFtpServer _ftpServer;
        readonly IFileServer _errorsFileServer;
        readonly IFileServer _localFileServer;

        #region CTOR

        public WashingMachine(IImportProcessor importProcessor, ISqlServer sqlServer, IEmailServer emailServer, IFtpServer ftpServer, IFileServer errorsFileServer, IFileServer localFileServer)
        {
            if(importProcessor == null)
            {
                throw new ArgumentNullException("importProcessor");
            }
            if(sqlServer == null)
            {
                throw new ArgumentNullException("sqlServer");
            }
            if(emailServer == null)
            {
                throw new ArgumentNullException("emailServer");
            }
            if(ftpServer == null)
            {
                throw new ArgumentNullException("ftpServer");
            }
            if(errorsFileServer == null)
            {
                throw new ArgumentNullException("errorsFileServer");
            }
            if(localFileServer == null)
            {
                throw new ArgumentNullException("localFileServer");
            }
            _importProcessor = importProcessor;
            _sqlServer = sqlServer;
            _emailServer = emailServer;
            _ftpServer = ftpServer;
            _errorsFileServer = errorsFileServer;
            _localFileServer = localFileServer;
        }

        #endregion

        #region Export

        public void Export()
        {
            var runDate = DateTime.Now;

            // Fetch data from SQL server
            var dt = _sqlServer.FetchDataTable(FetchDataCommandText, FetchDataCommandType);

            // Send data dump email
            SendEmail(
                ConfigurationManager.AppSettings["DataDumpRecipients"], "Raw Data File for Data Cleaning",
                string.Format(ConfigurationManager.AppSettings["DataDumpBody"], runDate, dt.Rows.Count), false,
                string.Format(ConfigurationManager.AppSettings["DataDumpFileName"], runDate),
                dt.ToCsv("|", null, null));

            // Clean up column names
            dt.Columns["GovID"].ColumnName = "RSA ID";
            dt.Columns["FirstNames"].ColumnName = "FirstName";
            dt.Columns["LastName"].ColumnName = "LastName";

            // Filter data by "DateLastContactInfoCleaned > 90" and remove DateLastContactInfoCleaned column
            var exportCsv = dt.ToCsv("|", "DateLastContactInfoCleaned > 90", new[] { "DateLastContactInfoCleaned" });
            var exportFileName = string.Format(ConfigurationManager.AppSettings["ExportFileName"], runDate);

            // Upload CSV to FTP server
            var fileUri = string.Format("{0}/{1}", ConfigurationManager.AppSettings["ExportUri"], exportFileName);
            _ftpServer.Upload(fileUri, exportCsv);

            // Send CSV email
            SendEmail(
                ConfigurationManager.AppSettings["ExportRecipients"], "New File for Data Cleaning",
                string.Format(ConfigurationManager.AppSettings["ExportBody"], runDate, dt.Rows.Count), false,
                exportFileName,
                exportCsv);

            // Save sent row count
            _localFileServer.WriteAllText(string.Format("{0:yyyyMMdd}.txt", runDate), dt.Rows.Count.ToString(CultureInfo.InvariantCulture));
        }

        #endregion

        #region Import

        public ImportResult Import(DateTime runDate)
        {
            var importResult = new ImportResult();

            // Read sent count for run date
            var sentCountFileName = string.Format("{0:yyyyMMdd}.txt", runDate);
            var sentCountStr = _localFileServer.ReadAllText(sentCountFileName);
            int sentCount;
            int.TryParse(sentCountStr, out sentCount);

            // Download CSV from FTP server
            var importCsvName = string.Format(ConfigurationManager.AppSettings["ImportFileName"], runDate);
            var importCsvUri = string.Format("{0}/{1}", ConfigurationManager.AppSettings["ImportUri"], importCsvName);
            var importCsv = _ftpServer.Download(importCsvUri);

            // Validate CSV
            var validRows = _importProcessor.Run(importCsv);

            importResult.SentCount = sentCount;
            importResult.ImportedCount = validRows.Length;
            importResult.ReturnedCount = _importProcessor.OutputData.Rows.Count;

            // Save errors to errors file share
            var errorsCsvName = string.Format(ConfigurationManager.AppSettings["ImportErrorsPath"], importCsvName.Replace(".csv", ""));
            var errorsCsv = _importProcessor.Errors.ToCsv("|", null, null);
            _errorsFileServer.WriteAllText(errorsCsvName, errorsCsv);

            // Send errors-link email
            SendEmail(
                ConfigurationManager.AppSettings["ImportRecipients"], "Data Cleaning Import Errors",
                CreateImportEmailBody(ConfigurationManager.AppSettings["ImportBody"], errorsCsvName, importResult),
                true);

            // Save valid rows to SQL server
            if(validRows.Length > 0)
            {
                var importXml = CreateImportXml(_importProcessor.Columns, validRows);
                using(var xmlReader = importXml.CreateReader())
                {
                    var importResults = _sqlServer.FetchDataTable(SaveCommandText, SaveCommandType,
                        new SqlParameter(SaveCommandParameterName, SaveCommandParameterType) { Value = new SqlXml(xmlReader) });

                    if(importResults.Rows.Count != validRows.Length)
                    {
                        // TODO: Figure out what this means!
                    }
                }
            }

            // Move file to complete on FTP server
            var toUri = string.Format("{0}/{1}", ConfigurationManager.AppSettings["CompletedUri"], importCsvName);
            _ftpServer.Rename(importCsvUri, toUri);

            _localFileServer.Delete(sentCountFileName);

            return importResult;
        }

        #endregion

        #region SendEmail

        void SendEmail(string recipients, string subject, string body, bool isBodyHtml, string attachmentName = null, string attachmentContent = null)
        {
            var message = new MailMessage { Body = body, Subject = subject, IsBodyHtml = isBodyHtml };
            message.To.Add(recipients);

            if(!string.IsNullOrEmpty(attachmentName) && !string.IsNullOrEmpty(attachmentContent))
            {
                var attachmentContentType = new ContentType { MediaType = MediaTypeNames.Text.Plain, Name = attachmentName };
                message.Attachments.Add(Attachment.CreateAttachmentFromString(attachmentContent, attachmentContentType));
            }

            _emailServer.Send(message);
        }

        #endregion

        #region CreateImportEmailBody

        string CreateImportEmailBody(string templateName, string errorsFilePath, ImportResult importResult)
        {
            var emailFormat = _localFileServer.ReadAllText(templateName);
            return FormatEmail(emailFormat, errorsFilePath, importResult);
        }

        static string FormatEmail(string emailFormat, string errorsFilePath, ImportResult importResult)
        {
            var diffCount = importResult.DifferenceCount > 0 ? string.Format("<b style='color: red'>{0}</b>", importResult.DifferenceCount) : "0";

            return emailFormat
                .Replace("##filename##", errorsFilePath)
                .Replace("##sent##", importResult.SentCount.ToString(CultureInfo.InvariantCulture))
                .Replace("##returned##", importResult.ReturnedCount.ToString(CultureInfo.InvariantCulture))
                .Replace("##imported##", importResult.ImportedCount.ToString(CultureInfo.InvariantCulture))
                .Replace("##difference##", diffCount);
        }

        #endregion

        #region CreateImportXml

        static XElement CreateImportXml(IList<DataColumn> columns, IEnumerable<DataRow> rows)
        {
            var result = new XElement("ProspectMessages");

            foreach(var row in rows)
            {
                var msg = new XElement("ProspectMessage");
                foreach(var column in columns)
                {
                    var value = row[column];
                    var element = value == null || Convert.IsDBNull(value) ? new XElement(column.ColumnName) : new XElement(column.ColumnName, value);
                    //if(value == null || Convert.IsDBNull(value))
                    //{
                    //    //value = column.DataType.IsValueType ? Activator.CreateInstance(column.DataType) : string.Empty;
                    //}

                    msg.Add(element);
                }
                result.Add(msg);
            }
            return result;
        }

        #endregion

        #region Overrides of DisposableObject

        protected override void OnDisposed()
        {
            if(_sqlServer != null)
            {
                _sqlServer.Dispose();
            }
            if(_emailServer != null)
            {
                _emailServer.Dispose();
            }
        }

        #endregion
    }
}
