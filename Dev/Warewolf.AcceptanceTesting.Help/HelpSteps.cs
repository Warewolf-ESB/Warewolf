using System;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Help
{
    [Binding]
    public class HelpSteps
    {
        [BeforeFeature("Help")]
        public static void SetupHelpDependencies()
        {
            var bootstrapper = new UnityBootstrapperForHelpTesting();
            bootstrapper.Run();
            FeatureContext.Current.Add("container", bootstrapper.Container);
            var view = bootstrapper.Container.Resolve<IHelpView>();
            var window = new Window();
            window.Content = view;
            var app = Application.Current;
            app.MainWindow = window;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var helpView = (HelpView)Application.Current.MainWindow.Content;
                Assert.IsNotNull(helpView);
                Assert.IsNotNull(helpView.DataContext);
                Assert.IsInstanceOfType(helpView.DataContext, typeof(IHelpWindowViewModel));
                Application.Current.Shutdown();
            }));

            Application.Current.Run(Application.Current.MainWindow);
        }


        [BeforeScenario("Help")]
        public void SetupForHelp()
        {
            var container = FeatureContext.Current.Get<IUnityContainer>("container");
            var helpView = container.Resolve<IHelpView>();
            ScenarioContext.Current.Add("helpView", helpView);

        }

        [Given(@"help is visible")]
        public void GivenHelpIsVisible()
        {
            var helpView = ScenarioContext.Current.Get<IHelpView>("helpView");
            Assert.IsNotNull(helpView);
            Assert.IsNotNull(helpView.DataContext);    
        }

        [When(@"main window is selected")]
        public void WhenMainWindowIsSelected()
        {
            
        }

        [Then(@"the default help of ""(.*)"" is displayed")]
        public void ThenTheDefaultHelpOfIsDisplayed(string expectedHelpText)
        {
            var helpView = ScenarioContext.Current.Get<IHelpView>("helpView");
            var currentHelpText = helpView.GetCurrentHelpText();
            StringAssert.Contains(currentHelpText,expectedHelpText);
        }
    }
}
