
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Caliburn.Micro;
using Infragistics.Windows.DockManager;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.WindowManagers
{
    public interface IDockAwareWindowManager : IWindowManager
    {
        /// <summary>
        ///   Shows a docked window.
        /// </summary>
        /// <param name = "viewModel">The view model.</param>
        /// <param name = "context">The context.</param>
        /// <param name="selectWhenShown">If set to <c>true</c> the window will be selected when shown.</param>
        /// <param name="dockstate"></param>
        void ShowDockedWindow(object viewModel,
                                                        object context = null,
                                                        bool selectWhenShown = true,
                                                        InitialPaneLocation dockstate = InitialPaneLocation.DockedLeft);

        /// <summary>
        ///   Shows a floating window.
        /// </summary>
        /// <param name = "viewModel">The view model.</param>
        /// <param name = "context">The context.</param>
        /// <param name="selectWhenShown">If set to <c>true</c> the window will be selected when shown.</param>
        void ShowFloatingWindow(object viewModel,
                                                        object context = null,
                                                        bool selectWhenShown = true);

        /// <summary>
        ///   Shows a document window.
        /// </summary>
        /// <param name = "viewModel">The view model.</param>
        /// <param name = "context">The context.</param>
        /// <param name="selectWhenShown">If set to <c>true</c> the window will be selected when shown.</param>
        void ShowDocumentWindow(object viewModel,
                                                        object context = null,
                                                        bool selectWhenShown = true);
    }

}
