using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Ribbon
{
    /// <summary>
    /// A converter that converts from a <see cref="ToolLocation"/> enumeration to a boolean.
    /// </summary>
    /// <remarks>
    /// <para class="body">Will convert to True if the <see cref="ToolLocation"/> is Menu, ApplicationMenu, ApplicationMenuRecentItems or ApplicationMenuSubMenu. Otherwise it converts to False.
    /// </para>
    /// </remarks>
    public class ToolLocationToIsInMenuConverter : IValueConverter
    {
        #region Members

        /// <summary>
        /// A static instance of the class (read-only)
        /// </summary>
        public static readonly ToolLocationToIsInMenuConverter Instance;

        #endregion //Members

        #region Constructor
        
        /// <summary>
        /// Creates a new instnace of <see cref="ToolLocationToIsInMenuConverter"/>
        /// </summary>
        public ToolLocationToIsInMenuConverter()
        {
        }

        static ToolLocationToIsInMenuConverter()
        {
            Instance = new ToolLocationToIsInMenuConverter();
        }
        #endregion //Constructor

        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((targetType == typeof(bool) ||
                 targetType == typeof(object)) && value is ToolLocation)
            {
                switch ((ToolLocation)value)
                {
                    case ToolLocation.ApplicationMenu:
                    case ToolLocation.ApplicationMenuSubMenu:
                    case ToolLocation.ApplicationMenuRecentItems:
                    case ToolLocation.Menu:
                        return KnownBoxes.TrueBox;
                }

                return KnownBoxes.FalseBox;
            }

             return Binding.DoNothing;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
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