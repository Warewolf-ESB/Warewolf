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
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Data;



using Infragistics.Windows.Licensing;
using Infragistics.Windows.Internal;


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;
using System.Reflection;

namespace Infragistics.Calculations
{

	/// <summary>
	/// An object that exposes both a collection of <see cref="ItemCalculation"/>s that will be used to calculate one or more values using properties exposed off of each item in the list 
	/// as well a collection of <see cref="ListCalculation"/>s that will be used to summarize values across all items in the list.
	/// </summary>
	/// <seealso cref="ItemCalculator"/>
	public sealed class ListCalculator : ItemCalculatorBase, ILogicalTreeNode
	{
		#region Private Members

		private ParticpantHelper _helper;
		private bool _propsAreDirty;
	
		private ObservableItemCollection _items;

		private ItemCalculationCollection _itemCalculations = new ItemCalculationCollection();
		private ListCalculationCollection _listCalculations = new ListCalculationCollection();
		private CalculationResultsDictionary _listResults;
		private Dictionary<string, object> _availableProperties = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
		private Dictionary<string, FormulaRefBase> _references = new Dictionary<string, FormulaRefBase>(StringComparer.InvariantCultureIgnoreCase);

		// JJD 10/27/11 - TFS92815
		// Special case known types (e.g. string, int, double, dateTime etc.). For these types
		// we expose a single pseudo read-only property named 'Value' which return the item itself
		private Type _knownType;

		#endregion //Private Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="ListCalculator"/>
		/// </summary>
		public ListCalculator()
		{
			_listResults = new CalculationResultsDictionary(this);

			_itemCalculations.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnCalculationsChanged);
			_listCalculations.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnCalculationsChanged);


			_items = new ObservableItemCollection(this, OnItemPropertyChanged, OnPropertyDescriptorChanged);




			_items.CollectionChanged += new NotifyCollectionChangedEventHandler(OnItemsCollectionChanged);
		}

		#endregion //Constructor	
    
		#region Base class overrides

		#region GetChildReferences

		internal override ICalculationReference[] GetChildReferences(ChildReferenceType referenceType)
		{
			Debug.Assert(referenceType == ChildReferenceType.ReferencesWithFormulas, "ListCalculator.GetChildReferences only supports ChildReferenceType.ReferencesWithFormulas");

			List<ICalculationReference> references = new List<ICalculationReference>();

			foreach (ItemCalculation ic in _itemCalculations)
			{
				if (!string.IsNullOrWhiteSpace(ic.Formula))
					references.Add(this.GetReference(ic.ReferenceIdResolved, false, true));
			}

			foreach (ListCalculation lc in _listCalculations)
			{
				if (!string.IsNullOrWhiteSpace(lc.Formula))
					references.Add(this.GetReference(lc.ReferenceIdResolved, false, true));
			}

			return references.ToArray();
		}

		#endregion //GetChildReferences	
    
		#region GetExistingReferences

		internal override List<FormulaRefBase> GetExistingReferences(ICollection collection)
		{
			List<FormulaRefBase> list = new List<FormulaRefBase>();

			bool isListCalculations = collection is ListCalculationCollection;

			foreach (FormulaRefBase reference in _references.Values)
			{
				if (isListCalculations)
				{
					if (reference is ListCalculationReference)
						list.Add(reference);
				}
				else
				{
					if (reference is ItemCalculationReference)
						list.Add(reference);
				}
			}

			return list;
		}

		#endregion //GetExistingReferences

		#region GetReference

		internal override ICalculationReference GetReference(string referenceId, bool includeItemProperties = true, bool createIfNotFound = true, bool includeSummaries = true, bool includeColumns = true)
		{
			XamCalculationManager mgr = this.CalculationManager;

			if (mgr == null || false == mgr.IsInitialized)
				return null;

			if (_helper == null)
			{
				this.RegisterReferences();

				Debug.Assert(_helper != null, "Root Ref registration failed");
				if (_helper == null)
					return null;
			}

			this.VerifyAvailableProperties();

			FormulaRefBase reference = null;

			// see if we already have the reference cached, if so return it
			if (_references != null && _references.TryGetValue(referenceId, out reference))
			{
				if (reference is ListCalculationReference )
				{
					 if (includeSummaries )
						return reference;
				}
				else
				{
					ListCalculatorColumnReference columnRef = reference as ListCalculatorColumnReference;

					if (columnRef != null)
					{
						if (columnRef.Calculation != null)
						{
							if (includeColumns)
								return columnRef;
						}
						else
						{
							if (includeItemProperties)
								return columnRef;
						}

						// JJD 10/27/11 - TFS92815
						// Special case known types (e.g. string, int, double, dateTime etc.). For these types
						// we expose a single pseudo read-only property named 'Value' which return the item itself
						// If this represents the Value type then return it always
						if (_knownType != null && KnownTypeValueAccessor.IsValuePropertyName(referenceId))
							return columnRef;
					}
				}
			}

			if (!createIfNotFound)
				return null;

			string formula = null;

			// next look thru the ListCalculations to see if we get a match
			if (includeSummaries && _listCalculations.Count > 0)
			{
				ListCalculation lc = null;

				foreach (ListCalculation listcalc in _listCalculations)
				{
					if (StringComparer.InvariantCultureIgnoreCase.Compare( listcalc.ReferenceIdResolved, referenceId) == 0)
					{
						// JJD 10/31/11 
						// Set the referenceId to the listcalc.ReferenceIdResolved so we register the reference with
						// the properly cased id 
						referenceId = listcalc.ReferenceIdResolved;

						lc = listcalc;
						break;
					}
				}

				if (lc != null && !string.IsNullOrWhiteSpace(lc.Formula))
				{
					reference = new ListCalculationReference(this.RootReference as ListCalculatorReference, lc);

					formula = lc.Formula;
				}

			}

			// next look thru the ItemCalculations to see if we get a match
			if (reference == null && includeColumns &&_itemCalculations.Count > 0)
			{
				ItemCalculation ic = null;

				foreach (ItemCalculation itemcalc in _itemCalculations)
				{
					if (StringComparer.InvariantCultureIgnoreCase.Compare(itemcalc.ReferenceIdResolved, referenceId) == 0)
					{
						// JJD 10/31/11 
						// Set the referenceId to the itemcalc.ReferenceIdResolved so we register the reference with
						// the properly cased id 
						referenceId = itemcalc.ReferenceIdResolved;

						ic = itemcalc;
						break;
					}
				}


				if (ic != null && !string.IsNullOrWhiteSpace(ic.Formula))
				{
					IItemPropertyValueAccessor valueAccessor = null;

					bool createRef = true;

					if (ic.HasTargetProperty)
					{
						// JJD 10/27/11 - TFS92815
						// Special case known types (e.g. string, int, double, dateTime etc.). For these types
						// we expose a single pseudo read-only property named 'Value' which return the item itself
						if (_knownType != null)
						{
							// Known types can not support target properties because their
							// single 'Value' property is read-only
							this.RaiseTargetPropertyNotFoundError(ic.TargetProperty, null);

							// Don't create the reference below if its reference ID would block the special 'Value' property
							if (KnownTypeValueAccessor.IsValuePropertyName(referenceId))
								createRef = false;

						}
						else
						{
							valueAccessor = GetAccessorForProp(ic.TargetProperty);
						}
					}

					if (createRef)
					{
						reference = new ListCalculatorColumnReference(this.RootReference as ListCalculatorReference, ic, referenceId, valueAccessor);

						formula = ic.Formula;
					}
				}
			}

			// Finally see if the item exposes a property of this name
			if (reference == null && includeItemProperties)
			{
				IItemPropertyValueAccessor valueAccessor = GetAccessorForProp(referenceId);

				if ( valueAccessor != null )
				{
					// JJD 10/31/11 
					// Set the refernceId to the valueAccessor.Name so we register the reference with
					// the properly cased name (i.e. it has the same casing as the Property name
					referenceId = valueAccessor.Name;

					reference = new ListCalculatorColumnReference(this.RootReference as ListCalculatorReference, null, referenceId, valueAccessor);
				}
			}

			// if we found a reference the cache it in the dictionary
			if (reference != null)
			{
				_references[referenceId] = reference;

				// register the formula if there is one
				if (!string.IsNullOrWhiteSpace(formula))
				{
					reference.EnsureFormulaRegistered(formula);

					// call OnFormulaRegisteredOnReference to update the results
					this.OnFormulaRegisteredOnReference(reference);
				}
			}

			return reference;
		}
		
		#endregion //GetReference
    
		#region GetReferenceTree

		internal override CalculationReferenceNode GetReferenceTree(object formulaTarget)
		{
			XamCalculationManager mgr = this.CalculationManager;

			if (mgr == null || false == mgr.IsInitialized)
				return new CalculationReferenceNode(this.ReferenceIdResolved, false, ReferenceNodeType.Control);
			
			this.VerifyAvailableProperties();

			CalculationReferenceNode rootNode = new CalculationReferenceNode(this.RootReference, this.ReferenceIdResolved, false, ReferenceNodeType.Calculator);

			ObservableCollection<CalculationReferenceNode> childNodes = new ObservableCollection<CalculationReferenceNode>();
			HashSet<string> refIdsAdded = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

			CalculationReferenceNode listCalcGroupNode = null;
			ObservableCollection<CalculationReferenceNode> listCalcNodes = null;


			// first get the summary, i.e. ListCalculations
			// and create a group node to contain them all
			foreach (ListCalculation lc in _listCalculations)
			{
				string refId = lc.ReferenceIdResolved;

				//bypass calculations that don't have a ref id
				if (string.IsNullOrWhiteSpace(refId))
					continue;

				ICalculationReference childRef = this.GetReference(refId, false, true, true, false);

				if (childRef == null)
					continue;

				if (listCalcGroupNode == null)
				{
					listCalcGroupNode = new CalculationReferenceNode(SRUtil.GetString("ListCalculationsTreeNode"), false, ReferenceNodeType.ListCalculationsGroup);
					listCalcNodes = new ObservableCollection<CalculationReferenceNode>();
					listCalcGroupNode.SortPriority = -1;
					listCalcGroupNode.IsExpanded = true;
				}

				// create and add the child node to the collection
				listCalcNodes.Add(new CalculationReferenceNode(childRef, refId, true, ReferenceNodeType.CalculatorCalculation));

				refIdsAdded.Add(refId);
			}

			// if we have a list calc group then add it now
			if (listCalcGroupNode != null)
			{
				listCalcGroupNode.ChildReferences = new ReadOnlyObservableCollection<CalculationReferenceNode>(listCalcNodes);
				childNodes.Add(listCalcGroupNode);
			}

			// them get the item calculations
			foreach (ItemCalculation ic in _itemCalculations)
			{
				string refId = ic.ReferenceIdResolved;

				//bypass calculations that don't have a ref id
				if (string.IsNullOrWhiteSpace(refId))
					continue;

				if (refIdsAdded.Contains(refId))
					continue;

				ICalculationReference childRef = this.GetReference(refId, false, true, false, true);

				if (childRef == null)
					continue;

				// create and add the child node to the collection
				childNodes.Add(new CalculationReferenceNode(childRef, refId, true, ReferenceNodeType.CalculatorCalculation));

				refIdsAdded.Add(refId);
			}

			// finally get any other item properties
			foreach (KeyValuePair<string, object> entry in _availableProperties)
			{
				if (refIdsAdded.Contains(entry.Key))
					continue;

				ICalculationReference childRef = this.GetReference(entry.Key, true, true, false, false);

				// if the ref's type is ItemCalculationReference then bypass it since it should already
				// be in the collection from the logic above
				if (childRef == null || childRef is ItemCalculationReference)
					continue;

				// create and add the child node to the collection
				childNodes.Add(new CalculationReferenceNode(childRef, entry.Key, true, ReferenceNodeType.CalculatorItemProperty));
			}

			rootNode.ChildReferences = new ReadOnlyObservableCollection<CalculationReferenceNode>(childNodes);

			return rootNode;
		}

		#endregion //GetReferenceTree

		#region OnCalculationAdded

		internal override void OnCalculationAdded(ItemCalculationBase calculation)
		{
			// must call the base to wire prop change notifications
			base.OnCalculationAdded(calculation);

			string refId = calculation.ReferenceIdResolved;

			// if an id is not specific then return
			if (string.IsNullOrWhiteSpace(refId))
				return;

			// add a Result for the new calculation if it is a ListCalculation
			if ( calculation is ListCalculation )
				_listResults.VerifyCalculationResultExists(refId);

			// if we haven't been initialized yet then we can bail
			if (_helper == null)
				return;

			XamCalculationManager mgr = this.CalculationManager;

			if (mgr == null || false == mgr.IsInitialized)
				return;

			ICalculationReference reference = this.GetReference(refId, false);

			// let the calc manager know that a reference has been added after 
			// the root reference was registered
			if (reference != null)
				mgr.InternalAddReference(reference);
		}

		#endregion //OnCalculationAdded	

		// JJD 10/27/11 - TFS92815
		#region IsKnownType

		// Special case known types (e.g. string, int, double, dateTime etc.). For these types
		// we expose a single pseudo read-only property named 'Value' which return the item itself
		internal override Type KnownType { get { return _knownType; } }

		#endregion //IsKnownType	
 
		#region OnCalculationRefIdChanged

		internal override void OnCalculationRefIdChanged(ItemCalculationBase calculation, string oldRefId, string newRefId)
		{
			// must call the base to maintain refId dictionary
			base.OnCalculationRefIdChanged(calculation, oldRefId, newRefId);

			ListCalculation lc = calculation as ListCalculation;

			// if this is a ListCalculation then remove the old result and add the new one
			if (lc != null)
			{
				_listResults.InternalRemove(oldRefId);
				_listResults.VerifyCalculationResultExists(newRefId);
			}

			FormulaRefBase oldRef;
			ListCalculatorColumnReference oldcalcRef = null;
			ListCalculatorColumnReference newcalcref = null;

			if (_references.TryGetValue(oldRefId, out oldRef))
			{
				// remove the old reference from our map
				_references.Remove(oldRefId);

				oldcalcRef = oldRef as ListCalculatorColumnReference;

				// call GetReference to cache the new reference
				newcalcref = this.GetReference(newRefId, false, true) as ListCalculatorColumnReference;
			}

			//dispose the old reference (note: this will notify the calcManager that the ref has been removed) 
			if ( oldcalcRef != null )
				oldcalcRef.Dispose(true);

			XamCalculationManager mgr = this.CalculationManager;

			if ( mgr == null || false == mgr.IsInitialized)
				return;

			//let the calc manager know about the new ref
			if ( newcalcref != null )
				mgr.InternalAddReference(newcalcref);

			// Raise the appropriate notifications on the calculation
			calculation.InvalidateReference();
		}

		#endregion //OnCalculationRefIdChanged	

		#region OnCalculationRemoved

		internal override void OnCalculationRemoved(ItemCalculationBase calculation)
		{
			// must call the base to unwire prop change notifications
			base.OnCalculationRemoved(calculation);

			string refId = calculation.ReferenceIdResolved;

			if (string.IsNullOrWhiteSpace(refId))
				return;

			bool isListReference = calculation is ListCalculation;

			string key = calculation.ReferenceIdResolved;

			FormulaRefBase reference;

			if (_references.TryGetValue(key, out reference))
			{
				if (isListReference)
				{
					ListCalculationReference listRef = reference as ListCalculationReference;

					Debug.Assert(listRef != null, "Wrong ref type in listcalc dictionary");
					Debug.Assert(listRef != null && listRef.Calculation == calculation, "Wrong ref instance in listcalc dictionary");

					if (listRef == null || listRef.Calculation != calculation)
						return;

					// remove the associated result from the results dictionary
					_listResults.InternalRemove(key);
					
					//dispose the old reference (note: this will notify the calcManager that the ref has been removed) 
					listRef.Dispose(true);
				}
				else
				{
					ListCalculatorColumnReference columnRef = reference as ListCalculatorColumnReference;

					Debug.Assert(columnRef != null, "Wrong ref type in dictionary");
					Debug.Assert(columnRef != null && columnRef.Calculation == calculation, "Wrong ref instance in dictionary");

					if (columnRef == null || columnRef.Calculation != calculation)
						return;

					//dispose the old reference (note: this will notify the calcManager that the ref has been removed) 
					columnRef.Dispose(true);
				}

				_references.Remove(key);
			}
		}

		#endregion //OnCalculationRemoved	

		#region OnFormulaRegisteredOnReference

		internal override void OnFormulaRegisteredOnReference(FormulaRefBase reference)
		{
			ListCalculationReference lcr = reference as ListCalculationReference;

			// if there is a syntax error then we should update the results
			if (lcr != null)
			{
				if (lcr.FormulaSyntaxErrorValue != null)
					_listResults.InternalSet(lcr.Calculation.ReferenceIdResolved, lcr.FormulaSyntaxErrorValue);
			}
		}

		#endregion //OnFormulaRegisteredOnReference	
		
		#region OnSubObjectPropertyChanged

		internal override void OnSubObjectPropertyChanged(object sender, string property, object extraInfo)
		{
			base.OnSubObjectPropertyChanged(sender, property, extraInfo);

			XamCalculationManager mgr = this.CalculationManager;

			if (mgr == null || false == mgr.IsInitialized)
				return;

			if (sender == this)
			{
				this.ProcessRefListOnCalculatorChange(mgr, property, _references);
				return;
			}

			ItemCalculation calculation = sender as ItemCalculation;

			if (calculation != null)
				ProcessRefListOnItemCalculationChange(mgr, calculation, property, _references.Values);

			ListCalculation listCalculation = sender as ListCalculation;

			if (listCalculation != null)
				ProcessRefListOnListCalculationChange(mgr, listCalculation, property);
		}

		#endregion //OnSubObjectPropertyChanged	

		#region ProcessRefListOnItemCalculationChange

		internal void ProcessRefListOnListCalculationChange(XamCalculationManager mgr, ListCalculation calculation, string property)
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
				foreach (ICalculationReference reference in _references.Values)
				{
					ListCalculationReference listCalcRef = reference as ListCalculationReference;

					if (listCalcRef == null || listCalcRef.Calculation != calculation)
						continue;

					if (newFormula)
					{
						listCalcRef.EnsureFormulaRegistered(calculation.Formula);

						// call OnFormulaRegisteredOnReference to update the results
						this.OnFormulaRegisteredOnReference(listCalcRef);
					}
					else
					{
						string oldRefId = listCalcRef.ElementName;
						string newRefId = calculation.ReferenceIdResolved;
						// if the ref id changed then let the derived class know
						if (StringComparer.InvariantCultureIgnoreCase.Compare(newRefId, oldRefId) != 0)
							this.OnCalculationRefIdChanged(calculation, oldRefId, newRefId);
					}
				}
			}
		}

		#endregion //ProcessRefListOnItemCalculationChange

		#region Participant

		internal override ICalculationParticipant Participant { get { return _helper; } }

		#endregion //Participant	
    
		#region RegisterReferences

		internal override void RegisterReferences()
		{
			// call the base so it can reset its pending flag
			base.RegisterReferences();

			this.VerifyAvailableProperties();

			XamCalculationManager mgr = this.CalculationManager;

			if (mgr == null || false == mgr.IsInitialized)
				return;

			if (_helper != null)
				return;

			_helper = new ParticpantHelper(mgr, this, new ListCalculatorReference(this));
			_helper.Initialize();

			// get the child refs to force their creation
			ICalculationReference[] chidRefs = this.GetChildReferences(ChildReferenceType.ReferencesWithFormulas);
			
			// raise the appropriate notifications on the calculations
			_itemCalculations.InvalidateReferences();
			_listCalculations.InvalidateReferences();

		}

		#endregion //RegisterReferences

		#region RootReference

		internal override ItemCalculatorReferenceBase RootReference { get { return _helper != null ? _helper.RootReference as ItemCalculatorReferenceBase : null; } }

		#endregion //RootReference	

		#region SetResult

		internal override void SetResult(CalculationValue result, string name, FormulaRefBase reference)
		{
			if (reference is ListCalculationReference)
			{
				_listResults.InternalSet(name, result);
				return;
			}
		}

		#endregion //SetResult	

		#region ToString

		/// <summary>
		/// Returns a string that represents this object;
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(SRUtil.GetString("ListCalculator_Desc"), this.ReferenceIdResolved);
		}

		#endregion //ToString	
 
		#region UnregisterReferences

		internal override void UnregisterReferences()
		{
			// call the base to clear any state flags
			base.UnregisterReferences();

			if (_helper != null)
			{
				_helper.Dispose();
				_helper = null;
			}

			int count = _references.Count;

			if (count > 0)
			{
				FormulaRefBase[] oldRefs = new FormulaRefBase[count];

				_references.Values.CopyTo(oldRefs, 0);
				_references.Clear();

				for (int i = 0; i < count; i++)
				{
					DisposeReference(oldRefs[i]);
				}
			}

			// raise the appropriate notifications on the calculations
			_itemCalculations.InvalidateReferences();
			_listCalculations.InvalidateReferences();
		}

		#endregion //UnregisterReferences

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region Items

		/// <summary>
		/// The collection of items on which calculations will be performed.
		/// </summary>
		public IList Items
		{
			get { return _items; }
		}

		#endregion //Items

		#region ItemsSource

		/// <summary>
		/// Returns or sets an enumerable used to populate the <see cref="Items"/> collection
		/// </summary>
		// /// <seealso cref="ItemsSourceProperty"/>
		public IEnumerable ItemsSource
		{
			get
			{
				return _items.ItemsSource;
			}
			set
			{
				_items.ItemsSource = value;
			}
		}

		#endregion //ItemsSource

		#region ItemCalculations

		/// <summary>
		/// Item calculations. These calculations are done across a single item, like "[A] + [B]" where A and B are properties
		/// of the data items in the <see cref="Items"/> collection. One can also refer to other item or list calculations using ReferenceIds.
		/// For example, if there's a calculation with 'Total' ReferenceId, one can have a formula like "[A] / [Total]".
		/// </summary>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

		public ItemCalculationCollection ItemCalculations
		{
			get { return _itemCalculations; }
		}

		#endregion //ItemCalculations

		#region ListCalculations

		/// <summary>
		/// List calculations. These calculations are done across all the items of the list, like "sum([A])" where A is a property
		/// of the data items in the <see cref="Items"/> collection. One can also refer to other item or list calculations using ReferenceIds.
		/// </summary>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

		public ListCalculationCollection ListCalculations
		{
			get { return _listCalculations; }
		}

		#endregion //ListCalculations

		#region ListResults

		/// <summary>
		/// Returns a dictionary containing the results of the list calculations (read-only).
		/// </summary>
		/// <seealso cref="ListCalculations"/>
		public CalculationResultsDictionary ListResults
		{
			get
			{
				return _listResults;
			}
		}

		#endregion //ListResults

		#endregion //Public Properties	
    
		#endregion //Properties	
        
		#region Methods

		#region Public Methods

		#region GetItemCalculationResult

		/// <summary>
		/// Gets the result of a item calculation in the <see cref="ItemCalculations"/> using 
		/// the <see cref="ItemCalculationBase.ReferenceId"/> identifier.
		/// </summary>
		/// <param name="item">Identifies the item in the <see cref="Items"/> collection.</param>
		/// <param name="referenceId">Identifies the calculation using the <see cref="ItemCalculationBase.ReferenceId"/> property.</param>
		public CalculationResult GetItemCalculationResult(object item, string referenceId)
		{
			CoreUtilities.ValidateNotNull(item, "item");
			CoreUtilities.ValidateNotEmpty(referenceId, "referenceId");

			// get the column ref
			ICalculationReference reference = this.GetReference(referenceId, false, true, false, true);

			CalculationValue calcValue = null;
			if ( reference != null )
			{
				ListCalculatorColumnReference columnRef = reference as ListCalculatorColumnReference;

				Debug.Assert(columnRef != null, "This reference should be a ListCalculatorColumnReference");

				if (columnRef != null)
				{
					// get the cell ref for this item from the column ref
					ListCalculatorCellReference cellref = columnRef.GetCellRef(item);

					if ( cellref != null )
						calcValue = cellref.Value;
				}
			}
			
			// if we don't have a value at this point then set the value to an exception
			if ( calcValue == null )
				calcValue = new CalculationValue(  new InvalidOperationException(SRUtil.GetString("FindReferenceError")));

			CalculationResult result = new CalculationResult();

			result.CalculationValue = calcValue;

			return result;
		}

		#endregion //GetItemCalculationResult

		#endregion //Public Methods	

		#region Internal Methods

		#region GetAccessorFroProp

		internal IItemPropertyValueAccessor GetAccessorForProp(string propName)
		{
			object prop;
			if (_availableProperties.TryGetValue(propName, out prop))
			{
				PropertyInfo pi = prop as PropertyInfo;

				if (pi != null)
					return new PropertyInfoValueAccessor(pi);

				PropertyDescriptor pd = prop as PropertyDescriptor;

				if (pd != null)
					return new PropertyDescriptorValueAccessor(pd);


				// JJD 10/27/11 - TFS92815
				// For known types (string, double, int, DateTime etc.) we need to  
				// return a KnownTypeValueAccessor (that is essentially read-only)
				Type type = prop as Type;

				if (type != null)
				{
					Debug.Assert(type == _knownType, "The only time a type should be in the available props hash set is if this is a known type");
					return new KnownTypeValueAccessor(type);;
				}
				
				Debug.Assert(false, "Unknown prop type in _availableProperties");
			}

			return null;
		}

		#endregion //GetAccessorFroProp	

		#region GetAllCellsInColumnReference

		internal ICalculationReference GetAllCellsInColumnReference(string name)
		{
			ListCalculatorColumnReference columnRef = this.GetReference(name, true, true, false, true) as ListCalculatorColumnReference;

			return columnRef != null ? columnRef.GetAllCellsReference() : null;
		}

		#endregion //GetAllCellsInColumnReference	

		#region GetCellAtIndexReference

		internal ICalculationReference GetCellAtIndexReference(string name, int index, bool isRelative)
		{
			ListCalculatorColumnReference columnRef = this.GetReference(name, true, true, false, true) as ListCalculatorColumnReference;

			return columnRef != null ? columnRef.GetCellAtRelativeOrAbsoluteIndex(index, isRelative) : null;
		}

		#endregion //GetCellAtIndexReference

		#endregion //Internal Methods

		#region Private Methods

		#region InvalidateAvailableProperties

		private void InvalidateAvailableProperties()
		{
			_propsAreDirty = true;
		}

		#endregion //InvalidateAvailableProperties	
		
		#region InvalidateListCalcValues

		internal void InvalidateListCalcValues()
		{
			XamCalculationManager mgr = this.CalculationManager;

			if (mgr != null && mgr.IsInitialized)
			{
				foreach (ListCalculation lc in this._listCalculations)
				{
					ICalculationReference reference = this.GetReference(lc.ReferenceIdResolved, false, false, true, false);

					if (reference != null)
					{
						mgr.InternalNotifyValueChanged(reference);

						Debug.Assert(reference is ListCalculationReference, "Unexpected ref type");
					}
				}
			}
		}

		#endregion //InvalidateListCalcValues	
 
		#region OnItemsAdded

		private void OnItemsAdded(IList items)
		{
			foreach (FormulaRefBase refernece in _references.Values)
			{
				ListCalculatorColumnReference columnRef = refernece as ListCalculatorColumnReference;

				if (columnRef != null)
					columnRef.OnItemsAdded(items);
			}
		}

		#endregion //OnItemsAdded

		#region OnItemsRemoved

		private void OnItemsRemoved(IList items)
		{
			foreach (FormulaRefBase refernece in _references.Values)
			{
				ListCalculatorColumnReference columnRef = refernece as ListCalculatorColumnReference;

				if (columnRef != null)
					columnRef.OnItemsRemoved(items);
			}
		}

		#endregion //OnItemsRemoved
           
		#region OnItemsCollectionChanged

		private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
					
				case NotifyCollectionChangedAction.Add:
					bool wascollectionEmpty = this.Items.Count <= e.NewItems.Count;
					// if the collection was empty before then treat the notification as a reset
					if (wascollectionEmpty)
					{
						this.OnItemsCollectionReset();
						return;
					}
					this.OnItemsAdded(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Remove:
					if ( null == e.OldItems || 0 == e.OldItems.Count || null == e.OldItems[0] )
						this.OnItemsCollectionReset( );
					else
						this.OnItemsRemoved(e.OldItems);
					break;
				case NotifyCollectionChangedAction.Replace:
					this.OnItemsRemoved(e.OldItems);
					this.OnItemsAdded(e.NewItems);
					break;
				case NotifyCollectionChangedAction.Reset:

					this.OnItemsCollectionReset();
					break;

				case NotifyCollectionChangedAction.Move:
					{
						if (_helper != null)
						{
							XamCalculationManager mgr = this.CalculationManager;

							if (mgr != null && mgr.IsInitialized)
								((ICalculationManager)mgr).RowsCollectionReferenceSorted(this.RootReference);
						}
						break;
					}

			}

			this.InvalidateListCalcValues();
		}

		private void OnItemsCollectionReset()
		{
			this.InvalidateAvailableProperties();

			if (_helper != null)
			{
				XamCalculationManager mgr = this.CalculationManager;

				if (mgr != null && mgr.IsInitialized)
				{
					((ICalculationManager)mgr).RowsCollectionReferenceResynched(this.RootReference);
				}
			}
		}

		#endregion //OnItemsCollectionChanged	
  
		#region OnItemPropertyChanged

		private void OnItemPropertyChanged(object owner, object item, string propertyName)
		{
			FormulaRefBase reference;

			if (_references.TryGetValue(propertyName, out reference))
			{
				ListCalculatorColumnReference columnRef = reference as ListCalculatorColumnReference;

				if (columnRef != null)
				{
					ListCalculatorCellReference cellRef = columnRef.GetCellRef(item, false);

					if (cellRef != null && cellRef.Index >= 0)
					{
						cellRef.OnPropertyChanged();
					}
				}
			}
		}

		#endregion //OnItemPropertyChanged	

		#region OnPropertyAdded

		private bool OnPropertyAdded(string name)
		{
			// if the new property is filtered out then return
			if (!this.IsPropertyIncluded(name))
				return false;

			// if we haven't been initialized yet then we can bail
			if (_helper == null)
				return true;

			XamCalculationManager mgr = this.CalculationManager;

			if (mgr == null || false == mgr.IsInitialized)
				return true;

			ICalculationReference reference = this.GetReference(name);

			// let the calc manager know that a reference has been added after 
			// the root reference was registered
			if (reference != null)
				mgr.InternalAddReference(reference);

			return true;
		}

		#endregion //OnPropertyAdded	
    
		#region OnPropertyDescriptorAdded


		private void OnPropertyDescriptorAdded(PropertyDescriptor pd)
		{
			Debug.Assert(pd != null, "Null PropertyDescriptor");
			if (pd == null)
				return;

			string name = pd.Name;

			if (OnPropertyAdded(name))
			{
				_availableProperties[name] = pd;
			}

		}

		#endregion //OnPropertyDescriptorAdded	

		#region OnPropertyDescriptorChanged


		private void OnPropertyDescriptorChanged(object owner, DataListEventListener listener, DataListChangeInfo changeInfo)
		{

			switch (changeInfo._changeType)
			{

				case DataListChangeInfo.ChangeType.PropertyDescriptorAdded:
					{
						this.OnPropertyDescriptorAdded( changeInfo._propertyDescriptor );
					}
					break;
				case DataListChangeInfo.ChangeType.PropertyDescriptorChanged:
					{
						// remove and re-add the property
						this.OnPropertyDescriptorRemoved( changeInfo._propertyDescriptor );
						this.OnPropertyDescriptorAdded( changeInfo._propertyDescriptor );
					}
					break;
				case DataListChangeInfo.ChangeType.PropertyDescriptorRemoved:
					{
						this.OnPropertyDescriptorRemoved( changeInfo._propertyDescriptor );
					}
					break;
			}
		}

		#endregion // OnPropertyDescriptorChanged
    		
		#region OnPropertyDescriptorRemoved


		private void OnPropertyDescriptorRemoved(PropertyDescriptor pd)
		{
			Debug.Assert(pd != null, "Null PropertyDescriptor");
			if (pd == null)
				return;

			string name = pd.Name;

			this.OnPropertyRemoved(name);
		}


		#endregion //OnPropertyDescriptorRemoved	
 
		#region OnPropertyRemoved

		private void OnPropertyRemoved(string name)
		{
			if (_availableProperties.ContainsKey(name))
				_availableProperties.Remove(name);

			FormulaRefBase reference;

			if (_references.TryGetValue(name, out reference))
			{
				_references.Remove(name);

				// dispose the reference (note: this will notify the calc manager that it has been removed
				DisposeReference(reference);
			}
		}

		#endregion //OnPropertyRemoved	
       
		#region VerifyAvailableProperties

		private void VerifyAvailableProperties()
		{
			if ( !_propsAreDirty)
				return;

			_propsAreDirty = false;

			// cache the old keys
			HashSet<string> oldKeys = new HashSet<string>(_availableProperties.Keys);

			// clear the map of available properties
			_availableProperties.Clear();

			// JJD 10/27/11 - TFS92815
			// Clear the cached known type
			_knownType = null;

			try
			{

				IEnumerable underlyingList = _items.ItemsSourceUnderlying;

				if (underlyingList == null)
					return;

				object item = null;

				if (_items.Count > 0)
					item = _items[0];

				if (underlyingList == null && item == null)
					return;

				PropertyDescriptorProvider provider = PropertyDescriptorProvider.CreateProvider(item, underlyingList);

				if (provider == null)
					return;

				#region Check for known types

				// JJD 10/27/11 - TFS92815
				// For known types (string, double, int, DateTime etc.) we need to set the
				// _isKnownType flag so we can prevent a formula from targeting the type
				TypePropertyDescriptorProvider typeProvider = provider as TypePropertyDescriptorProvider;

				if (typeProvider != null)
				{
					Type type = typeProvider.Type;

					if (typeof(object) == type || CoreUtilities.IsKnownType(type))
					{
						// JJD 10/27/11 - TFS92815
						// For known types (string, double, int, DateTime etc.) we access the item thru an 
						// arbitrary read-only 'Value" property
						_knownType = type;
						_availableProperties.Add(KnownTypeValueAccessor.PropertyName, type);
						return;
					}
				}

				#endregion //Check for known types

				#region Process properties from ITypedList

				PropertyDescriptorCollection pds = provider.GetProperties();

				if (pds != null)
				{
					int count = pds.Count;

					for (int i = 0; i < count; i++)
					{
						PropertyDescriptor pd = pds[i];

						string name = pd.Name;

						if (!IsPropertyIncluded(name))
							continue;

						_availableProperties[name] = pd;
					}
				}

				#endregion //Process properties from provider


#region Infragistics Source Cleanup (Region)



































































































#endregion // Infragistics Source Cleanup (Region)

			}
			finally
			{

				// create a hash of the intersection of the old and new keys
				HashSet<string> intersectHash = new HashSet<string>(oldKeys);
				intersectHash.IntersectWith(_availableProperties.Keys);

				// walk over the old keys and remove any that are not still there
				foreach (string key in oldKeys)
				{
					if (!intersectHash.Contains(key))
						this.OnPropertyRemoved(key);
				}

				// walk over the new keys and add any that weren't there before
				foreach (string key in _availableProperties.Keys)
				{
					if (!intersectHash.Contains(key))
						this.OnPropertyAdded(key);
				}
			}

		}

		#endregion //VerifyAvailableProperties	
    
		#endregion //Private Methods	
    
		#endregion //Methods	
    	
		#region ILogicalTreeNode Members


		void  ILogicalTreeNode.AddLogicalChild(object child)
		{
			// Do nothing
 		}

		void  ILogicalTreeNode.RemoveLogicalChild(object child)
		{
			// Do nothing
		}


		#endregion

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