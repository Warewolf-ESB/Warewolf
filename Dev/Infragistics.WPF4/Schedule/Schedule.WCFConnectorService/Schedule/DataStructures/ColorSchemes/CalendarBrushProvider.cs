using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;


using Infragistics.Services;
using Infragistics.Collections.Services;
using Infragistics.Controls.Primitives.Services;
using Infragistics.Controls.Schedules.Services;
using Infragistics.Controls.Schedules.Primitives.Services;

namespace Infragistics.Controls.Schedules.Primitives.Services


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

{
	#region CalendarBrushProvider class
	/// <summary>
	/// Class which exposes brush properties which are used by the visual elements of the shedule controls
	/// </summary>
	public sealed class CalendarBrushProvider : PropertyChangeNotifier
	{
		#region Member variables

		private Color _baseColor;
 
		private CalendarColorScheme _manager;

		private Brush[] _brushCache;
		
		// assigned counts by owner are maintained by the CalendarColorScheme to make sure
		// the standard providers are allocated evenly and to know when to cleanup a provider
		private WeakDictionary<object, int> _ownerAssignmentCounts = new WeakDictionary<object,int>(true, false);
		
		internal int _overallAssignCountSnapshot;
	   
		private int _standardSlotIndex;

		private int _brushVersion;

		private static int s_brushIdCount;
		private static CalendarBrushId[] s_brushIds;

		internal static readonly Brush TransparentBrush;

		#endregion Member variables

		#region Constructor

		static CalendarBrushProvider()
		{
			s_brushIds = ScheduleUtilities.GetEnumValues<CalendarBrushId>().ToArray();
			s_brushIdCount = s_brushIds.Length;


			TransparentBrush = Brushes.Transparent;



		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal CalendarBrushProvider( CalendarColorScheme manager, Color baseColor )
		{
			this._manager           = manager;
			this._baseColor         = baseColor;

			this.InvalidateBrushCache();
		}


		#endregion Constructor

		#region Properties

		#region Public Properties

		#region BaseColor
		/// <summary>
		/// Color used as the base color from which all other color
		/// properties exposed by this class are derived.
		/// </summary>
		/// <seealso cref="CalendarBrushProvider"/>
		public Color BaseColor
		{
			get { return this._baseColor; }
		}
		#endregion BaseColor

		#region BrushVersion

		/// <summary>
		/// Returns the version of the brush cache. 
		/// </summary>
		/// <remarks>
		/// <para class="body">When something changes that invalidates the cache, e.g. the <see cref="OfficeColorScheme"/> is changed then this number is incremented.</para>
		/// </remarks>
		[ReadOnly(true)]
		[Bindable(true)]
		public int BrushVersion
		{
			get { return this._brushVersion; }
		 }

		#endregion //BrushVersion

		#region BrushProvider

		/// <summary>
		/// Identifies the BrushProvider attached dependency property
		/// </summary>
		/// <seealso cref="GetBrushProvider"/>
		/// <seealso cref="SetBrushProvider"/>
		public static readonly DependencyProperty BrushProviderProperty = DependencyPropertyUtilities.RegisterAttached("BrushProvider",
			typeof(CalendarBrushProvider), typeof(CalendarBrushProvider),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Gets the value of the attached BrushProvider DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="BrushProviderProperty"/>
		/// <seealso cref="SetBrushProvider"/>
		public static CalendarBrushProvider GetBrushProvider(DependencyObject d)
		{
			return (CalendarBrushProvider)d.GetValue(CalendarBrushProvider.BrushProviderProperty);
		}

		/// <summary>
		/// Sets the value of the attached BrushProvider DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="BrushProviderProperty"/>
		/// <seealso cref="GetBrushProvider"/>
		public static void SetBrushProvider(DependencyObject d, CalendarBrushProvider value)
		{
			d.SetValue(CalendarBrushProvider.BrushProviderProperty, value);
		}

		#endregion //BrushProvider
		
		#endregion //Public Properties
		
		#region Internal Properties

		#region StandardSlotIndex

		// if this is a standard base color it will be set with the index of the slot. If it is
		// a custom base color this property will return -1
		internal int StandardSlotIndex 
		{ 
			get { return this._standardSlotIndex; } 
			set { this._standardSlotIndex = value; }
		}

		#endregion //StandardSlotIndex	
	
		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region GetBrush

		/// <summary>
		/// Returns a brush based on the specified Id.
		/// </summary>
		/// <param name="id">The id of the brush</param>
		/// <returns></returns>
		/// <seealso cref="CalendarBrushId"/>
		public Brush GetBrush(CalendarBrushId id)
		{
			// see if there is already a brush in the cache
			Brush br = this._brushCache[(int)id];

			// if so return it
			if (br != null)
				return br;




			// update the brush cache
			this._brushCache[(int)id] = br;

			return br;
		}

		#endregion //GetBrush

		#endregion //Public Methods	

		#region Internal Methods

		#region ClearAllOwners

		internal void ClearAllOwners()
		{
			this._ownerAssignmentCounts.Clear();
		}

		#endregion //ClearAllOwners

		#region DecrementOwnerAssignmentCount

		internal void DecrementOwnerAssignmentCount(object owner)
		{
			this.SetOwnerAssignmentCount(owner, this.GetOwnerAssignmentCount(owner) - 1);
		}

		#endregion //DecrementOwnerAssignmentCount

		#region GetAllOwners
    
    	internal IEnumerable<object> GetAllOwners()
		{
			return _ownerAssignmentCounts.Keys;
		}

   		#endregion //GetAllOwners	
    
		#region GetOwnerAssignmentCount

		internal int GetOwnerAssignmentCount(object owner)
		{
			int count;

			if (_ownerAssignmentCounts.TryGetValue(owner, out count))
				return count;

			return 0;
		}

		#endregion //GetOwnerAssignmentCount

		#region GetTotalAssignmentCount

		internal int GetTotalAssignmentCount()
		{
			int total = 0;

			foreach (int ownerCount in _ownerAssignmentCounts.Values)
				total += ownerCount;

			return total;
		}

		#endregion //GetTotalAssignmentCount	

		#region IncrementOwnerAssignmentCount

		internal void IncrementOwnerAssignmentCount(object owner)
		{
			this.SetOwnerAssignmentCount(owner, this.GetOwnerAssignmentCount(owner) + 1);
		}

		#endregion //IncrementOwnerAssignmentCount

		#region InvalidateBrushCache

		internal void InvalidateBrushCache()
		{
			this._brushCache = new Brush[s_brushIdCount];

			this.BumpBrushVersion();
		}

		#endregion //InvalidateBrushCache

		#region RemoveOwner

		internal void RemoveOwner(object owner)
		{
			if ( _ownerAssignmentCounts.ContainsKey(owner ))
				_ownerAssignmentCounts.Remove(owner);
		}

		#endregion //RemoveOwner

		#region SetOwnerAssignmentCount

		internal void SetOwnerAssignmentCount(object owner, int count)
		{
			_ownerAssignmentCounts[owner] = count;
		}

		#endregion //SetOwnerAssignmentCount

		#region SnapshotOverallAssignCount

		internal void SnapshotOverallAssignCount()
		{
			_overallAssignCountSnapshot = this.GetTotalAssignmentCount();
		}

		#endregion //SnapshotOverallAssignCount	
    
		#endregion //Internal Methods

		#region Private Methods

		#region BumpBrushVersion

		private void BumpBrushVersion()
		{
			this._brushVersion++;
			this.RaisePropertyChangedEvent("BrushVersion");
			this.RaisePropertyChangedEvent("Count");
			this.RaisePropertyChangedEvent("Item[]");

		}

		#endregion //BumpBrushVersion	

		#endregion //Private Methods

		#endregion //Methods

		/// <summary>
		/// Indexer takes a string or a <see cref="CalendarBrushId"/>
		/// </summary>
		public object this[object key]
		{
			get
			{
				CalendarBrushId id;
				if (key is CalendarBrushId)
					id = (CalendarBrushId)key;
				else
				{
					string strKey = key as string;

					if (strKey == null || !Enum.TryParse<CalendarBrushId>(strKey, out id))
					{
						IConvertible convertible = key as IConvertible;

						if (convertible == null)
							throw new KeyNotFoundException();

						int keyInt;
						try { keyInt = convertible.ToInt32(null); }
						catch (Exception e) { throw new KeyNotFoundException(ScheduleUtilities.GetString("LE_BrushIdNotFound"), e); } // "CalendarBrushId not found"

						if (keyInt >= 0 && keyInt < s_brushIdCount)
							id = (CalendarBrushId)keyInt;
						else
							throw new KeyNotFoundException();
							
					}
				}

				return this.GetBrush(id);
			}
		}

		/// <summary>
		/// Indexer takes a string that can be converted into a <see cref="CalendarBrushId"/>
		/// </summary>
		public object this[string key]
		{
			get
			{
				return this[(object)key];
			}
		}
	}
	#endregion CalendarBrushProvider class
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