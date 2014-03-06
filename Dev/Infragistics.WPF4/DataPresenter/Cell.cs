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
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Selection;
using Infragistics.Windows.DataPresenter.Events;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using Infragistics.Windows.Internal;
using System.Collections.ObjectModel;

namespace Infragistics.Windows.DataPresenter
{
	#region Cell class

	/// <summary>
	/// Class used by the to represent the value of a specific Field of a specific DataRecord.
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note: </b>Note: Cells are not UIElements but rather lightweight objects that wrap the values from the data source for an associated <see cref="Infragistics.Windows.DataPresenter.Field"/> for a specific <see cref="DataRecord"/>. They are represented in the UI by corresponding <see cref="CellValuePresenter"/> elements.</para>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</p>
	/// <p class="body">Refer to the <a href="xamData_Cells_CellValuePresenters_and_Cell_Virtualization.html">Cells, CellValuePresenters and Cell Virtualization</a> topic in the Developer's Guide for an explanation of cells.</p>
	/// </remarks>
	/// <seealso cref="CellValuePresenter"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
	/// <seealso cref="DataRecord"/>
	/// <seealso cref="DataRecord.Cells"/>
	// JJD 4/26/07
	// Optimization - replaced sparse array in CellsCollection
	//public class Cell : PropertyChangeNotifier, ISparseArrayItem, ISelectableItem
	public class Cell : PropertyChangeNotifier, ISelectableItem
	{
		#region Private Members

		private DataRecord _record;
		private Field _field;
		private Type _editAsType;
		private Type _editorType;
		private Style _editorStyle;
		private bool _selected;
		private bool _isDataChanged;
		private bool _isRaisingPropChangeNotifactions; // JJD 3/7/08 - added anti-recursion flag

		// AS 4/29/09 TFS17168
		//private WeakReference _associatedCellValuePresenter;
		private static readonly WeakReference UninitializedCVP = new WeakReference(null);
		private WeakReference _associatedCellValuePresenter = UninitializedCVP;

        // JJD 2/20/08
        // Added Tag property
        private object _tag;

		//internal object _sparseArrayOwnerData = null;

		// JM 6/12/09 NA 2009.2 DataValueChangedEvent
		private DataValueInfoHistory		_dataValueInfoHistoryCache;

		// AS 7/31/09 NA 2009.2 Field Sizing
		private int _fieldAutoSizeVersion;

		#endregion //Private Members

		#region Constructor

		internal Cell(DataRecord record, Field field)
		{
			if (record == null)
				throw new ArgumentNullException("record");

			if (field == null)
				throw new ArgumentNullException("field");

			this._record = record;
			this._field = field;
		}

		#endregion //Constructor

		#region Base class overrides

		#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Cell: ");
			sb.Append(this.Field.ToString());
			sb.Append(", ");
			sb.Append(this.Record.ToString());

			return sb.ToString();
		}

		#endregion //ToString

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

                // JJD 3/5/08 - added support for Converter properies on Field
				#region ConvertedValue

		/// <summary>
		/// Gets/sets the converted value of the cell.
		/// </summary>
        /// <value>The converted value based on the <see cref="Infragistics.Windows.DataPresenter.Field"/>'s <see cref="Infragistics.Windows.DataPresenter.Field.Converter"/> and its <see cref="Infragistics.Windows.DataPresenter.Field.EditAsTypeResolved"/>.</value>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field.Converter"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field.ConverterCulture"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field.ConverterParameter"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field.DataType"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field.EditAsTypeResolved"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.GetCellValue(Field, bool)"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.SetCellValue(Field, object, bool)"/>
		public virtual object ConvertedValue
		{
			get
			{
				
				
				
				
				return this._record.GetCellValue( this._field, true );
			}
			set
			{
				
				
				
                
				this._record.SetCellValue( this._field, value, true );
			}
		}

				#endregion //ConvertedValue

				#region DataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Returns the field data error as indicated by the underlying data item's IDataErrorInfo implementation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// A data item can provide error information regarding the data item and individual fields
		/// of the data item by implementing IDataErrorInfo interface. If the data item associated with
		/// the data record implements IDataErrorInfo, this property returns the field error if any 
		/// for this cell - basically the value returned by the IDataErrorInfo's Item[fieldName] indexer.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the corresponding <see cref="Cell.HasDataError"/> property returns the
		/// value indicating whether there's a data error on this cell.
		/// </para>
		/// <para class="body">
		/// Also <b>Note</b> that by default the data presenter doesn't display the data error information. 
		/// To have the data presenter display the data error information, set the
		/// FieldLayoutSettings' <see cref="FieldLayoutSettings.SupportDataErrorInfo"/> property.
		/// </para>
		/// <seealso cref="Cell.HasDataError"/>
		/// <seealso cref="DataRecord.GetCellDataError"/>
		/// <seealso cref="DataRecord.GetCellHasDataError"/>
		/// <seealso cref="DataRecord.HasDataError"/>
		/// <seealso cref="DataRecord.DataError"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="FieldSettings.SupportDataErrorInfo"/>
		/// </remarks>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public object DataError
		{
			get
			{
				return _record.GetCellDataError( _field );
			}
		}

				#endregion // DataError

				#region DataPresenterBase

		/// <summary>
		/// Returns the associated <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> (read-only)
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DataPresenterBase DataPresenter
		{
			get
			{
				return this._field.Owner.DataPresenter;
			}
		}

				#endregion //DataPresenterBase

				#region EditAsType

		/// <summary>
		/// Gets/sets a type that will be used to edit the data while in edit mode.
		/// </summary>
		/// <remarks>
		/// <para class="body">This might be used with a cell whose underlying datatype is 'string' or 'object' but where it should be edited as another type, e.g. a DataTime or double.</para>
		/// <p class="body">Refer to the <a href="xamData_Terms_Editors.html">Editors</a> topic in the Developer's Guide for an explanation of editors.</p>
		/// <p class="body">Refer to the <a href="xamData_Editing_Cell_Values.html">Editing Cell Values</a> topic in the Developer's Guide for an explanation of how editing works.</p>
		/// <para class="body">
		/// <b>Note</b> that if you need to set more than one of <see cref="EditAsType"/>, <see cref="EditorType"/>
		/// and <see cref="EditorStyle"/> properties, you may want to use the <see cref="SetEditorSettings"/> method
		/// to set them all at once for better performance as it updates the CellValuePresenter associated with the 
		/// cell only once.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldSettings.EditAsType"/>
		/// <seealso cref="Cell.SetEditorSettings"/>
		public Type EditAsType
		{
			get
			{
				return this._editAsType;
			}
			set
			{
				if (value != this._editAsType)
				{
					this._editAsType = value;
					this.RaisePropertyChangedEvent("EditAsType");

					// SSP 10/20/09 TFS23589
					// When a cell's EditorStyle, EditorType or EditAsType changes we need to make sure
					// that the associated cell value presenter re-creates/re-initializes its editor 
					// based on the new value of the property.
					// 
					this.ReInitializeCVPEditor( );
				}
			}
		}

				#endregion //EditAsType	

				#region EditorType

		/// <summary>
		/// The type for the <see cref="ValueEditor"/> used within this cell
		/// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamData_Terms_Editors.html">Editors</a> topic in the Developer's Guide for an explanation of editors.</p>
		/// <p class="body">Refer to the <a href="xamData_Editing_Cell_Values.html">Editing Cell Values</a> topic in the Developer's Guide for an explanation of how editing works.</p>
		/// <para class="body">
		/// <b>Note</b> that if you need to set more than one of <see cref="EditAsType"/>, <see cref="EditorType"/>
		/// and <see cref="EditorStyle"/> properties, you may want to use the <see cref="SetEditorSettings"/> method
		/// to set them all at once for better performance as it updates the CellValuePresenter associated with the 
		/// cell only once.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldSettings.EditorType"/>
		/// <seealso cref="Cell.SetEditorSettings"/>
		public Type EditorType
		{
			get
			{
				return this._editorType;
			}
			set
			{
				if (value != this._editorType)
				{
					this._editorType = value;
					this.RaisePropertyChangedEvent("EditorType");

					// SSP 10/20/09 TFS23589
					// When a cell's EditorStyle, EditorType or EditAsType changes we need to make sure
					// that the associated cell value presenter re-creates/re-initializes its editor 
					// based on the new value of the property.
					// 
					this.ReInitializeCVPEditor( );
				}
			}
		}

				#endregion //EditorType	

				#region EditorStyle

		/// <summary>
		/// The style for the <see cref="ValueEditor"/> used within this cell
		/// </summary>
		/// <remarks>
		/// <para class="body"> The TargetType of the style must me set to a class derived from <see cref="ValueEditor"/>, otherwise an exception will be thrown.</para>
		/// <para class="body">If either the <see cref="ValueEditor.EditTemplate"/> or <b>Template</b> properties are not set on the style then default templates will be supplied based on look.</para>
		/// <p class="body">Refer to the <a href="xamData_Terms_Editors.html">Editors</a> topic in the Developer's Guide for an explanation of editors.</p>
		/// <p class="body">Refer to the <a href="xamData_Editing_Cell_Values.html">Editing Cell Values</a> topic in the Developer's Guide for an explanation of how editing works.</p>
		/// <para class="body">
		/// <b>Note</b> that if you need to set more than one of <see cref="EditAsType"/>, <see cref="EditorType"/>
		/// and <see cref="EditorStyle"/> properties, you may want to use the <see cref="SetEditorSettings"/> method
		/// to set them all at once for better performance as it updates the CellValuePresenter associated with the 
		/// cell only once.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldSettings.EditorStyle"/>
		/// <seealso cref="Cell.SetEditorSettings"/>
		public Style EditorStyle
		{
			get
			{
				return this._editorStyle;
			}
			set
			{
				if (value != this._editorStyle)
				{
					this._editorStyle = value;
					this.RaisePropertyChangedEvent("EditorStyle");

					// SSP 10/20/09 TFS23589
					// When a cell's EditorStyle, EditorType or EditAsType changes we need to make sure
					// that the associated cell value presenter re-creates/re-initializes its editor 
					// based on the new value of the property.
					// 
					this.ReInitializeCVPEditor( );
				}
			}
		}

				#endregion //EditorStyle	
    
				#region Field

		/// <summary>
		/// Returns the associated <see cref="Infragistics.Windows.DataPresenter.Field"/> (read-only)
		/// </summary>
		public Field Field { get { return this._field; } }

				#endregion //Field

				#region HasDataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Indicates if the cell has a data error as indicated by the underlying data item's 
		/// IDataErrorInfo implementation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasDataError</b> property indicates whether the cell has data error. Cell's <see cref="Cell.DataError"/>
		/// property returns the actual data error if any.
		/// </para>
		/// <seealso cref="Cell.DataError"/>
		/// <seealso cref="DataRecord.GetCellDataError"/>
		/// <seealso cref="DataRecord.GetCellHasDataError"/>
		/// <seealso cref="DataRecord.DataError"/>
		/// <seealso cref="DataRecord.HasDataError"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="FieldSettings.SupportDataErrorInfo"/>
		/// </remarks>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool HasDataError
		{
			get
			{
				return _record.GetCellHasDataError( _field );
			}
		}

				#endregion // HasDataError

				#region IsActive

		/// <summary>
		/// Gets/sets whether this cell is set as the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>'s <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/> 
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellActivating"/> 
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellActivated"/> 
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellDeactivating"/> 
		public bool IsActive
		{
			get
			{
				DataPresenterBase dp = this.DataPresenter;
				if (dp == null)
					return false;

				return dp.ActiveCell == this;
			}
			set
			{
				if (this.IsActive != value)
				{
					DataPresenterBase dp = this.DataPresenter;
					if (dp == null)
						throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_5" ) );

					if (value == true)
						this.DataPresenter.ActiveCell = this;
					else
					{
						if (this == this.DataPresenter.ActiveCell)
							this.DataPresenter.ActiveCell = null;
					}
				}
			}
		}

				#endregion //IsActive

				#region IsDataChanged

		/// <summary>
		/// Returns true if the cell data has been edited and have not yet been commited to the data source.
		/// </summary>
		public bool IsDataChanged 
		{ 
			get 
			{
				if (this._isDataChanged)
					return true;

				// if this is the active cell and it is in edit mode
				// then return true if the user has edited the value
				if (this.IsActive)
				{
					CellValuePresenter cvp = this.AssociatedCellValuePresenter;
					if (cvp != null &&
						cvp.Editor != null)
						return cvp.Editor.HasValueChanged;
				}

				return false;
			} 
		}

			#endregion //IsDataChanged	

				#region IsInEditMode

		/// <summary>
		/// Returns true if the cell is in edit mode.
		/// </summary>
		public bool IsInEditMode 
		{ 
			get 
			{
				CellValuePresenter cvp = this.AssociatedCellValuePresenter;

				if (cvp != null)
					return cvp.IsInEditMode;

				return false; 
			}
		}

				#endregion //IsInEditMode

				#region IsSelected

		/// <summary>
		/// Gets/sets whether the cell is selected.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItems"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder.Cells"/>
		/// <seealso cref="SelectedCellCollection"/>
		public bool IsSelected
		{
			get
			{
				return this._selected;
			}
			set
			{
				if (this._selected != value)
				{
					DataPresenterBase dp = this.DataPresenter;

					if (null != dp)
					{
						bool clearExistingItems = true;

						SelectionStrategyBase selectionStrategy = this.Field.Owner.GetSelectionStrategyForItem(this);

						if (null != selectionStrategy)
							clearExistingItems = selectionStrategy.IsSingleSelect;

						// Also clear the selected rows as rows and cells are mutually 
						// exclusive for selection. If the user cancels the selection
						// then return without selecting the row.
						//
						if (dp.ClearSelectedRecords()==false)
							return;

						// JM 02-05-09 TFS12744
						//dp.InternalSelectItem(this, clearExistingItems, value);
						bool selected = dp.InternalSelectItem(this, clearExistingItems, value);
						if (selected && dp.SelectedItems.Cells.Count == 1)
							((ISelectionHost)dp).SetPivotItem(this, false);
					}
				}
			}
		}

				#endregion //IsSelected

				#region IsUnbound

		/// <summary>
		/// Returns true if this is an UnboundField (read-only)
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field.IsUnbound"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.UnboundField"/>
		/// <seealso cref="UnboundCell"/>
		public virtual bool IsUnbound
		{
			get
			{
				return false;
			}
		}

				#endregion //IsUnbound

				#region Record

		/// <summary>
		/// Returns the associated <see cref="DataRecord"/> (read-only)
		/// </summary>
		public DataRecord Record { get { return this._record; } }

				#endregion //Record

                // JJD 2/20/08
                // Added Tag property
                #region Tag

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information about this object.
        /// </summary>
        public object Tag
        {
            get { return this._tag; }
            set
            {
                if (this._tag != value)
                {
                    this._tag = value;
                    this.RaisePropertyChangedEvent("Tag");
                }
            }
        }

                #endregion //Tag	

				#region Value

		/// <summary>
		/// Gets/sets the value of the cell.
		/// </summary>
        /// <value>The underlying/raw value based on the <see cref="Infragistics.Windows.DataPresenter.Field"/>'s <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/>.</value>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field.Converter"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field.ConverterCulture"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field.ConverterParameter"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field.DataType"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field.EditAsTypeResolved"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.GetCellValue(Field)"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.SetCellValue(Field, object)"/>
        // JJD 2/17/09 - TFS14029
        // Made non-virtual since we always want to force the logic to go thru the field's get/set metjods
        //public virtual object Value
        public object Value
		{
			get
			{
				
				
				
				
				return this._record.GetCellValue( this._field, false );
			}
			set
			{
                //object oldValue = this.Value;

				
				
				
                
				this._record.SetCellValue( this._field, value, false );

                // JJD 3/5/08 
                // We don't to raise the cell's property changed events
                // since this is now done from inside the Field's SetCellValue
                //this.RaiseAutomationValueChanged(oldValue, value);

                //this.RaisePropertyChangedEvent("ConvertedValue");
                //this.RaisePropertyChangedEvent("Value");
             }
		}

				#endregion //Value

			#endregion //Public Properties

			#region Internal Properties

				#region AssociatedCellValuePresenterInternal

		internal CellValuePresenter AssociatedCellValuePresenterInternal
		{
			get
			{
				// AS 11/16/11 TFS95955
				// Moved to a helper routine so we can conditionally search for an element.
				//
				//if (this._associatedCellValuePresenter == null)
				//    return null;
				//
				//// AS 4/29/09 TFS17168
				//// The cell could have been allocated after the CVP was set up. If that is
				//// the case then the _associatedCellValuePresenter would still be set to 
				//// a static weakreference instance we use to identify that it hasn't been
				//// initialized yet. When that is the case we will do a search to try and 
				//// get the CVP. Note we need to clear the _associatedCellValuePresenter first
				//// because FromRecordAndField will call into this property.
				////
				////CellValuePresenter cvp = Utilities.GetWeakReferenceTargetSafe(this._associatedCellValuePresenter) as CellValuePresenter;
				//CellValuePresenter cvp = null;
				//
				//if (_associatedCellValuePresenter == UninitializedCVP)
				//{
				//    _associatedCellValuePresenter = null;
				//
				//    // AS 6/24/09 NA 2009.2 Field Sizing
				//    // Do not bother walking the element tree for the template record cells.
				//    //
				//    //cvp = CellValuePresenter.FromRecordAndField(_record, _field);
				//    if (_record is TemplateDataRecord)
				//        cvp = null;
				//    else
				//    {
				//        // MD 8/20/10
				//        // We don't need the CVPCache anymore. We can get similar gains by caching the cell panel on the data record presenter.
				//        ////~ SSP 6/7/10 - Optimizations - TFS34031
				//        ////~ 
				//        ////~cvp = CellValuePresenter.FromRecordAndField(_record, _field);
				//        //cvp = CellValuePresenter.FromRecordAndField( _record, _field, true );
				//        cvp = CellValuePresenter.FromRecordAndField(_record, _field);
				//    }
				//
				//    // if we find one then  cache a reference to it
				//    if (null != cvp)
				//    {
				//        // JJD 3/11/11 - TFS67970 - Optimization
				//        // Use the cvp's cached WeakRef instead
				//        // This prevents heap fragmentation when the cvp is recycled
				//        //_associatedCellValuePresenter = new WeakReference(cvp);
				//        _associatedCellValuePresenter = cvp.WeakRef;
				//    }
				//
				//    return cvp;
				//}
				//
				//cvp = Utilities.GetWeakReferenceTargetSafe(this._associatedCellValuePresenter) as CellValuePresenter;
				//
				//if (cvp == null)
				//{
				//    // JJD 1/24/08 - BR29985
				//    // Call ClearAssociatedCellValuePresenter
				//    this.ClearAssociatedCellValuePresenter();
				//    return null;
				//}
				//
				//// verify the CellPresenter is still valid
				//if (cvp.Record != this._record ||
				//     cvp.Field != this._field)
				//{
				//    // JJD 1/24/08 - BR29985
				//    // Call ClearAssociatedCellValuePresenter instead
				//    //this._associatedCellValuePresenter = null;
				//    this.ClearAssociatedCellValuePresenter();
				//    return null;
				//}
				//
				//return cvp;
				return this.GetAssociatedCellValuePresenter(true);
			}
			set
			{
				//if ( value != this.AssociatedCellValuePresenterInternal )
				// AS 11/16/11 TFS95955
				// Don't bother walking to find the element when we are just interesting in 
				// knowing what we already have referenced.
				//
                //if (value != this.AssociatedCellValuePresenterInternal)
                if (value != this.GetAssociatedCellValuePresenter(false))
                {
                    // JJD 1/24/08 - BR29985
                    // If the value is being set to null then call 
                    // ClearAssociatedCellValuePresenter
					if (value != null)
					{
						// AS 8/4/09 NA 2009.2 Field Sizing
						// Do not cache a reference to an element created for calculating the auto size.
						//
						if (AutoSizeFieldHelper.GetIsAutoSizeElement(value))
							return;

						// JJD 3/11/11 - TFS67970 - Optimization
						// Use the cvp's cached WeakRef instead
						// This prevents heap fragmentation when the cvp is recycled
						//_associatedCellValuePresenter = new WeakReference(value);
						_associatedCellValuePresenter = value.WeakRef;
					}
					else
						this.ClearAssociatedCellValuePresenter();
                }
                        

			}
		}

				#endregion //AssociatedCellValuePresenterInternal

				#region AssociatedCellValuePresenter

		// SSP 5/23/07
		// We need to verify and make sure the returned cell value presenter is the
		// one that's currently associated with the cell. The original method was
		// renamed to AssociatedCellValuePresenterInternal and is above.
		// 
		internal CellValuePresenter AssociatedCellValuePresenter
		{
			get
			{
				return CellValuePresenter.FromCell( this );
			}
		}

				#endregion //AssociatedCellValuePresenter

				#region AutomationPeer
		internal AutomationPeer AutomationPeer
		{
			get
			{
				RecordAutomationPeer recordPeer = this._record != null
					? this._record.AutomationPeer
					: null;
				return recordPeer != null
					? recordPeer.GetCellPeer(this)
					: null;
			}
		} 
				#endregion //AutomationPeer

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent - Added
				#region DataValueInfoHistoryCache

		internal DataValueInfoHistory DataValueInfoHistoryCache
		{
			get
			{
				// JJD 3/11/11 - TFS67970 - Optimization
				// Moved logic into GetDataValueHistory method
				return GetDataValueHistoryCache(this.AssociatedCellValuePresenter);
			}
		}

				#endregion //DataValueInfoHistoryCache

				#region EditAsTypeResolved

		internal Type EditAsTypeResolved
		{
			get
			{
				if ( this._editAsType != null )
					return this._editAsType;

				return this._field.EditAsTypeResolved;
			}
		}

				#endregion //EditAsTypeResolved	

				#region EditorTypeResolved

		internal Type EditorTypeResolved
		{
			get
			{
				// if a style was specified then use the style's target type
				if (this._editorType != null)
					return this._editorType;

				Type type;

				// if the editastype was supplied use it's default editor
				if (this._editAsType != null)
				{
					type = ValueEditor.GetDefaultEditorForType(this._editAsType);

					if (type != null)
						return type;
				}

				return this._field.EditorTypeResolved;
			}
		}

				#endregion //EditorTypeResolved	

				// AS 7/31/09 NA 2009.2 Field Sizing
				#region FieldAutoSizeVersion
		internal int FieldAutoSizeVersion
		{
			get { return _fieldAutoSizeVersion; }
			set { _fieldAutoSizeVersion = value; }
		}
				#endregion //FieldAutoSizeVersion

                // JJD 4/3/08 - added support for printing
                #region HasCloneableSettings

        internal virtual bool HasCloneableSettings
        {
            get
            {
                return this._editAsType != null ||
                        this._editorType != null ||
                        this._editorStyle != null ||
                        this._tag != null;
            }
        }

                #endregion //HasCloneableSettings	
 
				#region IsEditingAllowed

		internal bool IsEditingAllowed
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)
































































#endregion // Infragistics Source Cleanup (Region)

                return this.Field.GetIsEditingAllowed(this._record);
            }
		}

				#endregion //IsEditingAllowed	

                // JJD 1/9/09 - NA 2009 vol 1 - record filtering
				#region IsSelectable






        internal virtual bool IsSelectable
        {
            get
            {
                // Also check if the record is disabled Disabled.
                //
                if (!this._record.IsEnabledResolved)
                    return false;

                SelectionStrategyBase strategy = this.Field.Owner.GetSelectionStrategyForItem(this);
                if (null == strategy || strategy is SelectionStrategyNone)
                    return false;

                return true;
            }
        }

                #endregion // IsSelectable

                // JJD 2/17/09 - TFS13820
				#region IsTabStop






        internal virtual bool IsTabStop
        {
            get
            {
                // Check if the record is disabled.
                //
                if (!this._record.IsEnabledResolved)
                    return false;

                CellValuePresenter cvp = this.AssociatedCellValuePresenterInternal;

                if (cvp != null)
                {
                    // Check if the cell is visible and enabled.
                    //
                    // JJD 2/27/09 
                    // Check the visibility property instead of the IsVisisble since that
                    // isn't always reliable
                    //if (cvp.IsVisible == false ||
                    if (cvp.Visibility != Visibility.Visible ||
                        cvp.IsEnabled == false)
                        return false;
                }

                return true;
            }
        }

                #endregion // IsTabStop

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region EndEditMode

		/// <summary>
		/// Ends edit mode for this cell accepting any changes made while in edit mode.
		/// </summary>
		/// <returns>True if the operation completed successfully</returns>
		public bool EndEditMode()
		{
			return this.EndEditMode(true, false);
		}

		/// <summary>
		/// Ends edit mode for this cell.
		/// </summary>
		/// <param name="acceptChanges">If true then accept any changes made while in edit mode.</param>
		/// <returns>True if the operation completed successfully</returns>
		internal bool EndEditMode(bool acceptChanges)
		{
			return this.EndEditMode(acceptChanges, false);
		}

		/// <summary>
		/// Ends edit mode for this cell.
		/// </summary>
		/// <param name="acceptChanges">If true then accept any changes made while in edit mode.</param>
		/// <param name="forceExit">If true then ignore event cancellation.</param>
		/// <returns>True if the operation completed successfully</returns>
		internal bool EndEditMode(bool acceptChanges,
									bool forceExit )
		{
            // JJD 12/08/08 - save the cvp as a stack variable
            CellValuePresenter cvp = this.AssociatedCellValuePresenter;

			if (cvp == null)
				return true;

			cvp.EndEditMode(acceptChanges, forceExit);

			return !cvp.IsInEditMode;
		}

				#endregion //EndEditMode

				#region SetEditorSettings

		// SSP 10/20/09 TFS23589
		// 
		/// <summary>
		/// Used for specifying cell's <see cref="Cell.EditorType"/>, <see cref="Cell.EditorStyle"/>
		/// and <see cref="Cell.EditAsType"/> properties.
		/// </summary>
		/// <param name="editorType">New value of <see cref="Cell.EditorType"/>.</param>
		/// <param name="editorStyle">New value of <see cref="Cell.EditorStyle"/>.</param>
		/// <param name="editAsType">New value of <see cref="Cell.EditAsType"/>.</param>
		public void SetEditorSettings( Type editorType, Style editorStyle, Type editAsType )
		{
			bool reinitialize = false;

			if ( editorType != this._editorType )
			{
				this._editorType = editorType;
				this.RaisePropertyChangedEvent( "EditorType" );
				reinitialize = true;
			}


			if ( editorStyle != this._editorStyle )
			{
				this._editorStyle = editorStyle;
				this.RaisePropertyChangedEvent( "EditorStyle" );
				reinitialize = true;
			}

			if ( editAsType != this._editAsType )
			{
				this._editAsType = editAsType;
				this.RaisePropertyChangedEvent( "EditAsType" );
			}

			if ( reinitialize )
				this.ReInitializeCVPEditor( );
		}

				#endregion // SetEditorSettings

			#endregion //Public Methods	
        
			#region Internal Methods

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent
				#region AddDataValueChangedHistoryEntryForCurrentValue

		internal void AddDataValueChangedHistoryEntryForCurrentValue()
		{
			// JJD 3/11/11 - TFS67970 - Optimization
			// Create a stack variable to save the overhead of the get twice 
			//if (this.DataValueInfoHistoryCache != null)
			//    this.DataValueInfoHistoryCache.AddEntry(this.Value, DateTime.Now, null);
			DataValueInfoHistory history = this.DataValueInfoHistoryCache;
			if (history != null)
				history.AddEntry(this.Value, DateTime.Now, null);
		}

				#endregion //AddDataValueChangedHistoryEntryForCurrentValue

				// JJD 3/9/11 - TFS67970 - Optimization
				#region ClearAssociatedIfMatch

		internal void ClearAssociatedIfMatch(CellValuePresenter cvp)
		{
			if (this._associatedCellValuePresenter != null &&
				cvp == Utilities.GetWeakReferenceTargetSafe(this._associatedCellValuePresenter))
				this.ClearAssociatedCellValuePresenter();

		}
				#endregion //ClearAssociatedIfMatch	
    
				// JM 6/12/09 NA 2009.2 DataValueChangedEvent
				#region ClearDataValueChangedHistory







		internal void ClearDataValueChangedHistory(bool destroyCache)
		{
			if (this._dataValueInfoHistoryCache != null)
			{
				if (destroyCache)
				{
					this._dataValueInfoHistoryCache.ClearCacheEntries(false);

					// JJD 3/11/11 - TFS67970 - Optimization
					// Call the static Release method so we can re-use this object later
					DataValueInfoHistory.Release(this._dataValueInfoHistoryCache);
					
					this._dataValueInfoHistoryCache = null;

					CellValuePresenter cvp = Utilities.GetWeakReferenceTargetSafe(this._associatedCellValuePresenter) as CellValuePresenter;
					if (cvp != null)
						cvp.ClearValue(CellValuePresenter.ValueHistoryPropertyKey);
				}
				else
					this._dataValueInfoHistoryCache.ClearCacheEntries(true);
			}
		}

				#endregion //ClearDataValueChangedHistory

				// JJD 4/3/08 - added support for printing
                #region CloneAssociatedSettings

                internal virtual void CloneAssociatedCellSettings(Cell associatedCell)
                {
                    this._editAsType    = associatedCell._editAsType;
                    this._editorType    = associatedCell._editorType;
                    this._editorStyle   = associatedCell._editorStyle;
                    this._tag           = associatedCell._tag;
                }

                #endregion //CloneAssociatedCellSettings	
    
				// AS 7/31/09 NA 2009.2 Field Sizing
				#region DirtyFieldAutoSizeVersion
				internal void DirtyFieldAutoSizeVersion()
				{
					_fieldAutoSizeVersion--;
				}
				#endregion //DirtyFieldAutoSizeVersion

				#region FocusIfActive

				internal void FocusIfActive()
		{
			if (this.IsActive)
			{
				CellValuePresenter cvp = CellValuePresenter.FromRecordAndField(this.Record, this.Field);

				if (cvp != null)
					cvp.FocusIfAppropriate();
			}
		}

				#endregion //FocusIfActive

				// AS 11/16/11 TFS95955
				// Moved the implementation from the AssociatedCellValuePresenterInternal getter into 
				// this helper method so we can conditionally determine if we can/should walk the 
				// element tree to find the CVP.
				// 
				#region GetAssociatedCellValuePresenter
				internal CellValuePresenter GetAssociatedCellValuePresenter(bool canSearchElementTree)
				{
					if (this._associatedCellValuePresenter == null)
						return null;

					// AS 4/29/09 TFS17168
					// The cell could have been allocated after the CVP was set up. If that is
					// the case then the _associatedCellValuePresenter would still be set to 
					// a static weakreference instance we use to identify that it hasn't been
					// initialized yet. When that is the case we will do a search to try and 
					// get the CVP. Note we need to clear the _associatedCellValuePresenter first
					// because FromRecordAndField will call into this property.
					//
					//CellValuePresenter cvp = Utilities.GetWeakReferenceTargetSafe(this._associatedCellValuePresenter) as CellValuePresenter;
					CellValuePresenter cvp = null;

					if (_associatedCellValuePresenter == UninitializedCVP)
					{
						if (!canSearchElementTree)
							return null;

						_associatedCellValuePresenter = null;

						// AS 6/24/09 NA 2009.2 Field Sizing
						// Do not bother walking the element tree for the template record cells.
						//
						//cvp = CellValuePresenter.FromRecordAndField(_record, _field);
						if (_record is TemplateDataRecord)
							cvp = null;
						else
						{
							// MD 8/20/10
							// We don't need the CVPCache anymore. We can get similar gains by caching the cell panel on the data record presenter.
							////~ SSP 6/7/10 - Optimizations - TFS34031
							////~ 
							////~cvp = CellValuePresenter.FromRecordAndField(_record, _field);
							//cvp = CellValuePresenter.FromRecordAndField( _record, _field, true );
							cvp = CellValuePresenter.FromRecordAndField(_record, _field);
						}

						// if we find one then  cache a reference to it
						if (null != cvp)
						{
							// JJD 3/11/11 - TFS67970 - Optimization
							// Use the cvp's cached WeakRef instead
							// This prevents heap fragmentation when the cvp is recycled
							//_associatedCellValuePresenter = new WeakReference(cvp);
							_associatedCellValuePresenter = cvp.WeakRef;
						}

						return cvp;
					}

					cvp = Utilities.GetWeakReferenceTargetSafe(this._associatedCellValuePresenter) as CellValuePresenter;

					if (cvp == null)
					{
						// JJD 1/24/08 - BR29985
						// Call ClearAssociatedCellValuePresenter
						this.ClearAssociatedCellValuePresenter();
						return null;
					}

					// verify the CellPresenter is still valid
					if (cvp.Record != this._record ||
						 cvp.Field != this._field)
					{
						// JJD 1/24/08 - BR29985
						// Call ClearAssociatedCellValuePresenter instead
						//this._associatedCellValuePresenter = null;
						this.ClearAssociatedCellValuePresenter();
						return null;
					}

					return cvp;
				} 
				#endregion //GetAssociatedCellValuePresenter

				// JJD 3/11/11 - TFS67970 - Optimization - Added
				#region GetDataValueHistory

				internal DataValueInfoHistory GetDataValueHistoryCache(CellValuePresenter cvp)
				{
					int dataValueChangedHistoryLimitResolved = this.Field.DataValueChangedHistoryLimitResolved;

					// If the DataValueChangedHistoryLimitResolved is zero, null out our cache if we have one.
					if (dataValueChangedHistoryLimitResolved < 1)
					{
						if (this._dataValueInfoHistoryCache != null)
							this.ClearDataValueChangedHistory(true);
					}

					// JJD 3/11/11 - TFS67970 - Optimization
					// The cvp is now passed i nto the method
					//CellValuePresenter cvp = this.AssociatedCellValuePresenter;

					// If we have not yet allocated a DataValueInfoHistory cache, do it now.
					if (this._dataValueInfoHistoryCache == null)
					{
						if (dataValueChangedHistoryLimitResolved > 0)
						{
							// JJD 3/11/11 - TFS67970 - Optimization
							// Use static Create method instead
							//this._dataValueInfoHistoryCache = new DataValueInfoHistory(dataValueChangedHistoryLimitResolved);
							this._dataValueInfoHistoryCache = DataValueInfoHistory.Create(dataValueChangedHistoryLimitResolved);
							if (cvp != null && cvp.ValueHistory == null)
								cvp.SetValue(CellValuePresenter.ValueHistoryPropertyKey, this._dataValueInfoHistoryCache);
						}
					}
					else
						// If we have a DataValueHistory cache but it is the wrong size, resize it now.
						if (this._dataValueInfoHistoryCache.Capacity != dataValueChangedHistoryLimitResolved)
						{
							if (dataValueChangedHistoryLimitResolved > 0)
							{
								this._dataValueInfoHistoryCache = DataValueInfoHistory.GetResizedInstance(this._dataValueInfoHistoryCache, dataValueChangedHistoryLimitResolved);
								if (cvp != null)
									cvp.SetValue(CellValuePresenter.ValueHistoryPropertyKey, this._dataValueInfoHistoryCache);
							}
						}

					// If the member variable for the cache is still null, make sure that we also null out the cache reference onthe 
					// associated CellValuePresenter (if any).
					if (this._dataValueInfoHistoryCache == null)
					{
						if (cvp != null)
							cvp.ClearValue(CellValuePresenter.ValueHistoryPropertyKey);
					}

					return this._dataValueInfoHistoryCache;
				}

				#endregion //GetDataValueHistory	
    
				#region InternalSelect







		internal void InternalSelect(bool value)
		{
			if (this._selected != value)
			{
				this._selected = value;

                Debug.Assert(this._selected == false || this.IsSelectable, "We are selecting a cell that is not selectable");

				this.RaiseAutomationIsSelectedChanged();

				this.RaisePropertyChangedEvent("IsSelected");

				this.Record.OnCellSelectionChange();
			}
		}

				#endregion //InternalSelect

				#region InternalSetDataChanged







		internal void InternalSetDataChanged(bool value)
		{
			if (this._isDataChanged != value)
			{
				this._isDataChanged = value;
				this.RaisePropertyChangedEvent("IsDataChanged");

				if (value == true)
					this.Record.InternalSetDataChanged(true);
				else
				{
					// get the cellvaluepresenter
					CellValuePresenter cvp = this.AssociatedCellValuePresenter;

					// re-sync its value property
					if (cvp != null)
                        // JJD 3/5/08 - Added support for Converter properties
                        //cvp.Value = this.Value;
						// SSP 1/21/09 TFS12327
						// Ensure that CVP doesn't try to update field value since it's being
						// set to the field value. See notes on CellValuePresenter.CVPValueWrapper 
						// class for more info.
						// 
						//cvp.Value = this.ConvertedValue;
						cvp.SetValueInternal( this.ConvertedValue, false );
				}
			}
		}

				#endregion //InternalSetDataChanged

				#region IsOnSameIier

		// JJD 7/16/07 - BR19511
		// Added IsOnSameIier method
		internal bool IsOnSameTier(Cell cell, Orientation orientation)
		{
			if (cell == null ||
				 cell.Record != this.Record)
				return false;

			Field.FieldGridPosition pos = this.Field.GridPosition;
			Field.FieldGridPosition posToTest = cell.Field.GridPosition;

			if (orientation == Orientation.Vertical)
				return pos.Row == posToTest.Row &&
						pos.RowSpan == posToTest.RowSpan;
			else
				return pos.Column == posToTest.Column &&
						pos.ColumnSpan == posToTest.ColumnSpan;
		}

				#endregion //IsOnSameIier	
    
				#region OnActiveCellChanged

		internal virtual void OnActiveCellChanged()
		{
			this.RaisePropertyChangedEvent("IsActive");

			// have the record raise a notification also so CellPresenter's can be notified to recheck their state
			this.Record.OnActiveCellChanged();
		}

				#endregion //OnActiveCellChanged

				#region OnEditValueChanged

		// SSP 5/23/07
		// Added cellValuePresenter parameter to the method.
		// 
		//internal void OnEditValueChanged( )
		internal void OnEditValueChanged( CellValuePresenter cellValuePresenter )
		{
			if (this._isDataChanged == false)
			{
				this.RaisePropertyChangedEvent("IsDataChanged");
				// AS 5/15/09 NA 2009.2 ClipboardSupport
				//this.Record.OnEditValueChanged();
				this.Record.OnEditValueChanged(true);
			}

			// when the edit value is changed, we need to clear the 
			// size manager if it is only there because we are sizing to content
			this.Record.ClearSizeToContentManager(this);

			// raise the cell changed event
			// SSP 5/23/07
			// Pass the editor into the CellChangedEventArgs so it can expose it.
			// This way the user can get the new value of the cell.
			// 
			//this.DataPresenter.RaiseCellChanged(new CellChangedEventArgs(this));
			CellChangedEventArgs cellChangedArgs = new CellChangedEventArgs( this, cellValuePresenter.Editor );
			this.DataPresenter.RaiseCellChanged( cellChangedArgs );

		}

				#endregion //OnEditValueChanged	
    
				#region RaiseAutomationIsSelectedChanged
		internal void RaiseAutomationIsSelectedChanged()
		{
			AutomationPeer peer = this.AutomationPeer;

			if (null != peer)
				peer.RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, !this._selected, this._selected);
		} 
				#endregion //RaiseAutomationIsSelectedChanged

				#region RaiseAutomationSelectedChange
		internal bool RaiseAutomationSelectedChange(AutomationEvents eventId)
		{
			AutomationPeer peer = this.AutomationPeer;

			if (null != peer)
			{
				peer.RaiseAutomationEvent(eventId);
				return true;
			}

			return false;
		} 
				#endregion //RaiseAutomationSelectedChange

				#region RaiseAutomationValueChanged
		internal void RaiseAutomationValueChanged(object oldValue, object newValue)
		{
			CellAutomationPeer peer = this.AutomationPeer as CellAutomationPeer;

			if (null != peer)
				peer.RaiseValuePropertyChangedEvent(oldValue, newValue);
		} 
				#endregion //RaiseAutomationValueChanged

				#region RaiseDataErrorNotifications

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Raises property change notifications for DataError and HasDataError properties
		/// for the cell as well as the underlying CellValuePresenter if any.
		/// </summary>
		internal void RaiseDataErrorNotifications( )
		{
			if ( this.HasListeners )
			{
				this.RaisePropertyChangedEvent( "DataError" );
				this.RaisePropertyChangedEvent( "HasDataError" );
			}

			CellValuePresenter cvp = this.AssociatedCellValuePresenter;
			if ( null != cvp )
				cvp.UpdateDataError( );
		}

				#endregion // RaiseDataErrorNotifications

                // JJD 3/5/08 - Added 
				#region RaiseValueChangedNotifications
		internal void RaiseValueChangedNotifications()
		{
            // JJD 3/7/08
            // If there are no listeners then don't bother raising the notifications
            if (this.HasListeners && !this._isRaisingPropChangeNotifactions)
            {
                this._isRaisingPropChangeNotifactions = true;

                try
                {
                    this.RaisePropertyChangedEvent("Value");
                    this.RaisePropertyChangedEvent("ConvertedValue");
                }
                finally
                {
                    this._isRaisingPropChangeNotifactions = false;
                }
            }
		} 
				#endregion //RaiseValueChangedNotifications

			#endregion //Internal Methods

            #region Private Methods

                // JJD 1/24/08 - BR29985 - added
                #region ClearAssociatedCellValuePresenter

        private void ClearAssociatedCellValuePresenter()
        {
			// JM 6/12/09 NA 2009.2 DataValueChangedEvent
			if (this._dataValueInfoHistoryCache != null && this.Field.DataValueChangedScopeResolved == DataValueChangedScope.OnlyRecordsInView)
			{
				// JJD 3/11/11 - TFS67970 - Optimization
				// Call the static Release method so we can re-use this object later
				DataValueInfoHistory.Release(this._dataValueInfoHistoryCache);

				this._dataValueInfoHistoryCache = null;

				CellValuePresenter cvp = Utilities.GetWeakReferenceTargetSafe(this._associatedCellValuePresenter) as CellValuePresenter;
				if (cvp != null)
					cvp.ClearValue(CellValuePresenter.ValueHistoryPropertyKey);
			}

			this._associatedCellValuePresenter = null;

            // JJD 1/24/08 - BR29985
            // If MaintainBindings is false then clear the binding
            if (this is UnboundCell && !((UnboundCell)this).MaintainBindings)
                ((UnboundCell)this).ClearValueHolder();
		}

                #endregion //ClearAssociatedCellValuePresenter

				#region ReInitializeCVPEditor

		// SSP 10/20/09 TFS23589
		// When a cell's EditorStyle, EditorType or EditAsType changes we need to make sure
		// that the associated cell value presenter re-creates/re-initializes its editor 
		// based on the new value of the property.
		// 
		/// <summary>
		/// Called when EditorStyle, EditorType or EditAsType property changes
		/// to re-initialize the editor in the cvp.
		/// </summary>
		private void ReInitializeCVPEditor( )
		{
			CellValuePresenter cvp = this.AssociatedCellValuePresenter;
            if (null != cvp)
            {
                // JJD 11/12/09 - TFS24752
                // Pass the new value
                cvp.InitializeEditor(cvp.Value);
            }
		}

				#endregion // ReInitializeCVPEditor

            #endregion //Private Methods	
    
		#endregion //Methods

		#region Obsolete code

		//#region ISparseArrayItem Members

		//object ISparseArrayItem.GetOwnerData(SparseArray context)
		//{
		//    throw new NotSupportedException( SR.GetString( "LE_NotSupportedException_6" ) );
		//}

		//void ISparseArrayItem.SetOwnerData(object ownerData, SparseArray context)
		//{
		//    throw new NotSupportedException( SR.GetString( "LE_NotSupportedException_7" ) );
		//}

		//#endregion

		#endregion //Obsolete code		
    
		#region ISelectableItem Members

		bool ISelectableItem.IsDraggable
		{
			get { return false; } 
		}

		bool ISelectableItem.IsSelectable
		{
            // JJD 1/9/09 - NA 2009 vol 1 - record filtering
			//get { return true; } 
			get { return this.IsSelectable; } 
		}

		bool ISelectableItem.IsSelected
		{
			get { return this.IsSelected; }
		}

		bool ISelectableItem.IsTabStop
		{
            // JJD 1/9/09 - NA 2009 vol 1 - record filtering
			//get { return true; } 
			get { return this.IsTabStop; } 
		}

		#endregion
	}

	#endregion //Cell class

	#region UnboundCell class

	/// <summary>
	/// Class used by the to represent the value of a specific UnboundField of a specific DataRecord/> 
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note: </b>Note: UnboundCells are not UIElements but rather lightweight objects that hold the values for an associated <see cref="Infragistics.Windows.DataPresenter.UnboundField"/> for a specific <see cref="DataRecord"/>. They are represented in the UI by corresponding <see cref="CellValuePresenter"/> elements.</para>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</p>
	/// <p class="body">Refer to the <a href="xamData_Cells_CellValuePresenters_and_Cell_Virtualization.html">Cells, CellValuePresenters and Cell Virtualization</a> topic in the Developer's Guide for an explanation of cells.</p>
	/// <p class="body">Refer to the <a href="xamData_Assigning_a_FieldLayout.html">Assigning a FieldLayout</a> topic in the Developer's Guide for an example of adding an <see cref="UnboundField"/> to a <see cref="FieldLayout"/>.</p>
	/// </remarks>
	/// <seealso cref="CellValuePresenter"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
	/// <seealso cref="Cell"/>
	/// <seealso cref="Field.IsUnbound"/>
	/// <seealso cref="UnboundField"/>
	/// <seealso cref="DataRecord"/>
	/// <seealso cref="DataRecord.Cells"/>
	public class UnboundCell : Cell
	{
		#region Private Members

		private object _unboundData = null;
        // JJD 2/11/10 - TFS26644
        //private ValueHolder _valueHolder;
		private IValueHolder _valueHolder;

		#endregion //Private Members

		#region Consructor

		internal UnboundCell(DataRecord record, Field field)
			: base(record, field)
		{
			Debug.Assert(field.IsUnbound || record.IsAddRecord);
		}

		#endregion //Consructor

		#region Base class overrides

        // JJD 4/3/08 - added support for printing
        #region CloneAssociatedSettings

        internal override void CloneAssociatedCellSettings(Cell associatedCell)
        {
            base.CloneAssociatedCellSettings(associatedCell);

            UnboundCell associatedUnboundCell = associatedCell as UnboundCell;

            Debug.Assert(associatedUnboundCell != null);

            if ( associatedUnboundCell != null )
                this._unboundData           = associatedUnboundCell._unboundData;
        }

        #endregion //CloneAssociatedCellSettings	

        // JJD 4/3/08 - added support for printing
        #region HasCloneableSettings

        internal override bool HasCloneableSettings
        {
            get
            {
                return this._unboundData != null ||
                        base.HasCloneableSettings;
            }
        }

        #endregion //HasCloneableSettings	

        #region IsUnbound

        /// <summary>
        /// Returns true (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field.IsUnbound"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.UnboundField"/>
        public override bool IsUnbound
        {
            get
            {
                return true;
            }
        }

        #endregion //IsUnbound	
 
        // JJD 1/24/08 - BR29985 - added
        #region OnHasListenersChanged

        /// <summary>
        /// Override method to unbind value when there are no listeners
        /// </summary>
        protected override void OnHasListenersChanged()
        {
            base.OnHasListenersChanged();

            if (this.MaintainBindings)
                this.VerifyBindingStatus();
            else
                this.ClearValueHolder();
        }

        #endregion //OnHasListenersChanged	

        // JJD 1/24/08 - BR29985 - added
        #region MaintainBindings

        internal bool MaintainBindings
        {
            get
            {
				// SSP 3/22/10 - Optimizations
				// Added BindingRetentionMode property.
				// 
				UnboundField field = this.Field as UnboundField;
				if ( null != field && BindingRetentionMode.Retain == field.BindingRetentionMode )
					return true;
				
				// JJD 4/18/12 - TFS103589
				// If we have a value holder that doesn't have a bindin expression then we
				// should retain it because a value was set which stepped on the binding
				// and the value should be preserved
				if (_valueHolder != null && _valueHolder.Retain)
					return true;

				// SSP 3/10/10 TFS26510
				// If there's a summary result that's dependent on this cell's value then we
				// have to maintain the binding to get notified of changes to cell data.
				// 
				if ( ! FieldLayout.LayoutPropertyDescriptorProvider.HasPropertyDescriptorFor( this )
					&& SummaryResultCollection.HasSummaryDependentOn( this, false ) )
					return true;

                // if there are listeners then return true
                if (this.HasListeners)
                    return true;

                // if the cell is in view return true
				// AS 11/16/11 TFS95955
				// We don't need to do an element walk. If a CVP was created for an unbound cell then 
				// we would have forced the allocation of the cell and then we would have set the 
				// AssociatedCellValuePresenterInternal property to that CVP. Doing an element walk 
				// could actually cause a problem as it did in this case because we were not allowed to 
				// manipulate the element tree at this time.
				//
                //return this.AssociatedCellValuePresenterInternal != null;
                return this.GetAssociatedCellValuePresenter(false) != null;
            }
        }

        #endregion //MaintainBindings	
    
		#region ValueInternal

        // JJD 2/17/09 - TFS14029
        // Made property internal
        // /// <summary>
        // /// Gets/sets the value of the cell.
        // /// </summary>
        // /// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.GetCellValue(Field)"/>
        // /// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.SetCellValue(Field, object)"/>
        //public override object Value
        internal object ValueInternal
		{
			get
			{
				// SSP 6/28/10 TFS23257
				// Refactored. Moved the existing code into the new GetValueHelper method
				// so it can be used from the DataErrorInternal property.
				// 
				
				object val;
				string error;

				this.GetValueHelper( out val, out error, true, false );

				return val;
 
				
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

				
			}
			set
			{
				//object oldValue = this._unboundData;
				object oldValue = this.Value;

				this.VerifyBindingStatus();

                // JJD 12/3/10 - TFS61016
                // Create a stack variable to hold the binding expression
                BindingExpression bexp = null;;
				
				// JJD 4/11/07
				// If a value holder is being used then set its value
                if (this._valueHolder != null)
                {
					// JJD 4/18/12 - TFS103589
					// Create a stack variable so we know if the binding is pushable
					bool isBindingPushable = true;

					// JJD 11/08/10 - TFS31178
					// See if the field is bound to a property directly off the DataItem,
					UnboundField ufld = this.Field as UnboundField;

					if (ufld != null && ufld.IsBoundToRootProperty)
					{
						DataRecord dr = this.Record;
						if (dr != null &&
							dr.IsAddRecord == false &&
							dr.DataItem != null)
						{
							bexp = BindingOperations.GetBindingExpression(this._valueHolder as DependencyObject, ValueHolder.ValueProperty);

							// JJD 11/08/10 - TFS31178
							// See if the binding expression's DataItem is the same as to the DataRecord's DataItem and if there is
							// no binding error. Then check the binding mode. If it is 2-way or 1 way to source then call BeginEdit 
							// JJD 11/18/11 - TFS79001 
							// Use the GridUtilities.AreEqual helper method instead
							//if (bexp != null && bexp.DataItem == dr.DataItem && bexp.HasError == false && bexp.ParentBinding != null)
							if (bexp != null && GridUtilities.AreEqual( bexp.DataItem, dr.DataItem) && bexp.HasError == false && bexp.ParentBinding != null)
							{
								switch (bexp.ParentBinding.Mode)
								{
									case BindingMode.TwoWay:
									case BindingMode.OneWayToSource:
										dr.BeginEdit();
										break;
									default:
										// JJD 4/18/12 - TFS103589
										// set the flag so we can tell below that the binding was not pushable
										isBindingPushable = false;
										break;
								}
							}
						}
					}

                    this._valueHolder.Value = value;

					// JJD 4/18/12 - TFS103589
					// we can bail if the binding wasn't pushable
					if (!isBindingPushable && null == BindingOperations.GetBindingExpression(this._valueHolder as DependencyObject, ValueHolder.ValueProperty))
					{
						// set the Retain flag on the value holder
						_valueHolder.Retain = true;
						return;
					}

                    // JJD 1/24/08 - BR29985
                    // If MaintainBindings is false then clear the value holder which will clear the bindings
                    if (!this.MaintainBindings)
                        this.ClearValueHolder();
                    else
                    {
                        // JJD 12/3/10 - TFS61016
                        // If we don't already have the binding expression then get it
                        // and then call UpdateTarget so that we are sure to be in sync 
                        // with the source of the binding.
                        if (this._valueHolder != null)
                        {
                            if ( bexp == null )
                                bexp = BindingOperations.GetBindingExpression(this._valueHolder as DependencyObject, ValueHolder.ValueProperty);

							// JJD 1/4/11 - TFS61016
							// Only call UpdateTarget if the binding is active
                            //if (bexp != null)
                            if (bexp != null && bexp.Status == BindingStatus.Active)
                                bexp.UpdateTarget();
                        }
                    }
                }
                else
                {
                    this._unboundData = value;
                    this.InternalOnValueChanged(oldValue, value);
                }
			}
		}

		#endregion //ValueInternal

		#endregion //Base class overrides

		#region Methods

			#region Internal Methods

				#region InternalOnValueChanged

		// JJD 4/11/07
		// Added class to bind to object when a BindingPath is specified on thr UnboundField.
		internal void InternalOnValueChanged(object oldValue, object newValue)
		{
			this.RaiseAutomationValueChanged(oldValue, newValue);

//			this.RaisePropertyChangedEvent("Value");
            this.RaiseValueChangedNotifications();

            // JJD 6/26/08 - BR33656
            // If the cell is unbound then call the RefreshCellValueHelper method
            // so that child ExpandableFieldRecords get refreshed if this field
            // is an expandablefield
			DataRecord record = this.Record;
			Field field = this.Field;
			if ( this.IsUnbound )
			{
				record.RefreshCellValueHelper( field, newValue );

				// SSP 6/28/10 TFS23257
				// 
				record.RefreshDataErrorInfo( field, false );
			}

			// SSP 3/10/10 TFS26510
			// Call OnDataChanged which will dirty any summary and filtering.
			// 
			RecordManager recordManager = record.RecordManager;
			if ( null != recordManager )
				recordManager.OnDataChanged( DataChangeType.CellDataChange, record, field );

			// SSP 3/3/09 TFS11407
			// Added code to raise InitializeRecord event whenever a cell value changes.
			// 
			// JJD 11/17/11 - TFS78651 
			// Added sortValueChanged parameter
			//record.FireInitializeRecord( true );
			record.FireInitializeRecord( true, field.SortStatus != Infragistics.Windows.Controls.SortStatus.NotSorted );

			CellValuePresenter cvp = this.AssociatedCellValuePresenter;

            // JJD 3/5/08 - Added support for Converter properties
            if (cvp != null)
				// SSP 1/21/09 TFS12327
				// Ensure that CVP doesn't try to update field value since it's being
				// set to the field value. See notes on CellValuePresenter.CVPValueWrapper 
				// class for more info.
				// 
                //cvp.Value = this.ConvertedValue; // this.Value;
				cvp.SetValueInternal( this.ConvertedValue, false ); 
		}

				#endregion //InternalOnValueChanged	
    
				// JM 11-12-08 TFS10196
				#region RefreshBindings

		internal void RefreshBindings()
		{
			this.ClearValueHolder();
			this.VerifyBindingStatus();
		}

				#endregion //RefreshBindings

				// JJD 11/8/11 - TFS95675 - added
				#region RefreshCellValue

		internal void RefreshCellValue()
		{
			DependencyObject valHolder = this._valueHolder as DependencyObject;

			// If we have a ValueHolder object get its BindingExpression for the Value property
			// and call UpdateTarget
			if (valHolder != null)
			{
				BindingExpressionBase exp = BindingOperations.GetBindingExpressionBase(valHolder, ValueHolder.ValueProperty);

				if (exp != null)
					exp.UpdateTarget();
			}
		}

				#endregion //RefreshCellValue	
    
			#endregion //Internal Methods	
    
			#region Private Methods

				#region ClearValueHolder

		// JJD 4/11/07
		// Added class to bind to object when a BindingPath is specified on thr UnboundField.
		internal void ClearValueHolder()
		{
			if (this._valueHolder != null)
			{
                // JJD 1/24/08 - BR29985
                // Moved logic to ClearBinding method
                //BindingOperations.ClearAllBindings(this._valueHolder);
				this._valueHolder.ClearBinding();
				this._valueHolder = null;
			}
		}

                #endregion //ClearValueHolder

				#region GetValueHelper

		private void GetValueHelper( out object value, out string error, bool getValue, bool getError )
		{
			value = null;
			error = null;

			// JJD 4/11/07
			// Added class to bind to object when a BindingPath is specified on thr UnboundField.
			// Call VerifyBindingStatus
			this.VerifyBindingStatus( );

			// JJD 4/11/07
			// If a value holder is being used then return its value
			if ( this._valueHolder != null )
			{
				//return this._valueHolder.Value;
				if ( getValue )
					value = this._valueHolder.Value;

				if ( getError )
					error = _valueHolder.DataError;

				// JJD 1/24/08 - BR29985
				// If MaintainBindings is false then clear the value holder which will clear the bindings
				if ( !this.MaintainBindings )
					this.ClearValueHolder( );
			}
			else
			{
				if ( getValue )
					value = this._unboundData;
			}
		}

				#endregion // GetValueHelper

                #region IsSameBinding

        // JJD 2/11/10 - TFS26644
        // Added helper methid to support both types of bindings
        private bool IsSameBinding(DataRecord dr, BindingBase binding, PropertyPath bindingPath, BindingMode mode)
        {
            if (binding != null)
            {
                ValueHolderWithDataContext vdc = this._valueHolder as ValueHolderWithDataContext;

				// JJD 11/18/11 - TFS79001 
				// Use the GridUtilities.AreEqual helper method instead
				//return vdc != null && vdc.Binding == binding && dr.DataItem == vdc.DataContext;
                return vdc != null && vdc.Binding == binding && GridUtilities.AreEqual( dr.DataItem, vdc.DataContext);
            }

            ValueHolder vh = this._valueHolder as ValueHolder;

            return vh != null && vh.IsSameBinding(this, dr, bindingPath, mode);
        }

                #endregion //IsSameBinding	
    
				#region VerifyBindingStatus

		// JJD 4/11/07
		// Added class to bind to object when a BindingPath is specified on thr UnboundField.
		private void VerifyBindingStatus()
		{
			UnboundField fld = this.Field as UnboundField;

			Debug.Assert(fld != null);

			if (fld == null)
				return;

			PropertyPath bindingPath = fld.BindingPath;
            
            // JJD 2/11/10 - TFS26644 
            // Added Binding property
            BindingBase binding = fld.Binding;

			DataRecord dr = this.Record;

            // JJD 2/17/09 - TFS14029
            // If the data record is an addrecord template then don't bother trying to 
            // bind since there is no object to bind to
            // JJD 2/11/10 - TFS26644
            // Check both bindingPath and binding
            //if (bindingPath == null || dr.IsAddRecordTemplate)
            if ((bindingPath == null && binding == null ) || dr.IsAddRecordTemplate)
			{
				this.ClearValueHolder();
				return;
			}

			BindingMode mode = fld.BindingMode;

			if (this._valueHolder != null)
			{
				// if the binding is the same then return
                // JJD 2/11/10 - TFS26644
                // Call IsSameBinding helper method instead whic will support
                // the new Binding property on UnboundField
				//if (this._valueHolder.IsSameBinding(this, dr, bindingPath, mode))
				if (this.IsSameBinding(dr, binding, bindingPath, mode))
					return;

				// otherwise clear the value holder
				this.ClearValueHolder();
			}

            if (dr != null)
            {
                // JJD 2/11/10 - TFS26644
                // If a Binding is specified we need to create a value holder that
                // supports DataContext
                if (binding != null)
                    this._valueHolder = new ValueHolderWithDataContext(this, dr, binding);
                else
                    this._valueHolder = new ValueHolder(this, dr, bindingPath, mode);
            }
		}

				#endregion //VerifyBindingStatus	

			#endregion //Private Methods	
    
		#endregion //Methods	

		#region Properties

			#region Internal Properties

				#region DataErrorInternal

		// SSP 6/28/10 TFS23257
		// 
		/// <summary>
		/// Gets the data error.
		/// </summary>
		internal string DataErrorInternal
		{
			get
			{
				object val;
				string error;

				this.GetValueHelper( out val, out error, false, true );

				return error;
			}
		}

				#endregion // DataErrorInternal

			#endregion // Internal Properties 

		#endregion // Properties

        // JJD 2/11/10 - TFS26644 - added
        #region IValueHolder private interface

        // JJD 2/11/10 - TFS26644
        // Added interface so we could add a FrameworkContentElement derived class to
        // support the new Binding property on UnboundField
        private interface IValueHolder
        {
            void ClearBinding();

            object Value { get; set; }

			// SSP 6/28/10 TFS23257
			// Added DataError and Cell properties.
			// 
			string DataError { get; }

			UnboundCell Cell { get; }

			// JJD 4/18/12 - TFS103589 - added
			bool Retain { get; set; }
		}

        #endregion //IValueHolder private interface	
    
		#region DataErrorHolder Class

		// SSP 6/28/10 TFS23257
		// 
		private class DataErrorHolder
		{
			#region Member Vars

			private IValueHolder _holder;
			private PropertyValueTracker _errorsPvt;
			internal static readonly PropertyInfo g_binding_ValidatesOnDataErrorsProperty; 

			#endregion // Member Vars

			#region Constructor

			static DataErrorHolder( )
			{
				g_binding_ValidatesOnDataErrorsProperty = typeof( Binding ).GetProperty( "ValidatesOnDataErrors" );
			}

			internal DataErrorHolder( IValueHolder holder )
			{
				Debug.Assert( holder is DependencyObject, "Holder must be a DependencyObject." );

				_holder = holder;
			}
			
			#endregion // Constructor

			#region Properties

			#region DataError

			public string DataError
			{
				get
				{
					string error = GridUtilities.GetValidationErrorText( Validation.GetErrors( (DependencyObject)_holder ) );
					return error;
				}
			}

			#endregion // DataError 

			#endregion // Properties

			#region Methods

			#region OnBindingCleared

			internal void OnBindingCleared( )
			{
				_errorsPvt = null;
			} 

			#endregion // OnBindingCleared

			#region EnableDataErrorSupportHelper

			internal static bool EnableDataErrorSupportHelper( UnboundCell cell, Binding binding )
			{
				if ( cell.Field.SupportDataErrorInfoResolved )
				{
					try
					{
						PropertyInfo p = g_binding_ValidatesOnDataErrorsProperty;
						if ( null != p )
						{
							if ( null != binding )
								p.SetValue( binding, true, null );

							return true;
						}
					}
					catch
					{
					}
				}

				return false;
			}

			#endregion // EnableDataErrorSupportHelper 

			#region OnErrorsChanged

			private void OnErrorsChanged( )
			{
				if ( null != _errorsPvt )
				{
					UnboundCell cell = _holder.Cell;
					if ( null != cell )
						cell.Record.RefreshDataErrorInfo( cell.Field, false );
				}
			}

			#endregion // OnErrorsChanged

			#region OnValueChanged

			/// <summary>
			/// Called by the holder whenever the value changes.
			/// </summary>
			internal void OnValueChanged( )
			{
				// The binding evaluates validation rules after the value is changed. So we have to track the 
				// Validation.Errors to get notified if there is a validation error with the new value.
				// 
				if ( null == _errorsPvt )
					_errorsPvt = new PropertyValueTracker( _holder, "(Validation.Errors).Count", this.OnErrorsChanged );
			}

			#endregion // OnValueChanged

			#endregion // Methods
		}

		#endregion // DataErrorHolder Class

		#region ValueHolder private class

		// JJD 4/11/07
		// Added class to bind to object when a BindingPath is specified on thr UnboundField.
        // JJD 2/11/10 - TFS26644
        // Implement new IValueHolder interface
		private class ValueHolder : DependencyObject, IValueHolder
		{
			#region Private Members

			private UnboundCell _cell;
			// JJD 1/24/08 - BR29985
			// Changed flag name since we also use this dring a clear now
			//private bool _initializing;
			private bool _bypassNotifications;

			// SSP 10/18/08 BR35820
			// 
			private Binding _binding;

			// SSP 6/28/10 TFS23257
			// 
			private DataErrorHolder _dataErrorHolder;
			private bool _dataErrorSupportEnabled;

			#endregion //Private Members	
    
			#region Constructor

			internal ValueHolder(UnboundCell cell, DataRecord dr, PropertyPath bindingPath, BindingMode mode)
			{
				this._cell = cell;

				Debug.WriteLine("ValueHolder created.");

				Binding binding = new Binding();

				binding.Path = bindingPath;
				binding.Mode = mode;
				binding.Source = dr.DataItem;

				// SSP 6/28/10 TFS23257
				// 
				_dataErrorSupportEnabled = DataErrorHolder.EnableDataErrorSupportHelper(cell, binding);

				// SSP 7/28/10 TFS35638
				// In case Language property is set on the window or data presenter or the culture is
				// set on the field, we should set that on the binding as well so the value gets converted
				// correctly.
				// 
				Field field = cell.Field;
				binding.ConverterCulture = field.ConverterCultureResolved;

				this._bypassNotifications = true;

				try
				{
					BindingOperations.SetBinding(this, ValueProperty, binding);

					// SSP 10/18/08 BR35820
					// If BindingMode is OneWay, once the property is set, the binding is removed by the
					// framework. Therefore store the binding and use that for future comparison purposes.
					// 
					_binding = binding;
				}
				catch (Exception e)
				{
					throw new InvalidOperationException(DataPresenterBase.GetString("LE_InvalidOperationException_6", cell.Field.Name), e);
				}
				finally
				{
					this._bypassNotifications = false;
				}
			}

			#endregion //Constructor	
    
			#region Cell

			// SSP 6/28/10 TFS23257
			// 
			public UnboundCell Cell
			{
				get
				{
					return _cell;
				}
			}

			#endregion // Cell

			#region DataError

			// SSP 6/28/10 TFS23257
			// 
			public string DataError
			{
				get
				{
					if ( null == _dataErrorHolder )
						_dataErrorHolder = new DataErrorHolder( this );

					return _dataErrorHolder.DataError;
				}
			}

			#endregion // DataError

			#region Value

			internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
				typeof(object), typeof(ValueHolder), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

			private static void OnValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
			{
				ValueHolder holder = target as ValueHolder;

				if (holder != null )
				{
					if ( holder._bypassNotifications == false && holder._cell != null )
					{
						holder._cell.InternalOnValueChanged( e.OldValue, e.NewValue );

						// SSP 6/28/10 TFS23257
						// 
						
						if ( null != holder._dataErrorHolder )
							holder._dataErrorHolder.OnValueChanged( );
						
					}
				}
			}

			public object Value
			{
				get
				{
					return (object)this.GetValue(ValueHolder.ValueProperty);
				}
				set
				{
					this.SetValue(ValueHolder.ValueProperty, value);
				}
			}

			#endregion //Value

            #region IsSameBinding

            internal bool IsSameBinding(UnboundCell cell, DataRecord dr, PropertyPath bindingPath, BindingMode mode)
            {
                if (this._cell != cell ||
                    dr == null)
                    return false;

				// SSP 10/18/08 BR35820
				// If BindingMode is OneWay, once the property is set, the binding is removed by the
				// framework. Therefore store the binding and use that for future comparison purposes.
				// 
                //Binding binding = BindingOperations.GetBinding(this, ValueProperty);
				Binding binding = _binding;

                if (binding == null)
                    return false;

				return binding.Path == bindingPath &&
					   binding.Mode == mode &&
					   binding.Source == dr.DataItem
					   // SSP 6/28/10 TFS23257
					   // 
					   && _dataErrorSupportEnabled == DataErrorHolder.EnableDataErrorSupportHelper( cell, null )
					   // SSP 7/28/10 TFS35638
					   // 
					   && binding.ConverterCulture == cell.Field.ConverterCultureResolved;
            }

            #endregion //IsSameBinding	

            // JJD 1/24/08 - BR29985 - added
            #region ClearBinding

            public void ClearBinding()
            {
                // JJD 1/24/08 - BR29985
                // Set the bypass flag before clearing the bindings
                this._bypassNotifications = true;

				// SSP 6/28/10 TFS23257
				// 
				if ( null != _dataErrorHolder )
					_dataErrorHolder.OnBindingCleared( );

                BindingOperations.ClearAllBindings(this);
            }

            #endregion //ClearBinding	
			
			// JJD 4/18/12 - TFS103589 - added
			#region Retain

			public bool Retain { get; set; }

			#endregion //Retain	
           
		}

		#endregion //ValueHolder private class

        // JJD 2/11/10 - TFS26644 - added
		#region ValueHolderWithDataContext private class

		// Added class to bind to object when a Binding is specified on the UnboundField.
		private class ValueHolderWithDataContext : FrameworkContentElement, IValueHolder
		{
			#region Private Members

			private UnboundCell _cell;
			private bool _bypassNotifications;

			private BindingBase _binding;

			// SSP 6/28/10 TFS23257
			// 
			private DataErrorHolder _dataErrorHolder;

			#endregion //Private Members	
    
			#region Constructor

			internal ValueHolderWithDataContext(UnboundCell cell, DataRecord dr, BindingBase binding)
			{
				this._cell = cell;

				this._bypassNotifications = true;

				try
				{
					// set the DataContext on this object so the binding can work
					this.DataContext = dr.DataItem;

					// SSP 7/28/10 TFS35638
					// In case Language property is set on the window or data presenter or the culture is
					// set on the field, we should set that on the binding as well so the value gets converted
					// correctly.
					// 
					System.Windows.Markup.XmlLanguage language = this.GetLanguageHelper();
					if (null != language)
						this.Language = language;

					BindingOperations.SetBinding(this, ValueProperty, binding);

					// SSP 10/18/08 BR35820
					// If BindingMode is OneWay, once the property is set, the binding is removed by the
					// framework. Therefore store the binding and use that for future comparison purposes.
					// 
					_binding = binding;
				}
				catch (Exception e)
				{
					throw new InvalidOperationException(DataPresenterBase.GetString("LE_InvalidOperationException_6", cell.Field.Name), e);
				}
				finally
				{
					this._bypassNotifications = false;
				}
			}

			#endregion //Constructor	
    
			#region Binding

			internal BindingBase Binding { get { return this._binding; } }

			#endregion //Binding	
    
			#region GetLanguageHelper

			// SSP 7/28/10 TFS35638
			// 
			internal System.Windows.Markup.XmlLanguage GetLanguageHelper( )
			{
				CultureInfo culture = _cell.Field.ConverterCultureResolved;
				if ( null != culture )
				{
					try
					{
						return System.Windows.Markup.XmlLanguage.GetLanguage( culture.IetfLanguageTag );
					}
					catch
					{
					}
				}

				return null;
			}

			#endregion // GetLanguageHelper

			#region Cell

			// SSP 6/28/10 TFS23257
			// 
			public UnboundCell Cell
			{
				get
				{
					return _cell;
				}
			}

			#endregion // Cell

			#region DataError

			// SSP 6/28/10 TFS23257
			// 
			public string DataError
			{
				get
				{
					if ( null == _dataErrorHolder )
						_dataErrorHolder = new DataErrorHolder( this );

					return _dataErrorHolder.DataError;
				}
			}

			#endregion // DataError

			#region Value

            
            
            
            
            internal static readonly DependencyProperty ValueProperty = ValueHolder.ValueProperty.AddOwner(typeof(ValueHolderWithDataContext), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

			private static void OnValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
			{
				ValueHolderWithDataContext holder = target as ValueHolderWithDataContext;

				if ( holder != null )
				{
					if ( holder._bypassNotifications == false && holder._cell != null )
					{
						holder._cell.InternalOnValueChanged( e.OldValue, e.NewValue );

						// SSP 6/28/10 TFS23257
						// 
						if ( null != holder._dataErrorHolder )
							holder._dataErrorHolder.OnValueChanged( );
					}
				}
			}

			public object Value
			{
				get
				{
					return (object)this.GetValue(ValueHolderWithDataContext.ValueProperty);
				}
				set
				{
					this.SetValue(ValueHolderWithDataContext.ValueProperty, value);
				}
			}

			#endregion //Value

            // JJD 1/24/08 - BR29985 - added
            #region ClearBinding

            public void ClearBinding()
            {
                // JJD 1/24/08 - BR29985
                // Set the bypass flag before clearing the bindings
                this._bypassNotifications = true;

				// SSP 6/28/10 TFS23257
				// 
				if ( null != _dataErrorHolder )
					_dataErrorHolder.OnBindingCleared( );

                BindingOperations.ClearBinding(this, ValueProperty);
            }

            #endregion //ClearBinding	
			
			// JJD 4/18/12 - TFS103589 - added
			#region Retain

			public bool Retain { get; set; }

			#endregion //Retain	
        
		}

		#endregion //ValueHolderWithDataContext private class
	}

	#endregion //UnboundCell class
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