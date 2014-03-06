using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Collections;
using System.Diagnostics;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A Panel derived element used to arrange logically related tools horizontally within a <see cref="RibbonGroup"/>.  
	/// </summary>
	/// <remarks>
	/// <p class="body">The ButtonGroup panel is used to provide tool a single grouping of tools within a <see cref="RibbonGroup"/> 
	/// similar to the groupings displayed within the Font group of the Home tab in Microsoft Word 2007. Items within the panel will have 
	/// attached properties (<see cref="IsFirstInButtonGroupProperty"/>, <see cref="IsInButtonGroupProperty"/> and 
	/// <see cref="IsLastInButtonGroupProperty"/>) set to indicate their position within the panel.</p>
	/// <p class="note"><b>Note: </b>Each of the contained tools' <see cref="RibbonToolSizingMode"/> is set to 'ImageOnly'; the value of the 
	/// <see cref="RibbonGroup.MinimumSizeProperty"/> will not be honored.</p>
	/// </remarks>
	/// <seealso cref="RibbonGroup"/>
	/// <seealso cref="ToolHorizontalWrapPanel"/>
	/// <seealso cref="ToolVerticalWrapPanel"/>
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class ButtonGroup : Panel, IRibbonToolPanel
	{
		#region Member Variables

		private UIElement _firstVisibleTool;
		private UIElement _lastVisibleTool;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ButtonGroup"/>
		/// </summary>
		public ButtonGroup()
		{
		}

		static ButtonGroup()
		{
			RibbonGroup.MinimumSizeProperty.OverrideMetadata(typeof(ButtonGroup), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageOnlyBox));
			RibbonToolHelper.SizingModePropertyKey.OverrideMetadata(typeof(ButtonGroup), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageOnlyBox, null, new CoerceValueCallback(CoerceSizingMode)));
			RibbonGroup.MaximumSizeProperty.OverrideMetadata(typeof(ButtonGroup), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageOnlyBox));
		}
		#endregion //Constructor

		#region Base class overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Rect rect = new Rect(finalSize);

			UIElementCollection elements = this.InternalChildren;

			for (int i = 0, count = elements.Count; i < count; i++)
			{
				UIElement child = elements[i];

				if (child == null || child.Visibility == Visibility.Collapsed)
					continue;

				rect.Width = child.DesiredSize.Width;

				child.Arrange(rect);

				rect.X += rect.Width;
			}

			return finalSize;
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
			#region Setup

			UIElementCollection elements = this.InternalChildren;

			UIElement firstVisibleTool = null;
			UIElement lastVisibleTool = null;

			Size desiredSize = new Size();

			#endregion //Setup

			#region Find First/Last Visible Tool
			// first establish the first and last visible tools since the metric of
			// those tools could be (and in the case of buttons is) reliant upon whether
			// it is the first or last button in a button group
			// measure every one
			for (int i = 0, count = elements.Count; i < count; i++)
			{
				UIElement child = elements[i];

				if (child == null || child.Visibility == Visibility.Collapsed)
					continue;

				firstVisibleTool = child;
				break;
			}

			for (int i = elements.Count - 1; i >= 0; i--)
			{
				UIElement child = elements[i];

				if (child == null || child.Visibility == Visibility.Collapsed)
					continue;

				lastVisibleTool = child;
				break;
			} 
			#endregion //Find First/Last Visible Tool

			#region Mark first and last tools with attached properties

			if (firstVisibleTool != this._firstVisibleTool)
			{
				if (this._firstVisibleTool != null)
					this._firstVisibleTool.ClearValue(IsFirstInButtonGroupPropertyKey);

				this._firstVisibleTool = firstVisibleTool;

				if (this._firstVisibleTool != null)
					this._firstVisibleTool.SetValue(IsFirstInButtonGroupPropertyKey, KnownBoxes.TrueBox);
			}

			if (lastVisibleTool != this._lastVisibleTool)
			{
				if (this._lastVisibleTool != null)
					this._lastVisibleTool.ClearValue(IsLastInButtonGroupPropertyKey);

				this._lastVisibleTool = lastVisibleTool;

				if (this._lastVisibleTool != null)
					this._lastVisibleTool.SetValue(IsLastInButtonGroupPropertyKey, KnownBoxes.TrueBox);
			}

			#endregion //Mark first and last tools with attached properties	

			#region Measure the tools
			
			// measure every one
			for (int i = 0, count = elements.Count; i < count; i++)
			{
				UIElement child = elements[i];

				if (child == null || child.Visibility == Visibility.Collapsed)
					continue;

				RibbonToolSizingMode sizingMode = RibbonToolHelper.GetSizingMode(child);
				bool isLarge = sizingMode == RibbonToolSizingMode.ImageAndTextLarge;

				child.Measure(availableSize);

				Size childSize = child.DesiredSize;

				desiredSize.Width += childSize.Width;
				desiredSize.Height = Math.Max(desiredSize.Height, childSize.Height);
			}
			#endregion //Measure the tools
    
			return desiredSize;
		}
			#endregion //MeasureOverride

			#region OnVisualChildrenChanged

		/// <summary>
		/// Invoked when a child element has been added or removed.
		/// </summary>
		/// <param name="visualAdded">Visual being added</param>
		/// <param name="visualRemoved">Visual being removed</param>
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);

			// clear the attached property settings of a tool that is being removed
			if (visualRemoved != null)
			{
				if (visualRemoved == this._firstVisibleTool)
				{
					visualRemoved.ClearValue(IsFirstInButtonGroupPropertyKey);
					this._firstVisibleTool = null;
				}

				if (visualRemoved == this._lastVisibleTool)
				{
					visualRemoved.ClearValue(IsLastInButtonGroupPropertyKey);
					this._lastVisibleTool = null;
				}

				visualRemoved.ClearValue(IsInButtonGroupPropertyKey);
			}

			// set the IsInButtonGroup attached property on the tool being added
			if (visualAdded != null)
				visualAdded.SetValue(IsInButtonGroupPropertyKey, KnownBoxes.TrueBox);

		}

			#endregion //OnVisualChildrenChanged	
    
		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region IsFirstInButtonGroup

		private static readonly DependencyPropertyKey IsFirstInButtonGroupPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsFirstInButtonGroup",
			typeof(bool), typeof(ButtonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the IsFirstInButtonGroup" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsFirstInButtonGroup"/>
		public static readonly DependencyProperty IsFirstInButtonGroupProperty =
			IsFirstInButtonGroupPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns a boolean indicating whether the object represents the first visible item within the panel.
		/// </summary>
		/// <remarks>
		/// <para class="body">This property is set by the <see cref="ButtonGroup"/> on the first visible tool it contains. It is intended for use within the style of the tool to trigger the display of top/left and bottom/left rounded corners.</para>
		/// </remarks>
		/// <seealso cref="IsFirstInButtonGroupProperty"/>
		/// <seealso cref="GetIsLastInButtonGroup"/>
		/// <seealso cref="GetIsInButtonGroup"/>
		[AttachedPropertyBrowsableForChildren()]
		public static bool GetIsFirstInButtonGroup(DependencyObject d)
		{
			return (bool)d.GetValue(ButtonGroup.IsFirstInButtonGroupProperty);
		}

				#endregion //IsFirstInButtonGroup

				#region IsInButtonGroup

		private static readonly DependencyPropertyKey IsInButtonGroupPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsInButtonGroup",
			typeof(bool), typeof(ButtonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the IsInButtonGroup" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsInButtonGroup"/>
		public static readonly DependencyProperty IsInButtonGroupProperty =
			IsInButtonGroupPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns a boolean indicating whether the object is contained within a <see cref="ButtonGroup"/> panel.
		/// </summary>
		/// <remarks>
		/// <para class="body">This property is set by the <see cref="ButtonGroup"/> on all tools it contains. It is intended for use within the style of the tool to trigger changes in the display of the tool.</para>
		/// </remarks>
		/// <seealso cref="IsInButtonGroupProperty"/>
		/// <seealso cref="GetIsLastInButtonGroup"/>
		/// <seealso cref="GetIsInButtonGroup"/>
		[AttachedPropertyBrowsableForChildren()]
		public static bool GetIsInButtonGroup(DependencyObject d)
		{
			return (bool)d.GetValue(ButtonGroup.IsInButtonGroupProperty);
		}

				#endregion //IsInButtonGroup

				#region IsLastInButtonGroup

		private static readonly DependencyPropertyKey IsLastInButtonGroupPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsLastInButtonGroup",
			typeof(bool), typeof(ButtonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the IsLastInButtonGroup" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsLastInButtonGroup"/>
		public static readonly DependencyProperty IsLastInButtonGroupProperty =
			IsLastInButtonGroupPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns a boolean indicating whether the object represents the last visible item within the panel.
		/// </summary>
		/// <remarks>
		/// <para class="body">This property is set by the <see cref="ButtonGroup"/> on the last visible tool it contains. It is intended for use within the style of the tool to trigger the display of top/right and bottom/right rounded corners.</para>
		/// </remarks>
		/// <seealso cref="IsLastInButtonGroupProperty"/>
		/// <seealso cref="GetIsLastInButtonGroup"/>
		/// <seealso cref="GetIsInButtonGroup"/>
		[AttachedPropertyBrowsableForChildren()]
		public static bool GetIsLastInButtonGroup(DependencyObject d)
		{
			return (bool)d.GetValue(ButtonGroup.IsLastInButtonGroupProperty);
		}

				#endregion //IsLastInButtonGroup

			#endregion //Public Properties	
    
		#endregion //Properties	
    
		#region Methods

			#region CoerceSizingMode

		private static object CoerceSizingMode(DependencyObject target, object value)
		{
			// the sizing mode for ButtonGroups is always ImageOnly
			return RibbonKnownBoxes.RibbonToolSizingModeImageOnlyBox;
		}

			#endregion //CoerceSizingMode	
    
		#endregion //Methods

		#region IRibbonToolPanel

		IList<FrameworkElement> IRibbonToolPanel.GetResizableTools(RibbonToolSizingMode destinationSize)
		{
			// Always return null because 
			return null;
		}

		#endregion //IRibbonToolPanel
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