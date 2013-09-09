using System.Collections.Generic;
using Dev2.Activities.Preview;
using Dev2.Activities.QuickVariableInput;
using Dev2.Providers.Errors;
using Moq;

namespace Dev2.Activities.Designers.Tests.QuickVariableInput
{
    public class QuickVariableInputViewModelMock : QuickVariableInputViewModel
    {
        public QuickVariableInputViewModelMock(IActivityCollectionViewModel activityCollectionViewModel = null)
            : base(activityCollectionViewModel ?? new Mock<IActivityCollectionViewModel>().Object)
        {
            ValidationErrorsValue = new List<IErrorInfo>();
        }

        public int DoAddHitCount { get; private set; }

        protected override void DoAdd(object o)
        {
            DoAddHitCount++;
            base.DoAdd(o);
        }

        public int DoClearHitCount { get; private set; }

        protected override void DoClear(object o)
        {
            DoClearHitCount++;
            base.DoClear(o);
        }

        public int DoPreviewHitCount { get; private set; }

        public void DoPreview()
        {
            DoPreview(this, new PreviewRequestedEventArgs());
        }

        protected override void DoPreview(object sender, PreviewRequestedEventArgs args)
        {
            DoPreviewHitCount++;
            base.DoPreview(sender, args);
        }

        public int GetPreviewOutputHitCount { get; private set; }

        protected override string GetPreviewOutput()
        {
            GetPreviewOutputHitCount++;
            return base.GetPreviewOutput();
        }

        public int ValidationErrorsHitCount { get; private set; }
        public List<IErrorInfo> ValidationErrorsValue { get; private set; }

        public override IEnumerable<IErrorInfo> ValidationErrors()
        {
            ValidationErrorsHitCount++;
            return ValidationErrorsValue;
        }

    }
}
