using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;




using Microsoft.VisualBasic;


namespace Infragistics.Documents.Excel.CalcEngine
{

	#region RecalcChainSortComparer 
	





	internal class RecalcChainSortComparer : IComparer
	{
		int IComparer.Compare( object xVal, object yVal )
		{
			// MD 8/17/08 - Excel formula solving
			// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
			//UltraCalcFormula xFormula = xVal is IUltraCalcReference
			//    ? (UltraCalcFormula)((IUltraCalcReference)xVal).Formula
			//    : null;
			//
			//UltraCalcFormula yFormula = yVal is IUltraCalcReference
			//    ? (UltraCalcFormula)((IUltraCalcReference)yVal).Formula
			//    : null;				
			ExcelCalcFormula xFormula = xVal is IExcelCalcReference
				? (ExcelCalcFormula)((IExcelCalcReference)xVal).Formula
				: null;

			ExcelCalcFormula yFormula = yVal is IExcelCalcReference
				? (ExcelCalcFormula)((IExcelCalcReference)yVal).Formula
				: null;				

		//	Debug.Assert( null != xFormula && null != yFormula );

			// Sort numbers are always greater than 0 so this will take care of null
			// references and references with null formulas as well although formulas
			// should not be null.
			//
			int xSortNumber = null != xFormula ? xFormula.DependencySortNumber : -1;
			int ySortNumber = null != yFormula ? yFormula.DependencySortNumber : -1;

			return xSortNumber < ySortNumber ? -1 : ( xSortNumber > ySortNumber ? 1 : 0 );
		}
	}

	#endregion // RecalcChainSortComparer 

	#region ReferenceHolder Class



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal class ReferenceHolder
	{
		#region Private Vars

		private IExcelCalcReference reference;
		private ExcelCalcFormula referenceFormula;

		#endregion // Private Vars

		#region Constructor

		internal ReferenceHolder(IExcelCalcReference reference, ExcelCalcFormula referenceFormula)
		{
			if ( null == reference )
				throw new ArgumentNullException( "reference" );

			this.reference = reference;
			this.referenceFormula = null != referenceFormula ? referenceFormula : (ExcelCalcFormula)reference.Formula;
		}

		#endregion // Constructor

		#region Reference






		internal IExcelCalcReference Reference
		{
			get
			{
				return this.reference;
			}
		}

		#endregion // Reference

		#region Formula






		internal ExcelCalcFormula Formula
		{
			get
			{
				return this.referenceFormula;
			}
		}			

		#endregion // Formula

		#region GetHashCode

		public override int GetHashCode( )
		{
			return this.reference.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			ReferenceHolder rh = obj as ReferenceHolder;
			return null != rh && ExcelCalcEngine.CompareReferences( this.reference, rh.reference );
		}

		#endregion // Equals
	}

	#endregion // ReferenceHolder Class

	#region HashSet






	internal class HashSet : ICollection
	{
		#region Private Vars

		private static readonly object DUMMY_VALUE = DBNull.Value;

		// MD 8/27/08 - Code Analysis - Performance
		//private Hashtable table = null;
        private Dictionary<object, object> table;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public HashSet( )
		{
		    this.table = new Dictionary<object, object>();
		}

		#endregion // Constructor

		#region Add

		/// <summary>
		/// Adds the item to the set. If the item already exists in the set, does nothing.
		/// </summary>
		/// <param name="item"></param>
		public void Add( object item )
		{
			this.table[ item ] = DUMMY_VALUE;
		}

		#endregion // Add

		#region Remove

		/// <summary>
		/// Removes the specified item from the set. If the item doesn't exist in the set
		/// does nothing.
		/// </summary>
		/// <param name="item"></param>
		public void Remove( object item )
		{
			this.table.Remove( item );
		}

		#endregion // Remove

		#region Exists

		/// <summary>
		/// Returns true if the specified item exists in this set.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Exists( object item )
		{
			return this.table.ContainsKey( item );
		}

		#endregion // Exists

		#region AddItems

		/// <summary>
		/// Adds items from the specified set to this set.
		/// </summary>
		/// <param name="source"></param>
		public void AddItems( HashSet source )
		{
			foreach ( object item in source )
				this.Add( item );
		}

		#endregion // AddItems

		#region GetUnion

		/// <summary>
		/// Calculates the union of the specified sets.
		/// </summary>
		/// <param name="set1"></param>
		/// <param name="set2"></param>
		/// <returns></returns>
		public static HashSet GetUnion( HashSet set1, HashSet set2 )
		{
			HashSet result = new HashSet( );
			result.AddItems( set1 );
			result.AddItems( set2 );

			return result;
		}

		#endregion // GetUnion

		#region GetIntersection

		/// <summary>
		/// Calculates the intersection of the specified sets.
		/// </summary>
		/// <param name="set1"></param>
		/// <param name="set2"></param>
		/// <returns></returns>
		public static HashSet GetIntersection( HashSet set1, HashSet set2 )
		{
			HashSet result = new HashSet( );

			if ( set1.Count > set2.Count )
				return GetIntersection( set2, set1 );

			foreach ( object item in set1 )
			{
				if ( set2.Exists( item ) )
					result.Add( item );
			}

			return result;
		}

		#endregion // GetIntersection

		#region DoesIntersect

		/// <summary>
		/// Returns true of the specified set and this set intersect.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public bool DoesIntersect( HashSet s )
		{
			if ( this.Count > s.Count )
				return s.DoesIntersect( this );

			foreach ( object item in this )
			{
				if ( s.Exists( item ) )
					return true;
			}

			return false;
		}

		#endregion // DoesIntersect

		#region IsSubsetOf

		/// <summary>
		/// Returns true if this set is a subset of the specified set.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public bool IsSubsetOf( HashSet s )
		{
			if ( this.Count > s.Count )
				return false;

			foreach ( object item in this )
			{
				if ( ! s.Exists( item ) )
					return false;
			}

			return true;
		}

		#endregion // IsSubsetOf

		#region Clear

		/// <summary>
		/// Clears the set.
		/// </summary>
		public void Clear( )
		{
			this.table.Clear( );
		}

		#endregion // Clear

		#region Count

		/// <summary>
		/// Returns the number of items contained in the set.
		/// </summary>
		public int Count
		{
			get
			{
				return this.table.Count;
			}
		}

		#endregion // Count

		#region IsEmpty

		/// <summary>
		/// Returns true if the set is empty, that is it has no elements.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return 0 == this.Count;
			}
		}

		#endregion // IsEmpty

		#region GetEnumerator

		/// <summary>
		/// Returns a new enumerator that enumerates all the elements of this set.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator( )
		{
			return this.table.Keys.GetEnumerator( );
		}

		#endregion // GetEnumerator

		#region IsSynchronized

		/// <summary>
		/// Indicates whether this data structure is synchronized.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
			    return CalcManagerUtilities.IsSynchronized(this.table);
			}
		}

		#endregion // IsSynchronized

		#region CopyTo

		/// <summary>
		/// Copies all the elements of this set to the spcified array starting at the specified index in the array.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo( System.Array array, int arrayIndex )
		{
			this.table.Keys.CopyTo( array as object[], arrayIndex );
		}

		#endregion // CopyTo

		#region SyncRoot

		/// <summary>
		/// Returns the object that can be used to synchronize the access to this data structure.
		/// </summary>
		public object SyncRoot
		{
			get
			{
                return CalcManagerUtilities.SyncRoot(this.table);
			}
		}

		#endregion // SyncRoot

		#region ToArray

		/// <summary>
		/// Returns an array containing all the elements of this set.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public object[] ToArray( Type type )
		{
			Array arr = Array.CreateInstance( type, this.Count );
			this.CopyTo( arr, 0 );
			return (object[])arr;
		}

		#endregion // ToArray
	}

	#endregion // HashSet

	internal class CalcManagerUtilities
    {
        internal static DateTime DateAndTimeDateAdd(string interval, double number, DateTime dateValue)
        {



            return DateAndTime.DateAdd(interval, number, dateValue);

        }

        internal static DateTime DateAndTimeDateAdd(DateInterval interval, double number, DateTime dateValue)
		{



            return DateAndTime.DateAdd(interval, number, dateValue);

        }

        internal static long DateAndTimeDateDiff(string interval, DateTime date1, DateTime date2, FirstDayOfWeek firstDayOfWeek, FirstWeekOfYear firstWeekOfYear)
		{



            return DateAndTime.DateDiff(interval, date1, date2, firstDayOfWeek, firstWeekOfYear);

        }

        internal static long DateAndTimeDateDiff(DateInterval interval, DateTime date1, DateTime date2, FirstDayOfWeek firstDayOfWeek, FirstWeekOfYear firstWeekOfYear)
		{



            return DateAndTime.DateDiff(interval, date1, date2, firstDayOfWeek, firstWeekOfYear);

        }

        [Conditional("DEBUG")]
        internal static void DebugFail(string message)
		{



            Debug.Fail(message);

        }

        [Conditional("DEBUG")]
        internal static void DebugWriteLineIf(bool condition, string message)
		{






            Debug.WriteLineIf(condition, message);

        }

        /// <summary>
        /// Get the values from enumeration
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns>array of values in the enumeration</returns>
        internal static System.Array EnumGetValues(Type enumType)
		{


#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

            return Enum.GetValues(enumType);

        }

        internal static bool IsSynchronized(ICollection syncObject)
		{



            return syncObject.IsSynchronized;

        }

		internal static object SyncRoot(ICollection syncObject)
		{



			return syncObject.SyncRoot;

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