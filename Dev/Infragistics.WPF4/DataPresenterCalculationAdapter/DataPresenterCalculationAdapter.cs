using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Linq;


using Infragistics.Windows.Licensing;


using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using Infragistics.Calculations;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Internal;
using Infragistics.Calculations.Engine;

namespace Infragistics.Windows.DataPresenter.Calculations 
{
	/// <summary>
	/// Class that exposes <see cref="DataPresenterBase"/> objects to a <see cref="XamCalculationManager"/>'s network.
	/// </summary>
	public sealed class DataPresenterCalculationAdapter : DependencyObject, ICalculationParticipant, IDataPresenterCalculationAdapterInternal, ISupportPropertyChangeNotifications
	{
		#region Member Variables

		private DataPresenterBase		_dataPresenter;
		private XamCalculationManager	_cachedCalcManager;
		private string					_referenceId;

		private PropertyValueTracker _calcManagerPvt;
		private PropertyValueTracker _dpCalcManagerPvt;

		private DataPresenterReference _rootReference;
		private DataPresenterBase _verify_lastDataPresenter;
		private XamCalculationManager _verify_lastCalcManager;
		private PropertyChangeListenerList _listeners = new PropertyChangeListenerList( );
		private int _dpFieldLayoutsVersion;
		private int _verify_lastDpFieldLayoutVersion;
		private bool _verifyAsyncPending;

		#endregion //Member Variables

		#region Constructor

		static DataPresenterCalculationAdapter()
		{
			XamCalculationManager.RegisterFormulaProviderResolver( new FormulaProviderResolver( ) );
		}

		/// <summary>
		/// Initializes a new <see cref="DataPresenterCalculationAdapter"/>
		/// </summary>
		public DataPresenterCalculationAdapter()
		{
			_calcManagerPvt = new PropertyValueTracker( this, XamCalculationManager.CalculationManagerProperty, this.VerifyCalcManager );
		}

		#endregion // Constructor
		
		#region Properties

		#region Public Properties

		#region DataPresenter

		private static readonly DependencyPropertyKey DataPresenterPropertyKey =
			DependencyProperty.RegisterReadOnly("DataPresenter",
			typeof(DataPresenterBase), typeof(DataPresenterCalculationAdapter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDataPresenterChanged)));
		
		private static void OnDataPresenterChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			DataPresenterCalculationAdapter adapter = target as DataPresenterCalculationAdapter;

			DataPresenterBase newDataPresenter = e.NewValue as DataPresenterBase;
			adapter._dataPresenter = newDataPresenter;

			adapter._dpCalcManagerPvt = null == newDataPresenter ? null
				: new PropertyValueTracker( newDataPresenter, XamCalculationManager.CalculationManagerProperty, adapter.VerifyCalcManager );

			adapter.VerifyCalcManager( );
			adapter.Verify( );
		}

		/// <summary>
		/// Identifies the <see cref="DataPresenter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataPresenterProperty =
			DataPresenterPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated DataPresenter (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this is initialized by setting the <see cref="DataPresenterBase"/>'s <see cref="DataPresenterBase.CalculationAdapter"/> property to this object.</para>
		/// </remarks>
		/// <seealso cref="DataPresenterProperty"/>
		/// <seealso cref="DataPresenterBase.CalculationAdapter"/>
		/// <seealso cref="DataPresenterBase"/>
		[Browsable(false)]
		[ReadOnly(true)]
		public DataPresenterBase DataPresenter
		{
			get
			{
				return _dataPresenter;
			}
		}

		#endregion //DataPresenter

		#region ReferenceId

		/// <summary>
		/// Identifies the <see cref="ReferenceId"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ReferenceIdProperty = DependencyProperty.Register("ReferenceId",
			typeof(string), typeof(DataPresenterCalculationAdapter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnReferenceIdChanged)));

		private static void OnReferenceIdChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			DataPresenterCalculationAdapter adapter = target as DataPresenterCalculationAdapter;

			adapter._referenceId = e.NewValue as string;

			adapter.Verify( );
		}

		/// <summary>
		/// Gets/sets the root reference Id for any calculation references exposed by the <see cref="DataPresenter"/> for use in the associated <see cref="XamCalculationManager"/>'s network.
		/// </summary>
		/// <seealso cref="DataPresenter"/>
		/// <seealso cref="ReferenceIdProperty"/>
		[Description("Gets/sets the root reference Id for any calculation references exposed by the DataPresenter for use in the CalculationManager's network.")]
		[Category("Data")]
		public string ReferenceId
		{
			get
			{
				return _referenceId;
			}
			set
			{
				this.SetValue(DataPresenterCalculationAdapter.ReferenceIdProperty, value);
			}
		}

		#endregion //ReferenceId
		
		#endregion //Public Properties

		#region Internal Properties

		#region CalcManager

		/// <summary>
		/// Gets the associated calculation manager instance if any.
		/// </summary>
		internal XamCalculationManager CalcManager
		{
			get
			{
				return _cachedCalcManager;
			}
		}

		#endregion // CalcManager

		#region VisibleRecords

		/// <summary>
		/// Gets the records that are visible on screen. This is used to support recalc deferred mode where
		/// we only calculate visible records.
		/// </summary>
		internal IEnumerable<Record> VisibleRecords
		{
			get
			{
				return null;
			}
		} 

		#endregion // VisibleRecords

		#endregion // Internal Properties

		#endregion //Properties	

		#region Methods

		#region Private Methods

		#region DirtyRootReference

		private void DirtyRootReference( bool verify )
		{
			_dpFieldLayoutsVersion++;

			if ( verify && null != _rootReference )
				this.Verify( );
		} 

		#endregion // DirtyRootReference

		#region OnFieldLayouts_CollectionChanged

		/// <summary>
		/// Event handler for CollectionChanged of the data presenter's field layouts collection.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFieldLayouts_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			this.DirtyRootReference( true );
		}

		#endregion // OnFieldLayouts_CollectionChanged

		#region ResolveElementName

		private string ResolveElementName( )
		{
			string elementName = _referenceId;
			if ( string.IsNullOrEmpty( elementName ) && null != _dataPresenter )
				elementName = _dataPresenter.Name;

			return null != elementName ? RefParser.EscapeString( elementName, false ) : null;
		}

		#endregion // ResolveElementName

		#region Verify

		private void Verify( )
		{
			if ( null == _dataPresenter || !_dataPresenter.IsInitialized )
				return;

			string elementName = this.ResolveElementName( );
			if ( _verify_lastDataPresenter != _dataPresenter || _verify_lastCalcManager != this.CalcManager
				|| null != _rootReference && _rootReference.ElementName != elementName 
				|| _verify_lastDpFieldLayoutVersion != _dpFieldLayoutsVersion )
			{
				_verify_lastDpFieldLayoutVersion = _dpFieldLayoutsVersion;
				var calcManager = this.CalcManager;
				ICalculationManager iiCalcManager = calcManager;

				if ( null != _verify_lastDataPresenter )
					_verify_lastDataPresenter.FieldLayouts.CollectionChanged -= new NotifyCollectionChangedEventHandler( OnFieldLayouts_CollectionChanged );

				var oldRootReference = _rootReference;
				if ( null != oldRootReference )
				{
					_rootReference = null;
					ICalculationManager oldCalcManager = oldRootReference.CalcManager;
					oldCalcManager.RemoveReference( oldRootReference );
					oldCalcManager.RemoveParticipant( this );
					_verify_lastDataPresenter = null;
					_verify_lastCalcManager = null;
				}

				if ( null == _rootReference && null != _dataPresenter && null != calcManager && !string.IsNullOrEmpty( elementName )
					&& null != _dataPresenter.DataSource )
				{
					_rootReference = new DataPresenterReference( this, elementName );
					iiCalcManager.AddParticipant( this );
					iiCalcManager.AddReference( _rootReference );

					_verify_lastDataPresenter = _dataPresenter;
					_verify_lastCalcManager = calcManager;

					_dataPresenter.FieldLayouts.CollectionChanged += new NotifyCollectionChangedEventHandler( OnFieldLayouts_CollectionChanged );
				}

				if ( oldRootReference != _rootReference )
					_listeners.OnPropertyValueChanged( this, "RootReference", null );
			}
		}

		#endregion // Verify

		#region VerifyAsync

		private void VerifyAsync( )
		{
			if ( !_verifyAsyncPending )
			{
				_verifyAsyncPending = true;
				this.Dispatcher.BeginInvoke( DispatcherPriority.Background, new Action( this.VerifyAsyncHandler ) );
			}
		}

		#endregion // VerifyAsync

		#region VerifyAsyncHandler

		private void VerifyAsyncHandler( )
		{
			_verifyAsyncPending = false;
			this.Verify( );
		}

		#endregion // VerifyAsyncHandler

		#region VerifyCalcManager

		private void VerifyCalcManager( )
		{
			var calcManager = XamCalculationManager.GetCalculationManager( this );

			if ( null == calcManager )
			{
				var dp = this.DataPresenter;
				calcManager = null != dp ? XamCalculationManager.GetCalculationManager( dp ) : null;
			}

			if ( _cachedCalcManager != calcManager )
			{
				_cachedCalcManager = calcManager;
				_listeners.OnPropertyValueChanged( this, "CalcManager", null );

				this.Verify( );
			}
		}

		#endregion // VerifyCalcManager

		#endregion // Private Methods 

		#region Internal Methods

		#region GetReference

		internal FieldReference GetReference( Field field )
		{
			return null != _rootReference ? (FieldReference)_rootReference._utils.GetFieldReference( field, false ) : null;
		}

		internal SummaryDefinitionReference GetReference( SummaryDefinition summary )
		{
			return null != _rootReference ? (SummaryDefinitionReference)_rootReference._utils.GetSummaryDefinitionReference( summary, false ) : null;
		}

		#endregion // GetReference 

		#endregion // Internal Methods

		#endregion // Methods

		#region ICalculationParticipant Members

		CalculationReferenceNode ICalculationParticipant.GetReferenceTree(object formulaTarget)
		{
			CalculationReferenceNode root = null;

			if ( null != _rootReference )
			{
				root = new CalculationReferenceNode( _rootReference.ElementName, false, ReferenceNodeType.Control )
				{
					Reference = _rootReference
				};

				var childRefs = _rootReference.GetChildReferences( ChildReferenceType.ReferencesWithFormulas );
				if ( null != childRefs )
				{
					ObservableCollection<CalculationReferenceNode> iiList = new ObservableCollection<CalculationReferenceNode>( );

					foreach ( var ii in childRefs )
					{
						var iiNode = new CalculationReferenceNode( ii.ElementName, false, ReferenceNodeType.Control )
						{
							Reference = ii
						};

						iiList.Add( iiNode );

						ObservableCollection<CalculationReferenceNode> jjListFields = new ObservableCollection<CalculationReferenceNode>( );
						ObservableCollection<CalculationReferenceNode> jjListSummaries = new ObservableCollection<CalculationReferenceNode>( );
						var iiChildNodes = ii.GetChildReferences( ChildReferenceType.ReferencesWithFormulas );
						if ( null != iiChildNodes )
						{
							foreach ( var jj in iiChildNodes )
							{
								var jjNode = new CalculationReferenceNode( jj.ElementName, true, ReferenceNodeType.Control )
								{
									Reference = jj
								};

								if ( jjNode.Reference is FieldReference )
									jjListFields.Add( jjNode );
								else
									jjListSummaries.Add( jjNode );
							}
						}

						// Make summaries subnode a sibling of fields.
						// 
						if ( jjListSummaries.Count > 0 )
						{
							var summariesNode = new CalculationReferenceNode( "Summaries", false, ReferenceNodeType.Control )
							{
								ChildReferences = new ReadOnlyObservableCollection<CalculationReferenceNode>( jjListSummaries )
							};

							summariesNode.SortPriority = 1;
							summariesNode.IsExpanded = true;
							jjListFields.Add( summariesNode );
						}

						iiNode.ChildReferences = new ReadOnlyObservableCollection<CalculationReferenceNode>( jjListFields );
					}

					root.ChildReferences = new ReadOnlyObservableCollection<CalculationReferenceNode>( iiList );
				}			
			}

			return root;
		}

		Infragistics.Calculations.Engine.ICalculationReference ICalculationParticipant.RootReference
		{
			get 
			{
				return _rootReference;
			}
		}

		#endregion // ICalculationParticipant Members

		#region IDataPresenterCalculationAdapterInternal Members

		DataPresenterBase IDataPresenterCalculationAdapterInternal.DataPresenter
		{
			get
			{
				return _dataPresenter;
			}
			set
			{
				this.SetValue(DataPresenterPropertyKey, value);
			}
		}

		#endregion // IDataPresenterCalculationAdapterInternal Members

		#region ITypedPropertyChangeListener

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged( object dataItem, string property, object extraInfo )
		{
			if ( _dataPresenter == dataItem )
			{
				switch ( property )
				{
					case "Initialized":
					case "DataSource":
						this.Verify( );
						break;
				}
			}
			else
			{
				var utils = null != _rootReference ? _rootReference._utils : null;
				var calcManager = this.CalcManager;
				var iiCalcManager = calcManager as ICalculationManager;
				bool handleChangeNotifications = null != utils && null != calcManager && _rootReference.IsValid;

				FieldReference fieldRef = null;
				RecordReferenceBase recordRef = null;
				if ( handleChangeNotifications )
				{
					if ( extraInfo is Field )
						fieldRef = (FieldReference)utils.GetFieldReference( (Field)extraInfo, false );

					if ( dataItem is Record )
						recordRef = utils.GetRecordReference( (Record)dataItem, false );
				}


				switch ( property )
				{
					case "CellValue":
						{
							if ( handleChangeNotifications )
							{
								var cellRef = utils.GetCellReference( (DataRecordReference)recordRef, fieldRef, false );
								if ( null != cellRef )
									cellRef.NotifyCalcEngineValueChanged( );
							}
						}
						break;
					case "Record":
						{
							if ( handleChangeNotifications )
							{
								if ( null != recordRef )
									recordRef.NotifyCalcEngineValueChanged( );
							}
						}
						break;
					case "Add":
						{
							if ( handleChangeNotifications )
							{
								Record dr = (Record)dataItem;

								recordRef = recordRef ?? utils.GetRecordReference( dr, true );
								if ( null != recordRef )
									iiCalcManager.AddRowReference( recordRef );
							}
						}
						break;
					case "IsAddRecord":
					case "Visibility":
						{
							if ( handleChangeNotifications )
							{
								Record dr = (Record)dataItem;
								if ( null != dr )
								{
									if ( utils.IsIncludedInCalcScope( dr ) )
									{
										recordRef = recordRef ?? utils.GetRecordReference( dr, true );
										if ( null != recordRef )
											iiCalcManager.AddRowReference( recordRef );
									}
									else if ( null != recordRef )
									{
										iiCalcManager.RemoveReference( recordRef );
									}
								}
							}
						}
						break;
					case "Remove":
						{
							if ( handleChangeNotifications )
							{
								if ( null != recordRef )
									iiCalcManager.RemoveRowReference( recordRef );
							}
						}
						break;
					case "Reset":
					case "Sorted":
					case "ItemMoved":
						{
							bool reset = "Reset" == property;

							if ( handleChangeNotifications )
							{
								RecordCollectionBase rc = dataItem as RecordCollectionBase;

								if ( null == rc && dataItem is ViewableRecordCollection )
									rc = ( (ViewableRecordCollection)dataItem ).RecordCollection;

								if ( null != rc )
								{
									var rcRef = utils.GetRecordCollectionReference( rc, false );
									if ( null != rcRef )
									{
										if ( reset )
											iiCalcManager.RowsCollectionReferenceResynched( rcRef );
										else
											iiCalcManager.RowsCollectionReferenceSorted( rcRef );
									}
								}
								else
									Debug.Assert( false );
							}
						}
						break;
					case "Description":
					case "ParentFieldLayout":
						{
							FieldLayout fl = dataItem as FieldLayout;
							if ( null != fl )
							{
								this.DirtyRootReference( false );
								this.VerifyAsync( );
							}
						}
						break;
					case "Fields":
						{
							FieldLayout fl = dataItem as FieldLayout;
							FieldLayoutReference flRef = null != fl && null != _rootReference
								? _rootReference.GetFieldLayoutReference( fl, false ) as FieldLayoutReference 
								: null;

							if ( null != flRef )
								flRef.ProcessFieldsChanged( );
						}
						break;
				}
			}
		} 

		#endregion // ITypedPropertyChangeListener

		#region ITypedSupportPropertyChangeNotifications

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			_listeners.Add( listener, useWeakReference );
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			_listeners.Remove( listener );
		} 

		#endregion // ITypedSupportPropertyChangeNotifications
	}

	internal class FormulaProvider : PropertyChangeNotifierExtended, IFormulaProvider
	{
		private PropertyValueTracker _adapterPvt;
		private object _target;
		private DataPresenterCalculationAdapter _adapter;

		internal FormulaProvider( object target )
		{
			_target = target;

			string adapterPath;

			if ( target is Field )
			{
				adapterPath = "Owner.DataPresenter.CalculationAdapter";
			}
			else if ( target is SummaryDefinition )
			{
				adapterPath = "FieldLayout.DataPresenter.CalculationAdapter";
			}
			else
				throw new ArgumentException( );

			_adapterPvt = new PropertyValueTracker( _target, adapterPath, this.OnAdapterChanged );
			this.OnAdapterChanged( );
		}

		private void OnAdapterChanged( )
		{
			var newAdapter = (DataPresenterCalculationAdapter)_adapterPvt.Target;
			if ( _adapter != newAdapter )
				PropertyChangeListenerList.ManageListenerHelper( ref _adapter, newAdapter, this, true );

			this.Verify( );
		}

		internal override void OnSubObjectPropertyChanged( object sender, string property, object extraInfo )
		{
			base.OnSubObjectPropertyChanged( sender, property, extraInfo );

			if ( sender == _adapter )
			{
				switch ( property )
				{
					case "RootReference":
						this.Verify( );
						break;
				}
			}
		}

		private void Verify( )
		{
			Exception error;
			var newFormulaProvider = this.GetFormulaProvider( _target, out error );

			if ( newFormulaProvider != _formulaProvider )
			{
				_formulaProvider = newFormulaProvider;
				this.RaisePropertyChangedEvent( string.Empty );
			}
		}

		private ICalculationReference GetReference( object target )
		{
			if ( null != _adapter )
			{
				Field field = target as Field;
				if ( null != field )
					return _adapter.GetReference( field );

				SummaryDefinition summary = target as SummaryDefinition;
				if ( null != summary )
					return _adapter.GetReference( summary );
			}

			return null;
		}

		public IFormulaProvider GetFormulaProvider( object target, out Exception error )
		{
			error = null;

			ICalculationReference rr = this.GetReference( target );

			FieldReference fieldRef = rr as FieldReference;
			if ( null != fieldRef )
				return fieldRef._referenceManager;

			SummaryDefinitionReference summaryRef = rr as SummaryDefinitionReference;
			if ( null != summaryRef )
				return summaryRef._referenceManager;

			error = new Exception( "Target is not supported." );
			return null;
		}

		public bool SupportsTarget( object target )
		{
			return target is Field || target is SummaryDefinition;
		}



		private IFormulaProvider _formulaProvider;

		public ICalculationManager CalculationManager
		{
			get 
			{ 
				return null != _formulaProvider ? _formulaProvider.CalculationManager : null;
			}
		}

		public string Formula
		{
			get
			{
				return null != _formulaProvider ? _formulaProvider.Formula : null;
			}
			set
			{
				if ( null != _formulaProvider )
				{
					_formulaProvider.Formula = value;
				}
				else
				{
					Debug.Assert( false );
				}
			}
		}

		public ICalculationParticipant Participant
		{
			get 
			{ 
				return null != _formulaProvider ? _formulaProvider.Participant : null;
			}
		}

		public ICalculationReference Reference
		{
			get 
			{ 
				return null != _formulaProvider ? _formulaProvider.Reference : null;
			}
		}
	}


	internal class FormulaProviderResolver : PropertyChangeNotifierExtended, IFormulaProviderResolver
	{
		internal FormulaProviderResolver( )
		{
		}

		public IFormulaProvider GetFormulaProvider( object target, out Exception error )
		{
			error = null;
			return new FormulaProvider( target );
		}

		public bool SupportsTarget( object target )
		{
			return target is Field || target is SummaryDefinition;
		}
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