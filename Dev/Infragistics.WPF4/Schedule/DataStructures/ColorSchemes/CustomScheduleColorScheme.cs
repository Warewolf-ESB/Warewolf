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
using System.Windows.Markup;
using System.Collections.ObjectModel;
using Infragistics.Controls.Primitives;
using Infragistics.Controls.Schedules.Primitives;
using System.Collections.Specialized;
using System.ComponentModel;


using Infragistics.Windows.Themes;


namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Custom <see cref="CalendarColorScheme"/>
	/// </summary>
	/// <remarks>
	/// <p class="body">The <see cref="CustomBaseColors"/> define the base colors used by the calendars. The <see cref="ResourceOverrides"/> 
	/// and <see cref="ResourceOverridesHighContrast"/> may be used to override individual brush resources. The key of the brushes in those 
	/// dictionaries would be the string name of the <see cref="CalendarBrushId"/> member it is meant to override.</p>
	/// </remarks>
	[ContentProperty("CustomBaseColors")]
	public class CustomScheduleColorScheme : CalendarColorScheme
		, ISupportInitialize
	{
		#region Member Variables

		private ObservableCollection<Color> _calendarBaseColors;
		private ReadOnlyCollection<Color> _readOnlyBaseColors;
		private ScheduleResourceProvider _resourceProvider;
		private bool _isInitializing;

		private static ReadOnlyCollection<Color> _defaultBaseColors;

		#endregion //Member Variables

		#region Constructor
		static CustomScheduleColorScheme()
		{
			_defaultBaseColors = new ReadOnlyCollection<Color>(new Color[] { Color.FromArgb(255, 141, 174, 217) });
		}

		/// <summary>
		/// Initializes a new <see cref="CustomScheduleColorScheme"/>
		/// </summary>
		public CustomScheduleColorScheme()
		{
			_calendarBaseColors = new ObservableCollection<Color>();
			_calendarBaseColors.CollectionChanged += new NotifyCollectionChangedEventHandler(OnBaseColorsChanged);

			_readOnlyBaseColors = new ReadOnlyCollection<Color>(_calendarBaseColors);
			_resourceProvider = new ScheduleResourceProvider();
		}
		#endregion //Constructor

		#region Base class overrides

		#region BaseColors
		/// <summary>
		/// Returns the collection of colors used by the control.
		/// </summary>
		[Browsable(false)]
		public override ReadOnlyCollection<Color> BaseColors
		{
			get
			{
				this.VerifyHasBaseColors();

				if (_readOnlyBaseColors.Count == 0)
					return _defaultBaseColors;

				return _readOnlyBaseColors;
			}
		}
		#endregion //BaseColors

		#region CreateBrush
		/// <summary>
		/// Creates a Brush based on a specific id
		/// </summary>
		/// <param name="id">The id of the brush to create</param>
		/// <param name="baseColor">The base color to use</param>
		/// <returns>The brush to se for the specified id</returns>
		/// <seealso cref="CalendarBrushId"/>
		internal protected override Brush CreateBrush(CalendarBrushId id, Color baseColor)
		{
			if (_resourceProvider != null)
			{
				var resource = _resourceProvider[id];

				if (resource is Brush)
				{

					var washer = ((IResourceWasherTarget)this).ResourceWasher;

					if (null != washer)
					{
						// AS 4/3/12 TFS107893
						//washer.CreateWashedResource(washer);
						resource = washer.CreateWashedResource(resource);
					}


					return resource as Brush;
				}
			}

			return base.CreateBrush(id, baseColor);
		}
		#endregion //CreateBrush

		#region DefaultBaseColor
		/// <summary>
		/// Returns the base color to use for the default provider.
		/// </summary>
		protected override Color DefaultBaseColor
		{
			get
			{
				this.VerifyHasBaseColors();

				if (_readOnlyBaseColors.Count == 0)
					return _defaultBaseColors[0];


				return this.BaseColors[0];
			}
		}

		#endregion DefaultBaseColor

		#endregion //Base class overrides

		#region Properties

		#region CustomrBaseColors
		/// <summary>
		/// Returns a modifiable collection of base colors to be used by the schedule controls.
		/// </summary>
		public ObservableCollection<Color> CustomBaseColors
		{
			get { return _calendarBaseColors; }
		}
		#endregion //CustomBaseColors

		#region ResourceOverrides

		/// <summary>
		/// Identifies the <see cref="ResourceOverrides"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResourceOverridesProperty = DependencyPropertyUtilities.Register("ResourceOverrides",
			typeof(ResourceDictionary), typeof(CustomScheduleColorScheme), null, new PropertyChangedCallback(OnResourceOverridesChanged));

		private static void OnResourceOverridesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CustomScheduleColorScheme instance = (CustomScheduleColorScheme)d;
			instance._resourceProvider.ResourceOverrides = e.NewValue as ResourceDictionary;
			instance.InvalidateBrushCacheHelper();
		}

		/// <summary>
		/// Gets/sets an optional <see cref="ResourceDictionary"/> that contains resources keyed by the enum values of T that will be used instead of the the corresponding defalse values.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> In WPF the x:Key used for the resoure can either be the enum value or its exact string equivalent. 
		/// In Silverlight however, since that framework only supports keys that are strings or Types, you can only use the string equivalent as the key.</para>
		/// </remarks>
		/// <seealso cref="ResourceOverridesProperty"/>
		public ResourceDictionary ResourceOverrides
		{
			get
			{
				return (ResourceDictionary)this.GetValue(CustomScheduleColorScheme.ResourceOverridesProperty);
			}
			set
			{
				this.SetValue(CustomScheduleColorScheme.ResourceOverridesProperty, value);
			}
		}

		#endregion //ResourceOverrides

		#region ResourceOverridesHighContrast

		/// <summary>
		/// Identifies the <see cref="ResourceOverridesHighContrast"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResourceOverridesHighContrastProperty = DependencyPropertyUtilities.Register("ResourceOverridesHighContrast",
			typeof(ResourceDictionary), typeof(CustomScheduleColorScheme), null, new PropertyChangedCallback(OnResourceOverridesHighContrastChanged));

		private static void OnResourceOverridesHighContrastChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CustomScheduleColorScheme instance = (CustomScheduleColorScheme)d;
			instance._resourceProvider.ResourceOverridesHighContrast = e.NewValue as ResourceDictionary;
			instance.InvalidateBrushCacheHelper();
		}

		/// <summary>
		/// Gets/sets an optional <see cref="ResourceDictionary"/> that contains resources keyed by the enum values of T that will be used instead of the the corresponding defalse values.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> In WPF the x:Key used for the resoure can either be the enum value or its exact string equivalent. 
		/// In Silverlight however, since that framework only supports keys that are strings or Types, you can only use the string equivalent as the key.</para>
		/// </remarks>
		/// <seealso cref="ResourceOverridesHighContrastProperty"/>
		public ResourceDictionary ResourceOverridesHighContrast
		{
			get
			{
				return (ResourceDictionary)this.GetValue(CustomScheduleColorScheme.ResourceOverridesHighContrastProperty);
			}
			set
			{
				this.SetValue(CustomScheduleColorScheme.ResourceOverridesHighContrastProperty, value);
			}
		}

		#endregion //ResourceOverridesHighContrast

		#endregion //Properties

		#region Methods

		#region InvalidateBrushCacheHelper
		private void InvalidateBrushCacheHelper()
		{
			if (_isInitializing)
				return;

			this.InvalidateBrushCache();
		}
		#endregion //InvalidateBrushCacheHelper

		#region OnBaseColorsChanged
		private void OnBaseColorsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.InvalidateBrushCacheHelper();
		}
		#endregion //OnBaseColorsChanged

		#region VerifyHasBaseColors
		private void VerifyHasBaseColors()
		{
			if (_readOnlyBaseColors.Count == 0 && !DesignerProperties.GetIsInDesignMode(this))
				throw new InvalidOperationException("The 'CustomBaseColors' must contain at least 1 Color.");
		}
		#endregion //VerifyHasBaseColors

		#endregion //Methods

		#region ISupportInitialize members
		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			_isInitializing = false;
		}
		#endregion //ISupportInitialize members

		#region ScheduleResourceProvider class
		/// <summary>
		/// A class that exposes an indexer to retrieve resources used by elements within the XamSchedule controls
		/// </summary>
		private class ScheduleResourceProvider : ResourceProviderWithOverrides<CalendarBrushId>
		{
			#region IsResourceValid
			/// <summary>
			/// Indicates if the specified resource is of the required type.
			/// </summary>
			/// <param name="id">The id of the resource</param>
			/// <param name="resource">The value for the specified resource that is to be evaluated</param>
			/// <returns>Returns true if the resource is valid and of the expected type; otherwise false is returned.</returns>
			protected override bool IsResourceValid(CalendarBrushId id, object resource)
			{
				return resource is Brush;
			}
			#endregion //IsResourceValid
		}
		#endregion //ScheduleResourceProvider class
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