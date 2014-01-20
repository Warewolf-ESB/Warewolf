using Dev2.Dialogs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Workflow;
using Moq;

namespace Dev2.Core.Tests.Dialogs
{
    public class TestResourcePickerDialogOkay : ResourcePickerDialog
    {
        public TestResourcePickerDialogOkay()
            : base(enDsfActivityType.Service)
        {
        }

        public DsfActivityDropViewModel CreateDialogDataContext { get; set; }

        protected override IDialog CreateDialog(DsfActivityDropViewModel dataContext)
        {
            dataContext.DialogResult = ViewModelDialogResults.Okay;
            dataContext.SelectedResourceModel = new Mock<IContextualResourceModel>().Object;

            CreateDialogDataContext = dataContext;

            var dialog = new Mock<IDialog>();
            return dialog.Object;
        }
    }
}