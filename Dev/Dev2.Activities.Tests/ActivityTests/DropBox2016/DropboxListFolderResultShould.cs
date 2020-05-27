/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.DropBox2016.Result;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    [TestClass]

        public class DropboxListFolderResultShould
        {
            [TestMethod]
        [Timeout(60000)]
            [Owner("Nkosinathi Sangweni")]
            [TestCategory(nameof(DropboxListFolderSuccesResult))]
            public void DropboxListFolderSuccesResult_ConstructDropBoxSuccessResult_GivenListFolderResult_ShouldRetunNewSuccessResult()
            {
                //---------------Set up test pack-------------------
                var successResult = new DropboxListFolderSuccesResult(It.IsAny<ListFolderResult>());
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                //---------------Test Result -----------------------
                Assert.IsNotNull(successResult);
            }

            [TestMethod]
        [Timeout(60000)]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(DropboxListFolderSuccesResult))]
            public void DropboxListFolderSuccesResult_GivenListFolderResult_Expect_ILIstFolderResult()
            {
                //---------------Set up test pack-------------------
                var expected = new ListFolderResult();
            
                var dropboxListFolderSuccesResult = new DropboxListFolderSuccesResult(expected);
                //---------------Assert Precondition----------------
                //---------------Execute Test ----------------------
                var result = dropboxListFolderSuccesResult.GetListFolderResult();
                //---------------Test Result -----------------------
                Assert.IsNotNull(result as IListFolderResult);
            }
        }
    
}
