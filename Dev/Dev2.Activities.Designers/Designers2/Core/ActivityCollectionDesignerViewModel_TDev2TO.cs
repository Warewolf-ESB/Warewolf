using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Studio.Core.Activities.Utils;
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
                    AddDTO(1);
                    AddDTO(2);
                    break;
                case 1:
                    AddDTO(2);
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
                    RemoveDTO(dto);
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

        public override void Validate()
        {
            var errors = new List<IActionableErrorInfo>();
            foreach(var error in ModelItemCollection.SelectMany(mi => ((TDev2TOFn)mi.GetCurrentValue()).Errors ?? new Dictionary<string, List<IActionableErrorInfo>>()))
            {
                errors.AddRange(error.Value);
            }
            Errors = errors.Count == 0 ? null : errors;
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
                    var dto = GetDTO(indexNumber);
                    dto.ClearRow();
                }
            }
            else
            {
                var dto = GetDTO(indexNumber);
                RemoveDTO(dto);
            }
        }

        public override void InsertAt(int indexNumber)
        {
            if(!CanInsertAt(indexNumber))
            {
                return;
            }
            AddDTO(indexNumber);
            Renumber(indexNumber);
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
                AddDTO(indexNumber, s);
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
                AddDTO(indexNumber);
            }
            else
            {
                var lastDTO = GetLastDTO();
                indexNumber = lastDTO.IndexNumber;

                if(ModelItemCollection.Count == 2)
                {
                    // Check whether we have 2 blank rows
                    var firstDTO = GetDTO(1);
                    if(firstDTO.CanRemove() && lastDTO.CanRemove())
                    {
                        RemoveAt(lastDTO.IndexNumber, lastDTO);
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

        TDev2TOFn GetDTO(int indexNumber)
        {
            var item = GetModelItem(indexNumber);
            return item.GetCurrentValue() as TDev2TOFn;
        }

        TDev2TOFn GetLastDTO()
        {
            return GetDTO(ItemCount);
        }

        void AddBlankRow()
        {
            var lastDTO = GetLastDTO();
            var isLastRowBlank = lastDTO.CanRemove();
            if(!isLastRowBlank)
            {
                AddDTO(lastDTO.IndexNumber+1);
                UpdateDisplayName();
            }
        }

        void AddDTO(int indexNumber, string initializeWith = "")
        {
            //
            // DO NOT invoke Renumber() from here - this method is called MANY times when invoking AddToCollection()!!
            //
            var dto = CreateDTO(indexNumber, initializeWith);
            AttachEvents(dto);

            var idx = indexNumber-1;
            if(idx >= ModelItemCollection.Count)
            {
                ModelItemCollection.Add(dto);
            }
            else
            {
                ModelItemCollection.Insert(idx, dto);
            }
        }

        protected virtual IDev2TOFn CreateDTO(int indexNumber, string initializeWith)
        {  
            return DTOFactory.CreateNewDTO(_initialDto, indexNumber, false, initializeWith);
        }

        void RemoveDTO(TDev2TOFn dto)
        {
            if(ModelItemCollection.Count > 2 && dto.IndexNumber >= 0 && dto.IndexNumber < ModelItemCollection.Count)
            {
                RemoveAt(dto.IndexNumber, dto);
                Renumber(dto.IndexNumber - 1);
                UpdateDisplayName();
            }
        }

        void RemoveAt(int indexNumber, INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnDTOPropertyChanged;
            var idx = indexNumber - 1;
            ModelItemCollection.RemoveAt(idx);
        }

        void OnDTOPropertyChanged(object sender, PropertyChangedEventArgs args)
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
                    var firstDTO = GetDTO(1);
                    if(!firstDTO.CanRemove())
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
            dto.PropertyChanged += OnDTOPropertyChanged;
        }

        /// <summary>
        /// Renumbers the ModelItemCollection starting at the specified zero-based index.
        /// </summary>
        void Renumber(int startIndex)
        {
            var indexNumber = startIndex + 1;
            ProcessModelItemCollection(startIndex, mi => SetIndexNumber(mi, indexNumber++));
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