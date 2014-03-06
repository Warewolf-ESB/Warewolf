using System;
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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Infragistics.Windows.Commands;
using System.Collections.Specialized;




namespace Infragistics.Windows.DataPresenter
{

	#region FieldChooser Class

	/// <summary>
	/// A control that allows the user to customize which fields are displayed in the data presenter.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>FieldChooser</b> displays list of fields that are available for displaying in the data presenter.
	/// The user can drag and drop fields between the data presenter and the FieldChooser to show or hide
	/// them in the data presenter.
	/// </para>
	/// <para class="body">
	/// To show a field that's currently hidden, the user can either check the checkbox next to the field in the 
	/// field chooser or simply drag the field from the field chooser and drop ir anywhere on the data presenter.
	/// Note that the the field can be dropped at a specific location inside the field layout area, just like 
	/// the drag-and-drop functionalty.
	/// </para>
	/// <para class="body">
	/// To hide a field that's currently visible in the data presenter, the user can either uncheck the checkbox
	/// next to the field in the field chooser, or simply drag the field from the data presenter and drop it
	/// anywhere outside of the data presenter, including over the field chooser.
	/// </para>
	/// <para class="body">
	/// You can prevent the user from showing or hiding a specific field by setting the
	/// FieldSettings' <see cref="FieldSettings.AllowHiding"/> property to <i>Never</i>.
	/// The field will not be displayed in the field chooser and the user will not
	/// be able to hide the field, even by dragging it outside of the data presenter.
	/// </para>
	/// </remarks>
	/// <seealso cref="FieldSettings.AllowHiding"/>
	/// <seealso cref="FieldChooser.DisplayHiddenFieldsOnly"/>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	[TemplatePart(Name = "PART_FieldsListBox", Type = typeof(ListBox))] // JJD 3/28/12 - TFS107016 - added a part for the fields list box
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class FieldChooser : IGControlBase, ICommandHost
	{
		#region Nested Data Structures

		#region FieldComparer Class

		/// <summary>
		/// Comparer used for sorting fields in the order they are to be displayed in the field chooser.
		/// </summary>
		private class FieldComparer : IComparer<FieldChooserEntry>
		{
			private FieldChooser _fieldChooser;
			private FieldChooserDisplayOrder _displayOrder;
			private IComparer<Field> _customComparer;

			internal FieldComparer( FieldChooser fieldChooser )
			{
				_fieldChooser = fieldChooser;
				_displayOrder = fieldChooser.FieldDisplayOrder;
				_customComparer = fieldChooser.FieldDisplayOrderComparer;
			}

			public int Compare( FieldChooserEntry x, FieldChooserEntry y )
			{
				Field xx = x.Field;
				Field yy = y.Field;

				if ( null != _customComparer )
					return _customComparer.Compare( xx, yy );

				if ( FieldChooserDisplayOrder.SameAsDataPresenter == _displayOrder )
				{
					Debug.Assert( xx.Owner == yy.Owner );

					FieldPosition xxPos = xx.ActualPosition;
					FieldPosition yyPos = yy.ActualPosition;

					int r = xxPos.Row.CompareTo( yyPos.Row );

					if ( 0 == r )
						r = xxPos.Column.CompareTo( yyPos.Column );

					// SSP 1/12/10 TFS25122
					// When a hidden item is made visible, items sharing the same position are shifted
					// to the right. Therefore hidden items should be included in the list before the
					// visible items if their positions are the same.
					// 
					if ( 0 == r && xx.IsVisibleInCellArea != yy.IsVisibleInCellArea )
						r = ! xx.IsVisibleInCellArea ? -1 : 1;

					// Fallback to using the field indexes if the grid positions haven't been
					// initialized yet.
					// 
					if ( 0 == r )
						r = xx.Index.CompareTo( yy.Index );

					return r;
				}
				else
				{
					string xxLabel = GridUtilities.ToString( xx.Label, xx.Name );
					string yyLabel = GridUtilities.ToString( yy.Label, yy.Name );

					return GridUtilities.Compare( xxLabel, yyLabel, false );
				}
			}
		}

		#endregion // FieldComparer Class

		#endregion // Nested Data Structures

		#region Member Vars

		private FieldDragManager _fieldDragManager;

		private List<PropertyValueTracker> _pvtTrackers;

		/// <summary>
		/// All fields of the current group.
		/// </summary>
		private List<FieldChooserEntry> _currentGroupFields;

		/// <summary>
		/// Fields of the current group that are to be displayed in the list. If 
		/// DisplayHiddenFieldsOnly is true, this list doesn't include visible fields.
		/// </summary>
		private ObservableCollection<FieldChooserEntry> _currentFieldsObservableList;

		// Anti-recursion flags used to manage AllCurrentFieldsVisible property.
		// 
		private bool _settingAllCurrentFieldsVisible;
		private bool _processingAllCurrentFieldsVisiblePropChange;

		// SSP 1/12/10 TFS25122
		// 
		private bool _pendingReInitializeCurrentFields;

		#endregion // Member Vars

		#region Constructor

		static FieldChooser( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( FieldChooser ), 
				new FrameworkPropertyMetadata( typeof( FieldChooser ) ) );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FieldChooser"/>.
		/// </summary>
		public FieldChooser( )
		{
			this.SetValue( FieldFiltersPropertyKey, new ObservableCollection<FieldChooserFilter>( ) );

			#region Trackers

			_pvtTrackers = new List<PropertyValueTracker>( );
			PropertyValueTracker pvt;

			pvt = new PropertyValueTracker( this, "DataPresenter.FieldLayouts.Count", this.InitializeFieldGroups, true );
			_pvtTrackers.Add( pvt );

			
			
			
			
			pvt = new PropertyValueTracker( this, "CurrentFieldGroup.FieldLayout.Fields.Version", this.ReInitializeCurrentFields, true );

			_pvtTrackers.Add( pvt );

			#endregion // Trackers
		}

		#endregion // Constructor

		#region Base Overrides

		#region Commands

		/// <summary>
		/// Gets the supported commands (read-only) 
		/// </summary>
		/// <value>A static instance of the <see cref="FieldChooserCommands"/> class.</value>
		/// <remarks>
		/// <para class="body">
		/// This class exposes properties that return all of the commands that the control understands. 
		/// </para>
		/// </remarks>
		internal protected override CommandsBase Commands
		{
			get { return FieldChooserCommands.Instance; }
		}

		#endregion // Commands

		// JJD 3/28/12 - TFS107016 - added
		#region OnApplyTemplate


		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// JJD 3/28/12 - TFS107016
			// Set the PanningMode on the listbox to 'None' so the scrlllviewer
			// doesn't interfere with dragging logic
			ListBox fieldsListBox = base.GetTemplateChild("PART_FieldsListBox") as ListBox;
			if (fieldsListBox != null)
				ScrollViewer.SetPanningMode(fieldsListBox, PanningMode.None);
		}


		#endregion //OnApplyTemplate	
    
		#region OnLostMouseCapture

		/// <summary>
		/// Overridden. Called when mouse capture is lost.
		/// </summary>
		/// <param name="e">Associated event args.</param>
		protected override void OnLostMouseCapture( MouseEventArgs e )
		{
			base.OnLostMouseCapture( e );

			if ( null != _fieldDragManager )
			{
				_fieldDragManager.OnDragEnd( e, true );
			}
		}

		#endregion // OnLostMouseCapture

		#region OnMouseMove

		/// <summary>
		/// Overridden. Called when mouse is moved.
		/// </summary>
		/// <param name="e">Associated mouse event args.</param>
		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if ( null != _fieldDragManager )
			{
				_fieldDragManager.OnMouseMove( e );
			}
		}

		#endregion // OnMouseMove

		#region OnMouseLeftButtonUp

		/// <summary>
		/// Overridden. Called when left mouse button is released.
		/// </summary>
		/// <param name="e">Mouse event args.</param>
		protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
		{
			if ( null != _fieldDragManager )
			{
				_fieldDragManager.OnDragEnd( e, false );
				this.ReleaseMouseCapture( );
			}

			base.OnMouseLeftButtonUp( e );
		}

		#endregion // OnMouseLeftButtonUp

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region AllCurrentFieldsVisible

		/// <summary>
		/// Identifies the <see cref="AllCurrentFieldsVisible"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AllCurrentFieldsVisibleProperty = DependencyProperty.Register(
			"AllCurrentFieldsVisible",
			typeof( bool? ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnAllCurrentFieldsVisibleChanged ) )
		);

		/// <summary>
		/// Indicates if all the fields currently displayed in the FieldChooser are visible in the data presenter.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>AllCurrentFieldsVisible</b> property returns <b>True</b> if all the fields currently displayed
		/// in the FieldChooser are visible in the data presenter, <b>False</b> if all current fields
		/// are hidden in the data presenter, and <b>Null</b> some fields are hidden and some are visible.
		/// It's used in the control template to provide the capability of being able to toggle visibility
		/// of all fields.
		/// </para>
		/// </remarks>
		//[Description( "Indicates if all fields currently in view are visible in data presenter." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool? AllCurrentFieldsVisible
		{
			get
			{
				return (bool?)this.GetValue( AllCurrentFieldsVisibleProperty );
			}
			set
			{
				this.SetValue( AllCurrentFieldsVisibleProperty, value );
			}
		}

		private static void OnAllCurrentFieldsVisibleChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;
			bool? newVal = (bool?)e.NewValue;

			if ( ! fieldChooser._settingAllCurrentFieldsVisible )
				fieldChooser.SyncWithAllCurrentFieldsVisible( );
		}

		#endregion // AllCurrentFieldsVisible

		#region CurrentFieldGroup

		/// <summary>
		/// Identifies the <see cref="CurrentFieldGroup"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty CurrentFieldGroupProperty = DependencyProperty.Register(
			"CurrentFieldGroup",
			typeof( FieldChooserGroup ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnCurrentFieldGroupChanged ) )
		);

		/// <summary>
		/// Specifies which group of fields are currently displayed in the FieldChooser.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When <see cref="FieldFilters"/> property is not set, the <i>CurrentFieldGroup</i>
		/// corresponds to a field layout. If <i>FieldFilters</i> is set, the <i>CurrentFieldGroup</i>
		/// can correspond to all fields of a field layout, or subset of fields of a field layout 
		/// filtered with a <i>FieldChooserFilter</i> that's part of <i>FieldFilters</i> collection.
		/// </para>
		/// <para class="body">
		/// The field group selector control displayed above fields is populated with all the field layouts
		/// of the data presenter. If <i>FieldFilters</i> is specified, the field group selector control's
		/// drop-down contains each field layout and underneath each field layout the applicable field 
		/// filters to allow the user to display subset of the fields of that field layout.
		/// </para>
		/// </remarks>
		/// <see cref="FieldFilters"/>
		//[Description( "Specifies the current group of fields that are displayed." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public FieldChooserGroup CurrentFieldGroup
		{
			get
			{
				return (FieldChooserGroup)this.GetValue( CurrentFieldGroupProperty );
			}
			set
			{
				this.SetValue( CurrentFieldGroupProperty, value );
			}
		}

		private static void OnCurrentFieldGroupChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;

			fieldChooser.InitializeCurrentFields( );
		}

		#endregion // CurrentFieldGroup

		#region CurrentFields

		/// <summary>
		/// Identifies the property key for read-only <see cref="CurrentFields"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey CurrentFieldsPropertyKey = DependencyProperty.RegisterReadOnly(
			"CurrentFields",
			typeof( ReadOnlyObservableCollection<FieldChooserEntry> ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="CurrentFields"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty CurrentFieldsProperty = CurrentFieldsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the fields currently displayed in the FieldChooser.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>CurrentFields</b> returns a collection of <see cref="FieldChooserEntry"/> objects 
		/// each of which represent a field that's currently displayed in the FieldChooser.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldChooserEntry"/>
		//[Description( "Read-only collection of fields currently displayed in the field chooser." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public ReadOnlyObservableCollection<FieldChooserEntry> CurrentFields
		{
			get
			{
				return (ReadOnlyObservableCollection<FieldChooserEntry>)this.GetValue( CurrentFieldsProperty );
			}
		}

		#endregion // CurrentFields

		#region DataPresenter

		/// <summary>
		/// Identifies the <see cref="DataPresenter"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DataPresenterProperty = DependencyProperty.Register(
			"DataPresenter",
			typeof( DataPresenterBase ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnDataPresenterChanged ) )
		);

		/// <summary>
		/// Specifies the data presenter whose fields are being customized by this FieldChooser.
		/// </summary>
		//[Description( "Specifies the data presenter whose fields are displayed." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public DataPresenterBase DataPresenter
		{
			get
			{
				return (DataPresenterBase)this.GetValue( DataPresenterProperty );
			}
			set
			{
				this.SetValue( DataPresenterProperty, value );
			}
		}

		private ResourceDictionary _lastDPResources;
		private Collection<ResourceDictionary> _lastFCMergedDictionaries;

		private void MergeDPResources( DataPresenterBase dp )
		{
			if ( null != _lastDPResources && null != _lastFCMergedDictionaries )
				_lastFCMergedDictionaries.Remove( _lastDPResources );

			_lastDPResources = null != dp ? dp.Resources : null;
			_lastFCMergedDictionaries = null != this.Resources ? this.Resources.MergedDictionaries : null;

			if ( null != _lastDPResources && null != _lastFCMergedDictionaries )
				_lastFCMergedDictionaries.Add( _lastDPResources );

		}

		private static void OnDataPresenterChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;
			DataPresenterBase oldDP = (DataPresenterBase)e.OldValue;
			DataPresenterBase newDP = (DataPresenterBase)e.NewValue;

			// Data presenter manages references to all the field choosers that are referencing.
			// 
			if ( null != oldDP )
				oldDP.UnregisterFieldChooser( fieldChooser );

			if ( null != newDP )
				newDP.RegisterFieldChooser( fieldChooser );

			// Merge the data presenter's resources with the field chooser's so the label presenters
			// look the same as they do in the data presenter.
			// 
			fieldChooser.MergeDPResources( newDP );

			// Initialize IsDraggingItemFromDataPresenter property based on the new data presenter.
			// 
			fieldChooser.UpdateIsDraggingItemFromDataPresenter( );

			fieldChooser.InitializeFieldGroups( );
		}

		#endregion // DataPresenter

		#region DisplayHiddenFieldsOnly

		/// <summary>
		/// Identifies the <see cref="DisplayHiddenFieldsOnly"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DisplayHiddenFieldsOnlyProperty = DependencyProperty.Register(
			"DisplayHiddenFieldsOnly",
			typeof( bool ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback( OnDisplayHiddenFieldsOnlyChanged ) )
		);

		/// <summary>
		/// Specifies whether to display only the hidden fields or all fields.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DisplayHiddenFieldsOnly</b> property specifies whether the FieldChooser displays
		/// only the hidden fields or all fields. When this property is set to <b>True</b>, 
		/// FieldChooser displays only the fields that are not currently displayed in the data 
		/// presenter. When this property is set to <b>False</b>, all fields are displayed.
		/// </para>
		/// <para class="body">
		/// When displaying all fields, each field displays a checkbox next to it that controls the
		/// visibility of the field.
		/// When displaying only the hidden fields, these check boxes are not be displayed. 
		/// In either mode, the user can hide a field from the data presenter by dragging it from
		/// the data presenter and dropping it over the FieldChooser or any area outside 
		/// of the data presenter. Likewise the user can unhide a field by dragging it from
		/// the FieldChooser and dropping it inside the data presenter. Furthermore the user can 
		/// choose the drop the field at a specific location in the field layout area, in the same way 
		/// when moving a field via drag-and-drop, to position it at a specific location relative to
		/// other fields in the field layout area.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that you can exclude specific fields from being displayed in the FieldChooser 
		/// by setting the Field's <see cref="FieldSettings.AllowHiding"/> property.
		/// </para>
		/// </remarks>
		//[Description( "Specifies whether to display all fields or just the hidden fields." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public bool DisplayHiddenFieldsOnly
		{
			get
			{
				return (bool)this.GetValue( DisplayHiddenFieldsOnlyProperty );
			}
			set
			{
				this.SetValue( DisplayHiddenFieldsOnlyProperty, value );
			}
		}

		private static void OnDisplayHiddenFieldsOnlyChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;

			// Re-initialize the current fields based on the new value of the property.
			// 
			fieldChooser.ReInitializeCurrentFields( );
		}

		/// <summary>
		/// Returns true if the DisplayHiddenFieldsOnly property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDisplayHiddenFieldsOnly( )
		{
			return Utilities.ShouldSerialize( DisplayHiddenFieldsOnlyProperty, this );
		}

		/// <summary>
		/// Resets the DisplayHiddenFieldsOnly property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDisplayHiddenFieldsOnly( )
		{
			this.ClearValue( DisplayHiddenFieldsOnlyProperty );
		}

		#endregion // DisplayHiddenFieldsOnly

		#region FieldDisplayOrder

		/// <summary>
		/// Identifies the <see cref="FieldDisplayOrder"/> dependency property.
		/// </summary>
		/// <seealso cref="FieldDisplayOrderComparer"/>
		public static readonly DependencyProperty FieldDisplayOrderProperty = DependencyProperty.Register(
			"FieldDisplayOrder",
			typeof( FieldChooserDisplayOrder ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( FieldChooserDisplayOrder.Alphabetical, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnFieldDisplayOrderChanged ) )
		);

		/// <summary>
		/// Specifies the order in which fields are displayed in the FieldChooser. Default is <b>Alphabetical</b>.
		/// </summary>
		//[Description( "Specifies the order in which to displaye the fields." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public FieldChooserDisplayOrder FieldDisplayOrder
		{
			get
			{
				return (FieldChooserDisplayOrder)this.GetValue( FieldDisplayOrderProperty );
			}
			set
			{
				this.SetValue( FieldDisplayOrderProperty, value );
			}
		}

		private static void OnFieldDisplayOrderChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;

			fieldChooser.ReInitializeCurrentFields( );
		}

		/// <summary>
		/// Returns true if the FieldDisplayOrder property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeFieldDisplayOrder( )
		{
			return Utilities.ShouldSerialize( FieldDisplayOrderProperty, this );
		}

		/// <summary>
		/// Resets the FieldDisplayOrder property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetFieldDisplayOrder( )
		{
			this.ClearValue( FieldDisplayOrderProperty );
		}

		#endregion // FieldDisplayOrder

		#region FieldDisplayOrderComparer

		/// <summary>
		/// Identifies the <see cref="FieldDisplayOrderComparer"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FieldDisplayOrderComparerProperty = DependencyProperty.Register(
			"FieldDisplayOrderComparer",
			typeof( IComparer<Field> ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnFieldDisplayOrderComparerChanged ) )
		);

		/// <summary>
		/// Specifies the comparer that's used to determine the order of fields.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If you want to display fields in a custom order then you can specify
		/// a comparer that the FieldChooser will use to determine the order
		/// in which to display the fields.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldDisplayOrder"/>
		//[Description( "Specifies a comparer that's used to determine the order in which to display the fields in the FieldChooser." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IComparer<Field> FieldDisplayOrderComparer
		{
			get
			{
				return (IComparer<Field>)this.GetValue( FieldDisplayOrderComparerProperty );
			}
			set
			{
				this.SetValue( FieldDisplayOrderComparerProperty, value );
			}
		}

		private static void OnFieldDisplayOrderComparerChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;
			fieldChooser.ReInitializeCurrentFields( );
		}

		#endregion // FieldDisplayOrderComparer

		#region FieldFilters

		/// <summary>
		/// Identifies the property key for read-only <see cref="FieldFilters"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey FieldFiltersPropertyKey = DependencyProperty.RegisterReadOnly(
			"FieldFilters",
			typeof( ObservableCollection<FieldChooserFilter> ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnFieldFiltersChanged ) )
		);

		/// <summary>
		/// Identifies the read-only <see cref="FieldFilters"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FieldFiltersProperty = FieldFiltersPropertyKey.DependencyProperty;

		/// <summary>
		/// Specifies a collection of field groups. Each field layout will break up its fields into
		/// sub groups based on the setting of this property.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FieldFilters</b> property lets you specify groups of fields. Each field layout's fields
		/// can be broken up into subgroups. For example, in a Customer/Orders hierarchical data source,
		/// you can group Orders' field layout's fields into "Date Fields", "Adress Fields", "Phone/Fax Fields"
		/// etc... The field-layout selector will display these subgroups under the applicable field-layout.
		/// The end user then can select, for example, "Date Fields" entry under "Orders" entry in the field-layout
		/// selector to show only the date fields of the Orders field-layout.
		/// </para>
		/// </remarks>
		//[Description( "Used to categorize fields into subgroups." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public ObservableCollection<FieldChooserFilter> FieldFilters
		{
			get
			{
				return (ObservableCollection<FieldChooserFilter>)this.GetValue( FieldFiltersProperty );
			}
		}

		private static void OnFieldFiltersChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;
			ObservableCollection<FieldChooserFilter> oldVal = (ObservableCollection<FieldChooserFilter>)e.OldValue;
			ObservableCollection<FieldChooserFilter> newVal = (ObservableCollection<FieldChooserFilter>)e.NewValue;

			if ( null != oldVal )
				oldVal.CollectionChanged -= new NotifyCollectionChangedEventHandler( fieldChooser.OnFieldFilters_CollectionChanged );

			if ( null != newVal )
				newVal.CollectionChanged += new NotifyCollectionChangedEventHandler( fieldChooser.OnFieldFilters_CollectionChanged );
		}

		private void OnFieldFilters_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			this.InitializeFieldGroups( );
		}

		#endregion // FieldFilters

		#region FieldGroups

		/// <summary>
		/// Identifies the property key for read-only <see cref="FieldGroups"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey FieldGroupsPropertyKey = DependencyProperty.RegisterReadOnly(
			"FieldGroups",
			typeof( ReadOnlyObservableCollection<FieldChooserGroup> ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnFieldGroupsChanged ) )
		);

		/// <summary>
		/// Identifies the read-only <see cref="FieldGroups"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FieldGroupsProperty = FieldGroupsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the drop-down items of the field-group selector control in the FieldChooser.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FieldGroups</b> returns the collection of items that the field-group
		/// selector will display in its drop-down. By default the collection contains a single entry
		/// for each field-layout representing the fields of that field-layout. However if <see cref="FieldFilters"/> 
		/// property is specified to subgroup fields, then a single field-layout can have multiple entries
		/// in this collection - one for the field-layout itself and one for each subgroup within that field-layout.
		/// </para>
		/// </remarks>
		//[Description( "Field groups that are displayed in the field group selector drop-down." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public ReadOnlyObservableCollection<FieldChooserGroup> FieldGroups
		{
			get
			{
				return (ReadOnlyObservableCollection<FieldChooserGroup>)this.GetValue( FieldGroupsProperty );
			}
		}

		private static void OnFieldGroupsChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;

			// Make sure the CurrentFieldGroup is one of the new groups otherwise re-initialize it
			// to the first group.
			// 
			fieldChooser.InitializeCurrentGroup( );

			// Since we are resolving the FieldGroupSelectorVisibility based on the number of field groups,
			// we need to reinitialize FieldGroupSelectorVisibilityResolved property whenever the field groups
			// changes.
			// 
			fieldChooser.InitializeFieldGroupSelectorVisibilityResolved( );
		}

		#endregion // FieldGroups

		#region FieldGroupSelectorVisibility

		/// <summary>
		/// Identifies the <see cref="FieldGroupSelectorVisibility"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FieldGroupSelectorVisibilityProperty = DependencyProperty.Register(
			"FieldGroupSelectorVisibility",
			typeof( Visibility? ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnFieldGroupSelectorVisibilityResolvedChanged ) )
		);

		/// <summary>
		/// Specifies whether to display the FieldLayout selector. It's a control that allows the user
		/// to change the current group of fields that are displayed in the FieldChooser.
		/// </summary>
		//[Description( "Specifies whether the field group selector is visible." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public Visibility? FieldGroupSelectorVisibility
		{
			get
			{
				return (Visibility?)this.GetValue( FieldGroupSelectorVisibilityProperty );
			}
			set
			{
				this.SetValue( FieldGroupSelectorVisibilityProperty, value );
			}
		}

		private static void OnFieldGroupSelectorVisibilityResolvedChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;
			fieldChooser.InitializeFieldGroupSelectorVisibilityResolved( );
		}

		/// <summary>
		/// Returns true if the FieldGroupSelectorVisibility property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeFieldGroupSelectorVisibility( )
		{
			return Utilities.ShouldSerialize( FieldGroupSelectorVisibilityProperty, this );
		}

		/// <summary>
		/// Resets the FieldGroupSelectorVisibility property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDisplayFieldLayoutSelector( )
		{
			this.ClearValue( FieldGroupSelectorVisibilityProperty );
		}

		#endregion // FieldGroupSelectorVisibility

		#region FieldGroupSelectorVisibilityResolved

		/// <summary>
		/// Identifies the property key for read-only <see cref="FieldGroupSelectorVisibilityResolved"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey FieldGroupSelectorVisibilityResolvedPropertyKey = DependencyProperty.RegisterReadOnly(
			"FieldGroupSelectorVisibilityResolved",
			typeof( Visibility ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( Visibility.Visible, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="FieldGroupSelectorVisibilityResolved"/> dependency property.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		public static readonly DependencyProperty FieldGroupSelectorVisibilityResolvedProperty = FieldGroupSelectorVisibilityResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the resolved value indicating whether the field-layout selector control is displayed.
		/// </summary>
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Visibility FieldGroupSelectorVisibilityResolved
		{
			get
			{
				return (Visibility)this.GetValue( FieldGroupSelectorVisibilityResolvedProperty );
			}
		}

		#endregion // FieldGroupSelectorVisibilityResolved

		#region IsDragItemOver

		/// <summary>
		/// Identifies the property key for read-only <see cref="IsDragItemOver"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey IsDragItemOverPropertyKey = DependencyProperty.RegisterReadOnly(
			"IsDragItemOver",
			typeof( bool ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsDragItemOver"/> dependency property.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		public static readonly DependencyProperty IsDragItemOverProperty = IsDragItemOverPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true when a field being dragged from the associated data presenter is 
		/// over the FieldChooser (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>IsDragItemOver</b> property returns true when a field being dragged from 
		/// the associated data presenter is over the field chooser. It can be used in 
		/// the style of the FieldChooser to highlight the FieldChooser control to provide 
		/// a hint to the user that the item can be dropped over the FieldChooser.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsDraggingItem"/>
		/// <seealso cref="IsDraggingItemFromDataPresenter"/>
		/// <seealso cref="LabelPresenter.IsInFieldChooser"/>
		/// <seealso cref="LabelPresenter.IsSelectedInFieldChooser"/>
		//[Description( "Indicates if the user has dragged a field over the field chooser currently." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsDragItemOver
		{
			get
			{
				return (bool)this.GetValue( IsDragItemOverProperty );
			}
		}

		internal void InternalSetIsDragItemOver( bool value )
		{
			this.SetValue( IsDragItemOverPropertyKey, KnownBoxes.FromValue( value ) );
		}

		#endregion // IsDragItemOver

		#region IsDraggingItem

		/// <summary>
		/// Identifies the property key for read-only <see cref="IsDraggingItem"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey IsDraggingItemPropertyKey = DependencyProperty.RegisterReadOnly(
			"IsDraggingItem",
			typeof( bool ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsDraggingItem"/> dependency property.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		public static readonly DependencyProperty IsDraggingItemProperty = IsDraggingItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates if a field is being dragged from this FieldChooser.
		/// </summary>
		//[Description( "Indicates if the user is currently dragging a field from the FieldChooser." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsDraggingItem
		{
			get
			{
				return (bool)this.GetValue( IsDraggingItemProperty );
			}
		}

		#endregion // IsDraggingItem

		#region IsDraggingItemFromDataPresenter

		/// <summary>
		/// Identifies the property key for read-only <see cref="IsDraggingItemFromDataPresenter"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey IsDraggingItemFromDataPresenterPropertyKey = DependencyProperty.RegisterReadOnly(
			"IsDraggingItemFromDataPresenter",
			typeof( bool ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsDraggingItemFromDataPresenter"/> dependency property.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		public static readonly DependencyProperty IsDraggingItemFromDataPresenterProperty = IsDraggingItemFromDataPresenterPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates if a field is being dragged from the associated data presenter.
		/// </summary>
		/// <remarks>
		/// <b>IsDraggingItemFromDataPresenter</b> indicates if a field is being dragged from the 
		/// associated data presenter. Associated data presenter is specified using the 
		/// <see cref="FieldChooser.DataPresenter"/> property.
		/// </remarks>
		/// <seealso cref="FieldChooser.DataPresenter"/>
		//[Description( "Indicates if the user is currently dragging a field from the data presenter." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsDraggingItemFromDataPresenter
		{
			get
			{
				return (bool)this.GetValue( IsDraggingItemFromDataPresenterProperty );
			}
		}

		#endregion // IsDraggingItemFromDataPresenter

		#region SelectedField

		/// <summary>
		/// Identifies the <see cref="SelectedField"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty SelectedFieldProperty = DependencyProperty.Register(
			"SelectedField",
			typeof( FieldChooserEntry ),
			typeof( FieldChooser ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnSelectedFieldChanged ),
				new CoerceValueCallback( OnCoerceSelectedField ) )
		);

		/// <summary>
		/// Gets or sets the field that's currently selected in the FieldChooser.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The user can select a field and perform keyboard navigation in the list of fields in 
		/// the FieldChooser. Only one field can be selected at a time. The <b>SelectedField</b> 
		/// property returns the field that's currently selected. You can also set it to select
		/// a field that's currently displayed in the FieldChooser.
		/// </para>
		/// </remarks>
		//[Description( "The field that's currently selected in the FieldChooser." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public FieldChooserEntry SelectedField
		{
			get
			{
				return (FieldChooserEntry)this.GetValue( SelectedFieldProperty );
			}
			set
			{
				this.SetValue( SelectedFieldProperty, value );
			}
		}

		private static void OnSelectedFieldChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldChooserEntry oldVal = (FieldChooserEntry)e.OldValue;
			FieldChooserEntry newVal = (FieldChooserEntry)e.NewValue;

			if ( null != oldVal )
				oldVal.InternalSetIsSelected( false );

			if ( null != newVal )
				newVal.InternalSetIsSelected( true );
		}

		private static object OnCoerceSelectedField( DependencyObject dependencyObject, object valueAsObject )
		{
			FieldChooser fieldChooser = (FieldChooser)dependencyObject;
			FieldChooserEntry val = (FieldChooserEntry)valueAsObject;

			if ( null != val )
			{
				IList<FieldChooserEntry> currentFields = fieldChooser.CurrentFields;
				if ( null != currentFields )
				{
					if ( currentFields.Contains( val ) )
						return val;

					return fieldChooser.FindFieldChooserEntry( val.Field );
				}
			}

			return null;
		}

		#endregion // SelectedField

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region ExecuteCommand

		/// <summary>
		/// Executes the RoutedCommand represented by the specified CommandWrapper.
		/// </summary>
		/// <param name="commandWrapper">The CommandWrapper that contains the RoutedCommand to execute</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="ExecuteCommand(RoutedCommand)"/>
		/// <seealso cref="FieldChooserCommands"/>
		public bool ExecuteCommand( CommandWrapper commandWrapper )
		{
			if ( commandWrapper == null )
				throw new ArgumentNullException( "commandWrapper" );

			return this.ExecuteCommand( commandWrapper.Command );
		}

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="command">The RoutedCommand to execute.</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="ExecuteCommand(CommandWrapper)"/>
		/// <seealso cref="FieldChooserCommands"/>
		public bool ExecuteCommand( RoutedCommand command )
		{
			return this.ExecuteCommand( command, null );
		}

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="command">The RoutedCommand to execute.</param>
		/// <param name="parameter">The parameter for the command execution</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="ExecuteCommand(CommandWrapper)"/>
		/// <seealso cref="FieldChooserCommands"/>
		public bool ExecuteCommand( RoutedCommand command, object parameter )
		{
			return this.ExecuteCommandImpl( command, parameter );
		}

		#endregion // ExecuteCommand

		#region Private/Internal Methods

		#region AddSubGroups

		private void AddSubGroups( FieldLayout fl, ObservableCollection<FieldChooserGroup> groups )
		{
			ObservableCollection<FieldChooserFilter> filters = this.FieldFilters;
			if ( null != filters )
			{
				foreach ( FieldChooserFilter ff in filters )
				{
					if ( null != ff )
					{
						IEnumerable<Field> matchingFields = ff.GetMatchingFields( fl );
						if ( GridUtilities.GetCount( matchingFields ) > 0 )
							groups.Add( new FieldChooserGroup( fl, ff ) );
					}
				}
			}
		}

		#endregion // AddSubGroups

		#region CreateFieldEntries

		private List<FieldChooserEntry> CreateFieldEntries( FieldChooserGroup group )
		{
			List<FieldChooserEntry> list = null;

			List<Field> fields = GridUtilities.ToList<Field>( group.GetMatchingFields( ), true );
			if ( null != fields )
			{
				list = new List<FieldChooserEntry>( );
				foreach ( Field f in fields )
				{
					if ( ! f.IsExpandableResolved 
						&& AllowFieldHiding.Never != f.AllowHidingResolved 
						// We allow setting the CurrentFieldGroup property to an instance that's not 
						// part of FieldGroups. In that case we display the fields that match that 
						// group. However if the set field group happens to reference a field layout 
						// that's from a different data presenter then we should not show those 
						// fields from a different data presenter.
						// 
						&& f.DataPresenter == this.DataPresenter )
					{
						FieldChooserEntry entry = new FieldChooserEntry( f );
						list.Add( entry );
						entry.InitializeFieldChooser( this );
					}
				}

				// Sort the fields based on the FieldDisplayOrder property.
				// 
				list.Sort( new FieldComparer( this ) );
			}

			return list;
		}

		#endregion // CreateFieldEntries

		#region CreateFieldGroups

		private ObservableCollection<FieldChooserGroup> CreateFieldGroups( )
		{
			DataPresenterBase dp = this.DataPresenter;
			ObservableCollection<FieldChooserGroup> groups = new ObservableCollection<FieldChooserGroup>( );

			if ( null != dp )
			{
				FieldLayoutCollection flColl = dp.FieldLayouts;
				foreach ( FieldLayout fl in flColl )
				{
					FieldChooserGroup allFieldsGroup = new FieldChooserGroup( fl, null );
					groups.Add( allFieldsGroup );
					this.AddSubGroups( fl, groups );
				}
			}

			return groups;
		}

		#endregion // CreateFieldGroups

		#region ExecuteCommandImpl

		private bool ExecuteCommandImpl( RoutedCommand command, object commandParameter )
		{
			return ExecuteCommandImpl( new ExecuteCommandInfo( command, commandParameter, null ) );
		}

		private bool ExecuteCommandImpl( ExecuteCommandInfo commandInfo )
		{
			ICommand command = commandInfo.Command;
			object parameter = commandInfo.Parameter;

			if ( FieldChooserCommands.ToggleVisibility == command )
			{
				FieldChooserEntry entry = this.GetFieldFromCommandInfo( commandInfo, true );
				if ( null != entry )
				{
					entry.IsVisible = !entry.IsVisible;
					return true;
				}
			}
			else if ( FieldChooserCommands.Stop == command )
			{
				if ( this.IsMouseCaptureWithin )
				{
					this.ReleaseMouseCapture( );
					return true;
				}
			}

			return false;
		}

		#endregion // ExecuteCommandImpl

		#region FindFieldChooserEntry

		/// <summary>
		/// Finds the matching item in the CurrentFields collection.
		/// </summary>
		/// <param name="field">Field to search the entry for.</param>
		/// <returns>FieldChooserEntry from CurrentFields associated with the specified field will be returned.</returns>
		public FieldChooserEntry FindFieldChooserEntry( Field field )
		{
			IList<FieldChooserEntry> fields = this.CurrentFields;
			if ( null != fields && null != field )
			{
				foreach ( FieldChooserEntry ii in fields )
				{
					if ( ii.Field == field )
						return ii;
				}
			}

			return null;
		}

		#endregion // FindFieldChooserEntry

		#region GetFieldFromCommandInfo

		private FieldChooserEntry GetFieldFromCommandInfo( ExecuteCommandInfo commandInfo, bool fallbackToSelectedField )
		{
			Field field = null;

			object parameter = commandInfo.Parameter;

			if ( parameter is Field )
			{
				field = (Field)parameter;
			}
			else if ( parameter is FieldChooserEntry )
			{
				field = ( (FieldChooserEntry)parameter ).Field;
			}
			else
			{
				LabelPresenter labelPresenter = commandInfo.OriginalSource as LabelPresenter;
				if ( null != labelPresenter )
					field = labelPresenter.Field;
			}

			FieldChooserEntry entry = null != field ? this.FindFieldChooserEntry( field ) : null;
			if ( null == entry && fallbackToSelectedField )
				entry = this.SelectedField;

			return entry;
		}

		#endregion // GetFieldFromCommandInfo

		#region InitializeCurrentFields

		/// <summary>
		/// Initializes the CurrentFields property based on the CurrentFieldGroup property value.
		/// </summary>
		private void InitializeCurrentFields( )
		{
			FieldChooserGroup currentGroup = this.CurrentFieldGroup;
			List<FieldChooserEntry> fieldsList = null != currentGroup ? this.CreateFieldEntries( currentGroup ) : null;

			this.InitializeCurrentFieldsHelper( fieldsList );
		}

		/// <summary>
		/// Initializes the CurrentFields property based on the specified fieldsList. If 
		/// DisplayHiddenFieldsOnly is true then this method will exclude visible fields.
		/// </summary>
		private void InitializeCurrentFieldsHelper( List<FieldChooserEntry> fieldsList )
		{
			ObservableCollection<FieldChooserEntry> fieldsObservableList = new ObservableCollection<FieldChooserEntry>( );

			if ( null != fieldsList && fieldsList.Count > 0 )
			{
				bool displayHiddenFieldsOnly = this.DisplayHiddenFieldsOnly;

				foreach ( FieldChooserEntry ii in fieldsList )
				{
					if ( ! displayHiddenFieldsOnly || ! ii.IsVisible )
						fieldsObservableList.Add( ii );
				}
			}

			ReadOnlyObservableCollection<FieldChooserEntry> fieldsReadOnlyList = null != fieldsObservableList
				? new ReadOnlyObservableCollection<FieldChooserEntry>( fieldsObservableList )
				: null;

			_currentGroupFields = fieldsList;
			_currentFieldsObservableList = fieldsObservableList;
			this.SetValue( CurrentFieldsPropertyKey, fieldsReadOnlyList );

			this.SyncAllCurrentFieldsVisible( );
		}

		#endregion // InitializeCurrentFields

		#region InitializeCurrentGroup

		/// <summary>
		/// Makes sure the CurrentFieldGroup is one of the FieldGroups otherwise it 
		/// sets the CurrentFieldGroup to the first item in the FieldGroups.
		/// </summary>
		private void InitializeCurrentGroup( )
		{
			FieldChooserGroup newCurrentGroup = null;

			// Try to maintain current group.
			// 
			FieldChooserGroup currentGroup = this.CurrentFieldGroup;
			if ( null != currentGroup )
			{
				if ( this.IsGroupValid( currentGroup ) )
				{
					newCurrentGroup = currentGroup;
				}
				else
				{
					FieldChooserGroup flGroup = new FieldChooserGroup( currentGroup.FieldLayout, null );
					if ( this.IsGroupValid( flGroup ) )
						newCurrentGroup = flGroup;
				}
			}

			if ( null == newCurrentGroup )
			{
				ReadOnlyCollection<FieldChooserGroup> groups = this.FieldGroups;
				newCurrentGroup = null != groups && groups.Count > 0 ? groups[0] : null;
			}

			this.CurrentFieldGroup = newCurrentGroup;
		}

		#endregion // InitializeCurrentGroup

		#region InitializeFieldGroups

		/// <summary>
		/// Initializes the FieldGroups property.
		/// </summary>
		private void InitializeFieldGroups( )
		{
			ObservableCollection<FieldChooserGroup> list = this.CreateFieldGroups( );
			ReadOnlyObservableCollection<FieldChooserGroup> groups = new ReadOnlyObservableCollection<FieldChooserGroup>( list );
			this.SetValue( FieldGroupsPropertyKey, groups );
		}

		#endregion // InitializeFieldGroups

		#region InitializeFieldGroupSelectorVisibilityResolved

		private void InitializeFieldGroupSelectorVisibilityResolved( )
		{
			Visibility? val = this.FieldGroupSelectorVisibility;
			if ( !val.HasValue )
			{
				// If the data presenter has only one field-layout then hide the field group 
				// selector by default. If the data presenter hasn't been initialized or
				// doesn't have its data source set then leave the field group selector
				// visible.
				// 
				val = null != this.FieldGroups && 1 == this.FieldGroups.Count
					? Visibility.Collapsed : Visibility.Visible;
			}

			this.SetValue( FieldGroupSelectorVisibilityResolvedPropertyKey, val );
		}

		#endregion // InitializeFieldGroupSelectorVisibilityResolved

		#region InternalSetAllCurrentFieldsVisible

		private void InternalSetAllCurrentFieldsVisible( bool? newVal )
		{
			bool origSettingAllCurrentFieldsVisible = _settingAllCurrentFieldsVisible;
			_settingAllCurrentFieldsVisible = true;
			try
			{
				this.AllCurrentFieldsVisible = newVal;
			}
			finally
			{
				_settingAllCurrentFieldsVisible = origSettingAllCurrentFieldsVisible;
			}
		}

		#endregion // InternalSetAllCurrentFieldsVisible

		#region IsGroupValid

		/// <summary>
		/// Indicates if the specified group is one of the groups in the collection returned by FieldGroups property.
		/// </summary>
		/// <param name="group">Field chooser group.</param>
		/// <returns>A boolean value indicating if the group is valid.</returns>
		private bool IsGroupValid( FieldChooserGroup group )
		{
			ReadOnlyObservableCollection<FieldChooserGroup> groups = this.FieldGroups;
			return null != groups && groups.IndexOf( group ) >= 0;
		}

		#endregion // IsGroupValid

		#region OnFieldVisibilityChanged

		/// <summary>
		/// Called by the FieldChooserEntry when a field's VisibilityResolved changes.
		/// If DisplayHiddenFieldsOnly is true then this method will add/remove the associated 
		/// field into the field chooser based on the new visibility state of the field.
		/// </summary>
		/// <param name="field">Field whose visibility changed.</param>
		internal void OnFieldVisibilityChanged( Field field )
		{
			if ( this.DisplayHiddenFieldsOnly )
			{
				this.InitializeCurrentFieldsHelper( _currentGroupFields );
			}
			else
			{
				this.SyncAllCurrentFieldsVisible( );
			}
		}

		#endregion // OnFieldVisibilityChanged

		#region ReInitializeCurrentFields

		/// <summary>
		/// Re-initializes the CurrentFields property based on the CurrentFieldGroup property value.
		/// </summary>
		internal void ReInitializeCurrentFields( )
		{
			this.InitializeCurrentFields( );
		}

		#endregion // ReInitializeCurrentFields

		#region ReInitializeCurrentFieldsAsync

		// SSP 1/12/10 TFS25122
		// 
		/// <summary>
		/// Re-initializes the CurrentFields property based on the CurrentFieldGroup property value.
		/// </summary>
		internal void ReInitializeCurrentFieldsAsync( )
		{
			if ( _pendingReInitializeCurrentFields )
				return;

			Dispatcher dispatcher = this.Dispatcher;
			if ( null != dispatcher )
			{
				dispatcher.BeginInvoke( DispatcherPriority.Normal, new GridUtilities.MethodDelegate( this.ReInitializeCurrentFieldsAsyncCallback ) );
				_pendingReInitializeCurrentFields = true;
			}
			else
			{
				this.InitializeCurrentFields( );
			}
		}

		private void ReInitializeCurrentFieldsAsyncCallback( )
		{
			_pendingReInitializeCurrentFields = false;

			this.ReInitializeCurrentFields( );
		}

		#endregion // ReInitializeCurrentFieldsAsync

		#region SelectFieldHelper

		internal void SelectFieldHelper( Field field )
		{
			FieldChooserEntry entry = this.FindFieldChooserEntry( field );
			
			Debug.Assert( null != entry );
			if ( null != entry )
				this.SelectedField = entry;
		}

		#endregion // SelectFieldHelper

		#region SetFieldDragManagerHelper

		/// <summary>
		/// Called when the user starts dragging a label presenter from the field chooser.
		/// </summary>
		/// <param name="fieldDragManager">Field drag manager that's managing the drag operation.</param>
		internal void SetFieldDragManagerHelper( FieldDragManager fieldDragManager )
		{
			Debug.Assert( null == _fieldDragManager || null == fieldDragManager );
			
			_fieldDragManager = fieldDragManager;

			bool isDragging = null != _fieldDragManager;
			this.SetValue( IsDraggingItemPropertyKey, KnownBoxes.FromValue( isDragging ) );
		}

		#endregion // SetFieldDragManagerHelper

		#region SyncAllCurrentFieldsVisible

		/// <summary>
		/// Called to set the AllCurrentFieldsVisible property based on the visibility of all the fields.
		/// </summary>
		private void SyncAllCurrentFieldsVisible( )
		{
			int visibleFieldCount = 0;
			int hiddenFieldCount = 0;

			if ( null != this.CurrentFields )
			{
				foreach ( FieldChooserEntry ii in this.CurrentFields )
				{
					if ( ii.IsVisible )
						visibleFieldCount++;
					else
						hiddenFieldCount++;
				}
			}

			bool? newVal = ( 0 != visibleFieldCount ) ^ ( 0 != hiddenFieldCount )
				? 0 != visibleFieldCount
				: (bool?)null;

			this.InternalSetAllCurrentFieldsVisible( newVal );
		}

		#endregion // SyncAllCurrentFieldsVisible

		#region SyncWithAllCurrentFieldsVisible

		/// <summary>
		/// Called when AllCurrentFieldsVisible changes to set the visibility of all the fields accordingly.
		/// </summary>
		private void SyncWithAllCurrentFieldsVisible( )
		{
			if ( _processingAllCurrentFieldsVisiblePropChange )
				return;

			_processingAllCurrentFieldsVisiblePropChange = true;
			try
			{
				bool? allFieldsVisible = this.AllCurrentFieldsVisible;
				if ( allFieldsVisible.HasValue )
				{
					if ( null != this.CurrentFields )
					{
						List<FieldChooserEntry> entries = new List<FieldChooserEntry>( this.CurrentFields );

						foreach ( FieldChooserEntry ii in entries )
						{
							ii.IsVisible = allFieldsVisible.Value;
						}
					}
				}
			}
			finally
			{
				_processingAllCurrentFieldsVisiblePropChange = false;
			}
		}

		#endregion // SyncWithAllCurrentFieldsVisible

		#region UpdateIsDraggingItemFromDataPresenter

		/// <summary>
		/// Updates the IsDraggingItemFromDataPresenter property based on the value of the IsDraggingField
		/// property of the data presenter.
		/// </summary>
		internal void UpdateIsDraggingItemFromDataPresenter( )
		{
			DataPresenterBase dp = this.DataPresenter;

			bool newVal = null != dp && dp.IsDraggingField;
			this.SetValue( IsDraggingItemFromDataPresenterPropertyKey, KnownBoxes.FromValue( newVal ) );

			// When the data presenter stops dragging an item, clear the IsDragItemOver if it
			// was set.
			// 
			if ( !newVal )
				this.SetValue( IsDragItemOverPropertyKey, KnownBoxes.FalseBox );
		}

		#endregion // UpdateIsDraggingItemFromDataPresenter

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region ICommandHost Members

		bool ICommandHost.CanExecute( ExecuteCommandInfo commandInfo )
		{
			ICommand command = commandInfo.Command;

			if ( FieldChooserCommands.ToggleVisibility == command )
			{
				FieldChooserEntry entry = this.GetFieldFromCommandInfo( commandInfo, true );
				if ( null != entry )
					return true;
			}
			else if ( FieldChooserCommands.Stop == command )
			{
				if ( this.IsMouseCaptureWithin )
					return true;
			}

			return false;
		}

		// SSP 3/18/10 TFS29783 - Optimizations
		// 
		//long ICommandHost.CurrentState
		long ICommandHost.GetCurrentState( long statesToQuery )
		{
			FieldChooserStates state = 0;

			FieldChooserEntry selectedField = this.SelectedField;
			if ( null != selectedField )
			{
				state |= FieldChooserStates.ItemSelected;

				if ( selectedField.IsVisible )
					state |= FieldChooserStates.SelectedItemVisible;
			}

			if ( this.IsDraggingItem )
				state |= FieldChooserStates.IsDraggingItem;

			return (long)state & statesToQuery;
		}

		bool ICommandHost.Execute( ExecuteCommandInfo commandInfo )
		{
			return this.ExecuteCommandImpl( commandInfo );
		}

		#endregion
	}

	#endregion // FieldChooser Class


	#region FieldChooserCommands Class

	/// <summary>
	/// Provides the list of RoutedCommands supported by the <see cref="FieldChooser"/>. 
	/// </summary>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	public class FieldChooserCommands : Commands<FieldChooser>
	{
		// AS 3/15/07 BR21148
		private const ModifierKeys AllCtrlShiftModifiers = ModifierKeys.None | ModifierKeys.Shift | ModifierKeys.Control;

		// ====================================================================================================================================
		// ADD NEW COMMANDS HERE with the minimum required control state (also add a CommandWrapper for each command to the CommandWrappers array
		// below which will let you specify the triggering KeyGestures and required/disallowed states)
		//
		// Note that while individual commands in this static list are defined as type RoutedCommand or RoutedUICommand,
		// we actually create IGRoutedCommands or IGRoutedUICommands (both derived from RoutedCommand) so we can specify
		// and store the minimum control state needed to execute the command.
		// ------------------------------------------------------------------------------------------------------------------------------------
		//

		#region Command Definitions

		/// <summary>
		/// Clears all currently selected items.
		/// </summary>
		public static readonly RoutedCommand ToggleVisibility = new IGRoutedCommand( "ToggleVisibility",
																					  typeof( FieldChooserCommands ),
																					  (Int64)0,
																					  (Int64)FieldChooserStates.ItemSelected );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		/// <summary>
		/// Cancels any current drag operations.
		/// </summary>
		public static readonly RoutedCommand Stop = ApplicationCommands.Stop;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

		#endregion //Command Definitions

		// ====================================================================================================================================


		// ====================================================================================================================================
		// ADD COMMANDWRAPPERS HERE FOR EACH COMMAND DEFINED ABOVE.
		// ------------------------------------------------------------------------------------------------------------------------------------
		//
		/// <summary>
		/// The list of CommandWrappers for each supported command.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.Commands.CommandWrapper"/>

		#region CommandWrapper Definitions

		private static CommandWrapper[] GetCommandWrappers( )
		{
			return new CommandWrapper[] {
				//					RoutedCommand					StateDisallowed					StateRequired										InputGestures
				//					=============					===============					=============										=============
				new CommandWrapper(	ToggleVisibility,				(Int64)0,						(Int64)FieldChooserStates.ItemSelected,				new InputGestureCollection(new KeyGesture[] { new KeyGesture( Key.Space ) })),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
				new CommandWrapper(	Stop,							(Int64)0,						(Int64)FieldChooserStates.IsDraggingItem,			new InputGestureCollection(new KeyGesture[] { new KeyGesture( Key.Escape ) })),
				//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
			};
		}
		#endregion //CommandWrapper Definitions

		// ====================================================================================================================================


		static FieldChooserCommands( )
		{
			// Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
			// by our CommandWrappers.
			Commands<FieldChooser>.Initialize( FieldChooserCommands.GetCommandWrappers( ) );
		}


		/// <summary>
		/// This method is provided as a convenience for initializing the statics in this class which kicks off
		/// the process of setting up and registering the commands.
		/// </summary>
		public static void LoadCommands( )
		{
		}

		private static FieldChooserCommands g_instance;
		internal static FieldChooserCommands Instance
		{
			get
			{
				if ( g_instance == null )
					g_instance = new FieldChooserCommands( );

				return g_instance;
			}
		}

		#region CreateGestureCombinations





		private static InputGestureCollection CreateGestureCombinations( Key key, ModifierKeys modifiers )
		{
			InputGestureCollection gestures = new InputGestureCollection( );

			AddGesture( gestures, key, modifiers, ModifierKeys.Shift | ModifierKeys.Control );
			AddGesture( gestures, key, modifiers, ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt );
			AddGesture( gestures, key, modifiers, ModifierKeys.Shift | ModifierKeys.Alt );
			AddGesture( gestures, key, modifiers, ModifierKeys.Alt | ModifierKeys.Control );
			AddGesture( gestures, key, modifiers, ModifierKeys.Shift );
			AddGesture( gestures, key, modifiers, ModifierKeys.Control );
			AddGesture( gestures, key, modifiers, ModifierKeys.Alt );
			AddGesture( gestures, key, modifiers, ModifierKeys.None );

			return gestures;
		}
		#endregion //CreateGestureCombinations

		#region AddGesture
		private static void AddGesture( InputGestureCollection gestures, Key key, ModifierKeys gestureModifiers, ModifierKeys modifierToCheck )
		{
			if ( ( gestureModifiers & modifierToCheck ) == modifierToCheck )
				gestures.Add( new KeyGesture( key, modifierToCheck ) );
		}
		#endregion //AddGesture

		#region ProcessKeyboardInput

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		/// <param name="e">The key event arguments</param>
		/// <param name="commandHost">The command host</param>
		/// <returns></returns>
		public override bool ProcessKeyboardInput( KeyEventArgs e, ICommandHost commandHost )
		{
			FieldChooser fieldChooser = commandHost as FieldChooser;

			if ( null != fieldChooser )
			{
				// if the element with focus is not in our focus scope then don't process the key and return false               
				if ( Keyboard.FocusedElement is DependencyObject &&
					FocusManager.GetFocusScope( (DependencyObject)Keyboard.FocusedElement ) != FocusManager.GetFocusScope( fieldChooser ) )
				{
					return false;
				}
			}

			return base.ProcessKeyboardInput( e, commandHost );
		}

		#endregion //ProcessKeyboardInput
	}

	#endregion // FieldChooserCommands Class

	#region FieldChooserStates Enum

	/// <summary>
	/// Represents the different states of the FieldChooser control. Used to evaluate whether a specific 
	/// command can be executed.
	/// </summary>
	[Flags]
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	public enum FieldChooserStates : long
	{
		/// <summary>
		/// A field is selected in the field chooser.
		/// </summary>
		ItemSelected			= 0x1,

		/// <summary>
		/// Indicates whether the selected field is visible in the data presenter.
		/// </summary>
		SelectedItemVisible		= 0x2,

		/// <summary>
		/// Indicates whether the user is currently dragging an item from the field chooser.
		/// </summary>
		IsDraggingItem			= 0x4
	}

	#endregion // FieldChooserStates Enum
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