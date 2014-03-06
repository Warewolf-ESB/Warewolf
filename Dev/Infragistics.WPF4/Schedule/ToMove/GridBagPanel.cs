using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.Controls.Layouts.Primitives;
using System.Windows.Data;

namespace Infragistics.Controls.Layouts
{
	/// <summary>
	/// A panel that arranges its children based on their Column, Row, ColumnSpan and RowSpan settings.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>GridBagPanel</b> by default arranges children by laying them out horizontally. You can control
	/// the positioning of each child by setting Column, Row, ColumnSpan and RowSpan attached properties.
	/// Column and Row values of -1 indicate the child element is to be positioned relative to the previous
	/// child element. ColumnSpan or RowSpan of 0 indicates the child element is to occupy the rest of the 
	/// horizontal or vertical area and further items with Column or Row values of -1 (relative) will be 
	/// positioned on the next logical row or column.
	/// </para>
	/// </remarks>
	[DesignTimeVisible(false)]
	public class GridBagPanel : Panel
	{
		#region Nested Data Structures

		#region LayoutContainer Class

		private class LayoutContainer : ILayoutContainer
		{
			#region Member Vars

			private GridBagPanel _gbPanel;
			private Rect _rect;
			private bool _isMeasure;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="gbPanel">The panel whose children will be measured/arranged</param>
			/// <param name="rect">The bounding rect for the measure/arrange</param>
			/// <param name="isMeasure">True if the container is to measure the elements only and false if the elements are to be arranged</param>
			internal LayoutContainer( GridBagPanel gbPanel, Rect rect, bool isMeasure )
			{
				_gbPanel = gbPanel;
				_rect = rect;
				_isMeasure = isMeasure;
			}

			#endregion // Constructor

			#region GetBounds

			/// <summary>
			/// Returns the rect in which elements are to be laid out.
			/// </summary>
			/// <param name="containerContext"></param>
			/// <returns></returns>
			public Rect GetBounds( object containerContext )
			{
				return _rect;
			}

			#endregion // GetBounds

			#region PositionItem

			/// <summary>
			/// Positions the element associated with the specified layout item.
			/// </summary>
			/// <param name="i">Layout item.</param>
			/// <param name="rect">Arrange rect of the item.</param>
			/// <param name="containerContext">Container context.</param>
			public void PositionItem( ILayoutItem i, Rect rect, object containerContext )
			{
				LayoutItem li = i as LayoutItem;

				if (_isMeasure)
					li.MeasureElem( new Size(rect.Width, rect.Height), false, true  );
				else
					li.ArrangeElem( rect, _gbPanel );
			}

			#endregion // PositionItem
		}

		#endregion // LayoutContainer Class

		#region LayoutItem Class

		private class LayoutItem : ILayoutItem
			, IGridBagConstraint
		{
			#region Member Vars

			private static Thickness g_DefaultMargin = new Thickness();

			internal UIElement _elem;
			// AS 5/9/12 TFS103196
			//internal Size _lastElemMeasureSize;
			internal Size _lastElemMeasureSize = Size.Empty;
			private GridBagPanel _panel;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="elem">Associated element.</param>
			/// <param name="panel">The containing panel</param>
			internal LayoutItem( UIElement elem, GridBagPanel panel )
			{
				_elem = elem;
				_panel = panel;
			}

			#endregion // Constructor

			#region Methods

			#region ArrangeElem

			internal void ArrangeElem( Rect rect, GridBagPanel gbPanel )
			{
				
#region Infragistics Source Cleanup (Region)







































#endregion // Infragistics Source Cleanup (Region)


				_elem.Arrange( rect );
			}

			#endregion // ArrangeElem

			#region GetMarginExtent

			private double GetMarginExtent( bool horiz )
			{
				FrameworkElement fe = _elem as FrameworkElement;
				if ( null != fe )
				{
					Thickness margin = fe.Margin;
					return horiz ? margin.Left + margin.Right : margin.Top + margin.Bottom;
				}

				return 0.0;
			}

			#endregion // GetMarginExtent

			#region MeasureElem

			internal bool MeasureElem( Size measureSize, bool usePreferredSettings, bool isPositioningItem = false  )
			{
				bool needsRemeasure = false;
				bool usePreferredWidth = usePreferredSettings;
				bool usePreferredHeight = usePreferredSettings;

				if (usePreferredWidth)
				{
					double preferredWidth = GridBagPanel.GetPreferredWidth(_elem);

					if (!double.IsNaN(preferredWidth))
						measureSize.Width = preferredWidth;
					else if (!double.IsInfinity(measureSize.Width) && this.ColumnWeight != 0)
					{
						// during the initial measure pass, we're going to use 0 for the 
						// measure size for elements that have a weight. during the second 
						// pass we'll measure with an actual value based upon the size 
						// of the element that the layout calculates
						usePreferredWidth = false;
					}
				}

				if (usePreferredHeight)
				{
					double preferredHeight = GridBagPanel.GetPreferredHeight(_elem);

					if (!double.IsNaN(preferredHeight))
						measureSize.Height = preferredHeight;
					else if (!double.IsInfinity(measureSize.Height) && this.RowWeight != 0)
					{
						// during the initial measure pass, we're going to use 0 for the 
						// measure size for elements that have a weight. during the second 
						// pass we'll measure with an actual value based upon the size 
						// of the element that the layout calculates
						usePreferredHeight = false;
					}
				}

				if (_panel._isInitialMeasure)
				{
					// if both dimensions are weighted then don't do anything now
					if (!usePreferredHeight && !usePreferredWidth)
						return true;

					if (!usePreferredHeight)
					{
						measureSize.Height = 0;
						needsRemeasure = true;
					}

					if (!usePreferredWidth)
					{
						measureSize.Width = 0;
						needsRemeasure = true;
					}
				}
				else
				{
					// AS 7/13/10
					// We found an issue when a nested element wanted to be larger due to some template change. What was 
					// happening is that the element was measured with a specific size so when it was remeasured after the 
					// template change its new desired size was constrained by the available size so a series of 
					// onchilddesiredsize changes didn't percolate up to the gridbagpanel for it to arrange it with a 
					// larger size. to get around this I think we need to pass down the larger size for the measure 
					// as long as the desired size is the same or smaller than the size we were about to measure with.
					//
					if (!usePreferredWidth)
					{
						// if the element wanted to be larger then we need to measure it with the smaller size that is 
						// provided but if it wants to be the same size or smaller then we should use the previous
						// measure size
						if (this.ColumnWeight == 0 && !CoreUtilities.GreaterThan(_elem.DesiredSize.Width, measureSize.Width) && !_lastElemMeasureSize.IsEmpty )
						{
							measureSize.Width = _lastElemMeasureSize.Width;
						}
					}

					if (!usePreferredHeight)
					{
						// if the element wanted to be larger then we need to measure it with the smaller size that is 
						// provided but if it wants to be the same size or smaller then we should use the previous
						// measure size
						if (this.RowWeight == 0 && !CoreUtilities.GreaterThan(_elem.DesiredSize.Height, measureSize.Height) && !_lastElemMeasureSize.IsEmpty )
						{
							measureSize.Height = _lastElemMeasureSize.Height;
						}
					}
				}


				// AS 5/9/12 TFS103196
				if (isPositioningItem || !_panel._isInitialMeasure )
				{
					if (!_lastElemMeasureSize.IsEmpty
						&& !usePreferredWidth // AS 5/9/12 TFS104555
						&& (isPositioningItem || double.IsPositiveInfinity(_panel._currentMeasureSize.Width)) // AS 5/9/12 TFS104555
						&& CoreUtilities.LessThanOrClose(measureSize.Width, _lastElemMeasureSize.Width)
						&& CoreUtilities.GreaterThanOrClose(measureSize.Width, _elem.DesiredSize.Width))
					{
						measureSize.Width = _lastElemMeasureSize.Width;
					}

					if (!_lastElemMeasureSize.IsEmpty
						&& !usePreferredHeight //AS 5/9/12 TFS104555
						&& (isPositioningItem || double.IsPositiveInfinity(_panel._currentMeasureSize.Height)) // AS 5/9/12 TFS104555
						&& CoreUtilities.LessThanOrClose(measureSize.Height, _lastElemMeasureSize.Height)
						&& CoreUtilities.GreaterThanOrClose(measureSize.Height, _elem.DesiredSize.Height))
					{
						measureSize.Height = _lastElemMeasureSize.Height;
					}
				}

				DebugHelper.DebugLayout(_panel, false, true, "MeasureElem Start", "Element:{0} [{1}], MeasureSize:{2}, NeedsRemeasure:{3}", _elem.GetType().Name, _elem.GetHashCode(), measureSize, needsRemeasure);

				_elem.Measure(measureSize);
				_lastElemMeasureSize = measureSize;

				DebugHelper.DebugLayout(_panel, true, false, "MeasureElem End", "Element:{0} [{1}], MeasureSize:{2}, DesiredSize:{3}", _elem.GetType().Name, _elem.GetHashCode(), measureSize, _elem.DesiredSize);

				return needsRemeasure;
			}

			#endregion // MeasureElem

			#endregion // Methods

			#region ILayoutItem Properties

			#region MaximumSize

			public Size MaximumSize
			{
				get
				{
					FrameworkElement fe = _elem as FrameworkElement;
					if ( null != fe )
						return new Size( fe.MaxWidth, fe.MaxHeight );

					return Size.Empty;
				}
			}

			#endregion // MaximumSize

			#region MinimumSize

			public Size MinimumSize
			{
				get
				{
					FrameworkElement fe = _elem as FrameworkElement;
					if ( null != fe )
						return new Size( fe.MinWidth, fe.MinHeight );

					return new Size( double.PositiveInfinity, double.PositiveInfinity );
				}
			}

			#endregion // MinimumSize

			#region PreferredSize

			public Size PreferredSize
			{
				get
				{
					double width = GridBagPanel.GetPreferredWidth( _elem );
					double height = GridBagPanel.GetPreferredHeight( _elem );

					Size desiredSize = _elem.DesiredSize;

					if ( double.IsNaN( width ) )
					{
						// during the initial pass we didn't measure the element in a given 
						// direction if the element had a weight in this orientation. instead 
						// we will wait for the second measure pass of the element at which 
						// point we will have been given the arranged size to use
						if (!double.IsInfinity(_lastElemMeasureSize.Width) && _panel._isInitialMeasure && this.ColumnWeight != 0)
							width = _lastElemMeasureSize.Width;
						else
							width = desiredSize.Width;
					}
					else
						width += this.GetMarginExtent( true );

					if ( double.IsNaN( height ) )
					{
						// during the initial pass we didn't measure the element in a given 
						// direction if the element had a weight in this orientation. instead 
						// we will wait for the second measure pass of the element at which 
						// point we will have been given the arranged size to use
						if (!double.IsInfinity(_lastElemMeasureSize.Height) && _panel._isInitialMeasure && this.RowWeight != 0)
							height = _lastElemMeasureSize.Height;
						else
							height = desiredSize.Height;
					}
					else
						height += this.GetMarginExtent( false );

					return new Size( width, height );
				}
			}

			#endregion // PreferredSize

			#region Visibility

			public Visibility Visibility
			{
				get
				{
					return _elem.Visibility;
				}
			}

			#endregion // Visibility

			#endregion // ILayoutItem Properties

			#region IGridBagConstraint Properties

			#region Column

			public int Column
			{
				get
				{
					return GridBagPanel.GetColumn(_elem);
				}
			}

			#endregion // Column

			#region ColumnSpan

			public int ColumnSpan
			{
				get
				{
					return GridBagPanel.GetColumnSpan(_elem);
				}
			}

			#endregion // ColumnSpan

			#region ColumnWeight

			public float ColumnWeight
			{
				get
				{
					return GridBagPanel.GetColumnWeight(_elem);
				}
			}

			#endregion // ColumnWeight

			#region HorizontalAlignment

			public HorizontalAlignment HorizontalAlignment
			{
				get
				{
					FrameworkElement fe = _elem as FrameworkElement;
					if (null != fe)
						return fe.HorizontalAlignment;

					return HorizontalAlignment.Stretch;
				}
			}

			#endregion // HorizontalAlignment

			#region Margin

			public Thickness Margin
			{
				get
				{
					FrameworkElement fe = _elem as FrameworkElement;
					if (null != fe)
						return fe.Margin;

					return g_DefaultMargin;
				}
			}

			#endregion // Margin

			#region Row

			public int Row
			{
				get
				{
					return GridBagPanel.GetRow(_elem);
				}
			}

			#endregion // Row

			#region RowSpan

			public int RowSpan
			{
				get
				{
					return GridBagPanel.GetRowSpan(_elem);
				}
			}

			#endregion // RowSpan

			#region RowWeight

			public float RowWeight
			{
				get
				{
					return GridBagPanel.GetRowWeight(_elem);
				}
			}

			#endregion // RowWeight

			#region VerticalAlignment

			public VerticalAlignment VerticalAlignment
			{
				get
				{
					FrameworkElement fe = _elem as FrameworkElement;
					if (null != fe)
						return fe.VerticalAlignment;

					return VerticalAlignment.Stretch;
				}
			}

			#endregion // VerticalAlignment

			#endregion // IGridBagConstraint Properties
		}

		#endregion // LayoutItem Class

		#region PanelChildrenObserver class


#region Infragistics Source Cleanup (Region)



































#endregion // Infragistics Source Cleanup (Region)

		#endregion // PanelChildrenObserver class

		#endregion // Nested Data Structures

		#region Member Variables

		private GridBagLayoutManager _layoutManager;

		// AS 6/2/10
		//private bool _skipOnChildDesiredSizeChanged;

		private bool _isInitialMeasure;
		private Size _currentMeasureSize; // AS 5/9/12 TFS104555





		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="GridBagPanel"/>
		/// </summary>
		public GridBagPanel( )
		{
			_layoutManager = new GridBagLayoutManager( );
			_layoutManager.ExpandToFitHeight = true;
			_layoutManager.ExpandToFitWidth = true;
			_layoutManager.PreferredSizeIncludesMargin = true;
			_layoutManager.IncludeMarginInPositionRect = true;




		}

		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride( Size finalSize )
		{
			Size originalFinal = finalSize;
			DebugHelper.DebugLayout(this, false, true, "ArrangeOverride Start", "FinalSize: {0}", originalFinal);

			UIElementCollection children = this.Children;

			LayoutContainer layoutContainer = new LayoutContainer( this, new Rect( 0, 0, finalSize.Width, finalSize.Height ), false );
			_layoutManager.LayoutContainer( layoutContainer, this );



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


			Size minSize = _layoutManager.CalculateMinimumSize( layoutContainer, this );

			// This is to make sure that items that are laid out beyond the finalSize parameter get
			// clipped. Items can get laid out beyond the finalSize if there are minimum size constraints
			// on the items.
			// 
			finalSize.Width = Math.Max( finalSize.Width, minSize.Width );
			finalSize.Height = Math.Max( finalSize.Height, minSize.Height );

			DebugHelper.DebugLayout(this, true, false, "ArrangeOverride End", "FinalSize: {0}, ArrangeSize:{1}", originalFinal, finalSize);

			return finalSize;
		}

		#endregion // ArrangeOverride

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride( Size availableSize )
		{
			DebugHelper.DebugLayout(this, false, true, "MeasureOverride Start", "AvailableSize: {0}", availableSize);

			_currentMeasureSize = availableSize; // AS 5/9/12 TFS104555

			// Syncrhonize the layout manager's LayoutItems collection with the child elements of the panel.
			// 
			this.VerifyLayoutManagerItems( );

			// Measure all child elements.
			// 
			LayoutItemsCollection layoutItems = _layoutManager.LayoutItems;
			List<LayoutItem> remeasureItems = null;
			Debug.Assert(!_isInitialMeasure, "Recursive measure?");

			DebugHelper.DebugLayout(this, false, true, "MeasureOverride Initial Measure Start", null);

			try
			{
				_isInitialMeasure = true;

				for (int i = 0, count = layoutItems.Count; i < count; i++)
				{
					LayoutItem layoutItem = (LayoutItem)layoutItems[i];
					bool remeasure = layoutItem.MeasureElem(availableSize, true);

					if (remeasure)
					{
						if (remeasureItems == null)
							remeasureItems = new List<LayoutItem>();

						remeasureItems.Add(layoutItem);
					}
				}
			}
			finally
			{
				_isInitialMeasure = false;
			}

			DebugHelper.DebugLayout(this, true, false, "MeasureOverride Initial Measure End", null);

			// Recalculate the layout.
			// 
			this.InvalidateLayoutHelper();
			LayoutContainer layoutContainer = new LayoutContainer(this, new Rect(0, 0, availableSize.Width, availableSize.Height), true);
			Size size = _layoutManager.CalculatePreferredSize(layoutContainer, this);
			Size minSize = _layoutManager.CalculateMinimumSize(layoutContainer, this);

			DebugHelper.DebugLayout(this, "MeasureOverride InitialMeasure End", "Size:{0}, MinSize:{1}", size, minSize);

			// if we had items that need to be remeasured...
			if (null != remeasureItems)
			{
				DebugHelper.DebugLayout(this, false, true, "MeasureOverride Remeasure Start", "Count: {0}", remeasureItems.Count);

 				GridBagLayoutItemDimensionsCollection cache = _layoutManager.GetLayoutItemDimensions(layoutContainer, this);

				for (int i = 0, count = remeasureItems.Count; i < count; i++)
				{
					LayoutItem layoutItem = (LayoutItem)remeasureItems[i];
					GridBagLayoutItemDimensions dims = cache[layoutItem];

					
					if (dims != null)
					{
						DebugHelper.DebugLayout(this, "MeasureOverride Remeasure", "Item: {0}, Size:{1}", layoutItem._elem, dims.Size);

						layoutItem.MeasureElem(dims.Size, false);
					}
				}

				// now recalculate the layout
				this.InvalidateLayoutHelper();
				size = _layoutManager.CalculatePreferredSize(layoutContainer, this);
				minSize = _layoutManager.CalculateMinimumSize(layoutContainer, this);

				DebugHelper.DebugLayout(this, true, false, "MeasureOverride Remeasure End", "Count: {0}, Size:{1}, MinSize:{2}", remeasureItems.Count, size, minSize);
			}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			// If not enough space is available to layout items at their preferred sizes then return
			// the smaller size to proportionally resize elements smaller (done in arrange).
			// 
			if ( !double.IsNaN( availableSize.Width ) && availableSize.Width < size.Width )
			{
				DebugHelper.DebugLayout(this, "MeasureOverride Reducing Width", "Available: {0}, Size:{1}, Min:{2}", availableSize.Width, size.Width, minSize.Width);

				size.Width = Math.Max(minSize.Width, availableSize.Width);
			}

			if ( !double.IsNaN( availableSize.Height ) && availableSize.Height < size.Height )
			{
				DebugHelper.DebugLayout(this, "MeasureOverride Reducing Height", "Available: {0}, Size:{1}, Min:{2}", availableSize.Height, size.Height, minSize.Height);

				size.Height = Math.Max(minSize.Height, availableSize.Height);
			}

			//// finally remeasure all the elements with the size that they will be arranged at 
			//// assuming we are arranged with the desired size
			Size measureSize = availableSize;

			if (double.IsInfinity(measureSize.Width))
				measureSize.Width = size.Width;

			if (double.IsInfinity(measureSize.Height))
				measureSize.Height = size.Height;

			DebugHelper.DebugLayout(this, false, true, "MeasureOverride LayoutContainer Start", "MeasureSize:{0}", measureSize);

			layoutContainer = new LayoutContainer(this, new Rect(0, 0, measureSize.Width, measureSize.Height), true);
			_layoutManager.LayoutContainer(layoutContainer, this);

			DebugHelper.DebugLayout(this, true, false, "MeasureOverride LayoutContainer End", "MeasureSize:{0}", measureSize);

			DebugHelper.DebugLayout(this, true, false, "MeasureOverride End", "AvailableSize: {0}, MinSize:{1}, Desired: {2}", availableSize, minSize, size);

			return size;
		}

		#endregion // MeasureOverride

		#region OnChildDesiredSizeChanged

		
#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)

		#endregion // OnChildDesiredSizeChanged

		#region OnVisualChildrenChanged


		/// <summary>
		/// Overridden. Called the visual children change.
		/// </summary>
		/// <param name="visualAdded">Visual that was added.</param>
		/// <param name="visualRemoved">Visual that was removed.</param>
		protected override void OnVisualChildrenChanged( DependencyObject visualAdded, DependencyObject visualRemoved )
		{
			this.InvalidateLayoutHelper( );

			base.OnVisualChildrenChanged( visualAdded, visualRemoved );
		}


		#endregion // OnVisualChildrenChanged

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region Column

		/// <summary>
		/// Identifies the Column attached dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnProperty = DependencyProperty.RegisterAttached(
			"Column",
			typeof( int ),
			typeof( GridBagPanel ),
			new PropertyMetadata( GridBagConstraintConstants.Relative, new PropertyChangedCallback( OnConstraintPropertyChanged ) )

			, new ValidateValueCallback( ValidateColumnRow )

		);

		/// <summary>
		/// Gets the value of the Column attached property of the specified element. Default value is -1, which indicates
		/// that the element will be positioned relative to the previous element in the panel.
		/// </summary>
		/// <param name="elem">This element's Column value will be returned.</param>
		/// <returns>The value of the Column attached property.</returns>

		[AttachedPropertyBrowsableForChildren()]

		public static int GetColumn( UIElement elem )
		{
			return (int)elem.GetValue( ColumnProperty );
		}

		/// <summary>
		/// Sets the value of the Column attached property of the specified element. Default value is -1, which indicates
		/// that the element will be positioned relative to the previous element in the panel.
		/// </summary>
		/// <param name="elem">This element's Column value will be set.</param>
		/// <param name="value">Value to set. This can be -1 which will position the element relative to previous element in the panel.</param>
		public static void SetColumn( UIElement elem, int value )
		{
			elem.SetValue( ColumnProperty, value );
		}

		#endregion // Column

		#region ColumnSpan

		/// <summary>
		/// Identifies the ColumnSpan attached dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.RegisterAttached(
			"ColumnSpan",
			typeof( int ),
			typeof( GridBagPanel ),
			new PropertyMetadata( 1,new PropertyChangedCallback( OnConstraintPropertyChanged ) )

			, new ValidateValueCallback( ValidateColumnRowSpan )

		);

		/// <summary>
		/// Gets the value of the ColumnSpan attached property of the specified element. Default value is 0, which indicates
		/// that the element will occupy the remainder of the space in its logical column.
		/// </summary>
		/// <param name="elem">This element's ColumnSpan value will be returned.</param>
		/// <returns>The value of the ColumnSpan attached property.</returns>

		[AttachedPropertyBrowsableForChildren()]

		public static int GetColumnSpan(UIElement elem)
		{
			return (int)elem.GetValue( ColumnSpanProperty );
		}

		/// <summary>
		/// Sets the value of the ColumnSpan attached property of the specified element. Default value is 0, which indicates
		/// that the element will occupy the remainder of the space in its logical column.
		/// </summary>
		/// <param name="elem">This element's ColumnSpan value will be set.</param>
		/// <param name="value">Value to set. This can be 0 to indicate that the element should occupy the remainder of the logical column.</param>
		public static void SetColumnSpan( UIElement elem, int value )
		{
			elem.SetValue( ColumnSpanProperty, value );
		}

		#endregion // ColumnSpan

		#region ColumnWeight

		/// <summary>
		/// Identifies the ColumnWeight attached dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnWeightProperty = DependencyProperty.RegisterAttached(
			"ColumnWeight",
			typeof( float ),
			typeof( GridBagPanel ),
			new PropertyMetadata( 0f, new PropertyChangedCallback( OnConstraintPropertyChanged ) )

			, new ValidateValueCallback( ValidateColumnRowWeight )

		);

		/// <summary>
		/// Gets the value of the ColumnWeight attached property of the specified element. ColumnWeight specifies
		/// how any extra width will be distributed among elements.
		/// </summary>
		/// <param name="elem">This element's ColumnWeight value will be returned.</param>
		/// <returns>The value of the ColumnWeight attached property.</returns>

		[AttachedPropertyBrowsableForChildren()]



		public static float GetColumnWeight(UIElement elem)
		{
			return (float)elem.GetValue( ColumnWeightProperty );
		}

		/// <summary>
		/// Sets the value of the ColumnWeight attached property of the specified element. ColumnWeight specifies
		/// how any extra width will be distributed among elements.
		/// </summary>
		/// <param name="elem">This element's ColumnWeight value will be returned.</param>
		/// <param name="value">Value to set.</param>
		/// <returns>The value of the ColumnWeight attached property.</returns>
		public static void SetColumnWeight( UIElement elem, float value )
		{
			elem.SetValue( ColumnWeightProperty, value );
		}

		#endregion // ColumnWeight

		#region PreferredHeight

		/// <summary>
		/// Identifies the PreferredHeight attached dependency property.
		/// </summary>
		public static readonly DependencyProperty PreferredHeightProperty = DependencyProperty.RegisterAttached(
			"PreferredHeight",
			typeof( double ),
			typeof( GridBagPanel ),
			new PropertyMetadata( double.NaN, new PropertyChangedCallback( OnConstraintPropertyChanged ) )

			, new ValidateValueCallback( ValidatePreferredWidthHeight )

		);

		/// <summary>
		/// Gets the preferred height of the element. Default value is double.NaN.
		/// </summary>
		/// <param name="d">Element whose preferred height to get.</param>
		/// <returns>Returns the preferred height of the specified element.</returns>

		[AttachedPropertyBrowsableForChildren()]

		public static double GetPreferredHeight(DependencyObject d)
		{
			return (double)d.GetValue( PreferredHeightProperty );
		}

		/// <summary>
		/// Sets the preferred height of the element. This height will be used by the panel to
		/// determine how much space to allocate to the element.
		/// </summary>
		/// <param name="d">Element to set the preferred height on.</param>
		/// <param name="value">Preferred height to set.</param>
		/// <remarks>
		/// <para class="body">
		/// PreferredHeight property is different from setting the Height property of the element
		/// directly. When you set Height property of the element, that will be its final height.
		/// It may be desirable instead to specify a preferred height and then let the layout panel
		/// adjust the size (make it bigger or smaller) based on the availability of space.
		/// </para>
		/// </remarks>
		public static void SetPreferredHeight( DependencyObject d, double value )
		{
			d.SetValue( PreferredHeightProperty, value );
		}

		#endregion // PreferredHeight

		#region PreferredWidth

		/// <summary>
		/// Identifies the PreferredWidth attached dependency property.
		/// </summary>
		public static readonly DependencyProperty PreferredWidthProperty = DependencyProperty.RegisterAttached(
			"PreferredWidth",
			typeof( double ),
			typeof( GridBagPanel ),
			new PropertyMetadata( double.NaN, new PropertyChangedCallback( OnConstraintPropertyChanged ) )

			, new ValidateValueCallback( ValidatePreferredWidthHeight )

		);

		/// <summary>
		/// Gets the preferred width of the element. Default value is double.NaN.
		/// </summary>
		/// <param name="d">Element whose preferred width to get.</param>
		/// <returns>Returns the preferred width of the specified element.</returns>

		[AttachedPropertyBrowsableForChildren()]

		public static double GetPreferredWidth(DependencyObject d)
		{
			return (double)d.GetValue( PreferredWidthProperty );
		}

		/// <summary>
		/// Sets the preferred width of the element. This width will be used by the panel to
		/// determine how much space to allocate to the element. 
		/// </summary>
		/// <param name="d">Element to set the preferred width on.</param>
		/// <param name="value">Preferred width to set.</param>
		/// <remarks>
		/// <para class="body">
		/// PreferredWidth property is different from setting the Width property of the element
		/// directly. When you set Width property of the element, that will be its final width.
		/// It may be desirable instead to specify a preferred width and then let the layout panel
		/// adjust the size (make it bigger or smaller) based on the availability of space.
		/// </para>
		/// </remarks>
		public static void SetPreferredWidth( DependencyObject d, double value )
		{
			d.SetValue( PreferredWidthProperty, value );
		}

		#endregion // PreferredWidth

		#region Row

		/// <summary>
		/// Identifies the Row attached dependency property.
		/// </summary>
		public static readonly DependencyProperty RowProperty = DependencyProperty.RegisterAttached(
			"Row",
			typeof( int ),
			typeof( GridBagPanel ),
			new PropertyMetadata( GridBagConstraintConstants.Relative, new PropertyChangedCallback( OnConstraintPropertyChanged ) )

			, new ValidateValueCallback( ValidateColumnRow )

		);

		/// <summary>
		/// Gets the value of the Row attached property of the specified element. Default value is -1, which indicates
		/// that the element will be positioned relative to the previous element in the panel.
		/// </summary>
		/// <param name="elem">This element's Row value will be returned.</param>
		/// <returns>The value of the Row attached property.</returns>

		[AttachedPropertyBrowsableForChildren()]

		public static int GetRow(UIElement elem)
		{
			return (int)elem.GetValue( RowProperty );
		}

		/// <summary>
		/// Sets the value of the Row attached property of the specified element. Default value is -1, which indicates
		/// that the element will be positioned relative to the previous element in the panel.
		/// </summary>
		/// <param name="elem">This element's Row value will be set.</param>
		/// <param name="value">Value to set. This can be -1 which will position the element relative to previous element in the panel.</param>
		public static void SetRow( UIElement elem, int value )
		{
			elem.SetValue( RowProperty, value );
		}

		#endregion // Row

		#region RowSpan

		/// <summary>
		/// Identifies the RowSpan attached dependency property.
		/// </summary>
		public static readonly DependencyProperty RowSpanProperty = DependencyProperty.RegisterAttached(
			"RowSpan",
			typeof( int ),
			typeof( GridBagPanel ),
			new PropertyMetadata( 1, new PropertyChangedCallback( OnConstraintPropertyChanged ) )

			, new ValidateValueCallback( ValidateColumnRowSpan )

		);

		/// <summary>
		/// Gets the value of the RowSpan attached property of the specified element. Default value is 0, which indicates
		/// that the element will occupy the remainder of the space in its logical column.
		/// </summary>
		/// <param name="elem">This element's RowSpan value will be returned.</param>
		/// <returns>The value of the RowSpan attached property.</returns>

		[AttachedPropertyBrowsableForChildren()]

		public static int GetRowSpan(UIElement elem)
		{
			return (int)elem.GetValue( RowSpanProperty );
		}

		/// <summary>
		/// Sets the value of the RowSpan attached property of the specified element. Default value is 0, which indicates
		/// that the element will occupy the remainder of the space in its logical row.
		/// </summary>
		/// <param name="elem">This element's RowSpan value will be set.</param>
		/// <param name="value">Value to set. This can be 0 to indicate that the element should occupy the remainder of the logical row.</param>
		public static void SetRowSpan( UIElement elem, int value )
		{
			elem.SetValue( RowSpanProperty, value );
		}

		#endregion // RowSpan

		#region RowWeight

		/// <summary>
		/// Identifies the RowWeight attached dependency property.
		/// </summary>
		public static readonly DependencyProperty RowWeightProperty = DependencyProperty.RegisterAttached(
			"RowWeight",
			typeof( float ),
			typeof( GridBagPanel ),
			new PropertyMetadata( 0f, new PropertyChangedCallback( OnConstraintPropertyChanged ) )

			, new ValidateValueCallback( ValidateColumnRowWeight )

		);

		/// <summary>
		/// Gets the value of the RowWeight attached property of the specified element. RowWeight specifies
		/// how any extra height will be distributed among elements.
		/// </summary>
		/// <param name="elem">This element's RowWeight value will be returned.</param>
		/// <returns>The value of the RowWeight attached property.</returns>

		[AttachedPropertyBrowsableForChildren()]



		public static float GetRowWeight(UIElement elem)
		{
			return (float)elem.GetValue( RowWeightProperty );
		}

		/// <summary>
		/// Sets the value of the RowWeight attached property of the specified element. RowWeight specifies
		/// how any extra height will be distributed among elements.
		/// </summary>
		/// <param name="elem">This element's RowWeight value will be returned.</param>
		/// <param name="value">Value to set.</param>
		/// <returns>The value of the RowWeight attached property.</returns>
		public static void SetRowWeight( UIElement elem, float value )
		{
			elem.SetValue( RowWeightProperty, value );
		}

		#endregion // RowWeight

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
		#region GetColumnExtent
		internal double GetColumnExtent( int column )
		{
			return GetExtent(column, false) ?? double.NaN;
		} 
		#endregion // GetColumnExtent

		// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
		#region GetRowExtent
		internal double GetRowExtent( int row )
		{
			return GetExtent(row, true) ?? double.NaN;
		} 
		#endregion // GetRowExtent

		#region Private Methods

		// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
		#region GetExtent
		private double? GetExtent( int offset, bool isRow )
		{
			var layoutContainer = new LayoutContainer(this, new Rect(0, 0, this.RenderSize.Width, this.RenderSize.Height), true);
			var itemDims = _layoutManager.GetLayoutItemDimensions(layoutContainer, this);
			var dims = isRow ? itemDims.RowDims : itemDims.ColumnDims;

			Debug.Assert(offset >= 0 && offset < dims.Length - 1, "This should be a row/column index");

			if ( offset < 0 || offset >= dims.Length - 1 )
				return null;

			return dims[offset + 1] - dims[offset];
		}
		#endregion // GetExtent

		#region InvalidateLayoutHelper

		private void InvalidateLayoutHelper( )
		{
			_layoutManager.InvalidateLayout( );
		}

		#endregion // InvalidateLayoutHelper

		#region OnConstraintPropertyChanged

		private static void OnConstraintPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{

			Visual v = d as Visual;


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


			GridBagPanel panel = null != v ? VisualTreeHelper.GetParent( v ) as GridBagPanel : null;
			if ( null != panel )
				panel.InvalidateMeasure( );
		}

		#endregion // OnConstraintPropertyChanged

		#region ValidateColumnRow

		private static bool ValidateColumnRow( object objVal )
		{
			int val = (int)objVal;
			return val >= 0 || val == GridBagConstraintConstants.Relative;
		}

		#endregion // ValidateColumnRow

		#region ValidateColumnRowSpan

		private static bool ValidateColumnRowSpan( object objVal )
		{
			int val = (int)objVal;
			return val >= 1 || val == GridBagConstraintConstants.Remainder;
		}

		#endregion // ValidateColumnRowSpan

		#region ValidateColumnRowWeight

		private static bool ValidateColumnRowWeight( object objVal )
		{
			float val = (float)objVal;

            if (float.IsNaN(val) || float.IsInfinity(val))
                return false;

            return val >= 0f;
		}

		#endregion // ValidateColumnRowWeight

		#region ValidatePreferredWidthHeight

		private static bool ValidatePreferredWidthHeight( object objVal )
		{
			double val = (double)objVal;
			return double.IsNaN( val ) || val >= 0 && !double.IsPositiveInfinity( val );
		}

		#endregion // ValidatePreferredWidthHeight

		#region VerifyLayoutManagerItems

		private bool VerifyLayoutManagerItems( )
		{
			LayoutItemsCollection items = _layoutManager.LayoutItems;
			UIElementCollection children = this.Children;

			bool childrenChanged = items.Count != children.Count;

			if ( !childrenChanged )
			{
				for ( int i = 0, count = items.Count; i < count; i++ )
				{
					LayoutItem item = (LayoutItem)items[i];
					UIElement elem = children[i];
					if ( elem != item._elem )
					{
						childrenChanged = true;
						break;
					}
				}
			}

			if ( childrenChanged )
			{
				items.Clear( );

				for ( int i = 0, count = children.Count; i < count; i++ )
				{
					UIElement elem = children[i];
					LayoutItem item = new LayoutItem(elem, this);
					items.Add( item, item );
				}

				return true;
			}

			return false;
		}

		#endregion // VerifyLayoutManagerItems

		#endregion // Private Methods

		#endregion // Methods
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