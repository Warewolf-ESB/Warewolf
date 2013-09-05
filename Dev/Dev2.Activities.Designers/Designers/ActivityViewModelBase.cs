using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Activities.Adorners;
using Dev2.Providers.Errors;
using Dev2.Services.Configuration;

namespace Dev2.Activities.Designers
{
    public abstract class ActivityViewModelBase : Screen, IActivityViewModelBase
    {
        private OverlayType _activeOverlay;
        HelpViewModel _helpViewModel;

        public OverlayType ActiveOverlay
        {
            get { return _activeOverlay; }
            set
            {
                if(_activeOverlay == value)
                {
                    return;
                }

                _activeOverlay = value;
                NotifyOfPropertyChange(() => ActiveOverlay);
            }
        }

        public OverlayType PreviousOverlayType { get; set; }

        public HelpViewModel HelpViewModel
        {
            get
            {
                return _helpViewModel;
            }
            set
            {
                if(_helpViewModel == value)
                {
                    return;
                }

                _helpViewModel = value;
                NotifyOfPropertyChange(() => HelpViewModel);
            }
        }

        public bool IsHelpViewCollapsed
        {
            get
            {
                return UserConfigurationService.Instance.Help.IsCollapsed[ModelItem.ItemType];
            }
            set
            {
                UserConfigurationService.Instance.Help.IsCollapsed[ModelItem.ItemType] = value;
                NotifyOfPropertyChange(() => IsHelpViewCollapsed);
            }
        }

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

        public virtual void HideContent()
        {
            var activityDesigner = ModelItem.View as ActivityDesignerBase;

            if(activityDesigner != null)
            {
                activityDesigner.HideContent();
            }
        }

        public abstract IEnumerable<IErrorInfo> ValidationErrors();
    }
}
