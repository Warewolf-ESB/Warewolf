using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements
{
	internal class ShadeElement : XmlElementBase
	{
		#region Constants

		public const string LocalName = "shade";

		public const string QualifiedName =
			ThemePart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			ShadeElement.LocalName;

		public const string ValAttributeName = "val";

		#endregion Constants

		#region Base Class Overrides

		#region ElementName

		public override string ElementName
		{
			get { return ShadeElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			// MD 3/12/12 - TFS102234
			#region Old Code

			//ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
			//
			//if (dataItem == null)
			//{
			//    Utilities.DebugFail("ChildDataItem object not found on the ContextStack.");
			//    return;
			//}
			//
			//if (dataItem.Data is Color == false)
			//{
			//    Utilities.DebugFail("Color object not found in ChildDataItem.");
			//    return;
			//}
			//
			//Color color = (Color)dataItem.Data;
			//
			//int val = (int)XmlElementBase.GetAttributeValue(element.Attributes[ShadeElement.ValAttributeName], DataType.Int32, -1);
			//if (val == -1)
			//{
			//    Utilities.DebugFail("val Attribute not properly set.");
			//    return;
			//}
			//
			//try
			//{
			//    color = Utilities.ApplyTint(color, -(1.0 - (double)val / 100000));
			//}
			//catch (Exception ex)
			//{
			//    Utilities.DebugFail("Unable to convert val attribute to a valid color. " + ex.Message);
			//    return;
			//}
			//
			//dataItem.Data = color;

			#endregion // Old Code
			int val = (int)XmlElementBase.GetAttributeValue(element.Attributes[ShadeElement.ValAttributeName], DataType.Int32, -1);
			if (val == -1)
			{
				Utilities.DebugFail("val Attribute not properly set.");
				return;
			}

			ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
			if (dataItem != null && dataItem.Data is Color)
				dataItem.Data = ShadeElement.ApplyShade((Color)dataItem.Data, val);

			ISolidColorItem solidColorItem = (ISolidColorItem)manager.ContextStack[typeof(ISolidColorItem)];
			if (solidColorItem != null)
				solidColorItem.Color = ShadeElement.ApplyShade(solidColorItem.Color, val);
		}

		#endregion Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			Utilities.DebugFail("Save() method for shade element not yet implemented.");
		}

		#endregion Save

		#endregion Base Class Overrides

		#region Methods

		// MD 3/12/12 - TFS102234
		#region ApplyShade

		private static Color ApplyShade(Color color, int val)
		{
			try
			{
				// MD 7/23/12 - TFS117429
				// The shade algorithm is not related to the tint algorithm.
				//return Utilities.ApplyTint(color, -(1.0 - (double)val / 100000));
				return Utilities.ApplyShade(color, (double)val / 100000);
			}
			catch (Exception ex)
			{
				Utilities.DebugFail("Unable to convert val attribute to a valid color. " + ex.Message);
				return color;
			}
		}

		#endregion // ApplyShade

		#endregion // Methods
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