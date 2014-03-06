using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;

namespace Infragistics
{
	#region ISupportPositioning Interface

	/// <summary>
	/// An interface implemented by objects that need to be sorted based on a list of before and after relationships.
	/// </summary>
	public interface ISupportPositioning
	{
		/// <summary>
		/// An array of string keys that represent the items before which the current item should be sorted.
		/// </summary>
		IEnumerable<string> Before { get; }

		/// <summary>
		/// An array of string keys that represent the items after which the current item should be sorted.
		/// </summary>
		IEnumerable<string> After { get; }

		/// <summary>
		/// The string key of the current item.  This key will be referenced in the <see cref="Before"/> and <see cref="After"/> lists of other items.
		/// </summary>
		string Key { get; }
	}
	#endregion //ISupportPositioning Interface

	#region PositionSorter Class
	internal class PositionSorter
	{
		#region Member Variables

		private IComparer<ISupportPositioning> 
											_defaultComparer;
		private List<ISupportPositioning>	_list;
		private Dictionary<string, IList<ISupportPositioning>>	
											_map = new Dictionary<string, IList<ISupportPositioning>>();
		private Dictionary<ISupportPositioning, HashSet<ISupportPositioning>> 
											_befores = new Dictionary<ISupportPositioning, HashSet<ISupportPositioning>>();
		private Dictionary<ISupportPositioning, HashSet<ISupportPositioning>> 
											_afters = new Dictionary<ISupportPositioning, HashSet<ISupportPositioning>>();

		private Dictionary<ISupportPositioning, HashSet<ISupportPositioning>> 
											_aggregatedBefores = new Dictionary<ISupportPositioning, HashSet<ISupportPositioning>>();
		private Dictionary<ISupportPositioning, HashSet<ISupportPositioning>> 
											_aggregatedAfters = new Dictionary<ISupportPositioning, HashSet<ISupportPositioning>>();

		private bool						_indeterminateOrderDetected;

		#endregion //Member Variables

		#region Constructor
		private PositionSorter()
		{
		}
		#endregion //Constructor

		#region Methods

		#region Public Methods

		#region Sort
		public static void Sort(List<ISupportPositioning> list, out bool isValid, IComparer<ISupportPositioning> defaultComparer = null)
		{
			PositionSorter sorter = new PositionSorter
			{
				_list = list,
				_defaultComparer = defaultComparer
			};

			isValid = sorter.Sort();
		}
		#endregion //Sort

		#endregion //Public Methods

		#region Private Methods

		#region AggregateBefores
		private void AggregateBefores(ISupportPositioning item, HashSet<ISupportPositioning> result)
		{
			foreach (var ii in _befores[item])
			{
				if (!result.Contains(ii))
				{
					result.Add(ii);
					this.AggregateBefores(ii, result);
				}
			}
		}
		#endregion //AggregateBefores

		#region AggregateAfters
		private void AggregateAfters(ISupportPositioning item, HashSet<ISupportPositioning> result)
		{
			foreach (var ii in _afters[item])
			{
				if (!result.Contains(ii))
				{
					result.Add(ii);
					this.AggregateAfters(ii, result);
				}
			}
		}
		#endregion //AggregateAfters

		#region CompareHelper
		private int CompareHelper(ISupportPositioning x, ISupportPositioning y, int level = 0)
		{
			if (x == y)
				return 0;

			// If y is in the list of aggregated 'befores' of x then x occurs before y.
			// 
			if (_aggregatedBefores[x].Contains(y))
				return -1;

			// If y is in the list of aggregated 'afters' of x then x occurs after y.
			// 
			if (_aggregatedAfters[x].Contains(y))
				return 1;

			if (1 == level)
			{
				if (x.Key != y.Key)
					_indeterminateOrderDetected = true;

				if (_defaultComparer != null)
					return _defaultComparer.Compare(x, y);

				return 0;
			}

			return -this.CompareHelper(y, x, 1);
		}
		#endregion //CompareHelper

		#region GetBeforeAfterItems
		private HashSet<ISupportPositioning> GetBeforeAfterItems(ISupportPositioning item, bool getBefore)
		{
			var keys = getBefore ? item.Before : item.After;

			HashSet<ISupportPositioning> result = new HashSet<ISupportPositioning>();
			if (null != keys)
				result.UnionWith(keys.Where(ii => _map.ContainsKey(ii)).SelectMany(ii => _map[ii]));

			return result;
		}
		#endregion //GetBeforeAfterItems

		#region Sort
		private bool Sort()
		{
			bool isValid = true;
			// Create a dictionary that maps key to item or items (to allow for duplicate keys).
			// 
			foreach (var ii in _list)
			{
				IList<ISupportPositioning> list;
				if (!_map.TryGetValue(ii.Key, out list))
					_map[ii.Key] = list = new List<ISupportPositioning>();

				list.Add(ii);
			}

			// Create separate before and after lists that we can manipulate.
			// 
			foreach (var ii in _list)
			{
				_befores[ii] = this.GetBeforeAfterItems(ii, true);
				_afters[ii] = this.GetBeforeAfterItems(ii, false);
			}

			// Basically this fills in before and after lists to mirror the concept
			// that if "a" is before "b" then "b" is after "a".
			// 
			foreach (var ii in _list)
			{
				foreach (var jj in _befores[ii])
					_afters[jj].Add(ii);

				foreach (var jj in _afters[ii])
					_befores[jj].Add(ii);
			}

			// This loop creates aggregate before and after sets for each item. Aggregate before
			// set contains before's of the item and all their before's recursively. Likewise
			// with the after's.
			// 
			foreach (var ii in _list)
			{
				HashSet<ISupportPositioning> aggregatedBeforesSet	= new HashSet<ISupportPositioning>();
				HashSet<ISupportPositioning> aggregatedAftersSet	= new HashSet<ISupportPositioning>();

				this.AggregateBefores(ii, aggregatedBeforesSet);
				this.AggregateAfters(ii, aggregatedAftersSet);

				_aggregatedBefores[ii] = aggregatedBeforesSet;
				_aggregatedAfters[ii] = aggregatedAftersSet;

				// If aggregated befores and aggregated afters intersect then invalid values
				// are provided for before and after such that the item occurs before AND after
				// another item.
				// 
				if (aggregatedBeforesSet.Overlaps(aggregatedAftersSet))
				{
					//throw new ArgumentException("Item occurs before and after same item.");
					isValid = false;
				}
			}

			// We are using the framework Sort method here which is not a proper sort.  This could end up changing the order
			// of items from their original order if the items do not specify before/after keys.  If this proves to 
			// be a problem we can use our IG SortMerge, but this would require access to the shared 
			// assembly for all platforms which is not currenty the case.
			_list.Sort((x, y) => this.CompareHelper(x, y));

			// This flag is set by the CompareHelper when order if two items being compared
			// cannot be determined unambiguously.
			// 
			if (_indeterminateOrderDetected)
			{
				//Debug.WriteLine("Incomplete information to determine order.");
			}

			return isValid;
		}
		#endregion //Sort

		#endregion //Private Methods

		#endregion //Methods
	}
	#endregion //PositionSorter Class
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