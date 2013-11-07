using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Copy;

namespace Dev2.Activities.Designers.Tests.Copy
{
    public class TestCopyDesignerViewModel : CopyDesignerViewModel
    {
        public TestCopyDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public int ValidateInputPathHitCount { get; private set; }
        public bool IsInputPathRequired { get; private set; }
        protected override void ValidateInputPath(bool isRequired = false)
        {
            ValidateInputPathHitCount++;
            IsInputPathRequired = isRequired;
            base.ValidateInputPath(isRequired);
        }

        public int ValidateOutputPathHitCount { get; private set; }
        public bool IsOutputPathRequired { get; private set; }
        protected override void ValidateOutputPath(bool isRequired = false)
        {
            ValidateOutputPathHitCount++;
            IsOutputPathRequired = isRequired;
            base.ValidateOutputPath(isRequired);
        }

        public int ValidateInputAndOutputPathHitCount { get; private set; }
        protected override void ValidInputAndOutputPaths(bool isOutputPathRequired = false)
        {
            ValidateInputAndOutputPathHitCount++;
            IsOutputPathRequired = isOutputPathRequired;
            base.ValidInputAndOutputPaths(isOutputPathRequired);
        }

        public int ValidateUserNameAndPasswordHitCount { get; private set; }
        protected override void ValidateUserNameAndPassword()
        {
            ValidateUserNameAndPasswordHitCount++;
            base.ValidateUserNameAndPassword();
        }


    }

}