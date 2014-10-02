
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    [TestClass]
    public class DsfActivityAbstractTest
    {
        readonly string _webserverUri = ServerSettings.WebserverURI;
        // Created by: Michael
        // For: Bug 7840
        [TestMethod]
        public void LastRecordSetNotionUpdatesEntry_Expected_RecordsCreated()
        {
            string PostData = String.Format("{0}{1}", _webserverUri, "INTEGRATION TEST SERVICES/LastRecordSetNotationUpdatesEntry");
            string expected = @"<nameSetindex=""1""><Name>Michael</Name><Surname>Cullen</Surname></nameSet>
                                <nameSetindex=""2""><Name>Massimo</Name><Surname>Guerrera</Surname></nameSet>
                                <nameSetindex=""3""><Name>MASSIMO</Name><Surname></Surname></nameSet>
                                <nameSetindex=""4""><Name>0x4d415353494d4f</Name><Surname></Surname></nameSet>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            // Standardize the outputs (Remove newlines, etc)
            expected = TestHelper.CleanUp(expected);
            ResponseData = TestHelper.CleanUp(ResponseData);
            StringAssert.Contains(ResponseData, expected);
        }



    }
}
