using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Unity;
using Moq;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer
{
    internal class UnityBootstrapperForSaveDialogTesting : UnityBootstrapperForTesting
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            var view = new RequestServiceNameView();
            Container.RegisterInstance<IRequestServiceNameViewModel>(new RequestServiceNameViewModel(new EnvironmentViewModel(Container.Resolve<IServer>(),Container.Resolve<IShellViewModel>()), view,Guid.NewGuid()));
            Container.RegisterInstance<IRequestServiceNameView>(view);
        }
    }
}