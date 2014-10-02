
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

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDataSplitActivityWFTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfIndexActivityWFTests
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void IndexToolWithTwoRecordsetsWithStars()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/IndexToolWithTwoRecordsetsWithStars");
            const string expected = @"<EmailResults index=""1""><result>The email to Barney.buchan@dev2.co.za was successful</result></EmailResults><EmailResults index=""2""><result>The email to Wallis.Buchan@dev2.co.za failed</result></EmailResults><EmailResults index=""3""><result>The email to Travis.Frisinger@dev2.co.za failed</result></EmailResults><EmailResults index=""4""><result>The email to Trevor.Williams-Ros@dev2.co.za was successful</result></EmailResults><EmailResults index=""5""><result>The email to Massimo.Guerrera@dev2.co.za was successful</result></EmailResults><ResultsCollection index=""1""><option>successful</option></ResultsCollection><ResultsCollection index=""2""><option>failed</option></ResultsCollection><IndexResult index=""1""><inres>43</inres></IndexResult><IndexResult index=""2""><inres>-1</inres></IndexResult><IndexResult index=""3""><inres>-1</inres></IndexResult><IndexResult index=""4""><inres>49</inres></IndexResult><IndexResult index=""5""><inres>46</inres></IndexResult><IndexResult index=""6""><inres>-1</inres></IndexResult><IndexResult index=""7""><inres>39</inres></IndexResult><IndexResult index=""8""><inres>42</inres></IndexResult><IndexResult index=""9""><inres>-1</inres></IndexResult><IndexResult index=""10""><inres>-1</inres></IndexResult>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }


        [TestMethod]
        public void IndexToolWithScalarAndAllOccurances()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "INTEGRATION TEST SERVICES/IndexToolWithScalarAndAllOccurances");
            const string expected = @"<DataList><Scalar>Massimo Guerrera</Scalar><result>2,16</result>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }
    }
}
