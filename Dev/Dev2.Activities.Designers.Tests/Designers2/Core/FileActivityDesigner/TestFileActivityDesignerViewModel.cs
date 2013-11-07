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
        public bool ValidateInputPathIsRequired { get; private set; }
        protected override void ValidateInputPath(bool isRequired = false)
        {
            ValidateInputPathHitCount++;
            ValidateInputPathIsRequired = isRequired;
            base.ValidateInputPath(isRequired);
        }

        public int ValidateOutputPathHitCount { get; private set; }
        public bool ValidateOutputPathIsRequired { get; private set; }
        protected override void ValidateOutputPath(bool isRequired = false)
        {
            ValidateOutputPathHitCount++;
            ValidateOutputPathIsRequired = isRequired;
            base.ValidateOutputPath(isRequired);
        }

        public int ValidatePathHitCount { get; private set; }
        public bool ValidatePathIsRequired { get; private set; }
        protected override string ValidatePath(string label, string path, Action onError, bool pathIsRequired = false)
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

        public void TestValidateInputPath(bool isRequired)
        {
            base.ValidateInputPath(isRequired);
        }

        public void TestValidateOutputPath(bool isRequired)
        {
            base.ValidateOutputPath(isRequired);
        }

        public void TestValidateInputAndOutputPaths(bool isOutputPathRequired)
        {
            base.ValidateInputAndOutputPaths(isOutputPathRequired);
        }
    }
}