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
	#region OfficeColorScheme class

	/// <summary>
	/// Handles coordination of CalendarBrushProviders for Office color schemes.
	/// </summary>
	public abstract class OfficeColorSchemeBase : CalendarColorScheme
	{
		#region Member variables

		private OfficeColorScheme _colorScheme;


		private ResourceDictionary _lastRDCreated;
		private ResourceDictionary _lastMergedResources;
		private bool _mergedResourcesDirty;
		private int _dialogResourcesLastGroupingCount;


		internal const int baseColorIndexBlue = 0;
		internal const int baseColorIndexBlack = 3;

		//private WeakDictionary<Color, Brush> _solidColorBrushCache = new WeakDictionary<Color, Brush>(false, true);

		static internal ReadOnlyCollection<Color> s_baseColors = new ReadOnlyCollection<Color>(new Color[]
		{
			Color.FromArgb(255, 141, 174, 217 ),
			Color.FromArgb(255, 156, 191, 139 ),
			Color.FromArgb(255, 209, 149, 170 ),
			Color.FromArgb(255, 176, 182, 190 ),
			Color.FromArgb(255, 140, 191, 192 ),
			Color.FromArgb(255, 140, 140, 215 ),
			Color.FromArgb(255, 141, 193, 157 ),
			Color.FromArgb(255, 211, 150, 150 ),
			Color.FromArgb(255, 186, 186, 137 ),
			Color.FromArgb(255, 174, 153, 216 ),
			Color.FromArgb(255, 195, 176, 141 ),
			Color.FromArgb(255, 139, 191, 174 ),
			Color.FromArgb(255, 144, 182, 200 ),
			Color.FromArgb(255, 255, 223, 134 ),
			Color.FromArgb(255, 150, 169, 209 )
		});

		#endregion Member variables

		#region Constructor





		internal OfficeColorSchemeBase()
		{
		}
		

		#endregion Constructor

		#region Base class overrides

		#region BaseColors
		/// <summary>
		/// Returns a read-only collection of the base colors that are supported.
		/// </summary>
		/// 
		public override ReadOnlyCollection<Color> BaseColors { get { return s_baseColors; } }

		#endregion BaseColors

		#region CreateBrush
		
#region Infragistics Source Cleanup (Region)

























































#endregion // Infragistics Source Cleanup (Region)

		#endregion //CreateBrush

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
    
		#region DefaultBaseColor
		/// <summary>
		/// Returns the base color to use for the default provider.
		/// </summary>
		protected override Color DefaultBaseColor { get { return this._colorScheme == OfficeColorScheme.Blue ? s_baseColors[baseColorIndexBlue] : s_baseColors[baseColorIndexBlack]; } }

		#endregion DefaultBaseColor
		
		#region GetBrushInfo

		internal override void GetBrushInfo(Color baseColor, CalendarBrushId id, ref Color? color, ref GradientStopCollection stops, ref Point gradientStart, ref Point gradientEnd)
		{
			OfficeColorScheme colorScheme = this.OfficeColorScheme;

			bool isBlackThemeDefault = colorScheme == OfficeColorScheme.Black && baseColor == s_baseColors[baseColorIndexBlack];
			bool isSilverThemeDefault = colorScheme == OfficeColorScheme.Silver && baseColor == s_baseColors[baseColorIndexBlack];

			switch (id)
			{
				case CalendarBrushId.DialogBackground:
					// JJD 4/14/11 - TFS71495
					// For HighContrast we can fall thru to pick up the base class value
					if (!this.IsHighContrast)
					{
						switch (this.OfficeColorScheme)
						{
							case Schedules.OfficeColorScheme.Black:
								color = Color.FromArgb(255, 131, 131, 131);
								break;
							case Schedules.OfficeColorScheme.Blue:
								color = Color.FromArgb(255, 207, 221, 238);
								break;
							case Schedules.OfficeColorScheme.Silver:
								color = Color.FromArgb(255, 233, 237, 241);
								break;
						}
						// JJD 4/14/11 - TFS71495
						// Return so we don't call the base implementation below
						return;
					}

					break;

				case CalendarBrushId.DialogForeground:
					// JJD 4/14/11 - TFS71495
					// For HighContrast we can fall thru to pick up the base class value
					if (!this.IsHighContrast)
					{
						switch (this.OfficeColorScheme)
						{
							case Schedules.OfficeColorScheme.Black:
								color = Colors.White;
								break;
							case Schedules.OfficeColorScheme.Blue:
								color = Colors.Black;
								break;
							case Schedules.OfficeColorScheme.Silver:
								color = Colors.Black;
								break;
						}
						// JJD 4/14/11 - TFS71495
						// Return so we don't call the base implementation below
						return;
					}

					break;

				case CalendarBrushId.RibbonLiteBackgroundBrush:
					// JJD 4/14/11 - TFS71495
					// For HighContrast we can fall thru to pick up the base class value
					if (!this.IsHighContrast)
					{
						gradientStart.X = 0;
						gradientStart.Y = 0;
						gradientEnd.X = 0;
						gradientEnd.Y = 1;

						switch (this.OfficeColorScheme)
						{
							case Schedules.OfficeColorScheme.Black:
								{
									stops = new GradientStopCollection();

									GradientStop stop = new GradientStop();
									stop.Color = Color.FromArgb(255, 204, 204, 204);
									stop.Offset = 0;
									stops.Add(stop);

									GradientStop stop2 = new GradientStop();
									stop2.Color = Colors.DarkGray;
									stop2.Offset = 1;
									stops.Add(stop2);

									break;
								}
							case Schedules.OfficeColorScheme.Blue:
								{
									stops = new GradientStopCollection();

									GradientStop stop = new GradientStop();
									stop.Color = Color.FromArgb(255, 225, 234, 246);
									stop.Offset = 0;
									stops.Add(stop);

									GradientStop stop2 = new GradientStop();
									stop2.Color = Color.FromArgb(255, 199, 215, 234);
									stop2.Offset = 1;
									stops.Add(stop2);

									break;
								}
							case Schedules.OfficeColorScheme.Silver:
								{
									stops = new GradientStopCollection();

									GradientStop stop = new GradientStop();
									stop.Color = Color.FromArgb(255, 255, 255, 255);
									stop.Offset = 0;
									stops.Add(stop);

									GradientStop stop2 = new GradientStop();
									stop2.Color = Colors.LightGray;
									stop2.Offset = 1;
									stops.Add(stop2);

									break;
								}
						}

						// JJD 4/14/11 - TFS71495
						// Return so we don't call the base implementation below
						return;
					}

					break;

				case CalendarBrushId.RibbonLiteGroupOuterBorderBrush:
					// JJD 4/14/11 - TFS71495
					// For HighContrast we can fall thru to pick up the base class value
					if (!this.IsHighContrast)
					{
						gradientStart.X = 0;
						gradientStart.Y = 0;
						gradientEnd.X = 0;
						gradientEnd.Y = 1;

						switch (this.OfficeColorScheme)
						{
							case Schedules.OfficeColorScheme.Black:
								{
									stops = new GradientStopCollection();

									GradientStop stop = new GradientStop();
									stop.Color = Color.FromArgb(50, 128, 128, 128);
									stop.Offset = 0;
									stops.Add(stop);

									GradientStop stop2 = new GradientStop();
									stop2.Color = Color.FromArgb(255, 128, 128, 128);
									stop2.Offset = 1;
									stops.Add(stop2);

									break;
								}
							case Schedules.OfficeColorScheme.Blue:
								{
									stops = new GradientStopCollection();

									GradientStop stop = new GradientStop();
									stop.Color = Color.FromArgb(50, 107, 121, 138);
									stop.Offset = 0;
									stops.Add(stop);

									GradientStop stop2 = new GradientStop();
									stop2.Color = Color.FromArgb(255, 107, 121, 138);
									stop2.Offset = 1;
									stops.Add(stop2);

									break;
								}
							case Schedules.OfficeColorScheme.Silver:
								{
									stops = new GradientStopCollection();

									GradientStop stop = new GradientStop();
									stop.Color = Color.FromArgb(50, 128, 128, 128);
									stop.Offset = 0;
									stops.Add(stop);

									GradientStop stop2 = new GradientStop();
									stop2.Color = Color.FromArgb(255, 128, 128, 128);
									stop2.Offset = 1;
									stops.Add(stop2);

									break;
								}
						}

						// JJD 4/14/11 - TFS71495
						// Return so we don't call the base implementation below
						return;
					}

					break;

				case CalendarBrushId.RibbonLiteGroupInnerBorderBrush:
					// JJD 4/14/11 - TFS71495
					// For HighContrast we can fall thru to pick up the base class value
					if (!this.IsHighContrast)
					{
						gradientStart.X = 0;
						gradientStart.Y = 0;
						gradientEnd.X = 0;
						gradientEnd.Y = 1;

						switch (this.OfficeColorScheme)
						{
							case Schedules.OfficeColorScheme.Black:
								{
									stops = new GradientStopCollection();

									GradientStop stop = new GradientStop();
									stop.Color = Color.FromArgb(50, 255, 255, 255);
									stop.Offset = 0;
									stops.Add(stop);

									GradientStop stop2 = new GradientStop();
									stop2.Color = Color.FromArgb(255, 255, 255, 255);
									stop2.Offset = 1;
									stops.Add(stop2);

									break;
								}
							case Schedules.OfficeColorScheme.Blue:
								{
									stops = new GradientStopCollection();

									GradientStop stop = new GradientStop();
									stop.Color = Color.FromArgb(50, 255, 255, 255);
									stop.Offset = 0;
									stops.Add(stop);

									GradientStop stop2 = new GradientStop();
									stop2.Color = Color.FromArgb(255, 255, 255, 255);
									stop2.Offset = 1;
									stops.Add(stop2);

									break;
								}
							case Schedules.OfficeColorScheme.Silver:
								{
									stops = new GradientStopCollection();

									GradientStop stop = new GradientStop();
									stop.Color = Color.FromArgb(50, 255, 255, 255);
									stop.Offset = 0;
									stops.Add(stop);

									GradientStop stop2 = new GradientStop();
									stop2.Color = Color.FromArgb(255, 255, 255, 255);
									stop2.Offset = 1;
									stops.Add(stop2);

									break;
								}
						}
						// JJD 4/14/11 - TFS71495
						// Return so we don't call the base implementation below
						return;
					}

					break;
			}

			base.GetBrushInfo(baseColor, id, ref color, ref stops, ref gradientStart, ref gradientEnd);
		}

		#endregion //GetBrushInfo	
		
		// JJD 10/29/10 - NA 2011 Volume 1 - Added IGTheme support
		#region IsBlackThemeDefault

		internal override bool IsBlackThemeDefault(Color color)
		{
			return this._colorScheme == OfficeColorScheme.Black && color == s_baseColors[baseColorIndexBlack];
		}

		#endregion //IsBlackThemeDefault	
    
		// JJD 10/29/10 - NA 2011 Volume 1 - Added IGTheme support
		#region IsSilverThemeDefault

		internal override bool IsSilverThemeDefault(Color color)
		{
			return this._colorScheme == OfficeColorScheme.Silver && color == s_baseColors[baseColorIndexBlack];
		}

		#endregion //IsSilverThemeDefault	
    
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

			string theme;

			switch (this.OfficeColorScheme)
			{
				case Schedules.OfficeColorScheme.Black:
					theme = Infragistics.Windows.Themes.ThemeManager.ThemeNameOffice2k7Black;
					break;
				case Schedules.OfficeColorScheme.Silver:
					theme = Infragistics.Windows.Themes.ThemeManager.ThemeNameOffice2k7Silver;
					break;
				default:
				case Schedules.OfficeColorScheme.Blue:
					if ( this is Office2007ColorScheme )
						theme = Infragistics.Windows.Themes.ThemeManager.ThemeNameOffice2k7Blue;
					else
						theme = Infragistics.Windows.Themes.ThemeManager.ThemeNameOffice2010Blue;
					break;
			}

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

				if ( this._lastMergedResources != null )
					this._lastRDCreated.MergedDictionaries.Add(this._lastMergedResources);
			}


		}

		#endregion //VerifyDialogResources

		#endregion //Base class overrides

		#region Properties

		#region OfficeColorScheme

		/// <summary>
		/// Identifies the <see cref="OfficeColorScheme"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OfficeColorSchemeProperty = DependencyPropertyUtilities.Register(
	  "OfficeColorScheme", typeof(OfficeColorScheme), typeof(OfficeColorSchemeBase),
			DependencyPropertyUtilities.CreateMetadata(OfficeColorScheme.Blue, new PropertyChangedCallback(OnOfficeColorSchemeChanged))
			);

		private static void OnOfficeColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			OfficeColorSchemeBase item = (OfficeColorSchemeBase)d;

			item._colorScheme = (OfficeColorScheme)e.NewValue;


			item._mergedResourcesDirty = true;


			item.InvalidateBrushCache();

			ScheduleUtilities.NotifyListenersHelper(item, e, item.PropChangeListeners, false, false);

		}

		/// <summary>
		/// Determines the overall color scheme to use for all controls
		/// </summary>
		/// <seealso cref="OfficeColorSchemeProperty"/>
		public OfficeColorScheme OfficeColorScheme
		{
			get
			{
				return this._colorScheme;
			}
			set
			{
				this.SetValue(OfficeColorSchemeBase.OfficeColorSchemeProperty, value);
			}
		}

		#endregion //OfficeColorScheme

		#endregion //Properties

		#region Private Propeties

		#endregion //Private Propeties

		#region Methods

		#region Internal Methods

		
#region Infragistics Source Cleanup (Region)





















































































































































































































































































































































































































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


		#endregion //Internal Methods

		#endregion //Methods

	}

	#endregion //OfficeColorScheme class

	/// <summary>
	/// Handles coordination of CalendarBrushProviders for Office 2007 color schemes.
	/// </summary>
	public class Office2007ColorScheme : OfficeColorSchemeBase
	{

		#region Base class overrides

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

				CalendarResourceSet rset;

				switch (this.OfficeColorScheme)
				{
					case Schedules.OfficeColorScheme.Black:
						rset = CalendarResourceSet.Office2007Black;
						break;
					case Schedules.OfficeColorScheme.Silver:
						rset = CalendarResourceSet.Office2007Silver;
						break;
					default:
						rset = CalendarResourceSet.Office2007Blue;
						break;
				}

				rp.ResourceSet = rset;

				return rp;
			}
		}

		#endregion //DateNavigatorResourceProvider

		#endregion //Base class overrides
	}

	/// <summary>
	/// Handles coordination of CalendarBrushProviders for Office 2010 color schemes.
	/// </summary>
	public class Office2010ColorScheme : OfficeColorSchemeBase
	{
		private ResourceDictionary _resources;
		private Style _scrollBarStyle;

		#region Base class overrides

		#region GetBrushInfo

		internal override void GetBrushInfo(Color baseColor, CalendarBrushId id, ref Color? color, ref GradientStopCollection stops, ref Point gradientStart, ref Point gradientEnd)
		{
			OfficeColorScheme colorScheme = this.OfficeColorScheme;

			bool isBlackThemeDefault = colorScheme == OfficeColorScheme.Black && baseColor == s_baseColors[baseColorIndexBlack];
			bool isSilverThemeDefault = colorScheme == OfficeColorScheme.Silver && baseColor == s_baseColors[baseColorIndexBlack];

			switch (id)
			{
				case CalendarBrushId.CurrentDayBorder:
					if (this.IsHighContrast)
						color = SystemColors.ControlTextColor;
					else
						color = Color.FromArgb(255, 235, 137, 0);
					return;

				case CalendarBrushId.CurrentDayHeaderBackground:
					if (this.IsHighContrast)
						color = SystemColors.ActiveCaptionColor;
					else
					{
						stops = new GradientStopCollection();
						GradientStop stop = new GradientStop();
						stop.Color = Color.FromArgb(255, 255, 237, 121);
						stop.Offset = 0;
						stops.Add(stop);
						GradientStop stop2 = new GradientStop();
						stop2.Color = Color.FromArgb(255, 255, 218, 108);
						stop2.Offset = .5;
						stops.Add(stop2);
						GradientStop stop3 = new GradientStop();
						stop3.Color = Color.FromArgb(255, 255, 234, 199);
						stop3.Offset = 1;
						stops.Add(stop3);
					}
					return;
			}

			base.GetBrushInfo(baseColor, id, ref color, ref stops, ref gradientStart, ref gradientEnd);
		}

		#endregion //GetBrushInfo	
 		
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

				CalendarResourceSet rset;

				switch (this.OfficeColorScheme)
				{
					case Schedules.OfficeColorScheme.Black:
						rset = CalendarResourceSet.Office2010Black;
						break;
					case Schedules.OfficeColorScheme.Silver:
						rset = CalendarResourceSet.Office2010Silver;
						break;
					default:
						rset = CalendarResourceSet.Office2010Blue;
						break;
				}

				rp.ResourceSet = rset;

				return rp;
			}
		}

		#endregion //DateNavigatorResourceProvider

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

					this._resources.Source = CoreUtilities.BuildEmbeddedResourceUri(typeof(OfficeColorSchemeBase).Assembly, "themes/scrollbar.wpf.xaml");





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