
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    /// <summary>
    /// Summary description for Resumption
    /// </summary>
    [TestClass]
    public class ComplexWebpages
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        // WebpageWithOutputMapping - 

        /// <summary>
        /// Created to ensure baseline sanity for resumption bugs.... I am sick of fixing them ;)
        /// </summary>
        [TestMethod]
        [Ignore]
        public void Ensure_Wizards_Can_Resume()
        {
            WebRequest wr = WebRequest.Create("http://localhost:1234/services/Button.wiz");
            WebResponse wrsp = wr.GetResponse();

            Stream s = wrsp.GetResponseStream();
            if(s != null)
            {
                StreamReader sr = new StreamReader(s);

                string payload = sr.ReadToEnd();

                sr.Close();
                s.Close();
                wrsp.Close();

                // Now chop up for action to fake it ;)
                // action="/services/Button.wiz/instances/343e68c2-90de-4fae-89b1-6f42b34a0783/bookmarks/dsfResumption"

                int start = payload.IndexOf("action=", System.StringComparison.Ordinal);
                if(start > 0)
                {
                    start += 8;

                    int end = (payload.IndexOf('"', start));

                    string action = "http://localhost:1234" + payload.Substring(start, (end - start));


                    HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(action);

                    ASCIIEncoding encoding = new ASCIIEncoding();
                    const string postData = "Dev2elementNameButton=ResumptionTestName";
                    //postData += "&password=pass";
                    byte[] data = encoding.GetBytes(postData);

                    httpWReq.Method = "POST";
                    httpWReq.ContentType = "application/x-www-form-urlencoded";
                    httpWReq.ContentLength = data.Length;

                    using(Stream newStream = httpWReq.GetRequestStream())
                    {
                        newStream.Write(data, 0, data.Length);
                    }

                    string result;

                    using(Stream rStream = httpWReq.GetResponse().GetResponseStream())
                    {
                        if(rStream != null)
                        {
                            sr = new StreamReader(rStream);
                        }

                        result = sr.ReadToEnd();

                        sr.Close();
                    }

                    // Ensure we can see the "saved" data in the result
                    Assert.AreNotEqual(-1, result.IndexOf("<Dev2elementNameButton>ResumptionTestName</Dev2elementNameButton>", System.StringComparison.Ordinal));

                }
                else
                {
                    Assert.Fail("Bad data");
                }
            }
        }

        /// <summary>
        /// Created to ensure baseline sanity for resumption bugs.... I am sick of fixing them ;)
        /// </summary>
        [TestMethod]
        [Ignore]
        public void Ensure_Wizards_Can_Use_Output_Mapping()
        {

            /*
             * This test case is impeded by the inability to properly save webpages....
             */

            WebRequest wr = WebRequest.Create("http://localhost:1234/services/WebpageWithOutputMapping");
            WebResponse wrsp = wr.GetResponse();

            Stream s = wrsp.GetResponseStream();
            if(s != null)
            {
                StreamReader sr = new StreamReader(s);

                string payload = sr.ReadToEnd();

                sr.Close();
                s.Close();
                wrsp.Close();

                // Now chop up for action to fake it ;)
                // action="/services/Button.wiz/instances/343e68c2-90de-4fae-89b1-6f42b34a0783/bookmarks/dsfResumption"

                int start = payload.IndexOf("action=", System.StringComparison.Ordinal);
                if(start > 0)
                {
                    start += 8;

                    int end = (payload.IndexOf('"', start));

                    string action = "http://localhost:1234" + payload.Substring(start, (end - start));

                    HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(action);

                    ASCIIEncoding encoding = new ASCIIEncoding();
                    const string postData = "inputRegion=TestValue";
                    //postData += "&password=pass";
                    byte[] data = encoding.GetBytes(postData);

                    httpWReq.Method = "POST";
                    httpWReq.ContentType = "application/x-www-form-urlencoded";
                    httpWReq.ContentLength = data.Length;

                    using(Stream newStream = httpWReq.GetRequestStream())
                    {
                        newStream.Write(data, 0, data.Length);
                    }

                    using(Stream rStream = httpWReq.GetResponse().GetResponseStream())
                    {
                        if(rStream != null)
                        {
                            sr = new StreamReader(rStream);
                        }

                        sr.ReadToEnd();

                        sr.Close();
                    }

                    // Ensure we can see the "saved" data in the result
                    Assert.Inconclusive("This test has been converted to an Inconclusive since the Web Framework has been removed.");
                    //Assert.AreNotEqual(-1, result.IndexOf("<inputRegion>TestValue</inputRegion>"));
                }
                else
                {
                    Assert.Fail("Bad data");
                }
            }
        }
    }
}
