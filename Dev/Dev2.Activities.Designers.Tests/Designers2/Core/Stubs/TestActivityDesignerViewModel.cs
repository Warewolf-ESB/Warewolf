/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Stubs
{
    public class TestActivityDesignerViewModel : ActivityDesignerViewModel
    {
        public TestActivityDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public TestActivityDesignerViewModel(ModelItem modelItem, Action<Type> showExampleWorkflow)
            : base(modelItem)
        {
        }

        public bool IsSmallViewActive => ShowSmall;

        public override void Validate()
        {
        }

        #region Overrides of ActivityDesignerViewModel

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion
    }
}
