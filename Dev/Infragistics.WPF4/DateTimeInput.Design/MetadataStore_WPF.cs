using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Editors.XamDateTimeInput.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Editors.XamDateTimeInput.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Editors.XamDateTimeInput);
				Assembly controlAssembly = t.Assembly;

				#region XamDateTimeInput Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamDateTimeInput");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDateTimeInputAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDateTimeInputAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowDropDown",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_AllowDropDown_Property")),
				    new DisplayNameAttribute("AllowDropDown"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowDropDownResolved",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_AllowDropDownResolved_Property")),
				    new DisplayNameAttribute("AllowDropDownResolved"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedMaxDate",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_ComputedMaxDate_Property")),
				    new DisplayNameAttribute("ComputedMaxDate"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedMinCalendarMode",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_ComputedMinCalendarMode_Property")),
				    new DisplayNameAttribute("ComputedMinCalendarMode"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedMinDate",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_ComputedMinDate_Property")),
				    new DisplayNameAttribute("ComputedMinDate"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DateValue",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_DateValue_Property")),
				    new DisplayNameAttribute("DateValue"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DropDownButtonDisplayMode",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_DropDownButtonDisplayMode_Property")),
				    new DisplayNameAttribute("DropDownButtonDisplayMode"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DropDownButtonStyle",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_DropDownButtonStyle_Property")),
				    new DisplayNameAttribute("DropDownButtonStyle"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DropDownButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_DropDownButtonVisibility_Property")),
				    new DisplayNameAttribute("DropDownButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDropDownOpen",
					new DescriptionAttribute(SR.GetString("XamDateTimeInput_IsDropDownOpen_Property")),
				    new DisplayNameAttribute("IsDropDownOpen"),
					new CategoryAttribute(SR.GetString("XamDateTimeInput_Properties"))
				);

				#endregion // XamDateTimeInput Properties

				#region Resources Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Resources");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Customizer",
					new DescriptionAttribute(SR.GetString("Resources_Customizer_Property")),
				    new DisplayNameAttribute("Customizer")				);

				#endregion // Resources Properties
                this.AddCustomAttributes(tableBuilder);
				return tableBuilder.CreateTable();
			}
		}
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