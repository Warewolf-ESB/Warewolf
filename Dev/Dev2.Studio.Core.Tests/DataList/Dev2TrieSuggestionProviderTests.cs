/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    [TestCategory("Studio Datalist Core")]
    public class Dev2TrieSuggestionProviderTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2TrieSuggestionProvider))]
        public void Dev2TrieSuggestionProvider_GetSuggestions_Should_Return_All_Types()
        {
            IEnumerable<string> variables = new List<string>
            {
                "[[var]]",
                "[[var2]]",
                "[[rec()]]",
                "[[rec().var]]",
                "[[rec().var2]]",
                "[[@Person]]",
                "[[@Person.Childs(*).name]]",
                "[[@Person.Childs().name]]",
            };

            var suggestionProvider = new Dev2TrieSuggestionProvider {VariableList = new ObservableCollection<string>(variables)};

            var suggestions = suggestionProvider.GetSuggestions("[", 0, false, enIntellisensePartType.None);

            Assert.IsNotNull(suggestions);

            var results = suggestions.ToList();
            Assert.AreEqual(10, results.ToList().Count);
            Assert.AreEqual("[[@Person]]", results.ToList()[0]);
            Assert.AreEqual("[[@Person.Childs(*)]]", results.ToList()[1]);
            Assert.AreEqual("[[@Person.Childs(*).name]]", results.ToList()[2]);
            Assert.AreEqual("[[@Person.Childs()]]", results.ToList()[3]);
            Assert.AreEqual("[[@Person.Childs().name]]", results.ToList()[4]);
            Assert.AreEqual("[[rec().var]]", results.ToList()[5]);
            Assert.AreEqual("[[rec().var2]]", results.ToList()[6]);
            Assert.AreEqual("[[rec()]]", results.ToList()[7]);
            Assert.AreEqual("[[var]]", results.ToList()[8]);
            Assert.AreEqual("[[var2]]", results.ToList()[9]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2TrieSuggestionProvider))]
        public void Dev2TrieSuggestionProvider_GetSuggestions_Should_Return_NonJsonObjects()
        {
            IEnumerable<string> variables = new List<string>
            {
                "[[var]]",
                "[[var2]]",
                "[[rec()]]",
                "[[rec().var]]",
                "[[rec().var2]]",
                "[[@Person]]",
                "[[@Person.Childs(*).name]]",
                "[[@Person.Childs().name]]",
            };

            var suggestionProvider = new Dev2TrieSuggestionProvider {VariableList = new ObservableCollection<string>(variables)};

            var fields = enIntellisensePartType.NonJsonObjects;
            var suggestions = suggestionProvider.GetSuggestions("[", 0, false, fields);

            Assert.IsNotNull(suggestions);

            var results = suggestions.ToList();
            Assert.AreEqual(5, results.ToList().Count);
            Assert.AreEqual("[[rec().var]]", results.ToList()[0]);
            Assert.AreEqual("[[rec().var2]]", results.ToList()[1]);
            Assert.AreEqual("[[rec()]]", results.ToList()[2]);
            Assert.AreEqual("[[var]]", results.ToList()[3]);
            Assert.AreEqual("[[var2]]", results.ToList()[4]);

        }
    }
}
