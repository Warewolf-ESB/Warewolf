using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design.Model;
using System.Windows;
using Microsoft.Windows.Design;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.Design
{
	#region DefaultInitializerXamTabControl

	/// <summary>
	/// Provides default values for various XamTabControl properties.
	/// </summary>
	public class DefaultInitializerXamTabControl : DefaultInitializer
	{
		/// <summary>
		/// Initializes various defaults for the element wrapped by the specified Modelitem.
		/// </summary>
		/// <param name="item">The ModeItem that wraps the element whose defaults are to be set.</param>
		public override void InitializeDefaults(ModelItem item)
		{
			if (item.Context != null)
				this.InitializeDefaults(item, item.Context);
		}

		/// <summary>
		/// Initializes various defaults for the element wrapped by the specified Modelitem.
		/// </summary>
		/// <param name="item">The ModeItem that wraps the element whose defaults are to be set.</param>
		/// <param name="context">The EditingContext associated with the ModeItem whose defaults are to be set.</param>
		public override void InitializeDefaults(ModelItem item, EditingContext context)
		{
			// Add default tabs by calling a routine that is also called by the pre-VS2010 design assembly
			// NOTE: wrapping this in a Try/Catch for a NullReferenceException since Blend3 throws an uncaught
			// NullreferenceException when the item.Root property is accessed.  This has been acknolwedged as 
			// a bug by MS.
			DependencyObject namescopeRootElement = null;
			try
			{
				namescopeRootElement = item.Root != null && item.Root.View != null ? item.Root.View.PlatformObject as DependencyObject :
																								  null;
			}
			catch (NullReferenceException)
			{
			}

			DefaultInitializersHelpers.InitializeDefaultsXamTabControl(item, 
																	   context, 
																	   namescopeRootElement);
		}
	}

	#endregion //DefaultInitializerXamTabControl

	// NOTE: There is another default initializer for TabItemEx defined in the pre-VS2010 Visual
	// Studio specific designer (Infragistics.Windows.VisualStudio.Design.dll).  If you are making 
	// changes to the defaults specified here you should consider whether the same changes should 
	// be made to the pre-VS2010 Visual Studio specific designer.
	#region DefaultInitializerTabItemEx Class

	/// <summary>
	/// Provides default values for various TabItemEx properties.
	/// </summary>
	public class DefaultInitializerTabItemEx : DefaultInitializer
	{
		/// <summary>
		/// Initializes various defaults for the element wrapped by the specified Modelitem.
		/// </summary>
		/// <param name="item">The ModeItem that wraps the element whose defaults are to be set.</param>
		public override void InitializeDefaults(ModelItem item)
		{
			// NOTE: There is another default initializer for TabItemEx defined in the pre-VS2010 Visual
			// Studio specific designer.  If you are making changes to the defaults specified here you
			// should consider whether the same changes should be made to the pre-VS2010 Visual Studio specific
			// designer.
			item.Properties["Height"].ClearValue();
			item.Properties["Width"].ClearValue();
		}

	}

	#endregion //DefaultInitializerTabItemEx Class
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