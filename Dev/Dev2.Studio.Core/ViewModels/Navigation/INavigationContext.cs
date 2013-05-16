using System;

namespace Dev2.Studio.Core.ViewModels.Navigation
{
    public interface INavigationContext
    {
        Guid? Context { get; }
    }
}