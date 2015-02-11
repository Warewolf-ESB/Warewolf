using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Unity;
using Moq;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.Models.Toolbox;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.ViewModels.ToolBox;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer
{
    internal class UnityBootstrapperForSaveDialogTesting : UnityBootstrapperForTesting
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            var view = new RequestServiceNameView();
            Container.RegisterInstance<IRequestServiceNameViewModel>(new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object,view,Guid.NewGuid()));
            Container.RegisterInstance<IRequestServiceNameView>(view);
        }
    }
}