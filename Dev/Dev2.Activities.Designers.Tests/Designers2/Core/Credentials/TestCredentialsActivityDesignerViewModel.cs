
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Credentials
{
    public class TestCredentialsActivityDesignerViewModel : CredentialsActivityDesignerViewModel
    {
        public TestCredentialsActivityDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public override void Validate()
        {
            throw new NotImplementedException();
        }

        public void TestValidateUserNameAndPassword()
        {
            base.ValidateUserNameAndPassword();
        }

        public void TestUpdateErrors(List<IActionableErrorInfo> errors)
        {
            UpdateErrors(errors);
        }
    }
}
