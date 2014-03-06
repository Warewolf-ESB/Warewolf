using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Infragistics.Windows;

namespace Infragistics.Windows.Themes
{
	#region PrimitivesResourceSet<T> base class

	/// <summary>
	/// Abstract base class used to supply style resources for a specific look for primitive elements that are shared.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class PrimitivesResourceSet<T> : ResourceSet where T : ResourceSetLocator, new()
	{
		#region Constants

		// AS 11/6/07 ThemeGroupingName
		//static internal readonly string GroupingName = "Primitives";
		internal const string GroupingName = "Primitives";

		#endregion //Constants

		#region Base class overrides

		#region Grouping

		/// <summary>
		/// Returns the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: DataPresenter, Editors, Primitives, WPF etc.
		/// </remarks>
		public override string Grouping
		{
			get { return Location.Grouping; }
		}

		#endregion //Grouping

		#region Theme

		/// <summary>
		/// Returns the name of the look (read-only)
		/// </summary>
		public override string Theme
		{
			get
			{
				return Location.Theme;
			}
		}

		#endregion //Theme

		#region Resources

		/// <summary>
		/// Returns the ResourceDictionary containing the associated styles (read-only)
		/// </summary>
		public override ResourceDictionary Resources
		{
			get
			{
				//return ResourcesInternal;
				ResourceDictionary rd = ResourcesInternal;

				// JJD 7/23/07 - ResourceWasher support
				// Call VerifyResources after the initial load so that we can delay the hydrating
				// of the resources by a ResourceWasher until this theme is actually used
				this.VerifyResources();

				return rd;
			}
		}

		#endregion //Resources

		#endregion //Base class overrides

		#region Static Properties

		#region Private Properties

		#region Location

		private static ResourceSetLocator g_Location;

		// AS 5/7/08
		/// <summary>
		/// Returns the <see cref="ResourceSetLocator"/> that describes the theme information for the resource set.
		/// </summary>
		//private static ResourceSetLocator Location
		public static ResourceSetLocator Location
		{
			get
			{
				if (g_Location == null)
					g_Location = new T();

				return g_Location;
			}
		}

		#endregion //Location

		#region ResourcesInternal

		private static ResourceDictionary g_ResourcesInternal;

		private static ResourceDictionary ResourcesInternal
		{
			get
			{
				if (g_ResourcesInternal == null)
				{
					g_ResourcesInternal = Utilities.CreateResourceSetDictionary(Location.Assembly, Location.ResourcePath);
				}

				return g_ResourcesInternal;
			}
		}

		#endregion //ResourcesInternal

		#endregion //Private Propeties

        // JJD 1/12/09 - NA 2009 vol 1 - record filtering
		#region ComparisonOperatorSelector

		private static Style g_ComparisonOperatorSelector;

		/// <summary>
        /// The style for a <see cref="ComparisonOperatorSelector"/>.
		/// </summary>
		public static Style ComparisonOperatorSelector
		{
			get
			{
				if (g_ComparisonOperatorSelector == null)
					g_ComparisonOperatorSelector = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.ComparisonOperatorSelector)) as Style;

				return g_ComparisonOperatorSelector;
			}
		}

		#endregion //ComparisonOperatorSelector

		
		
		#region DropIndicator

		private static Style g_DropIndicator;

		/// <summary>
		/// The style for the <see cref="DropIndicator"/> control.
		/// </summary>
		public static Style DropIndicator
		{
			get
			{
				if ( g_DropIndicator == null )
					g_DropIndicator = ResourceSet.GetSealedResource( ResourcesInternal, typeof( Infragistics.Windows.Controls.DropIndicator ) ) as Style;

				return g_DropIndicator;
			}
		}

		#endregion //DropIndicator

		#region EmbeddedTextBox

		private static Style g_EmbeddedTextBox;

		/// <summary>
		/// The style for an EmbeddedTextBox.
		/// </summary>
		public static Style EmbeddedTextBox
		{
			get
			{
				if (g_EmbeddedTextBox == null)
					g_EmbeddedTextBox = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Controls.Primitives.EmbeddedTextBox)) as Style;

				return g_EmbeddedTextBox;
			}
		}

		#endregion //EmbeddedTextBox

		#region ExpanderBar

		private static Style g_ExpanderBar;

		/// <summary>
		/// The style for an expansion indicator.
		/// </summary>
		public static Style ExpanderBar
		{
			get
			{
				if (g_ExpanderBar == null)
					g_ExpanderBar = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.ExpanderBar)) as Style;

				return g_ExpanderBar;
			}
		}

		#endregion //ExpanderBar

		#region ExpansionIndicator

		private static Style g_ExpansionIndicator;

		/// <summary>
		/// The style for an expansion indicator.
		/// </summary>
		public static Style ExpansionIndicator
		{
			get
			{
				if (g_ExpansionIndicator == null)
					g_ExpansionIndicator = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.ExpansionIndicator)) as Style;

				return g_ExpansionIndicator;
			}
		}

		#endregion //ExpansionIndicator

		#region CarouselListBox related styles



		#region XamCarouselListBox

		private static Style g_XamCarouselListBox;

		/// <summary>
		/// The style for a XamCarouselListBox.
		/// </summary>
		public static Style XamCarouselListBox
		{
			get
			{
				if (g_XamCarouselListBox == null)
					g_XamCarouselListBox = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.XamCarouselListBox)) as Style;

				return g_XamCarouselListBox;
			}
		}

		#endregion //XamCarouselListBox

		#region CarouselPanelItem

		private static Style g_CarouselPanelItem;

		/// <summary>
		/// The style for a CarouselPanelItem.
		/// </summary>
		public static Style CarouselPanelItem
		{
			get
			{
				if (g_CarouselPanelItem == null)
					g_CarouselPanelItem = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.CarouselPanelItem)) as Style;

				return g_CarouselPanelItem;
			}
		}

		#endregion //CarouselPanelItem

		#region CarouselListBoxItem

		private static Style g_CarouselListBoxItem;

		/// <summary>
		/// The style for a CarouselListBoxItem.
		/// </summary>
		public static Style CarouselListBoxItem
		{
			get
			{
				if (g_CarouselListBoxItem == null)
					g_CarouselListBoxItem = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.CarouselListBoxItem)) as Style;

				return g_CarouselListBoxItem;
			}
		}

		#endregion //CarouselListBoxItem

		#region CarouselPanelNavigator

		private static Style g_CarouselPanelNavigator;

		/// <summary>
		/// The style for a CarouselPanelNavigator.
		/// </summary>
		public static Style CarouselPanelNavigator
		{
			get
			{
				if (g_CarouselPanelNavigator == null)
					g_CarouselPanelNavigator = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.CarouselPanelNavigator)) as Style;

				return g_CarouselPanelNavigator;
			}
		}

		#endregion //CarouselPanelNavigator


		#endregion //CarouselListBox releated styles


		// JJD 8/23/07 PopupResizerBar
		#region PopupResizerBar

		private static Style g_PopupResizerBar;

		/// <summary>
		/// The style for the <see cref="PopupResizerBar"/> control.
		/// </summary>
		public static Style PopupResizerBar
		{
			get
			{
				if (g_PopupResizerBar == null)
					g_PopupResizerBar = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.PopupResizerBar)) as Style;

				return g_PopupResizerBar;
			}
		}

		#endregion //PopupResizerBar


		#region SortIndicator

		private static Style g_SortIndicator;

		/// <summary>
		/// The style for sort indictators.
		/// </summary>
		public static Style SortIndicator
		{
			get
			{
				if (g_SortIndicator == null)
					g_SortIndicator = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.SortIndicator)) as Style;

				return g_SortIndicator;
			}
		}

		#endregion //SortIndicator

		// AS 7/25/07 XamPager
		#region XamPager

		private static Style g_XamPager;

		/// <summary>
		/// The style for the <see cref="XamPager"/> control.
		/// </summary>
		public static Style XamPager
		{
			get
			{
				if (g_XamPager == null)
					g_XamPager = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.XamPager)) as Style;

				return g_XamPager;
			}
		}

		#endregion //XamPager


		// AS 6/10/08 NA 2008 Vol 1
		#region ToolWindow

		private static Style g_ToolWindow;

		/// <summary>
		/// The style for the <see cref="ToolWindow"/> control.
		/// </summary>
		public static Style ToolWindow
		{
			get
			{
				if (g_ToolWindow == null)
					g_ToolWindow = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.ToolWindow)) as Style;

				return g_ToolWindow;
			}
		}

		#endregion //ToolWindow

		// AS 7/25/07 XamTabControl
		#region XamTabControl

		private static Style g_XamTabControl;

		/// <summary>
		/// The style for the <see cref="XamTabControl"/> control.
		/// </summary>
		public static Style XamTabControl
		{
			get
			{
				if (g_XamTabControl == null)
					g_XamTabControl = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.XamTabControl)) as Style;

				return g_XamTabControl;
			}
		}

		#endregion //XamTabControl

        // GH 10/8/2008 TabItemEx
        #region TabItemEx

        private static Style g_TabItemEx;

        /// <summary>
        /// The style for the <see cref="TabItemEx"/> control.
        /// </summary>
        public static Style TabItemEx
        {
            get
            {
                if (g_TabItemEx == null)
                    g_TabItemEx = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.TabItemEx)) as Style;

                return g_TabItemEx;
            }
        }

        #endregion //TabItemEx
		
		// JJD 8/23/07 XamScreenTip
		#region XamScreenTip

		private static Style g_XamScreenTip;

		/// <summary>
		/// The style for the <see cref="XamScreenTip"/> control.
		/// </summary>
		public static Style XamScreenTip
		{
			get
			{
				if (g_XamScreenTip == null)
					g_XamScreenTip = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Controls.XamScreenTip)) as Style;

				return g_XamScreenTip;
			}
		}

		#endregion //XamScreenTip

		
		// JJD 1/19/10 TileDragIndicator
		#region TileDragIndicator

		private static Style g_TileDragIndicator;

		/// <summary>
		/// The style for the <see cref="TileDragIndicator"/> control.
		/// </summary>
		public static Style TileDragIndicator
		{
			get
			{
				if (g_TileDragIndicator == null)
					g_TileDragIndicator = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Tiles.TileDragIndicator)) as Style;

				return g_TileDragIndicator;
			}
		}

		#endregion //TileDragIndicator

		#endregion //Static Properties
	}

	#endregion //PrimitivesResourceSet<T> base class	
    
	#region PrimitivesGeneric

	/// <summary>
	/// Class used to supply style resources for the Generic look for primitive elements that are shared
	/// </summary>
	public class PrimitivesGeneric : PrimitivesResourceSet<PrimitivesGeneric.Locator>
	{

		#region Instance static property

		private static PrimitivesGeneric g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesGeneric Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesGeneric();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameGeneric; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesGeneric_Express.xaml;ResourceSets\Primitives\PrimitivesGeneric.xaml;generic_shared.xaml"; } }



		}
	}

	#endregion //PrimitivesGeneric	
    
	#region PrimitivesOnyx

	/// <summary>
	/// Class used to supply style resources for the Onyx look for primitive elements that are shared
	/// </summary>
	public class PrimitivesOnyx : PrimitivesResourceSet<PrimitivesOnyx.Locator>
	{

		#region Instance static property

		private static PrimitivesOnyx g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesOnyx Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesOnyx();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOnyx; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesOnyx.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesOnyx_Express.xaml;ResourceSets\Primitives\PrimitivesOnyx.xaml"; } }

		}
	}

	#endregion //PrimitivesOnyx	
    
	#region PrimitivesAero

	/// <summary>
	/// Class used to supply style resources for the Aero look for primitive elements that are shared
	/// </summary>
	public class PrimitivesAero : PrimitivesResourceSet<PrimitivesAero.Locator>
	{

		#region Instance static property

		private static PrimitivesAero g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesAero Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesAero();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameAero; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesAero.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesAero_Express.xaml;ResourceSets\Primitives\PrimitivesAero.xaml"; } }

		}
	}

	#endregion //PrimitivesAero	
    
	#region PrimitivesRoyale

	/// <summary>
	/// Class used to supply style resources for the Royale look for primitive elements that are shared
	/// </summary>
	public class PrimitivesRoyale : PrimitivesResourceSet<PrimitivesRoyale.Locator>
	{

		#region Instance static property

		private static PrimitivesRoyale g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesRoyale Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesRoyale();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameRoyale; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesRoyale.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesRoyale_Express.xaml;ResourceSets\Primitives\PrimitivesRoyale.xaml"; } }

		}
	}

	#endregion //PrimitivesRoyale		

	#region PrimitivesRoyaleStrong

	/// <summary>
	/// Class used to supply style resources for the RoyaleStrong look for primitive elements that are shared
	/// </summary>
	public class PrimitivesRoyaleStrong : PrimitivesResourceSet<PrimitivesRoyaleStrong.Locator>
	{

		#region Instance static property

		private static PrimitivesRoyaleStrong g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesRoyaleStrong Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesRoyaleStrong();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameRoyaleStrong; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesRoyaleStrong.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesRoyaleStrong_Express.xaml;ResourceSets\Primitives\PrimitivesRoyaleStrong.xaml"; } }

		}
	}

	#endregion //PrimitivesRoyaleStrong		

	#region PrimitivesLunaNormal

	/// <summary>
	/// Class used to supply style resources for the LunaNormal look for primitive elements that are shared
	/// </summary>
	public class PrimitivesLunaNormal : PrimitivesResourceSet<PrimitivesLunaNormal.Locator>
	{

		#region Instance static property

		private static PrimitivesLunaNormal g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesLunaNormal Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesLunaNormal();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameLunaNormal; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesLunaNormal.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesLunaNormal_Express.xaml;ResourceSets\Primitives\PrimitivesLunaNormal.xaml"; } }

		}
	}

	#endregion //PrimitivesLunaNormal		

	#region PrimitivesLunaOlive

	/// <summary>
	/// Class used to supply style resources for the LunaOlive look for primitive elements that are shared
	/// </summary>
	public class PrimitivesLunaOlive : PrimitivesResourceSet<PrimitivesLunaOlive.Locator>
	{

		#region Instance static property

		private static PrimitivesLunaOlive g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesLunaOlive Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesLunaOlive();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameLunaOlive; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesLunaOlive.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesLunaOlive_Express.xaml;ResourceSets\Primitives\PrimitivesLunaOlive.xaml"; } }

		}
	}

	#endregion //PrimitivesLunaOlive		

	#region PrimitivesLunaSilver

	/// <summary>
	/// Class used to supply style resources for the LunaSilver look for primitive elements that are shared
	/// </summary>
	public class PrimitivesLunaSilver : PrimitivesResourceSet<PrimitivesLunaSilver.Locator>
	{

		#region Instance static property

		private static PrimitivesLunaSilver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesLunaSilver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesLunaSilver();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameLunaSilver; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesLunaSilver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesLunaSilver_Express.xaml;ResourceSets\Primitives\PrimitivesLunaSilver.xaml"; } }

		}
	}

	#endregion //PrimitivesLunaSilver		

	// JJD 2/16/12 - Added Metro theme
	#region PrimitivesMetro

	/// <summary>
	/// Class used to supply style resources for the Metro look for primitive elements that are shared
	/// </summary>
	public class PrimitivesMetro : PrimitivesResourceSet<PrimitivesMetro.Locator>
	{

		#region Instance static property

		private static PrimitivesMetro g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesMetro Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesMetro();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameMetro; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesMetro.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesMetro_Express.xaml;ResourceSets\Primitives\PrimitivesMetro.xaml"; } }

		}
	}

	#endregion //PrimitivesMetro	
	
	#region PrimitivesOffice2k7Black

	/// <summary>
	/// Class used to supply style resources for the Lite look for primitive elements that are shared
	/// </summary>
	public class PrimitivesOffice2k7Black : PrimitivesResourceSet<PrimitivesOffice2k7Black.Locator>
	{

		#region Instance static property

		private static PrimitivesOffice2k7Black g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesOffice2k7Black Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesOffice2k7Black();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Black; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesOffice2k7Black.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesOffice2k7Black_Express.xaml;ResourceSets\Primitives\PrimitivesOffice2k7Black.xaml"; } }

		}
	}

	#endregion //PrimitivesOffice2k7Black	

	#region PrimitivesOffice2k7Blue

	/// <summary>
	/// Class used to supply style resources for the Lite look for primitive elements that are shared
	/// </summary>
	public class PrimitivesOffice2k7Blue : PrimitivesResourceSet<PrimitivesOffice2k7Blue.Locator>
	{

		#region Instance static property

		private static PrimitivesOffice2k7Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesOffice2k7Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesOffice2k7Blue();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Blue; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesOffice2k7Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesOffice2k7Blue_Express.xaml;ResourceSets\Primitives\PrimitivesOffice2k7Blue.xaml"; } }

		}
	}

	#endregion //PrimitivesOffice2k7Blue	

	#region PrimitivesOffice2k7Silver

	/// <summary>
	/// Class used to supply style resources for the Lite look for primitive elements that are shared
	/// </summary>
	public class PrimitivesOffice2k7Silver : PrimitivesResourceSet<PrimitivesOffice2k7Silver.Locator>
	{

		#region Instance static property

		private static PrimitivesOffice2k7Silver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesOffice2k7Silver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesOffice2k7Silver();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Silver; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesOffice2k7Silver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesOffice2k7Silver_Express.xaml;ResourceSets\Primitives\PrimitivesOffice2k7Silver.xaml"; } }

		}
	}

	#endregion //PrimitivesOffice2k7Silver	

	// JJD 8/30/10 - Added Office 2010 Blue theme
	#region PrimitivesOffice2010Blue

	/// <summary>
	/// Class used to supply style resources for the Lite look for primitive elements that are shared
	/// </summary>
	public class PrimitivesOffice2010Blue : PrimitivesResourceSet<PrimitivesOffice2010Blue.Locator>
	{

		#region Instance static property

		private static PrimitivesOffice2010Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesOffice2010Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesOffice2010Blue();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameOffice2010Blue; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesOffice2010Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesOffice2010Blue_Express.xaml;ResourceSets\Primitives\PrimitivesOffice2010Blue.xaml"; } }

		}
	}

	#endregion //PrimitivesOffice2010Blue	

	#region PrimitivesWashBaseDark

	/// <summary>
	/// Class used to supply style resources for the Office2k7Silver look for the primitive elements that are shared
	/// </summary>
	public class PrimitivesWashBaseDark : PrimitivesResourceSet<PrimitivesWashBaseDark.Locator>
	{

		#region Instance static property

		private static PrimitivesWashBaseDark g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesWashBaseDark Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesWashBaseDark();

				return g_Instance;
			}
		}

		#endregion Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameWashBaseDark; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesWashBaseDark.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesWashBaseDark.xaml"; } }
		}
	}

	#endregion PrimitivesWashBaseDark

	#region PrimitivesWashBaseLight

	/// <summary>
	/// Class used to supply style resources for the Office2k7Silver look for the primitive elements that are shared.
	/// </summary>
	public class PrimitivesWashBaseLight : PrimitivesResourceSet<PrimitivesWashBaseLight.Locator>
	{

		#region Instance static property

		private static PrimitivesWashBaseLight g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesWashBaseLight Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesWashBaseLight();

				return g_Instance;
			}
		}

		#endregion Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameWashBaseLight; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesWashBaseLight.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesWashBaseLight.xaml"; } }
		}
	}

	#endregion DockManagerWashBaseLight

	// JJD 10/29/10 - Added IGTheme
	#region PrimitivesIGTheme

	/// <summary>
	/// Class used to supply style resources for the IGTheme look for the primitive elements that are shared
	/// </summary>
	public class PrimitivesIGTheme : PrimitivesResourceSet<PrimitivesIGTheme.Locator>
	{

		#region Instance static property

		private static PrimitivesIGTheme g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static PrimitivesIGTheme Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new PrimitivesIGTheme();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameIGTheme; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return PrimitivesIGTheme.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Primitives\PrimitivesIGTheme_Express.xaml;ResourceSets\Primitives\PrimitivesIGTheme.xaml"; } }

		}
	}

	#endregion //PrimitivesIGTheme		
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