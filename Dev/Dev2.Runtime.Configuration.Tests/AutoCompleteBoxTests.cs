using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Dev2.Runtime.Configuration.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class AutoCompleteBoxTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_SetMinimumPrefixLength")]
        public void AutoCompleteBox_SetMinimumPrefixLength_WhenTextLengthGreaterThanOrEqual_ShouldFilter()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> {"item1","myValues","anotherThing"};
            autoCompleteBox.MinimumPrefixLength = 4;
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            //------------Execute Test---------------------------
            autoCompleteBox.Text = "item";
            //------------Assert Results-------------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(1,filteredList.Count);
            Assert.AreEqual("item1",filteredList[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_SetMinimumPrefixLength")]
        public void AutoCompleteBox_SetMinimumPrefixLength_WhenTextLengthLessThan_ShouldNotFilter()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> {"item1","myValues","anotherThing"};
            autoCompleteBox.MinimumPrefixLength = 4;
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            //------------Execute Test---------------------------
            autoCompleteBox.Text = "ite";
            //------------Assert Results-------------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(0,filteredList.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_DefaultText")]
        public void AutoCompleteBox_DefaultText_Set_ShouldSetValue()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();

            //------------Execute Test---------------------------
            autoCompleteBox.DefaultText = "DText";
            //------------Assert Results-------------------------
            Assert.AreEqual("DText",autoCompleteBox.DefaultText);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_Text")]
        public void AutoCompleteBox_WhenText_ShouldFilter()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> { "item1", "myValues", "anotherThing" };
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            //------------Execute Test---------------------------
            autoCompleteBox.Text = "t";
            //------------Assert Results-------------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(2, filteredList.Count);
            Assert.AreEqual("item1", filteredList[0]);
            Assert.AreEqual("anotherThing", filteredList[1]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_Text")]
        public void AutoCompleteBox_WhenSetSelectedItem_ShouldUpdateText()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> { "item1", "myValues", "anotherThing" };
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            autoCompleteBox.Text = "t";
            //------------Assert Preconditions-------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(2, filteredList.Count);
            Assert.AreEqual("item1", filteredList[0]);
            Assert.AreEqual("anotherThing", filteredList[1]);
            //------------Execute Test---------------------------
            autoCompleteBox.SelectedItem = filteredList[1];
            //------------Assert Results-------------------------
            Assert.AreEqual("anotherThing", autoCompleteBox.Text);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_Text")]
        public void AutoCompleteBox_WhenCustomSelection_ShouldNotUpdateText()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> { "item1", "myValues", "anotherThing" };
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            autoCompleteBox.Text = "t";
            autoCompleteBox.CustomSelection = true;
            //------------Assert Preconditions-------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(2, filteredList.Count);
            Assert.AreEqual("item1", filteredList[0]);
            Assert.AreEqual("anotherThing", filteredList[1]);
            //------------Execute Test---------------------------
            autoCompleteBox.SelectedItem = filteredList[1];
            //------------Assert Results-------------------------
            Assert.AreEqual("t", autoCompleteBox.Text);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_Text")]
        public void AutoCompleteBox_WhenTextFilterSet_ShouldUseFilterGetItems()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> { "item1", "myValues", "anotherThing" };
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            autoCompleteBox.TextFilter = (search, item) =>
            {
                if (search == "item2")
                {
                    return true;
                }
                return false;
            };
            //------------Execute Test---------------------------
            autoCompleteBox.Text = "item2";
            //------------Assert Results-------------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(3, filteredList.Count);
            Assert.AreEqual("item1", filteredList[0]);
            Assert.AreEqual("myValues", filteredList[1]);
            Assert.AreEqual("anotherThing", filteredList[2]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_Text")]
        public void AutoCompleteBox_WhenTextFilterSet_ShouldUseFilterNoItes()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> { "item1", "myValues", "anotherThing" };
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            autoCompleteBox.TextFilter = (search, item) =>
            {
                if (search == "item2")
                {
                    return true;
                }
                return false;
            };
            //------------Execute Test---------------------------
            autoCompleteBox.Text = "item1";
            //------------Assert Results-------------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(0, filteredList.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_Text")]
        public void AutoCompleteBox_WhenItemFilterSet_ShouldUseFilterGetItems()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> { "item1", "myValues", "anotherThing" };
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            autoCompleteBox.ItemFilter = (search, item) =>
            {
                if (search == "item2")
                {
                    return true;
                }
                return false;
            };
            //------------Execute Test---------------------------
            autoCompleteBox.Text = "item2";
            //------------Assert Results-------------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(3, filteredList.Count);
            Assert.AreEqual("item1", filteredList[0]);
            Assert.AreEqual("myValues", filteredList[1]);
            Assert.AreEqual("anotherThing", filteredList[2]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_Text")]
        public void AutoCompleteBox_WhenItemFilterSet_ShouldUseFilterNoItes()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> { "item1", "myValues", "anotherThing" };
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            autoCompleteBox.ItemFilter = (search, item) =>
            {
                if (search == "item2")
                {
                    return true;
                }
                return false;
            };
            //------------Execute Test---------------------------
            autoCompleteBox.Text = "item1";
            //------------Assert Results-------------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(0, filteredList.Count);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_PopulateComplete")]
        public void AutoCompleteBox_WhenPopulateComplete_ShouldUpdateTextCompletion()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> { "item1", "myValues", "anotherThing" };
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.StartsWith;
            autoCompleteBox.IsTextCompletionEnabled = true;
            autoCompleteBox.TextBox = new TextBox();            
            autoCompleteBox.ItemFilter = (search, item) =>
            {
                if (search == "item1")
                {
                    return true;
                }
                return false;
            };
            autoCompleteBox.Text = "item1";
            autoCompleteBox.TextBox.SelectionStart = 5;
            var userCalledPopulateField = autoCompleteBox.GetType().GetField("_userCalledPopulate", System.Reflection.BindingFlags.NonPublic
                                                                                   | System.Reflection.BindingFlags.Instance);
            userCalledPopulateField.SetValue(autoCompleteBox, true);
            var textSelectionStartField = autoCompleteBox.GetType().GetField("_textSelectionStart", System.Reflection.BindingFlags.NonPublic
                                                                                   | System.Reflection.BindingFlags.Instance);
            textSelectionStartField.SetValue(autoCompleteBox, 0);
            //------------Execute Test---------------------------
            autoCompleteBox.PopulateComplete();
            //------------Assert Results-------------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(3, filteredList.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AutoCompleteBox_PopulateComplete")]
        public void AutoCompleteBox_WhenRefreshView_ShouldInsertResults()
        {
            //------------Setup for test--------------------------
            var autoCompleteBox = new AutoCompleteBox();
            autoCompleteBox.ItemsSource = new List<string> { "item1", "myValues", "anotherThing" };
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            autoCompleteBox.TextFilter = null;
            autoCompleteBox.ItemFilter = (search, item) =>
            {
                if (search == "item1")
                {
                    return true;
                }
                return false;
            };
            
            var viewField = autoCompleteBox.GetType().GetField("_view", System.Reflection.BindingFlags.NonPublic
                                                                                  | System.Reflection.BindingFlags.Instance);
            viewField.SetValue(autoCompleteBox, new ObservableCollection<object> { "item2", "bob" });

            //------------Execute Test---------------------------
            autoCompleteBox.Text = "item1";
            //------------Assert Results-------------------------
            var filteredList = autoCompleteBox.View;
            Assert.IsNotNull(filteredList);
            Assert.AreEqual(3, filteredList.Count);
        }
    }
}
