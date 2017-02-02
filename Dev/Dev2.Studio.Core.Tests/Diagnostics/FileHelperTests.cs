/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
    public class FileHelperTests
    {
        private static string NewPath;
        private static string OldPath;
        static TestContext Context;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Context = testContext;
            NewPath = Context.TestDir + @"\Warewolf\";
            OldPath = Context.TestDir + @"\Dev2\";
        }

        #region Migrate Temp Data

        #endregion

        #region Create Directory from String


        #endregion
    }
}
