/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Interfaces;
using System;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("ControlFlow-Gate", "Gate", ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Gate")]
    public class GateActivity : DsfActivityAbstract<string>, IEquatable<GateActivity>
    {
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals(GateActivity other)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397);
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new NotImplementedException();
        }

        public override List<string> GetOutputs()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<StateVariable> GetState()
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            throw new NotImplementedException();
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            throw new NotImplementedException();
        }
    }
}
