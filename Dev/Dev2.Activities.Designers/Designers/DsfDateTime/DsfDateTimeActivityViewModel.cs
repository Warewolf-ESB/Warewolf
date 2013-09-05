using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Dev2.Common;
using Dev2.Converters.DateAndTime;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Activities.Designers.DsfDateTime
{
    public class DsfDateTimeActivityViewModel : ActivityViewModelBase
    {
        #region private fields

        private string _selectedTimeModifierType;
        private ObservableCollection<string> _timeModifierTypes;

        #endregion private fields

        #region ctor

        public DsfDateTimeActivityViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            DateTimeFormatter.TimeModifierTypes.ToList().ForEach(TimeModifierTypes.Add);
        }

        #endregion ctor

        #region public properties

        public string SelectedTimeModifierType
        {
            get
            {
                if(String.IsNullOrEmpty(_selectedTimeModifierType))
                {
                    string currentValue = ModelItemUtils.GetProperty("TimeModifierType", ModelItem) as string;
                    if(!string.IsNullOrEmpty(currentValue))
                    {
                        _selectedTimeModifierType = currentValue;
                    }
                }
                return _selectedTimeModifierType;
            }
            set
            {
                if(_selectedTimeModifierType == value)
                {
                    return;
                }

                SetTimeModifierType(value);
                NotifyOfPropertyChange(() => SelectedTimeModifierType);
            }
        }

        public ObservableCollection<string> TimeModifierTypes
        {
            get
            {
                return _timeModifierTypes ?? (_timeModifierTypes = new ObservableCollection<string>());
            }
        }

        public string Dev2DefaultDateTime
        {
            get
            {
                return GlobalConstants.Dev2CustomDefaultDateTimeFormat;
            }
            set
            {
                throw new ReadOnlyException();
            }
        }

        #endregion public properties

        #region private methods

        private void SetTimeModifierType(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                ModelItemUtils.SetProperty("TimeModifierAmountDisplay", value, ModelItem);
            }

            ModelItemUtils.SetProperty("TimeModifierType", value, ModelItem);
            _selectedTimeModifierType = value;
        }

        #endregion private methods

        #region Overrides of ActivityViewModelBase

        public override IEnumerable<IErrorInfo> ValidationErrors()
        {
            yield break;
        }

        #endregion
    }
}
