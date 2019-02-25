﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Resource.Tests
{
    [TestClass]
    public class IntellisenseTrackerMenuTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(IntellisenseTrackerMenu))]

        public void IntellisenseTrackerMenu()
        {
            Assert.AreEqual("Variable Events", Tracking.IntellisenseTrackerMenu.EventCategory);
            Assert.AreEqual("Incorrect Syntax", Tracking.IntellisenseTrackerMenu.IncorrectSyntax);
        }
    }
}
