using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.InterfaceImplementors;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Windows;

namespace Dev2.Core.Tests.Custom_Dev2_Controls.Intellisense
{
    [TestClass]
    public class IntellisenseTextBoxTests
    {
        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            Monitor.Enter(DataListSingletonTest.DataListSingletonTestGuard);

            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Monitor.Exit(DataListSingletonTest.DataListSingletonTestGuard);
        }

        static object _vtLock = new object();

        //BUG 9639
        [TestMethod]
        // This test is here for when the designers load. The check is to prevent them from hammering the providers on load ;)
        public void IntellisenseBoxDoesntQueryProvidersWhenTextLengthIsZero()
        {
            lock (_vtLock)
            {
                Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
                intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>())).Verifiable();

                IntellisenseTextBox textBox = new IntellisenseTextBox();
                textBox.CreateVisualTree();
                textBox.IntellisenseProvider = intellisenseProvider.Object;
                textBox.Text = "";

                // Ensure the get results method is never called, mimics a initalize event from the design surface ;)
                intellisenseProvider.Verify(s => s.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()), Times.Exactly(0));  
            }
        }


        //BUG 8761
        [TestMethod]
        public void IntellisenseBoxDoesntCrashWhenGettingResultsGivenAProviderThatThrowsAnException()
        {
            lock (_vtLock)
            {
                Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
                intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                                    .Throws(new Exception());

                IntellisenseTextBox textBox = new IntellisenseTextBox();
                textBox.CreateVisualTree();
                textBox.IntellisenseProvider = intellisenseProvider.Object;
                textBox.Text = "[[City([[Scalar]]).Na";

                // When exceptions are thrown, no results are to be displayed
                Assert.AreEqual(0, textBox.Items.Count);
                //The desired result is that an exception isn't thrown


                // GetIntellisenseResults -> OnIntellisenseProviderChanged
            }
        }

        //BUG 8761
        [TestMethod]
        public void IntellisenseBoxDoesntCrashWhenInsertingResultsGivenAProviderThatThrowsAnException()
        {
            lock (_vtLock)
            {
                Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
                intellisenseProvider.Setup(
                    a => a.PerformResultInsertion(It.IsAny<string>(), It.IsAny<IntellisenseProviderContext>()))
                                    .Throws(new Exception());
                intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);

                IntellisenseProviderResult intellisenseProviderResult =
                    new IntellisenseProviderResult(intellisenseProvider.Object, "City", "cake");

                IntellisenseTextBox textBox = new IntellisenseTextBox();
                textBox.CreateVisualTree();
                textBox.InsertItem(intellisenseProviderResult, true);

                // When exepctions are thrown, no results are to be displayed
                Assert.AreEqual(0, textBox.Items.Count, "Expected [ 0 ] But got [ " + textBox.Items.Count + " ]");
                //The desired result is that an exception isn't thrown
            }
        }
        
        [TestMethod]
        public void TextContaningTabIsPasedIntoAnIntellisenseTextBoxExpectedTabInsertedEventIsRaised()
        {
            lock (_vtLock)
            {
                bool eventRaised = false;
                IntellisenseTextBox sender = null;
                EventManager.RegisterClassHandler(typeof (IntellisenseTextBox), IntellisenseTextBox.TabInsertedEvent,
                                                  new RoutedEventHandler((s, e) =>
                                                      {
                                                          eventRaised = true;
                                                          sender = s as IntellisenseTextBox;
                                                      }));

                Clipboard.SetText("Cake\t");

                IntellisenseTextBox textBox = new IntellisenseTextBox();
                textBox.CreateVisualTree();

                textBox.Paste();

                Assert.IsTrue(eventRaised,
                              "The 'IntellisenseTextBox.TabInsertedEvent' wasn't raised when text containing a tab was pasted into the IntellisenseTextBox.");
                Assert.AreEqual(textBox, sender,
                                "The IntellisenseTextBox in which the text containg a tab was pasted was different from the one which raised teh event.");
            }
        }

        [TestMethod]
        public void TextContaningNoTabIsPasedIntoAnIntellisenseTextBoxExpectedTabInsertedEventNotRaised()
        {
            lock (_vtLock)
            {
                bool eventRaised = false;
                EventManager.RegisterClassHandler(typeof (IntellisenseTextBox), IntellisenseTextBox.TabInsertedEvent,
                                                  new RoutedEventHandler((s, e) =>
                                                      {
                                                          eventRaised = true;
                                                      }));

                Clipboard.SetText("Cake");

                IntellisenseTextBox textBox = new IntellisenseTextBox();
                textBox.CreateVisualTree();

                textBox.Paste();

                Assert.IsFalse(eventRaised,
                               "The 'IntellisenseTextBox.TabInsertedEvent' was raised when text that didn't contain a tab was pasted into the IntellisenseTextBox.");
            }
        }

        //08.04.2013: Ashley Lewis - Bug 6731
        [TestMethod]
        public void UpdateTextboxLayoutWithInvalidTextExpectedHasError()
        {
            //Initialize
            var textBox = new IntellisenseTextBox { IntellisenseProvider = new DefaultIntellisenseProvider(), Text = "[[ ]]" };

            //Assert
            Assert.IsTrue(textBox.HasError, "Invalid textbox is not showing an error");
        }

        #endregion Test Initialization
    }
}
