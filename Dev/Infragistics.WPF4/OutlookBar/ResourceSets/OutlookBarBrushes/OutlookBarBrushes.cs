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
using Infragistics.Windows.OutlookBar;

namespace Infragistics.Windows.Themes
{
	#region OutlookBarBrushes<T>
	/// <summary>
	/// Abstract base class used to supply brush resources for the <see cref="XamOutlookBar"/>.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class OutlookBarBrushes<T> : ResourceSet
		where T : ResourceSetLocator, new()
	{
		#region Member Variables

		private static readonly ResourceSetLocator g_Location;
		private static readonly ResourceDictionary g_ResourcesInternal;

		#endregion //Member Variables

		#region Constructor
		static OutlookBarBrushes()
		{
			g_Location = new T();
			g_ResourcesInternal = Utilities.CreateResourceSetDictionary(g_Location.Assembly, g_Location.ResourcePath);
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarBrushes&lt;T&gt;"/>
		/// </summary>
		protected OutlookBarBrushes()
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
	#endregion //OutlookBarBrushes<T>

	#region OutlookBarLunaNormalBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarLunaNormalBrushes : OutlookBarBrushes<OutlookBarLunaNormalBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarLunaNormalBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static OutlookBarLunaNormalBrushes()
		{
			g_Instance = new OutlookBarLunaNormalBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarLunaNormalBrushes"/>
		/// </summary>
		public OutlookBarLunaNormalBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarLunaNormalBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarLunaNormal_Brushes.xaml"; } }
		}
	}

	#endregion //OutlookBarLunaNormalBrushes

	#region OutlookBarLunaOliveBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarLunaOliveBrushes : OutlookBarBrushes<OutlookBarLunaOliveBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarLunaOliveBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static OutlookBarLunaOliveBrushes()
		{
			g_Instance = new OutlookBarLunaOliveBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarLunaOliveBrushes"/>
		/// </summary>
		public OutlookBarLunaOliveBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarLunaOliveBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarLunaOlive_Brushes.xaml"; } }
		}
	}

	#endregion //OutlookBarLunaOliveBrushes

	#region OutlookBarLunaSilverBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarLunaSilverBrushes : OutlookBarBrushes<OutlookBarLunaSilverBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarLunaSilverBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static OutlookBarLunaSilverBrushes()
		{
			g_Instance = new OutlookBarLunaSilverBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarLunaSilverBrushes"/>
		/// </summary>
		public OutlookBarLunaSilverBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarLunaSilverBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarLunaSilver_Brushes.xaml"; } }
		}
	}

	#endregion //OutlookBarLunaSilverBrushes

	#region OutlookBarAeroBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarAeroBrushes : OutlookBarBrushes<OutlookBarAeroBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarAeroBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static OutlookBarAeroBrushes()
		{
			g_Instance = new OutlookBarAeroBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarAeroBrushes"/>
		/// </summary>
		public OutlookBarAeroBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarAeroBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarAero_Brushes.xaml"; } }
		}
	}

	#endregion //OutlookBarAeroBrushes

	#region OutlookBarOffice2k7BlackBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarOffice2k7BlackBrushes : OutlookBarBrushes<OutlookBarOffice2k7BlackBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarOffice2k7BlackBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static OutlookBarOffice2k7BlackBrushes()
		{
			g_Instance = new OutlookBarOffice2k7BlackBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarOffice2k7BlackBrushes"/>
		/// </summary>
		public OutlookBarOffice2k7BlackBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarOffice2k7BlackBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarOffice2k7Black_Brushes.xaml"; } }
		}
	}

	#endregion //OutlookBarOffice2k7BlackBrushes

	#region OutlookBarOffice2k7BlueBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarOffice2k7BlueBrushes : OutlookBarBrushes<OutlookBarOffice2k7BlueBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarOffice2k7BlueBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static OutlookBarOffice2k7BlueBrushes()
		{
			g_Instance = new OutlookBarOffice2k7BlueBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarOffice2k7BlueBrushes"/>
		/// </summary>
		public OutlookBarOffice2k7BlueBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarOffice2k7BlueBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarOffice2k7Blue_Brushes.xaml"; } }
		}
	}

	#endregion //OutlookBarOffice2k7BlueBrushes

	#region OutlookBarOffice2k7SilverBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarOffice2k7SilverBrushes : OutlookBarBrushes<OutlookBarOffice2k7SilverBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarOffice2k7SilverBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static OutlookBarOffice2k7SilverBrushes()
		{
			g_Instance = new OutlookBarOffice2k7SilverBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarOffice2k7SilverBrushes"/>
		/// </summary>
		public OutlookBarOffice2k7SilverBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarOffice2k7SilverBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarOffice2k7Silver_Brushes.xaml"; } }
		}
	}

	#endregion //OutlookBarOffice2k7SilverBrushes

	#region OutlookBarRoyaleBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarRoyaleBrushes : OutlookBarBrushes<OutlookBarRoyaleBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarRoyaleBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static OutlookBarRoyaleBrushes()
		{
			g_Instance = new OutlookBarRoyaleBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarRoyaleBrushes"/>
		/// </summary>
		public OutlookBarRoyaleBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarRoyaleBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarRoyale_Brushes.xaml"; } }
		}
	}

	#endregion OutlookBarRoyaleBrushes

	#region OutlookBarWashBaseDarkBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarWashBaseDarkBrushes : OutlookBarBrushes<OutlookBarWashBaseDarkBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarWashBaseDarkBrushes g_Instance;

		#endregion Member Variables

		#region Constructor
		static OutlookBarWashBaseDarkBrushes()
		{
			g_Instance = new OutlookBarWashBaseDarkBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarWashBaseDarkBrushes"/>
		/// </summary>
		public OutlookBarWashBaseDarkBrushes()
		{
		}
		#endregion Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarWashBaseDarkBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarWashBaseDark_Brushes.xaml"; } }
		}
	}

	#endregion OutlookBarWashBaseDarkBrushes

	#region OutlookBarWashBaseLightBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamOutlookBar"/>
	/// </summary>
	public class OutlookBarWashBaseLightBrushes : OutlookBarBrushes<OutlookBarWashBaseLightBrushes.Locator>
	{
		#region Member Variables

		private static OutlookBarWashBaseLightBrushes g_Instance;

		#endregion Member Variables

		#region Constructor
		static OutlookBarWashBaseLightBrushes()
		{
			g_Instance = new OutlookBarWashBaseLightBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="OutlookBarWashBaseLightBrushes"/>
		/// </summary>
		public OutlookBarWashBaseLightBrushes()
		{
		}
		#endregion Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static OutlookBarWashBaseLightBrushes Instance
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
			public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\OutlookBarBrushes\OutlookBarWashBaseLight_Brushes.xaml"; } }
		}
	}

	#endregion OutlookBarWashBaseLightBrushes
	
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