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