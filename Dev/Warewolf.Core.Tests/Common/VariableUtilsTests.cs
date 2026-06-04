/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class VariableUtilsTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_AddError_BothNonNull_AddsError()
        {
            var utils = new VariableUtils();
            var errors = new List<IActionableErrorInfo>();

            utils.AddError(errors, new ActionableErrorInfo());

            Assert.AreEqual(1, errors.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_AddError_NullError_DoesNothing()
        {
            var utils = new VariableUtils();
            var errors = new List<IActionableErrorInfo>();

            utils.AddError(errors, null);

            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_AddError_NullList_DoesNotThrow()
        {
            var utils = new VariableUtils();

            utils.AddError(null, new ActionableErrorInfo());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_IsEvaluated_DetectsVariableExpression()
        {
            var utils = new VariableUtils();

            Assert.IsTrue(utils.IsEvaluated("[[a]]"));
            Assert.IsFalse(utils.IsEvaluated("plain"));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_IsValueRecordset_DetectsRecordset()
        {
            var utils = new VariableUtils();

            Assert.IsTrue(utils.IsValueRecordset("[[rec().field]]"));
            Assert.IsFalse(utils.IsValueRecordset("[[a]]"));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_RemoveLanguageBrackets_StripsBrackets()
        {
            var utils = new VariableUtils();

            Assert.AreEqual("a", utils.RemoveLanguageBrackets("[[a]]"));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_SplitIntoRegions_ReturnsRegions()
        {
            var utils = new VariableUtils();

            var regions = utils.SplitIntoRegions("[[a]][[b]]");

            CollectionAssert.Contains(regions, "[[a]]");
            CollectionAssert.Contains(regions, "[[b]]");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_ParseDataLanguageForIntellisense_ReturnsResults()
        {
            var utils = new VariableUtils();

            var results = utils.ParseDataLanguageForIntellisense("[[a]]", "<DataList><a/></DataList>");

            Assert.IsNotNull(results);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_ValidateName_ValidName_ReturnsNoError()
        {
            var utils = new VariableUtils();

            // A simple valid name produces no intellisense validation result.
            var result = utils.ValidateName("a", "display");

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_TryParseVariables_BlankInput_ReturnsNullError()
        {
            var utils = new VariableUtils();

            var error = utils.TryParseVariables("   ", out var output, () => { });

            Assert.IsNull(error);
            Assert.AreEqual("   ", output);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_TryParseVariables_ReplacesVariableWithDefaultText()
        {
            var utils = new VariableUtils();

            var error = utils.TryParseVariables("[[a]]", out var output, () => { });

            Assert.IsNull(error);
            Assert.AreEqual("a", output);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_TryParseVariables_WithVariableValue_ReplacesWithGivenValue()
        {
            var utils = new VariableUtils();

            var error = utils.TryParseVariables("[[a]]", out var output, () => { }, "VALUE");

            Assert.IsNull(error);
            Assert.AreEqual("VALUE", output);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_TryParseVariables_WithVariableValueAndLabel_ReplacesWithGivenValue()
        {
            var utils = new VariableUtils();

            var error = utils.TryParseVariables("[[a]]", out var output, () => { }, "VALUE", "Label");

            Assert.IsNull(error);
            Assert.AreEqual("VALUE", output);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_TryParseVariables_WithMatchingInput_ReplacesUsingInputValue()
        {
            var utils = new VariableUtils();
            var inputs = new ObservableCollection<ObservablePair<string, string>>
            {
                new ObservablePair<string, string>("[[a]]", "fromInput")
            };

            var error = utils.TryParseVariables("[[a]]", out var output, () => { }, "Label", "ignored", inputs);

            Assert.IsNull(error);
            Assert.AreEqual("fromInput", output);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(VariableUtils))]
        public void VariableUtils_TryParseVariables_WithNoMatchingInput_ReplacesWithEmpty()
        {
            var utils = new VariableUtils();
            var inputs = new ObservableCollection<ObservablePair<string, string>>();

            var error = utils.TryParseVariables("[[a]]", out var output, () => { }, "Label", "ignored", inputs);

            Assert.IsNull(error);
            Assert.AreEqual(string.Empty, output);
        }
    }
}
