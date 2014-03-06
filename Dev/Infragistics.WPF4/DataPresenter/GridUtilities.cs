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
using Infragistics.Windows.Selection;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.DataPresenter.Internal;
using System.Windows.Media.Media3D;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Xml;
using Infragistics.Controls.Layouts.Primitives;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Infragistics.Collections;






namespace Infragistics.Windows.DataPresenter
{
	#region GridUtilities Class

	internal static class GridUtilities
	{
		#region Members

		// JJD 4/21/11 - TFS73048 - Optimaztion - added list of elements to invlidate asynchronously
		[ThreadStatic()]
		private static HashSet _QueuedMeasureInvalidations;

		#endregion //Members	
    
		#region Methods

		#region Aggregate

		/// <summary>
		/// Aggregates items in the specified collections.
		/// </summary>
		/// <param name="collections"></param>
		/// <returns>Returns the array of items.</returns>
		public static T[] Aggregate<T>( params IEnumerable<T>[] collections )
		{
			List<T> list = new List<T>( );
			foreach ( IEnumerable<T> cc in collections )
			{
				if ( null != cc )
					list.AddRange( cc );
			}

			return list.ToArray( );
		}

		#endregion // Aggregate

		#region AreClose

		public static bool AreClose( double x, double y )
		{
            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
			//return Utilities.DoubleIsZero( x - y );
            if (x == y)
                return true;

            return Math.Abs(x - y) < .0000000001;
        }

		// AS 6/11/09 TFS18382
		public static bool AreClose(double x, double y, double tolerance)
		{
			if (x == y)
				return true;

			return Math.Abs(x - y) <= tolerance;
		}

		public static bool AreClose( Size x, Size y )
		{
			return AreClose( x.Width, y.Width )
				&& AreClose( x.Height, y.Height );
		}

		public static bool AreClose( Rect x, Rect y )
		{
			return AreClose( x.Width, y.Width )
				&& AreClose( x.Height, y.Height )
                && AreClose( x.Left, y.Left )
                && AreClose( x.Top, y.Top );
		}

		public static bool AreClose( Thickness x, Thickness y )
		{
			return AreClose( x.Left, y.Left )
				&& AreClose( x.Top, y.Top )
				&& AreClose( x.Right, y.Right )
				&& AreClose( x.Bottom, y.Bottom );
		}

		#endregion // AreClose

		// JJD 11/18/11 - TFS79001 - added
		#region AreEqual

		internal static bool AreEqual(object item1, object item2)
		{
			// if the '==' operator returns true then retirn true
			if (item1 == item2)
				return true;

			// Otherwise for value types use Object.Equals
			Type type = item1 != null ? item1.GetType() : null;

			if (type != null && type.IsValueType)
				return Object.Equals(item1, item2);

			return false;
		}

		// SSP 1/16/12 TFS98572
		// 
		/// <summary>
		/// Compares x and y for equality.
		/// </summary>
		/// <param name="x">A string to compare.</param>
		/// <param name="y">A string to compare.</param>
		/// <param name="considerNullAsEmptyString">If true then null and "" will be considered equal.</param>
		/// <returns>True if the strings are equal. False otherwise.</returns>
		internal static bool AreEqual( string x, string y, bool considerNullAsEmptyString )
		{
			return considerNullAsEmptyString 
				? ( x ?? string.Empty ) == ( y ?? string.Empty ) 
				: x == y;
		}

		#endregion //AreEqual	
    
		#region AreKeysEqual

		public static bool AreKeysEqual( string xxKey, string yyKey )
		{
			return AreKeysEqual( xxKey, yyKey, false );
		}

		public static bool AreKeysEqual( string xxKey, string yyKey, bool ignoreCase )
		{
			return 0 == Compare( xxKey, yyKey, ignoreCase )
				// null and empty string are not considered actual keys and thus should not
				// equal to even themselves - that is the indexer[null] or indexer[""] should 
				// never match an item.
				&& ! string.IsNullOrEmpty( xxKey ) && ! string.IsNullOrEmpty( yyKey );
		}

		#endregion // AreKeysEqual

		#region AreSameTypeAndEqual

		
		public static bool AreSameTypeAndEqual( object x, object y )
		{
			return x == y
				|| null != x && null != y && x.GetType( ) == y.GetType( ) && object.Equals( x, y );
		}

		#endregion // AreSameTypeAndEqual

		#region ArrMax

		public static int ArrMax( int[] arr )
		{
			return ArrMax( arr, 0, arr.Length );
		}

		public static int ArrMax( int[] arr, int startIndex, int count )
		{
			int max = arr[startIndex];

			for ( int i = 1; i < count; i++ )
				max = Math.Max( max, arr[ startIndex + i ] );

			return max;
		}

		#endregion // ArrMax

		#region ArrSum

		public static double ArrSum( double[] arr, int startIndex, int count )
		{
			double r = 0;

			for ( int i = 0; i < count; i++ )
				r += arr[startIndex + i];

			return r;
		}

		#endregion // ArrSum

		#region Compare

		public static int Compare( string xx, string yy, bool ignoreCase )
		{
			return string.Compare( xx, yy, ignoreCase, GridUtilities.StringCompareCulture );
		}

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        public static int Compare(FixedFieldLocation x, FixedFieldLocation y)
        {
            if (x != y)
            {
                if (x == FixedFieldLocation.FixedToNearEdge)
                    return -1;
                else if (x == FixedFieldLocation.FixedToFarEdge)
                    return 1;
                else if (y == FixedFieldLocation.FixedToNearEdge)
                    return 1;
                else //if (y == FixedFieldLocation.FixedToFarEdge)
                    return -1;
            }

            return 0;
        }
		#endregion // Compare

		#region CompareLists

		/// <summary>
		/// Compares if items in the lists are the same in the same order. Null lists are treated as empty lists so
		/// a null list is equal to a list with 0 items.
		/// </summary>
		/// <param name="xList">List of items</param>
		/// <param name="yList">List of items</param>
		/// <returns>Returns true if the items are in the same order and same.</returns>
		public static bool CompareLists( IEnumerable xList, IEnumerable yList )
		{
			int xCount = xList is IList ? ( (IList)xList ).Count : -1;
			int yCount = yList is IList ? ( (IList)yList ).Count : -1;

			// If counts are different then return false.
			if ( xCount >= 0 && yCount >= 0 && xCount != yCount )
				return false;

			// Treat null lists as lists with 0 items. So null list will be considered
			// equal to a list with 0 items.
			if ( null == xList )
				xList = GetEmptyEnumerable( );

			if ( null == yList )
				yList = GetEmptyEnumerable( );

			IEnumerator yEnumerator = yList.GetEnumerator( );

			foreach ( object xItem in xList )
			{
				// If one enumerator is exhausted before the other, return false as
				// number of items are different.
				if ( !yEnumerator.MoveNext( ) )
					return false;

				if ( yEnumerator.Current != xItem )
					return false;
			}

			// If one enumerator is exhausted before the other, return false as
			// number of items are different.
			if ( yEnumerator.MoveNext( ) )
				return false;

			return true;
		}

		#endregion // CompareLists

		#region Contains

        internal static bool Contains(Rect r, Point pt, bool horz)
        {
            return (horz && r.X <= pt.X && pt.X <= r.Right)
                ||
                (!horz && r.Y <= pt.Y && pt.Y <= r.Bottom);
        }

		internal static bool Contains( IEnumerable e, object item )
		{
			foreach ( object i in e )
			{
				if ( i == item )
					return true;
			}

			return false;
		}

		#endregion // Contains

		// AS 4/12/11 TFS62951
		// The Contains method of a Rect considers the Right/Bottom to be part of the rect. This routine 
		// considers the right/bottom as exclusive.
		//
		#region ContainsExclusive
		internal static bool ContainsExclusive(Rect rect, Point pt)
		{
			if (pt.X >= rect.X || AreClose(rect.X, pt.X))
			{
				if (pt.X < rect.Right && !AreClose(rect.Right, pt.X))
				{
					if (pt.Y >= rect.Y || AreClose(rect.Y, pt.Y))
					{
						if (pt.Y < rect.Bottom && !AreClose(rect.Bottom, pt.Y))
						{
							return true;
						}
					}
				}
			}

			return false;
		} 
		#endregion //ContainsExclusive

		// AS 10/13/09 NA 2010.1 - CardView
		// In a compressed card view we are going to collapsed the cell(value)presenter elements 
		// if there is an empty value.
		//
		#region CoerceCollapsedCellVisibility
		private static void CoerceCollapsedCellVisibility(DependencyObject d, ref object newValue)
		{
			Field f = null;
			Record r = null;

			CellValuePresenter cvp = d as CellValuePresenter;

			if (null != cvp)
			{
				f = cvp.Field;
				r = cvp.Record;
			}
			else
			{
				CellPresenter cp = d as CellPresenter;

				if (cp != null)
				{
					f = cp.Field;
					r = cp.Record;
				}
			}

			if (null != r && null != f && r.ShouldCollapseCell(f))
			{
				// AS 1/13/12 TFS73068
				// We may need the cell to provide the preferred size.
				//
				//newValue = KnownBoxes.VisibilityCollapsedBox;
				newValue = KnownBoxes.VisibilityHiddenBox;
			}
		}
		#endregion //CoerceCollapsedCellVisibility

		// AS 8/24/09 TFS19532
		#region CoerceFieldElementVisibility
		internal static object CoerceFieldElementVisibility(DependencyObject d, object newValue)
		{
			// previously we would set the Visibility of the cell element when the cell 
			// element (e.g. CVP, LP) was first initialized with the field. this worked 
			// because whenever the visibility of the field changed we would recreate the 
			// layout and the elements. however now we just update the existing layout 
			// and so the fields were not getting the updated visibility. since the 
			// visibility could go from hidden to visible though and we were previously
			// only setting the visibility if it was not visible (to allow the visibility 
			// to be set in element triggers - e.g. to hide a cell) we could not simply 
			// bind the visibility of the element to that of the field's visibility. so 
			// instead we will bind the attached FieldVisibility property to the field's 
			// VisibilityResolved. When that changes we will then coerce the visibility 
			// using the "most hidden" setting (i.e. collapsed if either is collapsed, 
			// hidden if either is hidden and visible otherwise
			if (newValue == null || !Visibility.Collapsed.Equals(newValue))
			{
				Visibility fieldVis = GetFieldVisibility(d);

				switch (fieldVis)
				{
					case Visibility.Collapsed:
						newValue = KnownBoxes.VisibilityCollapsedBox;
						break;
					case Visibility.Hidden:
						newValue = KnownBoxes.VisibilityHiddenBox;
						break;
				}

				// AS 8/26/09 CellContentAlignment
				if (fieldVis != Visibility.Collapsed)
				{
					CellContentAlignment cca = GetCellContentAlignment(d);

					if (CellContentAlignment.Default != cca)
					{
						if (d is LabelPresenter)
						{
							if (cca == CellContentAlignment.ValueOnly)
								newValue = KnownBoxes.VisibilityCollapsedBox;
						}
						else
						{
							Debug.Assert(d is CellValuePresenter || d is SummaryResultsPresenter);

							if (cca == CellContentAlignment.LabelOnly)
								newValue = KnownBoxes.VisibilityCollapsedBox;
						}
					}

					// AS 10/13/09 NA 2010.1 - CardView
					if (!Visibility.Collapsed.Equals(newValue))
						CoerceCollapsedCellVisibility(d, ref newValue);
				}
			}

			return newValue;
		}
		#endregion //CoerceFieldElementVisibility

        // AS 12/9/08 NA 2009 Vol 1 - Fixed Fields
        #region CreateGridBagLayoutManager
        internal static FieldGridBagLayoutManager CreateGridBagLayoutManager(FieldLayout fieldLayout, LayoutManagerType type)
        {
            FieldGridBagLayoutManager lm = new FieldGridBagLayoutManager(fieldLayout, type);
            lm.ExpandToFitHeight = true;
            lm.ExpandToFitWidth = true;

            return lm;
        } 
        #endregion //CreateGridBagLayoutManager

        // JJD 1/20/09 - NA 2009 vol 1 
        #region CreateRecordContentMarginBinding

        internal static BindingBase CreateRecordContentMarginBinding(FieldLayout fieldLayout, bool isHeader, Type targetType)
        {
            Debug.Assert(fieldLayout != null);

            if (fieldLayout == null)
                return null;

            MultiBinding mb = new MultiBinding();
            Binding binding = new Binding();
            binding.RelativeSource = RelativeSource.Self;
            binding.Mode = BindingMode.OneWay;
            mb.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = fieldLayout;
            binding.Mode = BindingMode.OneWay;
            binding.Path = new PropertyPath("RecordIndentVersion");
            mb.Bindings.Add(binding);
 
            // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view
            // Also bind to the DataContext so when a presenter is recycled
            // for another record the converter will be called
            binding = new Binding();
            binding.RelativeSource = RelativeSource.Self;
            binding.Mode = BindingMode.OneWay;
            binding.Path = new PropertyPath("DataContext");
            mb.Bindings.Add(binding);
 
            // JJD 8/26/09 - NA 2009 Vol 2 - Enhanced grid view
            
            // Added attached version number property that can be used to trigger a re-conversion
            binding = new Binding();
            binding.RelativeSource = RelativeSource.Self;
            binding.Mode = BindingMode.OneWay;
            binding.Path = new PropertyPath(RecordContentMarginConverter.VersionProperty);
            mb.Bindings.Add(binding);

            mb.Mode = BindingMode.OneWay;
            mb.Converter = RecordContentMarginConverter.Instance;
            mb.ConverterParameter = new object[] { KnownBoxes.FromValue(isHeader), targetType };

            return mb;
        }

        #endregion //CreateRecordContentMarginBinding	

        // JJD 2/9/09 - TFS13678 - added
        #region CopyNonSerializableSettings

        internal static void CopyNonSerializableSettings(object target, object source)
        {
            Debug.Assert(target != null);
            Debug.Assert(source != null);

            if (target == null || source == null)
                return;

            Type type = target.GetType();

            Debug.Assert(source.GetType() == type);

            if (source.GetType() != type)
                return;

            PropertyInfo[] props = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);

            foreach (PropertyInfo pi in props)
            {
                Debug.Assert(pi.CanRead && pi.CanWrite);

                if (pi.CanRead && pi.CanWrite)
                {
                    object[] attribs = pi.GetCustomAttributes(typeof(DesignerSerializationVisibilityAttribute), false);

                    if (attribs.Length == 1 &&
                         attribs[0] is DesignerSerializationVisibilityAttribute &&
                        ((DesignerSerializationVisibilityAttribute)attribs[0]).Visibility == DesignerSerializationVisibility.Hidden)
                    {
                        object value = pi.GetValue(source, null);

                        pi.SetValue(target, value, null);
                    }
                }

            }
        }

        #endregion //CopyNonSerializableSettings	

		#region CopyTo<T>

		// SSP 2/11/10 - TFS26273
		// 
		/// <summary>
		/// Copies specified items to the array starting from arrayIndex in the array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public static void CopyTo<T>( IEnumerable<T> items, T[] array, int arrayIndex )
		{
			foreach ( T ii in items )
				array[arrayIndex++] = ii;
		}

		#endregion // CopyTo<T>

		#region Distance

		public static double Distance( Point a, Point b )
		{
			double d1 = a.X - b.X;
			double d2 = a.Y - b.Y;

			return Math.Sqrt( d1 * d1 + d2 * d2 );
		}

		#endregion // Distance

		#region DoesElementContainPoint

		/// <summary>
		/// Checks to see if the p is inside the elem.
		/// </summary>
		/// <param name="elem">Element.</param>
		/// <param name="p">Point relative to the element.</param>
		/// <returns>True if the point is inside element.</returns>
		internal static bool DoesElementContainPoint( FrameworkElement elem, Point p )
		{
			return p.X >= 0 && p.Y >= 0 && p.X < elem.ActualWidth && p.Y < elem.ActualHeight;
		}

		#endregion // DoesElementContainPoint

		#region EnsureInElementBounds

		/// <summary>
		/// Ensure that the point is in the bounds of the specified elem. It makes sure that point.X and point.Y
		/// are greater than 0 and less than the elem's ActualWidth/Height.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="elem"></param>
		/// <returns></returns>
		public static Point EnsureInElementBounds( Point point, FrameworkElement elem )
		{
			point.X = GridUtilities.EnsureInRange( point.X, 0, elem.ActualWidth );
			point.Y = GridUtilities.EnsureInRange( point.Y, 0, elem.ActualHeight );

			return point;
		}

		#endregion // EnsureInElementBounds

		#region EnsureInRange

		public static double EnsureInRange( double val, double min, double max )
		{
			return Math.Min( Math.Max( val, min ), max );
		}

		#endregion // EnsureInRange

		#region Filter<T>

		public static IEnumerable<T> Filter<T>( IEnumerable<T> items, Predicate<T> condition )
		{
			List<T> list = new List<T>( );

			if ( null != items )
			{
				foreach ( T item in items )
				{
					if ( condition.Invoke( item ) )
						list.Add( item );
				}
			}

			return list;
		}

		public static IEnumerable<T> Filter<T>( IEnumerable<T> items, IMeetsCriteria criteria )
		{
			return Filter<T>( items, criteria, false );
		}

		public static IEnumerable<T> Filter<T>( IEnumerable<T> items, IMeetsCriteria criteria, bool flatten )
		{
			// SSP 5/24/11 TFS74231
			// Binding an enumerable or collection that does not implement INotifyCollectionChanged causes the
			// ViewManager in the framework to hold a strong reference to the collection in CLR4. So use the
			// filtered enumerable that implements INotifyCollectionChanged.
			// 
			//IEnumerable<T> filteredItems = new MeetsCriteriaEnumerator<T>.Enumerable( items, criteria );
			IEnumerable<T> filteredItems = new NotifyCollectionEnumerable<T>( items, criteria );

			return flatten
				? new List<T>( filteredItems )
				: filteredItems;
		}

		#endregion // Filter<T>

		#region FocusTrivialAncestor

		
		
		/// <summary>
		/// Focuses the element if its Focusable is true otherwise finds an ancestor element 
		/// whose Focusable is true and focuses it.
		/// </summary>
		/// <param name="elem"></param>
		/// <returns></returns>
		internal static bool FocusTrivialAncestor( DependencyObject elem )
		{
			while ( null != elem )
			{
				IInputElement ii = elem as IInputElement;
				if ( null != ii && ii.Focusable )
					return ii.Focus( );

				elem = Utilities.GetParent( elem, false );
			}

			return false;
		}

		#endregion // FocusTrivialAncestor

		#region GetAttribute

		/// <summary>
		/// Returns the value of the specified attribute on the node. If attribute doesn't exist, returns null.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="attrib"></param>
		/// <returns></returns>
		public static string GetAttribute( XmlNode node, string attrib )
		{
			return GetAttribute( node, attrib, null );
		}

		/// <summary>
		/// Returns the value of the specified attribute on the node. If attribute doesn't exist, returns defaultValue.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="attrib"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static string GetAttribute( XmlNode node, string attrib, string defaultValue )
		{
			XmlAttribute a = node.Attributes[attrib];
			return null != a ? a.Value : null;
		}

		#endregion // GetAttribute

		// AS 4/12/11 TFS62951
		// Refactored from the AutoSizeHelper class and added support for getting to the header.
		//
		#region GetCellPanel
		internal static VirtualizingDataRecordCellPanel GetCellPanel(RecordPresenter rp, bool header)
		{
			if (rp == null)
				return null;

			VirtualizingDataRecordCellPanel vdrcp = null;
			FrameworkElement contentSite = null;

			if (header)
			{
				if (rp != null && rp.FieldLayout != null && rp.FieldLayout.LabelLocationResolved == LabelLocation.InCells)
					header = false;
			}

			if (!header)
			{
				// MD 8/19/10
				// If the associated cell panel is cached, return it instead of searching for it.
				DataRecordPresenter drp = rp as DataRecordPresenter;
				if (drp != null)
				{
					VirtualizingDataRecordCellPanel associatedPanel = drp.AssociatedVirtualizingDataRecordCellPanel;

					if (associatedPanel != null)
						return associatedPanel;
				}

				contentSite = rp.GetRecordContentSite();
			}
			else
			{
				contentSite = rp.GetHeaderContentSite();
			}

			if (null != contentSite)
			{
				vdrcp = Utilities.GetDescendantFromType(contentSite, typeof(VirtualizingDataRecordCellPanel), true) as VirtualizingDataRecordCellPanel;
			}

			return vdrcp;
		}
		#endregion //GetCellPanel

		#region GetConvertedCollection<TInput, TOutput>

		// SSP 2/11/10 - TFS26273
		// 
		public static ICollection<TOutput> GetConvertedCollection<TInput, TOutput>( IEnumerable items, int itemsCount, Converter<TInput, TOutput> converter )
		{
			IEnumerable<TInput> typedItems = new TypedEnumerable<TInput>( items );
			IEnumerable<TOutput> convertedItems = new ConverterEnumerable<TInput, TOutput>( typedItems, converter );
			
			return new EnumeratorCollection<TOutput>( convertedItems, itemsCount );
		}

		#endregion // GetConvertedCollection<TInput, TOutput>

		#region GetCount

		// SSP 6/28/10 TFS23257
		// Renamed the existing GetCount to GetCountNonOptimized and added the new GetCount
		// to alleviate some confusion.
		// 

		internal static int GetCount( IEnumerable e )
		{
			ICollection c = e as ICollection;

			if ( null != c )
				return c.Count;

			return GetCountNonOptimized( e );
		}

		internal static int GetCountNonOptimized( IEnumerable e )
		{
			// Note: Don't optimize this by checking if the enumrable is a list and if so
			// get the list' count etc... This can be used to traverse IList enumrables
			// that skip items (for example garbase collected entries) and therefore
			// checking count in those cases would not be appropriate.

			int count = 0;
			foreach ( object o in e )
				count++;

			return count;
		}

        internal static int GetCount(object[] arr, object value)
        {
            int c = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (value == arr[i])
                    c++;
            }

            return c;
        }
		#endregion // GetCount

		#region GetSyncLockedCount

		// SSP 1/3/12 TFS73767 - Optimizations
		// 
		internal static int GetSyncLockedCount( ArrayList list )
		{
			int count;

			lock ( list.SyncRoot )
			{
				count = list.Count;
			}

			return count;
		} 

		#endregion // GetSyncLockedCount

		#region GetDataPresenter

		internal static DataPresenterBase GetDataPresenter( RecordCollectionBase records )
		{
			return null != records ? GetDataPresenter( records.FieldLayout ) : null;
		}

		internal static DataPresenterBase GetDataPresenter( FieldLayout fl )
		{
			return null != fl ? fl.DataPresenter : null;
		}

		#endregion // GetDataPresenter

		#region GetDefaultCulture

		internal static CultureInfo GetDefaultCulture( DataPresenterBase dp )
		{
			return null != dp ? dp.DefaultConverterCulture : null;
		}

		internal static CultureInfo GetDefaultCulture( FieldLayout fl )
		{
			return GetDefaultCulture( null != fl ? fl.DataPresenter : null );
		}

		internal static CultureInfo GetDefaultCulture( Field field )
		{
			return null != field ? field.ConverterCultureResolved : null;
		}

		// SSP 9/30/11 Calc
		// 
		internal static CultureInfo GetDefaultCulture( Field field, FieldLayout fl )
		{
			return null != field ? GetDefaultCulture( field ) : GetDefaultCulture( fl );
		}

		#endregion // GetDefaultCulture
    
		#region GetEmptyEnumerable

		public static IEnumerable GetEmptyEnumerable( )
		{
			return new EmptyEnumerable<object>( );
		}

		#endregion // GetEmptyEnumerable

		#region GetFields

		public static FieldCollection GetFields( FieldLayout fieldLayout )
		{
			return null != fieldLayout ? fieldLayout.FieldsIfAllocated : null;
		}

		#endregion // GetFields

		#region GetField

		public static Field GetField( FieldLayout fieldLayout, string fieldName, bool throwException )
		{
			return GetField( GetFields( fieldLayout ), fieldName, throwException );
		}

		public static Field GetField( FieldCollection fields, string fieldName, bool throwException )
		{
			int index = null != fields ? fields.IndexOf( fieldName ) : -1;
			Field field = index >= 0 ? fields[index] : null;

			if ( throwException && null == field )
                throw new ArgumentException(DataPresenterBase.GetString("LE_MissingField", fieldName), "fieldName");

			return field;
		}

		#endregion // GetField

        // JJD 9/12/08 - added support for printing
        #region GetFieldFromControl

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        //internal static Field GetFieldFromControl(Control ctrl)
        internal static Field GetFieldFromControl(UIElement ctrl)
        {
            
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

			// AS 9/17/09 TFS22285
			// As an optimization I'm readding these known type checks since they don't have to 
			// read from a DP.
			//
			DataItemPresenter dip = ctrl as DataItemPresenter;

			if (dip != null)
				return dip.Field;

			CellPresenterBase cp = ctrl as CellPresenterBase;

			if (cp != null)
				return cp.Field;

            Field field = ctrl.GetValue(CellValuePresenter.FieldProperty) as Field;

            if (null == field)
            {
                Debug.Fail(string.Format("This control '{0}' uses its own defined Field DP or doesn't have one. In any case this method should be updated so we don't use PropertyDescriptors except as a fail safe.", ctrl));

                PropertyDescriptor pd = TypeDescriptor.GetProperties(ctrl)["Field"];

                if (null != pd)
                    field = pd.GetValue(ctrl) as Field;
            }

            return field;
        }

        #endregion //GetFieldFromControl	

		#region GetFieldLayout

		public static FieldLayout GetFieldLayout( Field field )
		{
			return null != field ? field.Owner : null;
		}

		public static FieldLayout GetFieldLayout( Record recordContext,
			Field fieldContext, RecordCollectionBase recordsContext, RecordManager recordManagerContext )
		{
			FieldLayout fl = null;

			if ( null == fl && null != recordContext )
				fl = recordContext.FieldLayout;

			if ( null == fl && null != fieldContext )
				fl = fieldContext.Owner;

			if ( null == fl && null != recordContext )
				fl = recordContext.FieldLayout;

			if ( null == fl && null != recordManagerContext )
				fl = recordManagerContext.FieldLayout;

			return fl;
		}

		// AS 4/12/11 TFS62951
		#region GetFieldLayout(ISelectableItem)
		public static FieldLayout GetFieldLayout(ISelectableItem item)
		{
			if (item is Field)
				return ((Field)item).Owner;

			if (item is Cell)
				return ((Cell)item).Field.Owner;

			if (item is Record)
				return ((Record)item).FieldLayout;

			Debug.Assert(item == null, "Unrecognized selectable item");
			return null;
		}
		#endregion //GetFieldLayout(ISelectableItem)

		#endregion // GetFieldLayout

		// AS 6/25/09 NA 2009.2 Field Sizing
		#region GetFieldRecordManager
		internal static RecordManager GetFieldRecordManager(FrameworkElement element, Field field, bool canAssumeRootManager)
		{
			if (element == null || field == null)
				return null;
			
            FieldLayout fl = field.Owner;

			if (fl == null)
				return null;

			// JJD 2/9/11 - TFS63916 
			// Get the attached inherited property first
			RecordManager rm = HeaderPresenter.GetRecordManager(element);

			if (rm != null)
				return rm;

			// JJD 2/9/11 - TFS63916 
			// Note: this code should not normally be executed since the
			// HeaderPresenter's RecordManager property should have already been set
			// but we left this code in here in case it hasn't been set yet for some reason
			RecordPresenter rp = element as RecordPresenter;

			if ( rp == null )
				rp = Utilities.GetAncestorFromType(element, typeof(RecordPresenter), true) as RecordPresenter;

			// JJD 2/9/11 - TFS63916 
			// refactored code into GetRPRecordManager and GetElementRecordManager methods
			if (rp != null)
				return GetRPRecordManager(rp, fl, canAssumeRootManager);

			return GetElementRecordManager(element, fl, canAssumeRootManager);
		}

		internal static RecordManager GetRPRecordManager(RecordPresenter rp, FieldLayout fl, bool canAssumeRootManager)
		{
			if (rp == null || fl == null)
				return null;

            // JJD 8/7/09 - NA 2009 Vol 2 - Enhanced grid view
            #region Check for header record ancestor
            
            DataPresenterBase dp = fl.DataPresenter;

			if (dp == null)
				return null;

			DataRecord dr = rp.Record as DataRecord;

			// AS 6/8/11 TFS78436
			// When the labels are within cells we really should just return the recordmanager 
			// for that data record and not the root recordmanager.
			//
			if (dr != null && dr.RecordType == RecordType.DataRecord)
				return dr.RecordManager;


			// JJD 10/26/11 - TFS91364 
			// HeaderRecords can exist in flat and non-flat views in the case of a child
			// island where all the records are filtered-out but shows the filter icon inside
			// the label, i.e. there is no separate filter record.
			//if (dp.IsFlatView)
			{
				HeaderRecord hr = rp.Record as HeaderRecord;

				if (hr != null)
				{
					Record attachedRcd = hr.AttachedToRecord;

					if (attachedRcd != null)
					{
						// if the attached to record is an expandable field record then
						// the header represents its children so return its child record manager
						ExpandableFieldRecord efr = attachedRcd as ExpandableFieldRecord;

						if (efr != null)
						{
							// JJD 09/22/11  - TFS84708 - Optimization
							// Use the ChildRecordManagerIfNeeded instead which won't create
							// child rcd managers for leaf records
							//return efr.ChildRecordManager;
							return efr.ChildRecordManagerIfNeeded;
						}

						return attachedRcd.ParentCollection.ParentRecordManager;
					}
				
					return dp.RecordManager;
				}
			}
			#endregion //Check for header record ancestor

			// JJD 10/26/11 - TFS91364 
			// Moved logic above for checking HeaderRecords since they can exist in both flat and non-flat views now
			//else
			if (!dp.IsFlatView)
			{
				// JJD 2/9/11 - TFS63916 
				// If we are in nested grid view then look for the parent adorner
				if (dp.CurrentViewInternal is GridView)
				{
					GridViewPanelAdorner gvpAdorner = Utilities.GetAncestorFromType(rp, typeof(GridViewPanelAdorner), true, dp) as GridViewPanelAdorner;

					if (gvpAdorner != null && gvpAdorner.GridViewPanel != null)
						return GetElementRecordManager(gvpAdorner.GridViewPanel, fl, canAssumeRootManager);
				}
			}
			
			// JJD 2/9/11 - TFS63916 
			// re-factored code into GetElementRecordManager
			return GetElementRecordManager(rp, fl, canAssumeRootManager);
		}

		internal static RecordManager GetElementRecordManager(FrameworkElement element, FieldLayout fl, bool canAssumeRootManager)
		{
			if (element == null || fl == null)
				return null;
            
            DataPresenterBase dp = fl.DataPresenter;

			if (dp == null)
				return null;
    
			RecordListControl rlc = Utilities.GetAncestorFromType(element, typeof(RecordListControl), true) as RecordListControl;

			RecordManager rm;

			if (rlc == null)
			{
				if (!canAssumeRootManager)
					return null;

                
                
                
                
                
                
                
                

                rm = dp.RecordManager;

				if (rm == null || rm.ContainsFieldLayout(fl) == false)
					return null;
			}
			else
			{
                // JJD 8/7/09 - NA 2009 Vol 2 - Enhanced grid view
                // Use ViewableRecords property instead
                //ViewableRecordCollection vrc = rlc.ItemsSource as ViewableRecordCollection;
                ViewableRecordCollection vrc = rlc.ViewableRecords;

				Debug.Assert(vrc != null, "RecordListControls itemsource should be a ViewableRecordCollection");

				if (vrc == null)
					return null;

				rm = vrc.RecordManager;

				Debug.Assert(rm != null, "ViewableRecordCollection should have a recordmanager");
			}

			return rm;
		}
		
		#endregion //GetFieldRecordManager

		#region GetFirstItem

		internal static T GetFirstItem<T>( IEnumerable<T> e )
		{
			if ( null != e )
			{
				IList<T> list = e as IList<T>;
				if ( null != list )
				{
					if ( list.Count > 0 )
						return list[0];
				}
				else
				{
					foreach ( T t in e )
						return t;
				}
			}

			return default( T );
		}

		#endregion // GetFirstItem

		#region GetFixedRecordLocation

		internal static FixedRecordLocationInternal GetFixedRecordLocation( bool top, bool isFixed )
		{
			if ( isFixed )
				return top ? FixedRecordLocationInternal.Top : FixedRecordLocationInternal.Bottom;

			return FixedRecordLocationInternal.None;
		}

		#endregion // GetFixedRecordLocation

		#region GetElemRect

		/// <summary>
		/// Gets the rect of elem relative to the specified ancestor.
		/// </summary>
		/// <param name="ancestor"></param>
		/// <param name="elem"></param>
		/// <returns></returns>
		public static Rect GetElemRect( FrameworkElement ancestor, FrameworkElement elem )
		{
			Rect rect = new Rect( elem.TranslatePoint( new Point( 0, 0 ), ancestor ), elem.RenderSize );
			return rect;
		}

		#endregion // GetElemRect

		// AS 8/25/09 TFS17560
		#region GetExpansionIndicator
		internal static ExpansionIndicator GetExpansionIndicator(RecordPresenter rp)
		{
			ExpansionIndicator ei = Utilities.GetTemplateChild<ExpansionIndicator>(rp);

			Debug.Assert(ei == null || ei.TemplatedParent == rp);

			return ei;
		}
		#endregion //GetExpansionIndicator

        // AS 1/22/09 NA 2009 Vol 1 - Fixed Fields
        #region GetImmediateDescendant
        /// <summary>
        /// Helper routine similar to find ancestor except that it returns the direct child that 
        /// contains the descendant.
        /// </summary>
        /// <param name="descendant">The nested descendant from which to start searching the ancestors</param>
        /// <param name="ancestor">The ancestor whose direct child is to be returned</param>
        internal static DependencyObject GetImmediateDescendant(DependencyObject descendant, DependencyObject ancestor)
        {
            while (descendant is ContentElement)
                descendant = LogicalTreeHelper.GetParent(descendant);

            while (descendant != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(descendant);

                if (parent == ancestor)
                    return descendant;

                descendant = parent;
            }

            return null;
        }
        #endregion //GetImmediateDescendant

        // JJD 08/21/09 - TFS18897
        #region GetNonPrimaryExtent

        internal static double GetNonPrimaryExtent(Size size, bool isVerticalOrientation)
        {
            if (isVerticalOrientation)
                return size.Width;
            else
                return size.Height;
        }

        #endregion //GetNonPrimaryExtent	

		#region GetOneAndOnlyOneItem

		// SSP 1/22/09 - NAS9.1 Record Filtering
		// 
		public static object GetOneAndOnlyOneItem( IEnumerable e )
		{
			object r = null;

			foreach ( object o in e )
			{
				if ( null != r )
					return null;

				r = o;
			}

			return r;
		}

		#endregion // GetOneAndOnlyOneItem

        // JJD 08/21/09 - TFS18897
        #region GetPrimaryExtent

        internal static double GetPrimaryExtent(Size size, bool isVerticalOrientation)
        {
            if (isVerticalOrientation)
                return size.Height;
            else
                return size.Width;
        }

        #endregion //GetPrimaryExtent	
    
		// JJD 1/12/09 - NA 2009 vol 1
        #region GetRecordTypeFromType

        internal static RecordType? GetRecordTypeFromType(Type type)
        {
            // Check for FilterRecord 1st because it is derivedfrom DataRecord
            if (typeof(FilterRecord).IsAssignableFrom(type))
                return RecordType.FilterRecord;

            if (typeof(DataRecord).IsAssignableFrom(type))
                return RecordType.DataRecord;

            if (typeof(GroupByRecord).IsAssignableFrom(type))
                return RecordType.GroupByField;

            if (typeof(SummaryRecord).IsAssignableFrom(type))
                return RecordType.SummaryRecord;

            if (typeof(ExpandableFieldRecord).IsAssignableFrom(type))
                return RecordType.ExpandableFieldRecord;

            return null;
        }

        #endregion //GetRecordTypeFromType	
    
        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
        #region GetRelativeLabelPlacement
        internal static void GetRelativeLabelPlacement(CellContentAlignment contentAlignment, out bool isLabelAboveOrBelow, out bool isLabelLeftOrRight)
        {
            isLabelAboveOrBelow = false;
            isLabelLeftOrRight = false;
            switch (contentAlignment)
            {
                case CellContentAlignment.LabelAboveValueAlignCenter:
                case CellContentAlignment.LabelAboveValueAlignLeft:
                case CellContentAlignment.LabelAboveValueAlignRight:
                case CellContentAlignment.LabelAboveValueStretch:
                case CellContentAlignment.LabelBelowValueAlignCenter:
                case CellContentAlignment.LabelBelowValueAlignLeft:
                case CellContentAlignment.LabelBelowValueAlignRight:
                case CellContentAlignment.LabelBelowValueStretch:
                    isLabelAboveOrBelow = true;
                    break;
                case CellContentAlignment.LabelLeftOfValueAlignBottom:
                case CellContentAlignment.LabelLeftOfValueAlignMiddle:
                case CellContentAlignment.LabelLeftOfValueAlignTop:
                case CellContentAlignment.LabelLeftOfValueStretch:
                case CellContentAlignment.LabelRightOfValueAlignBottom:
                case CellContentAlignment.LabelRightOfValueAlignMiddle:
                case CellContentAlignment.LabelRightOfValueAlignTop:
                case CellContentAlignment.LabelRightOfValueStretch:
                    isLabelLeftOrRight = true;
                    break;
            }
        } 
        #endregion //GetRelativeLabelPlacement

		#region GetTrivialDescendantOfType

		public static DependencyObject GetTrivialDescendantOfType( DependencyObject ancestor, Type type, bool allowSubtypes )
		{
			foreach ( DependencyObject obj in GetTrivialDescendants( ancestor ) )
			{
				if ( allowSubtypes ? IsObjectOfType( obj, type ) : type == obj.GetType( ) )
					return obj;
			}

			return null;
		}

		#endregion // GetTrivialDescendantOfType

		#region GetTrivialDescendants

		/// <summary>
		/// Returns both the logical and visual descendants of the element.
		/// </summary>
		/// <param name="elem"></param>
		/// <returns></returns>
		public static IEnumerable GetTrivialDescendants( DependencyObject elem )
		{
			System.Collections.Specialized.OrderedDictionary list = new System.Collections.Specialized.OrderedDictionary( );
			list.Add( elem, elem );
			GetDescendantsHelper( elem, list );
			return list.Keys;
		}

		private static void GetDescendantsHelper( DependencyObject elem, 
			System.Collections.Specialized.OrderedDictionary orderedTable )
		{
			if ( null != elem )
			{
				int startIndex = orderedTable.Count;

				if ( elem is Visual || elem is Visual3D )
				{
					int count = VisualTreeHelper.GetChildrenCount( elem );
					for ( int i = 0; i < count; i++ )
					{
						DependencyObject child = VisualTreeHelper.GetChild( elem, i );
						if ( null != child && !orderedTable.Contains( child ) )
							orderedTable.Add( child, child );
					}
				}

				foreach ( object item in LogicalTreeHelper.GetChildren( elem ) )
				{
					DependencyObject child = item as DependencyObject;
					if ( null != child && !orderedTable.Contains( child ) )
						orderedTable.Add( child, child );
				}

				for ( int i = startIndex, count = orderedTable.Count; i < count; i++ )
					GetDescendantsHelper( (DependencyObject)orderedTable[i], orderedTable );
			}
		}

		#endregion // GetTrivialDescendants

		#region GetTrivialDescendantOfType

		/// <summary>
		/// Returns descendant elements of the specified type. The order will be as such that the parent
		/// element will always be before the child element.
		/// </summary>
		/// <param name="ancestor">Element whose descendants to return.</param>
		/// <param name="type">Descendant elements of this type will be returned.</param>
		/// <param name="allowSubtypes">Whether derived types will be allowed.</param>
		/// <returns>Matching descendant elements.</returns>
		public static IEnumerable GetTrivialDescendantsOfType( DependencyObject ancestor, Type type, bool allowSubtypes )
		{
			ArrayList list = new ArrayList( );			

			foreach ( DependencyObject obj in GetTrivialDescendants( ancestor ) )
			{
				if ( allowSubtypes ? IsObjectOfType( obj, type ) : type == obj.GetType( ) )
					list.Add( obj );
			}

			return list;
		}

		#endregion // GetTrivialDescendantOfType

		#region GetUnionRect

		/// <summary>
		/// Returns the union of rects of specified framework elements in relation to the specified ancestor.
		/// </summary>
		/// <param name="ancestor"></param>
		/// <param name="frameworkElems"></param>
		/// <returns></returns>
		public static Rect GetUnionRect( FrameworkElement ancestor, IEnumerable frameworkElems )
		{
			Rect rect = new Rect( );
			foreach ( FrameworkElement elem in frameworkElems )
				rect.Union( GetElemRect( ancestor, elem ) );

			return rect;
		}

		#endregion // GetUnionRect

		#region GetValidationErrorText

		// SSP 6/28/10 TFS23257
		// 

		internal static string GetValidationErrorText( ValidationError error )
		{
			string exceptionMsg = null != error.Exception ? error.Exception.Message : null;
			return GridUtilities.ToString( error.ErrorContent, exceptionMsg );
		}

		internal static string GetValidationErrorText( IList<ValidationError> errors )
		{
			int count = errors.Count;
			if ( count > 0 )
			{
				if ( 1 == count )
				{
					return GetValidationErrorText( errors[0] );
				}
				else
				{
					StringBuilder sb = new StringBuilder( );

					for ( int i = 0; i < count; i++ )
					{
						string str = GetValidationErrorText( errors[i] );

						if ( !string.IsNullOrEmpty( str ) )
						{
							if ( sb.Length > 0 )
								sb.AppendLine( );

							sb.Append( str );
						}
					}

					return sb.ToString( );
				}
			}

			return null;
		}

		#endregion // GetValidationErrorText

		#region GetView

		internal static ViewBase GetView( Field field )
		{
			return null != field ? GetView( field.Owner ) : null;
		}

		internal static ViewBase GetView( FieldLayout fieldLayout )
		{
			DataPresenterBase dp = null != fieldLayout ? fieldLayout.DataPresenter : null;
			return null != dp ? dp.CurrentViewInternal : null;
		}

		#endregion // GetView

		#region HasItems

		public static bool HasItems( IEnumerable e )
		{
			if ( null != e )
			{
				if ( e is ICollection )
					return ( (ICollection)e ).Count > 0;

				foreach ( object o in e )
					return true;
			}

			return false;
		}

		#endregion // HasItems

		#region HasSameItems

		internal static HashSet ToSet( IEnumerable items )
		{
			HashSet h = new HashSet( );
			h.AddItems( items );

			return h;
		}

		internal static bool HasSameItems( IEnumerable set1, IEnumerable set2 )
		{
			return HashSet.AreEqual( ToSet( set1 ), ToSet( set2 ) );
		}

		#endregion // HasSameItems

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        #region IncludesFixedButton
        internal static bool IncludesFixedButton(FixedFieldUIType type)
        {
            return type == FixedFieldUIType.ButtonAndSplitter || type == FixedFieldUIType.Button;
        }
        #endregion //IncludesFixedButton

		#region IndexOfReferenceComparison

		internal static int IndexOfReferenceComparison( IList list, object obj )
		{
			for ( int i = 0, count = list.Count; i < count; i++ )
			{
				if ( object.ReferenceEquals( obj, list[i] ) )
					return i;
			}

			return -1;
		}

		#endregion // IndexOfReferenceComparison

		// AS 1/6/12 TFS29076
		#region IsAddRecordTemplate
		internal static bool IsAddRecordTemplate(Record record)
		{
			DataRecord dr = record as DataRecord;

			return null != dr && dr.IsAddRecordTemplate;
		} 
		#endregion //IsAddRecordTemplate

        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
        #region IsLabelAboveBelowCell
        internal static bool IsLabelAboveBelowCell(CellContentAlignment alignment)
        {
            bool isAboveBelow, isLeftRight;
            GetRelativeLabelPlacement(alignment, out isAboveBelow, out isLeftRight);
            return isAboveBelow;
        } 
        #endregion //IsLabelAboveBelowCell

        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
        #region IsLabelLeftRightCell
        internal static bool IsLabelLeftRightCell(CellContentAlignment alignment)
        {
            bool isAboveBelow, isLeftRight;
            GetRelativeLabelPlacement(alignment, out isAboveBelow, out isLeftRight);
            return isLeftRight;
        } 
        #endregion //IsLabelLeftRightCell

		#region IsFixed

		// SSP 2/9/09
		// 
		internal static bool IsFixed( FixedRecordLocationInternal fixedLocation )
		{
			return FixedRecordLocationInternal.Top == fixedLocation
				|| FixedRecordLocationInternal.Bottom == fixedLocation;
		}

		#endregion // IsFixed

		#region IsNullOrEmpty

		public static bool IsNullOrEmpty( object value )
		{
			if ( null == value || DBNull.Value == value )
				return true;

			string str = value as string;
			if ( null != str && 0 == str.Length )
				return true;

			return false;
		}

		#endregion // IsNullOrEmpty

		#region IsObjectOfType



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal static bool IsObjectOfType( object obj, Type type )
		{
			return null != obj && IsObjectOfType( obj.GetType( ), type );
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static bool IsObjectOfType( Type objectType, Type typeToCheck )
		{
			return objectType == typeToCheck || typeToCheck.IsAssignableFrom( objectType );
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal static bool IsObjectOfType( Type objectType, Type[] typesToCheck )
		{
			for ( int i = 0; i < typesToCheck.Length; i++ )
			{
				if ( IsObjectOfType( objectType, typesToCheck[i] ) )
					return true;
			}

			return false;
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static bool IsObjectOfType( object obj, Type[] typesToCheck )
		{
			for ( int i = 0; i < typesToCheck.Length; i++ )
			{
				if ( IsObjectOfType( obj, typesToCheck[i] ) )
					return true;
			}

			return false;
		}

		#endregion // IsObjectOfType

        // JJD 1/12/09 - NA 2009 vol 1
        #region IsRecordOfType

        internal static bool IsRecordOfType(Record rcd, RecordType rcdType)
        {
            if (rcd == null)
                return false;

            if (rcd.RecordType != rcdType)
                return false;

            if (rcd.IsEnabledResolved == false)
                return false;

            if (rcdType == RecordType.DataRecord && 
                ((DataRecord)rcd).IsAddRecordTemplate)
            {
                return false;
            }

            return true;
        }

        // JJD 2/5/09 - TFS13478 - added overload
        internal static bool IsRecordOfType(Record record, RecordType? rcdType, Type fallbackType)
        {
            if (rcdType != null)
                return GridUtilities.IsRecordOfType(record, rcdType.Value);
            else
            {
                // JJD 7/17/09 - TFS19588
                // Make sure that record is not null;
                if (record == null)
                    return false;

                return fallbackType.IsAssignableFrom(record.GetType());
            }
        }

        #endregion //IsRecordOfType

		#region IsSameSortCriteria

		// SSP 8/25/10 TFS30982
		// 
		internal static bool IsSameSortCriteria( IList<FieldSortDescription> x, IList<FieldSortDescription> y )
		{
			if ( x.Count != y.Count )
				return false;

			for ( int i = 0, count = x.Count; i < count; i++ )
			{
				if ( !FieldSortDescription.AreEqual( x[i], y[i] ) )
					return false;
			}

			return true;
		}		

		#endregion // IsSameSortCriteria

		// AS 5/4/09 NA 2009.2 ClipboardSupport
		#region IsSerializable
		internal static bool IsSerializable( object value )
		{
			if (null == value)
				return false;

			if (!value.GetType().IsSerializable)
				return false;

			return true;
		} 
		#endregion //IsSerializable

		#region IsTemplateRecords

		// SSP 10/31/11 TFS94847
		// 
		/// <summary>
		/// Indicates if the specified records collection is for template records.
		/// </summary>
		/// <param name="records"></param>
		/// <returns></returns>
		internal static bool IsTemplateRecords( RecordCollectionBase records )
		{
			FieldLayout fl = null != records ? records.FieldLayout : null;
			DataPresenterBase dp = null != fl ? fl.DataPresenter : null;
			FieldLayoutCollection flColl = null != dp ? dp.FieldLayoutsIfAllocated : null;

			if ( null != flColl )
			{
				RecordCollectionBase dataRecords = flColl.TemplateDataRecords;
				RecordCollectionBase groupByRecords = flColl.TemplateGroupByRecords;

				if ( records == dataRecords || records == groupByRecords )
					return true;

				return IsTrivialDescendantOf( dataRecords, records )
					|| IsTrivialDescendantOf( groupByRecords, records );
			}

			return false;
		}

		#endregion // IsTemplateRecords

		#region IsTrivialDescendantOf

		// SSP 10/31/11 TFS94847
		// 
		internal static bool IsTrivialDescendantOf( RecordCollectionBase ancestor, RecordCollectionBase descendant )
		{
			if ( ancestor == descendant )
				return true;

			Record parentRecord = null != descendant ? descendant.ParentRecord : null;
			RecordCollectionBase parentColl = null != parentRecord ? parentRecord.ParentCollection : null;

			return null != parentRecord && IsTrivialDescendantOf( ancestor, parentColl );
		}

		#endregion // IsTrivialDescendantOf

        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
        #region IsVariableHeightRecordMode
        internal static bool IsVariableHeightRecordMode(DataRecordSizingMode mode)
        {
            switch (mode)
            {
                case DataRecordSizingMode.IndividuallySizable:
                case DataRecordSizingMode.SizedToContentAndFixed:
                case DataRecordSizingMode.SizedToContentAndIndividuallySizable:
                    return true;
                case DataRecordSizingMode.Fixed:
                case DataRecordSizingMode.SizableSynchronized:
                    return false;
                default:
                    Debug.Fail("Unexpected resolved DataRecordSizingMode:" + mode.ToString());
                    return false;
            }
        } 
        #endregion //IsVariableHeightRecordMode

		// JM 10-27-09 NA 10.1 CardView
		#region IsXmlNode

		internal static bool IsXmlNodeOptimized(object item)
		{
			if (item != null)
			{
				// first check to see if the type is in the System.Xml namespace.
				// If not we can return false. This will prevent us from calling 
				// IsXmlModeInternal below which would otherwise possibly force the
				// unnecessary loading of the xml assembly
				if (s_isXmlAssemblyLoaded == false &&
					!item.GetType().FullName.StartsWith("System.Xml", StringComparison.Ordinal))
					return false;

				return IsXmlModeInternal(item);
			}

			return false;
		}

		private static bool s_isXmlAssemblyLoaded = false;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool IsXmlModeInternal(object item)
		{
			s_isXmlAssemblyLoaded = true;

			return item is XmlNode;
		}

		#endregion //IsXmlNode



#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


		// JJD 06/21/12 - TFS113886 added
		#region LogDebuggerError/Warning

		internal static void LogDebuggerError(string errorMsg)
		{
			LogDebuggerError("Global", errorMsg);
		}
		internal static void LogDebuggerError(string category, string errorMsg)
		{
			LogDebuggerErrorHelper(category, errorMsg, 50);
		}

		internal static void LogDebuggerWarning(string errorMsg)
		{
			LogDebuggerWarning("Global", errorMsg);
		}
		internal static void LogDebuggerWarning(string category, string errorMsg)
		{
			LogDebuggerErrorHelper(category, errorMsg, 40);
		}

		private static void LogDebuggerErrorHelper(string category, string errorMsg, int level)
		{
			if (Debugger.IsAttached && Debugger.IsLogging())
				Debugger.Log(level, category, errorMsg);
		}

		#endregion //LogDebuggerError	

        // JJD 8/27/09 - TFS21513
        // Moved logic from RecordManager
        #region MoveFixedRecords

        internal static bool MoveFixedRecords(Record[] records)
        {
            List<Record> topFixedRecords = new List<Record>();
            List<Record> bottomFixedRecords = new List<Record>();
            int delta = 0;

            for (int i = 0; i < records.Length; i++)
            {
                Record r = records[i];

                FixedRecordLocation fixedLocation = r.FixedLocation;
                if (fixedLocation != FixedRecordLocation.Scrollable)
                {
                    if (fixedLocation == FixedRecordLocation.FixedToTop)
                        topFixedRecords.Add(r);
                    else
                        bottomFixedRecords.Add(r);

                    records[i] = null;
                    delta++;
                }
                // JJD 7/01/09 - TFS18991
                // If we have any delta we need to compress the list by moving rcds
                //else if ( i != delta )
                else if (0 != delta)
                {
                    records[i - delta] = r;
                }
            }

            if (delta > 0)
            {
                Array.Copy(records, 0, records, topFixedRecords.Count, records.Length - topFixedRecords.Count - bottomFixedRecords.Count);

                for (int i = 0; i < topFixedRecords.Count; i++)
                    records[i] = topFixedRecords[i];

                for (int i = 0; i < bottomFixedRecords.Count; i++)
                    records[records.Length - bottomFixedRecords.Count + i] = bottomFixedRecords[i];
            }

            return delta > 0;
        }

        #endregion //MoveFixedRecords	

		#region NotifyCalcAdapter

		// SSP 9/11/2011 Calc
		// 
		internal static void NotifyCalcAdapter( DataPresenterBase dp, object sender, string propName, object extraInfo )
		{

			if ( null != dp )
			{
				Infragistics.Windows.DataPresenter.Calculations.IDataPresenterCalculationAdapterInternal adapter = dp._calculationAdapter;
				if ( null != adapter )
					adapter.OnPropertyValueChanged( sender, propName, extraInfo );
			}

		} 

		#endregion // NotifyCalcAdapter
    
		// AS 8/19/09 TFS17864
		#region SuppressBringIntoView
		internal static void SuppressBringIntoView(Type classType)
		{
			Debug.Assert(null != classType);
			EventManager.RegisterClassHandler(classType, FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(SuppressBringIntoViewImpl));
		}

		private static void SuppressBringIntoViewImpl(object sender, RequestBringIntoViewEventArgs e)
		{
			if (e.OriginalSource == sender && e.TargetRect.IsEmpty)
			{
				// previously focusing the cell area (which happens when the record
				// gets focus) would not have caused a scroll operation. to mimic 
				// that we will just eat a direct unbound request to bring the 
				// cell area into view.
				e.Handled = true;
				return;
			}
		} 
		#endregion //SuppressBringIntoView

        #region ParseFlaggedEnumFromString

        // SSP 11/13/09 TFS23002
		// 
		internal static T ParseFlaggedEnumFromString<T>( string enumValues, string seperator )
		{
			string[] arr = !string.IsNullOrEmpty( enumValues )
				? enumValues.Split( new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries )
				: null;

			long value = 0;

			if ( null != arr )
			{
				foreach ( string ii in arr )
				{
					try
					{
						T iiVal = (T)Enum.Parse( typeof( T ), ii );
						long iiLong = Convert.ToInt64( iiVal );
						value |= iiLong;
					}
					catch
					{
						Debug.Assert( false, string.Format( "Invalid enum value {0} encountered", ii ) );
					}
				}
			}

			return (T)Enum.ToObject( typeof( T ), value );
        }

        #endregion // ParseFlaggedEnumFromString

        #region RaiseKeyNotFound

        internal static void RaiseKeyNotFound( )
		{
			throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_14" ) );
		}

		#endregion // RaiseKeyNotFound

        #region RemoveAll<T>

        // SSP 6/14/10 TFS32985
        // 
        internal static T[] RemoveAll<T>( T[] arr, T val )
        {
            EqualityComparer<T> c = EqualityComparer<T>.Default;

            List<T> list = new List<T>( );
            for ( int i = 0; i < arr.Length; i++ )
            {
                T ii = arr[i];

                if ( ! c.Equals( ii, val ) )
                    list.Add( ii );
            }

            return list.ToArray( );
        }

        #endregion // RemoveAll<T>

		#region ReverseDictionary<K, V>

		// SSP 1/12/10 TFS25122
		// 
		public static Dictionary<V, K> ReverseDictionary<K, V>( Dictionary<K, V> map )
		{
			Dictionary<V, K> ret = new Dictionary<V, K>( );

			foreach ( KeyValuePair<K, V> ii in map )
				ret[ii.Value] = ii.Key;

			return ret;
		}

		#endregion // ReverseDictionary<K, V>

		#region SetValue

        internal static void SetValue( bool[] arr, int startIndex, int count, bool value )
		{
			for ( int i = 0; i < count; i++ )
				arr[startIndex + i] = value;
		}

		#endregion // SetValue

        // JJD 1/12/09 - NA 2009 vol 1 - record filtering
        #region SetIsTextSearchEnabled

        internal static bool SetIsTextSearchEnabled(XamComboEditor comboEditor, ComparisonOperator comparisonOperator)
        {
            if (comboEditor == null)
                return false;

            ComboBox cb = comboEditor.ComboBox;

            if (cb == null)
                return false;

            switch (comparisonOperator)
            {
                case ComparisonOperator.Equals:
                case ComparisonOperator.NotEquals:
                    cb.IsTextSearchEnabled = true;
                    break;
                default:
                    cb.IsTextSearchEnabled = false;
                    break;
            }

            return true;
        }

        #endregion //SetIsTextSearchEnabled	
    
		#region ShouldSerialize
		internal static bool ShouldSerialize(DependencyObject d)
		{
			LocalValueEnumerator enumerator = d.GetLocalValueEnumerator();

			while (enumerator.MoveNext())
			{
				LocalValueEntry entry = enumerator.Current;

				if (!Object.Equals(entry.Property.DefaultMetadata.DefaultValue, entry.Value))
					return true;
			}

			return false;
		} 
		#endregion //ShouldSerialize

		#region Swap

		internal static void Swap( object[] arr, int i, int j )
		{
			object tmp = arr[i];
			arr[i] = arr[j];
			arr[j] = tmp;
		}

		internal static void Swap( long[] arr, int i, int j )
		{
			long tmp = arr[i];
			arr[i] = arr[j];
			arr[j] = tmp;
		}

		// SSP 7/29/09 - NAS9.2 Enhanced grid view
		// 
		internal static void Swap( SparseArray arr, int i, int j )
		{
			object o = arr[i];
			arr[i] = arr[j];
			arr[j] = o;
		}

		#endregion // Swap

        #region SwapValues
        internal static void SwapValues(ref int x, ref int y)
        {
            int temp = x;
            x = y;
            y = temp;
        }
        #endregion //SwapValues

		#region ToArray<T>

		
		
		/// <summary>
		/// Combines all of the items in the specified enumerables into a single array.
		/// </summary>
		/// <typeparam name="T">Type of objects in the specified enumerables</typeparam>
		/// <param name="items">Items to combine into a single array</param>
		/// <returns>Returns the array</returns>
		public static T[] ToArray<T>( IEnumerable items )
		{
			List<T> list = new List<T>( );

			foreach ( T item in items )
				list.Add( item );

			return list.ToArray( );
		}

		/// <summary>
		/// Combines all of the items in the specified enumerables into a single array.
		/// </summary>
		/// <typeparam name="T">Type of objects in the specified enumerables</typeparam>
		/// <param name="enumerables">Items to combine into a single array</param>
		/// <returns>Returns the array</returns>
		public static T[] ToArray<T>( params IEnumerable<T>[] enumerables )
		{
			List<T> list = new List<T>( );

			foreach ( T item in new AggregateEnumerable<T>( enumerables ) )
				list.Add( item );

			return list.ToArray( );
		}

		public static T[] ToArray<T>(IList<T> list)
		{
			if (list == null)
				return new T[0];

			T[] array = new T[list.Count];
			list.CopyTo(array, 0);
			return array;
		}

		#endregion // ToArray<T>

		#region ToList<T>

		/// <summary>
		/// Returns the specified items as list.
		/// </summary>
		/// <typeparam name="T">Type of objects in the specified items enumerable</typeparam>
		/// <param name="items">Items to return as list</param>
		/// <param name="returnNullIfEmpty">If there are no items, then this parameter indicates
		/// whether to return null instead of an empty list.</param>
		/// <returns>List containing the items</returns>
		public static List<T> ToList<T>( IEnumerable<T> items, bool returnNullIfEmpty )
		{
			List<T> list = returnNullIfEmpty ? null : new List<T>( );

			if ( null != items )
			{
				foreach ( T item in items )
				{
					if ( null == list )
						list = new List<T>( );

					list.Add( item );
				}
			}

			return list;
		}

		/// <summary>
		/// Returns the specified items as list.
		/// </summary>
		/// <typeparam name="T">Type of objects in the specified items enumerable</typeparam>
		/// <param name="items">Items to return as list</param>
		/// <param name="returnNullIfEmpty">If there are no items, then this parameter indicates
		/// whether to return null instead of an empty list.</param>
		/// <returns>List containing the items</returns>
		public static List<T> ToList<T>( IEnumerable items, bool returnNullIfEmpty )
		{
			List<T> list = returnNullIfEmpty ? null : new List<T>( );

			if ( null != items )
			{
				foreach ( T item in items )
				{
					if ( null == list )
						list = new List<T>( );

					list.Add( item );
				}
			}

			return list;
		}

		#endregion // ToList<T>

		#region ToString

		/// <summary>
		/// Converts the specified object to string. If the object is null or its ToString implementation
		/// returns null or empty string, it will return the specified defaultString. Also it will return
		/// the specified defaultString if object's ToString implementation returns the type name of the 
		/// object (which is the what the default implementation of System.Object.ToString does).
		/// </summary>
		/// <param name="obj">Object to convert to a string.</param>
		/// <param name="defaultString">Default string.</param>
		/// <returns>Converted string.</returns>
		internal static string ToString( object obj, string defaultString )
		{
			return ToString( obj, defaultString, null );
		}

		// SSP 3/23/10 TFS29800
		// Added an overload that takes in formatProvider.
		// 
		/// <summary>
		/// Converts the specified object to string. If the object is null or its ToString implementation
		/// returns null or empty string, it will return the specified defaultString. Also it will return
		/// the specified defaultString if object's ToString implementation returns the type name of the 
		/// object (which is the what the default implementation of System.Object.ToString does).
		/// </summary>
		/// <param name="obj">Object to convert to a string.</param>
		/// <param name="defaultString">Default string.</param>
		/// <param name="formatProvider">Format provider.</param>
		/// <returns>Converted string.</returns>
		internal static string ToString( object obj, string defaultString, IFormatProvider formatProvider )
		{
			if ( null != obj )
			{
				string objStr;

				// SSP 3/23/10 TFS29800
				// If the object is IFormattable then use its ToString implementation. Added the 
				// if block and enclosed the existing code into the else block.
				// 
				IFormattable formattable = obj as IFormattable;
				if ( null != formattable )
					objStr = formattable.ToString( null, formatProvider );
				else
					objStr = obj.ToString( );

				if ( ! string.IsNullOrEmpty( objStr ) )
				{
					string typeName = obj.GetType( ).ToString( );
					if ( objStr != typeName )
						return objStr;
				}
			}

			return defaultString;
		}

		#endregion // ToString

		#region ToStringFromFlaggedEnum

		// SSP 11/13/09 TFS23002
		// 
		internal static string ToStringFromFlaggedEnum( Enum enumVal, string seperator ) 
		{
			StringBuilder sb = new StringBuilder( );
			Type type = enumVal.GetType( );
			long enumValLong = Convert.ToInt64( enumVal );

			foreach ( object ii in Enum.GetValues( type ) )
			{
				long iiLong = Convert.ToInt64( ii );

				if ( iiLong == ( iiLong & enumValLong ) )
				{
					string name = Enum.GetName( type, iiLong );
					if ( sb.Length > 0 )
						sb.Append( seperator );

					sb.Append( name );
				}
			}


			return sb.ToString( );
		}

		#endregion // ToStringFromFlaggedEnu

        #region Union
        internal static Rect Union(ref Rect rect1, ref Rect rect2)
        {
            Rect result = Rect.Union(rect1, rect2);

            if (GridUtilities.AreClose(rect1.X, rect2.X) &&
                GridUtilities.AreClose(rect1.Width, rect2.Width))
            {
                result.X = rect1.X;
                result.Width = rect1.Width;
            }

            if (GridUtilities.AreClose(rect1.Y, rect2.Y) &&
                GridUtilities.AreClose(rect1.Height, rect2.Height))
            {
                result.Y = rect1.Y;
                result.Height = rect1.Height;
            }

            return result;
        } 
        #endregion //Union

		#region ValidateEnum

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 

		private static bool IsEnumFlags( Type enumType )
		{
			object[] flagsAttributes = enumType.GetCustomAttributes( typeof( FlagsAttribute ), true );
			return null != flagsAttributes && flagsAttributes.Length > 0;
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static void ValidateEnum( Type enumType, object enumVal )
		{
			if ( !Enum.IsDefined( enumType, enumVal ) && ! IsEnumFlags( enumType ) )
				throw new InvalidEnumArgumentException( enumType.Name, (int)enumVal, enumType );
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static void ValidateEnum( string argumentName, Type enumType, object enumVal )
		{
			if ( !Enum.IsDefined( enumType, enumVal ) && ! IsEnumFlags( enumType ) )
				throw new InvalidEnumArgumentException( argumentName, (int)enumVal, enumType );
		}

		#endregion // ValidateEnum

		#region ValidateNonNegative

		public static void ValidateNonNegative( int val )
		{
			if ( val < 0 )
				throw new ArgumentOutOfRangeException( DataPresenterBase.GetString("LE_ValueCannotBeNegative", val) );
		}

		public static void ValidateNonNegative( double val )
		{
			if ( val < 0 )
				throw new ArgumentOutOfRangeException( DataPresenterBase.GetString("LE_ValueCannotBeNegative", val) );
		}

        public static bool ValidateNonNegativeInt(object newValue)
        {
            if (newValue is int)
            {
                if ((int)newValue < 0)
                    return false;
            }

            return true;
        }
		#endregion // ValidateNonNegative

		#region ValidateNotNull

		public static void ValidateNotNull( object value )
		{
			if ( null == value )
				throw new ArgumentNullException( );
		}

		public static void ValidateNotNull( object value, string paramName )
		{
			if ( null == value )
				throw new ArgumentNullException( paramName );
		}

		#endregion // ValidateNotNull

		#region ValidatePositive

		public static void ValidatePositive( int val )
		{
			if ( val <= 0 )
				throw new ArgumentOutOfRangeException( DataPresenterBase.GetString("LE_ValueMustBePositive", val) );
		}

		#endregion // ValidatePositive

		#endregion // Methods

		#region Properties

		#region AntirecursionUtility

		// SSP 3/24/10 TFS26271
		// 

		[ThreadStatic]
		private static AntirecursionUtility g_antirecursionUtility;

		internal static AntirecursionUtility Antirecursion
		{
			get
			{
				if ( null == g_antirecursionUtility )
					g_antirecursionUtility = new AntirecursionUtility( );

				return g_antirecursionUtility;
			}
		}

		#endregion // AntirecursionUtility

		// AS 8/26/09 CellContentAlignment
		// Similar to how we had to listen for changes to the Field's VisibilityResolved we also 
		// need to listen for CellContentAlignment changes. For a CVP collapse if LabelOnly, for 
		// a LP collapse if ValueOnly otherwise do nothing.
		//
		#region CellContentAlignment

		private static readonly object CellContentAlignmentLabelOnly = CellContentAlignment.LabelOnly;
		private static readonly object CellContentAlignmentValueOnly = CellContentAlignment.ValueOnly;
		private static readonly object CellContentAlignmentDefault = CellContentAlignment.Default;

		/// <summary>
		/// CellContentAlignment Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty CellContentAlignmentProperty =
			DependencyProperty.RegisterAttached("CellContentAlignment", typeof(CellContentAlignment), typeof(GridUtilities),
				new FrameworkPropertyMetadata(CellContentAlignmentDefault,
					new PropertyChangedCallback(OnCellContentAlignmentChanged),
					new CoerceValueCallback(OnCoerceCellContentAlignment)));

		public static CellContentAlignment GetCellContentAlignment(DependencyObject d)
		{
			return (CellContentAlignment)d.GetValue(CellContentAlignmentProperty);
		}

		private static object OnCoerceCellContentAlignment(DependencyObject d, object newValue)
		{
			CellContentAlignment alignment = (CellContentAlignment)newValue;

			switch (alignment)
			{
				case CellContentAlignment.ValueOnly:
					return CellContentAlignmentValueOnly;
				case CellContentAlignment.LabelOnly:
					return CellContentAlignmentLabelOnly;
				default:
					return CellContentAlignmentDefault;
			}
		}

		private static void OnCellContentAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(UIElement.VisibilityProperty);
		}

		#endregion //CellContentAlignment

		// AS 8/24/09 TFS19532
		#region FieldVisibility

		/// <summary>
		/// FieldVisibility Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty FieldVisibilityProperty =
			DependencyProperty.RegisterAttached("FieldVisibility", typeof(Visibility), typeof(GridUtilities),
				new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox,
					new PropertyChangedCallback(OnFieldVisibilityChanged),
					new CoerceValueCallback(OnCoerceFieldVisibility)));

		public static Visibility GetFieldVisibility(DependencyObject d)
		{
			return (Visibility)d.GetValue(FieldVisibilityProperty);
		}

		private static object OnCoerceFieldVisibility(DependencyObject d, object newValue)
		{
			return KnownBoxes.FromValue((Visibility)newValue);
		}

		private static void OnFieldVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(UIElement.VisibilityProperty);
		}

		#endregion //FieldVisibility

		#region StringCompareCulture

		internal static CultureInfo StringCompareCulture
		{
			get
			{
				return System.Globalization.CultureInfo.CurrentCulture;
			}
		}

		#endregion // StringCompareCulture

		#endregion // Properties

		#region Nested Classes/Interfaces

		#region AggregateComparer<T> Class

		
		
		public class AggregateComparer<T> : IComparer<T>, IComparer
		{
			private IComparer<T>[] _comparers;

			public AggregateComparer( params IComparer<T>[] comparers )
			{
				_comparers = comparers;
			}

			public int Compare( T x, T y )
			{
				for ( int i = 0; i < _comparers.Length; i++ )
				{
					IComparer<T> c = _comparers[i];
					
					int r = c.Compare( x, y );
					if ( 0 != r )
						return r;
				}

				return 0;
			}

			int IComparer.Compare( object x, object y )
			{
				return this.Compare( (T)x, (T)y );
			}
		}

		#endregion // AggregateComparer<T> Class

		#region AggregateEnumerable

		// AS 8/5/09 NA 2009.2
		// Changed to generic.
		//
		public class AggregateEnumerable<T> : IEnumerable<T>
		{
			#region Enumerator Class

			public class Enumerator : IEnumerator<T>
			{
				private IEnumerable<T>[] _enumerables;
				private int _index;
				private IEnumerator<T> _current;

				public Enumerator( params IEnumerable<T>[] enumerables )
				{
					_enumerables = enumerables;
					this.Reset( );
				}

				public void Reset( )
				{
					_index = -1;
					_current = null;
				}

				public bool MoveNext( )
				{
					if ( null != _current && _current.MoveNext( ) )
						return true;

					while ( null != _enumerables && ++_index < _enumerables.Length )
					{
						IEnumerable<T> ee = _enumerables[_index];
						if ( null != ee )
						{
							_current = ee.GetEnumerator( );
							_current.Reset( );
							if ( _current.MoveNext( ) )
								return true;
						}
					}

					return false;
				}

				public object Current
				{
					get
					{
						return null != _current ? _current.Current : default(T);
					}
				}

				#region IEnumerator<T> Members

				T IEnumerator<T>.Current
				{
					get { return null != _current ? _current.Current : default(T); ; }
				}

				#endregion

				#region IDisposable Members

				public void Dispose()
				{
				}

				#endregion
			}

			#endregion // Enumerator Class

			private IEnumerable<T>[] _enumerables;

			public AggregateEnumerable( params IEnumerable<T>[] enumerables )
			{
				_enumerables = enumerables;
			}

			public IEnumerator GetEnumerator( )
			{
				return new Enumerator( _enumerables );
			}

			#region IEnumerable<T> Members

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return new Enumerator(_enumerables);
			}

			#endregion
		}

		#endregion // AggregateEnumerable

		#region AntirecursionUtility Class

		// SSP 3/24/10 TFS26271
		// 

		internal class AntirecursionUtility
		{
			#region Data Structures

			#region ID Structures

			private struct ID
			{
				internal object _item;
				internal object _flagId;

				internal ID( object item, object flagId )
				{
					_item = item;
					_flagId = flagId;
				}

				internal static int GetHashCode( object o )
				{
					return null != o ? o.GetHashCode( ) : 0;
				}

				public override int GetHashCode( )
				{
					return GetHashCode( _item ) ^ GetHashCode( _flagId );
				}

				public override bool Equals( object obj )
				{
					if ( obj is ID )
					{
						ID id = (ID)obj;

						if ( id._item == _item && id._flagId == _flagId )
							return true;
					}

					return false;
				}
			}

			#endregion // ID Structures

			#endregion // Data Structures

			#region Member Vars

			private Dictionary<ID, int> _map;

			#endregion // Member Vars

			#region Map

			private Dictionary<ID, int> Map
			{
				get
				{
					if ( null == _map )
						_map = new Dictionary<ID, int>( );

					return _map;
				}
			}

			#endregion // Map

			#region Enter

			public bool Enter( object item, object flagId, bool disallowMultipleEntries )
			{
				ID id = new ID( item, flagId );

				int val;
				if ( this.Map.TryGetValue( id, out val ) )
				{
					if ( disallowMultipleEntries )
						return false;
				}
				else
				{
					val = 0;
				}

				this.Map[id] = 1 + val;

				return true;
			}

			#endregion // Enter

			#region Exit

			public void Exit( object item, object flagId )
			{
				ID id = new ID( item, flagId );

				Debug.Assert( null != _map && _map.ContainsKey( id ) );

				int val;
				if ( null != _map && _map.TryGetValue( id, out val ) )
				{
					val--;

					if ( val <= 0 )
						_map.Remove( id );
					else
						_map[id] = val;
				}
				else
				{
					Debug.Assert( false, "Exit called without accompanying Enter call." );
				}
			}

			#endregion // Exit

			#region InProgress

			public bool InProgress( object item, object flagId )
			{
				return null != _map && _map.ContainsKey( new ID( item, flagId ) );
			}

			#endregion // InProgress
		}

		#endregion // AntirecursionUtility Class

		#region ConverterEnumerable<TInput, TOutput> Class

		// SSP 2/11/10 - TFS26273
		// 
		public class ConverterEnumerable<TInput, TOutput> : IEnumerable<TOutput>
		{
			private IEnumerable<TInput> _e;
			private Converter<TInput, TOutput> _converter;

			public ConverterEnumerable( IEnumerable<TInput> e, Converter<TInput, TOutput> converter )
			{
				_e = e;
				_converter = converter;
			}

			#region IEnumerable<TOutput> Members

			public IEnumerator<TOutput> GetEnumerator( )
			{
				return new ConverterEnumerator<TInput, TOutput>( _e.GetEnumerator( ), _converter );
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator( )
			{
				return this.GetEnumerator( );
			}

			#endregion
		}

		#endregion // ConverterEnumerable<TInput, TOutput> Class

		#region ConverterEnumerator<TInput, TOutput> Class

		// SSP 1/21/09 - NAS9.1 Record Filtering
		// 
		public class ConverterEnumerator<TInput, TOutput> : IEnumerator<TOutput>
		{
			private IEnumerator<TInput> _e;
			private Converter<TInput, TOutput> _converter;

			public ConverterEnumerator( IEnumerator<TInput> e, Converter<TInput, TOutput> converter )
			{
				_e = e;
				_converter = converter;
			}

			public void Reset( )
			{
				_e.Reset( );
			}

			public bool MoveNext( )
			{
				return _e.MoveNext( );
			}

			public TOutput Current
			{
				get
				{
					return _converter.Invoke( _e.Current );
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			public void Dispose( )
			{
			}
		}

		#endregion // ConverterEnumerator<TInput, TOutput> Class

		#region EmptyEnumerable<T>

		public class EmptyEnumerable<T> : IEnumerable<T>
		{
			public class Enumerator : IEnumerator<T>
			{
				public Enumerator( )
				{
				}

				#region IEnumerator<T> Members

				public T Current
				{
					get { throw new InvalidOperationException( ); }
				}

				#endregion

				#region IDisposable Members

				public void Dispose( )
				{
				}

				#endregion

				#region IEnumerator Members

				object IEnumerator.Current
				{
					get { throw new InvalidOperationException( ); }
				}

				public bool MoveNext( )
				{
					return false;
				}

				public void Reset( )
				{
				}

				#endregion
			}

			public EmptyEnumerable( )
			{
			}

			#region IEnumerable<T> Members

			public IEnumerator<T> GetEnumerator( )
			{
				return new Enumerator( );
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator( )
			{
				return this.GetEnumerator( );
			}

			#endregion
		}

		#endregion // EmptyEnumerable<T>

		#region EnumeratorCollection<T> Class

		// SSP 2/11/10 - TFS26273
		// 
		public class EnumeratorCollection<T> : ICollection<T>
		{
			private IEnumerable<T> _enumerable;
			private int _count;

			public EnumeratorCollection( IEnumerable<T> enumerable, int count )
			{
				_enumerable = enumerable;
				_count = count;
			}

			#region ICollection<T> Members

			public void Add( T item )
			{
				throw new NotSupportedException( );
			}

			public void Clear( )
			{
				throw new NotSupportedException( );
			}

			public bool Contains( T item )
			{
				throw new NotSupportedException( );
			}

			public void CopyTo( T[] array, int arrayIndex )
			{
				GridUtilities.CopyTo<T>( _enumerable, array, arrayIndex );
			}

			public int Count
			{
				get 
				{
					return _count;
				}
			}

			public bool IsReadOnly
			{
				get 
				{
					return true;
				}
			}

			public bool Remove( T item )
			{
				throw new NotSupportedException( );
			}

			#endregion

			#region IEnumerable<T> Members

			public IEnumerator<T> GetEnumerator( )
			{
				return _enumerable.GetEnumerator( );
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator( )
			{
				return _enumerable.GetEnumerator( );
			}

			#endregion
		}

		#endregion // EnumeratorCollection<T> Class

		#region IMeetsCriteria Interface

		internal interface IMeetsCriteria
		{
			bool MeetsCriteria( object item );
		}

		#endregion // IMeetsCriteria Interface

		// JJD 4/21/11 - TFS73048 - Optimization - added
		#region InvalidateMeasureAsynch
		
		internal static void InvalidateMeasureAsynch(UIElement element)
		{
			if (element == null || !element.IsMeasureValid)
				return;

			// JJD 2/16/12 - TFS101387
			// See if the element is still being used (i.e. if IsVsible is true or the DataContext is not null).
			// If not then just bail.
			// This fixes a memory leak by preventing presenters for old TemplateDataRecords from being rooted.
			if (!element.IsVisible)
			{
				if (element.GetValue(FrameworkElement.DataContextProperty) == null)
					return;
			}

			// JJD 10/21/11 - TFS86028
			// If we are processing invalidations synchronously the
			// invalidate the measure and arrange and return
			if (DataPresenterBase.ProcessCellInvalidationsSynchronously)
			{
				element.InvalidateMeasure();
				element.InvalidateArrange();
				return;
			}

			if (_QueuedMeasureInvalidations == null)
				_QueuedMeasureInvalidations = new HashSet();

			_QueuedMeasureInvalidations.Add(element);

			// on the first one added do a beginInvoke
			if (_QueuedMeasureInvalidations.Count == 1)
				// JM 05-11-2011 TFS74828, TFS74575
				//element.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MethodDelegate(ProcessAsyncInvalidations));
				element.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new MethodDelegate(ProcessAsyncInvalidations));
		}

		private static void ProcessAsyncInvalidations()
		{
			// JJD 6/20/11 - TFS78929
			// Moved logic into helper method 
			ProcessAsyncInvalidationsImpl();

			// JJD 6/20/11 - TFS78929
			// Retry to process the _QueuedMeasureInvalidations until
			// there are no more or we have retried 9 times.
			// This is a fail safe to prevent locking the ui in certain situations.
			int retryCount = 0;
			while (_QueuedMeasureInvalidations.Count > 0)
			{
				if (retryCount < 9)
				{
					retryCount++;
					ProcessAsyncInvalidationsImpl();
				}
				else
				{
					_QueuedMeasureInvalidations.Clear();
					Debug.Fail("ProcessAsyncInvalidations exceeded re-try limit");
					break;
				}
			}
		}

		// JJD 6/20/11 - TFS78929 - added
		private static void ProcessAsyncInvalidationsImpl()
		{
			int count = _QueuedMeasureInvalidations.Count;

			// JM 05-11-2011 TFS74828, TFS74575
			UIElement uie = null;

			foreach (UIElement element in _QueuedMeasureInvalidations)
			{
				// JM 05-11-2011 TFS74828, TFS74575
				uie = element;

				// wrap in try catch in case the element is in a bad state
				try
				{
					element.InvalidateMeasure();
					element.InvalidateArrange();
				}
				catch (Exception e)
				{
					Debug.Fail("ProcessAsyncInvalidations exception: " + e.ToString());
				}
			}

			// clear the thread static queue
			_QueuedMeasureInvalidations.Clear();

			// JM 05-11-2011 TFS74828, TFS74575
			if (null != uie)
				uie.UpdateLayout();
		}

		#endregion //InvalidateMeasureAsynch	
    
		#region InvalidateStylesAsyncInfo

		// SSP 9/2/09 TFS17893
		// Added InvalidateStylesAsyncInfo which is used by the FieldLayout's 
		// InvalidateGeneratedStylesAsnyc to asynchronously invalidate generated 
		// styles.
		// 
		internal class InvalidateStylesAsyncInfo
		{
			internal bool _bumpVersion, _regenerateTemplates;
			internal DispatcherOperation _dispatcherOperation;

			// SSP 2/4/10 TFS25283
			// 
			internal bool _bumpSpecialRecordsVersion;
		}

		#endregion // InvalidateStylesAsyncInfo

		#region MeetsCriteria_RecordFieldLayout

		internal class MeetsCriteria_RecordFieldLayout : IMeetsCriteria
		{
			private FieldLayout _fieldLayout;

			internal MeetsCriteria_RecordFieldLayout( FieldLayout fieldLayout )
			{
				_fieldLayout = fieldLayout;
			}

			public bool MeetsCriteria( object item )
			{
				Record record = (Record)item;
				return _fieldLayout == record.FieldLayout;
			}
		}

		#endregion // MeetsCriteria_RecordFieldLayout

		#region MeetsCriteria_RecordFilteredOut

		// SSP 1/5/09 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Filters out records that are filtered out. In other words, returns records that pass the filter.
		/// </summary>
		internal class MeetsCriteria_FilteredInRecords : IMeetsCriteria
		{
			public bool MeetsCriteria( object item )
			{
				return ! ((Record)item).InternalIsFilteredOut_Verify;
			}
		}

		#endregion // MeetsCriteria_RecordFilteredOut

		#region MeetsCriteria_RecordType

		internal class MeetsCriteria_RecordType : IMeetsCriteria
		{
			private RecordType _recordType;

			internal MeetsCriteria_RecordType( RecordType recordType )
			{
				_recordType = recordType;
			}

			public bool MeetsCriteria( object item )
			{
				Record record = (Record)item;
				return _recordType == record.RecordType;
			}
		}

		#endregion // MeetsCriteria_RecordType

		#region MeetsCriteria_RecordVisible

		internal class MeetsCriteria_RecordVisible : IMeetsCriteria
		{
			private bool _ignoreFilteringLogic;

			/// <summary>
			/// Constructor. Uses VisibilityResolved property to filter out non-visible records.
			/// </summary>
			internal MeetsCriteria_RecordVisible( )
				: this( false )
			{
			}

			/// <summary>
			/// Constructor. If ignoreFilteringLogic is true then uses VisibilityResolved_NoFiltering property,
			/// otherwise uses the VisibilityResolved property to filter out records.
			/// </summary>
			/// <param name="ignoreFilteringLogic">If is true then uses VisibilityResolved_NoFiltering property,
			/// otherwise uses the VisibilityResolved property to filter out records.</param>
			internal MeetsCriteria_RecordVisible( bool ignoreFilteringLogic )
			{
				_ignoreFilteringLogic = ignoreFilteringLogic;
			}

			public bool MeetsCriteria( object item )
			{
				Record record = (Record)item;

				Visibility v = _ignoreFilteringLogic
					? record.VisibilityResolved_NoFiltering
					: record.VisibilityResolved;

				return Visibility.Visible == v;
			}
		}

		#endregion // MeetsCriteria_RecordVisible

		#region MeetsCriteria_Type

		internal class MeetsCriteria_Type : IMeetsCriteria
		{
			private Type _type;
			private bool _allowSubtypes;

			internal MeetsCriteria_Type( Type type, bool allowSubtypes )
			{
				_type = type;
				_allowSubtypes = allowSubtypes;
			}

			public bool MeetsCriteria( object item )
			{
				return _allowSubtypes
					? GridUtilities.IsObjectOfType( item, _type )
					: null != item && _type == item.GetType( );
			}
		}

		#endregion // MeetsCriteria_Type

		// AS 8/7/09 NA 2009.2 Field Sizing
		#region MeetsCriteria_Predicate<T>
		internal class MeetsCriteria_Predicate<T> : IMeetsCriteria
		{
			private Predicate<T> _predicate;

			internal MeetsCriteria_Predicate(Predicate<T> predicate)
			{
				Debug.Assert(null != predicate);
				_predicate = predicate;
			}

			public bool MeetsCriteria(object item)
			{
				if (item is T == false)
					return false;

				return _predicate((T)item);
			}
		} 
		#endregion //MeetsCriteria_Predicate<T>

		#region MeetsCriteriaChain Class

		internal class MeetsCriteriaChain : IMeetsCriteria
		{
			private IMeetsCriteria[] _arr;
			private bool _combineUsingOr;

			internal MeetsCriteriaChain( IMeetsCriteria x, IMeetsCriteria y, bool combineUsingOr )
				: this( new IMeetsCriteria[] { x, y }, combineUsingOr )
			{
			}

			internal MeetsCriteriaChain( IMeetsCriteria[] arr, bool combineUsingOr )
			{
				_arr = arr;
				_combineUsingOr = combineUsingOr;
			}

			public bool MeetsCriteria( object item )
			{
				for ( int i = 0; i < _arr.Length; i++ )
				{
					bool r = _arr[i].MeetsCriteria( item );

					if ( _combineUsingOr )
					{
						if ( r )
							return true;
					}
					else
					{
						if ( !r )
							return false;
					}
				}

				return !_combineUsingOr;
			}
		}

		#endregion // MeetsCriteriaChain Class

		#region MeetsCriteriaComplement Class

		internal class MeetsCriteriaComplement : IMeetsCriteria
		{
			private IMeetsCriteria _i;

			internal MeetsCriteriaComplement( IMeetsCriteria i )
			{
				_i = i;
			}

			public bool MeetsCriteria( object item )
			{
				return !_i.MeetsCriteria( item );
			}
		}

		#endregion // MeetsCriteriaComplement Class		

		#region MeetsCriteriaEnumerator<T> Class

		internal class MeetsCriteriaEnumerator<T> : IEnumerator<T>
		{
			#region Enumerable Class

			internal class Enumerable : IEnumerable<T>
			{
				private IEnumerable<T> _e;
				private IMeetsCriteria _i;

				internal Enumerable( IEnumerable<T> e, IMeetsCriteria i )
				{
					// AS 8/25/09 TFS17560
					// When we created the template group by record I hit a scenario where
					// this was null.
					//
					//_e = e;
					_e = e ?? new T[0];
					_i = i;
				}

				internal Enumerable( IEnumerable<T> e, IMeetsCriteria[] criteria, bool combineUsingOr )
					: this( e, new MeetsCriteriaChain( criteria, combineUsingOr ) )
				{
				}

				public IEnumerator<T> GetEnumerator( )
				{
					return new MeetsCriteriaEnumerator<T>( _e.GetEnumerator( ), _i );
				}

				IEnumerator IEnumerable.GetEnumerator( )
				{
					return this.GetEnumerator( );
				}
			}

			#endregion // Enumerable Class

			private IEnumerator<T> _enumerator;
			private IMeetsCriteria _meetsCriteriaCallback;
			private T _currentItem;

			internal MeetsCriteriaEnumerator( IEnumerator<T> enumerator, IMeetsCriteria meetsCriteriaCallback )
			{
				_enumerator = enumerator;
				_meetsCriteriaCallback = meetsCriteriaCallback;
			}

			public void Reset( )
			{
				_enumerator.Reset( );
				_currentItem = default( T );
			}

			public bool MoveNext( )
			{
				do
				{
					if ( !_enumerator.MoveNext( ) )
					{
						// SSP 5/23/11 TFS74231
						// 
						_currentItem = default( T );

						return false;
					}

					_currentItem = _enumerator.Current;
				}
				while ( !_meetsCriteriaCallback.MeetsCriteria( _currentItem ) );

				return true;
			}

			public T Current
			{
				get
				{
					return _currentItem;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return _currentItem;
				}
			}

			public void Dispose( )
			{
				// SSP 5/23/11 TFS74231
				// 
				this.Reset( );
			}
		}

		#endregion // MeetsCriteriaEnumerator<T> Class

		#region MeetsCriteriaExcludeSingleItem Class

		internal class MeetsCriteriaExcludeSingleItem : IMeetsCriteria
		{
			private object _item;

			internal MeetsCriteriaExcludeSingleItem( object item )
			{
				_item = item;
			}

			public bool MeetsCriteria( object o )
			{
				return _item != o;
			}
		}

		#endregion // MeetsCriteriaExcludeSingleItem Class

        #region MethodDelegate

        // AS 1/27/09
        // Optimization - only have 1 parameterless void delegate class defined.
        //
        internal delegate void MethodDelegate(); 

        #endregion //MethodDelegate

		#region NotifyCollectionEnumerable<T> Class

		internal class NotifyCollectionEnumerable<T> : IEnumerable<T>, INotifyCollectionChanged
		{
			private IEnumerable<T> _sourceItems;
			private GridUtilities.IMeetsCriteria _filter;
			private object _tag;

			public NotifyCollectionEnumerable( IEnumerable<T> sourceItems )
				: this( sourceItems, null )
			{
			}

			public NotifyCollectionEnumerable( IEnumerable<T> sourceItems, GridUtilities.IMeetsCriteria filter )
			{
				_sourceItems = sourceItems;
				_filter = filter;
			}

			public object Tag
			{
				get
				{
					return _tag;
				}
				set
				{
					_tag = value;
				}
			}

			public IEnumerable<T> SourceItems
			{
				get
				{
					return _sourceItems;
				}
				set
				{
					if ( _sourceItems != value )
					{
						_sourceItems = value;
						this.RaiseCollectionChanged( );
					}
				}
			}

			public void RaiseCollectionChanged( )
			{
				if ( null != this.CollectionChanged )
					this.CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
			}

			#region INotifyCollectionChanged Members

			public event NotifyCollectionChangedEventHandler CollectionChanged;

			#endregion

			#region IEnumerable<T> Members

			public IEnumerator<T> GetEnumerator( )
			{
				// JM 06-23-11 TFS79500.  Check for null _sourceItems and return an enumerator for an empty array of T.
				if (null == _sourceItems)
					return ((IEnumerable<T>)(new T[0])).GetEnumerator();

				return null != _filter
					? new MeetsCriteriaEnumerator<T>( _sourceItems.GetEnumerator( ), _filter )
					: _sourceItems.GetEnumerator( );
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator( )
			{
				return this.GetEnumerator( );
			}

			#endregion
		}

		#endregion // NotifyCollectionEnumerable<T> Class

		#region Range Struct

		// SSP 8/2/09 - Summary Recalc Optimizations
		// 
		public struct Range
		{
			private int _start, _end;

			public Range( int start, int end )
			{
				_start = start;
				_end = end;
			}

			public int Start
			{
				get
				{
					return _start;
				}
				set
				{
					_start = value;
				}
			}

			public int End
			{
				get
				{
					return _end;
				}
				set
				{
					_end = value;
				}
			}
		}

		#endregion // Range Struct

		// JJD 1/20/09 - NA 2009 vol 1 
        #region RecordContentMarginConverter
        // This converter is used for prefix areas other than the summaryprefix area 
        // to adjust the extent of the prrefix to account for groupyby record indents
        internal class RecordContentMarginConverter : IMultiValueConverter
        {
            #region Member Variables

            private static readonly object PositiveInfinity = double.PositiveInfinity;
            private static readonly RecordContentMarginConverter instance;

            #endregion //Member Variables

            #region Constructor
            static RecordContentMarginConverter()
            {
                instance = new RecordContentMarginConverter();
            }

            private RecordContentMarginConverter()
            {
            }
            #endregion //Constructor

            #region Properties
            internal static IMultiValueConverter Instance
            {
                get { return RecordContentMarginConverter.instance; }
            }

            // JJD 8/26/09 - NA 2009 Vol 2 - Enhanced grid view
            
            // Added attached version number proerty that can be used to trigger a re-conversion
            #region Version

            internal static readonly DependencyProperty VersionProperty = DependencyProperty.RegisterAttached("Version",
                typeof(int), typeof(RecordContentMarginConverter), new FrameworkPropertyMetadata(0));

            internal static void BumpVersion(DependencyObject d)
            {
                int oldValue = (int)d.GetValue(RecordContentMarginConverter.VersionProperty);
                d.SetValue(RecordContentMarginConverter.VersionProperty, oldValue + 1);
            }

            #endregion //Version
	
            #endregion //Properties

            #region IMultiValueConverter Members

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                Thickness margin = new Thickness();
                object[] args = parameter as object[];

                bool isHeader   = (bool)args[0];
                Type elementType = (Type)args[1];

                FrameworkElement fe = values[0] as FrameworkElement;

                Debug.Assert(fe != null);

                if (fe == null)
                    return margin;

                RecordPresenter rp = fe as RecordPresenter;

                if (rp == null)
                {
                    rp = Utilities.GetAncestorFromType(fe, typeof(RecordPresenter), true) as RecordPresenter;

                    if (rp == null)
                        return margin;
                }

                // we don't mess the the margin of the Summary record except for its header
                // because the SummaryRecordPresenter exposes its own ContentAreaMargins
                if (isHeader == false && 
                    rp is SummaryRecordPresenter &&
                    elementType != typeof(SummaryRecordPresenter))
                    return margin;

                Record record = rp.Record;

                if (record == null)
                    return margin;

                RecordListControl rlc = Utilities.GetAncestorFromType(rp, typeof(RecordListControl), true) as RecordListControl;

                //Debug.Assert(rlc != null);

                if (rlc == null)
                    return margin;

                HeaderRecord hr = record as HeaderRecord;

                ViewableRecordCollection vrc = rlc.ItemsSource as ViewableRecordCollection;

                // JJD 7/31/09 - NA 2009 Vol 2 - Enhanced grid view
                // See if we are bound to a Flat list 
                if (vrc == null)
                {
                    FlatScrollRecordsCollection flatList = rlc.ItemsSource as FlatScrollRecordsCollection;

                    if (flatList != null)
                    {
                        Record rcdTemp = null;

                        // If this is a HedaerRecord then use its attached to record to
                        // get the associated viewable records collection
                        if (hr != null)
                        {
                            // JJD 11/10/09 - TFS24243
                            // use the RecordForLayoutCalculations instead
                            //rcdTemp = hr.AttachedToRecord;
                            rcdTemp = hr.RecordForLayoutCalculations;
                        }

                        if (rcdTemp == null)
                            rcdTemp = record;

                        vrc = rcdTemp.ParentCollection.ViewableRecords;
                    }
                }

                if (vrc == null)
                {
                    // JJD 6/4/09 - TFS17060
                    // When we are printing we are binding to something other that a ViewableRecordCollection
                    // In that case we want to let thru only GroupByRecordPresenters
					if (!(rp is GroupByRecordPresenter))
					{
						// JJD 4/14/11 - TFS70716
						// Only return if we have a vrc (e.g. inside a report) or if this
						// is not a SummaryRecordPresenter
						if ( vrc != null || !(rp is SummaryRecordPresenter))
							return margin;
					}
                }


                // JJD 2/3/09
                // If this is a summary record presenter or a GroupBySummariesPresenter
                // then don't bother checking if the colleciton is a GroupBy collection
                // JJD 8/26/09 - NA 2009 Vol 2 - Enhanced grid view
                
                // Don't do this mapping for header records
                //if (vrc != null && !(rp is SummaryRecordPresenter && !(fe is GroupBySummariesPresenter)))
                if (vrc != null && hr == null && !(rp is SummaryRecordPresenter && !(fe is GroupBySummariesPresenter)))
                {
                    switch (vrc.RecordCollection.RecordsType)
                    {
                        case RecordType.GroupByFieldLayout:
                        case RecordType.GroupByField:
                            {
                                // for groupby rcds we want to use the first record in the colleciton
                                int count = vrc.Count;

                                if (count > 0)
                                    record = vrc[0];

                                break;
                            }
                    }
                }

                FieldLayout fl = record.FieldLayout;

                if (fl == null)
                    return margin;

                double nearOffset = record.CalculateExtraOffsetInGroupBy(isHeader);

                // JJD 2/3/09
                // Since GroupBySummariesPresenters don't have a prefix area
                // we need to adjust for it here
                if (fe is GroupBySummariesPresenter)
                {
                    switch (fl.RecordSelectorLocationResolved)
                    {
                        case RecordSelectorLocation.LeftOfCellArea:
                            if (fl.IsHorizontal == false)
                            {
                                nearOffset += fl.RecordSelectorExtentResolved;
                            }
                            break;
                    }
                }

                if (nearOffset > 0)
                {
                    if (fl.IsHorizontal)
                        margin.Top = nearOffset;
                    else
                        margin.Left = nearOffset;
                }

                return margin;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                return new object[2] { Binding.DoNothing, Binding.DoNothing };
            }

            #endregion //IMultiValueConverter Members
        }
        #endregion //RecordContentMarginConverter

		#endregion // Nested Classes/Interfaces
	}

	#endregion // GridUtilities Class

	#region RecursiveRecordsEnumerator Class

	/// <summary>
	/// An enumerator calss for recursively enumerating records.
	/// </summary>
	internal class RecursiveRecordsEnumerator : IEnumerator<Record>
	{
		#region Enumerable Class

		internal class Enumerable : IEnumerable<Record>
		{
			private RecordCollectionBase _records;
			private FieldLayout _lowestLevelFieldLayout;
			private bool _visibleRecordsOnly;

			internal Enumerable( RecordCollectionBase records,
					FieldLayout lowestLevelFieldLayout,
					bool visibleRecordsOnly )
			{
				GridUtilities.ValidateNotNull( records );

				_records = records;
				_lowestLevelFieldLayout = lowestLevelFieldLayout;
				_visibleRecordsOnly = visibleRecordsOnly;
			}

			public IEnumerator<Record> GetEnumerator( )
			{
				return new RecursiveRecordsEnumerator( _records, _lowestLevelFieldLayout, _visibleRecordsOnly );
			}

			IEnumerator IEnumerable.GetEnumerator( )
			{
				return this.GetEnumerator( );
			}
		}

		#endregion // Enumerable Class

		#region FlatRecordsEnumerator Class

		internal class FlatRecordsEnumerator : IEnumerator<Record>
		{
			private RecordCollectionBase _records;
			private Record _current;
			private bool _visibleOnly;

			private IEnumerator _child;

			internal FlatRecordsEnumerator( RecordCollectionBase records, bool visibleOnly )
			{
				GridUtilities.ValidateNotNull( records );

				_records = records;
				_visibleOnly = visibleOnly;

				if ( visibleOnly )
				{
					
					
					
					
					_child = new GridUtilities.MeetsCriteriaEnumerator<Record>(
									( (IEnumerable<Record>)_records ).GetEnumerator( ),
									new GridUtilities.MeetsCriteria_RecordVisible( ) );
					
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

				}
				else
				{
					_child = ( (IEnumerable)_records ).GetEnumerator( );
				}

				this.Reset( );
			}

			public void Reset( )
			{
				_current = null;
				_child.Reset( );				
			}

			public bool MoveNext( )
			{
				if ( _child.MoveNext( ) )
				{
					_current = (Record)_child.Current;
					Debug.Assert( null != _current );
					return true;
				}
				
				_current = null;
				return false;
			}

			public Record Current
			{
				get
				{
					return _current;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return _current;
				}
			}

			public void Dispose( )
			{
			}
		}

		#endregion // FlatRecordsEnumerator Class

		#region Private Vars

		private RecordCollectionBase _records = null;
		private IEnumerator<Record> _recordsEnumerator = null;		

		private FieldLayout _lowestLevelFieldLayout = null;
		private RecursiveRecordsEnumerator _child = null;
		private Record _currentRecord = null;
		private bool _visibleRecordsOnly;

		#endregion // Private Vars

		#region Constructor

		internal RecursiveRecordsEnumerator( 
			RecordCollectionBase records, 
			FieldLayout lowestLevelFieldLayout, 
			bool visibleRecordsOnly )
		{
			GridUtilities.ValidateNotNull( records );

			_records = records;
			_lowestLevelFieldLayout = lowestLevelFieldLayout;
			_visibleRecordsOnly = visibleRecordsOnly;
			this.Reset( );
		}

		#endregion // Constructor

		#region Reset

		public void Reset( )
		{
			_recordsEnumerator = new FlatRecordsEnumerator( _records, _visibleRecordsOnly );				
			_child = null;
			_currentRecord = null;
		}

		#endregion // Reset

		#region MoveNext

		public bool MoveNext( )
		{
			do
			{
				// If there's a child enumerator, exhaust that first.
				if ( null != _child && _child.MoveNext( ) )
				{
					_currentRecord = _child.Current;
					return true;
				}

				// Traverse down current record's descendants.
				if ( null != _currentRecord )
				{
                    // SSP 6/15/10 - Optimizations
                    // 
                    // ----------------------------------------------------------------------------------
                    RecordCollectionBase currentRecordChildren = null;
                    bool traverseChildren = false;

                    if ( null == _lowestLevelFieldLayout
                        || _lowestLevelFieldLayout != _currentRecord.FieldLayout
                        // If lowest level field layout matches the current record's field layout
                        // then stop traversing further down to children unless children belong
                        // to the same record manager to account for group-by situation.
                        || _currentRecord is GroupByRecord
						// JJD 09/22/11  - TFS84708 - Optimization - Use ChildRecordsIfNeeded instead
						// && null != ( currentRecordChildren = _currentRecord.ChildRecordsInternal )
                           && null != ( currentRecordChildren = _currentRecord.ChildRecordsIfNeeded )
                           && _lowestLevelFieldLayout == currentRecordChildren.FieldLayout
                        )
                    {
                        traverseChildren = true;
                        if ( null == currentRecordChildren )
                        {
                            // If the record is a data record then check it's HasChildrenInternal before
                            // accessing its ChildRecordsInternal property otherwise the data record will
                            // end up allocating child records collection even if it doesn't have any 
                            // child expandable fields.
                            // 
                            DataRecord dr = _currentRecord as DataRecord;
                            if ( null != dr )
                            {
                                currentRecordChildren = dr.ChildRecordsIfAllocated;
                                if ( null == currentRecordChildren && dr.HasChildrenInternal )
                                    currentRecordChildren = dr.ChildRecordsInternal;
                            }
                            else
                            {
								// JJD 09/22/11  - TFS84708 - Optimization - Use ChildRecordsIfNeeded instead
								//currentRecordChildren = _currentRecord.ChildRecordsInternal;
                                currentRecordChildren = _currentRecord.ChildRecordsIfNeeded;
                            }
                        }
                    }

                    if ( traverseChildren && null != currentRecordChildren )
                    {
                        _child = new RecursiveRecordsEnumerator(
                            currentRecordChildren,
                            _lowestLevelFieldLayout,
                            _visibleRecordsOnly );

                        // Set _currentRecord to null so we don't traverse down its descendants again.
                        _currentRecord = null;
                        continue;
                    }
                    
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

                    // ----------------------------------------------------------------------------------
				}

				if ( _recordsEnumerator.MoveNext( ) )
				{
					// Get the next record from the root records.
					_currentRecord = _recordsEnumerator.Current;
					_child = null;
				}
				else
				{
					// Once the root records have been exhausted, break out of the enumerator.
					_currentRecord = null;
					break;
				}

			} while ( null == _currentRecord );

			return null != _currentRecord;
		}

		#endregion // MoveNext

		#region Current

		public Record Current
		{
			get
			{
				return _currentRecord;
			}
		}

		#endregion // Current

		#region IEnumerator.Current

		object IEnumerator.Current
		{
			get
			{
				return _currentRecord;
			}
		}

		#endregion // IEnumerator.Current

		#region Dispose

		public void Dispose( )
		{
		}

		#endregion // Dispose
	}

	#endregion // RecursiveRecordsEnumerator Class

	#region SparseList Class

	/// <summary>
	/// Sprase list implementation that uses hash table to store owner data.
	/// </summary>
	internal class SparseList : IList
	{
		#region Nested Structures/Classes

		#region HashSparseArray Class

		private class HashSparseArray : SparseArray
		{
			#region Private Vars

			private SparseList _owner;

			#endregion // Private Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="owner">Owning sparse list</param>
			internal HashSparseArray( SparseList owner ) : base( 20, 1f, true )
			{
				GridUtilities.ValidateNotNull( owner );
				_owner = owner;
			}

			#endregion // Constructor

			#region GetOwnerData

			protected override object GetOwnerData( object item )
			{
				return _owner._ownerData[item];
			}

			#endregion // GetOwnerData

			#region SetOwnerData

			protected override void SetOwnerData( object item, object ownerData )
			{
				if ( null == ownerData )
					_owner._ownerData.Remove( item );
				else
					_owner._ownerData[item] = ownerData;
			}

			#endregion // SetOwnerData
		}

		#endregion // HashSparseArray Class

		#endregion // Nested Structures/Classes

		#region Private Vars

		private HashSparseArray _sparseArray;
		private Hashtable _ownerData;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SparseList"/>.
		/// </summary>
		public SparseList( )
		{
			_ownerData = new Hashtable( );
			_sparseArray = new HashSparseArray( this );
		}

		#endregion // Constructor

		#region Indexer

		public object this[int index]
		{
			get
			{
				return _sparseArray[index];
			}
			set
			{
				_sparseArray[index] = value;
			}
		}

		#endregion // Indexer

		#region Methods

		#region Public Methods

		#region Add

		public int Add( object value )
		{
			return _sparseArray.Add( value );
		}

		#endregion // Add

		#region AddRange

		internal void AddRange( ICollection items )
		{
			_sparseArray.AddRange( items );
		}

		#endregion // AddRange

		#region Clear

		public void Clear( )
		{
			_sparseArray.Clear( );
			_ownerData.Clear( );
		}

		#endregion // Clear

		#region Contains

		public bool Contains( object value )
		{
			return _sparseArray.Contains( value );
		}

		#endregion // Contains

		#region CopyTo

		public void CopyTo( Array array, int index )
		{
			_sparseArray.CopyTo( array, index );
		}

		#endregion // CopyTo

		#region GetEnumerator

		public IEnumerator GetEnumerator( )
		{
			return _sparseArray.GetEnumerator( );
		}

		#endregion // GetEnumerator

		#region IndexOf

		public int IndexOf( object value )
		{
			return _sparseArray.IndexOf( value );
		}

		#endregion // IndexOf

		#region Insert

		public void Insert( int index, object value )
		{
			_sparseArray.Insert( index, value );
		}

		#endregion // Insert

		#region Remove

		public void Remove( object value )
		{
			_sparseArray.Remove( value );
			_ownerData.Remove( value );
		}

		#endregion // Remove

		#region RemoveAt

		public void RemoveAt( int index )
		{
			_sparseArray.RemoveAt( index );
		}

		#endregion // RemoveAt

		#endregion // Public Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		public int Count
		{
			get { return _sparseArray.Count; }
		}

		#endregion // Count

		#region IsFixedSize

		public bool IsFixedSize
		{
			get { return false; }
		}

		#endregion // IsFixedSize

		#region IsReadOnly

		public bool IsReadOnly
		{
			get { return false; }
		}

		#endregion // IsReadOnly

		#region IsSynchronized

		public bool IsSynchronized
		{
			get { return _sparseArray.IsSynchronized; }
		}

		#endregion // IsSynchronized

		#region SyncRoot

		public object SyncRoot
		{
			get { return _sparseArray.SyncRoot; }
		}

		#endregion // SyncRoot

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // SparseList Class

	#region SummaryStringFormatsParser Class

	/// <summary>
	/// Parser for parsing strings like "sum: TOTAL={0:c}, average: AVG={0:c}, count: COUNT={0}".
	/// </summary>
	internal class SummaryStringFormatsParser
	{
		private Dictionary<string, string> _calcFormats;

		internal static Dictionary<string, string> Parse( string calcStringFormats )
		{
			SummaryStringFormatsParser parser = new SummaryStringFormatsParser( );
			parser.ParseHelper( calcStringFormats );

			return parser._calcFormats;
		}

		internal static bool IsValid( string calcStringFormats )
		{
			Dictionary<string, string> formats = Parse( calcStringFormats );
			return null != formats && formats.Count > 0;
		}

		private void ParseHelper( string calcStringFormats )
		{
			Regex rx = new Regex( @"^(?:([^\:]+)\:([^,]+))(?:,(?:([^\:]+)\:([^,]+)))*$" );

			Match match = rx.Match( calcStringFormats );
			if ( !match.Success )
				return;

			_calcFormats = new Dictionary<string, string>( StringComparer.CurrentCultureIgnoreCase );

			GroupCollection groups = match.Groups;

			this.ProcessMatch( groups[1].Captures, groups[2].Captures );
			this.ProcessMatch( groups[3].Captures, groups[4].Captures );
		}

		private void ProcessMatch( CaptureCollection calcNames, CaptureCollection formats )
		{
			Debug.Assert( calcNames.Count == formats.Count );
			int count = Math.Min( calcNames.Count, formats.Count );

			for ( int i = 0; i < count; i++ )
				this.AddCalcFormat( calcNames[i].Value, formats[i].Value );
		}

		private void AddCalcFormat( string calcName, string format )
		{
			_calcFormats[calcName.Trim( )] = format.Trim( );
		}
	}

	#endregion // SummaryStringFormatsParser Class

	#region Parser_ListOfItems Class

	/// <summary>
	/// A helper class for parsing text that contains list of strings separated by predefined item seperator character. 
	/// The strings can be optionally enclosed in quote characters. It also has methods for constructing such a text
	/// from a list of strings. It manages escaping and unescaping of special characters.
	/// </summary>
	internal class Parser_ListOfItems
	{
		#region Nested Data Structures

		[Flags]
		private enum State
		{
			Initial = 0x1,
			Escape = 0x2,
			QuoteOpened = 0x4,
			QuoteClosed = 0x8
		}

		#endregion // Nested Data Structures

		#region Member Vars

		private char[] _openQuoteChars;
		private char[] _closeQuoteChars;
		private char _escapeCharacter;
		private char _itemsSepratorChar;

		private long _mask_allSpecialChars;
		private long _mask_openQuoteChars;
		private long _mask_closeQuoteChars;
		private long _mask_specialCharsWithinQuotes;
		private char[] _allSpecialChars;
		private char[] _specialCharsWithinQuotes;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="openQuoteChar">Open quote character. Examples: ", ', [, ( etc...</param>
		/// <param name="closeQuoteChar">Close quote character. Examples: ", ', ], ) etc...</param>
		/// <param name="escapeCharacter">Escape character. Examples: \</param>
		/// <param name="itemsSepratorChar">Character to seprate multiple items. Examples: , ; | etc...</param>
		public Parser_ListOfItems( char openQuoteChar, char closeQuoteChar, char escapeCharacter, char itemsSepratorChar )
			: this( new char[] { openQuoteChar }, new char[] { closeQuoteChar }, escapeCharacter, itemsSepratorChar )
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="openQuoteChars">Allowed open quote characters. Examples: ", ', [, ( etc... First one will be chosen when calling ConstructList method.</param>
		/// <param name="closeQuoteChars">Allowed close quote character. Examples: ", ', ], ) etc... First one will be chosen when calling ConstructList method.</param>
		/// <param name="escapeCharacter">Escape character. Examples: \</param>
		/// <param name="itemsSepratorChar">Character to seprate multiple items. Examples: , ; | etc...</param>
		public Parser_ListOfItems( char[] openQuoteChars, char[] closeQuoteChars, char escapeCharacter, char itemsSepratorChar )
		{
			if ( 0 == openQuoteChars.Length )
				throw new ArgumentException( "Open quote chars not specified." );

			if ( openQuoteChars.Length != closeQuoteChars.Length )
				throw new ArgumentException( "Open and close quote chars must match." );

			_openQuoteChars = openQuoteChars;
			_closeQuoteChars = closeQuoteChars;
			_escapeCharacter = escapeCharacter;
			_itemsSepratorChar = itemsSepratorChar;

			_allSpecialChars = GridUtilities.Aggregate<char>(
				openQuoteChars, closeQuoteChars, new char[] { escapeCharacter, itemsSepratorChar } );

			_specialCharsWithinQuotes = GridUtilities.Aggregate<char>(
				openQuoteChars, closeQuoteChars, new char[] { escapeCharacter } );

			_mask_allSpecialChars = CalcMask( _allSpecialChars );
			_mask_specialCharsWithinQuotes = CalcMask( _specialCharsWithinQuotes );

			_mask_openQuoteChars = CalcMask( _openQuoteChars );
			_mask_closeQuoteChars = CalcMask( _closeQuoteChars );
		}

		#endregion // Constructor

		#region Methods

		#region Private Methods

		#region AddItemHelper

		private void AddItemHelper( List<string> itemList, StringBuilder sb,
			ref int lastEscapedIndexInSB, bool trimTrailingSpaces, bool skipEmptyItems )
		{
			if ( trimTrailingSpaces )
				TrimTrailingSpaces( sb, lastEscapedIndexInSB );

			if ( sb.Length > 0 || !skipEmptyItems )
				itemList.Add( sb.ToString( ) );

			sb.Remove( 0, sb.Length );
			lastEscapedIndexInSB = -1;
		}

		#endregion // AddItemHelper

		#region Append_EncloseInQuotes

		private void Append_EncloseInQuotes( StringBuilder sb, string item )
		{
			sb.Append( _openQuoteChars[0] );

			string escapedItem = this.EscapeString( item, true, false );
			sb.Append( escapedItem );

			sb.Append( _closeQuoteChars[0] );
		}

		#endregion // Append_EncloseInQuotes

		#region CalcMask

		private static long CalcMask( char[] arr )
		{
			long r = 1;

			for ( int i = 0; i < arr.Length; i++ )
			{
				long tmp = r * (int)arr[i];
				if ( tmp > r )
				{
					r = tmp;
				}
				else
				{
					// We can't use the optmization anymore.
					r = 0;
					break;
				}
			}

			return r;
		}

		#endregion // CalcMask

		#region DoesCharNeedEscaping

		private bool DoesCharNeedEscaping( char c, bool withinQuotes )
		{
			return withinQuotes ? DoesCharNeedEscapingWithinQuotes( c ) : DoesCharNeedEscaping( c );
		}

		#endregion // DoesCharNeedEscaping

		#region DoesCharNeedEscaping

		private bool DoesCharNeedEscaping( char c )
		{
			return 0 == _mask_allSpecialChars % (int)c
				&& Array.IndexOf<char>( _allSpecialChars, c ) >= 0;
		}

		#endregion // DoesCharNeedEscaping

		#region DoesCharNeedEscapingWithinQuotes

		private bool DoesCharNeedEscapingWithinQuotes( char c )
		{
			return 0 == _mask_specialCharsWithinQuotes % (int)c
				&& Array.IndexOf<char>( _specialCharsWithinQuotes, c ) >= 0;
		}

		#endregion // DoesCharNeedEscapingWithinQuotes

		#region GetCorrespondingCloseQuoteChar

		private char GetCorrespondingCloseQuoteChar( char openQuote )
		{
			char[] openChars = _openQuoteChars;

			for ( int i = 0; i < openChars.Length; i++ )
			{
				if ( openQuote == openChars[i] )
					return _closeQuoteChars[i];
			}

			throw new ArgumentException( "Specified character is not an open quote character." );
		}

		#endregion // GetCorrespondingCloseQuoteChar

		#region IsCloseQuoteChar

		private bool IsCloseQuoteChar( char c )
		{
			return 0 == _mask_closeQuoteChars % (int)c
				&& Array.IndexOf<char>( _closeQuoteChars, c ) >= 0;
		}

		#endregion // IsCloseQuoteChar

		#region IsOpenQuoteChar

		private bool IsOpenQuoteChar( char c )
		{
			return 0 == _mask_openQuoteChars % (int)c
				&& Array.IndexOf<char>( _openQuoteChars, c ) >= 0;
		}

		#endregion // IsOpenQuoteChar

		#region TrimTrailingSpaces

		private static void TrimTrailingSpaces( System.Text.StringBuilder sb, int lastEscapedCharIndex )
		{
			// Remove trailing spaces however don't remove a space character that was escaped.
			//
			for ( int j = sb.Length - 1; j >= 0 && j > lastEscapedCharIndex; j-- )
			{
				if ( char.IsWhiteSpace( sb[j] ) )
					sb.Remove( j, 1 );
				else
					break;
			}
		}

		#endregion // TrimTrailingSpaces

		#endregion // Private Methods

		#region Public Methods

		#region ConstructList

		public string ConstructList( string[] items, bool seprateEachItemBySpace )
		{
			StringBuilder sb = new StringBuilder( );

			for ( int i = 0; i < items.Length; i++ )
			{
				if ( i > 0 )
				{
					sb.Append( _itemsSepratorChar );

					if ( seprateEachItemBySpace )
						sb.Append( ' ' );
				}

				//this.Append_EncloseInQuotes( sb, items[i] );
				sb.Append( this.EscapeString( items[i], false, true ) );
			}

			return sb.ToString( );
		}

		#endregion // ConstructList

		#region EscapeString

		/// <summary>
		/// Escapes all the special charactes in the specified text. If withinQuotes is true then it
		/// only escapes the special characters that need to be escaped within quotes, namely 
		/// the quote characters and the escape character.
		/// </summary>
		/// <param name="text">Text that needs to be escaped.</param>
		/// <param name="withinQuotes">Whether the text is going to be enclosed in double quotes. 
		/// If true then only escapes characters that need to be escaped in double quotes. 
		/// Note that this method doesn't actually enclose the string in quotes.</param>
		/// <param name="escapeLeadingAndTrailingWhiteSpace">Specifies whether to escape leading and trailing
		/// white space characters so they are not 'lost' when unescaping and processing.</param>
		/// <returns>Returns the escaped text.</returns>
		public string EscapeString( string text, bool withinQuotes, bool escapeLeadingAndTrailingWhiteSpace )
		{
			System.Text.StringBuilder sb = null;

			for ( int i = 0; i < text.Length; i++ )
			{
				char c = text[i];

				if ( this.DoesCharNeedEscaping( c, withinQuotes )
					// Also escape the leading and trailing spaces so they don't loose their
					// meaning when they are reparsed back. For example, in reference 
					// Customers( State\ = NY ), the column in the name-value pair is "State "
					// since the trailing space was escaped. So when escaping "State ", we should
					// end up with "State\ " as well so that when ToString operation is performed
					// on a parsed reference, you get the trailing space escaped in the column 
					// name. 
					//
					|| escapeLeadingAndTrailingWhiteSpace
						&& ( 0 == i || 1 + i == text.Length ) && !withinQuotes && char.IsWhiteSpace( c ) )
				{
					if ( null == sb )
						sb = new System.Text.StringBuilder( text, 0, i, 2 * text.Length - i );

					sb.Append( _escapeCharacter ).Append( c );
				}
				else if ( null != sb )
					sb.Append( c );
			}

			return null == sb ? text : sb.ToString( );
		}

		#endregion // EscapeString

		#region NormalizeEscapement

		/// <summary>
		/// This method has the same effect as calling UnEscapeString and then calling EscapeString
		/// on the specified text. This ensures that the text is minimally escaped.
		/// </summary>
		/// <param name="text">The text that should be normalized.</param>
		/// <param name="withinQuotes">Whether the text is going to be enclosed in double quotes. If true then only escapes characters that need to be escaped in double quotes.</param>
		/// <returns>A string that has been unescaped and then escaped.</returns>
		public string NormalizeEscapement( string text, bool withinQuotes )
		{
			return this.EscapeString( this.UnEscapeString( text ), withinQuotes, true );
		}

		#endregion // NormalizeEscapement

		#region ParseList

		public string[] ParseList( string text, bool skipEmptyItems )
		{
			List<string> itemList = new List<string>( );
			StringBuilder sb = new StringBuilder( );

			State state = State.Initial;
			char expectedCloseQuote = (char)0;
			int lastEscapedIndexInSB = -1;

			for ( int i = 0; i < text.Length; i++ )
			{
				char c = text[i];

				if ( State.Escape == ( State.Escape & state ) )
				{
					lastEscapedIndexInSB = sb.Length;
					sb.Append( c );
					state ^= State.Escape;
				}
				else if ( _escapeCharacter == c )
				{
					state |= State.Escape;
				}
				else if ( State.QuoteOpened == ( State.QuoteOpened & state ) && c == expectedCloseQuote )
				{
					this.AddItemHelper( itemList, sb, ref lastEscapedIndexInSB, false, skipEmptyItems );
					state ^= State.QuoteOpened;
					state |= State.QuoteClosed;
				}
				else if ( State.QuoteOpened != ( State.QuoteOpened & state ) && this.IsOpenQuoteChar( c ) )
				{
					expectedCloseQuote = this.GetCorrespondingCloseQuoteChar( c );
					state |= State.QuoteOpened;
					state &= ~State.QuoteClosed;
				}
				else if ( State.QuoteClosed == ( State.QuoteClosed & state ) && _itemsSepratorChar == c )
				{
					// Skip blanks and separator character after close quote.
					state ^= State.QuoteClosed;
				}
				else if ( State.QuoteClosed == ( State.QuoteClosed & state ) )
				{
					state ^= State.QuoteClosed;
				}
				else if ( State.Initial == state && char.IsWhiteSpace( c ) && 0 == sb.Length )
				{
					// Skip white spaces that are not part of quoted strings and that are
					// not part of any item.
				}
				else if ( State.Initial == state && _itemsSepratorChar == c )
				{
					this.AddItemHelper( itemList, sb, ref lastEscapedIndexInSB, true, skipEmptyItems );
					state &= ~State.QuoteClosed;
				}
				else
				{
					sb.Append( c );
				}
			}

			if ( State.Initial != state )
			{
				if ( State.Escape == state )
					Debug.Assert( false, "Unescaped escape character encountered at the end of the text." );
				else if ( State.QuoteOpened == state )
					Debug.Assert( false, "Text has open quote without a matching close quote." );
			}

			if ( sb.Length > 0 )
				this.AddItemHelper( itemList, sb, ref lastEscapedIndexInSB, true, skipEmptyItems );

			return itemList.ToArray( );
		}

		#endregion // ParseList

		#region UnEscapeString

		/// <summary>
		/// Unescapes all the escaped charactes in the specified text.
		/// </summary>
		/// <param name="text">The text whose escaped characters should be unescaped.</param>
		/// <returns>A string with all escaped characters unescaped.</returns>
		public string UnEscapeString( string text )
		{
			System.Text.StringBuilder sb = null;

			for ( int i = 0; i < text.Length; i++ )
			{
				char c = text[i];

				if ( _escapeCharacter == c )
				{
					if ( null == sb )
						sb = new System.Text.StringBuilder( text, 0, i, text.Length - 1 );

					i++;
					if ( i < text.Length )
						sb.Append( text[i] );
					else
						// If we get here then it means the last character is the escape character 
						// that wasn't escaped.
						Debug.Assert( false, "Unescaped escape character \\ encountered." );
				}
				else if ( null != sb )
					sb.Append( c );
			}

			return null == sb ? text : sb.ToString( );
		}

		#endregion // UnEscapeString

		#endregion // Public Methods

		#endregion // Methods
	}

	#endregion // Parser_ListOfItems Class

	#region DynamicResourceTracker Class

	
	
	internal class DynamicResourceTracker
	{
		public delegate void RaisePropertyValueChanged( string propName );
		private DynamicResourceString _dr;
		private RaisePropertyValueChanged _callback;
		private string _callbackPropName;

		internal DynamicResourceTracker( DynamicResourceString dr, RaisePropertyValueChanged propChangeCallback, string callbackPropName )
		{
			_dr = dr;
			_callback = propChangeCallback;
			_callbackPropName = callbackPropName;

			dr.PropertyChanged += new PropertyChangedEventHandler( Resource_PropertyChanged );
		}

		private void Resource_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			if ( "Value" == e.PropertyName )
			{
				_callback( _callbackPropName );
			}
		}
	}

	#endregion // DynamicResourceTracker Class

	#region DependencyObjectSerializationInfo Class

	// SSP 9/3/09 TFS18172
	// Added DependencyObjectSerializationInfo class.
	// 

	/// <summary>
	/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing dependency objects.
	/// </summary>
	internal class DependencyObjectSerializationInfo : ObjectSerializationInfo
	{
		#region Member Vars

		private DependencyProperty[] _dpProps;
		private Type _type;
		private PropertySerializationInfo[] _props;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="dpProps"></param>
		internal DependencyObjectSerializationInfo( Type type, DependencyProperty[] dpProps )
		{
			GridUtilities.ValidateNotNull( type );
			GridUtilities.ValidateNotNull( dpProps );

			if ( !typeof( DependencyObject ).IsAssignableFrom( type ) )
				throw new ArgumentException( );

			_type = type;
			_dpProps = dpProps;
		}

		#endregion // Constructor

		#region CreateFromDependencyProperties

		private static PropertySerializationInfo[] CreateFromDependencyProperties( DependencyProperty[] dpProps )
		{
			return Array.ConvertAll<DependencyProperty, PropertySerializationInfo>( dpProps,
						new Converter<DependencyProperty, PropertySerializationInfo>(
							delegate( DependencyProperty source )
							{
								return PropertySerializationInfo.Create( source );
							}
						)
					);
		}

		#endregion // CreateFromDependencyProperties

		#region Base Overrides

		#region SerializedProperties

		public override IEnumerable<PropertySerializationInfo> SerializedProperties
		{
			get
			{
				if ( null == _props )
				{
					_props = CreateFromDependencyProperties( _dpProps );
				}

				return _props;
			}
		}

		#endregion // SerializedProperties

		#region Deserialize

		public override object Deserialize( Dictionary<string, object> values )
		{
			DependencyObject dpObj = (DependencyObject)Activator.CreateInstance( _type );
			object v;

			foreach ( DependencyProperty dp in _dpProps )
			{
				if ( values.TryGetValue( dp.Name, out v ) )
					dpObj.SetValue( dp, v );
			}

			return dpObj;
		}

		#endregion // Deserialize

		#region Serialize

		public override Dictionary<string, object> Serialize( object obj )
		{
			DependencyObject dpObj = (DependencyObject)obj;

			Dictionary<string, object> values = new Dictionary<string, object>( );

			foreach ( DependencyProperty dp in _dpProps )
			{
				if ( Utilities.ShouldSerialize( dp, dpObj ) )
					values[dp.Name] = dpObj.GetValue( dp );
			}

			return values;
		}

		#endregion // Serialize

		#endregion // Base Overrides
	}

	#endregion // DependencyObjectSerializationInfo Class

	#region DataPresenterObjectSerializer Class

	// SSP 1/17/09 - NAS9.1 Record Filtering
	// Added ObjectSerializerBase in windows project and added DataPresenterObjectSerializer
	// here which derives from the ObjectSerializerBase.
	// 
	internal class DataPresenterObjectSerializer : ObjectSerializerBase
	{
		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal DataPresenterObjectSerializer( ) : base( )
		{
			this.RegisterInfo( typeof( SummaryDefinition ), new SummaryDefinition.SerializationInfo( ) );
		}

		#endregion // Constructor

		#region Copy

		private static void Copy( ConditionGroup source, ConditionGroup dest )
		{
			dest.LogicalOperator = source.LogicalOperator;

			dest.Clear( );
			foreach ( ICondition c in source )
				dest.Add( c );
		}

		#endregion // Copy

		#region SaveHelper

		private XmlElement SaveHelper( object obj )
		{
			Type type = obj.GetType( );

			if ( typeof( RecordFilter ) == type )
			{
				RecordFilter rf = (RecordFilter)obj;
				XmlElement elem = this.CreateNewElement( "RecordFilter" );

				if ( this.AppendValueHelper( elem, "FieldName", null, rf.FieldName, typeof( string ) )
					&& this.AppendValueHelper( elem, null, null, rf.Conditions, typeof( ConditionGroup ) ) )
					return elem;
			}

			return null;
		}

		#endregion // SaveHelper

		#region LoadHelper

		private object LoadHelper( XmlElement elem )
		{
			object v;

			string elemName = elem.Name;
			switch ( elemName )
			{
				case "RecordFilter":
					{
						RecordFilter rf = new RecordFilter( );

						if ( this.ParseAttribute( elem, "FieldName", typeof( string ), out v ) )
							rf.FieldName = (string)v;

						if ( this.ParseElement( elem.SelectSingleNode( "ConditionGroup" ) as XmlElement, typeof( ConditionGroup ), out v ) )
							Copy( (ConditionGroup)v, rf.Conditions );

						return rf;
					}
			}

			return null;
		}

		#endregion // LoadHelper

		#region Base Overrides

		#region SaveAsElementOverride

		protected override bool SaveAsElementOverride( object obj, Type propertyType, out XmlElement element )
		{
			element = this.SaveHelper( obj );
			if ( null != element )
				return true;

			return base.SaveAsElementOverride( obj, propertyType, out element );
		}

		#endregion // SaveAsElementOverride

		#region ParseElementOverride

		protected override bool ParseElementOverride( XmlElement element, Type propertyType, out object parsedValue )
		{
			parsedValue = this.LoadHelper( element );
			if ( null != parsedValue && ( null == propertyType || GridUtilities.IsObjectOfType( parsedValue, propertyType ) ) )
				return true;

			return base.ParseElementOverride( element, propertyType, out parsedValue );
		}

		#endregion // ParseElementOverride

		#endregion // Base Overrides
	}

	#endregion // DataPresenterObjectSerializer Class

    // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
    #region NegateConverter
    internal class NegateConverter : IValueConverter
    {
        internal static readonly IValueConverter Instance = new NegateConverter();

        private NegateConverter()
        {
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return -(double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    } 
    #endregion //NegateConverter

    // AS 1/16/09 NA 2009 Vol 1 - Fixed Fields
    #region FixedFarConverter
    internal class FixedFarConverter : IMultiValueConverter
    {
        internal static readonly FixedFarConverter Instance = new FixedFarConverter();

        private FixedFarConverter()
        {
        }

        #region IMultiValueConverter Members

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return Binding.DoNothing;

            for (int i = 0; i < 3; i++)
            {
                if (values[i] is double == false)
                    return Binding.DoNothing;
            }

            double offset = (double)values[0];
            double fixedAreaExtent = (double)values[1];
            double totalExtent = (double)values[2];

            if (fixedAreaExtent < totalExtent)
            {
                return -Math.Max(totalExtent - fixedAreaExtent - offset, 0);
            }

            return 0;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    } 
    #endregion //FixedFarConverter

	#region CellTextConverterInfo Class

	// SSP 4/1/09 - Cell Text
	// Added logic to be able to convert the cell values to display texts.
	// 
	/// <summary>
	/// Class used for converting cell values to cell display texts.
	/// </summary>
	internal class CellTextConverterInfo
	{
		#region Private Vars

		private Field _field;
		private ValueEditor _editor;
		private IValueConverter _converter;
		private CultureInfo _culture;

        // AS 4/30/09 NA 2009.2 ClipboardSupport
		private Type _editAsType;

		
		
		private int _verifiedTemplateCacheVersion;

		// SSP 1/20/10 TFS33040
		// 
		private bool? _cachedIsTimePortionVisible;
		private bool? _cachedIsSecondsPortionVisible;
		private PropertyValueTracker _cachedMaskedEditorSectionsTracker;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="field"></param>
		
		
		
		
		//internal CellTextConverterInfo( Field field )
		private CellTextConverterInfo( Field field )
		{
			_field = field;

			ValueEditor editor = null;

			FieldLayout fl = field.Owner;
			TemplateDataRecordCache templateDataRecordCache = fl.TemplateDataRecordCache;
			// AS 7/7/09 TFS19145
			// This method would have returned null if the field was not in the layout. This would 
			// come up when grouping by a field but have the field hidden in the display. I added
			// an overload to allow us to force creation of a cellplaceholder to get a cvp and therefore
			// the editor we need here.
			//
			//CellValuePresenter cvp = templateDataRecordCache.GetCellValuePresenter( field );
			CellValuePresenter cvp = templateDataRecordCache.GetCellValuePresenter( field, true );

			if ( null != cvp )
				editor = cvp.Editor;

			// SSP 5/5/09
			// 
			if ( null == editor )
			{
				editor = new XamTextEditor( );
				editor.ValueType = _field.EditAsTypeResolved;
			}

			IValueConverter converter = null;

			if ( editor is TextEditorBase )
				converter = ( (TextEditorBase)editor ).ValueToDisplayTextConverterResolved;
			else
				converter = editor.ValueToTextConverterResolved;

			Debug.Assert( null != converter );

			_editor = editor;
			_converter = converter;
			_culture = field.ConverterCultureResolved;

			// AS 4/30/09 NA 2009.2 ClipboardSupport
			_editAsType = field.EditAsTypeResolved;

			
			
			_verifiedTemplateCacheVersion = templateDataRecordCache.CacheVersion;
		}

		#endregion // Constructor

		#region Methods

		#region ConvertCellValue

		public static string ConvertCellValue( object value, Field field )
		{
			
			
			
			
			CellTextConverterInfo info = GetCachedConverter( field );
			return info.ConvertCellValue( value );
			
		}

		public static string ConvertCellValue( DataRecord record, Field field )
		{
			
			
			
			
			CellTextConverterInfo info = GetCachedConverter( field );
			return info.ConvertCellValue( record );
			
		}

		public string ConvertCellValue( DataRecord record )
		{
            // JJD 5/29/09 - TFS18063 
            // Use the new overload to GetCellValue which will return the value 
            // converted into EditAsType
            //return this.ConvertCellValue( record.GetCellValue( _field, true ) );
			return this.ConvertCellValue( record.GetCellValue( _field, CellValueType.EditAsType) );
		}

		public string ConvertCellValue( object cellValue )
		{
			Exception ex;
			return ConvertCellValue(cellValue, out ex);
		}

		public string ConvertCellValue( object cellValue, out Exception error )
		{
			string text = null;

			ValueEditor editor = _editor;
			TextEditorBase textEditor = editor as TextEditorBase;

			if ( null != textEditor )
				textEditor.ConvertValueToDisplayText( cellValue, out text, out error );
			else
				editor.ConvertValueToText( cellValue, out text, out error );

			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


			if ( null == text )
				text = string.Empty;

			return text;
		}

		#endregion // ConvertCellValue

		// AS 4/28/09 NA 2009.2 ClipboardSupport
		#region ConvertDisplayText
		internal object ConvertDisplayText(string displayText, out Exception error)
		{
			object convertedValue;

			ValueEditor editor = _editor;
			TextEditorBase textEditor = editor as TextEditorBase;

			if ( null != textEditor )
				textEditor.ConvertDisplayTextToValue( displayText, out convertedValue, out error );
			else
				editor.ConvertTextToValue( displayText, out convertedValue, out error );

			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


			return convertedValue;
		}
		#endregion //ConvertDisplayText

		#region GetCachedConverter

		
		
		internal static CellTextConverterInfo GetCachedConverter( Field field )
		{
			CellTextConverterInfo info = field._cachedCellTextConverterInfo;
			FieldLayout fl = field.Owner;
			if ( null == info || null != fl && info._verifiedTemplateCacheVersion != fl.TemplateDataRecordCache.CacheVersion )
				field._cachedCellTextConverterInfo = info = new CellTextConverterInfo( field );

			return info;
		}

		#endregion // GetCachedConverter

		#region GetVisiblePortion

		// SSP 1/20/10 TFS33040
		// 
		internal DateTime GetVisiblePortion( DateTime date )
		{
			if ( !this.IsTimePortionVisible )
				return date.Date;

			return _cachedIsSecondsPortionVisible.Value
				? new DateTime( date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second )
				: new DateTime( date.Year, date.Month, date.Day, date.Hour, date.Minute, 0 );
		}

		#endregion // GetVisiblePortion

		// AS 5/5/09 NA 2009.2 ClipboardSupport
		#region ValidateValue
		internal bool ValidateValue(object newValue, out Exception error)
		{
			return _editor.ValidateValue(newValue, out error);
		}
		#endregion //ValidateValue

		#endregion // Methods

		#region Properties

		#region CompareByText

		// SSP 6/16/09 - TFS18467
		// 

		/// <summary>
		/// Returns true to indicate that the cell value comparisons should be
		/// done using cell texts.
		/// </summary>
		internal bool CompareByText
		{
			get
			{



				XamComboEditor combo = _editor as XamComboEditor;
				return null != combo && null != combo.ItemsProvider;

			}
		}

		#endregion // CompareByText

		
		#region IsTimePortionVisible

		// SSP 1/20/10 TFS33040
		// 
		private void OnMaskedEditorSectionsChanged( )
		{
			_cachedIsTimePortionVisible = null;
		}

		// SSP 1/20/10 TFS33040
		// 
		internal bool IsTimePortionVisible
		{
			get
			{
				if ( ! _cachedIsTimePortionVisible.HasValue )
				{
					_cachedIsTimePortionVisible = true;
					_cachedIsSecondsPortionVisible = true;

					XamMaskedEditor maskedEditor = _editor as XamMaskedEditor;
					if ( null != maskedEditor )
					{
						if ( null == _cachedMaskedEditorSectionsTracker )
							_cachedMaskedEditorSectionsTracker = new PropertyValueTracker( maskedEditor, "Sections.Count", OnMaskedEditorSectionsChanged );

						SectionsCollection sections = maskedEditor.Sections;

						bool containsTimeSections = null != sections && GridUtilities.GetCount( GridUtilities.Filter(
								sections, new GridUtilities.MeetsCriteria_Type( typeof( HourSection ), true ) ) ) > 0;

						_cachedIsTimePortionVisible = containsTimeSections;

						_cachedIsSecondsPortionVisible = null != sections && GridUtilities.GetCount( GridUtilities.Filter(
								sections, new GridUtilities.MeetsCriteria_Type( typeof( SecondSection ), true ) ) ) > 0;
					}
				}

				return _cachedIsTimePortionVisible.Value;
			}				
		}

		#endregion // IsTimePortionVisible

		#region ShouldExportText

		// SSP 10/22/09 TFS24061
		// 
		/// <summary>
		/// Returns true to indicate that when exporting cell values (to excel exporter for example)
		/// use the cell's text instead of the value.
		/// </summary>
		internal bool ShouldExportText
		{
			get
			{
				return this.CompareByText;
			}
		}

		#endregion // ShouldExportText

		#region Field

		internal Field Field
		{
			get
			{
				return _field;
			}
		}

		#endregion // Field

		#region SortByText

		/// <summary>
		/// Returns true to indicate that the records should be sorted by 
		/// the cell texts instead of cell values.
		/// </summary>
		internal bool SortByText
		{
			get
			{



				XamComboEditor combo = _editor as XamComboEditor;
				return null != combo && null != combo.ItemsProvider;

			}
		}

		#endregion // SortByText

		#endregion // Properties
	}

	#endregion // CellTextConverterInfo Class

    // AS 4/21/09 NA 2009.2 ClipboardSupport
    #region CellVisiblePositionComparer
    /// <summary>
    /// Custom comparer for <see cref="Cell"/> instances that compares using the <see cref="RecordVisiblePositionComparer"/> and <see cref="FieldNavigationIndexComparer"/> comparers.
    /// </summary>
    internal class CellVisiblePositionComparer : ClassComparer<Cell>
    {
        #region Member Variables

        internal static readonly IComparer<Cell> Instance = new CellVisiblePositionComparer(); 

        #endregion //Member Variables

        #region Constructor
        private CellVisiblePositionComparer()
        {
        } 
        #endregion //Constructor
    
        #region Base class overrides

        protected override int CompareOverride(Cell x, Cell y)
        {
            int result = RecordVisiblePositionComparer.Instance.Compare(x.Record, y.Record);

            if (result == 0)
                result = FieldNavigationIndexComparer.Instance.Compare(x.Field, y.Field);

            return result;
        }

        #endregion //Base class overrides
    } 
    #endregion //CellVisiblePositionComparer

	// MD 8/20/10
	// We don't need the CVPCache anymore. We can get similar gains by caching the cell panel on the data record presenter.
	#region Removed

	
#region Infragistics Source Cleanup (Region)














































































































#endregion // Infragistics Source Cleanup (Region)


	#endregion // Removed

    // AS 4/21/09 NA 2009.2 ClipboardSupport
    #region FieldNavigationIndexComparer
    /// <summary>
    /// Custom comparer for <see cref="Field"/> instances that compares using the <see cref="Field.NavigationIndex"/> for fields in the same layout - otherwise using the index of the owning field layout.
    /// </summary>
    internal class FieldNavigationIndexComparer : ClassComparer<Field>
    {
        #region Member Variables

        internal static readonly IComparer<Field> Instance = new FieldNavigationIndexComparer();

        #endregion //Member Variables

        #region Constructor
        private FieldNavigationIndexComparer()
        {
        }
        #endregion //Constructor

        #region Base class overrides

        protected override int CompareOverride(Field x, Field y)
        {
            if (x.Owner == y.Owner)
                return x.NavigationIndex.CompareTo(y.NavigationIndex);

            if (x.Owner == null)
            {
                if (y.Owner == null)
                    return 0;

                return -1;
            }
            else if (y.Owner == null)
                return 1;

            return x.Owner.Index.CompareTo(y.Owner.Index);
        }

        #endregion //Base class overrides
    }
    #endregion //FieldNavigationIndexComparer

    #region RecordAndField Class

    // SSP 6/7/10 - Optimizations - TFS34031
    // Added RecordAndField structure.
    // 

    internal class RecordAndField
    {
        private object _record;
        private object _field;

        internal RecordAndField( Record record, Field field, bool useWeakReferences )
        {
            GridUtilities.ValidateNotNull( record );
            GridUtilities.ValidateNotNull( field );

            if ( useWeakReferences )
            {
                _record = new WeakReference( record );
                _field = new WeakReference( field );
            }
            else
            {
                _record = record;
                _field = field;
            }
        }

        public Record Record
        {
            get
            {
                WeakReference w = _record as WeakReference;
                if ( null != w )
                    return (Record)Utilities.GetWeakReferenceTargetSafe( w );

                return (Record)_record;
            }
        }

        public Field Field
        {
            get
            {
                WeakReference w = _field as WeakReference;
                if ( null != w )
                    return (Field)Utilities.GetWeakReferenceTargetSafe( w );

                return (Field)_field;
            }
        }

        public override int GetHashCode( )
        {
            Record record = this.Record;
            Field field = this.Field;

            return ( null != record ? record.GetHashCode( ) : 1 )
                ^ ( null != field ? field.GetHashCode( ) : 2 );
        }

        public override bool Equals( object obj )
        {
            if ( obj is RecordAndField )
            {
                RecordAndField rf = (RecordAndField)obj;
                return rf.Record == this.Record && rf.Field == this.Field;
            }

            return false;
        }


    }

    #endregion // RecordAndField Class

    // AS 4/21/09 NA 2009.2 ClipboardSupport
    #region RecordVisiblePositionComparer
    /// <summary>
    /// Custom comparer for <see cref="Record"/> instances that uses the <see cref="Record.VisibleIndex"/> for records in the same collection the <see cref="Record.OverallSelectionPosition"/> otherwise.
    /// </summary>
    internal class RecordVisiblePositionComparer : ClassComparer<Record>
    {
        #region Member Variables

        internal static readonly IComparer<Record> Instance = new RecordVisiblePositionComparer();

        #endregion //Member Variables

        #region Constructor
        private RecordVisiblePositionComparer()
        {
        }
        #endregion //Constructor

        #region Base class overrides

        protected override int CompareOverride(Record x, Record y)
        {
            // if they're in the same parent collection then we just need to consider their index
            if (x.ParentCollection == y.ParentCollection)
            {
                return x.VisibleIndex.CompareTo(y.VisibleIndex);
            }

            return x.OverallSelectionPosition.CompareTo(y.OverallSelectionPosition);
        }

        #endregion //Base class overrides
    }
    #endregion //RecordVisiblePositionComparer

	#region VisibleDataBlock Class

	// SSP 2/1/10
	// Added CellsInViewChanged event to the DataPresenterBase.
	// 
	/// <summary>
	/// Contains a list of data records and a list of fields that represent a block of cells
	/// that's currently visible in the data presenter.
	/// </summary>
	public class VisibleDataBlock
	{
		#region Nested Data Structures

		#region VisibleDataBlockCache Class

		/// <summary>
		/// This is used to cache the information that we passed along in the event args the last time
		/// we raised the CellsInViewChanged event. When the display changes, we need to make sure the
		/// set of cells are different from the last time we raised that event in order to raise the
		/// event again.
		/// </summary>
		internal class VisibleDataBlockCache
		{
			#region Member Vars

			private WeakList<DataRecord> _records;
			private WeakList<Field> _fields;

			#endregion // Member Vars

			#region Constructor

			internal VisibleDataBlockCache( )
			{
				_records = new WeakList<DataRecord>( );
				_fields = new WeakList<Field>( );
			}

			#endregion // Constructor

			#region Methods

			#region ClearLists

			internal void ClearLists( )
			{
				_records.Clear( );
				_fields.Clear( );
			}

			#endregion // ClearLists

			#region Initialize

			internal void Initialize( VisibleDataBlock block )
			{
				this.ClearLists( );

				_records.AddRange( block._records );
				_fields.AddRange( block._fields );
			}

			#endregion // Initialize

			#region IsSame

			internal bool IsSame( VisibleDataBlock block )
			{
				if ( !GridUtilities.CompareLists( _records, block._records )
					|| !GridUtilities.CompareLists( _fields, block._fields ) )
					return false;

				return true;
			}

			#endregion // IsSame

			#endregion // Methods
		}

		#endregion // VisibleDataBlockCache Class

		#endregion // Nested Data Structures

		#region Member Vars

		private List<DataRecord> _records;
		private List<Field> _fields;
		private RecordManager _recordManager;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="VisibleDataBlock"/>.
		/// </summary>
		private VisibleDataBlock( )
		{
		}

		#endregion // Constructor

		#region Methods

		#region Private/Internal Methods

		#region CacheVisibleBlockInfo

		/// <summary>
		/// Caches the visible data blocks that we passed into the last CellsInViewChanged event args
		/// on the data presenter to prevent us from unneccesarily raising the event again if the
		/// data in view hasn't changed.
		/// </summary>
		/// <param name="dp">Data presenter.</param>
		/// <param name="blocks">The list of visible data blocks.</param>
		internal static void CacheVisibleBlockInfo( DataPresenterBase dp, List<VisibleDataBlock> blocks )
		{
			List<VisibleDataBlockCache> lastVisibleDataBlocks = dp._lastVisibleDataBlocks;

			if ( null == lastVisibleDataBlocks )
				dp._lastVisibleDataBlocks = lastVisibleDataBlocks = new List<VisibleDataBlockCache>( );

			for ( int i = 0, count = blocks.Count; i < count; i++ )
			{
				if ( i == lastVisibleDataBlocks.Count )
					lastVisibleDataBlocks.Add( new VisibleDataBlockCache( ) );

				lastVisibleDataBlocks[i].Initialize( blocks[i] );
			}

			for ( int i = blocks.Count, count = lastVisibleDataBlocks.Count; i < count; i++ )
			{
				lastVisibleDataBlocks[i].ClearLists( );
			}
		}

		#endregion // CacheVisibleBlockInfo

		#region Create

		/// <summary>
		/// Returns a new list of VisibleDataBlock objects that descrive cells currently in view.
		/// </summary>
		/// <param name="dp">Data presenter.</param>
		/// <returns>Collection of VisibleDataBlock instances.</returns>
		internal static List<VisibleDataBlock> Create( DataPresenterBase dp )
		{
			// Get the record presenters that are currently in view.
			// 
			List<RecordPresenter> recordPresenters = dp.GetRecordPresentersInView(true, false);

			// AS 3/2/11 66934 - AutoSize
			return Create(recordPresenters);
		}

		// AS 3/2/11 66934 - AutoSize
		// Added an overload that took the record presenters since we will already have 
		// them available when processing the records in view.
		//
		internal static List<VisibleDataBlock> Create( List<RecordPresenter> recordPresenters )
		{
			List<VisibleDataBlock> blocksList = new List<VisibleDataBlock>( );
			Dictionary<RecordManager, VisibleDataBlock> blocks = new Dictionary<RecordManager, VisibleDataBlock>( );
			VisibleDataBlock lastBlock = null;

			for ( int i = 0, count = recordPresenters.Count; i < count; i++ )
			{
				RecordPresenter rp = recordPresenters[i];

				if ( rp is DataRecordPresenter )
				{
					// We are only concerned about data records. CellsInViewChanged is supposed to be raised
					// only for data cells.
					// 
					DataRecord dataRecord = rp.Record as DataRecord;
					if ( null != dataRecord && dataRecord.IsDataRecord )
					{
						RecordManager dataRecordRecordManager = dataRecord.RecordManager;

						// LastBlock stack variable is an optimization to avoid having to retrieve
						// the VisibleDataBlock for the associated record manager from the dictionary
						// unless it's a different record manager.
						// 
						if ( null == lastBlock || lastBlock._recordManager != dataRecordRecordManager )
						{
							if ( !blocks.TryGetValue( dataRecordRecordManager, out lastBlock ) )
							{
								lastBlock = new VisibleDataBlock( );
								lastBlock._recordManager = dataRecordRecordManager;
								blocksList.Add( lastBlock );
								blocks[dataRecordRecordManager] = lastBlock;
							}
						}

						// If the VisibleDataBlock is newly created then initialize its records list.
						// 
						if ( null == lastBlock._records )
							lastBlock._records = new List<DataRecord>( );

						lastBlock._records.Add( dataRecord );

						// If the VisibleDataBlock is newly created then initialize its fields list as well.
						// Since all records from the same record manager should be at the same level in
						// hierarchy, we only need to get the list of fields from only one of the record 
						// presenters.
						// 
						if ( null == lastBlock._fields )
						{
							// MD 8/19/10
							// If the associated cell panel is cached, use it instead of searching for it.
							//VirtualizingDataRecordCellPanel cellPanel = Utilities.GetDescendantFromType<VirtualizingDataRecordCellPanel>( rp, true, null );
							VirtualizingDataRecordCellPanel cellPanel = null;
							DataRecordPresenter drp = rp as DataRecordPresenter;

							if (drp != null)
								cellPanel = drp.AssociatedVirtualizingDataRecordCellPanel;

							if (cellPanel == null)
								cellPanel = Utilities.GetDescendantFromType<VirtualizingDataRecordCellPanel>(rp, true, null);

							if ( null != cellPanel )
								lastBlock._fields = cellPanel.GetFieldsInView( );
						}
					}
				}
			}

			return blocksList;
		}

		#endregion // Create		

		#region RaiseCellsInViewChanged

		private static int g_CreateCount = 0;

		internal static void RaiseCellsInViewChanged( DataPresenterBase dp )
		{
			Debug.WriteLine( ++g_CreateCount, " Calculating visible data." );

			List<VisibleDataBlock> currentDataBlocks = Create( dp );

			List<VisibleDataBlockCache> lastVisibleDataBlocks = dp._lastVisibleDataBlocks;
			if ( null != lastVisibleDataBlocks )
			{
				if ( lastVisibleDataBlocks.Count == currentDataBlocks.Count )
				{
					bool changed = false;

					for ( int i = 0, count = lastVisibleDataBlocks.Count; i < count; i++ )
					{
						if ( !lastVisibleDataBlocks[i].IsSame( currentDataBlocks[i] ) )
						{
							changed = true;
							break;
						}
					}

					if ( !changed )
						return;
				}
			}

			CacheVisibleBlockInfo( dp, currentDataBlocks );

			CellsInViewChangedEventArgs eventArgs = new CellsInViewChangedEventArgs( currentDataBlocks );
			dp.RaiseCellsInViewChanged( eventArgs );
		}

		#endregion // RaiseCellsInViewChanged

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Fields

		/// <summary>
		/// List of fields that are visible.
		/// </summary>
		public List<Field> Fields
		{
			get
			{
				return _fields;
			}
		}

		#endregion // Fields

		#region RecordManager

		/// <summary>
		/// Records from this record manager are included in the <see cref="Records"/> list.
		/// </summary>
		public RecordManager RecordManager
		{
			get
			{
				return _recordManager;
			}
		}

		#endregion // RecordManager

		#region Records

		/// <summary>
		/// List of records that are visible.
		/// </summary>
		public List<DataRecord> Records
		{
			get
			{
				return _records;
			}
		}

		#endregion // Records

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // VisibleDataBlock Class

	// AS 7/26/11 TFS80926
	#region SimpleValueHolder class
	internal class SimpleValueHolder<T> : INotifyPropertyChanged
	{
		#region Member Variables

		private T _value; 

		#endregion //Member Variables

		#region Constructor
		public SimpleValueHolder(T initialValue)
		{
			_value = initialValue;
		} 
		#endregion //Constructor

		#region Properties
		public T Value
		{
			get { return _value; }
			set
			{
				if (EqualityComparer<T>.Default.Equals(_value, value))
					return;

				_value = value;

				var handler = this.PropertyChanged;

				if (null != handler)
					handler(this, new PropertyChangedEventArgs("Value"));
			}
		} 
		#endregion //Properties

		#region Events

		public event PropertyChangedEventHandler PropertyChanged; 

		#endregion //Events
	} 
	#endregion //SimpleValueHolder class

	// AS - NA 11.2 Excel Style Filtering
	#region DelegateCommand
	internal class DelegateCommand : ICommand
	{
		#region Member Variables

		private Predicate<object> _canExecute;
		private Action<object> _execute;

		#endregion //Member Variables

		#region Constructor
		internal DelegateCommand(Action<object> execute, Predicate<object> canExecute)
		{
			Utilities.ValidateNotNull(execute, "execute");
			_execute = execute;
			_canExecute = canExecute;
		}

		internal DelegateCommand(Action<object> execute)
			: this(execute, null)
		{
		}
		#endregion //Constructor

		#region ICommand Members
		bool ICommand.CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute(parameter);
		}

		event EventHandler ICommand.CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		void ICommand.Execute(object parameter)
		{
			_execute(parameter);
		}
		#endregion //ICommand Members
	} 
	#endregion //DelegateCommand
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