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
	/// Custom element used to display a date time.
	/// </summary>
	public class ScheduleDateTimePresenter : ScheduleDatePresenterBase
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
		/// <returns>A string containing the <see cref="ScheduleDatePresenterBase.FormattedText"/></returns>
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
			typeof(bool), typeof(ScheduleDateTimePresenter),
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
				return (bool)this.GetValue(ScheduleDateTimePresenter.ConvertDateTimeToLocalProperty);
			}
			set
			{
				this.SetValue(ScheduleDateTimePresenter.ConvertDateTimeToLocalProperty, value);
			}
		}

		#endregion //ConvertDateTimeToLocal

		#region DateTime

		/// <summary>
		/// Identifies the <see cref="DateTime"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DateTimeProperty = DependencyPropertyUtilities.Register("DateTime",
			typeof(DateTime), typeof(ScheduleDateTimePresenter),
			DependencyPropertyUtilities.CreateMetadata(DateTime.MinValue, new PropertyChangedCallback(OnFormattedTextChanged))
			);

		/// <summary>
		/// Returns or sets the date time
		/// </summary>
		/// <seealso cref="DateTimeProperty"/>
		public DateTime DateTime
		{
			get
			{
				return (DateTime)this.GetValue(ScheduleDateTimePresenter.DateTimeProperty);
			}
			set
			{
				this.SetValue(ScheduleDateTimePresenter.DateTimeProperty, value);
			}
		}

		#endregion //DateTime

		#region FormatType

		/// <summary>
		/// Identifies the <see cref="FormatType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FormatTypeProperty = DependencyPropertyUtilities.Register("FormatType",
			typeof(DateTimeFormatType), typeof(ScheduleDateTimePresenter),
			DependencyPropertyUtilities.CreateMetadata(DateTimeFormatType.DayOfWeek, new PropertyChangedCallback(OnFormattedTextChanged))
			);

		/// <summary>
		/// Returns or sets an enumeration that specifies how to format the DateTime
		/// </summary>
		/// <seealso cref="FormatTypeProperty"/>
		public DateTimeFormatType FormatType
		{
			get
			{
				return (DateTimeFormatType)this.GetValue(ScheduleDateTimePresenter.FormatTypeProperty);
			}
			set
			{
				this.SetValue(ScheduleDateTimePresenter.FormatTypeProperty, value);
			}
		}

		#endregion //FormatType

		#endregion //Public Properties

		#endregion //Properties	

		#region Methods

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
			ScheduleDateTimePresenter instance = (ScheduleDateTimePresenter)d;
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

			DateTime date = this.DateTime;
			DateInfoProvider dateInfo = ScheduleUtilities.GetDateInfoProvider(ctrl);

			if (this.ConvertDateTimeToLocal)
			{
				TimeZoneInfoProvider tzProvider = ctrl != null ? ctrl.TimeZoneInfoProviderResolved : TimeZoneInfoProvider.DefaultProvider;
				date = ScheduleUtilities.ConvertFromUtc( tzProvider.LocalToken,  date);
			}
			else
			{
				date = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
			}

			this.FormattedText = dateInfo.FormatDate(date, this.FormatType);
		}

		#endregion //VerifyFormattedText

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