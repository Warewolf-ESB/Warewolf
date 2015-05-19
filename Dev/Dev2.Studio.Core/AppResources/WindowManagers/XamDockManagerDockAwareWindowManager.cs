
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Infragistics.Windows.DockManager;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.AppResources.WindowManagers
{
    public class XamDockManagerDockAwareWindowManager : WindowManager, IDockAwareWindowManager
    {
        private readonly Window _window;
        private readonly XamDockManager _dockManager;

        public XamDockManagerDockAwareWindowManager(Window window = null, XamDockManager dockManager = null)
        {
            _window = window;
            _dockManager = dockManager;
        }

        public void ShowDockedWindow(object viewModel, object context = null, bool selectWhenShown = true, InitialPaneLocation dockstate = InitialPaneLocation.DockedLeft)
        {
            ContentPane dockableWindow = CreateDockable(viewModel, context);
            SplitPane pane = XamDockManagerHelper.FindSplitPaneWithLocationOrCreate(GetDockingManager(_window), dockstate);

            pane.Panes.Add(dockableWindow);
            //If this is a new dockable location (there are no split panes for it)
            //we need to add it to the XamDockManager
            if(pane.Parent == null)
            {
                DockManager.Panes.Add(pane);
            }

            if(selectWhenShown)
            {
                dockableWindow.Activate();
            }

        }

        public void ShowFloatingWindow(object viewModel, object context = null, bool selectWhenShown = true)
        {
            ContentPane dockableWindow = CreateDockable(viewModel, context);
            SplitPane pane = new SplitPane();
            XamDockManager.SetInitialLocation(pane, InitialPaneLocation.DockableFloating);

            pane.Panes.Add(dockableWindow);
            DockManager.Panes.Add(pane);

            if(selectWhenShown)
            {
                dockableWindow.Activate();
            }
        }

        public void ShowDocumentWindow(object viewModel, object context = null, bool selectWhenShown = true)
        {
            ContentPane dockableWindow = CreateDockable(viewModel, context);
            TabGroupPane host = XamDockManagerHelper.FindTabGroupPane(GetDockingManager());

            host.Items.Add(dockableWindow);

            if(selectWhenShown)
            {
                dockableWindow.Activate();
            }
        }

        public XamDockManager DockManager
        {
            get { return GetDockingManager(); }
        }

        private static ContentPane CreateDockable(object rootModel, object context)
        {
            var view = EnsureDockWindow(ViewLocator.LocateForModel(rootModel, null, context));
            ViewModelBinder.Bind(rootModel, view, context);

            var haveDisplayName = rootModel as IHaveDisplayName;
            if(haveDisplayName != null && !ConventionManager.HasBinding(view, HeaderedContentControl.HeaderProperty))
            {
                Binding binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
                view.SetBinding(HeaderedContentControl.HeaderProperty, binding);
            }

            // ReSharper disable once ObjectCreationAsStatement
            new DockableWindowConductor(rootModel, view);

            return view;
        }

        private static ContentPane EnsureDockWindow(object view)
        {
            var window = view as ContentPane;

            if(window == null)
            {
                window = new ContentPane
                {
                    CloseAction = PaneCloseAction.RemovePane,
                    Content = view as UIElement
                };

                window.SetValue(View.IsGeneratedProperty, true);
            }

            return window;
        }

        /// <summary>
        /// Gets the parent window.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns>The parent window, or <see langword="null"/> if no parent window was found.</returns>
        private Window GetParentWindow(Window window)
        {
            Window parentWindow = _window ?? (Application.Current != null ? Application.Current.MainWindow : null);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            return parentWindow != window ? parentWindow : null;
        }

        /// <summary>
        /// Gets the dock site associated to the window.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns>The retrieved dock site.</returns>
        /// <exception cref="InvalidOperationException">No dock site could be retrieved.</exception>
        private XamDockManager GetDockingManager(Window window = null)
        {
            XamDockManager dockSite = _dockManager;

            if(dockSite == null)
            {
                Window parentWindow = GetParentWindow(window);

                if(parentWindow != null)
                    dockSite = parentWindow.FindChild<XamDockManager>();
            }

            if(dockSite == null)
                throw new InvalidOperationException("Unable to retrieve a docking manager");

            return dockSite;
        }

        private static class XamDockManagerHelper
        {
            static PaneLocation GetSplitPaneLocation(SplitPane pane)
            {
                return XamDockManager.GetPaneLocation(pane);
            }

            static SplitPane FindSplitPaneWithLocation(XamDockManager dockManager, PaneLocation location)
            {
                return dockManager.Panes.FirstOrDefault(p => GetSplitPaneLocation(p) == location);
            }

            public static SplitPane FindSplitPaneWithLocationOrCreate(XamDockManager dockManager, InitialPaneLocation location)
            {
                return FindSplitPaneWithLocationOrCreate(dockManager, location.ToPaneLocation());
            }

            static SplitPane FindSplitPaneWithLocationOrCreate(XamDockManager dockManager, PaneLocation location)
            {
                SplitPane pane = FindSplitPaneWithLocation(dockManager, location);

                if(pane != null)
                    return pane;

                pane = new SplitPane();
                XamDockManager.SetInitialLocation(pane, location.ToInitialPaneLocation());

                return pane;
            }

            public static TabGroupPane FindTabGroupPane(XamDockManager dockManager)
            {
                TabGroupPane tabs;

                dockManager.TryFindChild(out tabs);

                return tabs;
            }

        }

        /// <summary>
        ///   The dockable window conductor, used to allow for interaction between view and view model.
        /// </summary>
        private class DockableWindowConductor
        {
            #region Fields
            /// <summary>
            ///   The view.
            /// </summary>
            private readonly ContentPane _view;

            /// <summary>
            ///   The view model.
            /// </summary>
            private readonly object _viewModel;

            /// <summary>
            ///   The flag used to identify the view as closing.
            /// </summary>
            private bool _isClosing;

            /// <summary>
            ///   The flag used to determine if the view requested deactivation.
            /// </summary>
            private bool _isDeactivatingFromView;

            /// <summary>
            ///   The flag used to determine if the view model requested deactivation.
            /// </summary>
            private bool _isDeactivatingFromViewModel;
            #endregion

            /// <summary>
            ///   Initializes a new instance of the <see cref = "DockableWindowConductor" /> class.
            /// </summary>
            /// <param name = "viewModel">The view model.</param>
            /// <param name = "view">The view.</param>
            public DockableWindowConductor(object viewModel, ContentPane view)
            {
                _viewModel = viewModel;
                _view = view;

                var activatable = viewModel as IActivate;
                if(activatable != null)
                    activatable.Activate();

                var deactivatable = viewModel as IDeactivate;
                if(deactivatable != null)
                {
                    _view.Closed += OnClosed;
                    deactivatable.Deactivated += OnDeactivated;
                }

                var guard = viewModel as IGuardClose;
                if(guard != null)
                    view.Closing += OnClosing;
            }

            /// <summary>
            ///   Called when the view has been closed.
            /// </summary>
            /// <param name = "sender">The sender.</param>
            /// <param name = "e">The <see cref = "System.EventArgs" /> instance containing the event data.</param>
            private void OnClosed(object sender, System.EventArgs e)
            {
                _view.Closed -= OnClosed;
                _view.Closing -= OnClosing;

                if(_isDeactivatingFromViewModel)
                    return;

                var deactivatable = (IDeactivate)_viewModel;

                _isDeactivatingFromView = true;
                deactivatable.Deactivate(true);
                _isDeactivatingFromView = false;
            }

            /// <summary>
            ///   Called when the view has been deactivated.
            /// </summary>
            /// <param name = "sender">The sender.</param>
            /// <param name = "e">The <see cref = "Caliburn.Micro.DeactivationEventArgs" /> instance containing the event data.</param>
            private void OnDeactivated(object sender, DeactivationEventArgs e)
            {
                ((IDeactivate)_viewModel).Deactivated -= OnDeactivated;

                if(!e.WasClosed || _isDeactivatingFromView)
                    return;

                _isDeactivatingFromViewModel = true;
                _isClosing = true;
                _view.ExecuteCommand(ContentPaneCommands.Close);
                _isClosing = false;
                _isDeactivatingFromViewModel = false;
            }

            /// <summary>
            ///   Called when the view is about to be closed.
            /// </summary>
            /// <param name = "sender">The sender.</param>
            /// <param name = "e">The <see cref = "System.ComponentModel.CancelEventArgs" /> instance containing the event data.</param>
            private void OnClosing(object sender, Infragistics.Windows.DockManager.Events.PaneClosingEventArgs e)
            {
                var guard = (IGuardClose)_viewModel;

                if(_isClosing)
                {
                    _isClosing = false;
                    return;
                }

                bool shouldEnd = false;

                guard.CanClose(canClose => Execute.OnUIThread(() =>
                {
                    e.Cancel = !canClose;

                    shouldEnd = true;
                }));

                if(shouldEnd)
                    return;

                e.Cancel = true;
            }
        }
    }
}
