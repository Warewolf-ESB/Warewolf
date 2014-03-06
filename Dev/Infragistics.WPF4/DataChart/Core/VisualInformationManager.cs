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

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Helps to augment the visual components of the chart with metadata which can be used to 
    /// programmatically classify them.
    /// </summary>
    public class VisualInformationManager
        : DependencyObject
    {
        /// <summary>
        /// Gets or sets whether the target visual illustrates a negative change in the data.
        /// </summary>
        public static DependencyProperty IsNegativeVisualProperty =
            DependencyProperty.RegisterAttached("IsNegativeVisual",
            typeof(bool), typeof(VisualInformationManager),
            new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether the target visual is an outline of other displayed visuals.
        /// </summary>
        public static DependencyProperty IsOutlineVisualProperty =
            DependencyProperty.RegisterAttached("IsOutlineVisual",
            typeof(bool), typeof(VisualInformationManager),
            new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether the target visual is a solid outline of other displayed visuals.
        /// </summary>
        public static DependencyProperty IsSolidOutlineVisualProperty =
            DependencyProperty.RegisterAttached("IsSolidOutlineVisual",
            typeof(bool), typeof(VisualInformationManager),
            new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether the target visual is the translucent portion of a visual.
        /// </summary>
        public static DependencyProperty IsTranslucentPortionVisualProperty =
            DependencyProperty.RegisterAttached("IsTranslucentPortionVisual",
            typeof(bool), typeof(VisualInformationManager),
            new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether the target visual represents a chart trendline.
        /// </summary>
        public static DependencyProperty IsTrendLineVisualProperty =
           DependencyProperty.RegisterAttached("IsTrendLineVisual",
           typeof(bool), typeof(VisualInformationManager),
           new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether the target visual is part of a segmented multi-color line.
        /// </summary>
        public static DependencyProperty IsMultiColorLineVisualProperty =
            DependencyProperty.RegisterAttached("IsMultiColorLineVisual",
            typeof(bool), typeof(VisualInformationManager),
            new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether the target visual is the main geometry.
        /// </summary>
        public static DependencyProperty IsMainGeometryVisualProperty =
            DependencyProperty.RegisterAttached("IsMainGeometryVisual",
            typeof(bool), typeof(VisualInformationManager),
            new PropertyMetadata(false));

        /// <summary>
        /// Gets the value of the IsNegativeVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to get the value from.</param>
        /// <returns>The value of the property.</returns>
        public static bool GetIsNegativeVisual(DependencyObject target)
        {
            return (bool)target.GetValue(IsNegativeVisualProperty);
        }

        /// <summary>
        /// Sets the value of the IsNegativeVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsNegativeVisual(DependencyObject target, bool value)
        {
            target.SetValue(IsNegativeVisualProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsOutlineVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to get the value from.</param>
        /// <returns>The value of the property.</returns>
        public static bool GetIsOutlineVisual(DependencyObject target)
        {
            return (bool)target.GetValue(IsOutlineVisualProperty);
        }

        /// <summary>
        /// Sets the value of the IsOutlineVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsOutlineVisual(DependencyObject target, bool value)
        {
            target.SetValue(IsOutlineVisualProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsSolidOutlineVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to get the value from.</param>
        /// <returns>The value of the property.</returns>
        public static bool GetIsSolidOutlineVisual(DependencyObject target)
        {
            return (bool)target.GetValue(IsSolidOutlineVisualProperty);
        }

        /// <summary>
        /// Sets the value of the IsSolidOutlineVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsSolidOutlineVisual(DependencyObject target, bool value)
        {
            target.SetValue(IsSolidOutlineVisualProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsTranslucentPortionVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to get the value from.</param>
        /// <returns>The value of the property.</returns>
        public static bool GetIsTranslucentPortionVisual(DependencyObject target)
        {
            return (bool)target.GetValue(IsTranslucentPortionVisualProperty);
        }

        /// <summary>
        /// Sets the value of the IsTranslucentPortionVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsTranslucentPortionVisual(DependencyObject target, bool value)
        {
            target.SetValue(IsTranslucentPortionVisualProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsTrendLineVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to get the value from.</param>
        /// <returns>The value of the property.</returns>
        public static bool GetIsTrendLineVisual(DependencyObject target)
        {
            return (bool)target.GetValue(IsTrendLineVisualProperty);
        }

        /// <summary>
        /// Sets the value of the IsTrendLineVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsTrendLineVisual(DependencyObject target, bool value)
        {
            target.SetValue(IsTrendLineVisualProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsMultiColorLineVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to get the value from.</param>
        /// <returns>The value of the property.</returns>
        public static bool GetIsMultiColorLineVisual(DependencyObject target)
        {
            return (bool)target.GetValue(IsMultiColorLineVisualProperty);
        }

        /// <summary>
        /// Sets the value of the IsMultiColorLineVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsMultiColorLineVisual(DependencyObject target, bool value)
        {
            target.SetValue(IsMultiColorLineVisualProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsMainGeometryVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to get the value from.</param>
        /// <returns>The value of the property.</returns>
        public static bool GetIsMainGeometryVisual(DependencyObject target)
        {
            return (bool)target.GetValue(IsMainGeometryVisualProperty);
        }

        /// <summary>
        /// Sets the value of the IsMainGeometryVisual attached property for a given dependency object
        /// </summary>
        /// <param name="target">The dependency object to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsMainGeometryVisual(DependencyObject target, bool value)
        {
            target.SetValue(IsMainGeometryVisualProperty, value);
        }
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