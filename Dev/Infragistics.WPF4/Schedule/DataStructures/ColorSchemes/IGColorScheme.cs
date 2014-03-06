using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Controls.Schedules.Primitives;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Infragistics.Collections;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Primitives;
using System.Collections;
using System.Windows.Media;
using System.Diagnostics;


using Infragistics.Windows.Themes;



using Infragistics.Controls.Editors;


namespace Infragistics.Controls.Schedules
{
	// JJD 10/29/10 - NA 2011 Volume 1 - Added IGTheme support
	/// <summary>
	/// Handles coordination of CalendarBrushProviders for the 'IGTheme' color scheme.
	/// </summary>
	public class IGColorScheme : CalendarColorScheme
	{
		private ResourceDictionary _resources;
		private Style _scrollBarStyle;

		static internal ReadOnlyCollection<Color> s_baseColors = new ReadOnlyCollection<Color>(new Color[]
		{
			Color.FromArgb(255, 68, 172, 214 ),
			Color.FromArgb(255, 168, 168, 168 ),
			Color.FromArgb(255, 33, 110, 153 ),
			Color.FromArgb(255, 164, 186, 41 ),
			Color.FromArgb(255, 130, 130, 130 ),
			Color.FromArgb(255, 253, 189, 72 ),
			Color.FromArgb(255, 255, 106, 111 ),
			Color.FromArgb(255, 158, 111, 193 ),
			Color.FromArgb(255, 247, 144, 54 ),
			Color.FromArgb(255, 121, 59, 172 ),
			Color.FromArgb(255, 72, 137, 45 ),
			Color.FromArgb(255, 211, 64, 75 ),
            Color.FromArgb(255, 130, 130, 190 ),
            Color.FromArgb(255, 140, 190, 190 ),
			Color.FromArgb(255, 200, 167, 117 ),
		});

		// AS 3/9/12 TFS102032
		// Instead of the DataManager getting the dialog window resources we should have the color scheme do it.
		//
		private ResourceDictionary _lastRDCreated;

		private ResourceDictionary _lastMergedResources;
		private bool _mergedResourcesDirty;
		private int _dialogResourcesLastGroupingCount;


		#region Base class overrides

		#region BaseColors
		/// <summary>
		/// Returns a read-only collection of the base colors that are supported.
		/// </summary>
		/// 
		public override ReadOnlyCollection<Color> BaseColors { get { return s_baseColors; } }

		#endregion BaseColors

		#region CreateDialogResources

		/// <summary>
		/// Creates a ResourceDictionary that contains resources to be used by the dialogs
		/// </summary>
		/// <returns>A ResourceDictionary or null.</returns>
		protected override ResourceDictionary CreateDialogResources()
		{

			this._lastRDCreated = new ResourceDictionary();
			this._lastMergedResources = null;







			return this._lastRDCreated;
		}

		#endregion //CreateDialogResources	
 		
		#region DateNavigatorResourceProvider


		/// <summary>
		/// Returns an appropriate resource provider to be used by the <see cref="XamDateNavigator"/> (read-only).
		/// </summary>
		/// <seealso cref="XamDateNavigator"/>
		public override CalendarResourceProvider DateNavigatorResourceProvider
		{
			get
			{
				CalendarResourceProvider rp = base.DateNavigatorResourceProvider;

				rp.ResourceSet = CalendarResourceSet.IGTheme;

				return rp;
			}
		}

		#endregion //DateNavigatorResourceProvider
   
		#region GetBrushInfo

		internal override void GetBrushInfo(Color baseColor, CalendarBrushId id, ref Color? color, ref GradientStopCollection stops, ref Point gradientStart, ref Point gradientEnd)
		{

			bool isBlackThemeDefault = this.IsBlackThemeDefault(baseColor);
			bool isSilverThemeDefault = this.IsSilverThemeDefault(baseColor);

			switch (id)
			{
				case CalendarBrushId.CurrentDayBorderMonthCalendar:
				case CalendarBrushId.CurrentTimeIndicatorBackground:
				case CalendarBrushId.CurrentTimeIndicatorBorder:
				case CalendarBrushId.CurrentDayBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Color.FromArgb(255, 39, 136, 177);
					return;

				// JM 03-30-11 TFS69614
				case CalendarBrushId.DialogBackground:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
						color = Color.FromArgb(255, 145, 206, 230);
					return;

				case CalendarBrushId.CurrentDayHeaderBackground:
					if (this.IsHighContrast)
						color = SystemColors.ActiveCaptionColor;
					else
					{
						gradientStart = new Point(0.61d, 0d);
						gradientEnd = new Point(0.61d, 0.92d);
						stops = new GradientStopCollection();
						GradientStop stop = new GradientStop();
						stop.Color = Color.FromArgb(255, 59, 183, 235);
						stop.Offset = 0;
						stops.Add(stop);
						GradientStop stop2 = new GradientStop();
						stop2.Color = Color.FromArgb(255, 38, 134, 174);
						stop2.Offset = .98;
						stops.Add(stop2);
					}
					return;

				case CalendarBrushId.CurrentDayHeaderForeground:
					if (this.IsHighContrast)
						color = SystemColors.ActiveCaptionTextColor;
					else
						color = Colors.White;
					return;

				case CalendarBrushId.TimeslotHeaderTickmarkDayView:
				case CalendarBrushId.TimeslotHeaderTickmarkScheduleView:
					if (!this.IsHighContrast)
					{
						color = Color.FromArgb(255, 177, 177, 177);
						return;
					}
					break;
				case CalendarBrushId.TimeslotHeaderForegroundDayView:
				case CalendarBrushId.TimeslotHeaderForegroundScheduleView:
					if (!this.IsHighContrast)
					{
						color = Color.FromArgb(255, 57, 57, 57);
						return;
					}
					break;

				// JM 04-14-11 TFS72722
				case CalendarBrushId.RibbonLiteBackgroundBrush:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
					{
						gradientStart.X = 0;
						gradientStart.Y = 0;
						gradientEnd.X	= 0;
						gradientEnd.Y	= 1;
						stops = new GradientStopCollection();

						GradientStop stop	= new GradientStop();
						stop.Color			= Color.FromArgb(255, 255, 255, 255);
						stop.Offset			= 0;
						stops.Add(stop);

						GradientStop stop2	= new GradientStop();
						stop2.Color			= Color.FromArgb(255, 230, 230, 230);
						stop2.Offset		= 1;
						stops.Add(stop2);
					}

					return;

				// JM 04-14-11 TFS72722
				case CalendarBrushId.RibbonLiteGroupOuterBorderBrush:
					if (this.IsHighContrast)
						color = SystemColors.ActiveBorderColor;
					else
					{
						gradientStart.X = 0;
						gradientStart.Y = 0;
						gradientEnd.X	= 0;
						gradientEnd.Y	= 1;

						stops			= new GradientStopCollection();

						GradientStop stop	= new GradientStop();
						stop.Color			= Color.FromArgb(50, 107, 121, 138);
						stop.Offset			= 0;
						stops.Add(stop);

						GradientStop stop2	= new GradientStop();
						stop2.Color			= Color.FromArgb(255, 107, 121, 138);
						stop2.Offset		= 1;
						stops.Add(stop2);
					}

					return;

				// JM 04-14-11 TFS72722
				case CalendarBrushId.RibbonLiteGroupInnerBorderBrush:
					if (this.IsHighContrast)
						color = SystemColors.WindowColor;
					else
					{
						gradientStart.X = 0;
						gradientStart.Y = 0;
						gradientEnd.X	= 0;
						gradientEnd.Y	= 1;

						stops			= new GradientStopCollection();

						GradientStop stop	= new GradientStop();
						stop.Color			= Color.FromArgb(50, 255, 255, 255);
						stop.Offset			= 0;
						stops.Add(stop);

						GradientStop stop2	= new GradientStop();
						stop2.Color			= Color.FromArgb(255, 255, 255, 255);
						stop2.Offset		= 1;
						stops.Add(stop2);
					}

					return;
			}

			base.GetBrushInfo(baseColor, id, ref color, ref stops, ref gradientStart, ref gradientEnd);
		}

		#endregion //GetBrushInfo

		// AS 3/19/12 TFS105110
		#region OnResourceWasherChanged
		internal override void OnResourceWasherChanged()
		{
			_resources = null;
			base.OnResourceWasherChanged();
		}
		#endregion //OnResourceWasherChanged

		#region ScrollBarStyle
		/// <summary>
		/// Returns a scrollbar style or null (read-only).
		/// </summary>
		internal protected override Style ScrollBarStyle
		{
			get
			{
				if (this.IsHighContrast)
					return null;

				if (this._resources == null)
				{
					this._resources = new ResourceDictionary();

					this._resources.Source = CoreUtilities.BuildEmbeddedResourceUri(typeof(OfficeColorSchemeBase).Assembly, "themes/scrollbar.ig.wpf.xaml");





					// AS 3/19/12 TFS105110
					var washer = ((IResourceWasherTarget)this).ResourceWasher;
					if (null != washer)
						_resources = (ResourceDictionary)washer.CreateWashedResource(_resources);


					// Harry: uncomment out the following 2 lines of code to load the scrollbar styles
					this._scrollBarStyle = this._resources["ScrollbarStyle"] as Style;
					Debug.Assert(this._scrollBarStyle != null, "Scrollbar style not found");
				}
				return this._scrollBarStyle;
			}
		}

		#endregion ScrollBarStyle

		#region VerifyDialogResources

		/// <summary>
		/// Makes sure that the dialog resources dictonary is polpulated correctly
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: this method is always called from the get of DialogResources property. This gives derived classes an opportunity to update the contents before a dialog is shown.</para>
		/// </remarks>
		protected override void VerifyDialogResources()
		{

			if (this._lastRDCreated == null)
				return;

			string theme = Infragistics.Windows.Themes.ThemeManager.ThemeNameIGTheme;

			string[] groupings = Infragistics.Windows.Themes.ThemeManager.GetGroupingsForTheme(theme);

			// check the number of grouping to pick up any late registrations
			if (this._lastMergedResources == null || this._mergedResourcesDirty || groupings.Length > this._dialogResourcesLastGroupingCount)
			{
				this._mergedResourcesDirty = false; ;
				this._dialogResourcesLastGroupingCount = groupings.Length;

				if (this._lastMergedResources != null)
				{
					int index = this._lastRDCreated.MergedDictionaries.IndexOf(this._lastMergedResources);

					if (index >= 0)
						this._lastRDCreated.MergedDictionaries.RemoveAt(index);
				}

				this._lastMergedResources = Infragistics.Windows.Themes.ThemeManager.GetResourceSet(theme, Infragistics.Windows.Themes.ThemeManager.AllGroupingsLiteral);

				if (this._lastMergedResources != null)
					this._lastRDCreated.MergedDictionaries.Add(this._lastMergedResources);
			}


		}

		#endregion //VerifyDialogResources

		#endregion //Base class overrides
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