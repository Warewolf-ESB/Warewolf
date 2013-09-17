using Dev2.Studio.Core.Interfaces;

namespace Dev2.Messages
{
    public class UpdateWorksurfaceContext
    {
        public UpdateWorksurfaceContext(IContextualResourceModel newDataContext)
        {
            NewDataContext = newDataContext;
        }

        public IContextualResourceModel NewDataContext { get; set; }
    }
}
