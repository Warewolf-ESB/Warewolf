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
	#region DataPresenterBrushes<T> base class

	/// <summary>
	/// Abstract base class used to supply brush resources for a specific Office DataPresenter look.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class DataPresenterBrushes<T> : ResourceSet where T : ResourceSetLocator, new()
	{
		#region Constants

		// JJD 12/20/07
		// Since these resourceSets never get registered then return an empty string
		//static internal readonly string GroupingName = "DataPresenterBrushes";
		static internal readonly string GroupingName = "";

		#endregion //Constants

		#region Base class overrides

		#region Grouping

		/// <summary>
		/// Returns the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: DataPresenterBase, Editors, DataPresenterBase, WPF etc.
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

		#region Private Propeties

		#region Location

		private static ResourceSetLocator g_Location;

		private static ResourceSetLocator Location
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

		#region Public Properties

		#endregion //Public Properties

		#endregion //Static Properties
	}

	#endregion //DataPresenterBrushes<T> base class

	#region DataPresenterAeroBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using the Aero theme
	/// </summary>
	public class DataPresenterAeroBrushes : DataPresenterBrushes<DataPresenterAeroBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterAeroBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterAeroBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterAeroBrushes();

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
			public override string Theme { get { return "AeroBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterAero_Brushes.xaml"; } }
		}
	}

	#endregion DataPresenterAeroBrushes

	#region DataPresenterLunaNormalBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using the LunaNormal theme
	/// </summary>
	public class DataPresenterLunaNormalBrushes : DataPresenterBrushes<DataPresenterLunaNormalBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterLunaNormalBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterLunaNormalBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterLunaNormalBrushes();

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
			public override string Theme { get { return "LunaNormalBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterLunaNormal_Brushes.xaml"; } }
		}
	}

	#endregion DataPresenterLunaNormalBrushes

	#region DataPresenterLunaOliveBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using the LunaOlive theme
	/// </summary>
	public class DataPresenterLunaOliveBrushes : DataPresenterBrushes<DataPresenterLunaOliveBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterLunaOliveBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterLunaOliveBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterLunaOliveBrushes();

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
			public override string Theme { get { return "LunaOliveBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterLunaOlive_Brushes.xaml"; } }
		}
	}

	#endregion DataPresenterLunaOliveBrushes

	#region DataPresenterLunaSilverBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using the LunaSilver theme
	/// </summary>
	public class DataPresenterLunaSilverBrushes : DataPresenterBrushes<DataPresenterLunaSilverBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterLunaSilverBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterLunaSilverBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterLunaSilverBrushes();

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
			public override string Theme { get { return "LunaSilverBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterLunaSilver_Brushes.xaml"; } }
		}
	}

	#endregion DataPresenterLunaSilverBrushes

	#region DataPresenterOffice2k7BlackBrushes

	/// <summary>
	/// Class used to supply brush resources for the Black Office 2007 look for a DataPresenter
	/// </summary>
	public class DataPresenterOffice2k7BlackBrushes : DataPresenterBrushes<DataPresenterOffice2k7BlackBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterOffice2k7BlackBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOffice2k7BlackBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOffice2k7BlackBrushes();

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
			public override string Theme { get { return "Office2k7BlackBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterOffice2k7Black_Brushes.xaml"; } }
		}
	}

	#endregion //DataPresenterOffice2k7BlackBrushes

	#region DataPresenterOffice2k7BlueBrushes

	/// <summary>
	/// Class used to supply brush resources for the Blue Office 2007 look for a DataPresenter
	/// </summary>
	public class DataPresenterOffice2k7BlueBrushes : DataPresenterBrushes<DataPresenterOffice2k7BlueBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterOffice2k7BlueBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOffice2k7BlueBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOffice2k7BlueBrushes();

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
			public override string Theme { get { return "Office2k7BlueBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterOffice2k7Blue_Brushes.xaml"; } }
		}
	}

	#endregion //DataPresenterOffice2k7BlueBrushes

	#region DataPresenterOffice2k7SilverBrushes

	/// <summary>
	/// Class used to supply brush resources for the Silver Office 2007 look for a DataPresenter
	/// </summary>
	public class DataPresenterOffice2k7SilverBrushes : DataPresenterBrushes<DataPresenterOffice2k7SilverBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterOffice2k7SilverBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOffice2k7SilverBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOffice2k7SilverBrushes();

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
			public override string Theme { get { return "Office2k7SilverBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterOffice2k7Silver_Brushes.xaml"; } }
		}
	}

	#endregion //DataPresenterOffice2k7SilverBrushes

	// JJD 4/28/11 - Added 
	#region DataPresenterOffice2010BlueBrushes

	/// <summary>
	/// Class used to supply brush resources for the Blue Office 2010 look for a DataPresenter
	/// </summary>
	public class DataPresenterOffice2010BlueBrushes : DataPresenterBrushes<DataPresenterOffice2010BlueBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterOffice2010BlueBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOffice2010BlueBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOffice2010BlueBrushes();

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
			public override string Theme { get { return "Office2010BlueBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterOffice2010Blue_Brushes.xaml"; } }
		}
	}

	#endregion //DataPresenterOffice2010BlueBrushes

	#region DataPresenterOnyxBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using the Onyx theme
	/// </summary>
	public class DataPresenterOnyxBrushes : DataPresenterBrushes<DataPresenterOnyxBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterOnyxBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterOnyxBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterOnyxBrushes();

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
			public override string Theme { get { return "OnyxBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterOnyx_Brushes.xaml"; } }
		}
	}

	#endregion DataPresenterOnyxBrushes

	#region DataPresenterRoyaleBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using the Royale theme
	/// </summary>
	public class DataPresenterRoyaleBrushes : DataPresenterBrushes<DataPresenterRoyaleBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterRoyaleBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterRoyaleBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterRoyaleBrushes();

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
			public override string Theme { get { return "RoyaleBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterRoyale_Brushes.xaml"; } }
		}
	}

	#endregion DataPresenterRoyaleBrushes

	#region DataPresenterRoyaleStrongBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using the RoyaleStrong theme
	/// </summary>
	public class DataPresenterRoyaleStrongBrushes : DataPresenterBrushes<DataPresenterRoyaleStrongBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterRoyaleStrongBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterRoyaleStrongBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterRoyaleStrongBrushes();

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
			public override string Theme { get { return "RoyaleStrongBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterRoyaleStrong_Brushes.xaml"; } }
		}
	}

	#endregion DataPresenterRoyaleStrongBrushes

	#region DataPresenterWashBaseDarkBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using a dark high constrast theme
	/// </summary>
	public class DataPresenterWashBaseDarkBrushes : DataPresenterBrushes<DataPresenterWashBaseDarkBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterWashBaseDarkBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterWashBaseDarkBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterWashBaseDarkBrushes();

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
			public override string Theme { get { return "HighContrastBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterWashBaseDark_Brushes.xaml"; } }
		}
	}

	#endregion //DataPresenterWashBaseDarkBrushes

	#region DataPresenterWashBaseLightBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using a dark high constrast theme
	/// </summary>
	public class DataPresenterWashBaseLightBrushes : DataPresenterBrushes<DataPresenterWashBaseLightBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterWashBaseLightBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterWashBaseLightBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterWashBaseLightBrushes();

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
			public override string Theme { get { return "HighContrastBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterWashBaseLight_Brushes.xaml"; } }
		}
	}

	#endregion //DataPresenterWashBaseLightBrushes


    #region DataPresenterPrintBasicBrushes

    /// <summary>
    /// Class used to supply brush resources for a system using the Royale theme
    /// </summary>
    public class DataPresenterPrintBasicBrushes : DataPresenterBrushes<DataPresenterPrintBasicBrushes.Locator>
    {

        #region Instance static property

        private static DataPresenterPrintBasicBrushes g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static DataPresenterPrintBasicBrushes Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new DataPresenterPrintBasicBrushes();

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
            public override string Theme { get { return "PrintBrushes"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return DataPresenterGeneric.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterPrintBasic_Brushes.xaml"; } }
        }
    }

    #endregion //DataPresenterPrintBasicBrushes


	// JM 04-25-11 TFS73393
	#region DataPresenterIGThemeBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using IGTheme
	/// </summary>
	public class DataPresenterIGThemeBrushes : DataPresenterBrushes<DataPresenterIGThemeBrushes.Locator>
	{

		#region Instance static property

		private static DataPresenterIGThemeBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DataPresenterIGThemeBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new DataPresenterIGThemeBrushes();

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
			public override string Theme { get { return "IGThemeBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DataPresenterIGTheme.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DataPresenterBrushes\DataPresenterIGTheme_Brushes.xaml"; } }
		}
	}

	#endregion //DataPresenterIGThemeBrushes
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