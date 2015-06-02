
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Type: System.Activities.Core.Presentation.VerticalConnector
// Assembly: System.Activities.Core.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 05072FEA-ECBF-46A4-9B1A-EF2FF1066BCF
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Activities.Core.Presentation\v4.0_4.0.0.0__31bf3856ad364e35\System.Activities.Core.Presentation.dll

using System.Activities.Presentation;
using System.Activities.Presentation.Hosting;
using System.Windows;
using System.Windows.Media.Animation;

// ReSharper disable CheckNamespace
namespace System.Activities.Core.Presentation
{
    public partial class VerticalConnector
    {
        public static readonly DependencyProperty AllowedItemTypeProperty = DependencyProperty.Register("AllowedItemType", typeof(Type), typeof(VerticalConnector), new UIPropertyMetadata(typeof(object)));
        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register("Context", typeof(EditingContext), typeof(VerticalConnector));

        public Type AllowedItemType
        {
            get
            {
                return (Type)GetValue(AllowedItemTypeProperty);
            }
            set
            {
                SetValue(AllowedItemTypeProperty, value);
            }
        }

        public EditingContext Context
        {
            get
            {
                return (EditingContext)GetValue(ContextProperty);
            }
            set
            {
                SetValue(ContextProperty, value);
            }
        }

        static VerticalConnector()
        {
        }

        public VerticalConnector()
        {
            InitializeComponent();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            CheckAnimate(e, "Expand");
            dropTarget.Visibility = Visibility.Visible;
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            CheckAnimate(e, "Collapse");
            dropTarget.Visibility = Visibility.Collapsed;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            dropTarget.Visibility = Visibility.Collapsed;
            base.OnDrop(e);
        }

        private void CheckAnimate(DragEventArgs e, string storyboardResourceName)
        {
            if(e.Handled)
                return;
            if(!Context.Items.GetValue<ReadOnlyState>().IsReadOnly)
            {
                if(DragDropHelper.AllowDrop(e.Data, Context, new[]
        {
          AllowedItemType
        }))
                {
                    BeginStoryboard((Storyboard)Resources[storyboardResourceName]);
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
    }
}
