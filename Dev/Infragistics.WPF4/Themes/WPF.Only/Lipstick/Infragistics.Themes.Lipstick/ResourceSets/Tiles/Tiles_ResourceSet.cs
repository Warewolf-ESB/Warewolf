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

namespace Infragistics.Windows.Themes.Lipstick
{
    /// <summary>
    /// Class used to supply Tiles style resources for the Lipstick theme
    /// </summary>	
    public class Tiles : TilesResourceSet<Tiles.Locator>
    {
        #region Instance static property

        private static Tiles g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Tiles Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Tiles();

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
            public override string Theme { get { return "Lipstick"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return TilesGeneric.Location.Grouping; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Tiles\Tiles.xaml"; } }
        }
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