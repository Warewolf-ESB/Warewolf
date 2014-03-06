//  NOTE:
//  These things have to be in the project root, or at least
//  in the same location as SR.cs.
//


//
using System;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.ComponentModel;
using System.Collections;

namespace Infragistics.Documents
{
	/// <summary>
	/// Class used to provide the ability to customize resource strings.
	/// </summary>
	/// <remarks>
	/// </remarks>
	// JJD 12/05/02 - FxCop
	// Added ComVisible attribute to avoid fxcop violation
	[System.Runtime.InteropServices.ComVisible(false)]
	sealed public class DocumentsResourceCustomizer
	{
		#region Private Members

        private Hashtable customizedStrings;
 
		#endregion Private Members

        #region Constructor

        /// <summary>
        /// Creates a new instance of the DocumentsResourceCustomizer class.
        /// </summary>
        public DocumentsResourceCustomizer()
        {
        }
        #endregion //Constructor

        #region Properties

        #region CustomizedStrings

        private Hashtable CustomizedStrings
		{
			get 
			{
				if ( this.customizedStrings == null )
					this.customizedStrings = new Hashtable();

				return this.customizedStrings;
			}
		}

			#endregion CustomizedStrings
 
		#endregion Properties

		#region Methods

			#region GetCustomizedString

		/// <summary>
		/// Gets the customized string identified by the specified string resource name.
		/// </summary>
		/// <param name="name">Name of the string resource that was customized.</param>
		/// <returns>The customized string or null if the resource wasn't customized.</returns>
		/// <seealso cref="ResetAllCustomizedStrings"/>
		/// <seealso cref="ResetCustomizedString"/>
		/// <seealso cref="SetCustomizedString"/>
		public string GetCustomizedString( string name )
		{
			if ( this.CustomizedStrings.Contains(name) )
				return this.CustomizedStrings[name] as string;

			return null;
		}

			#endregion GetCustomizedString

			#region ResetAllCustomizedStrings

		/// <summary>
		/// Clears all strings customized by calls to <see cref="SetCustomizedString"/> method.
		/// </summary>
		/// <seealso cref="SetCustomizedString"/>
		/// <seealso cref="ResetCustomizedString"/>
		public void ResetAllCustomizedStrings( )
		{
			if ( this.customizedStrings != null )
				this.customizedStrings.Clear();

		}

			#endregion ResetAllCustomizedStrings

			#region ResetCustomizedString

		/// <summary>
		/// Resets a customized string identified by the specified string resource name so that it will load from the resource file.
		/// </summary>
		/// <param name="name">Name of the string resource to customize.</param>
		/// <seealso cref="SetCustomizedString"/>
		public void ResetCustomizedString( string name )
		{
			this.SetCustomizedString( name, null );
		}

			#endregion ResetCustomizedString

			#region SetCustomizedString

		/// <summary>
		/// Sets a customized string identified by the specified string resource name.
		/// </summary>
		/// <param name="name">Name of the string resource to customize.</param>
		/// <param name="customizedText">The customized string. If null this has the same effect as calling <see cref="ResetCustomizedString"/></param>
		/// <seealso cref="ResetAllCustomizedStrings"/>
		/// <seealso cref="ResetCustomizedString"/>
		public void SetCustomizedString( string name, string customizedText )
		{
			if ( this.CustomizedStrings.Contains(name) )
				this.CustomizedStrings.Remove(name);

			if ( customizedText != null )
				this.CustomizedStrings.Add( name, customizedText );

		}

			#endregion SetCustomizedString

		#endregion Methods

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