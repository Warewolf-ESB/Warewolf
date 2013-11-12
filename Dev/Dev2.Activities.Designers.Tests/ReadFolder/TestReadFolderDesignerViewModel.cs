using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.ReadFolder;

namespace Dev2.Activities.Designers.Tests.ReadFolder
{
    public class TestReadFolderDesignerViewModel : ReadFolderDesignerViewModel
    {
        public TestReadFolderDesignerViewModel(ModelItem modelItem)
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

        public bool IsFilesSelected { set { SetProperty(value); } get { return GetProperty<bool>(); } }
    }

}