using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.Core
{
    // -------------------------------------------------------------------------------------------------------------
    // NOTES
    // -------------------------------------------------------------------------------------------------------------
    // - All indexNumber parameters are 1-based
    // - MUST ALWAYS modify DTO properties via ModelItem properties - otherwise you WILL experience binding issues.
    // -------------------------------------------------------------------------------------------------------------

    public abstract class ActivityCollectionDesignerViewModel<TDev2TOFn> : ActivityCollectionDesignerViewModel
        where TDev2TOFn : class, IDev2TOFn, IPerformsValidation, new()
    {
        TDev2TOFn _initialDto = new TDev2TOFn();

        protected ActivityCollectionDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public int ItemCount { get { return ModelItemCollection.Count; } }

        protected void InitializeItems(ModelItemCollection modelItemCollection)
        {
            ModelItemCollection = modelItemCollection;

            // Do this before, because AddDTO() also attaches events
            AttachEvents(0);

            switch(modelItemCollection.Count)
            {
                case 0:
                    AddDto(1);
                    AddDto(2);
                    break;
                case 1:
                    AddDto(2);
                    break;
            }

            AddBlankRow();
            UpdateDisplayName();
        }

        public override void OnSelectionChanged(ModelItem oldItem, ModelItem newItem)
        {
            if(oldItem != null)
            {
                var dto = oldItem.GetCurrentValue() as TDev2TOFn;
                if(dto != null && dto.CanRemove())
                {
                    // old row is blank so remove
                    var index = ModelItemCollection.IndexOf(oldItem) + 1;
                    RemoveDto(dto, index);
                }
            }
        }

        public override void UpdateDisplayName()
        {
            var currentName = DisplayName;
            if(currentName.Contains("(") && currentName.Contains(")"))
            {
                currentName = currentName.Remove(currentName.Contains(" (")
                    ? currentName.IndexOf(" (", StringComparison.Ordinal)
                    : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (ItemCount - 1) + ")";
            DisplayName = currentName;
        }

        public sealed override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            result.AddRange(ValidateThis());

            var errorCount = result.Count;

            ProcessModelItemCollection(0, mi => result.AddRange(ValidateCollectionItemDataList(mi)));

            if(result.Count == errorCount)
            {
                ProcessModelItemCollection(0, mi => result.AddRange(ValidateCollectionItem(mi)));
            }

            Errors = result.Count == 0 ? null : result;
        }

        protected abstract IEnumerable<IActionableErrorInfo> ValidateThis();

        protected abstract IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi);

        private IEnumerable<IActionableErrorInfo> ValidateCollectionItemDataList(ModelItem mi)
        {
            if(DataListSingleton.ActiveDataList == null)
            {
                yield break;
            }

            DataListSingleton.ActiveDataList.RemoveUnusedDataListItems();

            if(string.IsNullOrEmpty(DataListSingleton.ActiveDataList.DataListErrorMessage))
            {
                yield break;
            }

            var dto = mi.GetCurrentValue() as ActivityDTO;
            if(dto == null)
            {
                yield break;
            }
            
            var fieldInError = DataListSingleton.ActiveDataList.DataList.FirstOrDefault(d => !string.IsNullOrEmpty(d.ErrorMessage));

            if(fieldInError != null)
            {
                if(dto.FieldName.Contains(fieldInError.DisplayName))
                {
                    yield return new ActionableErrorInfo(() => mi.SetProperty("IsFieldNameFocused", true))
                    {
                        ErrorType = ErrorType.Critical,
                        Message = fieldInError.ErrorMessage
                    };

                    yield break;
                }

                if(dto.FieldValue.Contains(fieldInError.DisplayName))
                {
                    yield return new ActionableErrorInfo(() => mi.SetProperty("IsFieldValueFocused", true))
                    {
                        ErrorType = ErrorType.Critical,
                        Message = fieldInError.ErrorMessage
                    };
                }
            }
        }

        public override bool CanRemoveAt(int indexNumber)
        {
            return ModelItemCollection.Count > 2 && indexNumber < ModelItemCollection.Count;
        }

        public override bool CanInsertAt(int indexNumber)
        {
            return ModelItemCollection.Count > 2 && indexNumber < ModelItemCollection.Count;
        }

        public override void RemoveAt(int indexNumber)
        {
            if(!CanRemoveAt(indexNumber))
            {
                return;
            }

            if(ModelItemCollection.Count == 2)
            {
                if(indexNumber == 1)
                {
                    var dto = GetDto(indexNumber);
                    dto.ClearRow();
                }
            }
            else
            {
                var dto = GetDto(indexNumber);
                RemoveDto(dto, indexNumber);
            }
        }

        public override void InsertAt(int indexNumber)
        {
            if(!CanInsertAt(indexNumber))
            {
                return;
            }
            AddDto(indexNumber);
            UpdateDisplayName();
        }

        protected override void AddToCollection(IEnumerable<string> source, bool overwrite)
        {
            var firstModelItem = ModelItemCollection.FirstOrDefault();
            if(firstModelItem != null)
            {
                _initialDto = (TDev2TOFn)firstModelItem.GetCurrentValue();
            }

            var indexNumber = GetIndexForAdd(overwrite);

            // Always insert items before blank row
            foreach(var s in source.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                AddDto(indexNumber, s);
                indexNumber++;
            }

            var lastModelItem = GetModelItem(ItemCount);
            SetIndexNumber(lastModelItem, indexNumber);

            UpdateDisplayName();

            // Restore 
            _initialDto = new TDev2TOFn();
        }

        /// <summary>
        /// Gets the insert index for <see cref="AddToCollection"/>. 
        /// Returns the index of the last blank row.
        /// </summary>
        int GetIndexForAdd(bool overwrite)
        {
            var indexNumber = 1;
            if(overwrite)
            {
                ModelItemCollection.Clear();

                // Add blank row
                AddDto(indexNumber);
            }
            else
            {
                var lastDto = GetLastDto();
                indexNumber = ModelItemCollection.IndexOf(GetModelItem(ItemCount)) + 1;

                if(ModelItemCollection.Count == 2)
                {
                    // Check whether we have 2 blank rows
                    var firstDto = GetDto(1);
                    if(firstDto.CanRemove() && lastDto.CanRemove())
                    {
                        RemoveAt(indexNumber, lastDto);
                        indexNumber = indexNumber - 1;
                    }
                }
            }
            return indexNumber;
        }

        ModelItem GetModelItem(int indexNumber)
        {
            return ModelItemCollection[indexNumber - 1];
        }

        TDev2TOFn GetDto(int indexNumber)
        {
            var item = GetModelItem(indexNumber);
            return item.GetCurrentValue() as TDev2TOFn;
        }

        TDev2TOFn GetLastDto()
        {
            return GetDto(ItemCount);
        }

        void AddBlankRow()
        {
            var lastDto = GetLastDto();
            var index = ItemCount + 1;
            var isLastRowBlank = lastDto.CanRemove();
            if(!isLastRowBlank)
            {
                AddDto(index + 1);
                UpdateDisplayName();
            }
        }

        void AddDto(int indexNumber, string initializeWith = "")
        {
            //
            // DO NOT invoke Renumber() from here - this method is called MANY times when invoking AddToCollection()!!
            //
            var dto = CreateDto(indexNumber, initializeWith);
            AttachEvents(dto);

            var idx = indexNumber - 1;
            if(idx >= ModelItemCollection.Count)
            {
                ModelItem modelItem = ModelItemUtils.CreateModelItem(dto);
                ModelItemCollection.Add(modelItem);
            }
            else
            {
                ModelItemCollection.Insert(idx, dto);
            }
        }

        protected virtual IDev2TOFn CreateDto(int indexNumber, string initializeWith)
        {
            return DTOFactory.CreateNewDTO(_initialDto, indexNumber, false, initializeWith);
        }

        void RemoveDto(TDev2TOFn dto, int indexNumber)
        {

            if(ModelItemCollection.Count > 2 && indexNumber < ModelItemCollection.Count)
            {
                RemoveAt(indexNumber, dto);
                UpdateDisplayName();
            }
        }

        void RemoveAt(int indexNumber, INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnDtoPropertyChanged;
            var idx = indexNumber - 1;
            ModelItemCollection.RemoveAt(idx);
        }

        void OnDtoPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName != "CanRemove")
            {
                return;
            }

            var dto = (TDev2TOFn)sender;
            if(dto.CanAdd())
            {
                if(ModelItemCollection.Count == 2)
                {
                    var firstDto = GetDto(1);
                    if(!firstDto.CanRemove())
                    {
                        // first row is not blank
                        AddBlankRow();
                    }
                }
                else
                {
                    AddBlankRow();
                }
            }
        }

        /// <summary>
        /// Attaches events to the ModelItemCollection starting at the specified zero-based index.
        /// </summary>
        void AttachEvents(int startIndex)
        {
            ProcessModelItemCollection(startIndex, mi =>
            {
                var dto = mi.GetCurrentValue() as TDev2TOFn;
                if(dto != null)
                {
                    AttachEvents(dto);
                }
            });
        }

        void AttachEvents(INotifyPropertyChanged dto)
        {
            dto.PropertyChanged += OnDtoPropertyChanged;
        }


        /// <summary>
        /// Process the ModelItemCollection starting at the specified zero-based index.
        /// </summary>
        void ProcessModelItemCollection(int startIndex, Action<ModelItem> processModelItem)
        {
            startIndex = Math.Max(startIndex, 0);
            for(var i = startIndex; i < ModelItemCollection.Count; i++)
            {
                processModelItem(ModelItemCollection[i]);
            }
        }

        static void SetIndexNumber(ModelItem mi, int indexNumber)
        {
            mi.SetProperty("IndexNumber", indexNumber);
        }
    }
}