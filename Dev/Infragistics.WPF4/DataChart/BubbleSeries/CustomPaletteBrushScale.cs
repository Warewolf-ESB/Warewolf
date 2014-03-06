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
    /// Represents a brush scale that uses index-based brush selection mode.
    /// </summary>
    public class CustomPaletteBrushScale:BrushScale
    {
        /// <summary>
        /// Creates a new instance of the CustomPaletteBrushScale.
        /// </summary>
        public CustomPaletteBrushScale()
        {
           
        }

        #region BrushSelectionMode Dependency Property
        internal const string BrushSelectionModePropertyName = "BrushSelectionMode";

        /// <summary>
        /// Identifies the BrushSelectionMode dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushSelectionModeProperty =
            DependencyProperty.Register(BrushSelectionModePropertyName, typeof(BrushSelectionMode), typeof(CustomPaletteBrushScale),
            new PropertyMetadata(BrushSelectionMode.Select, (o, e) =>
            {
                (o as CustomPaletteBrushScale).RaisePropertyChanged(BrushSelectionModePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the brush selection mode.
        /// </summary>
        public BrushSelectionMode BrushSelectionMode
        {
            get 
            { 
                return (BrushSelectionMode)GetValue(BrushSelectionModeProperty); 
            }
            set 
            {
                SetValue(BrushSelectionModeProperty, value);
            }
        }
        #endregion

        /// <summary>
        /// Returns calculated brush for the given index.
        /// </summary>
        /// <param name="index">Point index.</param>
        /// <param name="total">Total number of markers in the series.</param>
        /// <returns>Brush object at a specified index.</returns>
        public virtual Brush GetBrush(int index, int total)
        {
            if (Brushes == null || Brushes.Count == 0) return null;

            if (BrushSelectionMode == BrushSelectionMode.Select)
            {




                return GetBrush(index % Brushes.Count);

            }

            double scaledIndex = BubbleSeries.GetLinearSize(0, total - 1, 0, Brushes.Count - 1, index);
            
            return GetInterpolatedBrush(scaledIndex);
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