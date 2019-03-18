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


// Type: System.Activities.Core.Presentation.VerticalConnector
// Assembly: System.Activities.Core.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 05072FEA-ECBF-46A4-9B1A-EF2FF1066BCF
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Activities.Core.Presentation\v4.0_4.0.0.0__31bf3856ad364e35\System.Activities.Core.Presentation.dll

using System.Activities.Presentation;
using System.Activities.Presentation.Hosting;
using System.Windows;
using System.Windows.Media.Animation;


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
            DropTarget.Visibility = Visibility.Visible;
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            CheckAnimate(e, "Collapse");
            DropTarget.Visibility = Visibility.Collapsed;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            DropTarget.Visibility = Visibility.Collapsed;
            base.OnDrop(e);
        }

        void CheckAnimate(DragEventArgs e, string storyboardResourceName)
        {
            if (e.Handled)
            {
                return;
            }

            if (Context != null && !Context.Items.GetValue<ReadOnlyState>().IsReadOnly && DragDropHelper.AllowDrop(e.Data, Context, AllowedItemType))
            {
                BeginStoryboard((Storyboard)Resources[storyboardResourceName]);
                return;
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
    }
}
