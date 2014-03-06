using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Themes;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using Infragistics.Windows.Tiles;

namespace Infragistics.Windows.Themes
{
	#region TilesBrushes<T>
	/// <summary>
	/// Abstract base class used to supply brush resources for the <see cref="XamTilesControl"/>.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class TilesBrushes<T> : ResourceSet
		where T : ResourceSetLocator, new()
	{
		#region Member Variables

		private static readonly ResourceSetLocator g_Location;
		private static readonly ResourceDictionary g_ResourcesInternal;

		#endregion //Member Variables

		#region Constructor
		static TilesBrushes()
		{
			g_Location = new T();
			g_ResourcesInternal = Utilities.CreateResourceSetDictionary(g_Location.Assembly, g_Location.ResourcePath);
		}

		/// <summary>
		/// Initializes a new <see cref="TilesBrushes&lt;T&gt;"/>
		/// </summary>
		protected TilesBrushes()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region Grouping

		/// <summary>
		/// Returns the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: RibbonBase, Editors, RibbonBase, WPF etc.
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
				return ResourcesInternal;
			}
		}

		#endregion //Resources

		#endregion //Base class overrides

		#region Static Properties

		#region Private Properties

		#region Location

		private static ResourceSetLocator Location
		{
			get { return g_Location; }
		}

		#endregion //Location

		#region ResourcesInternal

		private static ResourceDictionary ResourcesInternal
		{
			get { return g_ResourcesInternal; }
		}

		#endregion //ResourcesInternal

		#endregion //Private Properties

		#region Public Properties

		#endregion //Public Properties

		#endregion //Static Properties
	} 
	#endregion //TilesBrushes<T>

	#region TilesGenericBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesGenericBrushes : TilesBrushes<TilesGenericBrushes.Locator>
	{
		#region Member Variables

		private static TilesGenericBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static TilesGenericBrushes()
		{
			g_Instance = new TilesGenericBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesGenericBrushes"/>
		/// </summary>
		public TilesGenericBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesGenericBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Generic"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesGeneric_Brushes.xaml"; } }
		}
	}

	#endregion //TilesGenericBrushes

	#region TilesLunaNormalBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesLunaNormalBrushes : TilesBrushes<TilesLunaNormalBrushes.Locator>
	{
		#region Member Variables

		private static TilesLunaNormalBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static TilesLunaNormalBrushes()
		{
			g_Instance = new TilesLunaNormalBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesLunaNormalBrushes"/>
		/// </summary>
		public TilesLunaNormalBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesLunaNormalBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "LunaNormal"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesLunaNormal_Brushes.xaml"; } }
		}
	}

	#endregion //TilesLunaNormalBrushes

	#region TilesLunaOliveBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesLunaOliveBrushes : TilesBrushes<TilesLunaOliveBrushes.Locator>
	{
		#region Member Variables

		private static TilesLunaOliveBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static TilesLunaOliveBrushes()
		{
			g_Instance = new TilesLunaOliveBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesLunaOliveBrushes"/>
		/// </summary>
		public TilesLunaOliveBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesLunaOliveBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "LunaOlive"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesLunaOlive_Brushes.xaml"; } }
		}
	}

	#endregion //TilesLunaOliveBrushes

	#region TilesLunaSilverBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesLunaSilverBrushes : TilesBrushes<TilesLunaSilverBrushes.Locator>
	{
		#region Member Variables

		private static TilesLunaSilverBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static TilesLunaSilverBrushes()
		{
			g_Instance = new TilesLunaSilverBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesLunaSilverBrushes"/>
		/// </summary>
		public TilesLunaSilverBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesLunaSilverBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "LunaSilver"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesLunaSilver_Brushes.xaml"; } }
		}
	}

	#endregion //TilesLunaSilverBrushes

	#region TilesAeroBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesAeroBrushes : TilesBrushes<TilesAeroBrushes.Locator>
	{
		#region Member Variables

		private static TilesAeroBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static TilesAeroBrushes()
		{
			g_Instance = new TilesAeroBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesAeroBrushes"/>
		/// </summary>
		public TilesAeroBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesAeroBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Aero"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesAero_Brushes.xaml"; } }
		}
	}

	#endregion //TilesAeroBrushes

	#region TilesOffice2k7BlackBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesOffice2k7BlackBrushes : TilesBrushes<TilesOffice2k7BlackBrushes.Locator>
	{
		#region Member Variables

		private static TilesOffice2k7BlackBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static TilesOffice2k7BlackBrushes()
		{
			g_Instance = new TilesOffice2k7BlackBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesOffice2k7BlackBrushes"/>
		/// </summary>
		public TilesOffice2k7BlackBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesOffice2k7BlackBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Office2k7Black"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesOffice2k7Black_Brushes.xaml"; } }
		}
	}

	#endregion //TilesOffice2k7BlackBrushes

	#region TilesOffice2k7BlueBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesOffice2k7BlueBrushes : TilesBrushes<TilesOffice2k7BlueBrushes.Locator>
	{
		#region Member Variables

		private static TilesOffice2k7BlueBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static TilesOffice2k7BlueBrushes()
		{
			g_Instance = new TilesOffice2k7BlueBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesOffice2k7BlueBrushes"/>
		/// </summary>
		public TilesOffice2k7BlueBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesOffice2k7BlueBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Office2k7Blue"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesOffice2k7Blue_Brushes.xaml"; } }
		}
	}

	#endregion //TilesOffice2k7BlueBrushes

	#region TilesOffice2k7SilverBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesOffice2k7SilverBrushes : TilesBrushes<TilesOffice2k7SilverBrushes.Locator>
	{
		#region Member Variables

		private static TilesOffice2k7SilverBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static TilesOffice2k7SilverBrushes()
		{
			g_Instance = new TilesOffice2k7SilverBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesOffice2k7SilverBrushes"/>
		/// </summary>
		public TilesOffice2k7SilverBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesOffice2k7SilverBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Office2k7Silver"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesOffice2k7Silver_Brushes.xaml"; } }
		}
	}

	#endregion //TilesOffice2k7SilverBrushes

	#region TilesRoyaleBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesRoyaleBrushes : TilesBrushes<TilesRoyaleBrushes.Locator>
	{
		#region Member Variables

		private static TilesRoyaleBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static TilesRoyaleBrushes()
		{
			g_Instance = new TilesRoyaleBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesRoyaleBrushes"/>
		/// </summary>
		public TilesRoyaleBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesRoyaleBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Royale"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesRoyale_Brushes.xaml"; } }
		}
	}

	#endregion TilesRoyaleBrushes

	#region TilesWashBaseDarkBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesWashBaseDarkBrushes : TilesBrushes<TilesWashBaseDarkBrushes.Locator>
	{
		#region Member Variables

		private static TilesWashBaseDarkBrushes g_Instance;

		#endregion Member Variables

		#region Constructor
		static TilesWashBaseDarkBrushes()
		{
			g_Instance = new TilesWashBaseDarkBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesWashBaseDarkBrushes"/>
		/// </summary>
		public TilesWashBaseDarkBrushes()
		{
		}
		#endregion Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesWashBaseDarkBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "WashBaseDark"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesWashBaseDark_Brushes.xaml"; } }
		}
	}

	#endregion TilesWashBaseDarkBrushes

	#region TilesWashBaseLightBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamTilesControl"/>
	/// </summary>
	public class TilesWashBaseLightBrushes : TilesBrushes<TilesWashBaseLightBrushes.Locator>
	{
		#region Member Variables

		private static TilesWashBaseLightBrushes g_Instance;

		#endregion Member Variables

		#region Constructor
		static TilesWashBaseLightBrushes()
		{
			g_Instance = new TilesWashBaseLightBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="TilesWashBaseLightBrushes"/>
		/// </summary>
		public TilesWashBaseLightBrushes()
		{
		}
		#endregion Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static TilesWashBaseLightBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "WashBaseDark"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return TilesGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\TilesBrushes\TilesWashBaseLight_Brushes.xaml"; } }
		}
	}

	#endregion TilesWashBaseLightBrushes

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