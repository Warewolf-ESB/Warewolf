using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Ribbon
{
	#region RibbonToolAttribute class

	/// <summary>
	/// This attribute identifies classes as ribbon tools. It is used to restrict the visibility of some <see cref="XamRibbon"/> attached properties.
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note:</b> this attribute has been placed the <see cref="IRibbonTool"/> interface so any class that implements that interface will automatically pick up this attribute.</para>
	/// </remarks>
	/// <seealso cref="RibbonToolHelper.CaptionProperty"/>
	/// <seealso cref="RibbonToolHelper.HasCaptionProperty"/>
	/// <seealso cref="RibbonToolHelper.IdProperty"/>
	/// <seealso cref="XamRibbon.IsActiveProperty"/>
	/// <seealso cref="RibbonToolHelper.IsOnQatProperty"/>
	/// <seealso cref="RibbonToolHelper.LargeImageProperty"/>
	/// <seealso cref="XamRibbon.LocationProperty"/>
	/// <seealso cref="RibbonGroup.MaximumSizeProperty"/>
	/// <seealso cref="RibbonGroup.MinimumSizeProperty"/>
	/// <seealso cref="RibbonToolHelper.SizingModeProperty"/>
	/// <seealso cref="RibbonToolHelper.SmallImageProperty"/>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class RibbonToolAttribute : Attribute
	{
	}

	#endregion //RibbonToolAttribute class
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