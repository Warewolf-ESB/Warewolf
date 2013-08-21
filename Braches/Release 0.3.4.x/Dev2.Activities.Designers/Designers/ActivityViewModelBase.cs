using System.Activities.Presentation.Model;
using Caliburn.Micro;
using Dev2.Activities.Adorners;

namespace Dev2.Activities.Designers
{
    public abstract class ActivityViewModelBase : Screen, IActivityViewModel
    {
        private OverlayType _activeOverlay;

        public OverlayType ActiveOverlay
        {
            get { return _activeOverlay; }
            set
            {
                if (_activeOverlay == value)
                {
                    return;
                }

                _activeOverlay = value;
                NotifyOfPropertyChange(() => ActiveOverlay);
            }
        }

        public OverlayType PreviousOverlayType { get; set; }

        protected ActivityViewModelBase(ModelItem modelItem)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);
            ModelItem = modelItem;
        }

        public virtual void OnModelItemChanged(ModelItem newItem)
        {
            ModelItem = newItem;
        }

        public ModelItem ModelItem { get; private set; }
    }
}
