using System;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core.QuickVariableInput;
using Dev2.Activities.Preview;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Activities.Designers.Tests.QuickVariableInput
{
    public class QuickVariableInputViewModelMock : QuickVariableInputViewModel
    {
        public QuickVariableInputViewModelMock(Action<IEnumerable<string>, bool> addToCollection = null, Action<List<IActionableErrorInfo>> setErrors = null)
            : base(addToCollection ?? ((source, overwrite) => { }))
        {
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

        public int ValidateHitCount { get; private set; }

        public override void Validate()
        {
            ValidateHitCount++;
        }
    }
}
