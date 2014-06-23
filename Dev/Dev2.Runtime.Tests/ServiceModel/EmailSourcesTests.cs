using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ServiceModel
{
    // PBI 953 - 2013.05.16 - TWR - Created
    [TestClass]    
    public class EmailSourcesTests
    {
        const int SmtpTimeout = 30000;

        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailSourcesConstructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
            var handler = new EmailSources(null);
        }

        #endregion

        #region Test

        [TestMethod]
        public void EmailSourcesTestWithInValidArgsExpectedInvalidValidationResult()
        {
            var handler = new EmailSources();
            var result = handler.Test("root:'hello'", Guid.Empty, Guid.Empty);
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void EmailSourcesTestWithInvalidHostExpectedInvalidValidationResult()
        {
            var source = new EmailSource { Host = "smtp.foobar.com", Port = 25 }.ToString();

            var handler = new EmailSources();
            var result = handler.Test(source, Guid.Empty, Guid.Empty);
            Assert.IsFalse(result.IsValid, result.ErrorMessage);
        }

        #endregion

        #region Get

        [TestMethod]
        public void EmailSourcesGetWithNullArgsExpectedReturnsNewSource()
        {
            var handler = new EmailSources();
            var result = handler.Get(null, Guid.Empty, Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        public void EmailSourcesGetWithInvalidArgsExpectedReturnsNewSource()
        {
            var handler = new EmailSources();
            var result = handler.Get("xxxxx", Guid.Empty, Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        public void EmailSourcesGetWithValidArgsExpectedReturnsSource()
        {
            var expected = CreateYahooSource();
            var saveArgs = expected.ToString();

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                var handler = new EmailSources();
                handler.Save(saveArgs, workspaceID, Guid.Empty);

                var actual = handler.Get(expected.ResourceID.ToString(), workspaceID, Guid.Empty);

                VerifySource(actual, expected);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        #endregion

        #region Save

        [TestMethod]
        public void EmailSourcesSaveWithInValidArgsExpectedInvalidValidationResult()
        {
            var handler = new EmailSources();
            var jsonResult = handler.Save("root:'hello'", Guid.Empty, Guid.Empty);
            var result = JsonConvert.DeserializeObject<ValidationResult>(jsonResult);
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void EmailSourcesSaveWithValidArgsExpectedInvokesResourceCatalogSave()
        {
            var expected = CreateYahooSource();

            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(c => c.SaveResource(It.IsAny<Guid>(), It.IsAny<IResource>(), It.IsAny<string>())).Verifiable();

            var handler = new EmailSources(catalog.Object);
            var jsonResult = handler.Save(expected.ToString(), Guid.Empty, Guid.Empty);
            var actual = JsonConvert.DeserializeObject<EmailSource>(jsonResult);

            catalog.Verify(c => c.SaveResource(It.IsAny<Guid>(), It.IsAny<IResource>(), It.IsAny<string>()));

            VerifySource(expected, actual);
        }

        #endregion

        #region VerifySource

        static void VerifySource(EmailSource actual, EmailSource expected)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ResourceID, actual.ResourceID);
            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.ResourcePath, actual.ResourcePath);
            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual(expected.ResourceType, actual.ResourceType);

            Assert.AreEqual(expected.Host, actual.Host);
            Assert.AreEqual(expected.Port, actual.Port);
            Assert.AreEqual(expected.EnableSsl, actual.EnableSsl);
            Assert.AreEqual(expected.UserName, actual.UserName);
            Assert.AreEqual(expected.Password, actual.Password);
            Assert.AreEqual(expected.Timeout, actual.Timeout);
        }

        #endregion

        #region CreateYahooSource

        static EmailSource CreateYahooSource()
        {
            return new EmailSource
            {
                Host = "smtp.mail.yahoo.com",
                Port = 25,
                EnableSsl = false,
                Timeout = SmtpTimeout,
                UserName = "dev2developer@yahoo.com",
                Password = "Q38qrDmsi36ei1R",
                TestFromAddress = "dev2developer@yahoo.com",
                TestToAddress = "test@dev2.co.za",

                ResourceID = Guid.NewGuid(),
                ResourceName = "TestGmail",
                ResourcePath = "Testing\\TestGmail",
            };
        }

        #endregion

    }
}
