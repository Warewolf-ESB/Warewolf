using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using Infragistics.AutomationPeers;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom element for use within a <see cref="TimeslotHeader"/> that displays the hour and minutes text.
	/// </summary>
	[DesignTimeVisible(false)]
	public class TimeslotHeaderTimePresenter : Panel
	{
		#region Private Members

		private TimeslotHeader _header;
		private TextBlock _tbHour;
		private TextBlock _tbSeparator;
		private TextBlock _tbMinutesOrAMPM;

		#endregion //Private Members	

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeslotHeaderTimePresenter"/>
		/// </summary>
		public TimeslotHeaderTimePresenter()
		{
		} 
		#endregion // Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			if (this._tbHour == null)
				return finalSize;

			Size sizeUsed = new Size();

			ArrangeHelper(ref sizeUsed, this._tbHour);
			ArrangeHelper(ref sizeUsed, this._tbSeparator);
			ArrangeHelper(ref sizeUsed, this._tbMinutesOrAMPM);

			return sizeUsed;
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
			if (this._header == null)
			{

				this._header = this.TemplatedParent as TimeslotHeader;

				if (this._header == null)

					this._header = PresentationUtilities.GetVisualAncestor<TimeslotHeader>(this, null);

				Debug.Assert(this._header != null, "this element should only be used inside a TimeslotHeader");

				if (this._header == null)
				{
					this.ClearChildren();

					return new Size(1, 1);
				}

				this._header.InitializeTimePresenter(this);
			}

			if (!this._header.IsFirstInMajor)
			{
				this.ClearChildren();
				return new Size(1, 1);
			}

			if (this._tbHour == null)
			{
				this._tbHour = new TextBlock();
				this._tbHour.Margin = new Thickness(4, 0, 0, 0);
				this.Children.Add(this._tbHour);
				this._tbSeparator = new TextBlock();
				this._tbSeparator.Margin = new Thickness(2, 0, 2, 0);
				this.Children.Add(this._tbSeparator);
				this._tbMinutesOrAMPM = new TextBlock();
				this._tbMinutesOrAMPM.Margin = new Thickness(0, 0, 4, 0);
				this.Children.Add(this._tbMinutesOrAMPM);
			}

			TimeslotBase timeslot = this._header.Timeslot;

			// AS 4/20/11 TFS73205
			//TimeSpan span = timeslot.End.Subtract(timeslot.Start);

			ScheduleControlBase control = this._header.ScheduleControl;

			// if the control is null that means this is the headr that is being used for
			// initial sizing so always bump the hour size in that case
			// AS 4/20/11 TFS73205
			// Allow the control to determine if the hour size should be bumped.
			//
			//bool bumpFontSizeOfHour = control == null || this._header.Tag == ScheduleControlBase.MeasureOnlyItemId || span.TotalMinutes < 31 && span.TotalMinutes >= 1;
			bool bumpFontSizeOfHour = control == null || this._header.Tag == ScheduleControlBase.MeasureOnlyItemId || control.ShouldBumpHeaderHourFontSize(timeslot);

			DateInfoProvider dateInfo = ScheduleUtilities.GetDateInfoProvider(control);

			bool showAMPMDesignator = false;

			if (!dateInfo.DisplayTimeIn24HourFormat)
			{
				showAMPMDesignator = _header.ShowAMPMDesignator;
			}

			this._tbHour.Text = dateInfo.FormatDate(timeslot.Start, DateTimeFormatType.Hour);

			if (showAMPMDesignator)
			{
				this._tbSeparator.Text = "";
				this._tbMinutesOrAMPM.Text = timeslot.Start.Hour > 11 ? dateInfo.PMDesignatorLowercase : dateInfo.AMDesignatorLowercase;
			}
			else
			{
				if (bumpFontSizeOfHour)
					this._tbSeparator.Text = "";
				else
					this._tbSeparator.Text = dateInfo.TimeSeparator;

				this._tbMinutesOrAMPM.Text = dateInfo.FormatDate(timeslot.Start, DateTimeFormatType.Minute);
			}


			// AS 2/16/12 TFS99687
			//Brush brForeground = this._header.ComputedForeground;
			Brush brForeground = this.Foreground ?? this._header.ComputedForeground;

			this._tbHour.Foreground = brForeground;
			this._tbSeparator.Foreground = brForeground;
			this._tbMinutesOrAMPM.Foreground = brForeground;

			Size size = new Size(double.PositiveInfinity, double.PositiveInfinity);
			this._tbSeparator.Measure(size);
			this._tbMinutesOrAMPM.Measure(size);
			
			if ( bumpFontSizeOfHour)
			{
				this._tbHour.FontSize = this._tbSeparator.FontSize * 1.5;
			}
			else
				this._tbHour.ClearValue(TextBlock.FontSizeProperty);

			this._tbHour.Measure(size);

			size = new Size();
			AggregateSize(ref size, this._tbHour);
			AggregateSize(ref size, this._tbSeparator);
			AggregateSize(ref size, this._tbMinutesOrAMPM);
			return size;
		}

		#endregion //MeasureOverride

		#endregion //Base class overrides	
        
		#region Properties

		// AS 2/16/12 TFS99687
		#region Foreground

		/// <summary>
		/// Identifies the <see cref="Foreground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ForegroundProperty = DependencyPropertyUtilities.Register("Foreground",
			typeof(Brush), typeof(TimeslotHeaderTimePresenter),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnForegroundChanged))
			);

		private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var timePresenter = d as TimeslotHeaderTimePresenter;
			timePresenter.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets the brush that should be used by the TextBlock elements created by the panel.
		/// </summary>
		/// <seealso cref="ForegroundProperty"/>
		public Brush Foreground
		{
			get
			{
				return (Brush)this.GetValue(TimeslotHeaderTimePresenter.ForegroundProperty);
			}
			set
			{
				this.SetValue(TimeslotHeaderTimePresenter.ForegroundProperty, value);
			}
		}

		#endregion //Foreground

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region AggregateSize

		private static void AggregateSize(ref Size size, TextBlock element)
		{
			Size desiredSize = element.DesiredSize;

			size.Width += desiredSize.Width;
			size.Height = Math.Max(size.Height, desiredSize.Height);
		}

		#endregion //AggregateSize	

		#region ArrangeHelper

		private static void ArrangeHelper(ref Size sizeUsed, TextBlock element)
		{
			Size desiredSize = element.DesiredSize;

			element.Arrange(new Rect(new Point(sizeUsed.Width, 0), desiredSize));

			sizeUsed.Width += desiredSize.Width;
			sizeUsed.Height = Math.Max(sizeUsed.Height, desiredSize.Height);
		}

		#endregion //ArrangeHelper	
        
		#region ClearChildren

		private void ClearChildren()
		{
			this._tbHour = null;
			this._tbSeparator = null;
			this._tbMinutesOrAMPM = null;
			this.Children.Clear();
		}

		#endregion //ClearChildren

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