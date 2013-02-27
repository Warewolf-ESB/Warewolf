using Dev2.Core.Tests.Utils;
using Dev2.Studio.AppResources.Behaviors;
using Dev2.Studio.Core.Messages;
using Infragistics.Windows.DockManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Behaviors
{
    [TestClass]
    public class XamlDockManagerLayoutPersistenceBehaviorTests
    {
        [TestMethod]
        public void ResetLayoutExpectedLayoutResturnsToWhatItWasAfterInitialized()
        {
            // Setup MEF
            CompositionInitializer.DefaultInitialize();

            // Setup dock manager
            XamDockManager dockManager = new XamDockManager();
            dockManager.CreateVisualTree();

            ContentPane contentPane = new ContentPane()
            {
                Name = "content",
                IsPinned = true,
            };

            SplitPane pane = new SplitPane()
            {
                Name = "split",
            };

            pane.Panes.Add(contentPane);
            dockManager.Panes.Add(pane);

            // Setup attached behavior, this will record the initial layout
            XamlDockManagerLayoutPersistenceBehavior xamlDockManagerLayoutPersistenceBehavior = new XamlDockManagerLayoutPersistenceBehavior();
            xamlDockManagerLayoutPersistenceBehavior.Attach(dockManager);
            xamlDockManagerLayoutPersistenceBehavior.LayoutName = "test";

            // Modify the layout
            contentPane.IsPinned = false;

            // Reset the layout
            xamlDockManagerLayoutPersistenceBehavior.Handle(new ResetLayoutMessage(dockManager));

            //Assert that the layout has returned to it's original state
            Assert.IsTrue(contentPane.IsPinned, "Layout failed to reset.");
        }
    }
}
