
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.Toolbox;
using Warewolf.Core;

// ReSharper disable CheckNamespace


namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
   [ToolDescriptorInfo("ControlFlow-Descision", "Decision", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Bob", "1.0.0.0", "c:\\", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]             
    public class DsfFlowDecisionActivity : DsfFlowNodeActivity<bool>
    {
        #region Ctor

        public DsfFlowDecisionActivity()
            : base("Decision")
        {
        }

        #endregion

        public override void UpdateForEachInputs(IList<System.Tuple<string, string>> updates, System.Activities.NativeActivityContext context)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<System.Tuple<string, string>> updates, System.Activities.NativeActivityContext context)
        {
            throw new System.NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new System.NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new System.NotImplementedException();
        }
    }



}
