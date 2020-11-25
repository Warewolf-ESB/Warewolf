/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using NUnit.Framework;

namespace Warewolf.Data.Tests
{
    [TestFixture]
    public class XmlHelperTest
    {
        [Test]
        [Author("Candice Daniel")]
        [Category(nameof(XmlHelper))]
        public void XmlHelper_IsXml_GivenValidXml_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            const string XmlFragment = @"<InnerError>Index #0
Message: Login failed for user 'testuser2'.
LineNumber: 65536
Source: .Net SqlClient Data Provider
Procedure: 
</InnerError><InnerError>ExecuteReader requires an open and available Connection. The connection's current state is closed.
   at System.Data.SqlClient.SqlCommand.ValidateCommand(String method, Boolean async)
   at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method, TaskCompletionSource`1 completion, Int32 timeout, Task& task, Boolean asyncWrite)
   at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method)
   at System.Data.SqlClient.SqlCommand.ExecuteReader(CommandBehavior behavior, String method)
   at System.Data.SqlClient.SqlCommand.ExecuteReader(CommandBehavior behavior)
   at Dev2.Services.Sql.SqlServer.ExecuteReader[T](SqlCommand command, CommandBehavior commandBehavior, Func`2 handler) in c:\Development\Dev\Dev2.Services.Sql\SqlServer.cs:line 121
   at Dev2.Services.Sql.SqlServer.FetchDataTable(SqlParameter[] parameters) in c:\Development\Dev\Dev2.Services.Sql\SqlServer.cs:line 61
   at Dev2.Services.Execution.DatabaseServiceExecution.SqlExecution(ErrorResultTO errors, Object& executeService) in c:\Development\Dev\Dev2.Services.Execution\DatabaseServiceExecution.cs:line 118</InnerError>";
            var isXml = XmlHelper.IsXml(XmlFragment, out bool isFragment, out bool isHtml);
            //---------------Test Result -----------------------
            Assert.IsTrue(isXml);
        }

        [Test]
        [Author("Candice Daniel")]
        [Category(nameof(XmlHelper))]
        public void XmlHelper_IsXml_GivenValidIvalidXml_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            const string XmlFragment = @"HHHHHHH";
            var isXml = XmlHelper.IsXml(XmlFragment, out bool isFragment, out bool isHtml);
            //---------------Test Result -----------------------
            Assert.IsFalse(isXml);
        }

        [Test]
        [Author("Candice Daniel")]
        [Category(nameof(XmlHelper))]
        public void XmlHelper_IsXml_TryProcessAllNodes_IsHtml()
        {
            //---------------Set up test pack-------------------
            const string HTMLFragment = @"<html><body></body></html>";
            var isXml = XmlHelper.IsXml(HTMLFragment, out bool isFragment, out bool isHtml);
            //---------------Test Result -----------------------
            Assert.IsFalse(isXml);
        }

        [Test]
        [Author("Candice Daniel")]
        [Category(nameof(XmlHelper))]
        public void XmlHelper_ToCleanXml_GivenDirtXmlWithToStripTags_ShouldReuturnCleanXml()
        {
            //---------------Set up test pack-------------------
            var xml = @"<XmlData>Hello world<XmlData>";
            //---------------Execute Test ----------------------
            var cleanXml = xml.ToCleanXml();
            //---------------Test Result -----------------------
            Assert.AreEqual("Hello world", cleanXml);
        }

        [Test]
        [Author("Candice Daniel")]
        [Category(nameof(XmlHelper))]
        public void XmlHelper_ToCleanXml_GivenDirtXmlWithnaughtyTags_ShouldReuturnNoData()
        {
            //---------------Set up test pack-------------------
            var xml = @"<WebXMLConfiguration>Hello world</WebXMLConfiguration>";
            //---------------Execute Test ----------------------
            var cleanXml = xml.ToCleanXml();
            //---------------Test Result -----------------------
            Assert.AreEqual("", cleanXml);
        }

        [Test]
        [Author("Siphamandla Dube")]
        [Category(nameof(XmlHelper))]
        public void XmlHelper_ToCleanXml_GivenDirtXmlWithnaughtyTagsAndValid_ShouldReuturnCleanXml()
        {
            //---------------Set up test pack-------------------
            var xml = @"<Person><WebXMLConfiguration>Hello world</WebXMLConfiguration></Person>";
            //---------------Execute Test ----------------------
            var cleanXml = xml.ToCleanXml();
            //---------------Test Result -----------------------
            Assert.AreEqual("<ADL><Person></Person></ADL>", cleanXml);
        }

        [Test]
        [Author("Siphamandla Dube")]
        [Category(nameof(XmlHelper))]
        public void XmlHelper_ToCleanXml_NotisXml_NotisFragment_NotisHtml_ShouldReuturnCleanXml()
        {
            //---------------Set up test pack-------------------
            var xml = @"<![CDATA[some stuff]]>";
            //---------------Execute Test ----------------------
            var cleanXml = xml.ToCleanXml();
            //---------------Test Result -----------------------
            Assert.AreEqual("<![CDATA[some stuff]]>", cleanXml);
        }

        [Test]
        [Author("Siphamandla Dube")]
        [Category(nameof(XmlHelper))]
        public void XmlHelper_ToCleanXml_NotisXml_NotisFragment_NotisHtml_ShouldReuturnCleanXml1()
        {
            //---------------Set up test pack-------------------
            var xml = "<?xml version='1.0' encoding='utf-8'?>"
                        +"<![CDATA[some stuff]]>";
            //---------------Execute Test ----------------------
            var cleanXml = xml.CleanXmlSOAP();
            //---------------Test Result -----------------------
            Assert.AreEqual("<![CDATA[some stuff]]>", cleanXml);
        }
    }
}
