using System;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.ViewModels.Navigation
{
    public interface INavigationContext
    {
        Guid? Context { get; }
        void Update(IEnvironmentModel environmentModel);
    }
}