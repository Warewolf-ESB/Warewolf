/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

// -----------------------------------------------------------------------------
// Coverage uplift for Dev2.TO.DecisionTO.
//
// At the time this file was written, DecisionTO was at 0% line coverage in the
// unit-tests bucket (230 uncovered lines) even though it is exercised indirectly
// through DecisionDesignerViewModel tests in Dev2.Activities.Designers.Tests.
// These targeted unit tests pin the public surface of DecisionTO without going
// through the designer view-model, so the type is covered directly by the
// activities test bucket.
//
// Scope covered here:
//   * All five public constructors (defaults, three-arg, four-arg with
//     inserted, eight-arg with from/to/actions, and the Dev2Decision-based
//     constructor with and without callbacks).
//   * Every property setter, including the same-value short-circuit paths on
//     From / To / SearchCriteria / MatchValue / SearchType.
//   * SearchType conversion through FindRecordsDisplayUtil.ConvertForDisplay
//     and propagation into UpdateMatchVisibility for ArgumentCount = 1, 2 and 3.
//   * UpdateMatchVisibility called directly with each Whereoptions ArgumentCount
//     plus an unknown operator (no-op path).
//   * CanAdd / CanRemove / IsEmpty / ClearRow / Inserted / IsLast / CanDelete.
//   * DeleteCommand invocation routing to DeleteAction.
//   * UpdateDisplayAction firing on property change and being suppressed during
//     construction (_isInitializing).
//   * GetRuleSet for every named branch: SearchType (Starts/Ends With and the
//     negated variants), From / To (Is Between, Is Not Between), SearchCriteria
//     (empty and non-empty), and the default branch.
// -----------------------------------------------------------------------------

using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.TOTests
{
    [TestClass]
    public class DecisionTOCoverageTests
    {
        const string Category = "DecisionTO_Coverage";

        // ---------- Constructors ----------------------------------------------

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_DefaultConstructor_SetsDefaults()
        {
            var to = new DecisionTO();

            Assert.AreEqual("Match", to.MatchValue);
            Assert.AreEqual("Match On", to.SearchCriteria);
            Assert.AreEqual("Equal", to.SearchType);
            Assert.AreEqual(0, to.IndexNumber);
            Assert.IsFalse(to.Inserted);
            Assert.IsTrue(to.IsSearchCriteriaEnabled);
            Assert.IsFalse(to.IsSearchTypeFocused);
            Assert.IsNotNull(to.DeleteCommand);
            Assert.IsNotNull(to.UpdateDisplayAction);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_ThreeArgConstructor_PropagatesValues()
        {
            var to = new DecisionTO("m", "c", "=", 7);

            Assert.AreEqual("m", to.MatchValue);
            Assert.AreEqual("c", to.SearchCriteria);
            // "=" is in the converted-display dictionary as itself; SearchType should map through ConvertForDisplay.
            Assert.AreEqual("=", to.SearchType);
            Assert.AreEqual(7, to.IndexNumber);
            Assert.IsFalse(to.Inserted);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_FourArgConstructor_InsertedTrue_SetsInserted()
        {
            var to = new DecisionTO("m", "c", "=", 2, true);

            Assert.IsTrue(to.Inserted);
            Assert.AreEqual(2, to.IndexNumber);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_FullConstructor_AssignsFromToAndActions()
        {
            var updateCalled = 0;
            var deleteCalled = 0;
            Action<DecisionTO> update = a => updateCalled++;
            Action<DecisionTO> delete = a => deleteCalled++;

            var to = new DecisionTO("m", "c", "=", 0, false, "1", "10", update, delete);

            Assert.AreEqual("1", to.From);
            Assert.AreEqual("10", to.To);
            Assert.AreSame(update, to.UpdateDisplayAction);
            Assert.AreSame(delete, to.DeleteAction);

            to.DeleteCommand.Execute(null);
            Assert.AreEqual(1, deleteCalled);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_FullConstructor_NullCallbacks_UsesNoOp()
        {
            var to = new DecisionTO("m", "c", "=", 0, false, "1", "10", null, null);

            // Should not throw when invoking the no-op update or trying to delete with a null DeleteAction.
            Assert.IsNotNull(to.UpdateDisplayAction);
            to.UpdateDisplayAction(to);
            to.DeleteCommand.Execute(null);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_Dev2DecisionConstructor_CopiesColsAndDisplaysEvaluationFn()
        {
            var dec = new Dev2Decision { Col1 = "x", Col2 = "y", Col3 = "z", EvaluationFn = enDecisionType.IsBetween };

            var to = new DecisionTO(dec, 3);

            Assert.AreEqual("x", to.MatchValue);
            Assert.AreEqual("y", to.SearchCriteria);
            Assert.AreEqual("Is Between", to.SearchType);
            Assert.AreEqual(3, to.IndexNumber);
            Assert.AreEqual("y", to.From);
            Assert.AreEqual("z", to.To);
            Assert.IsTrue(to.IsSearchCriteriaVisible);
            Assert.IsTrue(to.IsSearchCriteriaEnabled);
            Assert.IsFalse(to.IsLast);
            Assert.IsNotNull(to.DeleteCommand);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_Dev2DecisionConstructor_WithCallbacks_StoresActions()
        {
            var updateCount = 0;
            var deleteCount = 0;
            Action<DecisionTO> update = a => updateCount++;
            Action<DecisionTO> delete = a => deleteCount++;
            var dec = new Dev2Decision { Col1 = "a", Col2 = "b", Col3 = "c", EvaluationFn = enDecisionType.IsEqual };

            var to = new DecisionTO(dec, 1, update, delete);

            Assert.AreSame(update, to.UpdateDisplayAction);
            Assert.AreSame(delete, to.DeleteAction);
            // Property change after construction should fire UpdateDisplayAction.
            to.MatchValue = "changed";
            Assert.AreEqual(1, updateCount);
            to.DeleteCommand.Execute(null);
            Assert.AreEqual(1, deleteCount);
        }

        // ---------- Property setters ------------------------------------------

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_MatchValueSetter_ChangeAndShortCircuit()
        {
            var calls = 0;
            var to = new DecisionTO("m", "c", "=", 0, false, "", "", a => calls++, null);

            to.MatchValue = "new";
            to.MatchValue = "new"; // short-circuit; no extra display update

            Assert.AreEqual("new", to.MatchValue);
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_SearchCriteriaSetter_ChangeAndShortCircuit()
        {
            var calls = 0;
            var to = new DecisionTO("m", "c", "=", 0, false, "", "", a => calls++, null);

            to.SearchCriteria = "abc";
            to.SearchCriteria = "abc";

            Assert.AreEqual("abc", to.SearchCriteria);
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_FromSetter_ChangeAndShortCircuit()
        {
            var calls = 0;
            var to = new DecisionTO("m", "c", "=", 0, false, "", "", a => calls++, null);

            to.From = "1";
            to.From = "1";

            Assert.AreEqual("1", to.From);
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_ToSetter_ChangeAndShortCircuit()
        {
            var calls = 0;
            var to = new DecisionTO("m", "c", "=", 0, false, "", "", a => calls++, null);

            to.To = "9";
            to.To = "9";

            Assert.AreEqual("9", to.To);
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_SearchType_Null_DoesNothing()
        {
            var to = new DecisionTO();
            var before = to.SearchType;

            to.SearchType = null;

            Assert.AreEqual(before, to.SearchType);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_SearchType_ArgumentCountOne_HidesAllVisibility()
        {
            var to = new DecisionTO();
            to.IsSearchCriteriaVisible = true;
            to.IsSingleMatchCriteriaVisible = true;
            to.IsBetweenCriteriaVisible = true;

            to.SearchType = "Is NULL";

            Assert.AreEqual("Is NULL", to.SearchType);
            Assert.IsFalse(to.IsSearchCriteriaVisible);
            Assert.IsFalse(to.IsSingleMatchCriteriaVisible);
            Assert.IsFalse(to.IsBetweenCriteriaVisible);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_SearchType_ArgumentCountTwo_ShowsSingleMatch()
        {
            var to = new DecisionTO();

            to.SearchType = "=";

            Assert.AreEqual("=", to.SearchType);
            Assert.IsTrue(to.IsSearchCriteriaVisible);
            Assert.IsTrue(to.IsSingleMatchCriteriaVisible);
            Assert.IsFalse(to.IsBetweenCriteriaVisible);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_SearchType_ArgumentCountThree_ShowsBetween()
        {
            var to = new DecisionTO();

            to.SearchType = "Is Between";

            Assert.AreEqual("Is Between", to.SearchType);
            Assert.IsTrue(to.IsSearchCriteriaVisible);
            Assert.IsTrue(to.IsBetweenCriteriaVisible);
            Assert.IsFalse(to.IsSingleMatchCriteriaVisible);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_SearchType_SameValueAfterConvert_DoesNotRaiseChange()
        {
            // "Equals" maps via ConvertForDisplay to "=".  Subsequent set of "=" leaves _searchType the same
            // and exercises the short-circuit return path in the setter.
            var calls = 0;
            var to = new DecisionTO("m", "c", "=", 0, false, "", "", a => calls++, null);
            calls = 0;

            to.SearchType = "="; // _searchType already == "="; setter must early-return after UpdateMatchVisibility.

            Assert.AreEqual("=", to.SearchType);
            Assert.AreEqual(0, calls);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_IndexNumberSetter_Updates()
        {
            var to = new DecisionTO();

            to.IndexNumber = 42;

            Assert.AreEqual(42, to.IndexNumber);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_FocusFlags_Toggle()
        {
            var to = new DecisionTO
            {
                IsSearchTypeFocused = true,
                IsFromFocused = true,
                IsToFocused = true
            };

            Assert.IsTrue(to.IsSearchTypeFocused);
            Assert.IsTrue(to.IsFromFocused);
            Assert.IsTrue(to.IsToFocused);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_VisibilityFlags_Toggle()
        {
            var to = new DecisionTO
            {
                IsSearchCriteriaEnabled = false,
                IsSearchCriteriaVisible = true,
                IsSingleMatchCriteriaVisible = true,
                IsBetweenCriteriaVisible = true
            };

            Assert.IsFalse(to.IsSearchCriteriaEnabled);
            Assert.IsTrue(to.IsSearchCriteriaVisible);
            Assert.IsTrue(to.IsSingleMatchCriteriaVisible);
            Assert.IsTrue(to.IsBetweenCriteriaVisible);
        }

        // ---------- IsLast / CanDelete ----------------------------------------

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_IsLast_ControlsCanDelete()
        {
            var to = new DecisionTO();

            Assert.IsTrue(to.CanDelete(null));

            to.IsLast = true;
            Assert.IsTrue(to.IsLast);
            Assert.IsFalse(to.CanDelete(null));

            to.IsLast = false;
            Assert.IsFalse(to.IsLast);
            Assert.IsTrue(to.CanDelete(null));
        }

        // ---------- CanAdd / CanRemove / IsEmpty / ClearRow -------------------

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_CanRemove_True_WhenAllEmpty()
        {
            var to = new DecisionTO("", "", "", 0);
            to.SearchType = ""; // ensure empty

            Assert.IsTrue(to.CanRemove());
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_CanRemove_False_WhenAnyFieldPopulated()
        {
            var to = new DecisionTO("m", "", "", 0);

            Assert.IsFalse(to.CanRemove());
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_CanAdd_RespondsToMatchAndCriteria()
        {
            var to = new DecisionTO("", "", "", 0);
            Assert.IsFalse(to.CanAdd());

            to.MatchValue = "x";
            Assert.IsTrue(to.CanAdd());

            to.MatchValue = "";
            to.SearchCriteria = "y";
            Assert.IsTrue(to.CanAdd());
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_IsEmpty_True_WhenSearchTypeAndCriteriaEmpty()
        {
            var to = new DecisionTO("m", "", "", 0);
            Assert.IsTrue(to.IsEmpty());

            to.SearchCriteria = "anything";
            Assert.IsFalse(to.IsEmpty());
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_ClearRow_ResetsAllFields()
        {
            var to = new DecisionTO("m", "c", "=", 0);

            to.ClearRow();

            Assert.AreEqual("", to.MatchValue);
            Assert.AreEqual(string.Empty, to.SearchCriteria);
            // SearchType.set with "" runs through ConvertForDisplay (returns "") and the setter
            // skips the OnPropertyChanged path because _searchType is empty/null.
            Assert.IsTrue(string.IsNullOrEmpty(to.SearchType));
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_Inserted_GetSet()
        {
            var to = new DecisionTO { Inserted = true };
            Assert.IsTrue(to.Inserted);
            to.Inserted = false;
            Assert.IsFalse(to.Inserted);
        }

        // ---------- GetRuleSet ------------------------------------------------

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_GetRuleSet_EmptyTO_ReturnsEmptyRuleSet()
        {
            var to = new DecisionTO("", "", "", 0);
            var rs = to.GetRuleSet("SearchType", string.Empty);
            Assert.IsNotNull(rs);
            Assert.IsTrue(IsEmptyRuleSet(rs));
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_GetRuleSet_SearchType_StartsWith_AddsRules()
        {
            var to = new DecisionTO("m", "c", "Starts With", 0);
            var rs = to.GetRuleSet("SearchType", string.Empty);
            Assert.IsFalse(IsEmptyRuleSet(rs));
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_GetRuleSet_SearchType_NotMatching_NoRules()
        {
            var to = new DecisionTO("m", "c", "=", 0);
            var rs = to.GetRuleSet("SearchType", string.Empty);
            Assert.IsTrue(IsEmptyRuleSet(rs));
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_GetRuleSet_From_IsBetween_AddsRules()
        {
            var to = new DecisionTO("m", "c", "Is Between", 0, false, "1", "10", null, null);
            var rs = to.GetRuleSet("From", string.Empty);
            Assert.IsFalse(IsEmptyRuleSet(rs));
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_GetRuleSet_To_IsBetween_AddsRules()
        {
            var to = new DecisionTO("m", "c", "Is Between", 0, false, "1", "10", null, null);
            var rs = to.GetRuleSet("To", string.Empty);
            Assert.IsFalse(IsEmptyRuleSet(rs));
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_GetRuleSet_SearchCriteria_Empty_AddsEmptyRulePlusExpressionRule()
        {
            var to = new DecisionTO("m", "", "=", 0);
            // IsEmpty() is false because SearchType == "=", so we hit the SearchCriteria branch.
            to.SearchCriteria = string.Empty;
            var rs = to.GetRuleSet("SearchCriteria", string.Empty);
            Assert.IsFalse(IsEmptyRuleSet(rs));
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_GetRuleSet_SearchCriteria_NonEmpty_AddsExpressionRule()
        {
            var to = new DecisionTO("m", "c", "=", 0);
            var rs = to.GetRuleSet("SearchCriteria", string.Empty);
            Assert.IsFalse(IsEmptyRuleSet(rs));
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_GetRuleSet_UnknownPropertyName_HitsDefaultBranch()
        {
            var to = new DecisionTO("m", "c", "=", 0);
            var rs = to.GetRuleSet("NotARealProperty", string.Empty);
            // Default branch only logs and returns the empty ruleset.
            Assert.IsTrue(IsEmptyRuleSet(rs));
        }

        // ---------- UpdateMatchVisibility static helper -----------------------

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_UpdateMatchVisibility_UnknownOperator_NoChange()
        {
            var to = new DecisionTO();
            to.IsSearchCriteriaVisible = true;
            to.IsBetweenCriteriaVisible = true;
            to.IsSingleMatchCriteriaVisible = true;

            DecisionTO.UpdateMatchVisibility(to, "this operator does not exist", DecisionTO.Whereoptions);

            Assert.IsTrue(to.IsSearchCriteriaVisible);
            Assert.IsTrue(to.IsBetweenCriteriaVisible);
            Assert.IsTrue(to.IsSingleMatchCriteriaVisible);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_UpdateMatchVisibility_OneArg_HidesEverything()
        {
            var to = new DecisionTO();
            to.IsSearchCriteriaVisible = true;
            to.IsBetweenCriteriaVisible = true;
            to.IsSingleMatchCriteriaVisible = true;

            DecisionTO.UpdateMatchVisibility(to, "Is NULL", DecisionTO.Whereoptions);

            Assert.IsFalse(to.IsSearchCriteriaVisible);
            Assert.IsFalse(to.IsBetweenCriteriaVisible);
            Assert.IsFalse(to.IsSingleMatchCriteriaVisible);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_UpdateMatchVisibility_TwoArgs_ShowsSingleMatch()
        {
            var to = new DecisionTO();

            DecisionTO.UpdateMatchVisibility(to, "=", DecisionTO.Whereoptions);

            Assert.IsTrue(to.IsSearchCriteriaVisible);
            Assert.IsFalse(to.IsBetweenCriteriaVisible);
            Assert.IsTrue(to.IsSingleMatchCriteriaVisible);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_UpdateMatchVisibility_ThreeArgs_ShowsBetween()
        {
            var to = new DecisionTO();

            DecisionTO.UpdateMatchVisibility(to, "Is Between", DecisionTO.Whereoptions);

            Assert.IsTrue(to.IsSearchCriteriaVisible);
            Assert.IsTrue(to.IsBetweenCriteriaVisible);
            Assert.IsFalse(to.IsSingleMatchCriteriaVisible);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void DecisionTO_Whereoptions_IsPopulated()
        {
            // Defensive: confirms reflection-based registration found at least the basic operators.
            Assert.IsNotNull(DecisionTO.Whereoptions);
            Assert.IsTrue(DecisionTO.Whereoptions.Count > 0);
        }

        // ---------- Helpers ---------------------------------------------------

        static bool IsEmptyRuleSet(IRuleSet rs)
        {
            var concrete = rs as Dev2.Providers.Validation.Rules.RuleSet;
            return concrete != null && concrete.Rules.Count == 0;
        }
    }
}
