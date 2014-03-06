using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Infragistics.Windows.Themes
{
	/// <summary>
	/// TypeConverter used by string properties that identify themes. It provides a list of standard values based on all registered themes.
	/// </summary>
	public class ThemeTypeConverter : StringConverter
	{
		#region Member Variables

		private string _grouping;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ThemeTypeConverter"/> that will include all registered themes from its <see cref="TypeConverter.GetStandardValues()"/> method.
		/// </summary>
		public ThemeTypeConverter()
			: this(ThemeManager.AllGroupingsLiteral)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="ThemeTypeConverter"/> that will include all registered themes for a given grouping from its <see cref="TypeConverter.GetStandardValues()"/> method.
		/// </summary>
		public ThemeTypeConverter(string grouping)
		{
			this._grouping = grouping;
		} 
		#endregion //Constructor

		#region Base class overrides

			#region GetStandardValues

		/// <summary>
		/// Gets the list of registered themes.
		/// </summary>
		/// <returns>The list of registered themes.</returns>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			// AS 5/12/08
			//return new StandardValuesCollection(ThemeManager.GetThemes());
			return new StandardValuesCollection(ThemeManager.GetThemes(false, this._grouping));
		}

			#endregion //GetStandardValues	
    
			#region GetStandardValuesExclusive

		/// <summary>
		/// Indicates if a valkue can be supplied that is not in the standard values list
		/// </summary>
		/// <returns>False</returns>
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

			#endregion //GetStandardValuesExclusive	

			#region GetStandardValuesSupported

		/// <summary>
		/// Indicates that there is a list of standard values
		/// </summary>
		/// <returns>True</returns>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

			#endregion //GetStandardValuesSupported
    
		#endregion //Base class overrides
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