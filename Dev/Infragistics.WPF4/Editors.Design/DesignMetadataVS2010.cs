using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design.Metadata;
using Infragistics.Windows.Editors;
using Microsoft.Windows.Design.Features;

[assembly: ProvideMetadata(typeof(Infragistics.Windows.Design.Editors.VS2010.DesignMetadataVS2010))]

namespace Infragistics.Windows.Design.Editors.VS2010
{
	internal class DesignMetadataVS2010 : IProvideAttributeTable
	{
		#region IProvideAttributeTable Members

		AttributeTable IProvideAttributeTable.AttributeTable
		{
			get 
			{
				AttributeTableBuilder builder = DesignMetadataHelper.GetAttributeTableBuilder();

				//VS2010 specific attributes.
				//
				// Set a ComplexBindingAttribute on the DatPresenterBase's DataSource property.
				builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.ValueEditor), new System.ComponentModel.DefaultBindingPropertyAttribute("Value"));
				builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.XamMonthCalendar), new System.ComponentModel.DefaultBindingPropertyAttribute("SelectedDate"));

				// JM 05-19-10 SmartTagFramework Integration
				builder.AddCustomAttributes(typeof(XamCheckEditor), new FeatureAttribute(typeof(CommonAdornerProvider)));
				builder.AddCustomAttributes(typeof(XamComboEditor), new FeatureAttribute(typeof(CommonAdornerProvider)));
				builder.AddCustomAttributes(typeof(XamTextEditor), new FeatureAttribute(typeof(CommonAdornerProvider)));
				builder.AddCustomAttributes(typeof(XamMaskedEditor), new FeatureAttribute(typeof(CommonAdornerProvider)));
				builder.AddCustomAttributes(typeof(XamDateTimeEditor), new FeatureAttribute(typeof(CommonAdornerProvider)));
				builder.AddCustomAttributes(typeof(XamNumericEditor), new FeatureAttribute(typeof(CommonAdornerProvider)));
				builder.AddCustomAttributes(typeof(XamCurrencyEditor), new FeatureAttribute(typeof(CommonAdornerProvider)));

				return builder.CreateTable();
			}
		}

		#endregion
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