﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Dev2.Server.UI.Tests.DatePickerWizardUIMapClasses;

using System.Linq;
using System.Diagnostics;
using System.Management;
using System.IO;
using Microsoft.Win32;


namespace Dev2.Server.UI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class ServerBugTests
    {
        public ServerBugTests()
        {

        }

        [TestInitialize]
        public void CheckStartIsValid()
        {
            try
            {
                // Set default browser to IE for tests
                RegistryKey regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\shell\\Associations\\UrlAssociations\\http\\UserChoice", true);
                string browser = regkey.GetValue("Progid").ToString();
                if (browser != "IE.HTTP")
                {
                    regkey.SetValue("Progid", "IE.HTTP");
                }
            }
            catch (Exception ex)
            {
                // Some PC's don't have this value - We have to assume they only have IE :(
            }

            try
            {
                // Make sure no instances of IE are running (For Bug Test crashes)
                Process[] processList = System.Diagnostics.Process.GetProcessesByName("iexplore");
                foreach (Process p in processList)
                {
                    p.Kill();
                }
            }
            catch
            {
                throw new Exception("Error - Cannot close all instances of IE!");
            }
        }

        // Bug 5016
        [TestMethod]
        public void WebpartWizardDoesNotDisplayDefaultName()
        {
            System.Diagnostics.Process.Start("iexplore.exe", "http://127.0.0.1:1234/services/Date%20Picker.wiz");
            System.Threading.Thread.Sleep(2500); // Give time for IE to open
            var openWindowProcesses = System.Diagnostics.Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName == "iexplore");
            bool isValidName = false;
            foreach (var process in openWindowProcesses)
            {
                if (process.MainWindowTitle.ToUpper().Contains("DATE PICKER"))
                {
                    isValidName = true;
                    break;
                }
            }
            if (!isValidName)
            {
                Assert.Inconclusive("The webpage window lacks a valid name!");
            }
        }

        // Bug 5561
        [TestMethod]
        public void CursorRemainsConstantEnteringWebpartName()
        {
            System.Diagnostics.Process.Start("iexplore.exe", "http://127.0.0.1:1234/services/Date%20Picker.wiz");
            System.Threading.Thread.Sleep(2500); // Give time for IE to open
            DatePickerWizardUIMap datePickerWizardUIMap = new DatePickerWizardUIMap();
            datePickerWizardUIMap.ClickNameTextBox();
            Keyboard.SendKeys("1");
            Keyboard.SendKeys("4");
            Keyboard.SendKeys("{Left}");
            Keyboard.SendKeys("2");
            Keyboard.SendKeys("3");
            string nameText = datePickerWizardUIMap.GetNameTextBoxText();
            if (nameText != "1234")
            {
                Assert.Inconclusive("Error - The text is in the wrong order!");
            }
        }

        // Bug 5579
        [TestMethod]
        public void WorkflowWizard_OnEnter_Expected_NewWorkflow()
        {
            // Very broken test, since it uses the this.UIMap....

            /*
            System.Diagnostics.Process.Start("iexplore.exe", "http://127.0.0.1:1234/services/Dev2ServiceDetails?Dev2ServiceType=WorkFlow");
            System.Threading.Thread.Sleep(2500); // Give time for IE to open
            this.UIMap.WorkflowWebWizard_ClickNameTextBox();
            Keyboard.SendKeys("TestWorkFlow_OnEnter");
            this.UIMap.WorkflowWebWizard_ClickCategoryTextBox();
            Keyboard.SendKeys("CodedUITests");
            Keyboard.SendKeys("{ENTER}");
             */
        }

        // Bug 6084 - Commented out since its a Web-Based Bug
        [TestMethod]
        public void EnterNameClickDone()
        {
            /*
            System.Diagnostics.Process.Start("iexplore.exe", "http://127.0.0.1:1234/services/Date%20Picker.wiz");
            System.Threading.Thread.Sleep(2500); // Give time for IE to open
            DatePickerWizardUIMap datePickerWizardUIMap = new DatePickerWizardUIMap();
            datePickerWizardUIMap.ClickNameTextBox();
            this.UIMap.TypeTextInNameTextbox();
            this.UIMap.ClickDoneButton();
            UIMap.DialogExists_MissingRequiredInformation();
             */
        }

        // Bug 6657
        [TestMethod]
        public void DatabaseBrowseDoesNotReturnError()
        {
            System.Diagnostics.Process.Start("iexplore.exe", "http://127.0.0.1:1234/services/Dev2ServiceDetails?Dev2ServiceType=Database");
            System.Threading.Thread.Sleep(2500); // Give time for IE to open
            this.UIMap.DatabaseWizard_ClickServerBrowseButton();

            this.UIMap.AlertBoxDoesntExist();
        }

        // Bug 7846
        [TestMethod]
        public void OpeningStudioServerAndWorkflowWizardUsesSaneAmountsOfRAM_Expected_TestPasses()
        {
            // Get the server location
            string serverLocation = GetServerEXEFolder();
            string fileLocation = serverLocation + "Dev2.Server.exe";
            
            // Close it
            KillServer();

            // Wait for it to close
            System.Threading.Thread.Sleep(1000);

            // Reopen it
            Process p = new Process();
            ProcessStartInfo pSI = new ProcessStartInfo(fileLocation);

            pSI.WorkingDirectory = serverLocation;
            p.StartInfo = pSI;
            p.Start();
            string processTitle = String.Empty;
            
            // Make sure it's fully loaded
            System.Threading.Thread.Sleep(10000);

            // Start the Browser
            System.Diagnostics.Process.Start("http://127.0.0.1:1234/services/Dev2ServiceDetails?Dev2ServiceType=Workflow");
            System.Threading.Thread.Sleep(10000);

            double ramUsage = p.PeakWorkingSet64;
            ramUsage /= 1024;
            ramUsage /= 1024;
            if (ramUsage > 2000)
            {
                Assert.Fail("The RAM usage is too high!");
            }
        }

        private string GetServerEXEFolder()
        {
            var findDev2Studios = System.Diagnostics.Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith("Dev2.Server"));
            if (findDev2Studios.Count() != 1)
            {
                throw new Exception("Error - Cannot find location if more than 1 studio is open :(");
            }
            else
            {
                foreach (Process p in findDev2Studios)
                {
                    int processID = p.Id;
                    string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processID.ToString();
                    using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                    {
                        using (var results = searcher.Get())
                        {
                            ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                            if (mo != null)
                            {
                                string fileLocation = (string)mo["ExecutablePath"];
                                string folder = fileLocation.Substring(0, fileLocation.LastIndexOf(@"\") + 1);
                                return folder;
                            }
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        private void KillServer()
        {
            var findDev2Studios = System.Diagnostics.Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith("Dev2.Server"));
            if (findDev2Studios.Count() != 1)
            {
                throw new Exception("Error - Cannot find location if more than 1 studio is open :(");
            }
            else
            {
                foreach (Process p in findDev2Studios)
                {
                    p.Kill();
                }
            }
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
