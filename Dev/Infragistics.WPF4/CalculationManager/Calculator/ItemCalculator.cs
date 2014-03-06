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


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;
using System.Reflection;

namespace Infragistics.Calculations
{

	/// <summary>
	/// An object that exposes a collection of <see cref="ItemCalculation"/>s that will be used to calculate one or more values using properties exposed off of a single item as the source of the calculations.
	/// </summary>
	/// <seealso cref="ListCalculator"/>
	public sealed class ItemCalculator : ItemCalculatorBase
	{
		#region Private Members

		private ParticpantHelper _helper;
		private object _item;
		private ItemCalculationCollection _calculations = new ItemCalculationCollection();
		private CalculationResultsDictionary _results;
		private Dictionary<string, ItemPropertyReference> _itemReferences = new Dictionary<string,ItemPropertyReference>(StringComparer.InvariantCultureIgnoreCase);

		// JJD 10/27/11 - TFS92815
		// Special case known types (e.g. string, int, double, dateTime etc.). For these types
		// we expose a single pseudo read-only property named 'Value' which return the item itself
		private Type _knownType;

		#endregion //Private Members	
 
		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="ItemCalculator"/>
		/// </summary>
		public ItemCalculator()
		{
			_results = new CalculationResultsDictionary(this);
			_calculations.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnCalculationsChanged);
		}

		#endregion //Constructor	
    
		#region Base class overrides

		#region GetChildReferences

		internal override ICalculationReference[] GetChildReferences(ChildReferenceType referenceType)
		{
			Debug.Assert(referenceType == ChildReferenceType.ReferencesWithFormulas, "ItemCalculator.GetChildReferences only supports ChildReferenceType.ReferencesWithFormulas");

			List<ICalculationReference> references = new List<ICalculationReference>();

			foreach (ItemCalculation ic in _calculations)
			{
				if ( !string.IsNullOrWhiteSpace( ic.Formula ))
					references.Add(this.GetReference(ic.ReferenceIdResolved, false, true));
			}

			return references.ToArray();
		}

		#endregion //GetChildReferences	
    
		#region GetExistingReferences

		internal override List<FormulaRefBase> GetExistingReferences(ICollection collection)
		{
			List<FormulaRefBase> list = new List<FormulaRefBase>();

			foreach(FormulaRefBase reference in _itemReferences.Values)
			{
				if (reference is ItemCalculationReference)
					list.Add(reference);
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

			if (_item == null)
				return null;

			if (_helper == null)
			{
				this.RegisterReferences();

				Debug.Assert(_helper != null, "Root Ref registration failed");
				if (_helper == null)
					return null;
			}

			ItemPropertyReference itemRef = null;

			// see if we already have the reference cached, if so return it
			if (_itemReferences.TryGetValue(referenceId, out itemRef))
			{
				// JJD 10/27/11 - TFS92815
				// Special case known types (e.g. string, int, double, dateTime etc.). For these types
				// we expose a single pseudo read-only property named 'Value' which return the item itself
				//if (includeItemProperties ||
				//    itemRef is ItemCalculationReference)
				if (includeItemProperties ||
					(itemRef is ItemCalculationReference) ||
					(_knownType != null && KnownTypeValueAccessor.IsValuePropertyName(referenceId)))
					return itemRef;
			}

			if (!createIfNotFound)
				return null;

			// next look thru the ItemCalculations to see if we get a match
			if (_calculations.Count > 0)
			{
				ItemCalculation ic = null;

				foreach (ItemCalculation itemcalc in _calculations)
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

				if (ic != null)
				{
					object item = null;

					// JJD 10/27/11 - TFS92815
					// create a stack variable to hold the value accessor 
					IItemPropertyValueAccessor valueAccessor = null;
					bool createRef = true;

					if (_item != null && ic.HasTargetProperty)
					{
						// JJD 10/27/11 - TFS92815
						// Special case known types, e.g. string, int, double, dateTime etc.
						// but only for the special property value of 'Value'
						if (_knownType != null)
						{
							// Known types can not support target properties because their
							// single 'Value' property is read-only
							this.RaiseTargetPropertyNotFoundError(ic.TargetProperty, _item);

							// Don't create the reference below if its reference ID would block the special 'Value' property
							if ( KnownTypeValueAccessor.IsValuePropertyName(referenceId))
								createRef = false;
						}
						else
						{
							PropertyInfo propInfo = _item.GetType().GetProperty(ic.TargetProperty, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
					

							if (propInfo != null)
							{
								item = _item;

								valueAccessor = new PropertyInfoValueAccessor(propInfo);
							}
							else
								this.RaiseTargetPropertyNotFoundError(ic.TargetProperty, _item);
						}
					}

					// JJD 10/27/11 - TFS92815
					// Use the value accessor created above but only create the ref if the flag is true
					//itemRef = new ItemCalculationReference( this.RootReference as ItemCalculatorReference, ic, referenceId, item, propInfo != null ? new PropertyInfoValueAccessor( propInfo ) : null, true);
					if ( createRef )
						itemRef = new ItemCalculationReference( this.RootReference as ItemCalculatorReference, ic, referenceId, item, valueAccessor, true);
				}
			}

			// Finally see if the item exposes a property of this name
			// JJD 10/27/11 - TFS92815
			// Check for IsPropertyIncluded since we shouldn't bother doing anything if it isn't
			//if (itemRef == null && _item != null && includeItemProperties)
			if (itemRef == null && _item != null && includeItemProperties && IsPropertyIncluded(referenceId))
			{
				// JJD 10/27/11 - TFS92815
				// create a stack variable to hold the accessor instead of the propinfo
				//PropertyInfo propInfo = null;
				IItemPropertyValueAccessor valueAccessor = null;

				// JJD 10/27/11 - TFS92815
				// Special case known types, e.g. string, int, double, dateTime etc.
				// but only for the special property value of 'Value'
				if (_knownType != null)
				{
					if (KnownTypeValueAccessor.IsValuePropertyName(referenceId))
						valueAccessor = new KnownTypeValueAccessor(_knownType);
				}
				else
				{
					PropertyInfo propInfo = _item.GetType().GetProperty(referenceId, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);

					if (propInfo != null)
						valueAccessor = new PropertyInfoValueAccessor(propInfo);
				}

				// JJD 10/27/11 - TFS92815
				// Moved check for IsPropertyIncluded above
				//if (propInfo != null && !IsPropertyIncluded(referenceId))
					//propInfo = null;

				// JJD 10/27/11 - TFS92815
				// Use the value accessor created above
				//if (propInfo != null)
					//itemRef = new ItemPropertyReference( this.RootReference as ItemCalculatorReference, referenceId, _item, propInfo != null ? new PropertyInfoValueAccessor( propInfo ) : null, true);
				if (valueAccessor != null)
				{
					// JJD 10/31/11 
					// Set the refernceId to the valueAccessor.Name so we register the reference with
					// the properly cased name (i.e. it has the same casing as the Property name
					referenceId = valueAccessor.Name;

					itemRef = new ItemPropertyReference(this.RootReference as ItemCalculatorReference, referenceId, _item, valueAccessor, true);
				}
			}

			// if we found a reference the cache it in the dictionary
			if (itemRef != null)
			{
				_itemReferences.Add(referenceId, itemRef);

				// get the element name to ensure that it is cached
				string elementNam = itemRef.ElementName;

				ItemCalculationReference icr = itemRef as ItemCalculationReference;

				if (icr != null && !string.IsNullOrWhiteSpace(icr.Calculation.Formula))
				{
					icr.EnsureFormulaRegistered(icr.Calculation.Formula);

					// call OnFormulaRegisteredOnReference to update the results
					this.OnFormulaRegisteredOnReference(icr);
				}
			}

			return itemRef;
		}

		#endregion //GetReference
    
		#region GetReferenceTree

		internal override CalculationReferenceNode GetReferenceTree(object formulaTarget)
		{
			XamCalculationManager mgr = this.CalculationManager;

			if ( mgr == null || false == mgr.IsInitialized )
				return new CalculationReferenceNode(this.ReferenceIdResolved, false, ReferenceNodeType.Control);

			CalculationReferenceNode rootNode = new CalculationReferenceNode(this.RootReference, this.ReferenceIdResolved, false, ReferenceNodeType.Calculator);
			
			ObservableCollection<CalculationReferenceNode> childNodes = new ObservableCollection<CalculationReferenceNode>();

			foreach (ItemCalculation ic in _calculations)
			{
				string refId = ic.ReferenceIdResolved;

				//bypass calculations that don't have a ref id
				if (string.IsNullOrWhiteSpace(refId))
					continue;
				
				ICalculationReference childRef = this.GetReference(refId, false, true);

				if (childRef == null )
					continue;
				
				// create and add the child node to the collection
				childNodes.Add(new CalculationReferenceNode(childRef, refId, true, ReferenceNodeType.CalculatorCalculation));
			}

			if (_item != null)
			{
				PropertyInfo[] propInfos = _item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

				foreach (PropertyInfo pinfo in propInfos)
				{
					ICalculationReference childRef = this.GetReference(pinfo.Name, true, true);

					// if the ref's type is ItemCalculationReference then bypass it since it should already
					// be in the collection from the logic above
					if (childRef == null || childRef is ItemCalculationReference )
						continue;

					// create and add the child node to the collection
					childNodes.Add(new CalculationReferenceNode(childRef, pinfo.Name, true, ReferenceNodeType.CalculatorItemProperty));
				}
			}

			rootNode.ChildReferences = new ReadOnlyObservableCollection<CalculationReferenceNode>(childNodes);

			return rootNode;
		}

		#endregion //GetReferenceTree	

		// JJD 10/27/11 - TFS92815
		#region IsKnownType

		// Special case known types (e.g. string, int, double, dateTime etc.). For these types
		// we expose a single pseudo read-only property named 'Value' which return the item itself
		internal override Type KnownType { get { return _knownType; } }

		#endregion //IsKnownType	
 
		#region OnCalculationAdded

		internal override void OnCalculationAdded(ItemCalculationBase calculation)
		{
			// must call the base to wire prop change notifications
			base.OnCalculationAdded(calculation);

			string refId = calculation.ReferenceIdResolved;

			if (string.IsNullOrWhiteSpace(refId))
				return;

			_results.VerifyCalculationResultExists(refId);

		}

		#endregion //OnCalculationAdded	

		#region OnCalculationRefIdChanged

		internal override void OnCalculationRefIdChanged(ItemCalculationBase calculation, string oldRefId, string newRefId)
		{
			// must call the base to maintain refId dictionary
			base.OnCalculationRefIdChanged(calculation, oldRefId, newRefId);

			_results.InternalRemove(oldRefId);
			_results.VerifyCalculationResultExists(newRefId);

			ItemPropertyReference oldRef;

			if (_itemReferences.TryGetValue(oldRefId, out oldRef))
			{
				_itemReferences.Remove(oldRefId);
				ItemCalculationReference oldcalcRef = oldRef as ItemCalculationReference;
				ItemCalculationReference newcalcref = this.GetReference(newRefId, false, true) as ItemCalculationReference;
			}
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

			string key = calculation.ReferenceIdResolved;

			ItemPropertyReference reference;

			if (_itemReferences.TryGetValue(key, out reference))
			{
				ItemCalculationReference calcRef = reference as ItemCalculationReference;

				Debug.Assert(calcRef != null, "Wrong ref type in dictionary");
				Debug.Assert(calcRef != null && calcRef.Calculation == calculation, "Wrong ref instance in dictionary");

				if (calcRef != null && calcRef.Calculation == calculation)
				{
					_itemReferences.Remove(key);
				}
			}

			// remove the associated result from the results dictionary
			_results.InternalRemove(key);
		}

		#endregion //OnCalculationRemoved	

		#region OnFormulaRegisteredOnReference

		internal override void OnFormulaRegisteredOnReference(FormulaRefBase reference)
		{
			ItemCalculationReference icr = reference as ItemCalculationReference;

			// if there is a syntax error then we should update the results
			if (icr != null && icr.FormulaSyntaxErrorValue != null)
				_results.InternalSet(icr.Calculation.ReferenceIdResolved, icr.FormulaSyntaxErrorValue);
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
				this.ProcessRefListOnCalculatorChange(mgr, property, _itemReferences);
				return;
			}

			ItemCalculation calculation = sender as ItemCalculation;

			if (calculation != null)
				ProcessRefListOnItemCalculationChange(mgr, calculation, property, _itemReferences.Values);
		}

		#endregion //OnSubObjectPropertyChanged	

		#region Participant

		internal override ICalculationParticipant Participant { get { return _helper; } }

		#endregion //Participant	
    
		#region RegisterReferences

		internal override void RegisterReferences()
		{
			// call the base so it can reset its pending flag
			base.RegisterReferences();

			if (_item == null)
				return;

			XamCalculationManager mgr = this.CalculationManager;

			if (mgr == null || false == mgr.IsInitialized)
				return;

			//Debug.Assert(_helper == null, "RegisterReferences called redundantly");

			if (_helper != null)
				return;

			// update the results to not filter out errors
			_results.OnIsItemSetChanged(true);

			_helper = new ParticpantHelper(mgr, this, new ItemCalculatorReference(this));
			_helper.Initialize();

			// get the child refs to force their creation
			ICalculationReference[] chidRefs = this.GetChildReferences(ChildReferenceType.ReferencesWithFormulas);

			// raise the appropriate notifications on the calculations
			_calculations.InvalidateReferences();
		}

		#endregion //RegisterReferences	

		#region RootReference

		internal override ItemCalculatorReferenceBase RootReference { get { return _helper != null ? _helper.RootReference as ItemCalculatorReferenceBase : null; } }

		#endregion //RootReference	

		#region SetResult

		internal override void SetResult(CalculationValue result, string name, FormulaRefBase reference)
		{
			_results.InternalSet(name, result);
		}

		#endregion //SetResult	

		#region ToString

		/// <summary>
		/// Returns a string that represents this object;
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(SRUtil.GetString("ItemCalculator_Desc"), this.ReferenceIdResolved, this.Item);
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

			_itemReferences.Clear();

			// raise the appropriate notifications on the calculations
			_calculations.InvalidateReferences();

			// update the results to filter out errors
			_results.OnIsItemSetChanged(false);
		}

		#endregion //UnregisterReferences

		#endregion //Base class overrides	
    
		#region Properties

		#region Public Properties

		#region Item

		/// <summary>
		/// Specifies the item whose properties will provide source values for calculations. A property on the item
		/// can also be a target of a formula calculation.
		/// </summary>
		public object Item
		{
			get { return _item; }
			set
			{
				if (value != _item)
				{
					INotifyPropertyChanged notifier = _item as INotifyPropertyChanged;

					if (notifier != null)
						notifier.PropertyChanged -= new PropertyChangedEventHandler(this.OnItemPropertyChanged);

					_item = value;
					
					notifier = _item as INotifyPropertyChanged;

					if (notifier != null)
						notifier.PropertyChanged += new PropertyChangedEventHandler(this.OnItemPropertyChanged);

					// JJD 10/27/11 - TFS92815
					// Special case known types (e.g. string, int, double, dateTime etc.). For these types
					// we expose a single pseudo read-only property named 'Value' which return the item itself.
					// So check if the item's type is a known type and then cache it for later use
					if ( _item != null )
					{
						if (CoreUtilities.IsKnownType(_item.GetType()))
							_knownType = _item.GetType();
					}
					else
						_knownType = null;

					// dirty all references 
					this.DirtyAllReferencesAsync();
				}
			}
		}

		#endregion //Item

		#region Calculations

		/// <summary>
		/// Collection of calculations that will be performed on the item. Source values of a calculation can be
		/// properties of the item or other calculations (referenced via their ReferenceIds).
		/// </summary>

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content)]

		public ItemCalculationCollection Calculations
		{
			get { return _calculations; }
		}

		#endregion //Calculations

		#region Results

		/// <summary>
		/// Returns a dictionary containing the results of the items calculations (read-only).
		/// </summary>
		/// <seealso cref="Calculations"/>
		public CalculationResultsDictionary Results
		{
			get
			{
				return _results;
			}
		}

		#endregion //Results

		#endregion //Public Properties	
    
		#endregion //Properties	
 
		#region Methods

		#region Internal Methods

		#endregion //Internal Methods	

		#region Private Methods

		#region OnItemPropertyChanged

		private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ItemPropertyReference itemRef;

			if (_itemReferences.TryGetValue(e.PropertyName, out itemRef))
				itemRef.OnPropertyChanged();
		}

		#endregion //OnCalculationsChanged

		#endregion //Private Methods	
    
		#endregion //Methods
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