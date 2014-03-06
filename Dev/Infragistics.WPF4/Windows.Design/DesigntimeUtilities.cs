using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Shared;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Windows.Design.Model;

namespace Infragistics.Windows.Design
{
#if VS12
	internal
#else
	/// <summary>
	/// Designtime utility functions.
	/// </summary>
	public
#endif
		static class DesigntimeUtilities
	{
		#region GetNameSafe

		/// <summary>
		/// Returns a name that is unused in the namescope containing the specified element, and with the specified parts.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="resourceName"></param>
		/// <param name="desiredSuffix"></param>
		/// <param name="namescopeRootElement"></param>
		/// <returns></returns>
		public static string GetNameSafe(ModelItem item, string resourceName, int desiredSuffix, DependencyObject namescopeRootElement)
		{
			string nameCandidate = SR.GetString(resourceName, desiredSuffix.ToString());

			if (namescopeRootElement != null)
			{
				INameScope nameScope = NameScope.GetNameScope(namescopeRootElement);
				if (nameScope != null)
				{
					object o = nameScope.FindName(nameCandidate);
					while (o != null)
					{
						desiredSuffix++;
						nameCandidate	= SR.GetString(resourceName, desiredSuffix.ToString());
						o				= nameScope.FindName(nameCandidate);
					}

					return nameCandidate;
				}
			}

			return string.Empty;
		}

		#endregion //GetNameSafe

		#region SetPropValueHelper 

		/// <summary>
		/// Sets the value of the specified ModelProperty to the specified Value, clearing the current value if the specified Value equals the default value.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		public static void SetPropValueHelper(ModelProperty property, object value)
		{
			if (object.Equals(property.DefaultValue, value))
				property.ClearValue();
			else
				property.SetValue(value);
		}

		#endregion //SetPropValueHelper
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