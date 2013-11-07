using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.ReadFile;

namespace Dev2.Activities.Designers.Tests.ReadFile
{
    public class TestReadFileDesignerViewModel : ReadFileDesignerViewModel
    {
        public TestReadFileDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
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

        public int ValidateInputAndOutputPathHitCount { get; private set; }
        protected override void ValidateInputAndOutputPaths(bool isOutputPathRequired = false)
        {
            ValidateInputAndOutputPathHitCount++;
            ValidateOutputPathIsRequired = isOutputPathRequired;
            base.ValidateInputAndOutputPaths(isOutputPathRequired);
        }

        public int ValidateUserNameAndPasswordHitCount { get; private set; }
        protected override void ValidateUserNameAndPassword()
        {
            ValidateUserNameAndPasswordHitCount++;
            base.ValidateUserNameAndPassword();
        }


    }

}