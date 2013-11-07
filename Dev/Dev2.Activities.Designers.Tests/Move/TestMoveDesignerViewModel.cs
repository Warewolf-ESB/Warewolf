using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Move;

namespace Dev2.Activities.Designers.Tests.Move
{
    public class TestMoveDesignerViewModel : MoveDesignerViewModel
    {
        public TestMoveDesignerViewModel(ModelItem modelItem)
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