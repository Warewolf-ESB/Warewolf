
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Enums;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Workflows
{
    public class TestActivity : Activity, IDev2Activity
    {
        public string UniqueID { get; set; }

        public IList<IActionableErrorInfo> PerformValidation()
        {
            return null;
        }

        public enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }
    }

    public class TestDecisionActivity : Activity<bool>, IDev2Activity
    {
        public string UniqueID { get; set; }

        public IList<IActionableErrorInfo> PerformValidation()
        {
            return null;
        }

        public enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }
    }
}
