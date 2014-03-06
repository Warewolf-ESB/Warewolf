using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// Control that is used to separate menu items in <see cref="XamMenu"/> control. 
    /// </summary>
    [TemplateVisualState(GroupName = XamMenuSeparator.OrientationStateGroupName, Name = XamMenuSeparator.OrientationVertical)]
    [TemplateVisualState(GroupName = XamMenuSeparator.OrientationStateGroupName, Name = XamMenuSeparator.OrientationHorizontal)]
    public class XamMenuSeparator : Control
    {
        private const string OrientationStateGroupName = "OrientationState";
        private const string OrientationVertical = "Vertical";
        private const string OrientationHorizontal = "Horizontal";

        /// <summary>
        /// Initializes a new instance of the <see cref="XamMenuSeparator"/> class.
        /// </summary>
        public XamMenuSeparator()
        {

			Infragistics.Windows.Utilities.ValidateLicense(typeof(XamMenuSeparator), this);

            DefaultStyleKey = typeof(XamMenuSeparator);
            this.IsTabStop = false;
        }

        /// <summary>
        /// Builds the visual tree for the XamMenuSeparator control when a new control
        /// template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (SeparatorOrientation == Orientation.Horizontal)
                VisualStateManager.GoToState(this, OrientationHorizontal, false);
            else
                VisualStateManager.GoToState(this, OrientationVertical, false);

        }
        #region SeparatorOrientation

        /// <summary>
        /// Identifies the <see cref="SeparatorOrientation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SeparatorOrientationProperty =
            DependencyProperty.Register("SeparatorOrientation", typeof(Orientation), typeof(XamMenuSeparator),
            new PropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(SeparatorOrientationChanged)));

        /// <summary>
        /// Determinates how separator is orientated. It can be positioned either horizontally or vertically.
        /// </summary>
        public Orientation SeparatorOrientation
        {
            get { return (Orientation)this.GetValue(SeparatorOrientationProperty); }
            set { this.SetValue(SeparatorOrientationProperty, value); }
        }

        private static void SeparatorOrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuSeparator item = obj as XamMenuSeparator;
            Orientation val = (Orientation)e.NewValue;
            if (val == Orientation.Horizontal)
                VisualStateManager.GoToState(item, OrientationHorizontal, false);
            else
                VisualStateManager.GoToState(item, OrientationVertical, false);
        }

        #endregion // SeparatorOrientation 
          
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved