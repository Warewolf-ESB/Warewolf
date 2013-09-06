using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using Dev2.Providers.Errors;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.DsfMultiAssign
{
    public class DsfMultiAssignActivityViewModel : ActivityCollectionViewModelBase<ActivityDTO>, IHasActivityViewModelBase
    {
        List<IActionableErrorInfo> _errors;

        public DsfMultiAssignActivityViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            Errors = new List<IActionableErrorInfo>();
        }

        protected override string CollectionName
        {
            get
            {
                return "FieldsCollection";
            }
        }

        #region Overrides of ActivityViewModelBase

        public override IEnumerable<IErrorInfo> ValidationErrors()
        {
            var allErrors = new List<IActionableErrorInfo>();
            foreach(var error in Items.SelectMany(activityDto => activityDto.Errors))
            {
                allErrors.AddRange(error.Value);
            }
            Errors = allErrors;
            return allErrors;
        }

        public List<IActionableErrorInfo> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                NotifyOfPropertyChange(() => Errors);
            }
        }

        #endregion

        #region Implementation of IHasActivityViewModelBase

        public IActivityViewModelBase ActivityViewModelBase
        {
            get
            {
                return this;
            }
        }

        #endregion
    }
}
