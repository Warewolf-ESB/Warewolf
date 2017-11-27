/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Interfaces;
using System.Windows;
using System.Activities;
using System.Windows.Media;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Activities.Designers2.SelectAndApply
{
    public class SelectAndApplyDesignerViewModel : ActivityDesignerViewModel
    {
        public SelectAndApplyDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_LoopConstruct_Select_and_Apply;
            var dataFunc = modelItem.Properties["ApplyActivityFunc"]?.ComputedValue as ActivityFunc<string, bool>;
            DataFuncDisplayName = dataFunc?.Handler == null ? "" : dataFunc?.Handler?.DisplayName;
            var type = dataFunc?.Handler?.GetType();
            if (type != null)
            {
                DataFuncIcon = ModelItemUtils.GetImageSourceForToolFromType(type);
            }
        }

        public string DataFuncDisplayName
        {
            get { return (string)GetValue(DataFuncDisplayNameProperty); }
            set { SetValue(DataFuncDisplayNameProperty, value); }
        }

        public static readonly DependencyProperty DataFuncDisplayNameProperty =
            DependencyProperty.Register("DataFuncDisplayName", typeof(string), typeof(SelectAndApplyDesignerViewModel), new PropertyMetadata(null));

        public ImageSource DataFuncIcon
        {
            get { return (ImageSource)GetValue(DataFuncIconProperty); }
            set { SetValue(DataFuncIconProperty, value); }
        }

        public static readonly DependencyProperty DataFuncIconProperty =
            DependencyProperty.Register("DataFuncIcon", typeof(ImageSource), typeof(SelectAndApplyDesignerViewModel), new PropertyMetadata(null));

        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
