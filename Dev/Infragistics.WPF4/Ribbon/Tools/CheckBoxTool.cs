using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Ribbon.Events;
using Infragistics.Windows.Ribbon.Internal;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A tool that represents 3 user selectable states: Checked, Unchecked and Indeterminate.  Since the tool is ultimately derived from the WPF ToggleButton element, the usage
	/// semantics are the same as a WPF ToggleButton.
	/// </summary>
	/// <p class="note"><b>Note: </b>The CheckBoxTool dispays a standard CheckBox graphic along with its caption and Image (if any).  Unlike the <see cref="ToggleButtonTool"/> though,
	/// the CheckBoxTool's caption text is not hidden by default when the tool appears on the <see cref="QuickAccessToolbar"/> or when it is resized to its smallest size 
	/// on a <see cref="RibbonGroup"/>.  </p>
	/// <p class="note"><b>Note: </b>The <see cref="RadioButtonTool"/> supports the same 3 user selectable states but is designed to coordinate its state with other
	/// <see cref="RadioButtonTool"/>s via its GroupName property.  When multiple <see cref="RadioButtonTool"/>s have the same GroupName
	/// only 1 tool in the group can be checked at any given time.</p>
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class CheckBoxTool : ToggleButtonTool
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="CheckBoxTool"/> class.
		/// </summary>
		public CheckBoxTool()
		{
		}

		static CheckBoxTool()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBoxTool), new FrameworkPropertyMetadata(typeof(CheckBoxTool)));
			RibbonGroup.MinimumSizeProperty.OverrideMetadata(typeof(CheckBoxTool), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextNormalBox));
			RibbonGroup.MaximumSizeProperty.OverrideMetadata(typeof(CheckBoxTool), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextNormalBox));
			FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(CheckBoxTool), new FrameworkPropertyMetadata(new Style()));
			ToolTipService.ShowOnDisabledProperty.OverrideMetadata(typeof(CheckBoxTool), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));
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