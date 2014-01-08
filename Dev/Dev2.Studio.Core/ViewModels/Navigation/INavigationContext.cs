using System;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.ViewModels.Navigation
{
    public interface INavigationContext
    {
        Guid? Context { get; }
        void LoadEnvironmentResources(IEnvironmentModel environmentModel);
    }
}