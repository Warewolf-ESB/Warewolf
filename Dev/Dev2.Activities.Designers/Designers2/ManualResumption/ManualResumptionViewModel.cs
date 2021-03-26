/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.ManualResumption
{
    public class ManualResumptionViewModel : ActivityDesignerViewModel
    {
        public ManualResumptionViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Flow_ManualResumption;
            var overrideDataFunc = modelItem.Properties["OverrideDataFunc"]?.ComputedValue as ActivityFunc<string, bool>;
            DataFuncDisplayName = overrideDataFunc?.Handler == null ? "" : overrideDataFunc?.Handler?.DisplayName;
            var type = overrideDataFunc?.Handler?.GetType();
            if (type != null)
            {
                DataFuncIcon = ModelItemUtils.GetImageSourceForToolFromType(type);
            }
        }

        public string DataFuncDisplayName
        {
            get => (string)GetValue(DataFuncDisplayNameProperty);
            set => SetValue(DataFuncDisplayNameProperty, value);
        }

        public static readonly DependencyProperty DataFuncDisplayNameProperty =
            DependencyProperty.Register("DataFuncDisplayName", typeof(string), typeof(ManualResumptionViewModel), new PropertyMetadata(null));

        public ImageSource DataFuncIcon
        {
            get => (ImageSource)GetValue(DataFuncIconProperty);
            set => SetValue(DataFuncIconProperty, value);
        }

        public static readonly DependencyProperty DataFuncIconProperty =
            DependencyProperty.Register("DataFuncIcon", typeof(ImageSource), typeof(ManualResumptionViewModel), new PropertyMetadata(null));


        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public static bool MultipleItemsToSequence(IDataObject dataObject)
        {
            if(dataObject != null)
            {
                var formats = dataObject.GetFormats();
                if(!formats.Any())
                {
                    return false;
                }
                var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemsFormat", StringComparison.Ordinal) >= 0);
                if(!string.IsNullOrEmpty(modelItemString))
                {
                    var objectData = dataObject.GetData(modelItemString);

                    if (objectData is List<ModelItem> data && data.Count > 1)
                    {
                        return true;
                    }
                }
                var explorerItemViewModelString = formats.FirstOrDefault(s => s.IndexOf("Warewolf.Studio.ViewModels.ExplorerItemViewModel", StringComparison.Ordinal) >= 0);
                if(!string.IsNullOrEmpty(explorerItemViewModelString))
                {
                    var objectData = dataObject.GetData(explorerItemViewModelString);

                    if (objectData is List<ModelItem> data && data.Count > 1)
                    {
                        return true;
                    }

                    if (objectData is DsfActivity dsfActivity)
                    {
                        
                    }
                }
            }
            return false;
        }
    }
}