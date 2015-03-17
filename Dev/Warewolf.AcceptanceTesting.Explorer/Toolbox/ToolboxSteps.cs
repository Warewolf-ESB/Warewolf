using System;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer
{
    [Binding]    
    public class ToolboxSteps
    {
        [BeforeFeature("Toolbox")]
        public static void SetupToolboxDependencies()
        {
            var bootstrapper = new UnityBootstrapperForToolboxTesting();
            bootstrapper.Run();
            FeatureContext.Current.Add("container", bootstrapper.Container);
            var view = bootstrapper.Container.Resolve<IToolboxView>();
            var viewModel = bootstrapper.Container.Resolve<IToolboxViewModel>();

            FeatureContext.Current.Add(Utils.ViewNameKey, view);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, viewModel);
        }


        [BeforeScenario("Toolbox")]
        public void SetupForToolbox()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<IToolboxView>(Utils.ViewNameKey));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<IToolboxViewModel>(Utils.ViewModelNameKey));
            
        }

        [Given(@"""(.*)"" Toolbox is loaded")]
        public void GivenToolboxIsLoaded(string p0)
        {
            var view = Utils.GetView<IToolboxView>();
            Utils.ShowTheViewForTesting(view);
        }


    }
}