using Dev2.Activities.Adorners;
using Dev2.Providers.Validation;

namespace Dev2.Activities.Designers
{
    public interface IActivityViewModelBase : IActivityViewModel, IValidator, IOverlayManager
    {
    }
}