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
	/// <summary>
	/// Base class used to locate the resource set for a specific theme and grouping
	/// </summary>
	/// <seealso cref="PrimitivesResourceSet&lt;T&gt;"/>
	abstract public class ResourceSetLocator
	{
		/// <summary>
		/// The name of the theme
		/// </summary>
		/// <seealso cref="PrimitivesResourceSet&lt;T&gt;.Theme"/>
		abstract public string Theme { get; }

		/// <summary>
		/// Returns the name of the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: DataPresenter, Editors, Primitives, WPF etc.
		/// </remarks>
		/// <seealso cref="PrimitivesResourceSet&lt;T&gt;.Grouping"/>
		abstract public string Grouping { get; }

		/// <summary>
		/// Returns the assembly containg the resource set
		/// </summary>
		abstract public Assembly Assembly { get; }

		/// <summary>
		/// The path to the embedded resources within the assembly
		/// </summary>
		/// <remarks>
		/// <p class="body">The path can contain multiple resources within the <see cref="Assembly"/> separated by a semi-colon.</p>
		/// </remarks>
		abstract public string ResourcePath { get; }
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