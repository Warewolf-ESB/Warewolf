
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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Factories;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Moq;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels;

namespace Dev2.Core.Tests {
    /// <summary>
    /// Summary description for ConfigureDecisionViewModelTest
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class ConfigureDecisionViewModelTest {

        private IConfigureDecisionViewModel test;
        
        public ConfigureDecisionViewModelTest() {
            
        } 

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional result attributes
        [TestInitialize()]
        public void EnvironmentTestsInitialize() {
            test = new ConfigureDecisionViewModel();
        }
        //
        #endregion

        #region Positive Test Cases


        #endregion Positive Test Cases

        #region Negative Test Cases


        #endregion Negative Test Cases

    }
}
