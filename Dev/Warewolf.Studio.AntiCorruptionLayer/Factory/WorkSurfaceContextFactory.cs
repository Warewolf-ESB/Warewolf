
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
using Dev2.Helpers;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Warewolf.Studio.AntiCorruptionLayer.Factory;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Factory
{
    public static class WorkSurfaceContextFactory
    {
        public static WorkSurfaceContextViewModel CreateResourceViewModel(IContextualResourceModel resourceModel, bool createDesigner = true)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(resourceModel);

            //TODO Juries move to factory
            var workSurfaceVm = new WorkflowDesignerViewModel(resourceModel, createDesigner);

            var contextVm = new WorkSurfaceContextViewModel(key, workSurfaceVm)
                {
                    DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(resourceModel)
                };

            return contextVm;
        }

        public static WorkSurfaceContextViewModel CreateDeployViewModel(object input)
        {
//            WorkflowDesignerViewModel vm = DeployViewModelFactory.GetDeployViewModel(input);
//            var context = CreateUniqueWorkSurfaceContextViewModel<WorkflowDesignerViewModel>(vm, WorkSurfaceContext.DeployResources);
//            return context;
            return null;
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
        public static WorkSurfaceContextViewModel CreateUniqueWorkSurfaceContextViewModel<T>
            (WorkflowDesignerViewModel vm, WorkSurfaceContext workSurfaceContext)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(workSurfaceContext);
            return CreateWorkSurfaceContextViewModel<WorkflowDesignerViewModel>(vm, workSurfaceContext, key);
        }

        /// <summary>
        /// Creates the work surface context view model, only use for surfaces that are unique per server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vm">The vm.</param>
        /// <param name="workSurfaceContext">The work surface context.</param>
        /// <param name="serverID">The server ID.</param>
        /// <returns></returns>
        /// <date>3/6/2013</date>
        /// <author>
        /// Jurie.smit
        /// </author>
        public static WorkSurfaceContextViewModel CreateWorkSurfaceContextViewModelForServer<T>
            (T vm, WorkSurfaceContext workSurfaceContext, Guid serverID)
            where T : WorkflowDesignerViewModel
        {
            var key = WorkSurfaceKeyFactory.CreateKey(workSurfaceContext, serverID);
            return CreateWorkSurfaceContextViewModel<WorkflowDesignerViewModel>(vm, workSurfaceContext, key);
        }

        private static WorkSurfaceContextViewModel CreateWorkSurfaceContextViewModel<T>(WorkflowDesignerViewModel vm,
                                                                                        WorkSurfaceContext workSurfaceContext,
                                                                                        WorkSurfaceKey key)
        {
            var context = new WorkSurfaceContextViewModel(key, vm);
            //vm.DisplayName = workSurfaceContext.GetDescription();
            //vm.IconPath = workSurfaceContext.GetIconLocation();
            //vm.WorkSurfaceContext = workSurfaceContext;
            return context;
        }

        public static WorkSurfaceContextViewModel Create<T>(WorkSurfaceContext workSurfaceContext, Tuple<string, object>[] initParms)
            where T : WorkflowDesignerViewModel
        {
            var vm = Activator.CreateInstance<T>();
            PropertyHelper.SetValues(vm, initParms);
            var context = CreateUniqueWorkSurfaceContextViewModel<WorkflowDesignerViewModel>(vm, workSurfaceContext);
            return context;
        }

        public static WorkSurfaceContextViewModel Create<T>(WorkSurfaceKey key, Tuple<string, object>[] initParms)
            where T : WorkflowDesignerViewModel
        {
            var vm = Activator.CreateInstance<T>();
            PropertyHelper.SetValues(vm, initParms);
            var context = CreateWorkSurfaceContextViewModel<WorkflowDesignerViewModel>(vm, key.WorkSurfaceContext, key);
            return context;
        }
    }
}
