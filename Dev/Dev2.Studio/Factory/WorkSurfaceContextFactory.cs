/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Factory;
using Dev2.Services.Events;
using Dev2.Settings;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.Triggers;
using Dev2.Utilities;

namespace Dev2.Studio.Factory
{
    public static class WorkSurfaceContextFactory
    {
        public static WorkSurfaceContextViewModel CreateResourceViewModel(IContextualResourceModel resourceModel) => CreateResourceViewModel(resourceModel, true, CustomContainer.Get<IPopupController>(), new AsyncWorker());
        public static WorkSurfaceContextViewModel CreateResourceViewModel(IContextualResourceModel resourceModel, bool createDesigner, IPopupController popupController, IAsyncWorker asyncWorker)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(resourceModel);

            var workSurfaceVm = new WorkflowDesignerViewModel(EventPublishers.Aggregator, resourceModel, new WorkflowHelper(), popupController, asyncWorker, createDesigner);

            var contextVm = new WorkSurfaceContextViewModel(key, workSurfaceVm)
                {
                    DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(resourceModel)
                };

            return contextVm;
        }


        /// <summary>
        /// Creates the work surface context view model, only use for surfaces that are unique per context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vm">The vm.</param>
        /// <param name="workSurfaceContext">The work surface context.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>3/6/2013</date>
        static WorkSurfaceContextViewModel CreateUniqueWorkSurfaceContextViewModel<T>
            (T vm, WorkSurfaceContext workSurfaceContext)
            where T : IWorkSurfaceViewModel
        {

            var key = WorkSurfaceKeyFactory.CreateKey(workSurfaceContext) as WorkSurfaceKey;
            if (vm is HelpViewModel && key != null)
            {
                key.ResourceID = Guid.Empty;
            }
            if (vm is TriggersViewModel || vm is SettingsViewModel)
            {
                key = WorkSurfaceKeyFactory.CreateEnvKey(workSurfaceContext, CustomContainer.Get<IShellViewModel>().ActiveServer.EnvironmentID) as WorkSurfaceKey;
            }

            return CreateWorkSurfaceContextViewModel(vm, workSurfaceContext, key);
        }

        static WorkSurfaceContextViewModel CreateWorkSurfaceContextViewModel<T>(T vm,
                                                                                        WorkSurfaceContext workSurfaceContext,
                                                                                        WorkSurfaceKey key)
            where T : IWorkSurfaceViewModel
        {
            var context = new WorkSurfaceContextViewModel(key, vm);

            if (!(vm is TriggersViewModel) && !(vm is SettingsViewModel))
            {
                vm.DisplayName = workSurfaceContext.GetDescription();
            }

            vm.WorkSurfaceContext = workSurfaceContext;
            return context;
        }

        public static WorkSurfaceContextViewModel Create<T>(WorkSurfaceContext workSurfaceContext, out T vmr)
            where T : IWorkSurfaceViewModel
        {
            var vm = Activator.CreateInstance<T>();
            var context = CreateUniqueWorkSurfaceContextViewModel(vm, workSurfaceContext);

            vmr = vm;
            return context;
        }
    }
}
