using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;

namespace Infragistics.Windows.Layout
{
	#region GridBagPanel Class

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
	
	
	
	
	internal class GridBagPanel : Panel
	{
		#region Nested Data Structures

		#region Constraint Class

		private class Constraint : IGridBagConstraint
		{
			#region Member Vars

			private static Thickness g_DefaultMargin = new Thickness( );

			private UIElement _elem;
			private GridBagPanel _gbPanel;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="elem">Associated element.</param>
			/// <param name="gbPanel">Associated gridbag panel.</param>
			internal Constraint( UIElement elem, GridBagPanel gbPanel )
			{
				_elem = elem;
				_gbPanel = gbPanel;
			}

			#endregion // Constructor

			#region Properties

			#region Public Properties

			#region Column

			public int Column
			{
				get
				{
					return GridBagPanel.GetColumn( _elem );
				}
			}

			#endregion // Column

			#region ColumnSpan

			public int ColumnSpan
			{
				get
				{
					return GridBagPanel.GetColumnSpan( _elem );
				}
			}

			#endregion // ColumnSpan

			#region ColumnWeight

			public float ColumnWeight
			{
				get
				{
					return GridBagPanel.GetColumnWeight( _elem );
				}
			}

			#endregion // ColumnWeight

			#region HorizontalAlignment

			public HorizontalAlignment HorizontalAlignment
			{
				get
				{
					FrameworkElement fe = _elem as FrameworkElement;
					if ( null != fe )
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
					if ( null != fe )
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
					return GridBagPanel.GetRow( _elem );
				}
			}

			#endregion // Row

			#region RowSpan

			public int RowSpan
			{
				get
				{
					return GridBagPanel.GetRowSpan( _elem );
				}
			}

			#endregion // RowSpan

			#region RowWeight

			public float RowWeight
			{
				get
				{
					return GridBagPanel.GetRowWeight( _elem );
				}
			}

			#endregion // RowWeight

			#region VerticalAlignment

			public VerticalAlignment VerticalAlignment
			{
				get
				{
					FrameworkElement fe = _elem as FrameworkElement;
					if ( null != fe )
						return fe.VerticalAlignment;

					return VerticalAlignment.Stretch;
				}
			}

			#endregion // VerticalAlignment

			#endregion // Properties

			#endregion // Properties
		}

		#endregion // Constraint Class

		#region LayoutContainer Class

		private class LayoutContainer : ILayoutContainer
		{
			#region Member Vars

			private GridBagPanel _gbPanel;
			private Rect _rect;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="gbPanel"></param>
			/// <param name="rect"></param>
			internal LayoutContainer( GridBagPanel gbPanel, Rect rect )
			{
				_gbPanel = gbPanel;
				_rect = rect;
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
				( (LayoutItem)i ).ArrangeElem( rect, _gbPanel );
			}

			#endregion // PositionItem
		}

		#endregion // LayoutContainer Class

		#region LayoutItem Class

		private class LayoutItem : ILayoutItem
		{
			#region Member Vars

			internal UIElement _elem;
			internal Size _lastElemMeasureSize;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="elem">Associated element.</param>
			internal LayoutItem( UIElement elem )
			{
				_elem = elem;
			}

			#endregion // Constructor

			#region Methods

			#region ArrangeElem

			internal void ArrangeElem( Rect rect, GridBagPanel gbPanel )
			{
				// If DesiredSize of the element is bigger than the item's arrange rect, the
				// item will get clipped. It will actually be positioned with the RenderSize that's
				// same as DesiredSize (minus margin). So we have to perform a second Measure 
				// on the item with position rect's size as the available size parameter to the 
				// Measure call.
				// 
				// Apparently certain controls, like virtualizing panel, are making certain
				// assumptions regarding the measure and arrange sizes being the same. So
				// we have to re-measure the element even if its arrange rect is bigger.
				// 
				
				
				
				Size desiredSize = _elem.DesiredSize;
				if ( CoreUtilities.LessThan( rect.Width, desiredSize.Width )
					|| CoreUtilities.LessThan( rect.Height, desiredSize.Height )
					|| CoreUtilities.GreaterThan( rect.Width, _lastElemMeasureSize.Width )
					|| CoreUtilities.GreaterThan( rect.Height, _lastElemMeasureSize.Height ) )
				{
					// Set _skipOnChildDesiredSizeChanged flag which the gb panel uses in the
					// OnChildDesiredSizeChanged to prevent recursive invalidation of measure
					// otherwise measuring the child element will cause the gb panel's measure
					// to be invalidated.
					// 
					bool origVal = gbPanel._skipOnChildDesiredSizeChanged;
					gbPanel._skipOnChildDesiredSizeChanged = true;
					try
					{
						this.MeasureElem( new Size(rect.Width, rect.Height), false );
					}
					finally
					{
						gbPanel._skipOnChildDesiredSizeChanged = origVal;
					}
				}

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

			internal void MeasureElem( Size measureSize, bool usePreferredSettings )
			{
				if ( usePreferredSettings )
				{
					double preferredWidth = GridBagPanel.GetPreferredWidth( _elem );
					double preferredHeight = GridBagPanel.GetPreferredHeight( _elem );

					if ( !double.IsNaN( preferredWidth ) )
						measureSize.Width = preferredWidth;

					if ( !double.IsNaN( preferredHeight ) )
						measureSize.Height = preferredHeight;
				}

				_elem.Measure( measureSize );
				_lastElemMeasureSize = measureSize;
			}

			#endregion // MeasureElem

			#endregion // Methods

			#region Properties

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
						width = desiredSize.Width;
					else
						width += this.GetMarginExtent( true );

					if ( double.IsNaN( height ) )
						height = desiredSize.Height;
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

			#endregion // Properties
		}

		#endregion // LayoutItem Class

		#endregion // Nested Data Structures

		#region Vars

		private GridBagLayoutManager _layoutManager;
		private bool _skipOnChildDesiredSizeChanged;

		#endregion // Vars

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
			UIElementCollection children = this.Children;

			LayoutContainer layoutContainer = new LayoutContainer( this, new Rect( 0, 0, finalSize.Width, finalSize.Height ) );
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
			// Syncrhonize the layout manager's LayoutItems collection with the child elements of the panel.
			// 
			this.VerifyLayoutManagerItems( );

			// Measure all child elements.
			// 
			LayoutItemsCollection layoutItems = _layoutManager.LayoutItems;
			for ( int i = 0, count = layoutItems.Count; i < count; i++ )
			{
				LayoutItem layoutItem = (LayoutItem)layoutItems[i];
				layoutItem.MeasureElem( availableSize, true );
			}

			// Recalculate the layout.
			// 
			this.InvalidateLayoutHelper( );
			LayoutContainer layoutContainer = new LayoutContainer( this, new Rect( 0, 0, availableSize.Width, availableSize.Height ) );
			Size size = _layoutManager.CalculatePreferredSize( layoutContainer, this );
			Size minSize = _layoutManager.CalculateMinimumSize( layoutContainer, this );

			// If not enough space is available to layout items at their preferred sizes then return
			// the smaller size to proportionally resize elements smaller (done in arrange).
			// 
			if ( !double.IsNaN( availableSize.Width ) && availableSize.Width < size.Width )
				size.Width = Math.Max( minSize.Width, availableSize.Width );

			if ( !double.IsNaN( availableSize.Height ) && availableSize.Height < size.Height )
				size.Height = Math.Max( minSize.Height, availableSize.Height );

			return size;
		}

		#endregion // MeasureOverride

		#region OnChildDesiredSizeChanged


		/// <summary>
		/// Overridden. Called when the desired size of a child element changes.
		/// </summary>
		/// <param name="child">Child element whose desired size changed.</param>
		protected override void OnChildDesiredSizeChanged( UIElement child )
		{
			// When we measure the elements with a bigger size their DesiredSize will be big enough to
			// accomodate their contents. However when we go to position the element and the available
			// size for positioning is smaller than the DesiredSize, the framework will clip the element
			// instead of simply positioning it smaller. For example, the TextBox won't be positioned
			// with the rect that's passed into its Arrange call. Instead it will be positioned using
			// its DesiredSize and then clipped to arrange rect. To avoid this behavior, we have to
			// perform Measure again on the item before we arrange it with smaller size. This all happens
			// from within ArrangeOverride call of the panel and to prevent recursive invalidation of the
			// panel's Measure, skip calling base OnChildDesiredSizeChanged while we are re-measuring
			// a child element from within ArrangeOverride.
			// 
			if ( _skipOnChildDesiredSizeChanged )
				return;

			base.OnChildDesiredSizeChanged( child );
		}

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

		#region Private Methods

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
					items.Add( new LayoutItem( elem ), new Constraint( elem, this ) );
				}

				return true;
			}

			return false;
		}

		#endregion // VerifyLayoutManagerItems

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // GridBagPanel Class
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