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
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
	#region DataItemPresenter abstract base class

	/// <summary>
	/// Abstract base class for CellPresenter and LabelPresenter
	/// </summary>

    // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,            GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,              GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,           GroupName = VisualStateUtilities.GroupCommon)]


	public abstract class DataItemPresenter : ValuePresenter, IResizableElement, IWeakEventListener
	{
		#region Private Members

		private CellStyleSelectorHelper _styleSelectorHelper;
		private DataRecord _dataRecord;


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;



		#endregion //Private Members

		#region Constructors

		/// <summary>
		/// Static constructor
		/// </summary>
		static DataItemPresenter()
		{
            // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
            DataItemPresenter.ClipProperty.OverrideMetadata(typeof(DataItemPresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(VirtualizingDataRecordCellPanel.CoerceCellClip)));

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(DataItemPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DataItemPresenter"/> class
		/// </summary>
		protected DataItemPresenter()
		{
            // initialize the styleSelectorHelper
            this._styleSelectorHelper = new CellStyleSelectorHelper(this);
	}

		#endregion //Constructors

		#region Base class overrides

			#region HitTestCore

		// JJD 3/14/07 - BR21081
		// Overrode HitTestCore to make sure the the element gets mouse messages regardless of whether
		// its background is transparent or not.
		// 
		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="hitTestParameters"></param>
		/// <returns></returns>
		protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
		{
			Rect rect = new Rect( new Point( ), this.RenderSize );
			if ( rect.Contains( hitTestParameters.HitPoint ) )
				return new PointHitTestResult( this, hitTestParameters.HitPoint );

			return base.HitTestCore( hitTestParameters );
		}

			#endregion // HitTestCore
		
			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			// ensure that the field property has been initialized
			if (this.Field == null)
			{
				CellPresenter cp = Utilities.GetAncestorFromType(this, typeof(CellPresenter), false, null, typeof(DataRecordPresenter)) as CellPresenter;

				if (cp != null)
					this.Field = cp.Field;
			}

			return base.MeasureOverride(availableSize);
		}

			#endregion //MeasureOverride

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
        /// it to get the focus site from the control template whenever template gets applied to the control.
        /// </p>
        /// </remarks>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

            #endregion //OnApplyTemplate	
    
            #region OnMouseEnter
        /// <summary>
        /// Invoked when the mouse is moved within the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
            #endregion //OnMouseEnter

            #region OnMouseLeave
        /// <summary>
        /// Invoked when the mouse is moved outside the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
            #endregion //OnMouseLeave

			// MD 7/16/10 - TFS26592
			#region ShouldSuppressEvent

		/// <summary>
		/// Determines whether the event associated with the specified event arguments should be suppressed.
		/// </summary>
		/// <param name="args">The event arguments which will be fired with the event if it is not suppressed.</param>
		/// <returns>True if the event should be suppressed; False otherwise.</returns>
		protected internal override bool ShouldSuppressEvent(RoutedEventArgs args)
		{
			if (_dataRecord != null)
			{
				DataPresenterBase dataPresenter = _dataRecord.DataPresenter;
				if (dataPresenter != null && dataPresenter.IsEventSuppressed(args.RoutedEvent))
					return true;
			}

			return base.ShouldSuppressEvent(args);
		} 

			#endregion // ShouldSuppressEvent

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property is changed
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			DependencyProperty property = e.Property;

			if (property == DataItemPresenter.DataContextProperty)
			{
				DataRecord rcd = this.DataContext as DataRecord;

				if (rcd != null)
				{
					this._dataRecord = rcd;
					this.SetValue(RecordPropertyKey, this.Record);

					// AS 3/22/07 BR21259
					// We don't want the cell to show the isalternate state unless its within a record.
					//
					//this.SetValue(RecordPresenter.IsAlternatePropertyKey, KnownBoxes.FromValue(this.IsAlternate));
					if (this.IsWithinRecord)
						this.SetValue(RecordPresenter.IsAlternatePropertyKey, KnownBoxes.FromValue(this.IsAlternate));
					else
						this.ClearValue(RecordPresenter.IsAlternatePropertyKey);
				}
				else
				{
					this.ClearValue(RecordPropertyKey);
					this.ClearValue(RecordPresenter.IsAlternatePropertyKey);
				}
			}
			else if (property == DataItemPresenter.StyleProperty)
			{
				// AS 3/22/07 BR21259
				// We don't want a cell showing the highlight as primary state unless its in a record.
				//  
				//this.SetValue(HighlightAsPrimaryPropertyKey, KnownBoxes.FromValue(this.HighlightAsPrimary));
				if (this.IsWithinRecord)
					this.SetValue(HighlightAsPrimaryPropertyKey, KnownBoxes.FromValue(this.HighlightAsPrimary));
					else
						this.ClearValue(RecordPresenter.IsAlternatePropertyKey);
			}
			else if (property == DataItemPresenter.FieldProperty)
			{
				// unhook from the old
				if (this._cachedField != null)
				{
					// unhook the event listener for the old field
					PropertyChangedEventManager.RemoveListener(this._cachedField, this, string.Empty);
				}

				this._cachedField = e.NewValue as Field;

				// listen for property change notification
				if (this._cachedField != null)
				{
					if (this._cachedField.IsUnbound)
						this.SetValue(IsUnboundPropertyKey, KnownBoxes.TrueBox);

					FieldLayout fl = this._cachedField.Owner;

					if (fl != null)
						this.SetValue(HasSeparateHeaderProperty, KnownBoxes.FromValue(fl.HasSeparateHeader));

					// use the weak event manager to hook the event so we don't get rooted
					PropertyChangedEventManager.AddListener(this._cachedField, this, string.Empty);
				}

				// AS 3/22/07 BR21259
				// We don't want a cell showing the highlight as primary state unless its in a record.
				//
				//this.SetValue(HighlightAsPrimaryPropertyKey, KnownBoxes.FromValue(this.HighlightAsPrimary));
				if (this.IsWithinRecord)
					this.SetValue(HighlightAsPrimaryPropertyKey, KnownBoxes.FromValue(this.HighlightAsPrimary));
				else
					this.ClearValue(HighlightAsPrimaryPropertyKey);

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                // Manage the IsFixed state of the field.
                //
                this.SetValue(IsFixedPropertyKey, null != this._cachedField && this._cachedField.IsFixed 
                    ? KnownBoxes.TrueBox 
                    : DependencyProperty.UnsetValue);


                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                this.UpdateVisualStates(false);


			}
		}

			#endregion //OnPropertyChanged

			#region OnValueEditorKeyDown

		/// <summary>
		/// Overridden. Called by the ValueEditor before it process its OnKeyDown. Default 
		/// implementation returns false. You can override and return true from this method 
		/// if you want to prevent the ValueEditor from processing the key.
		/// </summary>
		/// <param name="e"></param>
		/// <returns>Return true to prevent the value editor from processing the key.</returns>
		internal protected override bool OnValueEditorKeyDown( KeyEventArgs e )
		{
			
			// This method is called by the ValueEditor to check whether it should process
			// the key. Here we should check if the key is one of the keys that we want
			// to process rather than the editor and return true from here. Processing the
			// key is actually optional. Returning true from here will cause the editor
			// to bypass its key processing.

			return false;
		}

			#endregion // OnValueEditorKeyDown

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region DataPresenterBase

		/// <summary>
		/// Returns the associated <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> (read-only)
		/// </summary>
		public DataPresenterBase DataPresenter
		{
			get
			{
				Field fld = this.Field;

				if (fld != null)
					return fld.Owner.DataPresenter;

				return null;
			}
		}

		#endregion //DataPresenterBase

		#region Field

		/// <summary>
		/// Identifies the 'Field' dependency property
		/// </summary>
		public static readonly DependencyProperty FieldProperty = DependencyProperty.Register("Field",
				  typeof(Field), typeof(DataItemPresenter), new FrameworkPropertyMetadata());

		private Field _cachedField = null;

		/// <summary>
		/// The associated field.
		/// </summary>
		public Field Field
		{
			get
			{
				return this._cachedField;
			}
			set
			{
				this.SetValue(DataItemPresenter.FieldProperty, value);
			}
		}

		#endregion //Field

		#region HasSeparateHeader

		/// <summary>
		/// Identifies the 'HasSeparateHeader' dependency property
		/// </summary>
		public static readonly DependencyProperty HasSeparateHeaderProperty = DependencyProperty.Register("HasSeparateHeader",
			typeof(bool), typeof(DataItemPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns true if the header area is separate
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasSeparateHeader
		{
			get
			{
				return (bool)this.GetValue(DataItemPresenter.HasSeparateHeaderProperty);
			}
			set
			{
				this.SetValue(DataItemPresenter.HasSeparateHeaderProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //HasSeparateHeader

		#region HighlightAsPrimary

		private static readonly DependencyPropertyKey HighlightAsPrimaryPropertyKey =
			DependencyProperty.RegisterReadOnly("HighlightAsPrimary",
			typeof(bool), typeof(DataItemPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the 'HighlightAsPrimary' dependency property
		/// </summary>
		public static readonly DependencyProperty HighlightAsPrimaryProperty =
			HighlightAsPrimaryPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the data item is associated with a <see cref="Field"/> that has been marked as a primary <see cref="Field"/>. 
		/// </summary>
		/// <seealso cref="Field"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field.IsPrimary"/>
		//[Description("Returns true if the data item is associated with a Field that has been marked as a primary Field")]
		//[Category("Behavior")]
		public bool HighlightAsPrimary
		{
			get
			{
				if (this._cachedField == null)
					return false;

				if (!this._cachedField.IsPrimary)
					return false;

				return this._cachedField.Owner.HighlightPrimaryFieldResolved;
			}
		}

		#endregion //HighlightAsPrimary

		#region IsAlternate

		/// <summary>
		/// Identifies the 'IsAlternate' dependency property
		/// </summary>
		public static readonly DependencyProperty IsAlternateProperty =
			RecordPresenter.IsAlternatePropertyKey.DependencyProperty.AddOwner(typeof(DataItemPresenter));

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

		#region IsDragIndicator



		
		
		/// <summary>
		/// Identifies the <see cref="IsDragIndicator"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDragIndicatorProperty = DependencyProperty.Register(
				"IsDragIndicator",
				typeof( bool ),
				typeof( DataItemPresenter ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox 

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));


		/// <summary>
		/// Indicates if the data item is part of drag indicator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// To enable field moving functionality, set the FieldLayoutSettings'
		/// <see cref="FieldLayoutSettings.AllowFieldMoving"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
		/// <seealso cref="LabelPresenter.IsDragSource"/>
		//[Description( "Indicates if the data item is part of drag indicator." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsDragIndicator
		{
			get
			{
				return (bool)this.GetValue( IsDragIndicatorProperty );
			}
			set
			{
				this.SetValue( IsDragIndicatorProperty, value );
			}
		}


		#endregion // IsDragIndicator

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        #region IsFixed

        private static readonly DependencyPropertyKey IsFixedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsFixed",
            typeof(bool), typeof(DataItemPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

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
                return (bool)this.GetValue(DataItemPresenter.IsFixedProperty);
            }
        }

        #endregion //IsFixed

		#region IsUnbound

		private static readonly DependencyPropertyKey IsUnboundPropertyKey =
			DependencyProperty.RegisterReadOnly("IsUnbound",
			typeof(bool), typeof(DataItemPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

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
				return (bool)this.GetValue(DataItemPresenter.IsUnboundProperty);
			}
		}

		#endregion //IsUnbound

		#region Record

		private static readonly DependencyPropertyKey RecordPropertyKey =
			DependencyProperty.RegisterReadOnly("Record",
			typeof(DataRecord), typeof(DataItemPresenter), new FrameworkPropertyMetadata());

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

		#endregion //Public Properties

		#region Private Properties

		#endregion //Private Properties

		#region Internal Properties

		// AS 3/22/07 BR21259
		// We needed to know if the cellvaluepresenter or labelpresenter were used within
		// a recordpresenter (specifically a labelarea or datacellarea) or externally (e.g.
		// in a record scroll tip) because we don't want to associate certain state with
		// the element in that case. I.e. it should not show as selected, active, alternate, etc.
		// from within the record scroll tip.
		//
		#region IsWithinRecord
		internal bool IsWithinRecord
		{
			get
			{
				DependencyObject templateParent = this.TemplatedParent;

				
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

				if (null != templateParent)
				{
					do
					{
						// if this is within a data record cell area, its ok
						if (templateParent is DataRecordCellArea ||
							templateParent is HeaderLabelArea ||
							templateParent is DataRecordPresenter ||
							templateParent is HeaderPresenter)
							return true;

						if (templateParent is FrameworkElement)
							templateParent = ((FrameworkElement)templateParent).TemplatedParent;
						else
							break;
					} while (templateParent != null);

					// cell value presenters must be created within the record
					return false;
				}

				DependencyObject parent = VisualTreeHelper.GetParent(this);

				if (parent == null)
					return true;

				// virtualized cell area is within a record
				if (parent is VirtualizingDataRecordCellPanel)
					return true;

				// within a cell presenter layout
				// SSP 4/8/08 - Summaries Functionality
				// Created CellPresenterLayoutElementBase class from which CellPresenterLayoutElement and
				// SummaryCellPresenterLayoutElement derive. This logic applies to both.
				// 
				//if (parent is CellPresenterLayoutElement)
				if ( parent is CellPresenterLayoutElementBase )
					return true;

				return false;
			}
		} 
		#endregion //IsWithinRecord

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Protected methods

                #region VisualState... Methods


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            if (this.IsEnabled == false)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else
            {
                if (this.IsMouseOver)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            }
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            DataItemPresenter dip = target as DataItemPresenter;

            if ( dip != null )
                dip.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



                #endregion //VisualState... Methods	

			#endregion //Protected methods

			#region InvalidateStyleSelector

		/// <summary>
		/// Called to invalidate the style
		/// </summary>
		protected void InvalidateStyleSelector()
		{
			if (this.Field != null && this._styleSelectorHelper != null)
			{
				this._styleSelectorHelper.InvalidateStyle();

				// JJD 7/05/11 - TFS80466 
				// Call a virtual method to let derived classes know when the style was invalidated
				this.OnStyleSelectorInvalidated();
			}
		}

		// JJD 7/05/11 - TFS80466 - added
		internal virtual void OnStyleSelectorInvalidated()
		{
		}

			#endregion //InvalidateStyleSelector

			#region OnFieldPropertyChanged

		/// <summary>
		/// Called when a property on the associated <see cref="Field"/> object changes
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnFieldPropertyChanged(PropertyChangedEventArgs e)
		{
            switch (e.PropertyName)
            {
                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                case "IsFixed":
                    this.SetValue(IsFixedPropertyKey, this._cachedField == null || false == this._cachedField.IsFixed 
                        ? DependencyProperty.UnsetValue 
                        : KnownBoxes.TrueBox);
                    break;


                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                case "IsSelected":
                    this.UpdateVisualStates();
                    break;


            }
		}

			#endregion //OnFieldPropertyChanged

			#region OnRecordPropertyChanged

		/// <summary>
		/// Called when a property on the associated <see cref="Record"/> object changes
		/// </summary>
		/// <param name="e">This is used only by derived classes where Record makes sense (i.e. CellValuePresenter)</param>
		protected virtual void OnRecordPropertyChanged(PropertyChangedEventArgs e)
		{
		}

			#endregion //OnFieldPropertyChanged

		#endregion //Methods

		#region CellPresenterStyleSelectorHelper private class

		private class CellStyleSelectorHelper : StyleSelectorHelperBase
		{
			private DataItemPresenter _cellBase;

			internal CellStyleSelectorHelper(DataItemPresenter cellBase)
				: base(cellBase)
			{
				this._cellBase = cellBase;
			}

			/// <summary>
			/// The style to be used as the source of a binding (read-only)
			/// </summary>
			public override Style Style
			{
				get
				{
					if (this._cellBase == null)
						return null;

					Field field = this._cellBase.Field;

					if (field == null)
						return null;

					FieldLayout fl = field.Owner;

					if (fl == null)
						return null;

					DataPresenterBase layoutOwner = fl.DataPresenter;

					if (layoutOwner == null)
						return null;

					if (this._cellBase is LabelPresenter)
						return layoutOwner.InternalLabelPresenterStyleSelector.SelectStyle(this._cellBase.Content, this._cellBase);

					if (this._cellBase is ExpandedCellPresenter)
						return layoutOwner.InternalExpandedCellStyleSelector.SelectStyle(((ExpandedCellPresenter)(this._cellBase)).Value, this._cellBase);

					if (this._cellBase is FilterCellValuePresenter)
                        return field.FilterCellValuePresenterStyleResolved;
					else
                    {   
                        // JJD 11/12/09 - TFS24752
                        // Use the cached cell value so we don't call the get accessor
                        // on the data item multiple times
                        //return layoutOwner.InternalCellValuePresenterStyleSelector.SelectStyle(((CellValuePresenter)(this._cellBase)).Value, this._cellBase);
                        return layoutOwner.InternalCellValuePresenterStyleSelector.SelectStyle(this._cellBase.GetValue(CellValuePresenter.ValueProperty), this._cellBase);
                    }
				}
			}
		}

		#endregion //CellPresenterStyleSelectorHelper private class

		#region IResizableElement Members

		object IResizableElement.ResizeContext
		{
			get { return this._cachedField; }
		}

		#endregion

		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					if ( sender is Field )
						this.OnFieldPropertyChanged(args);
					else if (sender is Record)
						this.OnRecordPropertyChanged(args);
					else
					{
						Debug.Fail("Invalid sender in ReceiveWeakEvent for DataItemSelector, sender: " + sender != null ? sender.ToString() : "null");
						return false;
					}
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for DataItemSelector, arg type: " + e != null ? e.ToString() : "null");
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for DataItemSelector, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion
	}

	#endregion //DataItemPresenter abstract base class

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