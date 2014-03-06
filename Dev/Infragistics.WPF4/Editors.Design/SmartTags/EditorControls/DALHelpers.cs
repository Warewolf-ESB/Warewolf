using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Design.SmartTagFramework;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Model;
using Infragistics.Windows.Editors;

namespace Infragistics.Windows.Design.Editors
{
	internal static class DALHelpers
	{
		#region AddValueEditorPropertiesGroup

		internal static DesignerActionItemGroup AddValueEditorPropertiesGroup(Type owningType, DesignerActionItemCollection items, bool isExpanded, ref int groupSequence)
		{
			DesignerActionItemGroup groupValueEditorProperties	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_General"), groupSequence++);
			groupValueEditorProperties.IsExpanded				= isExpanded;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupValueEditorProperties))
			{
				items.Add(pic.GetPropertyActionItem("AlwaysValidate"));
				items.Add(pic.GetPropertyActionItem("Format"));
				items.Add(pic.GetPropertyActionItem("InvalidValueBehavior"));
				items.Add(pic.GetPropertyActionItem("IsAlwaysInEditMode"));
				items.Add(pic.GetPropertyActionItem("IsReadOnly"));
				items.Add(pic.GetPropertyActionItem("Value"));
			}

			return groupValueEditorProperties;
		}

		#endregion //AddValueEditorPropertiesGroup

		#region AddTextEditorBasePropertiesGroup

		internal static DesignerActionItemGroup AddTextEditorBasePropertiesGroup(Type owningType, DesignerActionItemCollection items, bool isExpanded, ref int groupSequence)
		{
			DesignerActionItemGroup groupTextEditorBaseProperties	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_GeneralText"), groupSequence++);
			groupTextEditorBaseProperties.IsExpanded				= isExpanded;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupTextEditorBaseProperties))
			{
				items.Add(pic.GetPropertyActionItem("NullText"));
			}

			return groupTextEditorBaseProperties;
		}

		#endregion //AddTextEditorBasePropertiesGroup

		#region AddMaskedEditorPropertiesGroup

		internal static DesignerActionItemGroup AddMaskedEditorPropertiesGroup(Type owningType, DesignerActionItemCollection items, bool isExpanded, ref int groupSequence, string groupDescription)
		{
			DesignerActionItemGroup groupTextEditorBaseProperties = new DesignerActionItemGroup(groupDescription, groupSequence++);
			groupTextEditorBaseProperties.IsExpanded = isExpanded;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupTextEditorBaseProperties))
			{
				items.Add(pic.GetPropertyActionItem("AllowShiftingAcrossSections"));
				items.Add(pic.GetPropertyActionItem("AutoFillDate"));
				items.Add(pic.GetPropertyActionItem("ClipMode"));
				items.Add(pic.GetPropertyActionItem("DataMode"));
				items.Add(pic.GetPropertyActionItem("DisplayMode"));
				items.Add(pic.GetPropertyActionItem("InsertMode"));
				items.Add(pic.GetPropertyActionItem("Mask"));
				items.Add(pic.GetPropertyActionItem("PadChar"));
				items.Add(pic.GetPropertyActionItem("PromptChar"));
				items.Add(pic.GetPropertyActionItem("SelectAllBehavior"));
				items.Add(pic.GetPropertyActionItem("SpinButtonDisplayMode"));
				items.Add(pic.GetPropertyActionItem("SpinIncrement"));
				items.Add(pic.GetPropertyActionItem("SpinWrap"));
				items.Add(pic.GetPropertyActionItem("TabNavigation"));
			}

			return groupTextEditorBaseProperties;
		}

		#endregion //AddMaskedEditorPropertiesGroup

		#region AddAppearancePropertiesGroup

		internal static DesignerActionItemGroup AddAppearancePropertiesGroup(Type owningType, DesignerActionItemCollection items, bool isExpanded, ref int groupSequence)
		{
			DesignerActionItemGroup groupAppearanceProperties = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Appearance"), groupSequence++);
			groupAppearanceProperties.IsExpanded				= isExpanded;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupAppearanceProperties))
			{
				items.Add(pic.GetPropertyActionItem("Theme"));
			}

			return groupAppearanceProperties;
		}

		#endregion //AddAppearancePropertiesGroup
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