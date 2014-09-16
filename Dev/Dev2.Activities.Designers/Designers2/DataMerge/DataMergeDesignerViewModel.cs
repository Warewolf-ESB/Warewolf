using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.DataMerge
{
    public class DataMergeDesignerViewModel : ActivityCollectionDesignerViewModel<DataMergeDTO>
    {
        public Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;
        public IList<string> ItemsList { get; private set; }
        public IList<string> AlignmentTypes { get; private set; }

        public DataMergeDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();

            ItemsList = new List<string> { "None", "Index", "Chars", "New Line", "Tab" };
            AlignmentTypes = new List<string> { "Left", "Right" };
            MergeTypeUpdatedCommand = new DelegateCommand(OnMergeTypeChanged);

            dynamic mi = ModelItem;
            InitializeItems(mi.MergeCollection);

            for(var i = 0; i < mi.MergeCollection.Count; i++)
            {
                OnMergeTypeChanged(i);
            }
        }

        public override string CollectionName { get { return "MergeCollection"; } }

        public ICommand MergeTypeUpdatedCommand { get; private set; }

        void OnMergeTypeChanged(object indexObj)
        {
            var index = (int)indexObj;

            if(index < 0 || index >= ItemCount)
            {
                return;
            }

            var mi = ModelItemCollection[index];
            var mergeType = mi.GetProperty("MergeType") as string;

            if(mergeType == "Index" || mergeType == "Chars")
            {
                mi.SetProperty("EnableAt", true);
                if(mergeType == "Index")
                {
                    mi.SetProperty("EnablePadding", true);
                }
                else
                {
                    mi.SetProperty("EnablePadding", false);
                    mi.SetProperty("Padding", string.Empty);
                }
            }
            else
            {
                mi.SetProperty("At", string.Empty);
                mi.SetProperty("Padding", string.Empty);
                mi.SetProperty("EnableAt", false);
                mi.SetProperty("EnablePadding", false);
            }
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            yield break;
        }

        protected override IEnumerable<IActionableErrorInfo> ValidateCollectionItem(ModelItem mi)
        {
            var dto = mi.GetCurrentValue() as DataMergeDTO;
            if(dto == null)
            {
                yield break;
            }

            foreach(var error in dto.GetRuleSet("At", GetDatalistString()).ValidateRules("'Using'", () => mi.SetProperty("IsAtFocused", true)))
            {
                yield return error;
            }
            foreach(var error in dto.GetRuleSet("Padding", GetDatalistString()).ValidateRules("'Padding'", () => mi.SetProperty("IsPaddingFocused", true)))
            {
                yield return error;
            }
        }

        protected override void OnDispose()
        {
            foreach(var mi in  ModelItemCollection)
            {
                var dto = mi.GetCurrentValue() as DataMergeDTO;
                CEventHelper.RemoveAllEventHandlers(dto);
            }
            base.OnDispose();
        }
    }
}