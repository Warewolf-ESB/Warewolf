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
	/// Custom element used to display a date time range.
	/// </summary>
	public class ScheduleDateRangePresenter : ScheduleDatePresenterBase
	{
		#region Private Members

		private bool _formattedTextDirty;

		#endregion //Private Members	

		#region Base class overrides

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			this.VerifyFormattedText();

			return base.MeasureOverride(availableSize);
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
			typeof(bool), typeof(ScheduleDateRangePresenter),
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
				return (bool)this.GetValue(ScheduleDateRangePresenter.ConvertDateTimeToLocalProperty);
			}
			set
			{
				this.SetValue(ScheduleDateRangePresenter.ConvertDateTimeToLocalProperty, value);
			}
		}

		#endregion //ConvertDateTimeToLocal

		#region End

		/// <summary>
		/// Identifies the <see cref="End"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EndProperty = DependencyPropertyUtilities.Register("End",
			typeof(DateTime), typeof(ScheduleDateRangePresenter),
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
				return (DateTime)this.GetValue(ScheduleDateRangePresenter.EndProperty);
			}
			set
			{
				this.SetValue(ScheduleDateRangePresenter.EndProperty, value);
			}
		}

		#endregion //End

		#region FormatType

		/// <summary>
		/// Identifies the <see cref="FormatType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FormatTypeProperty = DependencyPropertyUtilities.Register("FormatType",
			typeof(DateRangeFormatType), typeof(ScheduleDateRangePresenter),
			DependencyPropertyUtilities.CreateMetadata(DateRangeFormatType.ActivityToolTip, new PropertyChangedCallback(OnFormatTypeChanged))
			);

		private static void OnFormatTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleDateRangePresenter instance = (ScheduleDateRangePresenter)d;
			
			instance.InvalidateFormattedText();
		}

		/// <summary>
		/// Returns or sets an enumeration that specified how to format the date range
		/// </summary>
		/// <seealso cref="FormatTypeProperty"/>
		public DateRangeFormatType FormatType
		{
			get
			{
				return (DateRangeFormatType)this.GetValue(ScheduleDateRangePresenter.FormatTypeProperty);
			}
			set
			{
				this.SetValue(ScheduleDateRangePresenter.FormatTypeProperty, value);
			}
		}

		#endregion //FormatType

		#region Start

		/// <summary>
		/// Identifies the <see cref="Start"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StartProperty = DependencyPropertyUtilities.Register("Start",
			typeof(DateTime), typeof(ScheduleDateRangePresenter),
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
				return (DateTime)this.GetValue(ScheduleDateRangePresenter.StartProperty);
			}
			set
			{
				this.SetValue(ScheduleDateRangePresenter.StartProperty, value);
			}
		}

		#endregion //Start

		#endregion //Public Properties	
    
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
			ScheduleDateRangePresenter instance = (ScheduleDateRangePresenter)d;
			instance.InvalidateFormattedText();
		}
		#endregion // OnFormattedTextChanged

		#region VerifyFormattedText

		private void VerifyFormattedText()
		{
			if (!this._formattedTextDirty)
				return;

			this._formattedTextDirty = false;

			ScheduleControlBase ctrl = ScheduleUtilities.GetControlFromElementTree(this);

			DateTime start = this.Start;
			DateTime end = this.End;

			DateInfoProvider dateInfo = ScheduleUtilities.GetDateInfoProvider(ctrl);

			if (this.ConvertDateTimeToLocal)
			{
				TimeZoneInfoProvider tzProvider = ctrl != null ? ctrl.TimeZoneInfoProviderResolved : TimeZoneInfoProvider.DefaultProvider;

				start = ScheduleUtilities.ConvertFromUtc( tzProvider.LocalToken, start);
				end = ScheduleUtilities.ConvertFromUtc( tzProvider.LocalToken, end);
			}
			else
			{
				start = DateTime.SpecifyKind(start, DateTimeKind.Unspecified);
				end = DateTime.SpecifyKind(end, DateTimeKind.Unspecified);
			}

			this.FormattedText = dateInfo.FormatDateRange(start, end, this.FormatType);

			this.OnFormattedTextVerfied(dateInfo, start, end);
		}

		#endregion //VerifyFormattedText

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