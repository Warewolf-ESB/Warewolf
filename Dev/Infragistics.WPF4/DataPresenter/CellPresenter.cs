using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
//using System.Windows.Events;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using System.Windows.Data;
using Infragistics.Shared;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Editors.Events;

namespace Infragistics.Windows.DataPresenter
{
    // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
    // We needed a common base class to store the cell/label rect calculated
    // by the record's gridbaglayoutmanager.
    //
    #region CellPresenterBase
    /// <summary>
    /// Base class for a cell within the <see cref="DataPresenterBase"/> that will contain a <see cref="CellPresenterLayoutElementBase"/> to position its label and/or value.
    /// </summary>
    public abstract class CellPresenterBase : Control
    {
        #region Member Variables

        private Rect _labelRect = Rect.Empty;
        private Rect _cellRect = Rect.Empty;
        private CellPresenterLayoutElementBase _layoutElement;

        #endregion //Member Variables

        #region Constructor
        static CellPresenterBase()
        {
            // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
            CellPresenterBase.ClipProperty.OverrideMetadata(typeof(CellPresenterBase), new FrameworkPropertyMetadata(null, new CoerceValueCallback(VirtualizingDataRecordCellPanel.CoerceCellClip)));

			// AS 8/24/09 TFS19532
			UIElement.VisibilityProperty.OverrideMetadata(typeof(CellPresenterBase), new FrameworkPropertyMetadata(null, new CoerceValueCallback(GridUtilities.CoerceFieldElementVisibility)));
        }

        /// <summary>
        /// Initializes a new <see cref="CellPresenterBase"/>
        /// </summary>
        protected CellPresenterBase()
        {
        } 
        #endregion //Constructor

        #region Base class overrides

        #region OnApplyTemplate
        /// <summary>
        /// Invoked when the layout has changed.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // we need to get the layout element because we need to be able to provide
            // the size of the label/cellvaluepresenter to the virtualizingdatarecordcellpanel's
            // layout manager since it has 2 layout items for each cell presenter so that we 
            // can line up the label presenters/values across the cells.
            _layoutElement = Utilities.GetTemplateChild<CellPresenterLayoutElementBase>(this);

            base.OnApplyTemplate();
        }
        #endregion //OnApplyTemplate

        #endregion //Base class overrides

        #region Properties

        #region Public

        #region Field
        /// <summary>
        /// Returns or sets the associated <see cref="Field"/>
        /// </summary>
        public abstract Field Field
        {
            get;
            set;
        }
        #endregion //Field 

        #endregion //Public

        #region Internal
        internal Rect CellRect
        {
            get { return this._cellRect; }
            set { this._cellRect = value; }
        }

        internal Rect LabelRect
        {
            get { return this._labelRect; }
            set { this._labelRect = value; }
        } 
        #endregion //Internal

        #endregion //Properties

        #region Methods

        // AS 1/26/09 TFS13026
        #region InvalidateLayoutElement
        internal void InvalidateLayoutElement()
        {
            if (null != _layoutElement)
                _layoutElement.InvalidateMeasure();
        }
        #endregion //InvalidateLayoutElement

        #region GetChild
        internal FrameworkElement GetChild(bool label)
        {
            if (null != _layoutElement)
                return label ? _layoutElement.LabelPresenter : _layoutElement.CellValuePresenter;

            return null;
        }
        #endregion //GetChild 

        // JJD 5/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        #region SynchronizeChildElements

        internal virtual void SynchronizeChildElements() { }

        #endregion //SynchronizeChildElements	
    
        #region VerifyLayoutElements
        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
        // Make sure that the elements have been created as needed.
        //
        internal void VerifyLayoutElements()
        {
            if (null != _layoutElement)
                _layoutElement.VerifyChildElements();
        } 
        #endregion //VerifyLayoutElements

        #endregion //Methods

    } 
    #endregion //CellPresenterBase

	#region CellPresenter

	/// <summary>
	/// A control used to display a cell <see cref="Infragistics.Windows.DataPresenter.Cell.Value"/> and/or <see cref="Infragistics.Windows.DataPresenter.Field.Label"/>.  This control is automatically generated by the <see cref="DataPresenterBase"/> to display the value and/or label for a particular <see cref="Field"/> in a particular <see cref="DataRecord"/>.
	/// </summary>
	/// <remarks>
	/// <para class="body">The element is used to contain a <see cref="CellValuePresenter"/> and a corresponding <see cref="LabelPresenter"/> and provide a styling point 
    /// for adding chrome around both elements (refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide). 
	/// It positions its contained CellValuePresenter and LabelPresenter relative to one another based on the <see cref="FieldSettings"/>'s <see cref="FieldSettings.CellContentAlignment"/> setting.
	/// </para>
	/// <para class="note"><b>Note: </b>This element is only used when the <see cref="FieldLayout"/>'s <see cref="FieldLayout.LabelLocationResolved"/> property returns 'InCells'.</para>
	/// </remarks>
	/// <seealso cref="FieldSettings.CellContentAlignment"/> 
	/// <seealso cref="FieldLayoutSettings.LabelLocation"/>
	/// <seealso cref="FieldLayout.LabelLocationResolved"/>
	/// <seealso cref="CellValuePresenter"/>
	/// <seealso cref="LabelPresenter"/>
	//[Description("A control used to display a cell value and/or label.  This control is automatically generated by the DataPresenterBase to display the value and/or label for a particular field in a particular record.")]
    // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
    // I needed a common base class for the CellPresenter and SummaryCellPresenter
    //
	//public class CellPresenter : Control, IWeakEventListener
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CellPresenter : CellPresenterBase, IWeakEventListener
	{
		#region Private Members

		private CellPresenterStyleSelectorHelper _styleSelectorHelper;
		private DataRecord _dataRecord;
		private Field _cachedField = null;
        
        // JJD 5/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _isDragIndicator;

		private CellPresenterLayoutElement _layoutElement;
		private bool _recordOrFieldHasChangedSinceLastMeasure;

		#endregion //Private Members

		#region Constructors

		static CellPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CellPresenter), new FrameworkPropertyMetadata(typeof(CellPresenter)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(CellPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CellPresenter"/> class
		/// </summary>
		public CellPresenter()
		{

			// initialize the styleSelectorHelper
			this._styleSelectorHelper = new CellPresenterStyleSelectorHelper(this);
		}

		#endregion //Constructors

		#region Base class overrides

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
            if (this._layoutElement == null)
            {
                this._layoutElement = Utilities.GetDescendantFromType(this, typeof(CellPresenterLayoutElement), false) as CellPresenterLayoutElement;

                // JJD 5/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                this.SynchronizeChildElements();
            }
			
			// JJD 5/30/07
			// Check flag for delayed initialization
			if (this._recordOrFieldHasChangedSinceLastMeasure)
			{
				this._recordOrFieldHasChangedSinceLastMeasure = false;

				if (this._cachedField != null && this._dataRecord != null)
				{
					this.SetValue(RecordPresenter.IsAlternatePropertyKey, KnownBoxes.FromValue(this.IsAlternate));
					// JM 04-26-11 TFS65510.  Should be accessing OUR IsActive property - not the _dataRecord's IsActive property
					//this.SetValue(IsActiveProperty, KnownBoxes.FromValue(this._dataRecord.IsActive));
					this.SetValue(IsActiveProperty, KnownBoxes.FromValue(this.IsActive));

					this.SetValue(IsSelectedProperty, KnownBoxes.FromValue(this.IsSelected));
				}
			}

			return base.MeasureOverride(availableSize);
		}

			#endregion //MeasureOverride

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property has been changed
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			DependencyProperty property = e.Property;

			if (property == CellPresenter.DataContextProperty)
			{
				// JJD 5/30/07
				// Added flag to postpone some initialization until the next measure
				this._recordOrFieldHasChangedSinceLastMeasure = true;

				// JJD 5/30/07
				// Moved logic to InitializeRecord method
				//if (this.DataContext != null)
				//{
				//    DataRecord rcd = this.DataContext as DataRecord;

				//    if (rcd != null)
				//    {
				//        this._dataRecord = rcd;
				//        this.SetValue(RecordPropertyKey, this.Record);
				//        this.SetValue(RecordPresenter.IsAlternatePropertyKey, KnownBoxes.FromValue(this.IsAlternate));
				//        if ( this._cachedField != null )
				//            this.SetValue(ValueProperty, this.Value);
				//    }

				//    if (!(this.DataContext is Record))
				//        this.InvalidateStyleSelector();
				//}
				this.InitializeRecord();
			}
			else if (property == CellPresenter.SortStatusInternalProperty)
			{
				this.SetValue(SortStatusPropertyKey, this.SortStatusInternal);
			}
			else if (property == CellPresenter.FieldProperty)
			{
				this._cachedField = e.NewValue as Field;

				// JJD 5/30/07
				// Added flag to postpone some initialization until the next measure
				this._recordOrFieldHasChangedSinceLastMeasure = true;

				if (this._cachedField == null)
				{
					this.UnhookAndClearRecord();

					// JJD 5/1/07
					// Clear the bindings
					BindingOperations.ClearBinding(this, SortStatusInternalProperty);
					BindingOperations.ClearBinding(this, IsFieldSelectedProperty);

					// AS 8/24/09 TFS19532
					BindingOperations.ClearBinding(this, GridUtilities.FieldVisibilityProperty);

                    // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                    BindingOperations.ClearBinding(this, IsFixedInternalProperty);
				}
				else
				{
					if (this._dataRecord != null && this._cachedField != null)
						this.SetValue(ValueProperty, this.Value);

					RelativeContentLocation contentLocation = RelativeContentLocation.AboveContentLeft;

					// get the resolved label alignment value
					int enumValue = (int)this._cachedField.CellContentAlignmentResolved;

					// convert the enum to an equivalent value in the RelativeContentLocation enumeration
					enumValue -= (int)CellContentAlignment.LabelAboveValueAlignLeft;

					if (enumValue >= 0)
						contentLocation = (RelativeContentLocation)enumValue;

					//Debug.WriteLine("CellContentAlignmentResolved: " + this._cachedField.CellContentAlignmentResolved.ToString());
					//Debug.WriteLine("RelativeContentLocation: " + ((RelativeContentLocation)enumValue).ToString());

					this.SetValue(ContentLocationPropertyKey, contentLocation);

					// AS 8/24/09 TFS19532
					// See GridUtilities.CoerceFieldElementVisibility for details.
					//
					//this.Visibility = this._cachedField.VisibilityResolved;
					// JJD 3/9/11 - TFS67970 - Optimization - use the cached binding
					//this.SetBinding(GridUtilities.FieldVisibilityProperty, Utilities.CreateBindingObject("VisibilityResolved", BindingMode.OneWay, this._cachedField));
					this.SetBinding(GridUtilities.FieldVisibilityProperty, this._cachedField.VisibilityBinding);

					this.SetBinding(SortStatusInternalProperty, Utilities.CreateBindingObject(Field.SortStatusProperty, BindingMode.OneWay, this._cachedField));

					// JJD 5/1/07
					// Bind IsFieldSelectedProperty to field's IsSelected property
					this.SetBinding(IsFieldSelectedProperty, Utilities.CreateBindingObject(Field.IsSelectedProperty, BindingMode.OneWay, this._cachedField));

                    // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                    this.SetBinding(IsFixedInternalProperty, Utilities.CreateBindingObject(Field.IsFixedProperty, BindingMode.OneWay, this._cachedField));

					FieldLayout fl = this._cachedField.Owner;

					if (fl != null)
					{
						// JJD 5/1/07
						// Initialize the HasSeparateHeader property
						this.SetValue(HasSeparateHeaderProperty, fl.HasSeparateHeader);

                        #region Commented out
						// JJD 5/30/07 
						// Moved logic to InitializeRecord method
						//DataPresenterBase layoutOwner = fl.DataPresenter;

						//if (layoutOwner != null)
						//{
						//    this._styleSelectorHelper.InvalidateStyle();
							
							//DataRecord rcd = this.Record as DataRecord;

							//if (rcd != this._dataRecord)
							//{
							//    this.UnhookAndClearRecord();

							//    this._dataRecord = rcd;

							//    // hook datarecord's property changed event
							//    if (this._dataRecord != null)
							//    {
							//        // use the weak event manager to hook the event so we don't get rooted
							//        PropertyChangedEventManager.AddListener(this._dataRecord, this, string.Empty);
							//    }

							//    // initialize the active property
							//    if (this.IsActive)
							//    {
							//        this.SetValue(IsActiveProperty, KnownBoxes.TrueBox);
							//    }
							//}
						//}
                        #endregion // Commented out
					}
				}

				this.InvalidateStyleSelector();
			}
            else if (e.Property == IsFixedInternalProperty) // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
            {
                this.SetValue(IsFixedPropertyKey, (bool)this.GetValue(IsFixedInternalProperty));
            }
		}

			#endregion //OnPropertyChanged

            // JJD 5/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            #region SynchronizeChildElements

        internal override void SynchronizeChildElements()
        {
            LabelPresenter lp = this.LabelPresenter;

            if (lp != null && this._isDragIndicator != lp.IsDragIndicator)
            {
                if (this._isDragIndicator == true)
                    lp.SetValue(DataItemPresenter.IsDragIndicatorProperty, KnownBoxes.TrueBox);
                else
                    lp.ClearValue(IsDragIndicatorProperty);
            }

            CellValuePresenter cvp = this.CellValuePresenter;

            if (cvp != null && this._isDragIndicator != cvp.IsDragIndicator)
            {
                if (this._isDragIndicator == true)
                    cvp.SetValue(DataItemPresenter.IsDragIndicatorProperty, KnownBoxes.TrueBox);
                else
                    cvp.ClearValue(IsDragIndicatorProperty);
            }
        }

            #endregion //SynchronizeChildElements	
    
			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("CellPresenter: ");

			if (this._cachedField != null)
			{
				sb.Append(this._cachedField.ToString());
				sb.Append(", ");
			}
			if (this._dataRecord != null)
				sb.Append(this._dataRecord.ToString());

			return sb.ToString();
		}

			#endregion //ToString

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region ContentLocation

		private static readonly DependencyPropertyKey ContentLocationPropertyKey =
			DependencyProperty.RegisterReadOnly("ContentLocation",
			typeof(RelativeContentLocation), typeof(CellPresenter), new FrameworkPropertyMetadata(RelativeContentLocation.AboveContentLeft));

		/// <summary>
		/// Identifies the 'ContentLocation' dependency property
		/// </summary>
		public static readonly DependencyProperty ContentLocationProperty =
			ContentLocationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the location of the label (read-only)
		/// </summary>
		//[Description("Returns the location of the label (read-only)")]
		//[Category("Behavior")]
		public RelativeContentLocation ContentLocation
		{
			get
			{
				return (RelativeContentLocation)this.GetValue(CellPresenter.ContentLocationProperty);
			}
		}

				#endregion //ContentLocation

				#region DataPresenterBase

		/// <summary>
		/// Returns the associated <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> (read-only)
		/// </summary>
		public DataPresenterBase DataPresenter
		{
			get
			{
				if (this._cachedField != null)
					return this._cachedField.Owner.DataPresenter;

				return null;
			}
		}

				#endregion //DataPresenterBase

				#region Field

		/// <summary>
		/// Identifies the 'Field' dependency property
		/// </summary>
		public static readonly DependencyProperty FieldProperty = CellValuePresenter.FieldProperty.AddOwner(
			typeof(CellPresenter),
			
			
			
			
			
			
			
			//new FrameworkPropertyMetadata(null, null, new CoerceValueCallback(CoerceField))
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnFieldChanged ) )
		);

		
		
		
		
		
		private static void OnFieldChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			CellPresenter cellPresenter = (CellPresenter)dependencyObject;
			Field newVal = (Field)e.NewValue;

			cellPresenter._cachedField = newVal;

            // AS 1/26/09 TFS13026
            cellPresenter.InvalidateLayoutElement();
        }

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// The associated field.
		/// </summary>
        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
		//public Field Field
		public override Field Field
		{
			get
			{
				return this._cachedField;
			}
			set
			{
				this.SetValue(CellPresenter.FieldProperty, value);
			}
		}

				#endregion //Field

				#region HasSeparateHeader

		/// <summary>
		/// Identifies the 'HasSeparateHeader' dependency property
		/// </summary>
		public static readonly DependencyProperty HasSeparateHeaderProperty = DataItemPresenter.HasSeparateHeaderProperty.AddOwner(
			typeof(CellPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns true if the header area is separate
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasSeparateHeader
		{
			get
			{
				return (bool)this.GetValue(CellPresenter.HasSeparateHeaderProperty);
			}
			set
			{
				this.SetValue(CellPresenter.HasSeparateHeaderProperty, value);
			}
		}

		#		endregion //HasSeparateHeader

				#region IsActive

		/// <summary>
		/// Identifies the 'IsActive' dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
			typeof(bool), typeof(CellPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsActiveChanged), new CoerceValueCallback(CoerceIsActive)));

		private static void OnIsActiveChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CellPresenter cp = target as CellPresenter;

			Debug.Assert(cp != null);

			if (cp != null)
			{
				// JJD 5/30/07 
				// Attempt a coerce if the property was cleared
				if (cp.ReadLocalValue(IsActiveProperty) == DependencyProperty.UnsetValue)
				{
					bool newValue = (bool)e.NewValue;

					CoerceIsActive(cp, KnownBoxes.FromValue(newValue));

					// JJD 5/30/07 
					// If we are still out of sync then set the dependencyproperty value back
					if (cp.IsActive != newValue)
						cp.SetValue(IsActiveProperty, KnownBoxes.FromValue(cp.IsActive));
				}
			}
		}

		private static object CoerceIsActive(DependencyObject target, object value)
		{
			CellPresenter cp = target as CellPresenter;

			Debug.Assert(cp != null);
			Debug.Assert(value is bool);

			if (cp != null)
			{
				DataPresenterBase dp = cp.DataPresenter;

				if ( dp != null)
				{
					if ((bool)value == false)
					{
						// get the curretn active cell
						Cell currentActiveCell = dp.ActiveCell;

						if (currentActiveCell != null)
						{
							// if the field and record are the same then clear the active cell
							if (cp._cachedField == currentActiveCell.Field &&
								cp._dataRecord == currentActiveCell.Record)
							{
								dp.ActiveCell = null;
							}
						}
					}
					else
					{
						DataRecord dr = cp._dataRecord;
						Field fld = cp._cachedField;

						Debug.Assert(dr != null);
						Debug.Assert(fld != null);

						// set the active cell on the datapresenter
						if (dr != null && fld != null)
							dp.ActiveCell = dr.Cells[fld];
					}

					return KnownBoxes.FromValue(cp.IsActive);
				}
			}

			return KnownBoxes.FalseBox;
		}

		/// <summary>
		/// Gets/sets if this <see cref="Infragistics.Windows.DataPresenter.Field"/> is active.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell.IsActive"/>
		public bool IsActive
		{
			get
			{
				if (this._dataRecord == null ||
					!this._dataRecord.IsActive)
					return false;

				if (this._cachedField == null)
					return false;

				Cell activeCell = this.DataPresenter.ActiveCell;

				if (activeCell != null)
					return activeCell.Field == this._cachedField;

				return false;
			}
			set
			{
				this.SetValue(CellPresenter.IsActiveProperty, value);
			}
		}

				#endregion //IsActive

				#region IsAlternate

		/// <summary>
		/// Identifies the 'IsAlternate' dependency property
		/// </summary>
		public static readonly DependencyProperty IsAlternateProperty =
			RecordPresenter.IsAlternatePropertyKey.DependencyProperty.AddOwner(typeof(CellPresenter));

		/// <summary>
		/// Returns true for every other row in the list (readonly)
		/// </summary>
		public bool IsAlternate
		{
			get
			{
				if (this._cachedField != null)
				{
					FieldLayout fl = this._cachedField.Owner;

					if (fl != null &&
						fl.HighlightAlternateRecordsResolved)
					{
						RecordPresenter rp = Utilities.GetAncestorFromType(this, typeof(RecordPresenter), true, this) as RecordPresenter;

						if (rp != null)
							return rp.IsAlternate;
					}
				}
				return false;
			}
		}

				#endregion //IsAlternate

                // JJD 5/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
		        #region IsDragIndicator

		/// <summary>
		/// Identifies the <see cref="IsDragIndicator"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDragIndicatorProperty = DataItemPresenter.IsDragIndicatorProperty.AddOwner(typeof(CellPresenter),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsDragIndicatorChanged)));

        private static void OnIsDragIndicatorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            CellPresenter cp = target as CellPresenter;

            cp._isDragIndicator = (bool)e.NewValue;

            cp.SynchronizeChildElements();
        }

		/// <summary>
		/// Indicates if the CellPresenter is part of the drag indicator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// To enable field moving functionality, set the FieldLayoutSettings'
		/// <see cref="FieldLayoutSettings.AllowFieldMoving"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.LabelPresenter.IsDragSource"/>
		//[Description( "Indicates if the data item is part of drag indicator." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsDragIndicator
		{
			get
			{
				return this._isDragIndicator;
			}
			set
			{
				this.SetValue( IsDragIndicatorProperty, value );
			}
		}

		        #endregion // IsDragIndicator

				#region IsFieldSelected

		/// <summary>
		/// Identifies the <see cref="IsFieldSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFieldSelectedProperty = CellValuePresenter.IsFieldSelectedProperty.AddOwner(
			typeof(CellPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns true if the field is selected (read-only)
		/// </summary>
		/// <seealso cref="IsFieldSelectedProperty"/>
		//[Description("Returns true if the field is selected (read-only)")]
		//[Category("Behavior")]
		public bool IsFieldSelected
		{
			get
			{
				return (bool)this.GetValue(CellPresenter.IsFieldSelectedProperty);
			}
			set
			{
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_2" ) );
				//this.SetValue(CellPresenter.IsFieldSelectedProperty, value);
			}
		}

				#endregion //IsFieldSelected

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                #region IsFixed

        private static readonly DependencyPropertyKey IsFixedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsFixed",
            typeof(bool), typeof(CellPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsFixed"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsFixedProperty =
            IsFixedPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns a boolean indicating if the field is currently fixed.
        /// </summary>
        /// <seealso cref="IsFixedProperty"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field.FixedLocation"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowFixing"/>
        //[Description("Returns a boolean indicating if the field is currently fixed.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public bool IsFixed
        {
            get
            {
                return (bool)this.GetValue(CellPresenter.IsFixedProperty);
            }
        }

                #endregion //IsFixed

				#region IsSelected

		/// <summary>
		/// Identifies the 'IsSelected' dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
			typeof(bool), typeof(CellPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsSelectedChanged), new CoerceValueCallback(CoerceIsSelected)));

		private static void OnIsSelectedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CellPresenter cp = target as CellPresenter;

			Debug.Assert(cp != null);

			if (cp != null)
			{
				// JJD 5/30/07 
				// Attempt a coerce if the property was cleared
				if (cp.ReadLocalValue(IsSelectedProperty) == DependencyProperty.UnsetValue)
				{
					bool newValue = (bool)e.NewValue;

					CoerceIsSelected(cp, KnownBoxes.FromValue(newValue));

					// JJD 5/30/07 
					// If we are still out of sync then set the dependencyproperty value back
					if (cp.IsSelected != newValue)
						cp.SetValue(IsSelectedProperty, KnownBoxes.FromValue(cp.IsSelected));
				}
			}
		}

		private static object CoerceIsSelected(DependencyObject target, object value)
		{
			CellPresenter cp = target as CellPresenter;

			Debug.Assert(cp != null);
			Debug.Assert(value is bool);

			if (cp != null)
			{
				if (cp.DataPresenter != null)
				{
					DataRecord dr = cp._dataRecord;
					Field fld = cp._cachedField;

					Debug.Assert(dr != null);
					Debug.Assert(fld != null);

					if ((bool)value == false)
					{
						// de-select the cell
						if (dr.IsCellAllocated(fld))
							dr.Cells[fld].IsSelected = false;
					}
					else
					{

						// select the cell
						if (dr != null && fld != null)
							dr.Cells[fld].IsSelected = true;
					}

					return KnownBoxes.FromValue( cp.IsSelected );
				}
			}

			return KnownBoxes.FalseBox;
		}

		/// <summary>
		/// Gets/sets if this <see cref="Infragistics.Windows.DataPresenter.Field"/> is selected.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItems"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell.IsSelected"/>
		public bool IsSelected
		{
			get
			{
				if (this._dataRecord == null || this._cachedField == null)
					return false;
				
                // JJD 1/26/08 - Optimization
                // Call IsCellAllocated on the record which will not allocate the Cells collection
                // unnecssarily
                //CellCollection cells = this._dataRecord.Cells;

                //if (cells.IsCellAllocated(this._cachedField) == false)
                //    return false;
                if (this._dataRecord.IsCellAllocated(this._cachedField) == false)
                    return false;

				return this._dataRecord.Cells[this._cachedField].IsSelected;
			}
			set
			{
				this.SetValue(CellPresenter.IsSelectedProperty, value);
			}
		}

				#endregion //IsSelected

				#region IsUnbound

		private static readonly DependencyPropertyKey IsUnboundPropertyKey =
			DependencyProperty.RegisterReadOnly("IsUnbound",
			typeof(bool), typeof(CellPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsUnbound"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsUnboundProperty =
			IsUnboundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the Field is unbound (read-only)
		/// </summary>
		/// <seealso cref="IsUnboundProperty"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field.IsUnbound"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.UnboundField"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.UnboundCell"/>
		//[Description("Returns true if the Field is unbound (read-only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsUnbound
		{
			get
			{
				return (bool)this.GetValue(CellPresenter.IsUnboundProperty);
			}
		}

				#endregion //IsUnbound

				#region Record

		private static readonly DependencyPropertyKey RecordPropertyKey =
			DependencyProperty.RegisterReadOnly("Record",
			typeof(DataRecord), typeof(CellPresenter), new FrameworkPropertyMetadata());

		/// <summary>
		/// Identifies the 'Record' dependency property
		/// </summary>
		public static readonly DependencyProperty RecordProperty =
			RecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated record (read-only)
		/// </summary>
		/// <remarks>Returns null when not used in a DataPresenterBase </remarks>
		public DataRecord Record
		{
			get
			{
				return this._dataRecord;
			}
		}

				#endregion //Record

				#region SortStatus

		private static readonly DependencyPropertyKey SortStatusPropertyKey =
			DependencyProperty.RegisterReadOnly("SortStatus",
			typeof(SortStatus), typeof(CellPresenter), new FrameworkPropertyMetadata(SortStatus.NotSorted));

		/// <summary>
		/// Identifies the 'SortStatus' dependency property
		/// </summary>
		public static readonly DependencyProperty SortStatusProperty =
			SortStatusPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the sort status of the Field (read-only)
		/// </summary>
		//[Description("Returns the sort status of the Field (read-only)")]
		//[Category("Behavior")]
		public SortStatus SortStatus
		{
			get
			{
				return (SortStatus)this.GetValue(CellPresenter.SortStatusProperty);
			}
		}

				#endregion //SortStatus

				#region Value

		/// <summary>
		/// Identifies the 'Value' dependency property
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
			// AS 10/13/09 NA 2010.1 - CardView
			//typeof(object), typeof(CellPresenter), new FrameworkPropertyMetadata());
			typeof(object), typeof(CellPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 10/13/09 NA 2010.1 - CardView
			// A cell element in a card can be collapsed based on its value.
			//
			d.CoerceValue(VisibilityProperty);
		}

		/// <summary>
		/// Gets/sets the value of the cell
		/// </summary>
		public virtual object Value
		{
			get
			{
				if (this._cachedField == null || this._dataRecord == null)
					return null;

                // JJD 5/29/09 - TFS18063 
                // Use the new overload to GetCellValue which will return the value 
                // converted into EditAsType
                //return this._cachedField.GetCellValue(this._dataRecord, true);
				return this._dataRecord.GetCellValue( this._cachedField, CellValueType.EditAsType);
			}
			set
			{
				if (this._cachedField == null || this._dataRecord == null)
					return;

				
				
				
				
				this._dataRecord.SetCellValue( this._cachedField, value, true );
			}
		}

				#endregion //Value

			#endregion //Public Properties

			#region Internal Properties

				#region Cell

		internal Cell Cell
		{
			get
			{
				if (this._cachedField == null || this._dataRecord == null)
					return null;

				return this._dataRecord.Cells[this._cachedField];
			}
		}

				#endregion //Cell

				#region CellValuePresenter
		internal CellValuePresenter CellValuePresenter
		{
			get
			{
				return this._layoutElement != null
					? this._layoutElement.CellValuePresenter
					: null;
			}
		} 
				#endregion //CellValuePresenter

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                #region IsFixedInternal

        private static readonly DependencyProperty IsFixedInternalProperty = DependencyProperty.Register("IsFixedInternal",
            typeof(bool), typeof(CellPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

                #endregion //IsFixedInternal

				// AS 3/13/07 BR21065
				#region LabelPresenter
		internal LabelPresenter LabelPresenter
		{
			get
			{
				return this._layoutElement != null
					? this._layoutElement.LabelPresenter
					: null;
			}
		} 
				#endregion //LabelPresenter

				#region SortStatusInternal

		private static readonly DependencyProperty SortStatusInternalProperty = DependencyProperty.Register("SortStatusInternal",
			typeof(SortStatus), typeof(CellPresenter), new FrameworkPropertyMetadata(SortStatus.NotSorted));

		private SortStatus SortStatusInternal
		{
			get
			{
				return (SortStatus)this.GetValue(CellPresenter.SortStatusInternalProperty);
			}
			set
			{
				this.SetValue(CellPresenter.SortStatusInternalProperty, value);
			}
		}

				#endregion //SortStatusInternal

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region OnRecordResized

		internal void OnRecordResized()
		{
			this.InvalidateMeasureWithChildren();
		}

				#endregion //OnRecordResized	
    
			#endregion //Internal Methods

			#region Private Methods

				#region InitializeRecord

		private void InitializeRecord()
		{
			DataRecord rcd = this.DataContext as DataRecord;

			if (rcd == this._dataRecord)
				return;

			if (this._dataRecord != null)
				this.UnhookAndClearRecord();

			this._dataRecord = rcd;

			if (this._dataRecord == null)
				return;

			// use the weak event manager to hook the event so we don't get rooted
			PropertyChangedEventManager.AddListener(this._dataRecord, this, string.Empty);

			this.SetValue(RecordPropertyKey, this._dataRecord);
			// JJD 5/30/07 
			// Moved to measure override
			//			this.SetValue(RecordPresenter.IsAlternatePropertyKey, KnownBoxes.FromValue(this.IsAlternate));
			//			this.SetValue(IsActiveProperty, KnownBoxes.FromValue( this._dataRecord.IsActive ));

			if (this._cachedField != null)
				this.SetValue(ValueProperty, this.Value);

			// AS 1/27/10 NA 2010.1 - CardView
			// The value may not change but the associated record's should collapse cells may be different.
			// Technically the Field's visibility/contentalignment/etc could be different if recycled with 
			// a different cell.
			//
			if (_dataRecord.FieldLayout.IsEmptyCellCollapsingSupportedByView)
				this.CoerceValue(VisibilityProperty);

			this.InvalidateStyleSelector();

            // AS 1/26/09 TFS13026
            this.InvalidateLayoutElement();
        }

				#endregion //InitializeRecord	
		
				#region InvalidateMeasureWithChildren

		private void InvalidateMeasureWithChildren()
		{
			this.InvalidateMeasure();

			if (this._layoutElement != null)
			{
				this._layoutElement.InvalidateMeasure();

				if (this._layoutElement.CellValuePresenter != null)
					this._layoutElement.CellValuePresenter.InvalidateMeasure();

				if (this._layoutElement.LabelPresenter != null)
					this._layoutElement.LabelPresenter.InvalidateMeasure();
			}
		}

				#endregion //InvalidateMeasureWithChildren	
    
				#region InvalidateStyleSelector

		/// <summary>
		/// Called to invalidate the style
		/// </summary>
		protected void InvalidateStyleSelector()
		{
			// JJD 5/30/07
			// Only invalidate the style if we have both a field and a record set
			//if (this._styleSelectorHelper != null && this.Field != null)
			if (this._styleSelectorHelper != null && this._cachedField != null && this._dataRecord != null)
				this._styleSelectorHelper.InvalidateStyle();
		}

				#endregion //InvalidateStyleSelector
    
				#region OnRecordPropertyChanged

		private void OnRecordPropertyChanged(PropertyChangedEventArgs e)
		{
			if (this._dataRecord != null)
			{
				switch (e.PropertyName)
				{
					case "ActiveCell":
						this.SetValue(IsActiveProperty, KnownBoxes.FromValue(this.IsActive));
						break;

					case "CellSelection":
						this.SetValue(IsSelectedProperty, KnownBoxes.FromValue(this.IsSelected));
						break;

					// AS 10/27/09 NA 2010.1 - CardView
					case "ShouldCollapseEmptyCells":
						this.CoerceValue(VisibilityProperty);
						break;
				}
			}
		}

				#endregion //OnRecordPropertyChanged

				#region UnhookAndClearRecord

		private void UnhookAndClearRecord()
		{
			// unhook from old datarecord
			if (this._dataRecord != null)
			{
				// unhook the event listener for the old record
				PropertyChangedEventManager.RemoveListener(this._dataRecord, this, string.Empty);
				this._dataRecord = null;
			}
		}

				#endregion //UnhookAndClearRecord

			#endregion //Private Methods

		#endregion //Methods

		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					this.OnRecordPropertyChanged(args);
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for DataItemSelector, arg type: " + e != null ? e.ToString() : "null");
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for DataItemSelector, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion

		#region CellPresenterStyleSelectorHelper private class

		private class CellPresenterStyleSelectorHelper : StyleSelectorHelperBase
		{
			private CellPresenter _cellPresenter;

			internal CellPresenterStyleSelectorHelper(CellPresenter cellPresenter)
				: base(cellPresenter)
			{
				this._cellPresenter = cellPresenter;
			}

			/// <summary>
			/// The style to be used as the source of a binding (read-only)
			/// </summary>
			public override Style Style
			{
				get
				{
					if (this._cellPresenter == null)
						return null;

					Field field = this._cellPresenter._cachedField;

					if (field == null)
						return null;

					FieldLayout fl = field.Owner;

					if (fl == null)
						return null;

					DataPresenterBase layoutOwner = fl.DataPresenter;

					if (layoutOwner == null)
						return null;

					return layoutOwner.InternalCellPresenterStyleSelector.SelectStyle(((CellPresenter)(this._cellPresenter)).Value, this._cellPresenter);
				}
			}
		}

		#endregion //CellPresenterStyleSelectorHelper private class
	}

	#endregion //CellPresenter
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