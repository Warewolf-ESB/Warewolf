using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.Network
{
    public interface IServiceEndpointGenerationStrategyProvider
    {
        void RegisterEndpointGenerationStrategies(IServiceLocator serviceLocator);
    }
}
