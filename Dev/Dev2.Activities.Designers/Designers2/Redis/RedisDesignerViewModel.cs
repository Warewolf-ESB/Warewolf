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
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Interfaces;
using System.Activities;
using System.Windows.Media;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Common.ExtMethods;

namespace Dev2.Activities.Designers2.Redis
{
    public class RedisDesignerViewModel : ActivityDesignerViewModel
    {
        public RedisDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            var dataFunc = modelItem.Properties["ActivityFunc"]?.ComputedValue as ActivityFunc<string, bool>;
            ActivityFuncDisplayName = dataFunc?.Handler == null ? "" : dataFunc?.Handler?.DisplayName;
            var type = dataFunc?.Handler?.GetType();
            if (type != null)
            {
                ActivityFuncIcon = ModelItemUtils.GetImageSourceForToolFromType(type);
            }
        }

      
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        public string ActivityFuncDisplayName
        {
            get { return (string)GetValue(ActivityFuncDisplayNameProperty); }
            set { SetValue(ActivityFuncDisplayNameProperty, value); }
        }

        public static readonly DependencyProperty ActivityFuncDisplayNameProperty =
            DependencyProperty.Register("ActivityFuncDisplayName", typeof(string), typeof(RedisDesignerViewModel), new PropertyMetadata(null));

        public ImageSource ActivityFuncIcon
        {
            get { return (ImageSource)GetValue(ActivityFuncIconProperty); }
            set { SetValue(ActivityFuncIconProperty, value); }
        }

        public static readonly DependencyProperty ActivityFuncIconProperty =
            DependencyProperty.Register("ActivityFuncIcon", typeof(ImageSource), typeof(RedisDesignerViewModel), new PropertyMetadata(null));


        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.Register("Visibility", typeof(Visibility), typeof(RedisDesignerViewModel), new PropertyMetadata(null));

      
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
