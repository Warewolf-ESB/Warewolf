using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using System;

namespace Dev2.Studio.Factory
{
    public static class WorkSurfaceContextFactory
    {

        public static WorkSurfaceContextViewModel CreateResourceViewModel(IContextualResourceModel resourceModel)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(resourceModel);

            //TODO Juries move to factory
            var workSurfaceVM = new WorkflowDesignerViewModel(resourceModel)
                {
                    IconPath = ResourceHelper.GetIconPath(resourceModel),
                    DisplayName = resourceModel.ResourceName
                };

            var contextVM = new WorkSurfaceContextViewModel(key, workSurfaceVM)
                {
                    DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(resourceModel)
                };

            return contextVM;
        }

        public static WorkSurfaceContextViewModel CreateDeployViewModel(object input)
        {
            var vm = DeployViewModelFactory.GetDeployViewModel(input);
            var context = CreateWorkSurfaceContextViewModel(vm, WorkSurfaceContext.DeployResources);
            return context;
        }

        public static WorkSurfaceContextViewModel CreateRuntimeConfigurationViewModel(IEnvironmentModel environmentModel)
        {
            var vm = RuntimeConfigurationViewModelFactory.CreateRuntimeConfigurationViewModel(environmentModel);
            var context = CreateWorkSurfaceContextViewModel(vm, WorkSurfaceContext.Settings);
            return context;
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
        public static WorkSurfaceContextViewModel CreateWorkSurfaceContextViewModel<T>
            (T vm, WorkSurfaceContext workSurfaceContext)
            where T : IWorkSurfaceViewModel
        {
            var key = WorkSurfaceKeyFactory.CreateKey(workSurfaceContext);
            var context = new WorkSurfaceContextViewModel(key, vm);
            vm.DisplayName = workSurfaceContext.GetDescription();
            vm.IconPath = workSurfaceContext.GetIconLocation();
            return context;
        }

        public static WorkSurfaceContextViewModel Create<T>(WorkSurfaceContext workSurfaceContext, Tuple<string, object>[] initParms)
            where T : IWorkSurfaceViewModel
        {
            var vm = Activator.CreateInstance<T>();
            PropertyHelper.SetValues(vm, initParms);
            var context = CreateWorkSurfaceContextViewModel(vm, workSurfaceContext);
            return context;
        }

    }
}
