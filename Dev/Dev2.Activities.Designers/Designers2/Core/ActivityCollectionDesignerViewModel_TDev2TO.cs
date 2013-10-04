using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Studio.Core.ViewModels.Base;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityCollectionDesignerViewModel<TDev2TOFn> : ActivityCollectionDesignerViewModel
        where TDev2TOFn : class, IDev2TOFn, IPerformsValidation, new()
    {
        ModelItemCollection _modelItemCollection;

        protected ActivityCollectionDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddRowCommand = new RelayCommand(o => AddBlankRow(), o => true);
        }

        public ICommand AddRowCommand { get; private set; }

        public int ItemCount { get { return _modelItemCollection.Count; } }

        protected void InitializeItems(ModelItemCollection modelItemCollection)
        {
            _modelItemCollection = modelItemCollection;
            if(modelItemCollection != null && modelItemCollection.Count <= 0)
            {
                modelItemCollection.Add(DTOFactory.CreateNewDTO(new TDev2TOFn(), 1, true));
                modelItemCollection.Add(DTOFactory.CreateNewDTO(new TDev2TOFn(), 2));
            }

            UpdateDisplayName();
        }

        public override void UpdateDisplayName()
        {
            var currentName = DisplayName;
            if(currentName.Contains("(") && currentName.Contains(")"))
            {
                currentName = currentName.Remove(currentName.Contains(" (")
                    ? currentName.IndexOf(" (", System.StringComparison.Ordinal)
                    : currentName.IndexOf("(", System.StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (ItemCount - 1) + ")";
            DisplayName = currentName;
        }

        public override void Validate()
        {
            var errors = new List<IActionableErrorInfo>();
            foreach(var error in _modelItemCollection.SelectMany(mi => ((TDev2TOFn)mi.GetCurrentValue()).Errors))
            {
                errors.AddRange(error.Value);
            }
            Errors = errors.Count == 0 ? null : errors;
        }

        protected override void AddToCollection(IEnumerable<string> source, bool overwrite)
        {
            var collectionProperty = ModelItem.Properties[CollectionName];
            if(collectionProperty != null)
            {
                var mic = collectionProperty.Collection;
                if(mic != null)
                {
                    if(overwrite)
                    {
                        AddToCollection(mic, source);
                    }
                    else
                    {
                        InsertToCollection(mic, source);
                    }
                }
            }
        }

        void AddToCollection(ModelItemCollection mic, IEnumerable<string> source)
        {
            mic.Clear();

            var startIndex = 0;
            foreach(var s in source)
            {
                mic.Add(DTOFactory.CreateNewDTO(new TDev2TOFn(), startIndex + 1, false, s));
                startIndex++;
            }
            CleanUpCollection(mic, startIndex);
        }

        void InsertToCollection(ModelItemCollection mic, IEnumerable<string> source)
        {
            // gets non empty rows which is valid
            var listOfValidRows = mic.Select(mi => mi.GetCurrentValue() as TDev2TOFn).Where(c => c != null && !c.CanRemove()).ToList();

            // if there are valid (non-empty rows)
            if(listOfValidRows.Count > 0)
            {
                // find last valid row
                var startIndex = listOfValidRows.Last().IndexNumber;

                // and insert before that
                foreach(var s in source)
                {
                    mic.Insert(startIndex, DTOFactory.CreateNewDTO(new TDev2TOFn(), startIndex + 1, false, s));
                    startIndex++;
                }

                // now remove the old one
                CleanUpCollection(mic, startIndex);
            }
            else
            {
                AddToCollection(mic, source);
            }
        }

        void CleanUpCollection(ModelItemCollection mic, int startIndex)
        {
            // remove last valid row
            if(startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }

            // and add a new blank row to the end
            mic.Add(DTOFactory.CreateNewDTO(new TDev2TOFn(), startIndex + 1, true));

            UpdateDisplayName();
        }

        void AddBlankRow()
        {
            var lastRow = _modelItemCollection[_modelItemCollection.Count - 1];
            var lastRowValue = lastRow.GetCurrentValue() as TDev2TOFn;
            if(lastRowValue == null)
            {
                return;
            }

            var isLastRowBlank = lastRowValue.CanRemove();
            if(!isLastRowBlank)
            {
                _modelItemCollection.Add(DTOFactory.CreateNewDTO(new TDev2TOFn(), lastRowValue.IndexNumber + 1));
                UpdateDisplayName();
            }
        }
    }
}