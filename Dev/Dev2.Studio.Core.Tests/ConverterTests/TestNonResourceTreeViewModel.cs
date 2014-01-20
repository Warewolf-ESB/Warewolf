using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;
using Moq;

namespace Dev2.Core.Tests.ConverterTests
{
    public class TestNonResourceTreeViewModel : AbstractTreeViewModel<string>
    {        
        public TestNonResourceTreeViewModel(ITreeNode parent)
            : base(new Mock<IEventAggregator>().Object, parent)
        {
        }

        public override IEnvironmentModel EnvironmentModel { get; protected set; }

        protected override ITreeNode CreateParent(string displayName)
        {
            return null;
        }
    }
}