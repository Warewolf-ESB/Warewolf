
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
using System.Collections.Generic;
using Dev2.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.Utils
{
    /// <summary>
    /// Dummy activity for testing ;)
    /// </summary>
    public class DsfFakeActivity : DsfActivityAbstract<bool>
    {
        protected override void OnExecute(System.Activities.NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, System.Activities.NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, System.Activities.NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new NotImplementedException();
        }
    }
}
