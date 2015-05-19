
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
using Dev2.AppResources.Repositories;
using Dev2.ConnectionHelpers;
using Dev2.Dialogs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Threading;
using Moq;

namespace Dev2.Core.Tests.Dialogs
{
    public class TestResourcePickerDialog : ResourcePickerDialog
    {
        /// <summary>
        /// Creates a picker suitable for dropping from the toolbox.
        /// </summary>
        public TestResourcePickerDialog(enDsfActivityType activityType)
            : base(activityType)
        {
            var dialog = new Mock<IDialog>();
            dialog.Setup(d => d.ShowDialog()).Verifiable();
            CreateDialogResult = dialog.Object;
        }

        /// <summary>
        /// Creates a picker suitable for picking from the given environment.
        /// </summary>
        public TestResourcePickerDialog(enDsfActivityType activityType, IEnvironmentModel source)
            : base(activityType, source)
        {
        }

        public TestResourcePickerDialog(enDsfActivityType activityType, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, bool isFromDrop, IStudioResourceRepository studioResourceRepository)
            : base(activityType, environmentRepository, eventPublisher, asyncWorker, isFromDrop, studioResourceRepository, new Mock<IConnectControlSingleton>().Object)
        {
        }

        public IDialog CreateDialogResult { get; set; }
        public DsfActivityDropViewModel CreateDialogDataContext { get; set; }

        protected override IDialog CreateDialog(DsfActivityDropViewModel dataContext)
        {
            CreateDialogDataContext = dataContext;
            return CreateDialogResult;
        }
    }
}
