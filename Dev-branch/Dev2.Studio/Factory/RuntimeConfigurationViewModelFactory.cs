using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.ViewModels.Configuration;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Factory
{
    public static class RuntimeConfigurationViewModelFactory
    {
        public static RuntimeConfigurationViewModel CreateRuntimeConfigurationViewModel(IEnvironmentModel environmentModel)
        {
            return new RuntimeConfigurationViewModel(environmentModel);
        }
    }
}
