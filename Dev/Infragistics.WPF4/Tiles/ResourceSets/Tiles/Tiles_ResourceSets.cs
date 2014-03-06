using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Reflection;
using System.ComponentModel;

namespace Infragistics.Windows.Themes
{
    #region TilesResourceSet<T> base class

    /// <summary>
    /// Abstract base class used to supply style resources for a specific look for XamTiles.
    /// </summary>
    /// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
    public abstract class TilesResourceSet<T> : ResourceSet where T : ResourceSetLocator, new()
    {
        #region Constants

        internal const string GroupingName = "Tiles";

        #endregion //Constants

        #region Base class overrides

        #region Grouping

        /// <summary>
        /// Returns the grouping that the resources are applied to (read-only)
        /// </summary>
        /// <remarks>
        /// Examples: Tiles, Editors, Primitives, etc.
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

                // JJD 7/25/07 - ResourceWasher support
                // Call VerifyResources after the initial load so that we can delay the hydrating
                // of the resources by a ResourceWasher until this theme is actually used
                this.VerifyResources();

                return rd;
            }
        }

        #endregion //Resources

        #endregion //Base class overrides

        #region Static Properties

        #region Private Propeties

        #region Location

        private static ResourceSetLocator g_Location;

        /// <summary>
        /// Returns the <see cref="ResourceSetLocator"/> that describes the theme information for the resource set.
        /// </summary>
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

        #region Public Properties

        #region Tile

        private static Style g_Tile;

        /// <summary>
        /// The style for the <see cref="Tile"/> control.
        /// </summary>
        public static Style Tile
        {
            get
            {
                if (g_Tile == null)
                    g_Tile = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Tiles.Tile)) as Style;

                return g_Tile;
            }
        }

        #endregion //Tile

        #region TileAreaSplitter

        private static Style g_TileAreaSplitter;

        /// <summary>
        /// The style for the <see cref="TileAreaSplitter"/> control.
        /// </summary>
        public static Style TileAreaSplitter
        {
            get
            {
                if (g_TileAreaSplitter == null)
                    g_TileAreaSplitter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Tiles.TileAreaSplitter)) as Style;

                return g_TileAreaSplitter;
            }
        }

        #endregion //TileAreaSplitter

        #region TileHeaderPresenter

        private static Style g_TileHeaderPresenter;

        /// <summary>
        /// The style for the <see cref="TileHeaderPresenter"/> control.
        /// </summary>
        public static Style TileHeaderPresenter
        {
            get
            {
                if (g_TileHeaderPresenter == null)
                    g_TileHeaderPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Tiles.TileHeaderPresenter)) as Style;

                return g_TileHeaderPresenter;
            }
        }

        #endregion //TileHeaderPresenter

        #region ToolTip

        private static Style g_ToolTip;

        /// <summary>
        /// The style for a <see cref="ToolTip"/> used within the <see cref="XamTilesControl"/>
        /// </summary>
        /// <seealso cref="Infragistics.Windows.Tiles.XamTilesControl.ToolTipStyleKey"/>
        public static Style ToolTip
        {
            get
            {
                if (g_ToolTip == null)
                    g_ToolTip = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Tiles.XamTilesControl.ToolTipStyleKey) as Style;

                return g_ToolTip;
            }
        }

        #endregion //ToolTip

        #region XamTilesControl

        private static Style g_XamTilesControl;

        /// <summary>
        /// The style for the <see cref="XamTilesControl"/> control.
        /// </summary>
        public static Style XamTilesControl
        {
            get
            {
                if (g_XamTilesControl == null)
                    g_XamTilesControl = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Tiles.XamTilesControl)) as Style;

                return g_XamTilesControl;
            }
        }

        #endregion //XamTilesControl

        #endregion //Public Properties

        #endregion //Static Properties
    }

    #endregion //TilesResourceSet<T> base class

    #region TilesGeneric

    /// <summary>
    /// Class used to supply style resources for the Generic look for a Tiles
    /// </summary>
    public class TilesGeneric : TilesResourceSet<TilesGeneric.Locator>
    {

        #region Instance static property

        private static TilesGeneric g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesGeneric Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesGeneric();

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
            public override string Grouping { get { return TilesGeneric.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesGeneric.xaml"; } }
        }
    }

    #endregion //TilesGeneric

    #region TilesAero

    /// <summary>
    /// Class used to supply style resources for the Aero look for a Tiles
    /// </summary>
    public class TilesAero : TilesResourceSet<TilesAero.Locator>
    {

        #region Instance static property

        private static TilesAero g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesAero Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesAero();

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
            public override string Grouping { get { return TilesAero.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesAero.xaml"; } }
        }
    }

    #endregion //TilesAero

    #region TilesLunaNormal

    /// <summary>
    /// Class used to supply style resources for the LunaNormal look for a Tiles
    /// </summary>
    public class TilesLunaNormal : TilesResourceSet<TilesLunaNormal.Locator>
    {

        #region Instance static property

        private static TilesLunaNormal g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesLunaNormal Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesLunaNormal();

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
            public override string Grouping { get { return TilesLunaNormal.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesLunaNormal.xaml"; } }
        }
    }

    #endregion //TilesLunaNormal

    #region TilesLunaOlive

    /// <summary>
    /// Class used to supply style resources for the LunaOlive look for a Tiles
    /// </summary>
    public class TilesLunaOlive : TilesResourceSet<TilesLunaOlive.Locator>
    {

        #region Instance static property

        private static TilesLunaOlive g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesLunaOlive Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesLunaOlive();

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
            public override string Grouping { get { return TilesLunaOlive.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesLunaOlive.xaml"; } }
        }
    }

    #endregion //TilesLunaOlive

    #region TilesLunaSilver

    /// <summary>
    /// Class used to supply style resources for the LunaSilver look for a Tiles
    /// </summary>
    public class TilesLunaSilver : TilesResourceSet<TilesLunaSilver.Locator>
    {

        #region Instance static property

        private static TilesLunaSilver g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesLunaSilver Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesLunaSilver();

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
            public override string Grouping { get { return TilesLunaSilver.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesLunaSilver.xaml"; } }
        }
    }

    #endregion //TilesLunaSilver

    #region TilesOffice2k7Black

    /// <summary>
    /// Class used to supply style resources for the Office2k7Black look for a Tiles
    /// </summary>
    public class TilesOffice2k7Black : TilesResourceSet<TilesOffice2k7Black.Locator>
    {

        #region Instance static property

        private static TilesOffice2k7Black g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesOffice2k7Black Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesOffice2k7Black();

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
            public override string Grouping { get { return TilesOffice2k7Black.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesOffice2k7Black.xaml"; } }
        }
    }

    #endregion //TilesOffice2k7Black

    #region TilesOffice2k7Blue

    /// <summary>
    /// Class used to supply style resources for the Office2k7Blue look for a Tiles
    /// </summary>
    public class TilesOffice2k7Blue : TilesResourceSet<TilesOffice2k7Blue.Locator>
    {

        #region Instance static property

        private static TilesOffice2k7Blue g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesOffice2k7Blue Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesOffice2k7Blue();

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
            public override string Grouping { get { return TilesOffice2k7Blue.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesOffice2k7Blue.xaml"; } }
        }
    }

    #endregion //TilesOffice2k7Blue

    #region TilesOffice2k7Silver

    /// <summary>
    /// Class used to supply style resources for the Office2k7Silver look for a Tiles
    /// </summary>
    public class TilesOffice2k7Silver : TilesResourceSet<TilesOffice2k7Silver.Locator>
    {

        #region Instance static property

        private static TilesOffice2k7Silver g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesOffice2k7Silver Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesOffice2k7Silver();

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
            public override string Grouping { get { return TilesOffice2k7Silver.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesOffice2k7Silver.xaml"; } }
        }
    }

    #endregion //TilesOffice2k7Silver

	// JJD 8/30/10 - Added Office 2010 Blue theme
	#region TilesOffice2010Blue

    /// <summary>
    /// Class used to supply style resources for the Office2010Blue look for a Tiles
    /// </summary>
    public class TilesOffice2010Blue : TilesResourceSet<TilesOffice2010Blue.Locator>
    {

        #region Instance static property

        private static TilesOffice2010Blue g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesOffice2010Blue Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesOffice2010Blue();

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
            public override string Grouping { get { return TilesOffice2010Blue.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesOffice2010Blue.xaml"; } }
        }
    }

    #endregion //TilesOffice2010Blue

    #region TilesRoyale

    /// <summary>
    /// Class used to supply style resources for the Royale look for a Tiles
    /// </summary>
    public class TilesRoyale : TilesResourceSet<TilesRoyale.Locator>
    {

        #region Instance static property

        private static TilesRoyale g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesRoyale Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesRoyale();

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
            public override string Grouping { get { return TilesRoyale.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesRoyale.xaml"; } }
        }
    }

    #endregion //TilesRoyale

    #region TilesWashBaseLight

    /// <summary>
    /// Class used to supply style resources for the WashBaseLight look for a Tiles
    /// </summary>
    public class TilesWashBaseLight : TilesResourceSet<TilesWashBaseLight.Locator>
    {

        #region Instance static property

        private static TilesWashBaseLight g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesWashBaseLight Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesWashBaseLight();

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
            public override string Theme { get { return ThemeManager.ThemeNameWashBaseLight; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return TilesWashBaseLight.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesWashBaseLight.xaml"; } }
        }
    }

    #endregion //TilesWashBaseLight

    #region TilesWashBaseDark

    /// <summary>
    /// Class used to supply style resources for the WashBaseDark look for a Tiles
    /// </summary>
    public class TilesWashBaseDark : TilesResourceSet<TilesWashBaseDark.Locator>
    {

        #region Instance static property

        private static TilesWashBaseDark g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesWashBaseDark Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesWashBaseDark();

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
            public override string Theme { get { return ThemeManager.ThemeNameWashBaseDark; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return TilesWashBaseDark.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesWashBaseDark.xaml"; } }
        }
    }

    #endregion //TilesWashBaseDark

	// JJD 10/29/10 - NA 2011 Volumne 1 - IGTheme
	#region TilesIGTheme

    /// <summary>
    /// Class used to supply style resources for the IGTheme look for a Tiles
    /// </summary>
    public class TilesIGTheme : TilesResourceSet<TilesIGTheme.Locator>
    {

        #region Instance static property

        private static TilesIGTheme g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static TilesIGTheme Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new TilesIGTheme();

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
            public override string Grouping { get { return TilesIGTheme.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\TilesIGTheme.xaml"; } }
        }
    }

    #endregion //TilesIGTheme

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