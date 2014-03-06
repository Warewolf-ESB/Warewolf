using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// Arranges into view only the largest single child element that is fully visible, based on the <see cref="Criteria"/> setting. And arranges all other children out of view.
	/// </summary>
	[DesignTimeVisible(false)]
	public class BestFitPanel : Panel
	{
		#region Member Variables

		private UIElement _elementToDisplay = null; 

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="BestFitPanel"/>
		/// </summary>
		public BestFitPanel()
		{
		} 
		#endregion // Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Size sizeUsed = new Size();

			Point ptOutOfView = new Point(-100000, -100000);

			UIElementCollection children = this.Children;

			int count = children.Count;

			// Loop over the children a second time and arrange all but the one child out of view
			for (int i = 0; i < count; i++)
			{
				UIElement child = children[i];

				if ( child == this._elementToDisplay)
				{
					sizeUsed = child.DesiredSize;
					child.Arrange(new Rect(new Point(0,0), sizeUsed));
				}
				else
				{
					// AS 1/10/12 TFS98969
					//child.Arrange(new Rect(ptOutOfView, child.DesiredSize));
					child.Arrange(new Rect(ptOutOfView, new Size()));
				}
			}

			return sizeUsed;
		}

		#endregion //ArrangeOverride

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size szToMeasure = new Size(double.PositiveInfinity, double.PositiveInfinity);

			bool useWidth = this.Criteria == BestFitCriteria.UseWidth;

			UIElementCollection children = this.Children;

			int count = children.Count;

			UIElement largestChildThatCanFit = null;
			UIElement smallestChild = null;

			double extentConstraint = useWidth ? availableSize.Width : availableSize.Height;
			double otherDimensionConstraint = useWidth ? availableSize.Height : availableSize.Width;
			double largestExtentSoFar = 0;
			double smallestExtentSoFar = 0;

			// Loop over the children looking for the largest item that can fit based on the criteria
			for (int i = 0; i < count; i++)
			{
				UIElement child = children[i];

				child.Measure(szToMeasure);

				if ( child.Visibility == System.Windows.Visibility.Collapsed )
					continue;

				Size desiredSize = child.DesiredSize;

				double extentToCheck = useWidth ? desiredSize.Width : desiredSize.Height;
				double otherDimensionToCheck = useWidth ? desiredSize.Height : desiredSize.Width;

				bool isSmallestChaild = false;
				if (i == 0 || extentToCheck < smallestExtentSoFar)
					isSmallestChaild = true;
				else
					if (CoreUtilities.AreClose(extentToCheck, smallestExtentSoFar))
					{
						double previousSmalessOtherDimension = useWidth ? smallestChild.DesiredSize.Height : smallestChild.DesiredSize.Width;

						if (otherDimensionToCheck < previousSmalessOtherDimension)
							isSmallestChaild = true;
					}

				if (isSmallestChaild)
				{
					smallestChild = child;
					smallestExtentSoFar = extentToCheck;
				}

				if (extentToCheck <= extentConstraint || CoreUtilities.AreClose(extentToCheck, extentConstraint))
				{
					bool isLargestChildSoFar = false;

					if (extentToCheck > largestExtentSoFar || largestChildThatCanFit == null)
						isLargestChildSoFar = true;
					else
						if (CoreUtilities.AreClose(extentToCheck, largestExtentSoFar))
						{
							double previousOtherDimension = useWidth ? largestChildThatCanFit.DesiredSize.Height : largestChildThatCanFit.DesiredSize.Width;

							if (otherDimensionToCheck <= otherDimensionConstraint && otherDimensionToCheck > previousOtherDimension)
								isLargestChildSoFar = true;
						}

					if (isLargestChildSoFar)
					{
						largestChildThatCanFit = child;
						largestExtentSoFar = extentToCheck;
					}
				}
			}

			this._elementToDisplay = largestChildThatCanFit;

			// if no child could fit then use the smallest child is ShowPartial was set to true
			if (this._elementToDisplay == null && this.ShowPartial)
				this._elementToDisplay = smallestChild;

			if (this._elementToDisplay != null)
				return this._elementToDisplay.DesiredSize;

			return new Size(1,1);
		}

		#endregion //MeasureOverride

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region Criteria

		/// <summary>
		/// Identifies the <see cref="Criteria"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CriteriaProperty = DependencyPropertyUtilities.Register("Criteria",
			typeof(BestFitCriteria), typeof(BestFitPanel),
			DependencyPropertyUtilities.CreateMetadata(BestFitCriteria.UseWidth, new PropertyChangedCallback(OnCriteriaChanged))
			);

		private static void OnCriteriaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			BestFitPanel instance = (BestFitPanel)d;
			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets whether to use the width or height to determine is a child element fits within this panel.
		/// </summary>
		/// <seealso cref="CriteriaProperty"/>
		public BestFitCriteria Criteria
		{
			get
			{
				return (BestFitCriteria)this.GetValue(BestFitPanel.CriteriaProperty);
			}
			set
			{
				this.SetValue(BestFitPanel.CriteriaProperty, value);
			}
		}

		#endregion //Criteria

		#region ShowPartial

		/// <summary>
		/// Identifies the <see cref="ShowPartial"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowPartialProperty = DependencyPropertyUtilities.Register("ShowPartial",
			typeof(bool), typeof(BestFitPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnShowPartialChanged))
			);

		private static void OnShowPartialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			BestFitPanel instance = (BestFitPanel)d;
			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets whether to show the smallest item partially if none can fit.
		/// </summary>
		/// <seealso cref="ShowPartialProperty"/>
		/// <seealso cref="Criteria"/>
		public bool ShowPartial
		{
			get
			{
				return (bool)this.GetValue(BestFitPanel.ShowPartialProperty);
			}
			set
			{
				this.SetValue(BestFitPanel.ShowPartialProperty, value);
			}
		}

		#endregion //ShowPartial

		#endregion //Public Properties	
	
		#endregion //Properties	
	}

	#region BestFitCriteria enum

	/// <summary>
	/// Determines what criteria to use, width or height, when determining it an element fits within its parent panel.
	/// </summary>
	/// <seealso cref="BestFitPanel"/>
	/// <seealso cref="BestFitPanel.Criteria"/>
	public enum BestFitCriteria
	{
		/// <summary>
		/// Display the widest child element that completely fits in the panel
		/// </summary>
		UseWidth,
		/// <summary>
		/// Display the tallest child element that completely fits in the panel
		/// </summary>
		UseHeight
	}

	#endregion //BestFitCriteria enum

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