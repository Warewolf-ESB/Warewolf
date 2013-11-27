using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Rename;

namespace Dev2.Activities.Designers.Tests.Rename
{
    public class TestRenameDesignerViewModel : RenameDesignerViewModel
    {
        public TestRenameDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
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

        public int ValidateInputAndOutputPathHitCount { get; private set; }
        protected override void ValidateInputAndOutputPaths()
        {
            ValidateInputAndOutputPathHitCount++;
            base.ValidateInputAndOutputPaths();
        }

        public int ValidateUserNameAndPasswordHitCount { get; private set; }
        protected override void ValidateUserNameAndPassword()
        {
            ValidateUserNameAndPasswordHitCount++;
            base.ValidateUserNameAndPassword();
        }

        public int ValidateDestinationUsernameAndPasswordHitCount { get; private set; }
        protected override void ValidateDestinationUsernameAndPassword()
        {
            ValidateDestinationUsernameAndPasswordHitCount++;
            base.ValidateDestinationUsernameAndPassword();
        }


    }

}