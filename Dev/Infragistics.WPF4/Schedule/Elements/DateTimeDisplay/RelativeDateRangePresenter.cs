using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using Infragistics.AutomationPeers;
using System.Globalization;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom element used to display a date time range relative to a date.
	/// </summary>
	public class RelativeDateRangePresenter : ScheduleDatePresenterBase
	{
		#region Private Members

		private bool _formattedTextDirty;
		private double _measureWidth;

		[ThreadStatic()]
		private static Dictionary<FontInfo, double> _LongestTimeWidthMap;

		#endregion //Private Members
    
		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="arrangeBounds">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			if (this.IncludeEnd != this.IncludeStart &&
				this.SizeToWidestTime &&
				this.HorizontalAlignment != HorizontalAlignment.Stretch &&
				arrangeBounds.Width > _measureWidth)
				arrangeBounds.Width = _measureWidth;

			return base.ArrangeOverride(arrangeBounds);
		}

		#endregion //ArrangeOverride	
    
		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			bool sizeToWidestTime = this.IncludeEnd != this.IncludeStart && this.SizeToWidestTime;

			double longestTimeWidth = 0;

			// if the _LongestTimeWidth hasn't been calculated yet and we are just displaying either the start or the end time
			// then measure the time as if this was the widest time, i.e. 12:55 PM. This is so we can align all the times 
			if (sizeToWidestTime)
			{
				FontInfo fi = FontInfo.CreateFrom(this);

				if (_LongestTimeWidthMap == null)
					_LongestTimeWidthMap = new Dictionary<FontInfo, double>();

				if (false == _LongestTimeWidthMap.TryGetValue(fi, out longestTimeWidth))
				{
					DateInfoProvider dateInfo = this.DateInfoProvider;

					if (dateInfo != null)
					{
						List<DateTime> dts = new List<DateTime>();
						DateTime dt = DateTime.Now.Date;
						dt = dt.Add(new TimeSpan(0, 55, 0)); // 12:55 AM
						dts.Add(dt);
						dt = dt.Add(new TimeSpan(12, 0, 0)); // 12:55 PM
						dts.Add(dt);
						dt = dt.Add(new TimeSpan(10, 0, 0)); // 22:55 (to cover 24 hr time)
						dts.Add(dt);

						foreach (DateTime wideTime in dts)
						{
							this.FormattedText = dateInfo.FormatDateRange(wideTime, wideTime, wideTime.Date, true, false);

							// Add a pixel to cover the situation where these are not the widest times
							longestTimeWidth = Math.Max(longestTimeWidth, base.MeasureOverride(availableSize).Width + 1);
						}

						_LongestTimeWidthMap[fi] = longestTimeWidth;

						_formattedTextDirty = true;
					}
				}

			}

			this.VerifyFormattedText();

			Size size = base.MeasureOverride(availableSize);

			// cache the width from the measure
			_measureWidth = size.Width;

			// make sure we return a size that can display the widest time if we are just displaying a single time
			if (longestTimeWidth > 0 &&
				sizeToWidestTime)
			{
				size.Width = Math.Max(size.Width, longestTimeWidth);
			}

			return size;
		}

		#endregion //MeasureOverride

		#region ToString

		/// <summary>
		/// Returns the string representation of the object.
		/// </summary>
		/// <returns>A string containing the <see cref="Start"/> and <see cref="End"/></returns>
		public override string ToString()
		{
			this.VerifyFormattedText();
			return this.FormattedText;
		}

		#endregion //Base class overrides

		#endregion //ToString

		#region Properties

		#region Public Properties

		#region ConvertDateTimeToLocal

		/// <summary>
		/// Identifies the <see cref="ConvertDateTimeToLocal"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ConvertDateTimeToLocalProperty = DependencyPropertyUtilities.Register("ConvertDateTimeToLocal",
			typeof(bool), typeof(RelativeDateRangePresenter),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnFormattedTextChanged))
			);

		/// <summary>
		/// Returns or sets a value indicating whether the value specified by the <see cref="DateTime"/> should be converted to local using the associated <see cref="ScheduleDataConnectorBase.TimeZoneInfoProvider"/> before formatting.
		/// </summary>
		/// <seealso cref="ConvertDateTimeToLocalProperty"/>
		public bool ConvertDateTimeToLocal
		{
			get
			{
				return (bool)this.GetValue(RelativeDateRangePresenter.ConvertDateTimeToLocalProperty);
			}
			set
			{
				this.SetValue(RelativeDateRangePresenter.ConvertDateTimeToLocalProperty, value);
			}
		}

		#endregion //ConvertDateTimeToLocal

		#region End

		/// <summary>
		/// Identifies the <see cref="End"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EndProperty = DependencyPropertyUtilities.Register("End",
			typeof(DateTime), typeof(RelativeDateRangePresenter),
			DependencyPropertyUtilities.CreateMetadata(DateTime.MinValue, new PropertyChangedCallback(OnFormattedTextChanged))
			);

		/// <summary>
		/// Returns or sets the end of the range
		/// </summary>
		/// <seealso cref="EndProperty"/>
		public DateTime End
		{
			get
			{
				return (DateTime)this.GetValue(RelativeDateRangePresenter.EndProperty);
			}
			set
			{
				this.SetValue(RelativeDateRangePresenter.EndProperty, value);
			}
		}

		#endregion //End

		#region IncludeEnd

		/// <summary>
		/// Identifies the <see cref="IncludeEnd"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IncludeEndProperty = DependencyPropertyUtilities.Register("IncludeEnd",
			typeof(bool), typeof(RelativeDateRangePresenter),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnFormattedTextChanged))
			);

		/// <summary>
		/// Returns or sets whether the end time should be included in the FormattedText
		/// </summary>
		/// <seealso cref="IncludeEndProperty"/>
		public bool IncludeEnd
		{
			get
			{
				return (bool)this.GetValue(RelativeDateRangePresenter.IncludeEndProperty);
			}
			set
			{
				this.SetValue(RelativeDateRangePresenter.IncludeEndProperty, value);
			}
		}

		#endregion //IncludeEnd

		#region IncludeStart

		/// <summary>
		/// Identifies the <see cref="IncludeStart"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IncludeStartProperty = DependencyPropertyUtilities.Register("IncludeStart",
			typeof(bool), typeof(RelativeDateRangePresenter),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnFormattedTextChanged))
			);

		/// <summary>
		/// Returns or sets whether the start time should be included in the FormattedText
		/// </summary>
		/// <seealso cref="IncludeStartProperty"/>
		public bool IncludeStart
		{
			get
			{
				return (bool)this.GetValue(RelativeDateRangePresenter.IncludeStartProperty);
			}
			set
			{
				this.SetValue(RelativeDateRangePresenter.IncludeStartProperty, value);
			}
		}

		#endregion //IncludeStart

		#region RelativeDate

		/// <summary>
		/// Identifies the <see cref="RelativeDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RelativeDateProperty = DependencyPropertyUtilities.Register("RelativeDate",
			typeof(DateTime), typeof(RelativeDateRangePresenter),
			DependencyPropertyUtilities.CreateMetadata(DateTime.MinValue, new PropertyChangedCallback(OnFormattedTextChanged))
			);

		/// <summary>
		/// Returns or sets the Relative Date to use when formatting the text
		/// </summary>
		/// <seealso cref="RelativeDateProperty"/>
		public DateTime RelativeDate
		{
			get
			{
				return (DateTime)this.GetValue(RelativeDateRangePresenter.RelativeDateProperty);
			}
			set
			{
				this.SetValue(RelativeDateRangePresenter.RelativeDateProperty, value);
			}
		}

		#endregion //RelativeDate

		#region SizeToWidestTime

		/// <summary>
		/// Identifies the <see cref="SizeToWidestTime"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SizeToWidestTimeProperty = DependencyPropertyUtilities.Register("SizeToWidestTime",
			typeof(bool), typeof(RelativeDateRangePresenter),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnSizeToWidestTimeChanged))
			);

		private static void OnSizeToWidestTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RelativeDateRangePresenter instance = (RelativeDateRangePresenter)d;

			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets whether the element should ensure that its desired size is wide enough to accommodate the widest time, e.g. 12:55 AM.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if <see cref="IncludeStart"/> equals <see cref="IncludeEnd"/> then this property is ignored.</para>
		/// </remarks>
		/// <seealso cref="SizeToWidestTimeProperty"/>
		public bool SizeToWidestTime
		{
			get
			{
				return (bool)this.GetValue(RelativeDateRangePresenter.SizeToWidestTimeProperty);
			}
			set
			{
				this.SetValue(RelativeDateRangePresenter.SizeToWidestTimeProperty, value);
			}
		}

		#endregion //SizeToWidestTime

		#region Start

		/// <summary>
		/// Identifies the <see cref="Start"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StartProperty = DependencyPropertyUtilities.Register("Start",
			typeof(DateTime), typeof(RelativeDateRangePresenter),
			DependencyPropertyUtilities.CreateMetadata(DateTime.MinValue, new PropertyChangedCallback(OnFormattedTextChanged))
			);

		/// <summary>
		/// Returns or sets the start of the range
		/// </summary>
		/// <seealso cref="StartProperty"/>
		public DateTime Start
		{
			get
			{
				return (DateTime)this.GetValue(RelativeDateRangePresenter.StartProperty);
			}
			set
			{
				this.SetValue(RelativeDateRangePresenter.StartProperty, value);
			}
		}

		#endregion //Start

		#endregion //Public Properties

		#region Private Properties

		#region DateInfoProvider

		private DateInfoProvider DateInfoProvider
		{
			get
			{
				DayActivityToolTipInfo ttInfo = this.DataContext as DayActivityToolTipInfo;

				DateInfoProvider dateInfo = ttInfo != null ? ttInfo.DataManager.DateInfoProviderResolved : null;

				if (dateInfo == null)
				{
					ScheduleControlBase ctrl = ScheduleUtilities.GetControlFromElementTree(this);

					dateInfo = ScheduleUtilities.GetDateInfoProvider(ctrl);
				}

				return dateInfo;
			}
		}

		#endregion //DateInfoProvider

		#region TimeZoneInfoProvider

		private TimeZoneInfoProvider TimeZoneInfoProvider
		{
			get
			{
				DayActivityToolTipInfo ttInfo = this.DataContext as DayActivityToolTipInfo;

				TimeZoneInfoProvider tzProvider = ttInfo != null ? ttInfo.DataManager.TimeZoneInfoProviderResolved : null;

				if (tzProvider == null)
				{
					ScheduleControlBase ctrl = ScheduleUtilities.GetControlFromElementTree(this);

					tzProvider = ctrl != null ? ctrl.TimeZoneInfoProviderResolved : TimeZoneInfoProvider.DefaultProvider;
				}

				return tzProvider;
			}
		}

		#endregion //TimeZoneInfoProvider

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods
    
		#region OnFormattedTextVerfied

		internal virtual void OnFormattedTextVerfied(DateInfoProvider dateInfo, DateTime start, DateTime end)
		{
		}

		#endregion //OnFormattedTextVerfied

		#endregion //Internal Methods

		#region Private Methods

		#region InvalidateFormattedText

		private void InvalidateFormattedText()
		{
			this._formattedTextDirty = true;
			this.InvalidateMeasure();
		}

		#endregion //InvalidateFormattedText

		#region OnFormattedTextChanged
		private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RelativeDateRangePresenter instance = (RelativeDateRangePresenter)d;
			instance.InvalidateFormattedText();
		}
		#endregion // OnFormattedTextChanged
    
		#region VerifyFormattedText

		private void VerifyFormattedText()
		{
			if (!this._formattedTextDirty)
				return;

			this._formattedTextDirty = false;

			DateTime start = this.Start;
			DateTime end = this.End;

			DateInfoProvider dateInfo = this.DateInfoProvider;

			if (this.ConvertDateTimeToLocal)
			{
				TimeZoneInfoProvider tzProvider = this.TimeZoneInfoProvider;

				start = ScheduleUtilities.ConvertFromUtc(tzProvider.LocalToken, start);
				end = ScheduleUtilities.ConvertFromUtc(tzProvider.LocalToken, end);
			}
			else
			{
				start = DateTime.SpecifyKind(start, DateTimeKind.Unspecified);
				end = DateTime.SpecifyKind(end, DateTimeKind.Unspecified);
			}

			this.FormattedText = dateInfo.FormatDateRange(start, end, this.RelativeDate, this.IncludeStart, this.IncludeEnd);

			this.OnFormattedTextVerfied(dateInfo, start, end);
		}

		#endregion //VerifyFormattedText

		#endregion //Private Methods

		#endregion //Methods

		private struct FontInfo
		{
			#region Private members

			private FontFamily _family;
			private FontStretch _stretch;
			private FontStyle _style;
			private FontWeight _weight;
			private double _size;

			#endregion //Private members	
    
			#region Constructor

			private FontInfo(FontFamily family, double size, FontStretch stretch, FontStyle style, FontWeight weight)
			{
				_family = family;
				_size = size;
				_stretch = stretch;
				_style = style;
				_weight = weight;
			}

			internal static FontInfo CreateFrom(Control control)
			{
				return new FontInfo(control.FontFamily, control.FontSize, control.FontStretch, control.FontStyle, control.FontWeight);
			}

			#endregion //Constructor

			#region Base class overrides

			#region Equals

			/// <summary>
			/// Determines if the passed in object is equal.
			/// </summary>
			/// <param name="obj">The object to compare</param>
			/// <returns>True if equal</returns>
			public override bool Equals(object obj)
			{
				if (obj is FontInfo)
				{
					FontInfo fi = (FontInfo)obj;

					return fi._size == _size &&
						   fi._stretch == _stretch &&
						   fi._weight == _weight &&
						   fi._style == _style &&
						   fi._family == _family;
				}
				return false;
			}

			#endregion //Equals	
    
			#region GetHashCode

			/// <summary>
			/// Calculates a hash code
			/// </summary>
			/// <returns>The calculated hash code.</returns>
			public override int GetHashCode()
			{
				return _size.GetHashCode();
			}

			#endregion //GetHashCode

			#endregion //Base class overrides
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