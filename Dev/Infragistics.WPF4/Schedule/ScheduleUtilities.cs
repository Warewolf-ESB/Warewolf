using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Schedules.Primitives;
using System.Windows.Controls;
using System.Collections.Specialized;
using Infragistics.Collections;
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules
{
	internal partial class ScheduleUtilities
	{
		#region Constants

		internal const long MaxValueTicks = 0x2bca2875f4373fffL; 

		#endregion // Constants

		#region Nested Data Structures

		// AS 1/5/11 NA 11.1 Activity Categories
		#region ColorPercent struct
		internal struct ColorPercent
		{
			internal Color Color;
			private float _percent;

			internal float Percent
			{
				get { return _percent; }
				set
				{
					if ( _percent < 0 || _percent > 1 )
						throw new ArgumentOutOfRangeException();

					_percent = value;
				}
			}
		}
		#endregion //ColorPercent struct

		#endregion // Nested Data Structures

		#region Methods

		#region AddErrorFromIdHelper

		internal static void AddErrorFromIdHelper( List<DataErrorInfo> errorList, object context, string stringId, params object[] args )
		{
			DataErrorInfo error = DataErrorInfo.CreateError( context, ScheduleUtilities.GetString( stringId, args ) );
			errorList.Add( error );
		}

		#endregion // AddErrorFromIdHelper

		#region AddErrorHelper

		internal static void AddErrorHelper( List<DataErrorInfo> list, DataErrorInfo error, ErrorSeverity? severity = null )
		{
			if ( severity.HasValue )
				error.Severity = severity.Value;

			list.Add( error );
		}

		#endregion // AddErrorHelper

		#region AddListener

		internal static void AddListener(ISupportPropertyChangeNotifications notifier, IPropertyChangeListener listener, bool useWeakReference)
		{
			notifier.AddListener(listener, useWeakReference);
		}

		// This is used by the unit tests because they don't have access to the ISupportPropertyChangeNotifications,
		// which is defined in windows.
		// 
		internal static void AddListenerHelper( object notifier, Action<object, object, string, object> handler, object owner )
		{
			var listener = new PropertyChangeListener<object>( owner, handler );

			if ( notifier is XamScheduleDataManager )
			{
				( (XamScheduleDataManager)notifier ).PropChangeListeners.Add( listener, false );
			}
			else
			{
				ISupportPropertyChangeNotifications n = (ISupportPropertyChangeNotifications)notifier;
				n.AddListener( listener, false );
			}
		}
 
		#endregion // AddListener

		#region AdjustForFlowDirection
		internal static void AdjustForFlowDirection(FlowDirection flowDirection, ref SpatialDirection navigationDirection)
		{
			if (flowDirection == System.Windows.FlowDirection.RightToLeft)
			{
				if (navigationDirection == SpatialDirection.Left)
					navigationDirection = SpatialDirection.Right;
				else if (navigationDirection == SpatialDirection.Right)
					navigationDirection = SpatialDirection.Left;
			}
		} 
		#endregion // AdjustForFlowDirection

		#region BinarySearch

		#region BinarySearch(TimeslotRange[], DateTime)
		internal static int BinarySearch(TimeslotRange[] groupTemplates, DateTime date)
		{
			Func<IList<TimeslotRange>, int, DateRange> callback = ( IList<TimeslotRange> groups, int index) => { return CalculateDateRange(groups, index); };
			return BinarySearchDateRange(groupTemplates, callback, date, GetTimeslotCount(groupTemplates));
		}
		#endregion // BinarySearch(TimeslotRange[], DateTime)

		#region BinarySearch(IList<TimeslotRangeGroup>, DateTime)
		internal static int BinarySearch( IList<TimeslotRangeGroup> groups, DateTime date )
		{
			Func<IList<TimeslotRangeGroup>, int, DateRange> callback = ( IList<TimeslotRangeGroup> items, int index ) => 
			{
				TimeslotRangeGroup group = items[index];
				return new DateRange(group.Ranges.First().StartDate, group.Ranges.Last().EndDate);
			};
			return BinarySearchDateRange(groups, callback, date, groups.Count);
		}
		#endregion // BinarySearch(IList<TimeslotRangeGroup>, DateTime)

		#endregion // BinarySearch

		// AS 1/5/11 NA 11.1 Activity Categories
		#region BlendColors
		internal static Color BlendColors( params ColorPercent[] colors )
		{
			float a = 0;
			float r = 0;
			float g = 0;
			float b = 0;

			foreach ( var item in colors )
			{
				var color = item.Color;
				var pct = item.Percent;

				a += color.A * pct;
				r += color.R * pct;
				g += color.G * pct;
				b += color.B * pct;
			}

			Func<float, byte> constrain = ( float f ) => { return (byte)Math.Max(Math.Min(f, 255), 0); };
			return Color.FromArgb(constrain(a), constrain(r), constrain(g), constrain(b));
		} 
		#endregion // BlendColors

		#region CalculateDateRange
		internal static DateRange CalculateDateRange( IList<TimeslotRange> groupTemplates, int index, Func<DateTime, DateTime> modifyRangeFunc = null )
		{
			for ( int i = 0, iCount = groupTemplates.Count; i < iCount; i++ )
			{
				int count = groupTemplates[i].TimeslotCount;

				if ( count > index )
				{
					TimeslotRange range = groupTemplates[i];
					return CalculateDateRange(range, index, modifyRangeFunc);
				}

				index -= count;
			}

			return DateRange.Infinite;
		}

		internal static DateRange CalculateDateRange( TimeslotRange range, int index, Func<DateTime, DateTime> modifyRangeFunc = null )
		{
			Debug.Assert(index >= 0 && index < range.TimeslotCount);

			DateTime start = range.StartDate;
			TimeSpan tsInterval = range.TimeslotInterval;

			// offset it to the appropriate timeslot start
			start = start.AddTicks(tsInterval.Ticks * index);
			DateTime end = start.Add(tsInterval);

			// make sure its within the range so we don't spill into another date
			if ( end > range.EndDate )
				end = range.EndDate;

			// now do any adjustments
			if ( null != modifyRangeFunc )
			{
				start = modifyRangeFunc(start);
				end = modifyRangeFunc(end);
			}

			return new DateRange(start, end);
		} 
		#endregion // CalculateDateRange

		// AS 1/5/11 NA 11.1 Activity Categories
		#region CalculateForeground
		internal static Color CalculateForeground( Color background, params Color[] foregroundColors )
		{
			// calculate the gamma adjusted brightness
			double backBrightness = CalculateSRgbBrightness(background);

			double previousContrast = -1d;
			Color color = Colors.Transparent;

			foreach ( Color foreground in foregroundColors )
			{
				double brightness = CalculateSRgbBrightness(foreground);
				double contrast = brightness > backBrightness
					? (brightness + 0.05) / (backBrightness + 0.05)
					: (backBrightness + 0.05) / (brightness + 0.05);

				if ( contrast > previousContrast )
				{
					previousContrast = contrast;
					color = foreground;
				}
			}

			return color;
		}

		internal static Color CalculateForeground( Color background )
		{
			// calculate the gamma adjusted brightness
			double backBrightness = CalculateSRgbBrightness(background);

			double whiteContrast = 1.05d / (backBrightness + 0.05);
			double blackContrast = (backBrightness + 0.05) / 0.05;

			return blackContrast > whiteContrast ? Colors.Black : Colors.White;
		} 
		#endregion // CalculateForeground

		// AS 1/5/11 NA 11.1 Activity Categories
		#region CalculateSRgbBrightness
		internal static double CalculateSRgbBrightness( Color color )
		{
			
			
			
			const double Gamma = 2.2;

			// calculate the gamma adjusted brightness
			double brightness =
				(Math.Pow(color.R / 255.0, Gamma) * 0.2126) +
				(Math.Pow(color.G / 255.0, Gamma) * 0.7152) +
				(Math.Pow(color.B / 255.0, Gamma) * 0.0722);

			return brightness;
		} 
		#endregion // CalculateSRgbBrightness

		// AS 1/5/11 NA 11.1 Activity Categories
		// Moved here from CalendarColorScheme. Also removed the overload that doesn't take a saturationFactor and 
		// changed it to an optional parameter and updated the current callers so the parameters were reversed.
		//
		// JJD 10/29/10 - NA 2011 Volume 1 - Added IGTheme support
		// Moved up from OfficeColorSchemeBase
		#region ColorFromBaseColor



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Color ColorFromBaseColor( Color baseColor, float luminosityFactor, float saturationFactor = 1f )
		{
			float hue = ScheduleUtilities.GetHue(baseColor);
			float saturation = ScheduleUtilities.GetSaturation(baseColor);
			float luminosity = ScheduleUtilities.GetBrightness(baseColor);
			saturation *= saturationFactor;
			luminosity *= luminosityFactor;
			Color retVal = ScheduleUtilities.ColorFromHLS(255, (float)hue, (float)luminosity, (float)saturation);
			return retVal;
		}
		#endregion ColorFromBaseColor

		#region ColorFromHLS



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		// JJD 6/08/10 - NA 2010 Vol 3 - Schedule controls
		// Changedfrom private to internal
		internal static Color ColorFromHLS(byte alpha, float hue, float luminance, float saturation)
		{
			int red, green, blue;
			if (saturation == 0.0)
			{
				red = green = blue = (int)(luminance * 255.0);
			}
			else
			{
				float rm1, rm2;

				if (luminance <= 0.5f) rm2 = luminance + luminance * saturation;
				else rm2 = luminance + saturation - luminance * saturation;
				rm1 = 2.0f * luminance - rm2;
				red = (int)ToRGBHelper(rm1, rm2, hue + 120.0f);
				green = (int)ToRGBHelper(rm1, rm2, hue);
				blue = (int)ToRGBHelper(rm1, rm2, hue - 120.0f);
			}

			red = Math.Max(0, Math.Min(255, red));
			green = Math.Max(0, Math.Min(255, green));
			blue = Math.Max(0, Math.Min(255, blue));

			return Color.FromArgb(alpha, (byte)red, (byte)green, (byte)blue);
		}

		#endregion //ColorFromHLS	

		#region CombineDateAndTime
		internal static DateTime CombineDateAndTime(DateTime date, DateTime time)
		{
			return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
		}
		#endregion //CombineDateAndTime	

		#region ConstrainLogicalDayOffset
		internal static TimeSpan ConstrainLogicalDayOffset(TimeSpan logicalDayOffset)
		{
			const long TickBoundary = TimeSpan.TicksPerDay - TimeSpan.TicksPerSecond;

			if (logicalDayOffset.Ticks < -TickBoundary)
				logicalDayOffset = TimeSpan.FromTicks(-TickBoundary);
			else if (logicalDayOffset.Ticks > TickBoundary)
				logicalDayOffset = TimeSpan.FromTicks(TickBoundary);

			return logicalDayOffset;
		}
		#endregion // ConstrainLogicalDayOffset

		#region ConvertDataValue

		internal static bool ConvertDataValue( object value, Type targetType, ConverterInfo converterInfo, 
			object fallbackConverterParameter, CultureInfo fallbackCulture, bool convertingBack, 
			out object convertedValue, out DataErrorInfo dataError )
		{
			convertedValue = null;
			dataError = null;

			IValueConverter converter = null != converterInfo ? converterInfo._converter : null;
			CultureInfo culture = null != converterInfo ? converterInfo._culture : null;
			if ( null == culture )
				culture = fallbackCulture ?? ParseCulture;

			if ( null != converter )
			{
				object parameter = converterInfo._parameter;
				if ( null == parameter )
					parameter = fallbackConverterParameter;

				
				try
				{
					convertedValue = !convertingBack
						? converter.Convert( value, targetType, parameter, culture )
						: converter.ConvertBack( value, targetType, parameter, culture );

					return true;
				}
				catch ( Exception e )
				{
					dataError = new DataErrorInfo( e );
					return false;
				}
			}

			convertedValue = ConvertDataValue( value, targetType, culture, null );
			if ( null != convertedValue )
				return true;

			if ( !IsValueEmpty( value ) )
			{
				dataError = ScheduleUtilities.CreateErrorFromId(value, "LE_CanNotConvertValue", value, targetType); // "Unable to converter {0} value to target type {1}"
				return false;
			}

			return true;
		} 

		#endregion // ConvertDataValue
    
		#region CreateDictionary<TKey, TValue>

		internal static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>( TKey key, TValue value )
		{
			Dictionary<TKey, TValue> map = new Dictionary<TKey, TValue>( );
			map[key] = value;

			return map;
		} 

		#endregion // CreateDictionary<TKey, TValue>

		#region CreateRangeArray
		internal static TimeslotRange[] CreateRangeArray(IList<DateRange> combinedRanges, TimeSpan timeslotInterval)
		{
			TimeslotRange[] ranges = new TimeslotRange[combinedRanges.Count];

			for (int i = 0, count = combinedRanges.Count; i < count; i++)
			{
				ranges[i] = new TimeslotRange(combinedRanges[i].Start, combinedRanges[i].End, timeslotInterval);
			}

			return ranges;
		}
		#endregion //CreateRangeArray

		#region CreateTaskOccurrenceToBeDeleted







		internal static Task CreateTaskOccurrenceToBeDeleted(Task recurringRootTask)
		{
			Task taskOccurrence = new Task();
			taskOccurrence.Id = recurringRootTask.Id;
			taskOccurrence.DataItem = recurringRootTask.DataItem;
			taskOccurrence.RootActivity = recurringRootTask;
			return taskOccurrence;
		} 

		#endregion  // CreateTaskOccurrenceToBeDeleted

		#region EndEdit
		internal static bool EndEdit( XamScheduleDataManager dm, ActivityBase activity, bool force, out DataErrorInfo error )
		{
			error = null;
			Debug.Assert(null != dm && null != activity && activity.IsInEdit);

			if (dm == null || activity == null)
				return false;

			ActivityOperationResult result = dm.EndEdit(activity, force);

			if (null != result && result.IsComplete == false)
				dm.AddPendingOperation(result);

			error = result.Error;

			return !result.IsCanceled;
		} 
		#endregion // EndEdit

		#region FindBlockingError

		internal static DataErrorInfo FindBlockingErrors( DataErrorInfo error )
		{
			List<DataErrorInfo> blockingErrors = new List<DataErrorInfo>( );
			List<DataErrorInfo> nonBlockingErrors = new List<DataErrorInfo>( );

			FindBlockingErrors( error, blockingErrors, nonBlockingErrors );

			if ( blockingErrors.Count > 0 )
			{
				// if there aren't multiple errors then don't wrap it in an error
				if ( blockingErrors.Count == 1 )
					return blockingErrors[0];

				return new DataErrorInfo( blockingErrors );
			}

			return null;
		}

		internal static void FindBlockingErrors( DataErrorInfo error, IList<DataErrorInfo> blockingErrors, IList<DataErrorInfo> nonBlockingErrors )
		{
			if ( null != error )
			{
				// JJD 4/4/11 - TFS69535
				// If this is a list of errors process it first by call this method recursively for
				// each error in the list
				// Otherwise, add the error to either the blocking or non-blocking list
				//if ( ErrorSeverity.SevereError == error.Severity )
				//{
				//    blockingErrors.Add( error );
				//}
				//else
				//{
				//    var list = error.ErrorList;
				//    if ( null != list )
				//    {
				//        foreach ( DataErrorInfo ii in list )
				//            FindBlockingErrors( ii, blockingErrors, nonBlockingErrors );
				//    }
				//}
				var list = error.ErrorList;
				if ( null != list )
				{
					foreach ( DataErrorInfo ii in list )
						FindBlockingErrors( ii, blockingErrors, nonBlockingErrors );
				}
				else
				if ( ErrorSeverity.SevereError == error.Severity )
				{
					blockingErrors.Add( error );
				}
				else
				{
					nonBlockingErrors.Add( error );
				}
			}
		} 

		#endregion // FindBlockingError

		// AS 3/30/11 NA 2011.1 - Date Parsing
		#region FindIndex
		internal static int FindIndex<T>(IList<T> list, Predicate<T> match)
		{
			return FindIndex(list, 0, match);
		}

		internal static int FindIndex<T>(IList<T> list, int startIndex, Predicate<T> match)
		{
			for (int i = startIndex, count = list.Count; i < count; i++)
			{
				if (match(list[i]))
					return i;
			}

			return -1;
		} 
		#endregion //FindIndex

		// AS 3/30/11 NA 2011.1 - Date Parsing
		#region FindLastIndex
		internal static int FindLastIndex<T>(IList<T> list, Predicate<T> match)
		{
			return FindLastIndex(list, list.Count - 1, match);
		}

		internal static int FindLastIndex<T>(IList<T> list, int startIndex, Predicate<T> match)
		{
			for (int i = startIndex; i >= 0; i--)
			{
				if (match(list[i]))
					return i;
			}

			return -1;
		} 
		#endregion //FindLastIndex

		#region FindNearestAfterOrDefault
		/// <summary>
		/// Searches the specified list at the position after the starting point 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list to search</param>
		/// <param name="index">The starting index for the search</param>
		/// <param name="match">The method used to perform the check for which item should be returned</param>
		/// <returns></returns>
		internal static T FindNearestAfterOrDefault<T>(IList<T> list, int index, Predicate<T> match)
		{
			index = IndexOfNearestAfter(list, index, match);

			return index >= 0 ? list[index] : default(T);
		} 
		#endregion // FindNearestAfterOrDefault

		#region FindNextOrDefault
		/// <summary>
		/// Enumerates the list from the specified starting point skipping the specified number of items that match.
		/// </summary>
		/// <typeparam name="T">The type of item in the list</typeparam>
		/// <param name="list">The list to enumerate</param>
		/// <param name="startingIndex">The index of the item at which to start. Note the item itself is not considered even if <paramref name="returnLast"/> is true.</param>
		/// <param name="match">The method to invoke to determine if the item is a match</param>
		/// <param name="next">True to look forward in the list; false to iterate backwards</param>
		/// <param name="returnLast">True to return the last item that matches</param>
		/// <param name="matchCount">The number of matching items that must match</param>
		/// <returns></returns>
		internal static T FindNextOrDefault<T>( IList<T> list, int startingIndex, bool next, int matchCount, bool returnLast, Predicate<T> match )
		{
			int index = IndexOfNext(list, startingIndex, next, matchCount, returnLast, match);

			return index >= 0 ? list[index] : default(T);
		}
		#endregion // FindNextOrDefault

		// AS 1/5/11 NA 11.1 Activity Categories
		#region GetActivityCategoryBrush
		private static WeakDictionary<Color, LinearGradientBrush> _ActivityCategoryBackgroundBrushes;

		internal static Brush GetActivityCategoryBrush( Color baseColor, ActivityCategoryBrushId brushId )
		{
			LinearGradientBrush backBrush;

			switch ( brushId )
			{
				case ActivityCategoryBrushId.Border:
					return GetBrush(ColorFromBaseColor(baseColor, 0.6f, 0.7f));
				case ActivityCategoryBrushId.Foreground:
					return GetBrush(CalculateForeground(baseColor));
				case ActivityCategoryBrushId.Background:
					{
						if ( null == _ActivityCategoryBackgroundBrushes )
							_ActivityCategoryBackgroundBrushes = new WeakDictionary<Color, LinearGradientBrush>(false, true);

						if ( _ActivityCategoryBackgroundBrushes.TryGetValue(baseColor, out backBrush) )
							return backBrush;

						break;
					}
			}

			bool isDark = GetBrightness(baseColor) < 0.5;
			Color gradientEnd, gradientStart;

			if ( isDark )
			{
				gradientEnd = ColorFromBaseColor(baseColor, .80f, 1.05f);
				gradientStart = ColorFromBaseColor(baseColor, 1.125f, .95f);
			}
			else
			{
				gradientEnd = ColorFromBaseColor(baseColor, .95f, 1.25f);
				gradientStart = ColorFromBaseColor(baseColor, 1.05f, .75f);
			}

			if ( brushId == ActivityCategoryBrushId.Background )
			{
				backBrush = new LinearGradientBrush();
				backBrush.StartPoint = new Point(0.5, 0);
				backBrush.EndPoint = new Point(0.5, 1);
				backBrush.GradientStops.Add(new GradientStop { Color = gradientStart, Offset = 0 });
				backBrush.GradientStops.Add(new GradientStop { Color = baseColor, Offset = 0.38 });
				backBrush.GradientStops.Add(new GradientStop { Color = gradientEnd, Offset = 1 });


				backBrush.Freeze();


				_ActivityCategoryBackgroundBrushes[baseColor] = backBrush;
				return backBrush;
			}

			Debug.Assert(brushId == ActivityCategoryBrushId.ForegroundOverlay, "Unexpected brush id:" + brushId.ToString());

			// calculate the foreground when in overlay mode
			Color fore1 = BlendColors(new ColorPercent { Color = Colors.Black, Percent = 0.25f }, new ColorPercent { Color = gradientEnd, Percent = 0.75f });
			Color fore2 = BlendColors(new ColorPercent { Color = Colors.White, Percent = 0.40f }, new ColorPercent { Color = gradientEnd, Percent = 0.60f });
			fore2 = ColorFromBaseColor(fore2, 1f, 0.8f);

			return GetBrush(CalculateForeground(gradientEnd, fore1, fore2));
		} 
		#endregion // GetActivityCategoryBrush

		#region GetCalendarGroupBrushProvider
		internal static CalendarBrushProvider GetCalendarGroupBrushProvider(FrameworkElement element)
		{
			return GetCalendarGroupBrushProvider(element, ScheduleUtilities.GetControl(element));
		}

		internal static CalendarBrushProvider GetCalendarGroupBrushProvider(FrameworkElement element, ScheduleControlBase control)
		{
			if (control != null && control.CalendarDisplayModeResolved == CalendarDisplayMode.Merged)
			{
				return control.DefaultBrushProvider;
			}

			ResourceCalendar calendar = element.DataContext as ResourceCalendar;
			return calendar != null ? calendar.BrushProvider : null;
		} 
		#endregion // GetCalendarGroupBrushProvider

		#region GetBrightness

		internal static float GetBrightness(Color color)
		{
			float redPercent = ((float)color.R) / 255f;
			float greenPercent = ((float)color.G) / 255f;
			float bluePercent = ((float)color.B) / 255f;

			float maxPercent = redPercent > greenPercent ? redPercent : greenPercent;

			if (bluePercent > maxPercent)
				maxPercent = bluePercent;

			float minPercent = redPercent < greenPercent ? redPercent : greenPercent;

			if (bluePercent < minPercent)
				minPercent = bluePercent;

			float sum = maxPercent + minPercent;

			return sum / 2;
		}

		#endregion //GetBrightness

		#region GetActivityTypes
		internal static ActivityTypes GetActivityTypes(ActivityType type)
		{
			return (ActivityTypes)(1 << (int)type);
		} 
		#endregion // GetActivityTypes	

		// AS 1/5/11 NA 11.1 Activity Categories
		#region GetBrush

		[ThreadStatic]
		private static WeakDictionary<Color, SolidColorBrush> _SolidColorBrushTable;

		internal static SolidColorBrush GetBrush( Color color )
		{
			SolidColorBrush brush;

			if ( null == _SolidColorBrushTable )
				_SolidColorBrushTable = new WeakDictionary<Color, SolidColorBrush>(false, true);

			if ( !_SolidColorBrushTable.TryGetValue(color, out brush) )
			{
				brush = new SolidColorBrush(color);


				brush.Freeze();


				_SolidColorBrushTable[color] = brush;
			}

			return brush;
		} 
		#endregion // GetBrush

		#region GetControl

		internal static ScheduleControlBase GetControl(UIElement element)
		{
			ScheduleControlBase ctrl = ScheduleControlBase.GetControl(element);

			if (null == ctrl)
			{
				DependencyObject parent = VisualTreeHelper.GetParent(element);

				if (null != parent)
					ctrl = ScheduleControlBase.GetControl(parent);
			}

			return ctrl;
		} 

		#endregion // GetControl

		#region GetIScheduleControl

		internal static IScheduleControl GetIScheduleControl(UIElement element)
		{
			ScheduleControlBase scb = ScheduleUtilities.GetControl(element);

			if (scb != null)
				return scb;

			XamDateNavigator dn = Infragistics.Controls.Editors.CalendarUtilities.GetCalendar(element) as XamDateNavigator;

			if (dn != null)
				return dn;

			return null;
		}

		#endregion // GetIScheduleControl

		#region GetControlFromElementTree
		internal static ScheduleControlBase GetControlFromElementTree(UIElement element)
		{
			DependencyObject parent = element;

			while (parent != null)
			{
				ScheduleControlBase ctrl = parent as ScheduleControlBase;

				if (null != ctrl)
					return ctrl;

				ToolTip tooltip = parent as ToolTip;

				if (tooltip != null)
					parent = tooltip.PlacementTarget;
				else
				{

					FrameworkElement feParent = parent as FrameworkElement;
					if ( feParent != null )
					{
						DependencyObject templatedParent = feParent.TemplatedParent;

						if ( templatedParent != null)
						{
							parent = templatedParent;
							continue;
						}
					}

					parent = VisualTreeHelper.GetParent(parent);
				}
			}

			return null;
		} 
		#endregion // GetControlFromElementTree

		#region GetDataManager
		internal static XamScheduleDataManager GetDataManager(UIElement element)
		{
			ScheduleControlBase ctrl = GetControl(element);
			return ctrl != null ? ctrl.DataManagerResolved : null;
		} 
		#endregion // GetDataManager

		#region GetDefaultWorkDays

		internal static DayOfWeekFlags GetDefaultWorkDays(XamScheduleDataManager dm)
		{
			if (dm != null && dm.Settings != null && dm.Settings.WorkDays != DayOfWeekFlags.None)
				return dm.Settings.WorkDays;
			else
				return ScheduleSettings.DEFAULT_WORKDAYS;
		}

		#endregion //GetDefaultWorkDays	
    
		#region GetFirstDayOfWeek
		internal static DayOfWeek? GetFirstDayOfWeek(XamScheduleDataManager dm)
		{
			DayOfWeek? firstDayOfWeek = null;

			if (null != dm)
			{
				if (dm.CurrentUser != null)
					firstDayOfWeek = dm.CurrentUser.FirstDayOfWeek;

				if (null == firstDayOfWeek && dm.Settings != null)
					firstDayOfWeek = dm.Settings.FirstDayOfWeek;
			}

			return firstDayOfWeek;
		}
		#endregion // GetFirstDayOfWeek

		#region GetHue

		internal static float GetHue(Color color)
		{
			float redPercent = ((float)color.R) / 255f;
			float greenPercent = ((float)color.G) / 255f;
			float bluePercent = ((float)color.B) / 255f;

			// if they are all the same return 0
			if (redPercent == greenPercent)
			{
				if (redPercent == bluePercent)
					return 0f;
			}

			float maxPercent = redPercent > greenPercent ? redPercent : greenPercent;

			if (bluePercent > maxPercent)
				maxPercent = bluePercent;

			float minPercent = redPercent < greenPercent ? redPercent : greenPercent;

			if (bluePercent < minPercent)
				minPercent = bluePercent;

			float diff = maxPercent - minPercent;
			float hue = 0f;

			if (redPercent == maxPercent)
			{
				hue = ((greenPercent - bluePercent) / diff) * 60;
			}
			else
				if (greenPercent == maxPercent)
				{
					hue = ((bluePercent - redPercent) / diff) * 60;
					hue += 120;
				}
				else
					if (bluePercent == maxPercent)
					{
						hue = ((redPercent - greenPercent) / diff) * 60;
						hue += 240;
					}

			// make sure the calulated value is from 0 to 360
			if (hue < 0f)
				hue += 360f;

			return hue;
		}

		#endregion //GetHue

		#region GetLogicalDayOffset
		internal static TimeSpan GetLogicalDayOffset(ScheduleControlBase ctrl)
		{
			if (ctrl != null)
			{
				var dm = ctrl.DataManagerResolved;

				if (null != dm)
					return dm.LogicalDayOffset;
			}

			return TimeSpan.Zero;
		}
		#endregion // GetLogicalDayOffset

		#region GetLogicalDayRange
		internal static DateRange GetLogicalDayRange(DateTime dateTime, TimeSpan logicalDayOffset)
		{
			return GetLogicalDayRange(dateTime, logicalDayOffset, TimeSpan.FromTicks(TimeSpan.TicksPerDay));
		}

		internal static DateRange GetLogicalDayRange(DateTime dateTime, TimeSpan logicalDayOffset, TimeSpan logicalDayDuration)
		{
			//Debug.Assert(dateTime.Kind != DateTimeKind.Utc, "This should be dealing with local times");

			long _ticksOffset = logicalDayOffset.Ticks;
			long ticksStart = dateTime.Date.Ticks;
			long ticksEnd;

			if (_ticksOffset == 0)
			{
				ticksEnd = ticksStart + logicalDayDuration.Ticks;
			}
			else if (_ticksOffset > 0)
			{
				// shift forward
				ticksStart += _ticksOffset;

				// if the time is before the offset then shift back 1 day
				if (ticksStart > dateTime.Ticks)
				{
					ticksStart -= TimeSpan.TicksPerDay;
					ticksEnd = ticksStart + logicalDayDuration.Ticks;

					if (ticksStart < 0)
						ticksStart = 0;
				}
				else
				{
					ticksEnd = ticksStart + logicalDayDuration.Ticks;
				}
			}
			else //if (_ticksOffset < 0)
			{
				ticksStart += _ticksOffset;

				if (dateTime.Ticks >= ticksStart + TimeSpan.TicksPerDay)
				{
					ticksStart += TimeSpan.TicksPerDay;
					ticksEnd = ticksStart + logicalDayDuration.Ticks;
				}
				else
				{
					ticksEnd = ticksStart + logicalDayDuration.Ticks;
					if (ticksStart < 0)
						ticksStart = 0;
				}
			}

			// AS 4/1/11 TFS64258
			ticksEnd = Math.Min(Math.Max(ticksEnd, 0), MaxValueTicks);

			return new DateRange(new DateTime(ticksStart), new DateTime(ticksEnd));
		} 
		#endregion // GetLogicalDayRange

		#region GetMinMaxRange
		internal static DateRange GetMinMaxRange(XamScheduleDataManager dm)
		{
			ScheduleSettings settings = null != dm ? dm.Settings : null;

			DateInfoProvider dateProvider = null != dm ? dm.DateInfoProviderResolved : DateInfoProvider.CurrentProvider;

			DateRange minMaxRange = settings == null
				? DateRange.Infinite
				
				: new DateRange(settings.MinDate ?? dateProvider.MinSupportedDateTime, settings.MaxDate ?? dateProvider.MaxSupportedDateTime);

			minMaxRange.Normalize();
			minMaxRange.RemoveTime();
			return minMaxRange;
		}
		#endregion // GetMinMaxRange

		#region GetNonInclusiveEnd
		internal static DateTime GetNonInclusiveEnd(DateTime date)
		{
			if (date.Ticks <= TimeSpan.TicksPerSecond)
				return DateTime.MinValue;

			return date.AddSeconds(-1);
		}

		
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		#endregion // GetNonInclusiveEnd	

		#region GetRecurrenceCalculatorHelper

		// SSP 4/14/11
		// 
		/// <summary>
		/// Gets the recurrence calculator.
		/// </summary>
		internal static DateRecurrenceCalculatorBase GetRecurrenceCalculatorHelper( XamScheduleDataManager dm, ActivityBase activity, RecurrenceBase recurrence, DateTime? startOverride = null )
		{
			DateRecurrenceCalculatorBase calculator = null;

			var factory = dm.RecurrenceCalculatorFactory;
			Debug.Assert( null != factory );
			if ( null != factory )
			{
				var recurrenceInfo = ScheduleUtilities.GetRecurrenceInfo( activity, recurrence, dm.DataConnector, startOverride );
				calculator = factory.GetCalculator( recurrenceInfo );
			}

			return calculator;
		}

		#endregion // GetRecurrenceCalculatorHelper

		#region GetSaturation

		internal static float GetSaturation(Color color)
		{
			float redPercent = ((float)color.R) / 255f;
			float greenPercent = ((float)color.G) / 255f;
			float bluePercent = ((float)color.B) / 255f;

			float maxPercent = redPercent > greenPercent ? redPercent : greenPercent;

			if (bluePercent > maxPercent)
				maxPercent = bluePercent;

			float minPercent = redPercent < greenPercent ? redPercent : greenPercent;

			if (bluePercent < minPercent)
				minPercent = bluePercent;

			double diff = maxPercent - minPercent;

			if (diff == 0)
				return 0f;

			double sum = (maxPercent + minPercent);
			double average = sum / 2d;

			if (average > .5d)
				return (float)(diff / (2d - sum));

			return (float)(diff / sum);
		}

		#endregion //GetSaturation	
    
		#region GetIsReminderActive
		internal static bool GetIsReminderActive(ActivityBase activity, XamScheduleDataManager dm)
		{
			if (null != activity && activity.ReminderEnabled)
			{
				var resource = activity.OwningResource;
				if (null != resource)
				{
					if (dm != null && dm.CurrentUser == resource)
						return true;
				}
			}
			return false;
		}
		#endregion // GetIsReminderActive

		#region GetResourceIds

		internal static IEnumerable<string> GetResourceIds( IEnumerable<Resource> resources )
		{
			return null != resources ? from ii in resources select ii.Id : null;
		}

		#endregion // GetResourceIds

		#region GetCalendarGroupFromElement

		internal static CalendarGroupBase GetCalendarGroupFromElement(UIElement element)
		{
			CalendarGroupPresenterBase groupPresenter = PresentationUtilities.GetVisualAncestor<CalendarGroupPresenterBase>(element, null);

			return groupPresenter != null ? groupPresenter.CalendarGroup : null;
		}

		#endregion //GetCalendarGroupFromElement

		#region GetDateInfoProvider

		internal static DateInfoProvider GetDateInfoProvider(ScheduleControlBase control)
		{
			if (control != null)
				return control.DateInfoProviderResolved;

			return DateInfoProvider.CurrentProvider;
		}

		#endregion //GetDateInfoProvider		

		#region GetResourcesByToken
		internal static Dictionary<TimeZoneToken, List<Resource>> GetResourcesByToken(ICollection<Resource> resources, TimeZoneInfoProvider tzProvider, TimeZoneToken defaultToken = null)
		{
			var table = new Dictionary<TimeZoneToken, List<Resource>>();

			if (null == defaultToken)
				defaultToken = tzProvider.LocalToken;

			foreach (var resource in resources)
			{
				// we're going to fallback to the local id
				TimeZoneToken token;
				
				if ( resource == null || !tzProvider.TryGetTimeZoneToken(resource.PrimaryTimeZoneId, out token))
					token = defaultToken;

				if (token == null)
					continue;

				List<Resource> subset;

				if (!table.TryGetValue(token, out subset))
				{
					subset = new List<Resource>();
					table[token] = subset;
				}

				subset.Add(resource);
			}

			return table;
		}
		#endregion // GetResourcesByToken

		#region GetRulesByType
		internal static void GetRulesByType(DateRecurrence recurrence, out List<DayOfWeekRecurrenceRule> weekdayRules,
																out List<DayOfWeekRecurrenceRule> weekendDayRules,
																out List<DayOfMonthRecurrenceRule> dayOfMonthRules,
																out List<MonthOfYearRecurrenceRule> monthOfYearRules,
																out List<SubsetRecurrenceRule> subsetRules,
																out List<DateRecurrenceRuleBase> otherRules)
		{
			weekdayRules = new List<DayOfWeekRecurrenceRule>();
			weekendDayRules = new List<DayOfWeekRecurrenceRule>();
			dayOfMonthRules = new List<DayOfMonthRecurrenceRule>();
			monthOfYearRules = new List<MonthOfYearRecurrenceRule>();
			subsetRules = new List<SubsetRecurrenceRule>();
			otherRules = new List<DateRecurrenceRuleBase>();

			foreach (DateRecurrenceRuleBase rule in recurrence.Rules)
			{
				DayOfWeekRecurrenceRule dowRule = rule as DayOfWeekRecurrenceRule;
				DayOfMonthRecurrenceRule domRule = rule as DayOfMonthRecurrenceRule;
				MonthOfYearRecurrenceRule moyRule = rule as MonthOfYearRecurrenceRule;
				SubsetRecurrenceRule subsetRule = rule as SubsetRecurrenceRule;

				if (dowRule != null)
				{
					if (dowRule.Day == DayOfWeek.Saturday || dowRule.Day == DayOfWeek.Sunday)
						weekendDayRules.Add(dowRule);
					else
						weekdayRules.Add(dowRule);
				}
				else if (domRule != null)
					dayOfMonthRules.Add(domRule);
				else if (moyRule != null)
					monthOfYearRules.Add(moyRule);
				else if (subsetRule != null)
					subsetRules.Add(subsetRule);
				else
					otherRules.Add(rule);
			}
		}
		#endregion //GetRulesByType

		#region GetTimeSlotFromPoint

		internal static ITimeRangePresenter GetTimeRangePresenterFromPoint(UIElement subTree, Point pt, CalendarGroupBase restrictToGroup, bool includeHeaders = false)
		{



			// JJD 4/18/11 - TFS72915/TFS72927
			// Call GetVisualDescendantFromPoint instead since that will not return an element whose
			// IsHitTestVisible is false
			//HitTestResult htResult = VisualTreeHelper.HitTest(subTree, pt);

			//if (htResult == null)
			//    return null;

			//UIElement element = htResult.VisualHit as UIElement;

			//if (element == null && htResult.VisualHit != null)
			//    element = PresentationUtilities.GetVisualAncestor<UIElement>(htResult.VisualHit, null);
			UIElement element = PresentationUtilities.GetVisualDescendantFromPoint<UIElement>(subTree as UIElement, pt, null);

			if (element == null)
				return null;

			{
				// JJD 4/19/11 - TFS73143
				// Start with the child element not its parent. Otherwise we can miss some valid time range presenters
				//UIElement parent = VisualTreeHelper.GetParent(element) as UIElement;
				UIElement parent = element;

				while (parent != null && parent != subTree)
				{
					ITimeRangePresenter trp = parent as ITimeRangePresenter;

					if (trp != null)
					{
						TimeRangePresenterBase tsp = trp as TimeRangePresenterBase;

						if (restrictToGroup == null ||
							 tsp !=  null && restrictToGroup == GetCalendarGroupFromElement(tsp))
							return trp;

						switch ( trp.Kind )
						{
							case TimeRangeKind.TimeHeader:
							case TimeRangeKind.TimeHeaderWithDayContext:
								if ( includeHeaders )
									return trp;

								break;
						}
					}
					else
					{

						ITimeRangeArea timeRangeArea = parent as ITimeRangeArea;

						if (timeRangeArea != null)
						{
							TimeslotPanelBase panel = timeRangeArea.TimeRangePanel;

							if (panel != null && panel != subTree)
							{

								GeneralTransform transform = subTree.TransformToVisual(panel);
								pt = transform.Transform(pt);

								trp = GetTimeRangePresenterFromPoint(panel, pt, restrictToGroup, includeHeaders);

								if (trp != null)
									return trp;
							}

							break;
						}
					}

					parent = VisualTreeHelper.GetParent(parent) as UIElement;
				}
			}

			return null;
		}

		#endregion //GetTimeSlotFromPoint	

		#region GetTimeslotRange
		internal static DateRange GetTimeslotRange( TimeRangePresenterBase tsp, TimeSpan logicalDayOffset )
		{
			Debug.Assert(null != tsp);

			DateRange range = tsp.LocalRange;

			if (tsp.Kind == TimeRangeKind.TimeHeader)
			{
				// adjust relative to the selection anchor
				ScheduleControlBase control = ScheduleUtilities.GetControl(tsp);

				if (null != control)
				{
					DateRange? selectionAnchor = control.GetSelectionAnchor();

					if (null != selectionAnchor)
					{
						DateRange activeDayRange = ScheduleUtilities.GetLogicalDayRange(selectionAnchor.Value.Start, logicalDayOffset);
						DateRange tspDayRange = ScheduleUtilities.GetLogicalDayRange(range.Start, logicalDayOffset);
						TimeSpan delta = activeDayRange.Start.Subtract(tspDayRange.Start);

						range.Start = range.Start.Add(delta);
						range.End = range.End.Add(delta);
					}
				}
			}

			return range;
		}
		#endregion // GetTimeslotRange

		#region GetTimeZoneInfoProvider

		internal static TimeZoneInfoProvider GetTimeZoneInfoProvider(UIElement element)
		{
			ScheduleControlBase control = element as ScheduleControlBase ?? ScheduleUtilities.GetControl(element);

			if (control != null)
				return control.TimeZoneInfoProviderResolved;

			return TimeZoneInfoProvider.DefaultProvider;
		}

		#endregion //GetTimeZoneInfoProvider	

		#region GetTimeslotCount
		internal static int GetTimeslotCount(TimeslotRange[] groupTemplates)
		{
			int count = 0;

			foreach (var item in groupTemplates)
			{
				count += item.TimeslotCount;
			}
			return count;
		} 
		#endregion // GetTimeslotCount 

		#region GetWeeks
		internal static DateRange[] GetWeeks(IList<DateTime> sortedDates, CalendarHelper calHelper, DayOfWeek? firstDayOfWeek, int maxWeeks)
		{
			List<DateRange> ranges = new List<DateRange>();

			DateRange currentRange = new DateRange();
			int offset;

			// AS 4/21/11
			// Found this while working on 11.1. We shouldn't use the starting date as the basis for 
			// when to stop getting weeks when really what we want to limit is the number of weeks
			// for which the dates provided could span beyond maxWeeks with week gaps between them.
			//
			//DateTime maxDate = DateTime.MaxValue;

			if (maxWeeks > 0 && sortedDates.Count > 0)
			{
				DateTime tempDate = sortedDates[0];
				tempDate = calHelper.GetFirstDayOfWeekForDate(tempDate, firstDayOfWeek, out offset);
				// AS 4/21/11
				//// AS 12/6/10
				//// Found this while working on 11.1.
				////
				////maxDate = tempDate.AddDays((maxWeeks * 7) - offset);
				//maxDate = calHelper.AddDays(tempDate, (maxWeeks * 7) - offset);
			}

			for (int i = 0, visDateCount = sortedDates.Count; i < visDateCount; i++)
			{
				DateTime date = sortedDates[i];

				if (i > 0 && currentRange.ContainsExclusive(date))
					continue;

				// AS 4/21/11
				//if ( date >= maxDate )
				//    break;

				DateTime start = calHelper.GetFirstDayOfWeekForDate(date, firstDayOfWeek, out offset);
				DateTime end = calHelper.AddDays(start, 7 - offset);

				currentRange = new DateRange(start, end);
				ranges.Add(currentRange);

				// AS 4/21/11
				if (ranges.Count >= maxWeeks)
					break;
			}

			return ranges.ToArray();
		}
		#endregion // GetWeeks

		#region GetWorkingHourResources
		internal static ICollection<Resource> GetWorkingHourResources(WorkingHoursSource? workingHoursSource, ScheduleControlBase control)
		{
			ICollection<Resource> resources = null;

			// get the Working Hours for filtering time slots
			if (workingHoursSource.HasValue)
			{
				XamScheduleDataManager dataManager = control.DataManagerResolved;

				if (dataManager != null && workingHoursSource == WorkingHoursSource.CurrentUser)
				{
					if (dataManager.CurrentUser != null)
						resources = new Resource[] { dataManager.CurrentUser };
					else // including null anyway
						resources = new Resource[] { null };
				}
				else
				{
					CalendarGroupCollection resolvedGroups = control.CalendarGroupsResolvedSource;

					if (null != resolvedGroups)
						resources = resolvedGroups.VisibleResources;
				}
			}

			return resources;
		}
		#endregion // GetWorkingHourResources

		#region HasOverlappingInstances

		// SSP 4/14/11
		// 
		/// <summary>
		/// Returns true if at least one date range overlaps with another in the list.
		/// </summary>
		/// <param name="ranges">Date ranges.</param>
		/// <returns>True if at least one date range overlaps with another. False otherwise.</returns>
		internal static bool HasOverlappingInstances( IEnumerable<DateRange> ranges )
		{
			List<DateRange> list = ranges.ToList( );
			list.Sort( );

			var prev = list[0];

			for ( int i = 1; i < list.Count; i++ )
			{
				var ii = list[i];

				// SSP 4/22/11 TFS73403
				// Apparently IntersectsWith does not consider end time exclusive. Instead use the IntersectsWithExclusive.
				// 
				//if ( prev.IntersectsWith( ii ) )
				if ( prev.IntersectsWithExclusive( ii ) )
					return true;

				prev = ii;
			}

			return false;
		} 

		#endregion // HasOverlappingInstances

		#region HasVisibleCalendars
		internal static bool HasVisibleCalendars(CalendarGroupBase group)
		{
			return group != null && group.VisibleCalendars.Count > 0;
		} 
		#endregion // HasVisibleCalendars

		#region GetTimeString
		internal static string GetTimeString(DateTime dateTime)
		{
			
			return dateTime.ToShortTimeString();
		}
		#endregion //GetTimeString	
    
		#region IndexOfNearestAfter
		/// <summary>
		/// Searches the specified list at the position after the starting point 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list to search</param>
		/// <param name="index">The starting index for the search</param>
		/// <param name="match">The method used to perform the check for which item should be returned</param>
		/// <returns></returns>
		internal static int IndexOfNearestAfter<T>( IList<T> list, int index, Predicate<T> match )
		{
			ValidateNotNull(list, "list");
			ValidateNotNull(match, "match");

			// AS 10/29/10 TFS58663
			//for ( int i = index, count = list.Count; i < count; i++ )
			for ( int i = Math.Max(index, 0), count = list.Count; i < count; i++ )
			{
				if ( match(list[i]) )
					return i;
			}

			// AS 10/29/10 TFS58663
			//for ( int i = index - 1; i >= 0; i-- )
			for ( int i = Math.Min(index - 1, list.Count - 1); i >= 0; i-- )
			{
				if ( match(list[i]) )
					return i;
			}

			return -1;
		}
		#endregion // IndexOfNearestAfter

		#region IndexOfNext
		/// <summary>
		/// Enumerates the list from the specified starting point skipping the specified number of items that match.
		/// </summary>
		/// <typeparam name="T">The type of item in the list</typeparam>
		/// <param name="list">The list to enumerate</param>
		/// <param name="startingIndex">The index of the item at which to start. Note the item itself is not considered even if <paramref name="returnLast"/> is true.</param>
		/// <param name="match">The method to invoke to determine if the item is a match</param>
		/// <param name="next">True to look forward in the list; false to iterate backwards</param>
		/// <param name="returnLast">True to return the last item that matches</param>
		/// <param name="matchCount">The number of matching items that must match</param>
		/// <returns></returns>
		internal static int IndexOfNext<T>( IList<T> list, int startingIndex, bool next, int matchCount, bool returnLast, Predicate<T> match )
		{
			ValidateNotNull(list, "list");
			ValidateNotNull(match, "match");

			Debug.Assert(matchCount > 0, "This meant to be used to search forward/back");

			int adjustment = next ? 1 : -1;

			// ignore the starting item
			startingIndex += adjustment;

			if ( startingIndex >= 0 && startingIndex < list.Count )
			{
				int previousMatch = -1;

				int end = next ? list.Count : -1;

				for ( int i = startingIndex; i != end; i += adjustment )
				{
					T item = list[i];

					if ( !match(item) )
						continue;

					matchCount--;

					if ( matchCount == 0 )
						return i;

					previousMatch = i;
				}

				if ( returnLast )
					return previousMatch;
			}

			return -1;
		}
		#endregion // IndexOfNext

		#region IsSet

		internal static bool IsSet(DayOfWeekFlags flagsToCheck, DayOfWeek dayOfWeek)
		{
			return 0 != ( (int)flagsToCheck & ( 1 << (int)dayOfWeek ) );
		}

		#endregion // IsSet

		#region LogDebuggerError

		internal static void LogDebuggerError(string category, DataErrorInfo error)
		{

			if (Debugger.IsAttached && Debugger.IsLogging())
				LogDebuggerErrorHelper(category, error);
		}

		private static void LogDebuggerErrorHelper(string category, DataErrorInfo error)
		{
			if (error.ErrorList != null)
			{
				foreach (DataErrorInfo childError in error.ErrorList)
					LogDebuggerErrorHelper(category, childError);

				return;
			}

			int level = Math.Max((int)error.Severity, 0);
			
			Debugger.Log(level, category, error.ToString());
		}

		#endregion //LogDebuggerError	

		// AS 5/25/11 TFS74447
		#region Mod
		internal static double Mod(double dividend, double divisor)
		{
			if (CoreUtilities.AreClose(divisor, 0))
				return 0;

			return dividend % divisor;
		} 
		#endregion //Mod

		// AS 3/7/12 TFS102945
		#region OnTouchAwareClickHelperDown
		internal static void OnTouchAwareClickHelperDown(FrameworkElement element, System.Windows.Input.MouseButtonEventArgs e, Action clickAction, bool onlyWithinDoubleClickRange)
		{
			if (clickAction == null || element == null)
				return;

			var ctrl = ScheduleUtilities.GetControlFromElementTree(element);

			if (null != ctrl && ctrl.ShouldQueueTouchActions)
			{
				// if we're in the middle of a touch operation then we'll wait and if the operation was a tap/click
				// then we'll invoke the associated action. this ignores the double click parameter but I think that 
				// is ok for a touch tap
				ctrl.EnqueueTouchAction((a) =>
				{
					if (a == Infragistics.Controls.Primitives.ScrollInfoTouchAction.Click)
						clickAction();
				});
			}
			else
			{
				ClickHelper.OnMouseLeftButtonDown(element, e, clickAction, onlyWithinDoubleClickRange);
			}
		} 
		#endregion //OnTouchAwareClickHelperDown

		#region RaiseErrorHelper

		internal static void RaiseErrorHelper(XamScheduleDataManager dm, OperationResult result)
		{
			DataErrorInfo error = null != result ? result.Error : null;
			RaiseErrorHelper(dm, error);
		}

		internal static void RaiseErrorHelper(XamScheduleDataManager dm, DataErrorInfo error)
		{
			Debug.Assert(null != dm);
			if (null != error && null != dm)
				dm.ProcessError(error);
		}

		#endregion // RaiseErrorHelper

		#region RefreshDisplay
		internal static void RefreshDisplay(Panel recyclingPanel)
		{
			if (null != recyclingPanel)
			{
				RecyclingManager.Manager.ReleaseAll(recyclingPanel);
				recyclingPanel.InvalidateMeasure();
			}
		} 
		#endregion // RefreshDisplay

		#region Remove
		internal static bool Remove<T>( IList<T> list, Predicate<T> match, out T itemRemoved )
		{
			int index = IndexOfNext(list, -1, true, 1, true, match);

			if ( index >= 0 )
			{
				itemRemoved = list[index];
				list.RemoveAt(index);
				return true;
			}

			itemRemoved = default(T);
			return false;
		} 
		#endregion // Remove

		#region RemoveListener
		internal static void RemoveListener( ISupportPropertyChangeNotifications notifier, IPropertyChangeListener listener )
		{
			notifier.RemoveListener(listener);
		}
		#endregion // RemoveListener

		#region SetBoolTrueProperty
		/// <summary>
		/// Helper method for setting the value of a property to a boxed true when true and UnsetValue when false.
		/// </summary>
		/// <param name="element">The element on which the property is being set</param>
		/// <param name="dp">The property to set</param>
		/// <param name="newValue">True to set the value to true and false to clear the local value.</param>
		internal static void SetBoolTrueProperty(UIElement element, DependencyProperty dp, bool newValue)
		{
			if (newValue)
				element.SetValue(dp, KnownBoxes.TrueBox);
			else
				element.ClearValue(dp);
		}
		#endregion // SetBoolTrueProperty

		#region SpecifyKind

		internal static DateTime SpecifyKind( DateTime date, DateTimeKind kind )
		{
			return date.Kind == kind ? date : DateTime.SpecifyKind( date, kind );
		} 

		#endregion // SpecifyKind	

		#region ToRGBHelper



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static float ToRGBHelper(float rm1, float rm2, float rh)
		{
			if (rh > 360.0f) rh -= 360.0f;
			else if (rh < 0.0f) rh += 360.0f;

			if (rh < 60.0f) rm1 = rm1 + (rm2 - rm1) * rh / 60.0f;
			else if (rh < 180.0f) rm1 = rm2;
			else if (rh < 240.0f) rm1 = rm1 + (rm2 - rm1) * (240.0f - rh) / 60.0f;

			return (rm1 * 255);
		}

		#endregion //ToRGBHelper

		#endregion // Methods
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