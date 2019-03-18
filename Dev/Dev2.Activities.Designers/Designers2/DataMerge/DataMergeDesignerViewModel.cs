#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.DataMerge
{
    public class DataMergeDesignerViewModel : ActivityCollectionDesignerViewModel<DataMergeDTO>
    {
        internal Func<string> _getDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;
        public IList<string> ItemsList { get; private set; }
        public IList<string> AlignmentTypes { get; private set; }
        
        public DataMergeDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {            
            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();

            ItemsList = new List<string> { "None", "Index", "Chars", "New Line", "Tab" };
            AlignmentTypes = new List<string> { "Left", "Right" };
            MergeTypeUpdatedCommand = new DelegateCommand(OnMergeTypeChanged);

            dynamic mi = ModelItem;
            InitializeItems(mi.MergeCollection);
            for (var i = 0; i < mi.MergeCollection.Count; i++)
            {
                OnMergeTypeChanged(i);
            }
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Data_Data_Merge;
        }
        public override string CollectionName => "MergeCollection";

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

            foreach (var error in dto.GetRuleSet("Input", _getDatalistString?.Invoke()).ValidateRules("'Input'", () => mi.SetProperty("IsFieldNameFocused", true)))
            {
                yield return error;
            }

            foreach(var error in dto.GetRuleSet("At", _getDatalistString?.Invoke()).ValidateRules("'Using'", () => mi.SetProperty("IsAtFocused", true)))
            {
                yield return error;
            }
            foreach(var error in dto.GetRuleSet("Padding", _getDatalistString?.Invoke()).ValidateRules("'Padding'", () => mi.SetProperty("IsPaddingFocused", true)))
            {
                yield return error;
            }
        }

        protected override void RunValidation(int index)
        {
            if (index == -1)
            {
                return;
            }

            OnMergeTypeChanged(index);
        }

        protected override void OnDispose()
        {
            if(ModelItemCollection != null)
            {
                foreach(var mi in  ModelItemCollection)
                {
                    var dto = mi.GetCurrentValue() as DataMergeDTO;
                    CEventHelper.RemoveAllEventHandlers(dto);
                }
            }
            base.OnDispose();
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
