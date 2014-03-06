using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the header for a single day in a <see cref="XamMonthView"/>
	/// </summary>
	public class MonthViewDayHeader : DayHeaderBase
	{
		#region Constructor
		static MonthViewDayHeader()
		{

			MonthViewDayHeader.DefaultStyleKeyProperty.OverrideMetadata(typeof(MonthViewDayHeader), new FrameworkPropertyMetadata(typeof(MonthViewDayHeader)));

		}

		/// <summary>
		/// Initializes a new <see cref="MonthViewDayHeader"/>
		/// </summary>
		public MonthViewDayHeader()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="MonthViewDayHeader"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="MonthViewDayHeaderAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new MonthViewDayHeaderAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region MonthNameFormatType

		private static readonly DependencyPropertyKey MonthNameFormatTypePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("MonthNameFormatType",
			typeof(DateRangeFormatType), typeof(MonthViewDayHeader), DateRangeFormatType.MonthDayHeader, null);

		/// <summary>
		/// Identifies the read-only <see cref="MonthNameFormatType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MonthNameFormatTypeProperty = MonthNameFormatTypePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the preferred format type when the month name is to be displayed.
		/// </summary>
		/// <seealso cref="MonthNameFormatTypeProperty"/>
		public DateRangeFormatType MonthNameFormatType
		{
			get
			{
				return (DateRangeFormatType)this.GetValue(MonthViewDayHeader.MonthNameFormatTypeProperty);
			}
			private set
			{
				this.SetValue(MonthViewDayHeader.MonthNameFormatTypePropertyKey, value);
			}
		}

		#endregion //MonthNameFormatType

		#region MonthNameVisibility

		private static readonly DependencyPropertyKey MonthNameVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("MonthNameVisibility",
			typeof(Visibility), typeof(MonthViewDayHeader), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="MonthNameVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MonthNameVisibilityProperty = MonthNameVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value indicating whether the name of the month should be displayed.
		/// </summary>
		/// <seealso cref="MonthNameVisibilityProperty"/>
		public Visibility MonthNameVisibility
		{
			get
			{
				return (Visibility)this.GetValue(MonthViewDayHeader.MonthNameVisibilityProperty);
			}
			private set
			{
				this.SetValue(MonthViewDayHeader.MonthNameVisibilityPropertyKey, value);
			}
		}

		#endregion //MonthNameVisibility

		#endregion // Public Properties

		#region Internal Properties

		#region DayType
		private MonthDayType _dayType;

		internal MonthDayType DayType
		{
			get { return _dayType; }
			set
			{
				if (value != _dayType)
				{
					_dayType = value;

					if (value == MonthDayType.DayNumber)
						this.ClearValue(MonthNameVisibilityPropertyKey);
					else
					{
						this.SetValue(MonthNameVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);

						if (value == MonthDayType.MonthDayYear)
							this.MonthNameFormatType = DateRangeFormatType.MonthDayHeaderFull;
					}
				}
			}
		}
		#endregion // DayType

		#endregion // Internal Properties

		#endregion // Properties
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