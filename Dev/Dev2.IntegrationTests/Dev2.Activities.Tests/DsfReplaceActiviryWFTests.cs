
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDataSplitActivityWFTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfReplaceActivityWFTests
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ReplaceToolUsingRecordsetWithStar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Acceptance Testing Resources/ReplaceToolUsingRecordsetWithStar");
            const string expected = @"<ReplacementCount>3</ReplacementCount><People index=""1""><Name>Wallis Buchan</Name><Province>Kwa-Zulu Natal</Province></People><People index=""2""><Name>Barney Buchan</Name><Province>Kwa-Zulu Natal</Province></People><People index=""3""><Name>Jurie Smit</Name><Province>GP</Province></People><People index=""4""><Name>Massimo Guerrera</Name><Province>Kwa-Zulu Natal</Province></People>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }


        [TestMethod]
        public void ReplaceToolWithScalar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Acceptance Testing Resources/ReplaceToolWithScalar");

            string expected = @"<Document>To whom it may concern
I would like to inform you that the following document is for the purpose
of integration testing and not for the amusment of Dr Page. Dr Page will be
adament in telling you different but Dr Page is missinformed. Aswell as I believe Dr Page will be doing some user interface development during this sprint and I would like to wish Dr Page the best of luck.
Best Wishes
King of Tools
Dr Guerrera</Document><ReplaceCount>5</ReplaceCount>";
            expected = TestHelper.CleanUp(expected);
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            ResponseData = TestHelper.CleanUp(ResponseData);

            StringAssert.Contains(ResponseData, expected);
        }
    }
}
