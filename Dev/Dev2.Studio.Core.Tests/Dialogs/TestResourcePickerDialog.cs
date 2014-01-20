using Caliburn.Micro;
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

        public TestResourcePickerDialog(enDsfActivityType activityType, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, bool isFromDrop)
            : base(activityType, environmentRepository, eventPublisher, asyncWorker, isFromDrop)
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
