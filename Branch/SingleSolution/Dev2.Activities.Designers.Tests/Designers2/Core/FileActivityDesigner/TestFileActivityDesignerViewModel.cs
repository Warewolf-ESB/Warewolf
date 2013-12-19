using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.FileActivityDesigner
{
    public class TestFileActivityDesignerViewModel : FileActivityDesignerViewModel
    {
        public TestFileActivityDesignerViewModel(ModelItem modelItem, string inputPathLabel, string outputPathLabel)
            : base(modelItem, inputPathLabel, outputPathLabel)
        {
        }

        public int ValidateInputPathHitCount { get; private set; }
        protected override void ValidateInputPath()
        {
            ValidateInputPathHitCount++;
            base.ValidateInputPath();
        }

        public int ValidateOutputPathHitCount { get; private set; }
        protected override void ValidateOutputPath()
        {
            ValidateOutputPathHitCount++;
            base.ValidateOutputPath();
        }

        public int ValidatePathHitCount { get; private set; }
        public bool ValidatePathIsRequired { get; private set; }
        protected override string ValidatePath(string label, string path, Action onError, bool pathIsRequired)
        {
            ValidatePathHitCount++;
            ValidatePathIsRequired = pathIsRequired;
            return base.ValidatePath(label, path, onError, pathIsRequired);
        }

        public override void Validate()
        {
            throw new NotImplementedException();
        }

        public string TestValidatePath(string label, string path, Action onError, bool pathIsRequired)
        {
            return base.ValidatePath(label, path, onError, pathIsRequired);
        }

        public void TestValidateInputPath()
        {
            base.ValidateInputPath();
        }

        public void TestValidateOutputPath()
        {
            base.ValidateOutputPath();
        }

        public void TestValidateInputAndOutputPaths()
        {
            base.ValidateInputAndOutputPaths();
        }
    }
}