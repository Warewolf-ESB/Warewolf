
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
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.Utils
{
    public class MockDsfNativeActivity : DsfNativeActivity<string>
    {

        public bool IsDebugStateNull { get; set; }

        public MockDsfNativeActivity(bool isExecuteAsync, string displayName)
            : base(isExecuteAsync, displayName)
        {
        }

        public MockDsfNativeActivity(bool isExecuteAsync, string displayName, IDebugDispatcher debugDispatcher)
            : base(isExecuteAsync, displayName, debugDispatcher)
        {
        }

        public MockDsfNativeActivity()
            : base(false, string.Empty)
        {
        }

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        protected override void OnExecutedCompleted(NativeActivityContext context, bool hasError, bool isResumable)
        {
            IDebugState state = base.GetDebugState();
            if(state == null)
            {
                IsDebugStateNull = true;
            }
            base.OnExecutedCompleted(context, hasError, isResumable);

        }

        #endregion
    }
}
