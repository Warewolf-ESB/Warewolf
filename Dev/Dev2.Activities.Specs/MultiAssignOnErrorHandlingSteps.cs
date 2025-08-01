/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://www.warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Activities.Statements;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Data.TO;
using Warewolf.Storage;
using Dev2.DynamicServices;

namespace Dev2.Activities.Specs
{
    [Binding]
    public class MultiAssignOnErrorHandlingSteps : RecordSetBases
    {
        public MultiAssignOnErrorHandlingSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
        }

        // NOTE: Removed duplicate step definitions that conflict with:
        // - WorkflowExecutionSteps.GivenIHaveAWorkflow
        // - OnErrorFrameworkSteps.GivenHasOnErrorVariable
        // - OnErrorFrameworkSteps.GivenHasOnErrorWorkflow  
        // - OnErrorFrameworkSteps.GivenHasIsEndedOnError
        // - OnErrorFrameworkSteps.ThenTheExecutionHasError
        // - OnErrorFrameworkSteps.ThenVariableEquals
        //
        // The generic step definitions in those classes will handle the MultiAssign scenarios
        // through the common activity list infrastructure.

        protected override void BuildDataList()
        {
            // using workflow composition method
        }
    }
}