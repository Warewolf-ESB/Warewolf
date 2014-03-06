using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Helpers
{
	// AS 5/15/08 BR32816
	// There's a bug/bad behavior in the MS.Internal.XamlLanguageService.XamlFileContext.AddStandardValuesFromTypeConverter
	// method with regards to nullable types. The default converter for a nullable type is NullableConverter and this class
	// does not have a parameterless ctor since it needs to be given the concrete nullable type it represents. The 
	// XamlFileContext either isn't getting the NullableConverter or can't create it because it doesn't have a parameterless
	// ctor so you end up not getting any intellisense for nullable properties. This class can be used to get around that
	// issue. You just have to add a TypeConverter attribute to the property for the respective type. You can provide either
	// the nullable type or the underlying type.
	//
	/// <summary>
	/// Custom type converter used for nullable types.
	/// </summary>
	/// <typeparam name="T">The underlying type for the nullable type represented or the nullable type itself</typeparam>
	public class NullableConverter<T> : System.ComponentModel.NullableConverter
		where T : struct
	{
		#region Member Variables

		private static readonly Type _nullableType;

		#endregion //Member Variables

		#region Constructor
		static NullableConverter()
		{
			// store T if it is a nullable type otherwise get the nullable type for T
			_nullableType = Nullable.GetUnderlyingType(typeof(T)) == null
				? typeof(Nullable<T>)
				: typeof(T);
		}

		/// <summary>
		/// Initializes a new <see cref="NullableConverter&lt;T&gt;"/>
		/// </summary>
		public NullableConverter()
			: base(_nullableType)
		{
		}
		#endregion //Constructor
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