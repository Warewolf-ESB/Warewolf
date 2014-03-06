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
using System.Collections;
using System.Windows.Media;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.ComponentModel;


using Infragistics.Windows.Themes;




#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;
using Infragistics.Controls.Primitives;
using Infragistics.Controls.Schedules.Primitives;
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules

{

	#region CalendarColorScheme class

	/// <summary>
	/// Handles coordination of CalendarBrushCaches for ResourceCalendars.
	/// </summary>
	public abstract class CalendarColorScheme : DependencyObject, ISupportPropertyChangeNotifications

		, IResourceWasherTarget // AS 2/23/12 TFS102032

	{
		#region Member variables

		// a dictionary to keep track of active provider assignments. The key will be the BrushProviderToken supplied by
		// the ResourceCalendar. Since both the dictionary and the ResourceCalendar hold it via a weak reference
		// it is up to the using control to hold hard references to the BrushProviderTokens for each of the
		// ResourceCalendars that they are displaying
		private WeakDictionary<object, CalendarBrushProvider> _tokenProviderMap = new WeakDictionary<object, CalendarBrushProvider>(true, false);
		
		// contains all of the ResourceCalendars that have providers assigned to them
		private WeakSet<ResourceCalendar> _assignedCalendars = new WeakSet<ResourceCalendar>();

		// JJD 4/7/11 - TFS61028
		// Added a weak list of ResourceCalendars to keep track of the original assignment order
		private WeakList<ResourceCalendar> _assignmentOrder = new WeakList<ResourceCalendar>();

		// This list is maintained between BeginProviderAssignments and EndProviderAssignment. It is sorted so that it
		// can be used to evenly spread provider assignments over all the standard base colors
		private List<StandardSlotInfo>_standardBaseColorSlots;


		CalendarResourceProvider _dateNavigatorResourceProvider;


		// AS 1/5/11 NA 11.1 Activity Categories
		// Moved to threadstatic member of ScheduleUtilities.
		//
		//private WeakDictionary<Color, Brush> _solidColorBrushCache = new WeakDictionary<Color, Brush>(false, true);

		private PropertyChangeListenerList _propChangeListeners;

		private static bool g_cachedIsHighContrastDirty = true;
		private static bool g_cachedIsHighContrast;
		private static WeakSet<CalendarColorScheme> g_colorSchemes;

		private object _assignmentOwner;
		private bool _isHighContrast;


		private bool _areDialogResourcesInitialized;
		private ResourceDictionary _dialogResources;

		// AS 3/9/12 TFS102032
		// We need a layer in between so that we can change the resources after 
		// they have been handed out.
		//
		private ResourceDictionary _actualDialogResources;


		// AS 2/23/12 TFS102032

		private ResourceWasher _resourceWasher; 

		private ReadOnlyCollection<Color> _baseColorsResolved;

		#endregion Member variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the CalendarColorScheme class.
		/// </summary>
		protected CalendarColorScheme()
		{
			lock (g_colorSchemes)
				g_colorSchemes.Add(this);

			SetIsHighContrastInternal(IsHighContrastStatic);
		}

		[System.Security.SecuritySafeCritical] // AS 10/17/11 TFS89764
		static CalendarColorScheme()
		{

			g_colorSchemes = new WeakSet<CalendarColorScheme>();

			try
			{
				

				

				InitUserPreferenceChanged();
			}
			catch {}

		}
		
		#endregion Constructor

		#region Properties
		
		#region Pubic Properties

		#region BaseColors
		/// <summary>
		/// Returns a read-only collection of the base colors that are supported.
		/// </summary>
		/// 
		public abstract ReadOnlyCollection<Color> BaseColors { get; }

		#endregion BaseColors

		#region BrushVersion

		/// <summary>
		/// Identifies the property key for read-only <see cref="BrushVersion"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey BrushVersionPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"BrushVersion",
			typeof(int),
			typeof(CalendarColorScheme),
			0, 
			new PropertyChangedCallback(OnBrushVersionChanged)
		);

		/// <summary>
		/// Identifies the read-only <see cref="BrushVersion"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty BrushVersionProperty = BrushVersionPropertyKey.DependencyProperty;

		private void SetBrushVersionInternal(int version)
		{
			this.SetValue(BrushVersionPropertyKey, version);
		}

		/// <summary>
		/// For internal use only
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never)]
		[Browsable(false)]
		[ReadOnly(true)]

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		public int BrushVersion
		{
			get
			{
				return (int)this.GetValue(BrushVersionProperty);
			}
		}

		private static void OnBrushVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarColorScheme item = (CalendarColorScheme)d;
		   
			ScheduleUtilities.NotifyListenersHelper(item, e, item.PropChangeListeners, false, false);
		}

		#endregion // BrushVersion

		#region DateNavigatorResourceProvider


		/// <summary>
		/// Returns an appropriate resource provider to be used by the <see cref="XamDateNavigator"/> (read-only)/>
		/// </summary>
		/// <seealso cref="XamDateNavigator"/>

		[InfragisticsFeature(FeatureName = "XamCalendar", Version = "11.1")]

		public virtual CalendarResourceProvider DateNavigatorResourceProvider
		{
			get
			{
				if (_dateNavigatorResourceProvider == null)
				{
					_dateNavigatorResourceProvider = new CalendarResourceProvider();
				}

				return _dateNavigatorResourceProvider;
			}
		}

		#endregion //DateNavigatorResourceProvider

		#region DefaultBrushProvider

		/// <summary>
		/// Identifies the property key for read-only <see cref="DefaultBrushProvider"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey DefaultBrushProviderPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"DefaultBrushProvider",
			typeof(CalendarBrushProvider),
			typeof(CalendarColorScheme),
			null, 
			new PropertyChangedCallback(OnDefaultBrushProviderChanged)
		);

		/// <summary>
		/// Identifies the read-only <see cref="DefaultBrushProvider"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DefaultBrushProviderProperty = DefaultBrushProviderPropertyKey.DependencyProperty;

		private void SetDefaultBrushProviderInternal(CalendarBrushProvider provider)
		{
			this.SetValue(DefaultBrushProviderPropertyKey, provider);
		}

		/// <summary>
		/// Returns the default provider. (read-only).
		/// </summary>
		public CalendarBrushProvider DefaultBrushProvider
		{
			get
			{
				return this.GetValue(DefaultBrushProviderProperty) as CalendarBrushProvider;
			}
		}

		private static void OnDefaultBrushProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarColorScheme item = (CalendarColorScheme)d;

			ScheduleUtilities.NotifyListenersHelper(item, e, item.PropChangeListeners, false, false);
		}

		#endregion // DefaultBrushProvider

		#region DialogResources

		/// <summary>
		/// Returns a collection of resources to be merged into the dialogs (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>:</para> it is the responsiblity of the <see cref="XamScheduleDataManager.DialogFactory"/> to make use of these resources.
		/// </remarks>
		public ResourceDictionary DialogResources 
		{ 
			get 
			{
				// AS 3/14/12 TFS104787
				// SL doesn't allow you to add the same RD instance to the MergedDictionaries of 
				// multiple RD's so we need to create a new one each time it is asked.
				//


#region Infragistics Source Cleanup (Region)








































#endregion // Infragistics Source Cleanup (Region)

				if (!this._areDialogResourcesInitialized)
				{
					this._areDialogResourcesInitialized = true;
					this._dialogResources = this.CreateDialogResources();

					// In WPF we need to hack around a bug in the framework. If a new ResourceDictionary
					// is set on an element either as its Resources property or merged into that
					// ResourcesDictionary's Merged collection, it will take a hard reference on the 
					// inheritance context. This would have the effect in this instance of rooting the
					// first dialog that is displayed that merges this distionary in. 
					// So to get around the problem we create a dummy FrameworkElement and
					// set its resorces collection to _dialgResources. This will just end up rooting
 					// this lightweight element
					if (this._dialogResources != null)
					{
						FrameworkElement feDummy = new FrameworkElement();
						feDummy.Resources = this._dialogResources;
					}

				}

				this.VerifyDialogResources();

				// AS 3/9/12 TFS102032
				// We want an extra layer so even if the resources changed (or wash color), elements that 
				// are referencing the DialogResources returned previously will be updated.
				//
				if (_actualDialogResources == null)
				{
					_actualDialogResources = new ResourceDictionary();


					FrameworkElement feDummy = new FrameworkElement();
					feDummy.Resources = this._actualDialogResources;


					// AS 3/19/12 TFS105110
					this.VerifyActualDialogResources();
				}

				// AS 3/9/12 TFS102032
				//return this._dialogResources; 
				return _actualDialogResources;

			} 
		}

		#endregion //DialogResources	
	
		#region IsHighContrast

		/// <summary>
		/// Identifies the property key for read-only <see cref="IsHighContrast"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey IsHighContrastPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"IsHighContrast",
			typeof(bool),
			typeof(CalendarColorScheme),
			KnownBoxes.FalseBox, 
			new PropertyChangedCallback(OnIsHighContrastChanged)
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsHighContrast"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsHighContrastProperty = IsHighContrastPropertyKey.DependencyProperty;

		private void SetIsHighContrastInternal(bool isHighContrast)
		{
			this.SetValue(IsHighContrastPropertyKey, isHighContrast);
		}

		/// <summary>
		/// Gets whether controls should be rendered in high contrast based on the current system settings (read-only).
		/// </summary>
		/// <remarks>The conditions to determine this are consistent with Office2007 and Office2010.</remarks>
		[Browsable(false)]
		[ReadOnly(true)]

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		public bool IsHighContrast
		{
			get
			{
				return this._isHighContrast;
			}
		}

		private static void OnIsHighContrastChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarColorScheme item = (CalendarColorScheme)d;

			item._isHighContrast = (bool)e.NewValue;
		   
			ScheduleUtilities.NotifyListenersHelper(item, e, item.PropChangeListeners, false, false);
		}

		#endregion // IsHighContrast

		#endregion //Pubic Properties	
	
		#region Protected Properties

		#region DefaultBaseColor
		/// <summary>
		/// Returns the base color to use for the default provider.
		/// </summary>
		protected virtual Color DefaultBaseColor { get { return this.BaseColors[0]; } }

		#endregion DefaultBaseColor

		#region ScrollBarStyle
		/// <summary>
		/// Returns a scrollbar style or null (read-only).
		/// </summary>
		internal protected virtual Style ScrollBarStyle { get { return null; } }

		#endregion ScrollBarStyle

		#endregion //Protected Properties

		#region Internal Properties

		// AS 2/23/12 TFS102032
		// For CalendarColorScheme we're just going to wash 
		// the base colors and let the default calculations for 
		// each brush id be processed as it was before.
		//
		#region BaseColorsResolved
		internal ReadOnlyCollection<Color> BaseColorsResolved
		{
			get
			{
				if (_baseColorsResolved == null)
				{
					_baseColorsResolved = this.BaseColors;


					
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


				}

				return _baseColorsResolved;
			}
		}
		#endregion //BaseColorsResolved

		#region PropChangeListeners






		internal PropertyChangeListenerList PropChangeListeners
		{
			get
			{
				if (null == _propChangeListeners)
					_propChangeListeners = new PropertyChangeListenerList();

				return _propChangeListeners;
			}
		}

		#endregion // PropChangeListeners

		#endregion //Internnal Properties	
	
		#region Private Properties

		#region IsHighContrastStatic

		/// <summary>
		/// Gets whether the system should render controls with high contrast.
		/// </summary>
		/// <remarks>The conditions to determine this are consistant with Office2007.</remarks>
		private static bool IsHighContrastStatic
		{
			get
			{
				if (g_cachedIsHighContrastDirty)
				{
					g_cachedIsHighContrastDirty = false;
					g_cachedIsHighContrast = SystemParameters.HighContrast ||
						(SystemColors.ControlColor == Colors.Black && SystemColors.ControlTextColor == Colors.White) ||
						(SystemColors.ControlColor == Colors.White && SystemColors.ControlTextColor == Colors.Black);
				}

				return g_cachedIsHighContrast;
			}
		}

		#endregion //IsHighContrastStatic

		#endregion //Private Properties	
	
		#endregion //Properties

		#region Methods

		#region Public Methods

		#region InvalidateBrushCache

		/// <summary>
		/// Clears all cached brushes and bumps the BrushVersion property
		/// </summary>
		public void InvalidateBrushCache()
		{
			_baseColorsResolved = null; // AS 2/23/12 TFS102032

			this.VerifyDefaultBrushProvider();

			this.VerifyDialogResources();

			// AS 3/9/12 TFS102032
			// If we have a resource washer we really don't have a good way of knowing when things are added to the 
			// RD or any of its descendant merged dictionaries. Anyway, in all likelihood if this has a resource 
			// washer then there are also washed resources for the things in the dialog within the app's resources.
			//
			// AS 3/14/12 TFS104787
			// For SL we cannot return a singleton instance of the RD because SL doesn't allow multiple RD's to 
			// reference the same shared RD.
			//
			// AS 3/19/12 TFS105110
			// Moved to a helper method since we are also going to conditionally include the scrollbar style
			// instead of letting the datamanager always include it.
			// 
			// AS 3/19/12 TFS105110
			this.VerifyActualDialogResources();

			if (this._tokenProviderMap.Count > 0)
			{
				// Invalidate the cache from all the providers
				foreach (CalendarBrushProvider provider in this._tokenProviderMap.Values)
					provider.InvalidateBrushCache();
			}

			this.BumpBrushVersion();
		}

		#endregion //InvalidateBrushCache

		#endregion //Public Methods	
	
		#region Protected Methods

		#region CreateBrush


		/// <summary>
		/// Creates a Brush based on a specific id
		/// </summary>
		/// <param name="id">The id of the brush to create</param>
		/// <param name="baseColor">The base color to use</param>
		/// <returns>The brush to se for the specified id</returns>
		/// <seealso cref="CalendarBrushId"/>
		internal protected virtual Brush CreateBrush(CalendarBrushId id, Color baseColor)
		{
			Color? color = null;
			GradientStopCollection stops = null;
			Point ptStart = new Point(0, 0);
			Point ptEnd = new Point(0, 1);

			this.GetBrushInfo(baseColor, id, ref color, ref stops, ref ptStart, ref ptEnd);

#pragma warning disable 219
			bool isNewBrush = false;
#pragma warning restore 219

			Brush br = null;

			if (stops != null)
			{
				LinearGradientBrush lgbr = new LinearGradientBrush(stops, 0.0d);

				lgbr.StartPoint = ptStart;
				lgbr.EndPoint = ptEnd;

				br = lgbr;

				isNewBrush = true;
			}
			else
			{
				if (color != null)
				{
					// AS 1/5/11 NA 11.1 Activity Categories
					//this._solidColorBrushCache.TryGetValue(color.Value, out br);
					//
					//if (br == null)
					//{
					//    br = new SolidColorBrush(color.Value);
					//
					//    this._solidColorBrushCache[color.Value] = br;
					//
					//    isNewBrush = true;
					//}
					br = ScheduleUtilities.GetBrush(color.Value);
				}
			}

			// AS 3/9/12 TFS102032
			// We'll wash the actual brushes instead of the base colors since 
			// the colors may be hard coded - i.e. not based on the base colors.
			//
			if (_resourceWasher != null)
			    br = (Brush)_resourceWasher.CreateWashedResource(br);


			if (isNewBrush)
				br.Freeze();


			return br;

		}



		#endregion //CreateBrush

		#region CreateDialogResources

		/// <summary>
		/// Creates a ResourceDictionary that contains resources to be used by the dialogs
		/// </summary>
		/// <returns>A ResourceDictionary or null. The default implmentation returns null.</returns>
		protected virtual ResourceDictionary CreateDialogResources() { return null; }

		#endregion //CreateDialogResources	
    
		#region OnSystemColorsChanged

		/// <summary>
		/// Called when the system colors have changed
		/// </summary>
		protected virtual void OnSystemColorsChanged()
		{
		}

		#endregion //OnSystemColorsChanged	

		#region VerifyDialogResources

		/// <summary>
		/// Makes sure that the dialog resources dictonary is polpulated correctly
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: this method is always called from the get of DialogResources property. This gives derived classes an opportunity to update the contents before a dialog is shown.</para>
		/// </remarks>
		protected virtual void VerifyDialogResources() {}

		#endregion //VerifyDialogResources

		#endregion //Protected Methods

		#region Internal Methods

		#region AssignProvider






		internal void AssignProvider(ResourceCalendar resourceCalendar)
		{
			CoreUtilities.ValidateNotNull(resourceCalendar, "resourceCalendar");

			if (this._assignmentOwner == null)
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_BrushProvider_3"));

			CoreUtilities.ValidateNotNull(resourceCalendar, "resourceCalendar");

			object token = resourceCalendar.BrushProviderToken;

			Color? baseColor = resourceCalendar.BaseColor;
			CalendarBrushProvider providerToAssign = null;

			int standardSlotIndex = -1;

			bool isCustomColor = false;
			bool useNextAvailableSlot = false;

			ReadOnlyCollection<Color> baseColors = this.BaseColorsResolved;


			if (baseColor.HasValue)
			{
				standardSlotIndex = baseColors.IndexOf(baseColor.Value);

				isCustomColor = standardSlotIndex < 0;
			}

			// see if the token is already in our map
			if (!resourceCalendar.NeedsNewBrushProvider)
				this._tokenProviderMap.TryGetValue(token, out providerToAssign);

			if (providerToAssign != null)
			{
				standardSlotIndex = providerToAssign.StandardSlotIndex;
			}
			else
			{
				if (!isCustomColor)
				{
					// snapshot the total assignment counts so the comparer doesn't need
					// to re-calculate on each compare
					foreach (StandardSlotInfo slinfo in this._standardBaseColorSlots)
					{
						if (slinfo.Provider != null)
							slinfo.Provider.SnapshotOverallAssignCount();
					}

					// sort the providers so that the next slot to assign is the
					// first element in the list
					this._standardBaseColorSlots.Sort(CompareStandardSlotInfos);

					standardSlotIndex = this._standardBaseColorSlots[0].SlotIndex;
					providerToAssign = this._standardBaseColorSlots[0].Provider;

					Debug.Assert(standardSlotIndex >= 0, "Invalid SlotIndex inside AssignProvider");

					if (standardSlotIndex < 0)
						standardSlotIndex = 0;

					baseColor = baseColors[standardSlotIndex];

					// set a flag so we know the update the table entry with a newly created 
					// provider below
					useNextAvailableSlot = true;
				}
			}

			if (providerToAssign == null)
			{
				// enumerate the currently assigned providers to find the one with this base color
				foreach (CalendarBrushProvider provider in this._tokenProviderMap.Values)
				{
					// when we find an existing provider that has the correct base color then stop
					if (provider.BaseColor == baseColor.Value)
					{
						providerToAssign = provider;
						break;
					}
				}
			}
			
			// if there wasn't an existing provider then create a new one
			if (providerToAssign == null)
			{
				providerToAssign = new CalendarBrushProvider(this, baseColor.Value);

				// set the slot index
				providerToAssign.StandardSlotIndex = standardSlotIndex;

				if ( useNextAvailableSlot )
				{
					// update the first table entry with the new provider 
					StandardSlotInfo info = this._standardBaseColorSlots[0];

					Debug.Assert(standardSlotIndex == info.SlotIndex, "Invalid slot index when updating table in AssignProvider");

					info.Provider = providerToAssign;

					this._standardBaseColorSlots[0] = info;
				}
			}

			// bump the assigned count
			providerToAssign.IncrementOwnerAssignmentCount(_assignmentOwner);

			// update our WeakDictionary of assigned providers
			this._tokenProviderMap[token] = providerToAssign;

			// update our WeakSet of assigned calendars
			this._assignedCalendars.Add(resourceCalendar);
			
			// JJD 4/7/11 - TFS61028
			// Added a weak list of ResourceCalendars to keep track of the original assignment order
			this._assignmentOrder.Add(resourceCalendar);

			// finally assign the provider to the resource Calendar
			resourceCalendar.InitializeBrushProvider(providerToAssign);
		}

		#endregion AssignProvider
	
		#region BeginProviderAssigments

		internal void BeginProviderAssigments(object owner)
		{
			CoreUtilities.ValidateNotNull(owner, "owner");

			if (this._assignmentOwner != null)
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_BrushProvider_1"));

			this._assignmentOwner = owner;

			ReadOnlyCollection<Color> baseColors = this.BaseColorsResolved;

			Color defaultBaseColor = this.DefaultBaseColor;

			int baseColorsCount = baseColors.Count;

			// enumerate all assined resource calendars and reset the assignecd count each provider. 
			// Note: the AssignedCount will get recalclated below.
			// Note: we drive this with the _assignedCalendars list in case a control was unloaded
			// then the BrushProviderToken might have been garbage collected and removed from
			// the _tokenProviderMap
			foreach (ResourceCalendar calendar in this._assignedCalendars)
			{
				CalendarBrushProvider provider = calendar.BrushProvider;

				if (provider != null)
				{
					int index = provider.StandardSlotIndex;

					// make sure the StandardSlotIndex is correct
					if ( index < 0 || index >= baseColorsCount || provider.BaseColor != baseColors[index])
						provider.StandardSlotIndex = baseColors.IndexOf(provider.BaseColor);

					// clear the assignment count for this owner
					provider.SetOwnerAssignmentCount(_assignmentOwner, 0);
				}
			}

			Dictionary<Color, CalendarBrushProvider> existingProviderMap = new Dictionary<Color, CalendarBrushProvider>();

			// enumerate the currently assigned providers and create a dictionary of all
			// the standard base color providers keyed by there base color. This is to
			// optimize the logic below that initializes the _standardBaseColorSlots list
			foreach (KeyValuePair<object, CalendarBrushProvider> entry in this._tokenProviderMap)
			{
				CalendarBrushProvider provider = entry.Value;

				// Bump the AssignedCount that was cleared above
				if ( entry.Key != this )
					provider.IncrementOwnerAssignmentCount(_assignmentOwner);
	
				// ignore any providers for custom base colors
				if (provider.StandardSlotIndex >= 0)
					existingProviderMap[provider.BaseColor] = provider;
			}

			this._standardBaseColorSlots = new List<StandardSlotInfo>();

			// Create a list of slot infos which will be sorted inside the AssignProvider
			// method to ensure that provider evenly allocated 
			for (int i = 0; i < baseColorsCount; i++)
			{
				CalendarBrushProvider provider;

				existingProviderMap.TryGetValue(baseColors[i], out provider);

				StandardSlotInfo slotinfo = new StandardSlotInfo();
				slotinfo.SlotIndex = i;
				slotinfo.Provider = provider;
				slotinfo.IsDefault = baseColors[i] == defaultBaseColor;
				this._standardBaseColorSlots.Add(slotinfo);

			}
		}

		#endregion //BeginProviderAssigments

		#region EndProviderAssigments

		internal void EndProviderAssigments()
		{
			if (this._assignmentOwner == null)
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_BrushProvider_2"));

			this._standardBaseColorSlots = null;

			List<object> tokensToRemove = null;

			// enumerate the assigned prooviders so we can remove any entries where the assigned count
			// is zero
			foreach (KeyValuePair<object, CalendarBrushProvider> entry in this._tokenProviderMap)
			{
				// bypass the default provider entry since we don't want to remove it 
				if (entry.Key == this)
					continue;

				if (entry.Value.GetTotalAssignmentCount() < 1)
				{
					if (tokensToRemove == null)
						tokensToRemove = new List<object>();

					tokensToRemove.Add(entry.Key);
				}
			}

			// remove the zero assigned entries from the dictionary
			if (tokensToRemove != null)
			{
				foreach (object token in tokensToRemove)
					this._tokenProviderMap.Remove(token);
			}

			List<ResourceCalendar> calendarsToRemove = null;

			// enumerate the assigned calendars so we can remove any entries that
			// don't have an assigned provider
			foreach (ResourceCalendar resourceCalendar in this._assignedCalendars)
			{
				if ( !this._tokenProviderMap.ContainsKey(resourceCalendar.BrushProviderToken))
				{
					if (calendarsToRemove == null)
						calendarsToRemove = new List<ResourceCalendar>();

					calendarsToRemove.Add(resourceCalendar);
				}
			}

			// remove the calenars that didn't have an assigned provider
			if (calendarsToRemove != null)
			{
				foreach (ResourceCalendar resourceCalendar in calendarsToRemove)
				{
					this._assignedCalendars.Remove(resourceCalendar);

					// clear the BrushProvider property on the ResourceCalendar
					resourceCalendar.InitializeBrushProvider(null);
				}
			}

			this._assignmentOwner = null;
		}

		#endregion //EndProviderAssigments	

		// JJD 6/17/11 - TFS74180 - added
		#region OnResourceItemsChanged

		internal void OnResourceItemsChanged(ResourceCollection resources)
		{
			// JJD 6/17/11 - TFS74180
			// See if any of the old resource calendars with provider assignments are still in the collection.
			// If none are then we can clear our cached maps so that base color assignment order
			// can be consistent when all of the resource items have been changed out.
			if (resources == null || _assignedCalendars == null || _assignedCalendars.Count == 0)
				return;

			// Loop over the assigned calendars and if any of their owning resources are still
			// in the resource collection then return
			if (resources.Count > 0)
			{
				foreach (ResourceCalendar rc in _assignedCalendars)
				{
					Resource res = rc != null ? rc.OwningResource : null;
					if (res != null && resources.Contains(res))
						return;
				}
			}

			// Since no currently assigned calendar's owing resource is still
			// in the collection we can clear our cached maps
			if (_tokenProviderMap != null)
				_tokenProviderMap.Clear();

			if (_assignedCalendars != null)
				_assignedCalendars.Clear();

			if (_assignmentOrder != null)
				_assignmentOrder.Clear();
		}

		#endregion //OnResourceItemsChanged	
    
		// JJD 10/29/10 - NA 2011 Volume 1 - Added IGTheme support
		// Moved up from OfficeColorSchemeBase
		#region GetBrushInfo



		internal virtual void GetBrushInfo(Color baseColor, CalendarBrushId id, ref Color? color, ref GradientStopCollection stops, ref Point gradientStart, ref Point gradientEnd)
		{
			bool isBlackThemeDefault = this.IsBlackThemeDefault(baseColor);
			bool isSilverThemeDefault = this.IsSilverThemeDefault(baseColor);

			switch (id)
			{
				case CalendarBrushId.NavigationButtonForeground: // AS 12/10/10 NA 11.1 - XamOutlookCalendarView
					if ( this.IsHighContrast )
					{
						color = SystemColors.WindowTextColor;
					}
					else
					{
						stops = new GradientStopCollection();
						gradientStart = new Point(0, 0);
						gradientEnd = new Point(.5, .75);
						stops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0x84, 0x86, 0x88), Offset = 0 });
						stops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0x58, 0x59, 0x5a), Offset = 1 });
					}
					break;
				case CalendarBrushId.NavigationButtonBackground: // AS 12/10/10 NA 11.1 - XamOutlookCalendarView
					color = Colors.Transparent;
					break;
				case CalendarBrushId.NavigationButtonHoverBackground: // AS 12/10/10 NA 11.1 - XamOutlookCalendarView
					if ( this.IsHighContrast )
					{
						color = SystemColors.HighlightColor;
					}
					else
					{
						stops = new GradientStopCollection();
						gradientStart = new Point(.5, 0);
						gradientEnd = new Point(.5, 1);
						stops.Add(new GradientStop { Color = Color.FromArgb(255,219,225,231), Offset = 0 });
						stops.Add(new GradientStop { Color = Color.FromArgb(255,208,217,227), Offset = 1 });
					}
					break;
				case CalendarBrushId.NavigationButtonPressedBackground: // AS 12/10/10 NA 11.1 - XamOutlookCalendarView
					if ( this.IsHighContrast )
					{
						color = SystemColors.HighlightColor;
					}
					else
					{
						stops = new GradientStopCollection();
						gradientStart = new Point(.5, 0);
						gradientEnd = new Point(.5, 1);
						stops.Add(new GradientStop { Color = Color.FromArgb(255, 187, 197, 208), Offset = 0 });
						stops.Add(new GradientStop { Color = Color.FromArgb(255, 219, 225, 231), Offset = 1 });
					}
					break;
				case CalendarBrushId.MultiDayActivityAreaBackground:
				case CalendarBrushId.AlternateMonthDayBackground:
					if (this.IsHighContrast)
						this.GetBrushInfo(baseColor, CalendarBrushId.DayBackground, ref color, ref stops, ref gradientStart, ref gradientEnd);
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.0952f);
					break;
				case CalendarBrushId.AppointmentBackground:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
					{
						gradientStart.X = .5;
						gradientStart.Y = 0;
						gradientEnd.X = .5;
						gradientEnd.Y = 1;
						stops = new GradientStopCollection();
						GradientStop stop = new GradientStop();
						stop.Color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.308007f, 1.041993f);
						//stop.Color = this.ColorFromBaseColor(baseColor, 1.458007f);
						stop.Offset = 0;
						stops.Add(stop);
						GradientStop stop2 = new GradientStop();
						stop2.Color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.151786f);
						stop2.Offset = 1;
						stops.Add(stop2);
					}
					break;
				case CalendarBrushId.AppointmentBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlLightLightColor;
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.002992f, 1.002992f);
					break;
				case CalendarBrushId.CalendarBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlDarkColor;
					else
					{
						
						
						

						
						color = baseColor;
					}
					break;
				
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

				case CalendarBrushId.CalendarHeaderBackground:
					if (this.IsHighContrast)
						color = SystemColors.ControlColor;
					else
					{
						stops = new GradientStopCollection();
						GradientStop stop = new GradientStop();
						stop.Color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.1786f);
						stop.Offset = 0;
						stops.Add(stop);
						GradientStop stop2 = new GradientStop();
						stop2.Color = baseColor;
						stop2.Offset = 1;
						stops.Add(stop2);
					}
					break;
				
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

				case CalendarBrushId.CurrentDayBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Color.FromArgb(255, 238, 147, 17);
					break;
				case CalendarBrushId.CurrentDayHeaderBackground:
					if (this.IsHighContrast)
						color = SystemColors.ActiveCaptionColor;
					else
					{
						stops = new GradientStopCollection();
						GradientStop stop = new GradientStop();
						stop.Color = Color.FromArgb(255, 241, 159, 36);
						stop.Offset = 0;
						stops.Add(stop);
						GradientStop stop2 = new GradientStop();
						stop2.Color = Color.FromArgb(255, 246, 197, 95);
						stop2.Offset = 1;
						stops.Add(stop2);
					}
					break;

				case CalendarBrushId.CurrentDayHeaderForeground:
					if (this.IsHighContrast)
						color = SystemColors.ActiveCaptionTextColor;
					else
						color = Colors.Black;
					return;
				//case CalendarBrushId.CurrentDayHeaderHotTrackingBackground:
				//    if (this.IsHighContrast)
				//        color = SystemColors.ActiveCaptionColor;
				//    else
				//    {
				//        color = Color.FromArgb(255, 241, 159, 36);
				//        float h = ScheduleUtilities.GetHue(color.Value);
				//        float s = ScheduleUtilities.GetSaturation(color.Value);
				//        float l = ScheduleUtilities.GetBrightness(color.Value);

				//        //	I fudged this to de-emphasize the effect, it was too much otherwise
				//        l *= 1.125f;
				//        color = ScheduleUtilities.ColorFromHLS(255, h, l, s);

				//        stops = new GradientStopCollection();
				//        GradientStop stop = new GradientStop();
				//        stop.Color = color.Value;
				//        stop.Offset = 0;
				//        stops.Add(stop);

				//        color = Color.FromArgb(255, 246, 197, 95);
				//        h = ScheduleUtilities.GetHue(color.Value);
				//        s = ScheduleUtilities.GetSaturation(color.Value);
				//        l = ScheduleUtilities.GetBrightness(color.Value);

				//        //	I fudged this to de-emphasize the effect, it was too much otherwise
				//        l *= 1.125f;
				//        color = ScheduleUtilities.ColorFromHLS(255, h, l, s);

				//        GradientStop stop2 = new GradientStop();
				//        stop2.Color = color.Value;
				//        stop2.Offset = 1;
				//        stops.Add(stop2);
				//    }
				//    break;

				case CalendarBrushId.CurrentDayBorderMonthCalendar:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Color.FromArgb(255, 187, 85, 3);
					break;

				case CalendarBrushId.CurrentTimeIndicatorBorder:
					if (this.IsHighContrast)
						color = Colors.Transparent;
					else
						color = Color.FromArgb(255, 255, 216, 90);
					break;

				case CalendarBrushId.CurrentTimeIndicatorBackground:
					if (this.IsHighContrast)
						color = SystemColors.InactiveCaptionColor;
					else
						color = Color.FromArgb(255, 235, 137, 0);
					break;

				case CalendarBrushId.DayBackground:
					if (this.IsHighContrast)
						color = SystemColors.ControlColor;
					else
						color = Colors.White;
					break;

				case CalendarBrushId.DayBorder:
				case CalendarBrushId.WeekHeaderBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, .8214f);
					break;
				
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

				case CalendarBrushId.WeekHeaderBackground:
				case CalendarBrushId.DayHeaderBackground:
					if (this.IsHighContrast)
						color = SystemColors.ControlColor;
					else
					{
						stops = new GradientStopCollection();
						GradientStop stop = new GradientStop();
						stop.Color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.2024f);
						stop.Offset = 0;
						stops.Add(stop);
						GradientStop stop2 = new GradientStop();
						stop2.Color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.3274f);
						stop2.Offset = 1;
						stops.Add(stop2);
					}
					break;

				case CalendarBrushId.DayHeaderForground:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Colors.Black;
					break;


				case CalendarBrushId.JournalBackground:
				case CalendarBrushId.TaskBackground:
					if (this.IsHighContrast)
						this.GetBrushInfo(baseColor, CalendarBrushId.DayBackground, ref color, ref stops, ref gradientStart, ref gradientEnd);
					else
					{
						stops = new GradientStopCollection();
						GradientStop stop = new GradientStop();
						stop.Color = Color.FromArgb(255, 255, 246, 206);
						stop.Offset = 0;
						stops.Add(stop);
						GradientStop stop2 = new GradientStop();
						stop2.Color = Color.FromArgb(255, 250, 205, 110);
						stop2.Offset = 1;
						stops.Add(stop2);
					}
					break;

				case CalendarBrushId.JournalBorder:
				case CalendarBrushId.TaskBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlDarkColor;
					else
						color = Color.FromArgb(255, 180, 128, 2);
					break;

				case CalendarBrushId.MoreActivityIndicatorFill:
					// doesn't seem to change in high contrast
					color = Color.FromArgb(255, 190, 190, 190);
					break;
				case CalendarBrushId.MoreActivityIndicatorStroke:
					// doesn't seem to change in high contrast
					color = Colors.White;
					break;
				case CalendarBrushId.HotTrackingMoreActivityIndicatorFill:
					if (this.IsHighContrast)
						color = SystemColors.HighlightColor;
					else
						color = Color.FromArgb(255, 247, 190, 87);
					break;
				
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

				case CalendarBrushId.NonWorkingHourTimeslotBackground:
					if (this.IsHighContrast)
						this.GetBrushInfo(baseColor, CalendarBrushId.DayBackground, ref color, ref stops, ref gradientStart, ref gradientEnd);
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.3333f);
					break;

				case CalendarBrushId.NonWorkingHourTimeslotMinorBorder:
					if (this.IsHighContrast)
						this.GetBrushInfo(baseColor, CalendarBrushId.WorkingHourTimeslotMinorBorder, ref color, ref stops, ref gradientStart, ref gradientEnd);
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.2738f);
					break;
				
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

				case CalendarBrushId.SelectedDayBackgroundMonthView:
					if (this.IsHighContrast)
						this.GetBrushInfo(baseColor, CalendarBrushId.DayBackground, ref color, ref stops, ref gradientStart, ref gradientEnd);
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, 1.3333f);
					break;
				
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

				case CalendarBrushId.SelectedMultiDayActivityAreaBackground:
				case CalendarBrushId.SelectedTimeslotBackground:
					if (this.IsHighContrast)
						color = SystemColors.ControlLightLightColor;
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, .4583f);
					break;

				case CalendarBrushId.TimeslotMajorBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlDarkColor;
					else
						this.GetBrushInfo(baseColor, CalendarBrushId.AlternateMonthDayBackground, ref color, ref stops, ref gradientStart, ref gradientEnd);
					break;

				case CalendarBrushId.TimeslotHeaderTickmarkDayView:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Color.FromArgb(255, 184, 184, 184);
					break;

				case CalendarBrushId.TimeslotHeaderAreaSeparator:
					if (this.IsHighContrast)
						color = SystemColors.WindowTextColor;
					else
						color = Color.FromArgb(255, 59, 59, 59);
					break;

				// AS 7/21/10 TFS36040
				// I separated these from the block below that is using ControlTextColor since 
				// at least these are using window color in high contrast.
				//
				case CalendarBrushId.TimeslotHeaderForegroundDayView:
				case CalendarBrushId.TimeslotHeaderForegroundScheduleView:
				case CalendarBrushId.TimeslotHeaderTickmarkScheduleView:
					if (this.IsHighContrast)
						color = SystemColors.WindowTextColor;
					else
						color = Color.FromArgb(255, 30, 57, 91);
					break;

				// AS 7/21/10 TFS36040
				// Added background area brushes.
				//
				case CalendarBrushId.BlockingErrorBackground:
				case CalendarBrushId.TimeslotHeaderAreaBackgroundDayView:
				case CalendarBrushId.TimeslotHeaderAreaBackgroundScheduleView:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
						color = Colors.White;
					break;

				case CalendarBrushId.BlockingErrorHeaderForeground:
					if (this.IsHighContrast)
						color = SystemColors.WindowTextColor;
					else
						color = Colors.Red;
					break;

				case CalendarBrushId.BlockingErrorForeground:
					if (this.IsHighContrast)
						color = SystemColors.WindowTextColor;
					else
						color = Colors.Black;
					break;

				case CalendarBrushId.TimeslotDayBorder:
				case CalendarBrushId.AppointmentForeground:
				case CalendarBrushId.JournalForeground:
				case CalendarBrushId.TaskForeground:
				case CalendarBrushId.CalendarHeaderForeground:
				case CalendarBrushId.WeekHeaderForeground:
				case CalendarBrushId.SelectedActivityBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Colors.Black;
					break;

				case CalendarBrushId.AppointmentForegroundOverlayed:
					if (this.IsHighContrast)
					{
						color = SystemColors.ControlTextColor;
						color = Color.FromArgb(156, color.Value.R, color.Value.G, color.Value.B);
					}
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, 0.9129934f);

					break;

				case CalendarBrushId.MonthViewBackground:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
						color = baseColor;

					break;

				case CalendarBrushId.MonthViewDayOfWeekHeaderBackground:
					color = Colors.Transparent;
					break;

				case CalendarBrushId.MonthViewDayOfWeekHeaderBorder:
				case CalendarBrushId.MonthViewDayOfWeekHeaderForeground:
					if (this.IsHighContrast)
						color = SystemColors.WindowTextColor;
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, 0.5789977f, 0.9939911f);
					break;

				case CalendarBrushId.AppointmentDateTimeForeground:
				case CalendarBrushId.JournalDateTimeForeground:
				case CalendarBrushId.TaskDateTimeForeground:
					if (this.IsHighContrast)
					{
						color = SystemColors.ControlTextColor;
						color = Color.FromArgb(156, color.Value.R, color.Value.G, color.Value.B);
					}
					else
						color = ScheduleUtilities.ColorFromBaseColor(baseColor, 0.4579993f, 0.998991f);

					break;

				case CalendarBrushId.JournalForegroundOverlayed:
				case CalendarBrushId.TaskForegroundOverlayed:
					if (this.IsHighContrast)
					{
						color = SystemColors.ControlTextColor;
						color = Color.FromArgb(156, color.Value.R, color.Value.G, color.Value.B);
					}
					else
						color = Color.FromArgb(255, 156, 156, 156);

					break;

				case CalendarBrushId.ToolTipBackground:
					if (this.IsHighContrast)
						color = SystemColors.ControlColor;
					else
					{
						stops = new GradientStopCollection();
						GradientStop stop = new GradientStop();
						stop.Color = Color.FromArgb(255, 252, 253, 254);
						stop.Offset = 0;
						stops.Add(stop);
						GradientStop stop2 = new GradientStop();
						stop2.Color = Color.FromArgb(255, 206, 221, 240);
						stop2.Offset = 1;
						stops.Add(stop2);
					}
					break;

				case CalendarBrushId.ToolTipBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Color.FromArgb(255, 118, 118, 118);
					break;

				case CalendarBrushId.ToolTipErrorForeground:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Colors.Red;
					break;

				case CalendarBrushId.ToolTipForeground:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Color.FromArgb(255, 36, 36, 36);
					break;

				case CalendarBrushId.WorkingHourTimeslotBackground:
					if (this.IsHighContrast)
						color = SystemColors.ControlColor;
					else
						//	Same for all schemes
						color = Colors.White;
					break;

				case CalendarBrushId.WorkingHourTimeslotMinorBorder:
					if (this.IsHighContrast)
						this.GetBrushInfo(baseColor, CalendarBrushId.TimeslotMajorBorder, ref color, ref stops, ref gradientStart, ref gradientEnd);
					else
						this.GetBrushInfo(baseColor, CalendarBrushId.SelectedDayBackgroundMonthView, ref color, ref stops, ref gradientStart, ref gradientEnd);
					break;

				// AS 6/13/12 TFS105402
				case CalendarBrushId.ActivityCategoryColorPickerDropDownBackground:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
						color = Colors.White;
					break;

				// AS 6/13/12 TFS105402
				case CalendarBrushId.ActivityCategoryColorPickerDropDownBorder:
					if (this.IsHighContrast)
						color = SystemColors.WindowFrameColor;
					else
						color = Colors.Black;
					break;

				case CalendarBrushId.DialogBackground:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
						color = Color.FromArgb(255, 207, 221, 238);

					break;

				case CalendarBrushId.DialogForeground:
					if (this.IsHighContrast)
						color = SystemColors.WindowTextColor;
					else
						color = Colors.Black;

					break;

				case CalendarBrushId.RibbonLiteBackgroundBrush:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
					{
						gradientStart.X = 0;
						gradientStart.Y = 0;
						gradientEnd.X = 0;
						gradientEnd.Y = 1;
						stops = new GradientStopCollection();

						GradientStop stop = new GradientStop();
						stop.Color = Color.FromArgb(255, 225, 234, 246);
						stop.Offset = 0;
						stops.Add(stop);

						GradientStop stop2 = new GradientStop();
						stop2.Color = Color.FromArgb(255, 199, 215, 234);
						stop2.Offset = 1;
						stops.Add(stop2);
					}

					break;

				case CalendarBrushId.RibbonLiteGroupOuterBorderBrush:
					if (this.IsHighContrast)
						color = SystemColors.ActiveBorderColor;
					else
					{
						gradientStart.X = 0;
						gradientStart.Y = 0;
						gradientEnd.X = 0;
						gradientEnd.Y = 1;
						
						stops = new GradientStopCollection();

						GradientStop stop = new GradientStop();
						stop.Color = Color.FromArgb(50, 107, 121, 138);
						stop.Offset = 0;
						stops.Add(stop);

						GradientStop stop2 = new GradientStop();
						stop2.Color = Color.FromArgb(255, 107, 121, 138);
						stop2.Offset = 1;
						stops.Add(stop2);
					}

					break;

				case CalendarBrushId.RibbonLiteGroupInnerBorderBrush:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
					{
						gradientStart.X = 0;
						gradientStart.Y = 0;
						gradientEnd.X = 0;
						gradientEnd.Y = 1;

						stops = new GradientStopCollection();

						GradientStop stop = new GradientStop();
						stop.Color = Color.FromArgb(50, 255, 255, 255);
						stop.Offset = 0;
						stops.Add(stop);

						GradientStop stop2 = new GradientStop();
						stop2.Color = Color.FromArgb(255, 255, 255, 255);
						stop2.Offset = 1;
						stops.Add(stop2);
					}

					break;

				// AS 4/15/11 NA 11.1 - CalendarHeaderAreaWidth
				case CalendarBrushId.ResizerBarPreviewBackground:
					if (this.IsHighContrast)
						color = SystemColors.HighlightColor;
					else
						color = Color.FromArgb(180, 0x80, 0x80, 0x80);
					break;
			}
		}



		#endregion //GetBrushInfo

		// JJD 10/29/10 - NA 2011 Volume 1 - Added IGTheme support
		#region IsBlackThemeDefault

		internal virtual bool IsBlackThemeDefault(Color color)
		{
			return false;
		}

		#endregion //IsBlackThemeDefault	
    
		// JJD 10/29/10 - NA 2011 Volume 1 - Added IGTheme support
		#region IsSilverThemeDefault

		internal virtual bool IsSilverThemeDefault(Color color)
		{
			return false;
		}

		#endregion //IsSilverThemeDefault	

		// AS 3/19/12 TFS105110
		// Added helper method so derived classes could know when the resource washer has changed.
		//
		#region OnResourceWasherChanged
		internal virtual void OnResourceWasherChanged()
		{
			this.InvalidateBrushCache();
		}
		#endregion //OnResourceWasherChanged
    
		#region TransferProviderAssignmentsFrom

		internal void TransferProviderAssignmentsFrom(CalendarColorScheme source)
		{
			this.VerifyDefaultBrushProvider();

			if (source == null)
				return;

			try
			{
				// build a temp map of rcs keyed by their tokens
				Dictionary<object, ResourceCalendar> tokenRCMap = new Dictionary<object,ResourceCalendar>();
				foreach (ResourceCalendar rc in source._assignedCalendars)
					tokenRCMap[rc.BrushProviderToken] = rc;
				
				// build a map of rcs keyed by their assigned brush providers
				Dictionary<CalendarBrushProvider, ResourceCalendar> providerRCMap = new Dictionary<CalendarBrushProvider,ResourceCalendar>();
				foreach (KeyValuePair<object, CalendarBrushProvider> entry in source._tokenProviderMap)
				{
					ResourceCalendar rc;

					if ( tokenRCMap.TryGetValue(entry.Key, out rc))
						providerRCMap[entry.Value] = rc;
				}
				
				Dictionary<object, List<ResourceCalendar>> ownerRCs = new Dictionary<object, List<ResourceCalendar>>();

				// walk over the token provider map again
				// to get the Resource calendars and create a list for all rcs used by each owner
				foreach (KeyValuePair<object, CalendarBrushProvider> entry in source._tokenProviderMap)
				{
					ResourceCalendar rc;
					CalendarBrushProvider provider = entry.Value;
					if (provider == null || !providerRCMap.TryGetValue(provider, out rc))
						continue;

					if ( rc != null)
					{
						// for each owner mainatin a list of all the rcs it makes use of
						foreach (object owner in entry.Value.GetAllOwners())
						{
							if (owner != null)
							{
								List<ResourceCalendar> rcs;

								if (!ownerRCs.TryGetValue(owner, out rcs))
								{
									rcs = new List<ResourceCalendar>();
									ownerRCs[owner] = rcs;
								}

								rcs.Add(rc);
							}
						}
					}
				}

				// JJD 4/7/11 - TFS61028 
				// Instead of assigning the providers in a random order we need to sort the
				// list of ResourceCalendar and assign the providers in the same order
 				// that they were originally assigned which will ensure a consistent
				// assignment of providers between color scheme changes.
				//
				// walk over the owner rc list map and assign all the resources for each owner
				//foreach (KeyValuePair<object, List<ResourceCalendar>> entry in ownerRCs)
				//{
				//    if (entry.Key != null)
				//    {
				//        this.BeginProviderAssigments(entry.Key);

				//        try
				//        {
				//            foreach (ResourceCalendar rc in entry.Value)
				//                this.AssignProvider(rc);

				//        }
				//        finally
				//        {
				//            this.EndProviderAssigments();
				//        }
				//    }
				//}
				
				// JJD 4/7/11 - TFS61028
				// Compact the original assignment order weak list to remove any released entries
				this._assignmentOrder.Compact();

				// JJD 4/7/11 - TFS61028
				// walk over the owner rc list map and create a list of the ResourceCalendars
				// that need to be assigned a provider
				List<OwnerCalendar> ownerCalendarList = new List<OwnerCalendar>();
				foreach (KeyValuePair<object, List<ResourceCalendar>> entry in ownerRCs)
				{
					if (entry.Key != null)
					{
						foreach (ResourceCalendar rc in entry.Value)
							ownerCalendarList.Add(new OwnerCalendar(entry.Key, rc, _assignmentOrder.IndexOf(rc)));
					}
				}

				// JJD 4/7/11 - TFS61028
				// If there is more than 1 ResourceCalendar to assign a provider then
				// sort the list so it is in the same order as those ResourceCalendars
				// were originally assigned
				if (ownerCalendarList.Count > 1)
					ownerCalendarList.Sort(new OwnerCalendarComparer());


				// JJD 4/7/11 - TFS61028
				// Now that we have them in the correct order we can reassign them
				// in that order
				foreach (OwnerCalendar ownerCal in ownerCalendarList)
				{
					this.BeginProviderAssigments(ownerCal.Owner);
					try
					{
						this.AssignProvider(ownerCal.Calendar);

					}
					finally
					{
						this.EndProviderAssigments();
					}
				}

				// finally clear the owner references on the old providers
				foreach (CalendarBrushProvider provider in source._tokenProviderMap.Values)
					provider.ClearAllOwners();
			}
			finally
			{
				source._assignedCalendars.Clear();
				source._tokenProviderMap.Clear();
				source.VerifyDefaultBrushProvider();
			}

		}

		#endregion //TransferProviderAssignmentsFrom	
	
		#region UnassignProvider






		internal void UnassignProvider(ResourceCalendar resourceCalendar)
		{
			CoreUtilities.ValidateNotNull(resourceCalendar, "resourceCalendar");

			if (this._assignmentOwner == null)
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_BrushProvider_3"));

			object token = resourceCalendar.BrushProviderToken;

			CalendarBrushProvider provider;
			this._tokenProviderMap.TryGetValue(token, out provider);

			
			

			// decrement the provider's assigned count
			if (provider != null)
				provider.DecrementOwnerAssignmentCount(_assignmentOwner);

		}

		#endregion UnassignProvider

		#region VerifyDefaultBrushProvider

		internal void VerifyDefaultBrushProvider()
		{
			bool hasDefaultProviderChanged = false;

			Color defaultBaseColor = this.DefaultBaseColor;

			CalendarBrushProvider provider;

			// for the default provider we use 'this' as the token in the map
			if (this._tokenProviderMap.TryGetValue(this, out provider))
			{
				// make sure the base color hasn't changed
				if (provider.BaseColor != defaultBaseColor)
				{
					provider = null;
				}
			}

			if (provider == null)
			{
				provider = new CalendarBrushProvider(this, defaultBaseColor);

				this._tokenProviderMap[this] = provider;

				hasDefaultProviderChanged = true;
			}

			if (hasDefaultProviderChanged)
				SetDefaultBrushProviderInternal(provider);

		}

		#endregion VerifyDefaultBrushProvider

		#endregion //Internal Methods

		#region Private Methods

		#region BumpBrushVersion

		private void BumpBrushVersion()
		{
			this.SetBrushVersionInternal(this.BrushVersion + 1);
		}

		#endregion //BumpBrushVersion	

		#region CompareStandardSlotInfos

		private int CompareStandardSlotInfos(StandardSlotInfo x, StandardSlotInfo y)
		{

			CalendarBrushProvider providerX = x.Provider;
			CalendarBrushProvider providerY = y.Provider;

			int assignedCountX = providerX != null ? providerX._overallAssignCountSnapshot : 0;
			int assignedCountY = providerY != null ? providerY._overallAssignCountSnapshot : 0;

			if (assignedCountX < assignedCountY)
				return -1;

			if (assignedCountX > assignedCountY)
				return 1;

			if (x.IsDefault != y.IsDefault)
				return x.IsDefault ? -1 : 1;

			if (x.SlotIndex < y.SlotIndex)
				return -1;

			if (x.SlotIndex > y.SlotIndex)
				return 1;

			return 0;
		}

		#endregion //CompareStandardSlotInfos

		#region InitUserPreferenceChanged (private)

		[System.Security.SecurityCritical()] // AS 10/17/11 TFS89764
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void InitUserPreferenceChanged()
		{
			Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(OnUserPreferenceChanged);

		}

		#endregion InitUserPreferenceChanged

		#region OnSystemColorsChangedInternal

		private void OnSystemColorsChangedInternal()
		{
			if (!this.CheckAccess())
			{
				// marshall this call onto the dispatcher's thread
				this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.OnSystemColorsChangedInternal));
				return;
			}

			SetIsHighContrastInternal(IsHighContrastStatic);

			this.InvalidateBrushCache();

			this.OnSystemColorsChanged();

		}

		#endregion //OnSystemColorsChangedInternal	
	
		#region OnUserPreferenceChanged


		[System.Security.SecurityCritical] // AS 10/17/11 TFS89764
		private static void OnUserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
		{
			if (e.Category == Microsoft.Win32.UserPreferenceCategory.Color)
			{
				g_cachedIsHighContrastDirty = true;

				// copy our static instances into a stack list for processsing below
				List<CalendarColorScheme> managers;
				lock (g_colorSchemes)
					managers = new List<CalendarColorScheme>(g_colorSchemes);

				foreach (CalendarColorScheme manager in managers)
					manager.OnSystemColorsChangedInternal();

			}
		}


		#endregion //OnUserPreferenceChanged

		// AS 3/19/12 TFS105110
		#region ShouldDialogIncludeScrollBarStyle
		private bool ShouldDialogIncludeScrollBarStyle()
		{
			bool includeScrollBar = true;


			if (_resourceWasher != null)
			{
				var appResources = Application.Current != null ? Application.Current.Resources : null;

				if (appResources == null || appResources[typeof(ScrollBar)] != null)
					includeScrollBar = false;
			}


			return includeScrollBar;
		} 
		#endregion //ShouldDialogIncludeScrollBarStyle

		#region VerifyActualDialogResources
		private void VerifyActualDialogResources()
		{

			if (_actualDialogResources != null)
			{
				// AS 3/19/12 TFS105110
				// We are now leaving it up to the color scheme to include the scrollbar style.
				// Like the dialog resources we will only include it if there is no resource 
				// washer.
				// 

				if (!this.ShouldDialogIncludeScrollBarStyle())
					_actualDialogResources.Remove(typeof(ScrollBar));
				else
				{
					var sb = this.ScrollBarStyle;

					if (sb != null)
						_actualDialogResources[typeof(ScrollBar)] = sb;
				}

				if (_dialogResources != null)
				{
					if (_resourceWasher == null && !_actualDialogResources.MergedDictionaries.Contains(_dialogResources))
						_actualDialogResources.MergedDictionaries.Add(_dialogResources);
					else if (_resourceWasher != null && _actualDialogResources.MergedDictionaries.Contains(_dialogResources))
						_actualDialogResources.MergedDictionaries.Remove(_dialogResources);
				}
			}

		}
		#endregion //VerifyActualDialogResources

		#endregion //Private Methods

		#endregion //Methods

		#region ISupportPropertyChangeNotifications Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener(ITypedPropertyChangeListener<object, string> listener, bool useWeakReference)
		{
			this.PropChangeListeners.Add(listener, useWeakReference);
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener(ITypedPropertyChangeListener<object, string> listener)
		{
			this.PropChangeListeners.Remove(listener);
		}

		#endregion // ISupportPropertyChangeNotifications Implementation

		#region StandardSlotInfo private struct

		private struct StandardSlotInfo
		{
			internal bool IsDefault { get; set; }
			internal int SlotIndex { get; set; }
			internal CalendarBrushProvider Provider { get; set; }
		}

		#endregion //StandardSlotInfo private struct

		// JJD 4/7/11 - TFS61028 - added
		#region OwnerCalendar private struct

		private struct OwnerCalendar
		{
			private object _owner;
			private ResourceCalendar _calendar;
			private int _allocationIndex;

			internal OwnerCalendar(object owner, ResourceCalendar calendar, int allocationIndex)
			{
				_owner = owner;
				_calendar = calendar;
				_allocationIndex = allocationIndex;
			}

			internal int AallocationIndex { get { return _allocationIndex; } }
			internal object Owner { get { return _owner; } }
			internal ResourceCalendar Calendar { get { return _calendar; } }
		}

		#endregion //OwnerCalendar private struct	
    
		// JJD 4/7/11 - TFS61028 - added
		#region OwnerCalendarComparer

		private class OwnerCalendarComparer : IComparer<OwnerCalendar>
		{
			#region IComparer<OwnerCalendar> Members

			public int Compare(OwnerCalendar x, OwnerCalendar y)
			{
				int xIndex = x.AallocationIndex;
				int yIndex = y.AallocationIndex;

				if (xIndex < yIndex)
					return -1;

				if (xIndex > yIndex)
					return 1;

				return string.Compare(x.Calendar.Name, y.Calendar.Name);
			}

			#endregion
		}

		#endregion //OwnerCalendarComparer	
    		
		// AS 2/23/12 TFS102032
		#region IResourceWasherTarget

		ResourceWasher IResourceWasherTarget.ResourceWasher
		{
			get
			{
				return _resourceWasher;
			}
			set
			{
				// note i'm not checking for a difference because it's possible
				// that the color/mode of the washer is being changed so we want 
				// to dirty
				_resourceWasher = value;
				this.OnResourceWasherChanged();
			}
		} 

		#endregion //IResourceWasherTarget
	}

	#endregion CalendarColorScheme class

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