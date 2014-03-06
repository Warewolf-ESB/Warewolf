using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;



using System.Windows;
using System.Windows.Media;
using SizeF = System.Windows.Size;


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)



using SR = Infragistics.Shared.SR;


namespace Infragistics.Documents.Core
{
    internal class CommonUtilities
    {
        #region StringPropertyGetHelper
        /// <summary>
        /// Returns the specified <paramref name="value"/> if it is not null,
        /// otherwise returns string.Empty.
        /// </summary>
        static public string StringPropertyGetHelper( string value )
        {
            return string.IsNullOrEmpty( value ) ? string.Empty : value;
        }
        #endregion StringPropertyGetHelper

        #region DebugFail
        
        //  Came from Infragistics.Documents.Excel.Utilities

        [Conditional("DEBUG")]
        internal static void DebugFail(string message)
        {



            Debug.Fail( message );

        }
        #endregion DebugFail

        #region VerifyCulture
        static internal void VerifyCulture( string value )
        {
            //  BF 3/29/11
            //  Null is a valid value, it means clear it out.
            if ( string.IsNullOrEmpty(value) )
                return;

            System.Globalization.CultureInfo culture = null;
            try 
			{ 



				culture = System.Globalization.CultureInfo.CreateSpecificCulture(value);

			}
            catch{}

            if ( culture == null )
            {
                string format = SR.GetString("Exception_InvalidCulture");
                string message = string.Format(format, value);
                throw new ArgumentException( message );
            }
        }
        #endregion VerifyCulture

		#region GetWeakReferenceTarget

		internal static object GetWeakReferenceTarget(WeakReference weakRef)
		{
			try
			{
				if (weakRef == null || weakRef.IsAlive == false)
					return null;

				return weakRef.Target;
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}

		#endregion GetWeakReferenceTarget
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