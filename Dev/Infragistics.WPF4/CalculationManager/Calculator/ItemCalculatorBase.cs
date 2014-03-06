using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Infragistics.Calculations.Engine;
using System.Collections.Specialized;
using System.Collections;
using Infragistics.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Infragistics.Calculations
{
	/// <summary>
	/// Abstract base class for <see cref="ItemCalculator"/> and <see cref="ListCalculator"/>
	/// </summary>
	public abstract class ItemCalculatorBase : PropertyChangeNotifierExtended
	{
		#region Private Members

		private XamCalculationManager _manager;
		private string _referenceId;
		private string _defaultReferencId;
		private string _propertiesToInclude;
		private string _propertiesToExclude;
		private HashSet<string> _propsToInclude;
		private HashSet<string> _propsToExclude;
		private Dictionary<string, ItemCalculationBase> _refIdDictionary = new Dictionary<string, ItemCalculationBase>(StringComparer.InvariantCultureIgnoreCase);
		private bool _asyncRegisterPending;
		private bool _referencesDirty;
		private IValueConverter _valueConverter;

		#endregion //Private Members	
    
		#region Constructors

		static ItemCalculatorBase()
		{
			XamCalculationManager.RegisterFormulaProviderResolver(new FormulaProviderResolver());
		}

		internal ItemCalculatorBase() { }

		#endregion //Constructors	

		#region Base class overrides

		#region OnSubObjectPropertyChanged

		internal override void OnSubObjectPropertyChanged(object sender, string property, object extraInfo)
		{
			XamCalculationManager mgr = sender as XamCalculationManager;

			// if the mgr was just initialized then we can register our references
			if (mgr != null && property == "IsInitialized")
			{
				// First unwire the prop change notifications since we don't need them anymore
				((ITypedSupportPropertyChangeNotifications<object, string>)mgr).RemoveListener(this);

				if (mgr == _manager)
					this.RegisterReferencesAsync();
			}

			base.OnSubObjectPropertyChanged(sender, property, extraInfo);
		}

		#endregion //OnSubObjectPropertyChanged	
    
		#endregion //Base class overrides	

		#region DataAccessError

		/// <summary>
		/// Occurs when an exception is thrown while attempting to get or set a property on an item.
		/// </summary>
		public event EventHandler<DataAccessErrorEventArgs> DataAccessError;

		internal void RaiseDataAccessErrorEvent(DataAccessErrorEventArgs e)
		{
			if (this.DataAccessError != null)
				this.DataAccessError(this, e);

			Utils.LogDebuggerError(e.ErrorMessage + Environment.NewLine);
		}

		#endregion //DataAccessError	
    
		#region Properties

		#region Public Properties

		#region CalculationManager

		/// <summary>
		/// CalculationManager used to perform calculations.
		/// </summary>
		public XamCalculationManager CalculationManager
		{
			get { return _manager; }
			set
			{
				if (value != _manager)
				{
					this.UnregisterReferences();

					_manager = value;

					if (_manager != null)
					{
						// if the mgr is initialized then register the references asynchronously.
						if (_manager.IsInitialized)
						{
							this.RegisterReferencesAsync();
						}
						else
						{
							// since the mgr isn't initialized wire if prop change and
							// wait until it is
							((ITypedSupportPropertyChangeNotifications<object, string>)_manager).AddListener(this, false);
						}

					}

					this.RaisePropertyChangedEvent("CalculationManager");
				}
			}
		}

		#endregion //CalculationManager

		#region PropertiesToExclude

		/// <summary>
		/// Identifies properties, in a case-insensitive comma delimited list, of an item to exclude from the calculation network
		/// </summary>
		/// <value>A comma delimited list of case-insensitive property names to exclude from the calculation network.</value>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if neither <see cref="PropertiesToExclude"/> nor <see cref="PropertiesToInclude"/> are specified then all public instance properties will be exposed to the calculation network.
		/// If both settings are specified and the same property is in both then that property will be excluded.</para>
		/// </remarks>
		/// <seealso cref="PropertiesToInclude"/>
		public string PropertiesToExclude
		{
			get { return _propertiesToExclude; }
			set
			{
				if (value != _propertiesToExclude)
				{
					_propertiesToExclude = value;

					// null out the cached string array so it will get re-created on the next get of PropsToExclude
					_propsToExclude = null;

					// dirty all references 
					this.DirtyAllReferencesAsync();

					this.RaisePropertyChangedEvent("PropertiesToExclude");
				}
			}
		}

		#endregion //PropertiesToExclude

		#region PropertiesToInclude

		/// <summary>
		/// Identifies properties, in a case-insensitive comma delimited list, of an item to expose to the calculation network
		/// </summary>
		/// <value>A comma delimited list of case-insensitive property names to expose to the calculation network.</value>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if neither <see cref="PropertiesToExclude"/> nor <see cref="PropertiesToInclude"/> are specified then all public instance properties will be exposed to the calculation network.</para>
		/// </remarks>
		/// <seealso cref="PropertiesToInclude"/>
		public string PropertiesToInclude
		{
			get { return _propertiesToInclude; }
			set
			{
				if (value != _propertiesToInclude)
				{
					_propertiesToInclude = value;

					// null out the cached string array so it will get re-created on the next get of PropsToInclude
					_propsToInclude = null;

					// dirty all references 
					this.DirtyAllReferencesAsync();

					this.RaisePropertyChangedEvent("PropertiesToInclude");
				}
			}
		}

		#endregion //PropertiesToInclude

		#region ReferenceId

		/// <summary>
		/// Identifies the calculator to the calculation network.
		/// </summary>
		public string ReferenceId
		{
			get { return _referenceId; }
			set
			{
				if (value != _referenceId)
				{
					_referenceId = value;

					this.OnReferenceIdResolvedChanged();
				}
			}
		}

		#endregion //ReferenceId

		#region ValueConverter

		/// <summary>
		/// Specifies the converter to use to convert between the underlying value of the source item to the value
		/// that's used in calculations.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this converter's <see cref="System.Windows.Data.IValueConverter.Convert"/> will be called when getting source values from an item.
		/// Its <see cref="System.Windows.Data.IValueConverter.ConvertBack"/> method will be called when setting target property values, used only for <see cref="ItemCalculation"/>s
		/// where the <see cref="ItemCalculation.TargetProperty"/> has been specified. In both cases the property name will be passed as the parameter into each method.</para>
		/// </remarks>
		public IValueConverter ValueConverter
		{
			get
			{
				return _valueConverter;
			}
			set
			{
				if (_valueConverter != value)
				{
					_valueConverter = value;
					this.RaisePropertyChangedEvent("ValueConverter");
				}
			}
		}

		#endregion // ValueConverter

		#endregion //Public Properties	
 
		#region Internal Properties

		#region Culture

		internal CultureInfo Culture { get; set; }

		#endregion //Culture	
    
		#region CultureResolved

		internal CultureInfo CultureResolved
		{
			get
			{
				CultureInfo culture = this.Culture;

				if (culture != null)
					return culture;

				return CultureInfo.CurrentCulture;
			}
		}

		#endregion //CultureResolved	

		// JJD 10/27/11 - TFS92815
		#region KnownType

		// Special case known types (e.g. string, int, double, dateTime etc.). For these types
		// we expose a single pseudo read-only property named 'Value' which return the item itself
		internal abstract Type KnownType { get; }

		#endregion //KnownType	
    
		#region Participant

		internal abstract ICalculationParticipant Participant { get; }

		#endregion //Participant	
    
		#region PropsToExclude

		internal HashSet<string> PropsToExclude
		{
			get
			{
				if (_propsToExclude == null && !string.IsNullOrWhiteSpace(_propertiesToExclude))
				{
					_propsToExclude = new HashSet<string>(_propertiesToExclude.Split(','), StringComparer.InvariantCultureIgnoreCase);
				}

				return _propsToExclude;
			}
		}

		#endregion //PropsToExclude
 
		#region PropsToInclude

		internal HashSet<string> PropsToInclude
		{
			get
			{
				if (_propsToInclude == null && !string.IsNullOrWhiteSpace(_propertiesToInclude))
				{
					_propsToInclude = new HashSet<string>(_propertiesToInclude.Split(','), StringComparer.InvariantCultureIgnoreCase);
				}

				return _propsToInclude;
			}
		}

		#endregion //PropsToInclude

		#region ReferenceIdResolved

		internal string ReferenceIdResolved
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_referenceId))
				{
					if (string.IsNullOrWhiteSpace(_defaultReferencId))
						_defaultReferencId = Guid.NewGuid().ToString();

					return _defaultReferencId;
				}

				return _referenceId;
			}
		}

		#endregion //ReferenceIdResolved

		#region RootReference

		internal abstract ItemCalculatorReferenceBase RootReference { get; }

		#endregion //RootReference	
    
		#endregion //Internal Properties	
        
		#endregion //Properties	

		#region Methods

		#region Public Methods

		#region EnsureCalculated

		/// <summary>
		/// Ensures calculations are performed synchronously.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Depending upon the <see cref="XamCalculationManager.CalculationFrequency"/> property setting of the <see cref="XamCalculationManager"/>,
		/// calculations may be performed asynchronously on a timer. <b>EnsureCalculated</b> can be used to make sure calculations are
		/// performed right away.
		/// </para>
		/// </remarks>
		/// <seealso cref="XamCalculationManager.PerformCalculations"/>
		/// <seealso cref="XamCalculationManager.CalculationFrequency"/>
		public virtual void EnsureCalculated( )
		{
			if ( _asyncRegisterPending )
				this.RegisterReferences( );

			var calcManager = this.CalculationManager;
			var rootReference = this.RootReference;
			if ( null != calcManager && null != rootReference )
				this.EnsureCalculatedHelper( calcManager, rootReference );
		}

		#endregion // EnsureCalculated

		#endregion // Public Methods

		#region Internal Methods

		#region DirtyAllReferencesAsync

		internal void DirtyAllReferencesAsync()
		{
			this.UnregisterReferences();
			this.RegisterReferencesAsync();
		}

		#endregion //DirtyAllReferencesAsync

		#region DisposeReference

		internal static void DisposeReference(FormulaRefBase reference)
		{
			ListCalculationReference lc = reference as ListCalculationReference;

			if (lc != null)
			{
				lc.Dispose(true);
				return;
			}

			ListCalculatorColumnReference col = reference as ListCalculatorColumnReference;

			if (col != null)
			{
				col.Dispose(true);
				return;
			}

			ItemPropertyReference ipr = reference as ItemPropertyReference;

			if (ipr != null)
			{
				ipr.Dispose(true);
				return;
			}

			Debug.Assert(false, string.Format("Unknown reference type in dispose, reference: {0}", reference));
		}

		#endregion //DisposeReference	
    
		#region GetChildReferences

		internal abstract ICalculationReference[] GetChildReferences(ChildReferenceType referenceType);

		#endregion //GetChildReferences

		#region GetExistingReferences

		internal abstract List<FormulaRefBase> GetExistingReferences(ICollection collection);

		#endregion //GetExistingReferences

		#region GetReference

		internal abstract ICalculationReference GetReference(string referenceId, bool includeItemProperties = true, bool createIfNotFound = true, bool includeSummaries = true, bool includeColumns = true);

		#endregion //GetReference

		#region GetReferenceTree

		internal abstract CalculationReferenceNode GetReferenceTree(object formulaTarget);

		#endregion //GetReferenceTree

		#region InitializeDefaultRefId

		internal void InitializeDefaultRefId(string defaultRefId)
		{
			if (_defaultReferencId == defaultRefId)
				return;

			_defaultReferencId = defaultRefId;

			if (string.IsNullOrWhiteSpace(_referenceId))
				this.OnReferenceIdResolvedChanged();
		}

		#endregion //InitializeDefaultRefId	
    
		#region IsPropertyIncluded

		internal bool IsPropertyIncluded(string propertyName)
		{
			HashSet<string> exclusions = this.PropsToExclude;
			HashSet<string> inclusions = this.PropsToInclude;

			if (inclusions != null && !inclusions.Contains(propertyName))
				return false;

			if (exclusions != null && exclusions.Contains(propertyName))
				return false;

			return true;
		}

		#endregion //IsPropertyIncluded	
    
		#region OnCalculationsChanged

		internal void OnCalculationsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Reset:
					this.OnCalculationsReset(sender);
					break;
				case NotifyCollectionChangedAction.Add:
					this.OnCalculationsAdded(sender, e.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove:
					this.OnCalculationsRemoved(sender, e.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace:
					this.OnCalculationsRemoved(sender, e.OldItems);
					this.OnCalculationsAdded(sender, e.NewItems);
					break;

				case NotifyCollectionChangedAction.Move:
					// this is a NOOP
					return;

			}

			XamCalculationManager mgr = this.CalculationManager;

			if (mgr == null || !mgr.IsInitialized)
				return;

			this.DirtyAllReferencesAsync();
		}

		#endregion //OnCalculationsAdded
    
		#region OnCalculationAdded

		internal virtual void OnCalculationAdded(ItemCalculationBase calculation)
		{
			// listen in to any property changes from the calculation object
			ItemCalculationBase dummy = null;

			// initialize the calculation with a backward reference to the calculator
			calculation.Initialize(this);

			PropertyChangeListenerList.ManageListenerHelper<ItemCalculationBase>(ref dummy, calculation, this, true);

			string refId = calculation.ReferenceIdResolved;

			if (!string.IsNullOrWhiteSpace(refId))
			{
				ItemCalculationBase oldCalc;

				if ( _refIdDictionary.TryGetValue(refId, out oldCalc) && oldCalc != calculation )
					throw new InvalidOperationException(SRUtil.GetString("DuplicateReferenceId", refId, this));

				_refIdDictionary[refId] = calculation;
			}
		}

		#endregion //OnCalculationAdded

		#region OnCalculationsAdded

		internal void OnCalculationsAdded(object sender, IList newCalculations)
		{
			foreach (object o in newCalculations)
			{
				ItemCalculationBase calculation = o as ItemCalculationBase;

				if (calculation != null)
					this.OnCalculationAdded(calculation);
			}
		}

		#endregion //OnCalculationsAdded

		#region OnCalculationRefIdChanged

		internal virtual void OnCalculationRefIdChanged(ItemCalculationBase calculation, string oldRefId, string newRefId)
		{
			// first remove the old entry from the refId dictionary
			foreach (KeyValuePair<string, ItemCalculationBase> entry in _refIdDictionary)
			{
				if (entry.Value == calculation)
				{
					_refIdDictionary.Remove(entry.Key);
					break;
				}
			}

			string refId = calculation.ReferenceIdResolved;

			if (!string.IsNullOrWhiteSpace(refId))
			{
				if (_refIdDictionary.ContainsKey(refId))
					throw new InvalidOperationException(SRUtil.GetString("DuplicateReferenceId", refId, this));
				
				_refIdDictionary[refId] = calculation;
			}
		}

		#endregion //OnCalculationRefIdChanged	

		#region OnCalculationRemoved

		internal virtual void OnCalculationRemoved(ItemCalculationBase calculation)
		{
			// unwire from prop changes of the calculation object
			ItemCalculationBase dummy = calculation;

			// clear the backward reference to the calculator
			calculation.Initialize(null);

			PropertyChangeListenerList.ManageListenerHelper<ItemCalculationBase>(ref dummy, null, this, true);

			string refId = calculation.ReferenceIdResolved;

			if (!string.IsNullOrWhiteSpace(refId))
			{
				bool successful = _refIdDictionary.Remove(refId);

				//Debug.Assert(successful, "RefId not found in Dictionary on remove");
			}
			
		}

		#endregion //OnCalculationRemoved

		#region OnCalculationsRemoved

		internal void OnCalculationsRemoved(object sender, IList oldCalculations)
		{
			foreach (object o in oldCalculations)
			{
				ItemCalculationBase calculation = o as ItemCalculationBase;

				if (calculation != null)
					this.OnCalculationRemoved(calculation);
			}
		}

		#endregion //OnCalculationsRemoved
    
		#region OnCalculationsReset

		internal void OnCalculationsReset(object sender)
		{
			ICollection collection = sender as ICollection;

			Debug.Assert(collection != null, "Not a collection");

			if (collection == null)
				return;

			bool isListReferences = collection is ListCalculationCollection;

			List<ItemCalculationBase> oldCalcs = new List<ItemCalculationBase>();

			#region Get the existing calculations from the refs and initialize a List with them

			// get the existing references
			List<FormulaRefBase> existingRefs = this.GetExistingReferences(collection);

			foreach (FormulaRefBase refBase in existingRefs)
			{
				if (isListReferences)
				{
					ListCalculationReference listRef = refBase as ListCalculationReference;

					if (listRef != null)
					{
						oldCalcs.Add(listRef.Calculation);
						continue;
					}
				}
				else
				{
					ItemCalculationReference itemRef = refBase as ItemCalculationReference;

					if (itemRef != null)
					{
						oldCalcs.Add(itemRef.Calculation);
						continue;
					}
				}

				Debug.Assert(false, "Unknown reference type");
			}

			#endregion //Get the existing calculations from the refs and initialize a List with them	

			// Copy the new items into a temp list
			List<ItemCalculationBase> newCalcs = new List<ItemCalculationBase>(collection.Count);
			foreach(ItemCalculationBase calc in collection )
				newCalcs.Add(calc);

			// create a hash of the intersection of the old and new calculations
			HashSet<ItemCalculationBase> intersectHash = new HashSet<ItemCalculationBase>( oldCalcs );

			//intersect the old calculations with the new ones
			intersectHash.IntersectWith(newCalcs);

			// walk over the old items and remove any that are not still in the collection
			foreach (ItemCalculationBase oldCalc in oldCalcs)
			{
				if (!intersectHash.Contains(oldCalc))
					this.OnCalculationRemoved(oldCalc);
			}

			// walk over the new items and add any that weren't there before
			foreach (ItemCalculationBase newCalc in newCalcs)
			{
				if (!intersectHash.Contains(newCalc))
					this.OnCalculationAdded(newCalc);
			}
		}

		#endregion //OnCalculationsReset

		#region OnFormulaRegisteredOnReference

		internal abstract void OnFormulaRegisteredOnReference(FormulaRefBase reference);

		#endregion //OnFormulaRegisteredOnReference	
    
		#region ProcessRefListOnCalculatorChange

		internal void ProcessRefListOnCalculatorChange(XamCalculationManager mgr, string property, IDictionary references)
		{
			bool refreshValues = false;
			bool filterProperties = false;

			switch (property)
			{
				case "ValueConverter":
					refreshValues = true;
					break;

				case "PropertiesToInclude":
				case "PropertiesToExclude":
					filterProperties = true;
					break;

			}

			if (refreshValues || filterProperties)
			{
				// dirty all references 
				this.DirtyAllReferencesAsync();
			}
		}

		#endregion //ProcessRefListOnCalculatorChange

		#region ProcessRefListOnItemCalculationChange

		internal void ProcessRefListOnItemCalculationChange(XamCalculationManager mgr, ItemCalculation calculation, string property, IEnumerable references)
		{
			bool replaceReference = false;
			bool newFormula = false;
			switch (property)
			{
				case "ReferenceId":
				case "TargetProperty":
					replaceReference = true;
					break;

				case "Formula":
					newFormula = true;
					break;
			}

			if (replaceReference || newFormula)
			{
				foreach (ICalculationReference reference in references)
				{
					FormulaRefBase formulaRef = reference as FormulaRefBase;

					if (formulaRef == null)
						continue;

					// bypass references whose calculation doesn't match
					ItemCalculationReference calcRef = formulaRef as ItemCalculationReference;
					if (calcRef != null)
					{
						if (calcRef.Calculation != calculation)
							continue;
					}
					else
					{
						ListCalculatorColumnReference columnRef = formulaRef as ListCalculatorColumnReference;

						if (columnRef == null || columnRef.Calculation != calculation)
							continue;
					}

					if (newFormula)
					{
						formulaRef.EnsureFormulaRegistered(calculation.Formula);

						// let the derived class know that the formula has been registered
						this.OnFormulaRegisteredOnReference(formulaRef);
					}
					else
					{
						string oldRefId = formulaRef.ElementName;
						string newRefId = calculation.ReferenceIdResolved;
						// if the ref id changed then let the derived class know
						if (StringComparer.InvariantCultureIgnoreCase.Compare(newRefId, oldRefId) != 0)
							this.OnCalculationRefIdChanged(calculation, oldRefId, newRefId);
					}
				}
			}
		}

		#endregion //ProcessRefListOnItemCalculationChange

		#region RaiseTargetPropertyNotFoundError

		internal void RaiseTargetPropertyNotFoundError(string targetProperty, object item)
		{
			// JJD 10/27/11 - TFS92815
			// Special case known types (e.g. string, int, double, dateTime etc.). For these types
			// we expose a single pseudo read-only property named 'Value' which return the item itself
			//string errorMsg = string.Format(SRUtil.GetString("TargetPropertyNotFound"), targetProperty, item);
			string errorMsg;

			if (this.KnownType != null)
			{
				if (KnownTypeValueAccessor.IsValuePropertyName(targetProperty))
					errorMsg = string.Format(SRUtil.GetString("ValuePropertyIsReadOnly"), this.KnownType);
				else
					errorMsg = string.Format(SRUtil.GetString("TargetPropertyNotValue"), targetProperty, this.KnownType);
			}
			else
				errorMsg = string.Format(SRUtil.GetString("TargetPropertyNotFound"), targetProperty, item);

			DataAccessErrorEventArgs args = new DataAccessErrorEventArgs(item, targetProperty, errorMsg, false, null, null);

			// Raise the DataAccessError event on the calculator
			this.RaiseDataAccessErrorEvent(args);
		}

		#endregion //RaiseTargetPropertyNotFoundError	
    
		#region RegisterReferences

		internal void RegisterReferencesAsync()
		{
			if (_manager != null && _manager.IsInitialized && _asyncRegisterPending == false)
			{
				_asyncRegisterPending = true;

				_manager.Dispatcher.BeginInvoke(new Action(this.RegisterReferences));
			}
			else
			{
				if (_referencesDirty)
					this.UnregisterReferences();
			}
		}

		internal virtual void RegisterReferences()
		{
		    _asyncRegisterPending = false;

			if (_referencesDirty)
				this.UnregisterReferences();
		}

		#endregion //RegisterReferences

		#region SetResult

		internal abstract void SetResult(CalculationValue result, string name, FormulaRefBase reference);

		#endregion //SetResult

		#region UnregisterReferences

		internal virtual void UnregisterReferences()
		{
			_referencesDirty = false;
		}

		#endregion //UnregisterReferences

		#endregion //Internal Methods	

		#region Private Methods

		#region EnsureCalculatedHelper

		private void EnsureCalculatedHelper( XamCalculationManager calcManager, ICalculationReference reference )
		{
			if ( null != reference.Formula )
				calcManager.InternalEnsureCalculated( reference, true );

			var childRefs = reference.GetChildReferences( ChildReferenceType.ReferencesWithFormulas );
			if ( null != childRefs )
			{
				foreach ( var ii in childRefs )
					this.EnsureCalculatedHelper( calcManager, ii );
			}
		}

		#endregion // EnsureCalculatedHelper

		#region OnReferenceIdResolvedChanged

		private void OnReferenceIdResolvedChanged( )
		{
			this.UnregisterReferences( );

			if ( _manager != null )
				this.RegisterReferencesAsync( );

			this.RaisePropertyChangedEvent( "ReferenceId" );
		}

		#endregion //OnReferenceIdResolvedChanged	 

		#endregion // Private Methods
        
		#endregion //Methods

		#region ParticpantHelper internal nested class

		internal class ParticpantHelper : ICalculationParticipant
		{
			#region Private Members

			private XamCalculationManager _manager;
			private ItemCalculatorBase _calculator;
			private ItemCalculatorReferenceBase _rootReference;

			#endregion //Private Members	
    
			#region Constructor

			internal ParticpantHelper(XamCalculationManager manager, ItemCalculatorBase calculator, ItemCalculatorReferenceBase rootReference)
			{
				CoreUtilities.ValidateNotNull(manager);
				CoreUtilities.ValidateNotNull(calculator);
				CoreUtilities.ValidateNotNull(rootReference);

				_manager = manager;
				_calculator = calculator;
				_rootReference = rootReference;

			}

			#endregion //Constructor	
    
			#region Properties

			#region Manager

			internal XamCalculationManager Manager { get { return _manager; } }

			#endregion //Manager

			#endregion //Properties	
 
			#region Methods

			#region Dispose

			internal void Dispose()
			{
				_manager.InternalRemoveReference(_rootReference);
				((ICalculationManager)_manager).RemoveParticipant(this);
			}

			#endregion //Dispose

			#region Initialize

			internal void Initialize()
			{
				_manager.InternalAddParticipant(this);
				_manager.InternalAddReference(_rootReference);
			}

			#endregion //Initialize	
    
			#endregion //Methods	
        
			#region ICalculationParticipant Members

			public ICalculationReference RootReference
			{
				get { return _rootReference; }
			}

			public CalculationReferenceNode GetReferenceTree(object formulaTarget)
			{
				return _calculator.GetReferenceTree(formulaTarget);
			}

			#endregion
		}

		#endregion //ParticpantHelper internal nested class

		#region FormulaProviderResolver private nested class

		private class FormulaProviderResolver : IFormulaProviderResolver
		{
			#region IFormulaProviderResolver Members

			public bool SupportsTarget(object target)
			{
				return target is ItemCalculationBase;
			}

			public IFormulaProvider GetFormulaProvider(object target, out System.Exception error)
			{
				error = null;

				ItemCalculationBase itemCalc = target as ItemCalculationBase;

				if (itemCalc != null)
					return itemCalc;

				return null;
			}

			#endregion
		}

		#endregion //FormulaProviderResolver private nested class
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