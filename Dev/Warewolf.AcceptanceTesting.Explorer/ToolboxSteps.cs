using System;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Studio.Core.View_Interfaces;
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
            var window = new Window();
            window.Content = view;
            var app = Application.Current;
            app.MainWindow = window;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var viewWindow = (ExplorerView)Application.Current.MainWindow.Content;
                Assert.IsNotNull(viewWindow);
                Assert.IsNotNull(viewWindow.DataContext);
                Assert.IsInstanceOfType(viewWindow.DataContext, typeof(IToolboxViewModel));
                Application.Current.Shutdown();
            }));
            
            Application.Current.Run(Application.Current.MainWindow);
        }


        [BeforeScenario("Toolbox")]
        public void SetupForExplorer()
        {
            var container = FeatureContext.Current.Get<IUnityContainer>("container");
            var view = container.Resolve<IToolboxView>();
            ScenarioContext.Current.Add("toolBoxView", view);
            
        }

    }
}