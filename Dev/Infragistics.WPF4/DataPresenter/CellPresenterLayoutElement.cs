using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	#region CellPresenterLayoutElementBase class

	/// <summary>
	/// Base class for an element used in the visual tree of a cell to arrange label and the cell content.
	/// </summary>
	/// <seealso cref="CellPresenterLayoutElement"/>
	/// <seealso cref="SummaryCellPresenterLayoutElement"/>
	/// <seealso cref="CellValuePresenter"/>
	/// <seealso cref="LabelPresenter"/>
	// AS 4/15/10 Moved here from CellPresenterLayoutElement so that SummaryCellPresenterLayoutElement is hidden as well.
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public abstract class CellPresenterLayoutElementBase : FrameworkElement
	{
		#region Private Members

		private FrameworkElement _cellValuePresenter;
		private FrameworkElement _labelPresenter;
		private Field _cachedField;
		private Record _cachedRecord;
        
        // JJD 10/31/08 - TFS6094/BR33963
        // Cache the layout version so we can see if it has changed. If it has we need to
        // dump the old CellValuePresenters and LabelPresenters and recreate new ones.
        private int _layoutVersion;

        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
        private bool _isMeasuring;
        private Rect _measureCellRect;
        private Rect _measureLabelRect;

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        // Cache a reference to the containing CellPresenter rather then getting it every time.
        //
        private CellPresenterBase _cellPresenter;

		#endregion //Private Members

		#region Constructors

		static CellPresenterLayoutElementBase( )
		{
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( CellPresenterLayoutElementBase ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CellPresenterLayoutElementBase"/> class
		/// </summary>
		public CellPresenterLayoutElementBase( )
		{
		}

		#endregion //Constructors

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride( Size finalSize )
		{
            
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)


			#region Refactored
			
#region Infragistics Source Cleanup (Region)








































































































































































#endregion // Infragistics Source Cleanup (Region)

			#endregion

            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

            CellPresenterBase cp = this.CellPresenter;

            Debug.Assert(null != cp, "Cannot access the CellPresenterBase");
            Rect cellRect = null != cp ? cp.CellRect : Rect.Empty; 
            Rect labelRect = null != cp ? cp.LabelRect : Rect.Empty;
            
            if (cellRect.IsEmpty)
                cellRect = this._measureCellRect;
            if (labelRect.IsEmpty)
                labelRect = this._measureLabelRect;

            Rect arrangeRect = new Rect(finalSize);

            for (int i = 0; i < 2; i++)
            {
                bool isLabel = i == 0;
                FrameworkElement element = isLabel ? this._labelPresenter : this._cellValuePresenter;

                if (null != element)
                {
                    Rect rect = isLabel ? labelRect : cellRect;

                    this.ArrangeHelper(element, rect);
                    arrangeRect.Union(rect);
                }
            }

            return arrangeRect.Size;
        }

		#endregion //ArrangeOverride

		#region GetVisualChild

		/// <summary>
		/// Gets the visual child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child visual.</param>
		/// <returns>The visual child at the specified index.</returns>
		protected override Visual GetVisualChild( int index )
		{
			if ( index == 0 )
			{
				if ( this._cellValuePresenter != null )
					return this._cellValuePresenter;

				return this._labelPresenter;
			}

			if ( index == 1 )
			{
				return this._labelPresenter;
			}

			throw new IndexOutOfRangeException( );
		}

		#endregion //GetVisualChild

		    // JJD 5/30/07 return LogicalChildren enumerator
		    #region LogicalChildren
		/// <summary>
		/// Gets an enumerator for logical child elements in this panel.
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				FrameworkElement[] array = new FrameworkElement[this.VisualChildrenCount];

				for ( int i = 0; i < this.VisualChildrenCount; i++ )
					array[i] = this.GetVisualChild( i ) as FrameworkElement;

				return array.GetEnumerator( );
			}
		}
		    #endregion //LogicalChildren

		    #region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride( Size availableSize )
		{
			// AS 10/9/09 NA 2010.1 - CardView
			// In a card the size of the cellpresenter may not change but if any of 
			// the constraints change then we still want to invalidate the measure/arrange 
			// so we need to hook into the fieldlayout's notifications.
			//
			Field oldField = _cachedField;

			this.GetFieldAndRecordForCaching( out _cachedField, out _cachedRecord );

			// AS 10/9/09 NA 2010.1 - CardView
			if (_cachedField != oldField)
			{
				FieldLayout fl = _cachedField != null ? _cachedField.Owner : null;

				if (null != fl)
					this.SetBinding(GridColumnWidthVersionProperty, Utilities.CreateBindingObject(FieldLayout.GridColumnWidthVersionProperty, BindingMode.OneWay, fl));
			}

			Size sizeRequired = new Size( );

			Field field = _cachedField;
			Record record = _cachedRecord;

			if ( field != null && record != null )
			{
                // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                this.VerifyChildElementsImpl();

                #region OldLayout Code
                
#region Infragistics Source Cleanup (Region)
































































































































































































#endregion // Infragistics Source Cleanup (Region)

                #endregion //OldLayout Code
                // we need to use a layoutmanager that uses our constraints
                // and then cache the rects to use in case we're not in a 
                // 
				// JJD 08/16/10 - TFS26331 - pass availableSize as the constraining size
				//CPGridBagLayoutManager lm = field.GetCPLayoutManager();
                CPGridBagLayoutManager lm = field.GetCPLayoutManager(availableSize);

                // if this element is within the tool window then we want to use the 
                // size of the cell being dragged. if we don't do this then we end up
                // returning the desired size of the element which may be larger
                // than what the element actually is
                Rect preferredCellRect, preferredLabelRect;

				// JM 01-25-09 Wrap references to ToolWindow in #if

                DragToolWindow tw = ToolWindow.GetToolWindow(this) as DragToolWindow;
                CellPresenterBase cp = this.CellPresenter;

                // JJD 5/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                // Let the cellpresenter know that the child elements have been created
                if (cp != null)
                    cp.SynchronizeChildElements();

                if (null != tw && null != cp)
                {
                    preferredCellRect = cp.CellRect;
                    preferredLabelRect = cp.LabelRect;
                }
                else

				preferredLabelRect = preferredCellRect = Rect.Empty;

                lm.Initialize(this, new Rect(availableSize), preferredCellRect, preferredLabelRect);

                this._measureCellRect = lm.CellRect;
                this._measureLabelRect = lm.LabelRect;

                object rectContext = new Rect(availableSize);

                Size preferredSize = lm.CalculatePreferredSize(CalcSizeLayoutContainer.Instance, rectContext);
                Size minSize = lm.CalculateMinimumSize(CalcSizeLayoutContainer.Instance, rectContext);
                Size maxSize = lm.CalculateMaximumSize(CalcSizeLayoutContainer.Instance, rectContext);

                // then return that as our desired size
                if (double.IsPositiveInfinity(availableSize.Width) == false &&
                    availableSize.Width > preferredSize.Width)
                {
					preferredSize.Width = Math.Min(availableSize.Width, maxSize.Width);
                }

                if (double.IsPositiveInfinity(availableSize.Height) == false &&
                    availableSize.Height > preferredSize.Height)
                {
					preferredSize.Height = Math.Min(availableSize.Height, maxSize.Height);
                }

                // ensure its within the available and min range
                if (double.IsPositiveInfinity(availableSize.Width) == false &&
                    GridUtilities.AreClose(preferredSize.Width, availableSize.Width) == false &&
                    preferredSize.Width > availableSize.Width)
                {
                    preferredSize.Width = Math.Max(minSize.Width, availableSize.Width);
                }

                if (double.IsPositiveInfinity(availableSize.Height) == false &&
                    GridUtilities.AreClose(preferredSize.Height, availableSize.Height) == false &&
                    preferredSize.Height > availableSize.Height)
                {
                    preferredSize.Height = Math.Max(minSize.Height, availableSize.Height);
                }

                field.ReleaseCPLayoutManager(lm);
                sizeRequired = preferredSize;
            }

			sizeRequired.Width = Math.Max( sizeRequired.Width, 1 );
			sizeRequired.Height = Math.Max( sizeRequired.Height, 1 );

			return sizeRequired;
		}

		#endregion //MeasureOverride

        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
        #region OnChildDesiredSizeChanged
        /// <summary>
        /// Overriden. Invoked when the <see cref="UIElement.DesiredSize"/> for a child element has changed.
        /// </summary>
        /// <param name="child">The child element whose size has changed.</param>
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            if (this._isMeasuring)
                return;

            base.OnChildDesiredSizeChanged(child);
        }
        #endregion //OnChildDesiredSizeChanged

		#region OnPropertyChanged

        // AS 3/11/09 Optimization
        private static DependencyProperty WindowServiceProperty;

		/// <summary>
		/// Called whena dependency property has been changed
		/// </summary>
		/// <param name="e">The event arguments identifying the property plus the new and old value.</param>
		protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnPropertyChanged( e );

            // AS 3/11/09 Optimization
            if (WindowServiceProperty == null && e.Property.Name == "IWindowService")
                WindowServiceProperty = e.Property;
 
            // AS 3/11/09 Optimization
			//switch (e.Property.Name)
			//{
			//	case "IWindowService":
            if (e.Property == WindowServiceProperty)
            {
					if (e.NewValue == null)
					{
                        // JJD 10/31/08 - TFS6094/BR33963
                        // Make sure the layout version hasn't changed. If it has we need to
                        // dump the old CellValuePresenters and LabelPresenters so they will
                        // never be re-used.
                        this.VerifyLayoutVersion();

                        // JJD 10/31/08 - TFS6094/BR33963
                        // pass true into the Release... methods so the elements get
                        // inserted back into the cache for recycling
                        //this.ReleaseCellValuePresenter();
						//this.ReleaseLabelPresenter();
                        this.ReleaseCellValuePresenter(true);
						this.ReleaseLabelPresenter(true);
					}
					else
					{
						// AS 8/27/09 TFS21526
						// We removed the CVP/LP when we were unparented but if we get reparented
						// we need to make sure the measure is invalidated so we have the opportunity 
						// to recreated them as needed.
						//
						this.InvalidateMeasure();
					}
					//break;
			}
		}

		#endregion //OnPropertyChanged

		#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString( )
		{
			StringBuilder sb = new StringBuilder( );

			sb.Append( "CellPresenterLayoutElement: " );

			Field field = this.Field;
			if ( field != null )
			{
				sb.Append( field.ToString( ) );
				sb.Append( ", " );
			}

			return sb.ToString( );
		}

		#endregion //ToString

		#region VisualChildrenCount

		/// <summary>
		/// Returns the total numder of visual children (read-only).
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				int count = 0;

				if ( this._cellValuePresenter != null )
					count++;

				if ( this._labelPresenter != null )
					count++;

				return count;
			}
		}

		#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Properties

		#region Internal Properties

		#region CellContentAlignment

		// SSP 5/22/08 - BR33108 - Summaries Functionality
		// 
		internal CellContentAlignment CellContentAlignment
		{
			get
			{
				if ( null != _cachedField )
				{
					if ( _cachedField.Owner.HasSeparateHeader )
						return CellContentAlignment.ValueOnly;

					return _cachedField.CellContentAlignmentResolved;
				}

				return CellContentAlignment.Default;
			}
		}

		#endregion // CellContentAlignment

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        #region CellPresenter
        internal CellPresenterBase CellPresenter
        {
            get
            {
                if (null == _cellPresenter)
                {
                    // JJD 5/19/10 - Optimization - Use TemplatedParent instead 
                    //_cellPresenter = Utilities.GetAncestorFromType(this, typeof(CellPresenterBase), true, this.DataPresenter, typeof(RecordPresenter)) as CellPresenterBase;
                    _cellPresenter = this.TemplatedParent as CellPresenterBase;
                }

                return _cellPresenter;
            }
        } 
        #endregion //CellPresenter

		#region CellValuePresenter

		internal FrameworkElement CellValuePresenter { get { return this._cellValuePresenter; } }

		#endregion //CellValuePresenter

		#region DataPresenterBase

		internal DataPresenterBase DataPresenter
		{
			get
			{
				return null != _cachedField ? _cachedField.Owner.DataPresenter : null;
			}
		}

		#endregion //DataPresenterBase

		#region Field

		/// <summary>
		/// Returns the associated field.
		/// </summary>
		internal Field Field
		{
			get
			{
				return _cachedField;
			}
		}

		#endregion // Field

		#region LabelPresenter

		internal FrameworkElement LabelPresenter { get { return this._labelPresenter; } }

		#endregion //LabelPresenter

		#region Record

		/// <summary>
		/// Returns the associated record.
		/// </summary>
		internal Record Record
		{
			get
			{
				return _cachedRecord;
			}
		}

		#endregion // Record		

		#endregion //Internal Properties

		#region Private Properties

		// AS 10/9/09 NA 2010.1 - CardView
		#region GridColumnWidthVersion

		private static readonly DependencyProperty GridColumnWidthVersionProperty = FieldLayout.GridColumnWidthVersionProperty.AddOwner(typeof(CellPresenterLayoutElementBase),
			// AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
			// Optimization - instead of overriding OnPropertyChanged.
			//
			new FrameworkPropertyMetadata(new PropertyChangedCallback(OnGridColumnWidthVersionChanged)));

		private static void OnGridColumnWidthVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CellPresenterLayoutElementBase cp = (CellPresenterLayoutElementBase)d;

			if (null != cp._cachedField)
				cp.InvalidateMeasure();
		}
		#endregion //GridColumnWidthVersion

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
        #region ArrangeHelper
        private void ArrangeHelper(FrameworkElement child, Rect rect)
        {
            bool wasVerifying = this._isMeasuring;

            this._isMeasuring = true;

            try
            {
				// JJD 08/16/10 - TFS26331
                //child.Measure(rect.Size);
				Size size = rect.Size;

				// AS 3/15/11 TFS65372
				// This part of the fix for TFS26331 doesn't seem to be needed 
				// anymore (maybe because of some changes I made in 10.1 for 
				// supporting cards or maybe because of something else) but it 
				// is causing this issue because now the child is measured with 
				// infinity and using the natural size of the image.
				//
				//if (size.Height <= child.DesiredSize.Height)
				//	size.Height = double.PositiveInfinity;

                child.Measure(size);
                child.Arrange(rect);
            }
            finally
            {
                this._isMeasuring = wasVerifying;
            }
        }

        #endregion //ArrangeHelper

	    // AS 12/18/07 BR25223
	    // I need to be able to get to these sizes to find out the size of a cellvaluepresenter/labelpresenter
	    // based on the size that we would use for a cell value presenter.
	    //
	    #region CalculateRects
		internal static void CalculateRects(Size cellPresenterSize, Size labelDesiredSize, Size cellDesiredSize,
	            CellContentAlignment contentAlignment, out Rect labelRect, out Rect cellRect)
		{
			HorizontalAlignment hAlign = HorizontalAlignment.Left;
			VerticalAlignment vAlign = VerticalAlignment.Top;

			bool isLabelAbove = false;
			bool isLabelBelow = false;
			bool isLabelLeft = false;
			bool isLabelRight = false;

			#region ContentAlignment
			switch ( contentAlignment )
			{
				case CellContentAlignment.LabelAboveValueAlignCenter:
					isLabelAbove = true;
					hAlign = HorizontalAlignment.Center;
					vAlign = VerticalAlignment.Stretch;
					break;
				case CellContentAlignment.LabelAboveValueAlignLeft:
					isLabelAbove = true;
					hAlign = HorizontalAlignment.Left;
					vAlign = VerticalAlignment.Stretch;
					break;
				case CellContentAlignment.LabelAboveValueAlignRight:
					isLabelAbove = true;
					hAlign = HorizontalAlignment.Right;
					vAlign = VerticalAlignment.Stretch;
					break;
				case CellContentAlignment.LabelAboveValueStretch:
					isLabelAbove = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Stretch;
					break;
				case CellContentAlignment.LabelBelowValueAlignCenter:
					isLabelBelow = true;
					hAlign = HorizontalAlignment.Center;
					vAlign = VerticalAlignment.Stretch;
					break;
				case CellContentAlignment.LabelBelowValueAlignLeft:
					isLabelBelow = true;
					hAlign = HorizontalAlignment.Left;
					vAlign = VerticalAlignment.Stretch;
					break;
				case CellContentAlignment.LabelBelowValueAlignRight:
					isLabelBelow = true;
					hAlign = HorizontalAlignment.Right;
					vAlign = VerticalAlignment.Stretch;
					break;
				case CellContentAlignment.LabelBelowValueStretch:
					isLabelBelow = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Stretch;
					break;
				case CellContentAlignment.LabelLeftOfValueAlignBottom:
					isLabelLeft = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Bottom;
					break;
				case CellContentAlignment.LabelLeftOfValueAlignMiddle:
					isLabelLeft = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Center;
					break;
				case CellContentAlignment.LabelLeftOfValueAlignTop:
					isLabelLeft = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Top;
					break;
				case CellContentAlignment.LabelLeftOfValueStretch:
					isLabelLeft = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Stretch;
					break;
				case CellContentAlignment.LabelRightOfValueAlignBottom:
					isLabelRight = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Bottom;
					break;
				case CellContentAlignment.LabelRightOfValueAlignMiddle:
					isLabelRight = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Center;
					break;
				case CellContentAlignment.LabelRightOfValueAlignTop:
					isLabelRight = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Top;
					break;
				case CellContentAlignment.LabelRightOfValueStretch:
					isLabelRight = true;
					hAlign = HorizontalAlignment.Stretch;
					vAlign = VerticalAlignment.Stretch;
					break;
				default:
					Debug.Fail( "Invalid alignment in CellValueAndLabelElement ArrangeOverride" );
					break;
			}
			#endregion

			labelRect = new Rect( labelDesiredSize );
			cellRect = new Rect( cellDesiredSize );

			if ( isLabelAbove )
			{
				cellRect.Y = labelRect.Height;
			}
			else if ( isLabelBelow )
			{
				labelRect.Y = Math.Max( cellPresenterSize.Height - labelRect.Height, 0 );
			}
			else if ( isLabelLeft )
			{
				cellRect.X = labelRect.Width;
			}
			else if ( isLabelRight )
			{
				labelRect.X = Math.Max( cellPresenterSize.Width - labelRect.Width, 0 );
			}

			switch ( hAlign )
			{
				case HorizontalAlignment.Stretch:
					if ( isLabelAbove || isLabelBelow )
					{
						cellRect.Width = cellPresenterSize.Width;
						labelRect.Width = cellPresenterSize.Width;
					}
					else
					{
						if ( isLabelLeft )
							cellRect.Width = Math.Max( cellPresenterSize.Width - cellRect.X, 0 );
						else
							labelRect.Width = Math.Max( cellPresenterSize.Width - labelRect.X, 0 );
					}
					break;
				case HorizontalAlignment.Right:
					cellRect.X = Math.Max( cellPresenterSize.Width - cellRect.Width, 0 );
					labelRect.X = Math.Max( cellPresenterSize.Width - labelRect.Width, 0 );
					break;
				case HorizontalAlignment.Center:
					cellRect.X = Math.Max( ( cellPresenterSize.Width - cellRect.Width ) / 2, 0 );
					labelRect.X = Math.Max( ( cellPresenterSize.Width - labelRect.Width ) / 2, 0 );
					break;
			}

			switch ( vAlign )
			{
				case VerticalAlignment.Stretch:
					if ( isLabelLeft || isLabelRight )
					{
						cellRect.Height = cellPresenterSize.Height;
						labelRect.Height = cellPresenterSize.Height;
					}
					else
					{
						if ( isLabelAbove )
							cellRect.Height = Math.Max( cellPresenterSize.Height - cellRect.Y, 0 );
						else
							labelRect.Height = Math.Max( cellPresenterSize.Height - labelRect.Y, 0 );
					}
					break;
				case VerticalAlignment.Bottom:
					cellRect.Y = Math.Max( cellPresenterSize.Height - cellRect.Height, 0 );
					labelRect.Y = Math.Max( cellPresenterSize.Height - labelRect.Height, 0 );
					break;
				case VerticalAlignment.Center:
					cellRect.Y = Math.Max( ( cellPresenterSize.Height - cellRect.Height ) / 2, 0 );
					labelRect.Y = Math.Max( ( cellPresenterSize.Height - labelRect.Height ) / 2, 0 );
					break;
			}
		}
		#endregion //CalculateRects

		#region CreateCellValuePresenter

		internal abstract FrameworkElement CreateCellValuePresenter( FrameworkElement oldElem );

		#endregion // CreateCellValuePresenter

		#region CreateLabelPresenter

		internal abstract FrameworkElement CreateLabelPresenter( FrameworkElement oldElem );

		#endregion // CreateLabelPresenter

		#region GetExtent

        
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		#endregion // GetExtent

		#region GetFieldAndRecordForCaching

		/// <summary>
		/// This method should get the field and the record based on the ancestor element hierarchy.
		/// For example, data record cell would get it from the parent CellPresenter where as a
		/// summary record cell would get it from parent SummaryCellPresenter.
		/// </summary>
		/// <param name="field">This will be set to the associated field</param>
		/// <param name="record">This will be set to associated record</param>
		internal abstract void GetFieldAndRecordForCaching( out Field field, out Record record );

		#endregion // GetFieldAndRecordForCaching

		// JM 11-15-11 TFS95927 - Added.
		#region InitializeCellValuePresenter

		internal abstract void InitializeCellValuePresenter(FrameworkElement cvp);

		#endregion // InitializeCellValuePresenter

		#region ReleaseCellValuePresenter

		// JJD 10/31/08 - TFS6094/BR33963 - added
        // added recycle param
        //		private void ReleaseCellValuePresenter()
        internal void ReleaseCellValuePresenter(bool recycle)
        {
            if (this._cellValuePresenter != null)
            {
                // JJD 10/31/08 - TFS6094/BR33963
                // null out the _cellValuePresenter member first since this is
                // used to support GetVisualChild

                //this.ReleaseCellValuePresenter(_cellValuePresenter);
                //_cellValuePresenter = null;
                FrameworkElement cvpOld = this._cellValuePresenter;
                this._cellValuePresenter = null;

                // AS 3/24/09 TFS15816
                // The main issue was that we were passing off the _cellValuePresenter 
                // member which we just nulled out so the callee couldn't remove 
                // it from the logical/visual tree. We need to pass off the local 
                // cvpOld. That being said since this element adds the element as 
                // a logical/visual child, it should be the one to remove it 
                // before calling the virtual.
                //
                //this.ReleaseCellValuePresenter(_cellValuePresenter, recycle);
                this.RemoveLogicalChild(cvpOld);
                this.RemoveVisualChild(cvpOld);

                this.ReleaseCellValuePresenter(cvpOld, recycle);
            }
        }
        // JJD 10/31/08 - TFS6094/BR33963 - added
        // added recycle param
        //internal abstract void ReleaseCellValuePresenter(FrameworkElement cellValuePresenter);
        internal abstract void ReleaseCellValuePresenter(FrameworkElement cellValuePresenter, bool recycle);


        #endregion //ReleaseCellValuePresenter

        #region ReleaseLabelPresenter

        // JJD 10/31/08 - TFS6094/BR33963 - added
        // added recycle param
        //private void ReleaseLabelPresenter()
        internal void ReleaseLabelPresenter(bool recycle)
        {
            if (this._labelPresenter != null)
            {
                // JJD 10/31/08 - TFS6094/BR33963
                // null out the _cellValuePresenter member first since this is
                // used to support GetVisualChild
                //this.ReleaseLabelPresenter(_labelPresenter);
                //_labelPresenter = null;
                FrameworkElement lpOld = this._labelPresenter;
                this._labelPresenter = null;

                // AS 3/24/09 TFS15816
                // See ReleaseCellValuePresenter(bool) for details.
                //
                this.RemoveLogicalChild(lpOld);
                this.RemoveVisualChild(lpOld);

                this.ReleaseLabelPresenter(lpOld, recycle);
            }
        }

        // JJD 10/31/08 - TFS6094/BR33963 - added
        // added recycle param
        internal abstract void ReleaseLabelPresenter(FrameworkElement labelPresenter, bool recycle);

        #endregion //ReleaseLabelPresenter

        // AS 1/27/09 NA 2009 Vol 1 - Fixed Fields
        // Refactored the start of the measure override into a helper method
        // so we can ensure that the elements have been created.
        //
        #region VerifyChildElements
        internal void VerifyChildElements()
        {
            this.GetFieldAndRecordForCaching(out _cachedField, out _cachedRecord);

            Field field = _cachedField;
            Record record = _cachedRecord;

            if (field == null || record == null)
                return;

            this.VerifyChildElementsImpl();
        }

        private void VerifyChildElementsImpl()
        {
            // SSP 5/22/08 - BR33108 - Summaries Functionality
            // 
            //CellContentAlignment contentAlignment = field.CellContentAlignmentResolved;
            CellContentAlignment contentAlignment = this.CellContentAlignment;

            // JJD 10/31/08 - TFS6094/BR33963
            // Make sure the layout version hasn't changed. If it has we need to
            // dump the old CellValuePresenters and LabelPresenters and recreate
            // them below.
            this.VerifyLayoutVersion();

            #region InitializeChildElements

            // initialize cellvaluepresenter
            if (contentAlignment == CellContentAlignment.LabelOnly)
            {
                // JJD 10/31/08 - TFS6094/BR33963
                // pass true into the Release... methods so the elements get
                // inserted back into the cache for recycling
                //this.ReleaseCellValuePresenter();
                this.ReleaseCellValuePresenter(true);
            }
            else
            {
                FrameworkElement newCVP = this.CreateCellValuePresenter(_cellValuePresenter);
                if (newCVP != _cellValuePresenter)
                {
                    if (null != _cellValuePresenter)
                    {
                        // JJD 10/31/08 - TFS6094/BR33963
                        // pass true into the Release... methods so the elements get
                        // inserted back into the cache for recycling
                        //this.ReleaseCellValuePresenter(_cellValuePresenter);
                        this.ReleaseCellValuePresenter(_cellValuePresenter, true);
                    }

                    _cellValuePresenter = newCVP;

                    this.AddLogicalChild(this._cellValuePresenter);
                    this.AddVisualChild(this._cellValuePresenter);

					// JM 11-15-11 TFS95927
					this.InitializeCellValuePresenter(this._cellValuePresenter);
                }
            }

            // initialize labelpresenter
            if (contentAlignment == CellContentAlignment.ValueOnly)
            {
                // JJD 10/31/08 - TFS6094/BR33963
                // pass true into the Release... methods so the elements get
                // inserted back into the cache for recycling
                //this.ReleaseLabelPresenter();
                this.ReleaseLabelPresenter(true);
            }
            else
            {
                FrameworkElement newLP = this.CreateLabelPresenter(_labelPresenter);
                if (newLP != _labelPresenter)
                {
                    if (null != _labelPresenter)
                    {
                        // JJD 10/31/08 - TFS6094/BR33963
                        // pass true into the Release... methods so the elements get
                        // inserted back into the cache for recycling
                        //this.ReleaseLabelPresenter();
                        this.ReleaseLabelPresenter(true);
                    }

                    _labelPresenter = newLP;

                    this.AddLogicalChild(this._labelPresenter);
                    this.AddVisualChild(this._labelPresenter);
                }
            }

            #endregion //InitializeChildElements
        }
        #endregion //VerifyChildElements

        // JJD 10/31/08 - TFS6094/BR33963 - added
        #region VerifyLayoutVersion

        private void VerifyLayoutVersion()
        {
            FieldLayout fl = this._cachedField.Owner;

            if (fl != null)
            {
                // JJD 10/31/08 - TFS6094/BR33963
                // Cache the layout version so we can see if it has changed. If it has we need to
                // dump the old CellValuePresenters and LabelPresenters and recreate new ones.
                int layoutVersion = fl.InternalVersion;

                if (layoutVersion != this._layoutVersion)
                {
                    this._layoutVersion = layoutVersion;

                    // JJD 10/31/08 - TFS6094/BR33963
                    // pass false into the Release... methods so the elements don't get
                    // inserted back into the cache for recycling
                    this.ReleaseCellValuePresenter(false);
                    this.ReleaseLabelPresenter(false);
                }
            }
        }

        #endregion //VerifyLayoutVersion

		#endregion // Internal Methods

		#endregion //Methods
	}

	#endregion // CellPresenterLayoutElementBase class

	#region CellPresenterLayoutElement Class

	/// <summary>
	/// An element used in the visual tree of the CellPresenter control to arrange a CellValuePresenter element and/or a LabelPresenter element based on the LabelLocation in effect for the cell.
	/// </summary>
	/// <seealso cref="CellPresenter"/>
	/// <seealso cref="CellValuePresenter"/>
	/// <seealso cref="LabelPresenter"/>
	//[Description("An element used in the visual tree of the CellPresenter control to arrange a CellValuePresenter element and/or a LabelPresenter element based on the LabelLocation in effect for the cell.")]
	public sealed class CellPresenterLayoutElement : CellPresenterLayoutElementBase
	{
		#region Private Vars

		private CellPresenter _cachedParentCellPresenter;

		#endregion // Private Vars

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CellPresenterLayoutElement"/> class
		/// </summary>
		public CellPresenterLayoutElement( )
		{
		}

		#endregion //Constructors

		#region Base Overrides

		#region Overridden Methods

		#region CreateCellValuePresenter

		internal override FrameworkElement CreateCellValuePresenter( FrameworkElement oldElem )
		{
			CellValuePresenter cvp = oldElem as CellValuePresenter;

            // JJD 12/23/08 - added support for FilterCellValuePresenters
            bool isFilterRecord = this.Record is FilterRecord;

            // don't try to reuse CellValuePresenters for filterrecords
            if (isFilterRecord && !(cvp is FilterCellValuePresenter))
                cvp = null;

			Field field = this.Field;
			if ( null == cvp || cvp.Field != field )
			{
                if (null == cvp)
                {
                    // JJD 12/23/08 - added support for FilterCellValuePresenters
                    if (isFilterRecord)
                        cvp = new FilterCellValuePresenter();
                    else
                        cvp = new CellValuePresenter();
                }

				// JM 11-15-11 TFS95927
				//cvp.Field = field;
			}

			return cvp;
		}

		#endregion // CreateCellValuePresenter

		#region CreateLabelPresenter

		internal static LabelPresenter CreateLabelPresenter( FrameworkElement oldElem, Field field )
		{
			LabelPresenter lp = oldElem as LabelPresenter;

			if ( null == lp || lp.Field != field )
			{
				lp = new LabelPresenter( );
				lp.Field = field;
			}

			return lp;
		}

		internal override FrameworkElement CreateLabelPresenter( FrameworkElement oldElem )
		{
			return CreateLabelPresenter( oldElem, this.Field );
		}

		#endregion // CreateLabelPresenter

        #region GetExtent

        
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

        #endregion // GetExtent

		#region GetFieldAndRecordForCaching

		/// <summary>
		/// This method should get the field and the record based on the ancestor element hierarchy.
		/// For example, data record cell would get it from the parent CellPresenter where as a
		/// summary record cell would get it from parent SummaryCellPresenter.
		/// </summary>
		/// <param name="field">This will be set to the associated field</param>
		/// <param name="record">This will be set to associated record</param>
		internal override void GetFieldAndRecordForCaching( out Field field, out Record record )
		{
			if ( null == _cachedParentCellPresenter )
			{
				_cachedParentCellPresenter = Utilities.GetAncestorFromType( this, typeof( CellPresenter ), false, null, typeof( RecordListControl ) ) as CellPresenter;

				Debug.Assert( null != _cachedParentCellPresenter );
			}

			field = null != _cachedParentCellPresenter ? _cachedParentCellPresenter.Field : null;
			record = this.DataContext as Record;

			
			// Throwing asserts causes issues with blend. So only do so when not in design mode.
			
			




		}

		#endregion // GetFieldAndRecordForCaching

		// JM 11-15-11 TFS95927 - Added.
		#region InitializeCellValuePresenter

		internal override void InitializeCellValuePresenter(FrameworkElement cvp)
		{
			Debug.Assert(cvp is CellValuePresenter, "Expecting CellValuePresenter type!");
			((CellValuePresenter)cvp).Field = this.Field;
		}

		#endregion // InitializeCellValuePresenter

		#region ReleaseCellValuePresenter

        // JJD 10/31/08 - TFS6094/BR33963 - added
        // added recycle param
        //internal override void ReleaseCellValuePresenter( FrameworkElement cellValuePresenter )
		internal override void ReleaseCellValuePresenter( FrameworkElement cellValuePresenter, bool recycle )
		{
            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


            // JJD 10/31/08 - TFS6094/BR33963
            // Only release the element back into the recycle cache if 
            // the recycle param is true
            if (recycle)
            {
			    CellValuePresenter cvp = cellValuePresenter as CellValuePresenter;
			    Field field = this.Field;
                if (null != field && null != cvp)
                    field.ReleaseCellValuePresenter(cvp);
            }
		}

		#endregion //ReleaseCellValuePresenter

		#region ReleaseLabelPresenter

		internal static void ReleaseLabelPresenter( FrameworkElement labelPresenter, Field field )
		{
			LabelPresenter lp = labelPresenter as LabelPresenter;

			if ( null != field && null != lp )
				field.ReleaseLabelPresenter( lp );
		}

        // JJD 10/31/08 - TFS6094/BR33963 - added
        // added recycle param
        //internal override void ReleaseLabelPresenter(FrameworkElement labelPresenter)
        internal override void ReleaseLabelPresenter(FrameworkElement labelPresenter, bool recycle)
		{
            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


            // JJD 10/31/08 - TFS6094/BR33963
            // Only release the element back into the recycle cache if 
            // the recycle param is true
            if (recycle)
    			ReleaseLabelPresenter( labelPresenter, this.Field );
		}

		#endregion //ReleaseLabelPresenter

		#endregion // Overridden Methods

		#endregion // Base Overrides

		#region Properties

		#region CellValuePresenter

		internal new CellValuePresenter CellValuePresenter { get { return base.CellValuePresenter as CellValuePresenter; } }

		#endregion //CellValuePresenter

		#region LabelPresenter

		internal new LabelPresenter LabelPresenter { get { return base.LabelPresenter as LabelPresenter; } }

		#endregion //LabelPresenter

		#endregion // Properties
	}

	#endregion // CellPresenterLayoutElement Class

	#region SummaryCellPresenterLayoutElement Class

	/// <summary>
	/// An element used in the visual tree of the CellPresenter control to arrange a CellValuePresenter element and/or a LabelPresenter element based on the LabelLocation in effect for the cell.
	/// </summary>
	/// <seealso cref="CellPresenter"/>
	/// <seealso cref="CellValuePresenter"/>
	/// <seealso cref="LabelPresenter"/>
	//[Description("An element used in the visual tree of the CellPresenter control to arrange a CellValuePresenter element and/or a LabelPresenter element based on the LabelLocation in effect for the cell.")]
	public sealed class SummaryCellPresenterLayoutElement : CellPresenterLayoutElementBase
	{
		#region Private Vars

		private SummaryCellPresenter _cachedParentCellPresenter;

		#endregion // Private Vars

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryCellPresenterLayoutElement"/> class
		/// </summary>
		public SummaryCellPresenterLayoutElement( )
		{
		}

		#endregion //Constructors

		#region Base Overrides

		#region Overridden Methods

		#region CreateCellValuePresenter

		internal override FrameworkElement CreateCellValuePresenter( FrameworkElement oldElem )
		{
			SummaryResultsPresenter resultsPresenter = oldElem as SummaryResultsPresenter;

			if ( null == resultsPresenter )
				resultsPresenter = new SummaryResultsPresenter( );

			// JM 11015011 TFS95927 - Moved the following logic to the new InitializeCellVauePresenter abstract method.
			//SummaryRecord summaryRecord = this.Record as SummaryRecord;
			//Debug.Assert( null != summaryRecord );
			//resultsPresenter.SummaryResults = null == summaryRecord ? null 
			//    : summaryRecord.GetFixedSummaryResults( this.Field, true );

			return resultsPresenter;
		}

		#endregion // CreateCellValuePresenter

		#region CreateLabelPresenter

		internal override FrameworkElement CreateLabelPresenter( FrameworkElement oldElem )
		{
			return CellPresenterLayoutElement.CreateLabelPresenter( oldElem, this.Field );
		}

		#endregion // CreateLabelPresenter

		#region GetExtent

        
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		#endregion // GetExtent

		#region GetFieldAndRecordForCaching

		/// <summary>
		/// This method should get the field and the record based on the ancestor element hierarchy.
		/// For example, data record cell would get it from the parent CellPresenter where as a
		/// summary record cell would get it from parent SummaryCellPresenter.
		/// </summary>
		/// <param name="field">This will be set to the associated field</param>
		/// <param name="record">This will be set to associated record</param>
		internal override void GetFieldAndRecordForCaching( out Field field, out Record record )
		{
			if ( null == _cachedParentCellPresenter )
			{
				_cachedParentCellPresenter = Utilities.GetAncestorFromType( this, typeof( SummaryCellPresenter ), false, null, typeof( RecordListControl ) ) as SummaryCellPresenter;

				Debug.Assert( null != _cachedParentCellPresenter );
			}

			field = null != _cachedParentCellPresenter ? _cachedParentCellPresenter.Field : null;
			record = this.DataContext as Record;

			
			// Throwing asserts causes issues with blend. So only do so when not in design mode.
			
			




		}

		#endregion // GetFieldAndRecordForCaching

		// JM 11015011 TFS95927 - Added.
		#region InitializeCellValuePresenter

		internal override void InitializeCellValuePresenter(FrameworkElement cvp)
		{
			SummaryResultsPresenter resultsPresenter = cvp as SummaryResultsPresenter;
			if (resultsPresenter != null)
			{
				// SSP 4/10/12 TFS108549 - Optimizations
				// Now that we do this in the SummaryResultsPresenter whenever its DataContext changes, we don't need
				// to do it here. We do however need to set the Field property since that new logic relies on it 
				// being set.
				// 
				
				resultsPresenter.SetValue( DataItemPresenter.FieldProperty, this.Field );
				
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				
			}
		}

		#endregion // InitializeCellValuePresenter

		#region ReleaseCellValuePresenter

        // JJD 10/31/08 - TFS6094/BR33963 - added
        // added recycle param
        //internal override void ReleaseCellValuePresenter( FrameworkElement cellValuePresenter )
		internal override void ReleaseCellValuePresenter( FrameworkElement cellValuePresenter, bool recycle)
		{
            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        }

		#endregion //ReleaseCellValuePresenter

		#region ReleaseLabelPresenter


        // JJD 10/31/08 - TFS6094/BR33963 - added
        // added recycle param
		//internal override void ReleaseLabelPresenter( FrameworkElement labelPresenter )
		internal override void ReleaseLabelPresenter( FrameworkElement labelPresenter, bool recycle )
		{
            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


            // JJD 10/31/08 - TFS6094/BR33963
            // Only release the element back into the recycle cache if 
            // the recycle param is true
            if (recycle)
                CellPresenterLayoutElement.ReleaseLabelPresenter(labelPresenter, this.Field);
		}

		#endregion //ReleaseLabelPresenter

		#endregion // Overridden Methods

		#endregion // Base Overrides

		#region Properties

		#endregion // Properties
	}

	#endregion // SummaryCellPresenterLayoutElement Class

	//#region CellPresenterLayoutElement class

	///// <summary>
	///// An element used in the visual tree of the CellPresenter control to arrange a CellValuePresenter element and/or a LabelPresenter element based on the LabelLocation in effect for the cell.
	///// </summary>
	///// <seealso cref="CellPresenter"/>
	///// <seealso cref="CellValuePresenter"/>
	///// <seealso cref="LabelPresenter"/>
	////[Description("An element used in the visual tree of the CellPresenter control to arrange a CellValuePresenter element and/or a LabelPresenter element based on the LabelLocation in effect for the cell.")]
	//sealed public class CellPresenterLayoutElement : FrameworkElement
	//{
	//    #region Private Members

	//    private Field _cachedField;
	//    private CellValuePresenter _cellValuePresenter;
	//    private LabelPresenter _labelPresenter;
	//    private CellPresenter _parentCellPresenter;

	//    #endregion //Private Members

	//    #region Constructors

	//    static CellPresenterLayoutElement()
	//    {
	//        FrameworkElement.FocusableProperty.OverrideMetadata(typeof(CellPresenterLayoutElement), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
	//    }

	//    /// <summary>
	//    /// Initializes a new instance of the <see cref="CellPresenterLayoutElement"/> class
	//    /// </summary>
	//    public CellPresenterLayoutElement()
	//    {

	//    }

	//    #endregion //Constructors

	//    #region Base class overrides

	//        #region ArrangeOverride

	//    /// <summary>
	//    /// Positions child elements and determines a size for this element.
	//    /// </summary>
	//    /// <param name="finalSize">The size available to this element for arranging its children.</param>
	//    /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
	//    protected override Size ArrangeOverride(Size finalSize)
	//    {
	//        base.ArrangeOverride(finalSize);

	//        if (this._labelPresenter == null || this._cellValuePresenter == null)
	//        {
	//            if (this._cellValuePresenter != null)
	//                this._cellValuePresenter.Arrange(new Rect(finalSize));

	//            if (this._labelPresenter != null)
	//                this._labelPresenter.Arrange(new Rect(finalSize));

	//            return finalSize;
	//        }

	//        CellContentAlignment contentAlignment = this._cachedField.CellContentAlignmentResolved;

	//        #region Refactored
	//        /* AS 12/18/07 BR25223
	//         * Moved to a helper method.
	//         * 
	//        HorizontalAlignment hAlign = HorizontalAlignment.Left;
	//        VerticalAlignment vAlign = VerticalAlignment.Top;

	//        Rect cellRect;
	//        Rect labelRect;
	//        bool isLabelAbove = false;
	//        bool isLabelBelow = false;
	//        bool isLabelLeft = false;
	//        bool isLabelRight = false;

	//        switch (contentAlignment)
	//        {
	//            case CellContentAlignment.LabelAboveValueAlignCenter:
	//                isLabelAbove = true;
	//                hAlign = HorizontalAlignment.Center;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelAboveValueAlignLeft:
	//                isLabelAbove = true;
	//                hAlign = HorizontalAlignment.Left;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelAboveValueAlignRight:
	//                isLabelAbove = true;
	//                hAlign = HorizontalAlignment.Right;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelAboveValueStretch:
	//                isLabelAbove = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelBelowValueAlignCenter:
	//                isLabelBelow = true;
	//                hAlign = HorizontalAlignment.Center;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelBelowValueAlignLeft:
	//                isLabelBelow = true;
	//                hAlign = HorizontalAlignment.Left;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelBelowValueAlignRight:
	//                isLabelBelow = true;
	//                hAlign = HorizontalAlignment.Right;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelBelowValueStretch:
	//                isLabelBelow = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelLeftOfValueAlignBottom:
	//                isLabelLeft = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Bottom;
	//                break;
	//            case CellContentAlignment.LabelLeftOfValueAlignMiddle:
	//                isLabelLeft = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Center;
	//                break;
	//            case CellContentAlignment.LabelLeftOfValueAlignTop:
	//                isLabelLeft = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Top;
	//                break;
	//            case CellContentAlignment.LabelLeftOfValueStretch:
	//                isLabelLeft = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelRightOfValueAlignBottom:
	//                isLabelRight = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Bottom;
	//                break;
	//            case CellContentAlignment.LabelRightOfValueAlignMiddle:
	//                isLabelRight = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Center;
	//                break;
	//            case CellContentAlignment.LabelRightOfValueAlignTop:
	//                isLabelRight = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Top;
	//                break;
	//            case CellContentAlignment.LabelRightOfValueStretch:
	//                isLabelRight = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            default:
	//                Debug.Fail("Invalid alignment in CellValueAndLabelElement ArrangeOverride");
	//                break;
	//        }

	//        labelRect = new Rect(this._labelPresenter.DesiredSize);
	//        cellRect = new Rect(this._cellValuePresenter.DesiredSize);

	//        if (isLabelAbove)
	//        {
	//            cellRect.Y = labelRect.Height;
	//        }
	//        else if (isLabelBelow)
	//        {
	//            labelRect.Y = Math.Max(0, finalSize.Height - labelRect.Height);
	//        }
	//        else if (isLabelLeft)
	//        {
	//            cellRect.X = labelRect.Width;
	//        }
	//        else if (isLabelRight)
	//        {
	//            labelRect.X = Math.Max(0, finalSize.Width - labelRect.Width);
	//        }

	//        switch (hAlign)
	//        {
	//            case HorizontalAlignment.Stretch:
	//                if (isLabelAbove || isLabelBelow)
	//                {
	//                    cellRect.Width = finalSize.Width;
	//                    labelRect.Width = finalSize.Width;
	//                }
	//                else
	//                {
	//                    if (isLabelLeft)
	//                        cellRect.Width = Math.Max(0, finalSize.Width - cellRect.X);
	//                    else
	//                        labelRect.Width = Math.Max(0, finalSize.Width - labelRect.X);
	//                }
	//                break;
	//            case HorizontalAlignment.Right:
	//                cellRect.X = Math.Max(0, finalSize.Width - cellRect.Width);
	//                labelRect.X = Math.Max(0, finalSize.Width - labelRect.Width);
	//                break;
	//            case HorizontalAlignment.Center:
	//                cellRect.X = Math.Max(0, (finalSize.Width - cellRect.Width) / 2);
	//                labelRect.X = Math.Max(0, (finalSize.Width - labelRect.Width) / 2);
	//                break;
	//        }

	//        switch (vAlign)
	//        {
	//            case VerticalAlignment.Stretch:
	//                if (isLabelLeft || isLabelRight)
	//                {
	//                    cellRect.Height = finalSize.Height;
	//                    labelRect.Height = finalSize.Height;
	//                }
	//                else
	//                {
	//                    if (isLabelAbove)
	//                        cellRect.Height = Math.Max(0, finalSize.Height - cellRect.Y);
	//                    else
	//                        labelRect.Height = Math.Max(0, finalSize.Height - labelRect.Y);
	//                }
	//                break;
	//            case VerticalAlignment.Bottom:
	//                cellRect.Y = Math.Max(0, finalSize.Height - cellRect.Height);
	//                labelRect.Y = Math.Max(0, finalSize.Height - labelRect.Height);
	//                break;
	//            case VerticalAlignment.Center:
	//                cellRect.Y = Math.Max(0, (finalSize.Height - cellRect.Height) / 2);
	//                labelRect.Y = Math.Max(0, (finalSize.Height - labelRect.Height) / 2);
	//                break;
	//        }
	//        */
	//        #endregion

	//        Rect cellRect;
	//        Rect labelRect;
	//        CalculateRects(finalSize, this._labelPresenter.DesiredSize, this._cellValuePresenter.DesiredSize, contentAlignment, out labelRect, out cellRect);

	//        this._labelPresenter.Arrange(labelRect);
	//        this._cellValuePresenter.Arrange(cellRect);

	//        return finalSize;
	//    }

	//        #endregion //ArrangeOverride

	//        #region GetVisualChild

	//    /// <summary>
	//    /// Gets the visual child at a specified index.
	//    /// </summary>
	//    /// <param name="index">The zero-based index of the specific child visual.</param>
	//    /// <returns>The visual child at the specified index.</returns>
	//    protected override Visual GetVisualChild(int index)
	//    {
	//        if (index == 0)
	//        {
	//            if (this._cellValuePresenter != null)
	//                return this._cellValuePresenter;

	//            return this._labelPresenter;
	//        }

	//        if (index == 1)
	//        {
	//            return this._labelPresenter;
	//        }

	//        throw new IndexOutOfRangeException();
	//    }

	//        #endregion //GetVisualChild

	//    // JJD 5/30/07 return LogicalChildren enumerator
	//    #region LogicalChildren
	//    /// <summary>
	//    /// Gets an enumerator for logical child elements in this panel.
	//    /// </summary>
	//    protected override IEnumerator LogicalChildren
	//    {
	//        get
	//        {
	//            FrameworkElement[] array = new FrameworkElement[this.VisualChildrenCount];

	//            for (int i = 0; i < this.VisualChildrenCount; i++)
	//                array[i] = this.GetVisualChild(i) as FrameworkElement;

	//            return array.GetEnumerator();
	//        }
	//    }
	//    #endregion //LogicalChildren

	//        #region MeasureOverride

	//    /// <summary>
	//    /// Invoked to measure the element and its children.
	//    /// </summary>
	//    /// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
	//    /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
	//    protected override Size MeasureOverride(Size availableSize)
	//    {
	//        Size sizeRequired = new Size();

	//        if (this._parentCellPresenter == null)
	//        {
	//            this._parentCellPresenter = Utilities.GetAncestorFromType(this, typeof(CellPresenter), false, null, typeof(RecordListControl)) as CellPresenter;

	//            Debug.Assert(this._parentCellPresenter != null);
	//        }

	//        if ( this._parentCellPresenter != null )
	//            this._cachedField = this._parentCellPresenter.Field;

	//        DataRecord dr = this.DataContext as DataRecord;

	//        Debug.Assert(dr != null);

	//        if (this._cachedField != null && dr != null)
	//        {
	//            CellContentAlignment contentAlignment = this._cachedField.CellContentAlignmentResolved;

	//            #region InitializeChildElements

	//            // initialize cellvaluepresenter
	//            if (contentAlignment == CellContentAlignment.LabelOnly)
	//            {
	//                this.ReleaseCellValuePresenter();
	//            }
	//            else
	//            {
	//                if (this._cellValuePresenter == null ||
	//                     this._cellValuePresenter.Field != this._cachedField)
	//                {
	//                    // JJD 5/30/07
	//                    // try to re-use the element rather than release and create a new one
	//                    //    this.ReleaseCellValuePresenter();
	//                    if (this._cellValuePresenter == null)
	//                    {
	//                        this._cellValuePresenter = this._cachedField.GetCellValuePresenter();

	//                        this.AddLogicalChild(this._cellValuePresenter);
	//                        this.AddVisualChild(this._cellValuePresenter);
	//                    }

	//                    this._cellValuePresenter.Field = this._cachedField;

	//                    // JJD 5/30/07 
	//                    // These settings aren't necessary since they are done in the cvp's Field chnage logic
	//                    //this._cellValuePresenter.SetBinding(CellValuePresenter.IsFieldSelectedProperty, Utilities.CreateBindingObject(Field.IsSelectedProperty, BindingMode.OneWay, this._cachedField));
	//                    //this._cellValuePresenter.HasSeparateHeader = this._cachedField.Owner.HasSeparateHeader;
	//                }

	//                //Debug.Assert(dr == this._cellValuePresenter.Record);
	//            }

	//            // initialize labelpresenter
	//            if (contentAlignment == CellContentAlignment.ValueOnly)
	//            {
	//                this.ReleaseLabelPresenter();
	//            }
	//            else
	//                if (this._labelPresenter == null ||
	//                     this._labelPresenter.Field != this._cachedField)
	//                {
	//                    this.ReleaseLabelPresenter();

	//                    this._labelPresenter = this._cachedField.GetLabelPresenter();

	//                    this.AddLogicalChild(this._labelPresenter);
	//                    this.AddVisualChild(this._labelPresenter);

	//                    this._labelPresenter.Field = this._cachedField;
	//                }

	//            #endregion //InitializeChildElements

	//            #region Determine is label is above, below, left or right

	//            bool isLabelAboveOrBelow = false;
	//            bool isLabelLeftOrRight = false;
	//            switch (contentAlignment)
	//            {
	//                case CellContentAlignment.LabelAboveValueAlignCenter:
	//                case CellContentAlignment.LabelAboveValueAlignLeft:
	//                case CellContentAlignment.LabelAboveValueAlignRight:
	//                case CellContentAlignment.LabelAboveValueStretch:
	//                case CellContentAlignment.LabelBelowValueAlignCenter:
	//                case CellContentAlignment.LabelBelowValueAlignLeft:
	//                case CellContentAlignment.LabelBelowValueAlignRight:
	//                case CellContentAlignment.LabelBelowValueStretch:
	//                    isLabelAboveOrBelow = true;
	//                    break;
	//                case CellContentAlignment.LabelLeftOfValueAlignBottom:
	//                case CellContentAlignment.LabelLeftOfValueAlignMiddle:
	//                case CellContentAlignment.LabelLeftOfValueAlignTop:
	//                case CellContentAlignment.LabelLeftOfValueStretch:
	//                case CellContentAlignment.LabelRightOfValueAlignBottom:
	//                case CellContentAlignment.LabelRightOfValueAlignMiddle:
	//                case CellContentAlignment.LabelRightOfValueAlignTop:
	//                case CellContentAlignment.LabelRightOfValueStretch:
	//                    isLabelLeftOrRight = true;
	//                    break;
	//            }

	//            #endregion //Determine is label is above, below, left or right

	//            Size cellValueSize;

	//            //				bool isHorizontalLayout = this._cachedField.Owner.IsHorizontal;

	//            // NOTE: These can come back as double.NaN
	//            double labelWidth = this._cachedField.GetExtent(dr, true, true);
	//            double cellValueWidth = this._cachedField.GetExtent(dr, false, true);
	//            double labelHeight = this._cachedField.GetExtent(dr, true, false);
	//            double cellValueHeight = this._cachedField.GetExtent(dr, false, false);

	//            // if this is a cell presenter within the template record then measure
	//            // the cell and/or label and use its value if a default was not provided
	//            Debug.Assert(this._cachedField.Owner != null);
	//            if (this._cachedField.Owner != null && this._cachedField.Owner.TemplateDataRecordCache.IsInitializingCache)
	//            {
	//                if (double.IsNaN(cellValueWidth) || double.IsNaN(cellValueHeight))
	//                {
	//                    if (null != this._cellValuePresenter)
	//                    {
	//                        this._cellValuePresenter.Measure(availableSize);
	//                        cellValueHeight = this._cellValuePresenter.DesiredSize.Height;
	//                        cellValueWidth = this._cellValuePresenter.DesiredSize.Width;
	//                    }
	//                }

	//                if (double.IsNaN(labelHeight) || double.IsNaN(labelWidth))
	//                {
	//                    if (null != this._labelPresenter)
	//                    {
	//                        this._labelPresenter.Measure(availableSize);
	//                        labelHeight = this._labelPresenter.DesiredSize.Height;
	//                        labelWidth = this._labelPresenter.DesiredSize.Width;
	//                    }
	//                }
	//            }

	//            double cellValueWidthToAdd = 0.0;
	//            double cellValueHeightToAdd = 0.0;
	//            double labelWidthToAdd = 0.0;
	//            double labelHeightToAdd = 0.0;

	//            int numberOfElements = this.VisualChildrenCount;

	//            if (!double.IsPositiveInfinity(availableSize.Height))
	//            {
	//                // determine if there is extra height and calculate how to split
	//                // it up between the cell and the label
	//                if (isLabelLeftOrRight || numberOfElements < 2)
	//                {
	//                    if (cellValueHeight < availableSize.Height)
	//                        cellValueHeightToAdd = availableSize.Height - cellValueHeight;

	//                    if (labelHeight < availableSize.Height)
	//                        labelHeightToAdd = availableSize.Height - labelHeight;
	//                }
	//                else
	//                {
	//                    if (cellValueHeight + labelHeight < availableSize.Height)
	//                        cellValueHeightToAdd = availableSize.Height - (cellValueHeight + labelHeight);
	//                }
	//            }

	//            if (!double.IsPositiveInfinity(availableSize.Width))
	//            {
	//                // determine if there is extra width and calculate how to split
	//                // it up between the cell and the label
	//                if (isLabelAboveOrBelow || numberOfElements < 2)
	//                {
	//                    if (cellValueWidth < availableSize.Width)
	//                        cellValueWidthToAdd = availableSize.Width - cellValueWidth;

	//                    if (labelWidth < availableSize.Width)
	//                        labelWidthToAdd = availableSize.Width - labelWidth;
	//                }
	//                else
	//                {
	//                    if (cellValueWidth + labelWidth < availableSize.Width)
	//                        cellValueWidthToAdd = availableSize.Width - (cellValueWidth + labelWidth);
	//                }
	//            }

	//            // adjust the values that we use based on the above calculations
	//            if (cellValueHeightToAdd > 0.01)
	//                cellValueHeight += cellValueHeightToAdd;
	//            if (labelHeightToAdd > 0.01)
	//                labelHeight += labelHeightToAdd;
	//            if (cellValueWidthToAdd > 0.01)
	//                cellValueWidth += cellValueWidthToAdd;
	//            if (labelWidthToAdd > 0.01)
	//                labelWidth += labelWidthToAdd;

	//            if (this._cellValuePresenter != null)
	//            {
	//                Size availableCellSize = availableSize;

	//                if (isLabelLeftOrRight)
	//                {
	//                    if (double.IsNaN(cellValueWidth))
	//                        availableCellSize.Width = Math.Max(1, availableSize.Width - this._cachedField.GetLabelWidthResolvedHelper(true, true));
	//                }

	//                if (!double.IsNaN(cellValueWidth))
	//                {
	//                    this._cellValuePresenter.Width = cellValueWidth;
	//                    availableCellSize.Width = cellValueWidth;
	//                }
	//                else
	//                    this._cellValuePresenter.ClearValue(WidthProperty);

	//                if (!double.IsNaN(cellValueHeight))
	//                {
	//                    this._cellValuePresenter.Height = cellValueHeight;
	//                    availableCellSize.Height = cellValueHeight;
	//                }
	//                else
	//                    this._cellValuePresenter.ClearValue(HeightProperty);

	//                this._cellValuePresenter.Measure(availableCellSize);
	//                cellValueSize = this._cellValuePresenter.DesiredSize;
	//            }
	//            else
	//                cellValueSize = new Size();

	//            Size labelSize;

	//            if (this._labelPresenter != null)
	//            {
	//                Size availableLabelSize = availableSize;

	//                if (isLabelAboveOrBelow)
	//                {
	//                    if (double.IsNaN(labelHeight))
	//                        availableLabelSize.Height = Math.Max(1, availableSize.Height - this._cachedField.GetLabelHeightResolvedHelper(true, true));
	//                }

	//                if (!double.IsNaN(labelWidth))
	//                {
	//                    this._labelPresenter.Width = labelWidth;
	//                    availableLabelSize.Width = labelWidth;
	//                }
	//                else
	//                    this._labelPresenter.ClearValue(WidthProperty);

	//                if (!double.IsNaN(labelHeight))
	//                {
	//                    this._labelPresenter.Height = labelHeight;
	//                    availableLabelSize.Height = labelHeight;
	//                }
	//                else
	//                    this._labelPresenter.ClearValue(HeightProperty);

	//                this._labelPresenter.Measure(availableLabelSize);

	//                labelSize = this._labelPresenter.DesiredSize;
	//            }
	//            else
	//                labelSize = new Size();

	//            if (isLabelAboveOrBelow)
	//            {
	//                sizeRequired.Width = Math.Max(cellValueSize.Width, labelSize.Width);
	//                sizeRequired.Height = cellValueSize.Height + labelSize.Height;
	//            }
	//            else if (isLabelLeftOrRight)
	//            {
	//                sizeRequired.Width = cellValueSize.Width + labelSize.Width;
	//                sizeRequired.Height = Math.Max(cellValueSize.Height, labelSize.Height);
	//            }
	//            else
	//            {
	//                switch (contentAlignment)
	//                {
	//                    case CellContentAlignment.LabelOnly:
	//                        sizeRequired = labelSize;
	//                        break;
	//                    case CellContentAlignment.ValueOnly:
	//                        sizeRequired = cellValueSize;
	//                        break;
	//                }
	//            }
	//        }

	//        sizeRequired.Width = Math.Max(1, sizeRequired.Width);
	//        sizeRequired.Height = Math.Max(1, sizeRequired.Height);

	//        return sizeRequired;
	//    }

	//        #endregion //MeasureOverride

	//        #region OnPropertyChanged

	//    /// <summary>
	//    /// Called whena dependency property has been changed
	//    /// </summary>
	//    /// <param name="e">The event arguments identifying the property plus the new and old value.</param>
	//    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	//    {
	//        base.OnPropertyChanged(e);

	//        switch (e.Property.Name)
	//        {
	//            case "IWindowService":
	//                if (e.NewValue == null)
	//                {
	//                    this.ReleaseCellValuePresenter();
	//                    this.ReleaseLabelPresenter();
	//                }
	//                break;
	//        }
	//    }

	//        #endregion //OnPropertyChanged

	//        #region ToString

	//    /// <summary>
	//    /// Returns a string representation of the object
	//    /// </summary>
	//    public override string ToString()
	//    {
	//        StringBuilder sb = new StringBuilder();

	//        sb.Append("CellPresenterLayoutElement: ");

	//        if (this._cachedField != null)
	//        {
	//            sb.Append(this._cachedField.ToString());
	//            sb.Append(", ");
	//        }

	//        return sb.ToString();
	//    }

	//        #endregion //ToString

	//        #region VisualChildrenCount

	//    /// <summary>
	//    /// Returns the total numder of visual children (read-only).
	//    /// </summary>
	//    protected override int VisualChildrenCount
	//    {
	//        get
	//        {
	//            int count = 0;

	//            if (this._cellValuePresenter != null)
	//                count++;

	//            if (this._labelPresenter != null)
	//                count++;

	//            return count;
	//        }
	//    }

	//        #endregion //VisualChildrenCount

	//    #endregion //Base class overrides

	//    #region Properties

	//        #region Internal Properties

	//            #region CellValuePresenter

	//    internal CellValuePresenter CellValuePresenter { get { return this._cellValuePresenter; } }

	//            #endregion //CellValuePresenter

	//            #region DataPresenterBase

	//    internal DataPresenterBase DataPresenter
	//    {
	//        get
	//        {
	//            if (this._cachedField != null)
	//                return this._cachedField.Owner.DataPresenter;

	//            return null;
	//        }
	//    }

	//            #endregion //DataPresenterBase

	//            #region LabelPresenter

	//    internal LabelPresenter LabelPresenter { get { return this._labelPresenter; } }

	//            #endregion //LabelPresenter

	//    #endregion //Internal Properties

	//    #endregion //Properties

	//    #region Methods

	//        #region Private Methods

	//            #region ReleaseCellValuePresenter

	//    private void ReleaseCellValuePresenter()
	//    {
	//        if (this._cellValuePresenter != null)
	//        {
	//            this.RemoveLogicalChild(this._cellValuePresenter);
	//            this.RemoveVisualChild(this._cellValuePresenter);
	//            this._cachedField.ReleaseCellValuePresenter(this._cellValuePresenter);
	//            this._cellValuePresenter = null;
	//        }
	//    }

	//            #endregion //ReleaseCellValuePresenter

	//            #region ReleaseLabelPresenter

	//    private void ReleaseLabelPresenter()
	//    {
	//        if (this._labelPresenter != null)
	//        {
	//            this.RemoveLogicalChild(this._labelPresenter);
	//            this.RemoveVisualChild(this._labelPresenter);
	//            this._cachedField.ReleaseLabelPresenter(this._labelPresenter);
	//            this._labelPresenter = null;
	//        }
	//    }

	//            #endregion //ReleaseLabelPresenter

	//        #endregion //Private Methods

	//        #region Internal Methods

	//            // AS 12/18/07 BR25223
	//            // I need to be able to get to these sizes to find out the size of a cellvaluepresenter/labelpresenter
	//            // based on the size that we would use for a cell value presenter.
	//            //
	//            #region CalculateRects
	//    internal static void CalculateRects(Size cellPresenterSize, Size labelDesiredSize, Size cellDesiredSize,
	//CellContentAlignment contentAlignment, out Rect labelRect, out Rect cellRect)
	//    {
	//        HorizontalAlignment hAlign = HorizontalAlignment.Left;
	//        VerticalAlignment vAlign = VerticalAlignment.Top;

	//        bool isLabelAbove = false;
	//        bool isLabelBelow = false;
	//        bool isLabelLeft = false;
	//        bool isLabelRight = false;

	//        #region ContentAlignment
	//        switch (contentAlignment)
	//        {
	//            case CellContentAlignment.LabelAboveValueAlignCenter:
	//                isLabelAbove = true;
	//                hAlign = HorizontalAlignment.Center;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelAboveValueAlignLeft:
	//                isLabelAbove = true;
	//                hAlign = HorizontalAlignment.Left;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelAboveValueAlignRight:
	//                isLabelAbove = true;
	//                hAlign = HorizontalAlignment.Right;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelAboveValueStretch:
	//                isLabelAbove = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelBelowValueAlignCenter:
	//                isLabelBelow = true;
	//                hAlign = HorizontalAlignment.Center;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelBelowValueAlignLeft:
	//                isLabelBelow = true;
	//                hAlign = HorizontalAlignment.Left;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelBelowValueAlignRight:
	//                isLabelBelow = true;
	//                hAlign = HorizontalAlignment.Right;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelBelowValueStretch:
	//                isLabelBelow = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelLeftOfValueAlignBottom:
	//                isLabelLeft = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Bottom;
	//                break;
	//            case CellContentAlignment.LabelLeftOfValueAlignMiddle:
	//                isLabelLeft = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Center;
	//                break;
	//            case CellContentAlignment.LabelLeftOfValueAlignTop:
	//                isLabelLeft = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Top;
	//                break;
	//            case CellContentAlignment.LabelLeftOfValueStretch:
	//                isLabelLeft = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            case CellContentAlignment.LabelRightOfValueAlignBottom:
	//                isLabelRight = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Bottom;
	//                break;
	//            case CellContentAlignment.LabelRightOfValueAlignMiddle:
	//                isLabelRight = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Center;
	//                break;
	//            case CellContentAlignment.LabelRightOfValueAlignTop:
	//                isLabelRight = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Top;
	//                break;
	//            case CellContentAlignment.LabelRightOfValueStretch:
	//                isLabelRight = true;
	//                hAlign = HorizontalAlignment.Stretch;
	//                vAlign = VerticalAlignment.Stretch;
	//                break;
	//            default:
	//                Debug.Fail("Invalid alignment in CellValueAndLabelElement ArrangeOverride");
	//                break;
	//        }
	//        #endregion

	//        labelRect = new Rect(labelDesiredSize);
	//        cellRect = new Rect(cellDesiredSize);

	//        if (isLabelAbove)
	//        {
	//            cellRect.Y = labelRect.Height;
	//        }
	//        else if (isLabelBelow)
	//        {
	//            labelRect.Y = Math.Max(0, cellPresenterSize.Height - labelRect.Height);
	//        }
	//        else if (isLabelLeft)
	//        {
	//            cellRect.X = labelRect.Width;
	//        }
	//        else if (isLabelRight)
	//        {
	//            labelRect.X = Math.Max(0, cellPresenterSize.Width - labelRect.Width);
	//        }

	//        switch (hAlign)
	//        {
	//            case HorizontalAlignment.Stretch:
	//                if (isLabelAbove || isLabelBelow)
	//                {
	//                    cellRect.Width = cellPresenterSize.Width;
	//                    labelRect.Width = cellPresenterSize.Width;
	//                }
	//                else
	//                {
	//                    if (isLabelLeft)
	//                        cellRect.Width = Math.Max(0, cellPresenterSize.Width - cellRect.X);
	//                    else
	//                        labelRect.Width = Math.Max(0, cellPresenterSize.Width - labelRect.X);
	//                }
	//                break;
	//            case HorizontalAlignment.Right:
	//                cellRect.X = Math.Max(0, cellPresenterSize.Width - cellRect.Width);
	//                labelRect.X = Math.Max(0, cellPresenterSize.Width - labelRect.Width);
	//                break;
	//            case HorizontalAlignment.Center:
	//                cellRect.X = Math.Max(0, (cellPresenterSize.Width - cellRect.Width) / 2);
	//                labelRect.X = Math.Max(0, (cellPresenterSize.Width - labelRect.Width) / 2);
	//                break;
	//        }

	//        switch (vAlign)
	//        {
	//            case VerticalAlignment.Stretch:
	//                if (isLabelLeft || isLabelRight)
	//                {
	//                    cellRect.Height = cellPresenterSize.Height;
	//                    labelRect.Height = cellPresenterSize.Height;
	//                }
	//                else
	//                {
	//                    if (isLabelAbove)
	//                        cellRect.Height = Math.Max(0, cellPresenterSize.Height - cellRect.Y);
	//                    else
	//                        labelRect.Height = Math.Max(0, cellPresenterSize.Height - labelRect.Y);
	//                }
	//                break;
	//            case VerticalAlignment.Bottom:
	//                cellRect.Y = Math.Max(0, cellPresenterSize.Height - cellRect.Height);
	//                labelRect.Y = Math.Max(0, cellPresenterSize.Height - labelRect.Height);
	//                break;
	//            case VerticalAlignment.Center:
	//                cellRect.Y = Math.Max(0, (cellPresenterSize.Height - cellRect.Height) / 2);
	//                labelRect.Y = Math.Max(0, (cellPresenterSize.Height - labelRect.Height) / 2);
	//                break;
	//        }
	//    }
	//            #endregion //CalculateRects

	//        #endregion // Internal Methods
	//    #endregion //Methods

	//}

	//#endregion //CellPresenterLayoutElement class

	// JJD 5/3/07 - Optimization
	// Added lightweight element to represent a cell in the template record
	#region CellPlaceholder class

	/// <summary>
	/// For internal use only
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	sealed public class CellPlaceholder : FrameworkElement
		, IWeakEventListener // AS 10/20/10 TFS26331
	{
		#region Private Members

		private Field _cachedField;
		private Control _childCellElement;
		private bool _isChildControlOwnedByThisPlaceholder;
		private bool _isHooked = false; // AS 10/20/10 TFS26331

		// AS 2/19/10 TFS28036
		[ThreadStatic]
		private static int _measureCounter;

		// AS 10/9/09 NA 2010.1 - CardView
		// At least for the template, we don't want to virtualize the cellpresenters
		// in card view because we want to be able to get to the desired width of 
		// the labelpresenters to use as the preferred width.
		//
		private bool _alwaysCreateElement = false;

		#endregion //Private Members

        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="CellPlaceholder"/>
        /// </summary>
        public CellPlaceholder()
        {
            // AS 2/18/09 TFS7941
            NameScope.SetNameScope(this, new OwnerNameScope(this));
        }

		// AS 10/9/09 NA 2010.1 - CardView
		internal CellPlaceholder(bool alwaysCreateElement)
			: this()
		{
			_alwaysCreateElement = alwaysCreateElement;
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
			// [JM 05-31-07 BR23089]
			//if (this._isChildControlOwnedByThisPlaceholder)
			if (this._isChildControlOwnedByThisPlaceholder && this._childCellElement != null)
			{
				this._childCellElement.Arrange(new Rect(finalSize));
			}

			return finalSize;
		}

			#endregion //ArrangeOverride

			#region GetVisualChild

		/// <summary>
		/// Gets the visual child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child visual.</param>
		/// <returns>The visual child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
			if (index == 0)
			{
				if (this._isChildControlOwnedByThisPlaceholder)
					return this._childCellElement;
			}

			throw new IndexOutOfRangeException();
		}

			#endregion //GetVisualChild

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			// [JM 05-31-07 BR23089]
            if (this._childCellElement == null)
            {
                // AS 3/6/09 TFS15025
                // The available size may be infinity which is not a valid return value.
                //
                if (double.IsPositiveInfinity(availableSize.Width))
                    availableSize.Width = 0;
                if (double.IsPositiveInfinity(availableSize.Height))
                    availableSize.Height = 0;

                return availableSize;
            }

			try
			{
				// AS 2/19/10 TFS28036
				// keep track of whether we are measuring a placeholder since we don't 
				// want to mess with the containing panel
				//
				_measureCounter++;

				if (this._isChildControlOwnedByThisPlaceholder)
					this._childCellElement.Measure(availableSize);

				return this._childCellElement.DesiredSize;
			}
			finally
			{
				// AS 2/19/10 TFS28036
				_measureCounter--;
			}
		}

			#endregion //MeasureOverride

            // AS 2/18/09 TFS7941
            #region LogicalChildren
        /// <summary>
        /// Returns an enumerator used to iterate the logical children.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (_isChildControlOwnedByThisPlaceholder)
                    return new SingleItemEnumerator(this._childCellElement);

                return base.LogicalChildren;
            }
        } 
            #endregion //LogicalChildren

            #region OnRenderSizeChanged
        /// <summary>
        /// Invoked when the actual size of the element has been changed.
        /// </summary>
        /// <param name="sizeInfo">Provides information about the size change</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            FieldLayout fl = this.Field != null ? this.Field.Owner : null;

            // AS 2/18/09 TFS7941
            // When the size of a template item has been changed we need to dirty 
            // cached values on the layout items so they can get the new default
            // values.
            //
            // AS 3/6/09 TFS15025
            // This isn't specific to this bug but don't bother for elements that don't 
            // own a cell/label presenter.
            //
            //if (null != fl)
            if (null != fl && _isChildControlOwnedByThisPlaceholder)
            {
                // AS 2/20/09 TFS7941
                // We need to do this asynchronously. Bumping the version invalidates the arrange 
                // of the virtualizingdatarecordcellpanels. When an element has its measure/arrange 
                // invalidated during the RenderSizeChanged or LayoutUpdated, the process is halted.
                // So what ends up happening is that the first cellplaceholder gets its rendersize
                // changed. It dirties all the virtualizingdatarecordcellpanels. The wpf framework
                // stops calling RenderSizeChanged because something was dirtied. Then those panels
                // and other elements get measured/arranged. Then this is called for the second 
                // cellplaceholder and the process happens again. We really only want 1 bump so  
                // we need to call a method on the panel which will asynchronously update the version.
                //
                //fl.BumpLayoutItemVersion();
                fl.BumpLayoutItemVersionAsync();
            }

            base.OnRenderSizeChanged(sizeInfo);
        } 
            #endregion //OnRenderSizeChanged

            // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
            #region OnInitialized
        /// <summary>
        /// Invoked when the element has been initialized.
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            this.VerifyCachedElement();

            base.OnInitialized(e);
        } 
            #endregion //OnInitialized

			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("CellPlaceholder: ");

			if (this._cachedField != null)
			{
				sb.Append(this._cachedField.ToString());
				sb.Append(", ");
			}

			return sb.ToString();
		}

			#endregion //ToString

			#region VisualChildrenCount

		/// <summary>
		/// Returns the total numder of visual children (read-only).
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				if (this._isChildControlOwnedByThisPlaceholder)
					return 1;

				return 0;
			}
		}

			#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Properties

			#region Internal Properties

				#region ChildCellElement

		internal Control ChildCellElement { get { return this._childCellElement; } }

				#endregion //ChildCellElement	
    
				#region Field

		internal static readonly DependencyProperty FieldProperty = DependencyProperty.Register("Field",
				typeof(Field), typeof(CellPlaceholder), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFieldChanged), new CoerceValueCallback(OnCoerceField)));

		private static object OnCoerceField(DependencyObject target, object value)
		{
			CellPlaceholder control = target as CellPlaceholder;

			if (control != null)
			{
				control._cachedField = value as Field;
				return value;
			}

			return null;
		}

		private static void OnFieldChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CellPlaceholder control = target as CellPlaceholder;

			if (control != null)
			{
				control._cachedField = e.NewValue as Field;

                
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)

                control.VerifyCachedElement();
			}
		}

		internal Field Field
		{
			get
			{
				return this._cachedField;
			}
			set
			{
				this.SetValue(CellPlaceholder.FieldProperty, value);
			}
		}

				#endregion //Field

				// AS 2/19/10 TFS28036
				#region IsInMeasure
		internal static bool IsInMeasure
		{
			get { return _measureCounter != 0; }
		} 
				#endregion //IsInMeasure

                // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
                #region IsLabel

        internal static readonly DependencyProperty IsLabelProperty = DependencyProperty.Register("IsLabel",
            typeof(bool), typeof(CellPlaceholder), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

                #endregion //IsLabel

			#endregion //Internal Properties

		#endregion //Properties

        #region Methods

		// AS 6/16/09 NA 2009.2 Field Sizing
		// Moved this from within the VerifyCachedElement. This method does not try to cache the element it creates 
		// on the template cache. It leaves that to the calling routine. For the VerifyCachedElement, it will call
		// that as it did before. But if we're creating this to use for measurement then we won't.
		//
		#region CreateCachedElement
		private void CreateCachedElement(bool cacheInTemplateRecord)
		{
			if (_cachedField == null || _isChildControlOwnedByThisPlaceholder)
				return;

			FieldLayout fl = _cachedField.Owner;

			Debug.Assert(fl != null);

			if (fl == null)
				return;

			bool isLabel = (bool)this.GetValue(IsLabelProperty);

			// AS 3/6/09 TFS15025
			// If the Theme is set then the template may get re-verified for the 
			// disconnected template record presenter. If that's the case then we 
			// don't want to cache/create an element for this placeholder.
			//
			DataRecordPresenter rp = Utilities.GetAncestorFromType(this, typeof(DataRecordPresenter), true) as DataRecordPresenter;

			Debug.Assert(null != rp);

			if (false == fl.TemplateDataRecordCache.IsTemplateRecordPresenter(rp))
				return;

			this._isChildControlOwnedByThisPlaceholder = true;
			// AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
			if (isLabel)
			{
				this._childCellElement = new LabelPresenter();
			}
			else
				if (fl.UseCellPresenters)
				{
					this._childCellElement = new CellPresenter();
					// AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
					// Moved down since its the same DP for all these elements.
					//
					//((CellPresenter)(this._childCellElement)).Field = this._cachedField;
				}
				else
				{
					this._childCellElement = new CellValuePresenter();
					// AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
					// Moved down since its the same DP for all these elements.
					//
					//((CellValuePresenter)(this._childCellElement)).Field = this._cachedField;
				}

			// AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
			// Moved from above since its the same DP for all these elements.
			//
			//this._childCellElement.SetValue(CellValuePresenter.FieldProperty, this._cachedField);

			this.AddLogicalChild(this._childCellElement);
			this.AddVisualChild(this._childCellElement);

			this._childCellElement.SetValue(CellValuePresenter.FieldProperty, this._cachedField);

			// AS 10/20/10 TFS26331
			HookUnhookFieldProperties();

			if (cacheInTemplateRecord)
				fl.TemplateDataRecordCache.CacheCellElement(this._cachedField, this._childCellElement, isLabel);
		}
		#endregion //CreateCachedElement

		// AS 6/18/09 NA 2009.2 Field Sizing
		#region EnsureHasOwnControl
		internal void EnsureHasOwnControl()
		{
			this.VerifyCachedElement();

			Debug.Assert(null != _cachedField);

			if (_cachedField == null || _childCellElement == null)
				return;

			Field ctrlField = GridUtilities.GetFieldFromControl(_childCellElement);

			if (ctrlField == _cachedField)
				return;

			this.CreateCachedElement(false);
		}
		#endregion //EnsureHasOwnControl

		// AS 10/20/10 TFS26331
		#region HookUnhookFieldProperties
		private void HookUnhookFieldProperties()
		{
			bool hook = _cachedField != null && _childCellElement is CellPresenterBase && _isChildControlOwnedByThisPlaceholder;

			if (hook == _isHooked)
				return;

			_isHooked = hook;
			if (hook)
				PropertyChangedEventManager.AddListener(_cachedField, this, "LabelWidthResolved");
			else
				PropertyChangedEventManager.RemoveListener(_cachedField, this, "LabelWidthResolved");
		}
		#endregion //HookUnhookFieldProperties

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        // Moved from the OnFieldChanged since we have to wait to see if the 
        // IsLabel will be set so we have to wait for OnInitialized.
        //
        #region VerifyCachedElement
        private void VerifyCachedElement()
        {
            if (this.IsInitialized && this._cachedField != null)
            {
                FieldLayout fl = this._cachedField.Owner;

                Debug.Assert(fl != null);

                if (fl == null)
                    return;

                // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
                //this._childCellElement = fl.TemplateDataRecordCache.GetCellElement(this._cachedField);
                bool isLabel = (bool)this.GetValue(IsLabelProperty);
                this._childCellElement = fl.TemplateDataRecordCache.GetCellElement(this._cachedField, isLabel);

                // if the child cell element wasn't already in the cache then add it now
				if (this._childCellElement == null)
				{
					
#region Infragistics Source Cleanup (Region)


















































#endregion // Infragistics Source Cleanup (Region)

					this.CreateCachedElement(true);
				}
				// AS 10/9/09 NA 2010.1 - CardView
				else if (_alwaysCreateElement)
					this.CreateCachedElement(false);
            }
        }

        #endregion //VerifyCachedElement

        #endregion //Methods

        // AS 2/18/09 TFS7941
        #region OwnerNameScope class
        private class OwnerNameScope : NameScopeProxy
        {
            #region Member Variables

            private CellPlaceholder _cp;

            #endregion // Member Variables

            #region Constructor
            internal OwnerNameScope(CellPlaceholder cp)
            {
                this._cp = cp;
            }
            #endregion // Constructor

            #region Base class overrides
            protected override DependencyObject StartingElement
            {
                get { return _cp.Field != null ? _cp.Field.DataPresenter : null; }
            } 
            #endregion //Base class overrides
        }
        #endregion // OwnerNameScope class

		// AS 10/20/10 TFS26331
		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				// when hosting cell presenters and the label is resized we need to dirty the cached size
				CellPresenterBase cpBase = this._childCellElement as CellPresenterBase;

				if (null != cpBase)
					cpBase.InvalidateLayoutElement();

				this.InvalidateMeasure();
				return true;
			}

			return false;
		}

		#endregion
	}

	#endregion //CellPlaceholder class	
    
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