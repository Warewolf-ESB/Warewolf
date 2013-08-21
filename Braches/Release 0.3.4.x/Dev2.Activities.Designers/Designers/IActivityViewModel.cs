using System.Activities.Presentation.Model;

namespace Dev2.Activities.Designers
{
    // Implementations MUST provide a constructor with a ModelItem parameter!
    public interface IActivityViewModel
    {
        ModelItem ModelItem { get; }
    }
}