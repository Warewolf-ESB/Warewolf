using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Dev2.DataList;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.DsfFindRecordsMultipleCriteria
{
    public class DsfFindRecordsMultipleCriteriaViewModel : ActivityCollectionViewModelBase<FindRecordsTO>, IHasActivityViewModelBase
    {
        List<IActionableErrorInfo> _errors;
        string _fieldsToSearch;

        public DsfFindRecordsMultipleCriteriaViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            Errors = new List<IActionableErrorInfo>();
            
        }

        

        protected override string CollectionName
        {
            get
            {
                return "ResultsCollection";
            }
        }

       #region Overrides of ActivityViewModelBase

        public override IEnumerable<IErrorInfo> ValidationErrors()
        {
            var allErrors = new List<IActionableErrorInfo>();
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

        public string FieldsToSearch
        {
            get
            {
                if(string.IsNullOrEmpty(_fieldsToSearch))
                {
                    _fieldsToSearch = ModelItemUtils.GetProperty("FieldsToSearch", ModelItem) as string;
                }
                return _fieldsToSearch;
            }
            set
            {
                if(_fieldsToSearch != value)
                {
                    _fieldsToSearch = value;
                    ModelItemUtils.SetProperty("FieldsToSearch", value, ModelItem);
                    NotifyOfPropertyChange("FieldsToSearch");
                }
                
            }
        }

        public string Result
        {
            get
            {
                return ModelItemUtils.GetProperty("Result", ModelItem) as string;
            }
            set
            {
                ModelItemUtils.SetProperty("Result", value, ModelItem);
                NotifyOfPropertyChange("Result");
            }
        }

        #endregion
    }
}