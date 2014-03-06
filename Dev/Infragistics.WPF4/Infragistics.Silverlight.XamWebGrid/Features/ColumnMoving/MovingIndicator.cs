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

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A control that shows a visual indication of where an object will be moved. 
	/// </summary>
	[TemplatePart(Name = "TopIndicator", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "BottomIndicator", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "FillIndicator", Type = typeof(FrameworkElement))]
	[TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
	public class MovingIndicator : Control
	{
		#region Members

		FrameworkElement _topIndicator, _bottomIndicator, _fillIndicator;

		#endregion // Members

		#region Constructor


        /// <summary>
        /// Static constructor for the <see cref="MovingIndicator"/> class.
        /// </summary>
        static MovingIndicator()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MovingIndicator), new FrameworkPropertyMetadata(typeof(MovingIndicator)));
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="MovingIndicator"/> class.
		/// </summary>
		public MovingIndicator()
		{



		}

		#endregion // Constructor

        #region Properties

        #region Public

        #region HorizontalOffset

        /// <summary>
        /// Identifies the <see cref="HorizontalOffset"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(MovingIndicator), new PropertyMetadata(0.0, new PropertyChangedCallback(HorizontalOffsetChanged)));

        /// <summary>
        /// Gets/Sets the additional offset that the indicator should be positioned by. 
        /// </summary>
        public double HorizontalOffset
        {
            get { return (double)this.GetValue(HorizontalOffsetProperty); }
            set { this.SetValue(HorizontalOffsetProperty, value); }
        }

        private static void HorizontalOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // HorizontalOffset 
				

        #endregion // Public

        #endregion // Properties

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
		/// Builds the visual tree for the <see cref="MovingIndicator"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			this._topIndicator = base.GetTemplateChild("TopIndicator") as FrameworkElement;
			this._bottomIndicator = base.GetTemplateChild("BottomIndicator") as FrameworkElement;
			this._fillIndicator = base.GetTemplateChild("FillIndicator") as FrameworkElement;

			VisualStateManager.GoToState(this, "Normal", true);
		}

		#endregion // OnApplyTemplate

		#region ArrangeOverride
		/// <summary>
		/// Arranges the 3 indicators specified in the control template. 
		/// </summary>
		/// <param propertyName="finalSize">
		/// The final area within the parent that this object 
		/// should use to arrange itself and its children.
		/// </param>
		/// <returns></returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			if (this._topIndicator != null)
			{
				Canvas.SetTop(this._topIndicator, -this._topIndicator.ActualHeight);
				Canvas.SetLeft(this._topIndicator, -this._topIndicator.ActualWidth / 2);
			}

			if (this._bottomIndicator != null)
			{
				Canvas.SetTop(this._bottomIndicator, finalSize.Height);
				Canvas.SetLeft(this._bottomIndicator, -this._bottomIndicator.ActualWidth / 2);
			}

			if (this._fillIndicator != null)
			{





            }

			return base.ArrangeOverride(finalSize);
		}
		#endregion // ArrangeOverride

		#endregion // Overrides

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