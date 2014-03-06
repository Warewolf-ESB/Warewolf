using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Sorting;
using System.Globalization; 




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class SortConditionElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_SortCondition">
		//  <attribute name="descending" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="sortBy" type="ST_SortBy" use="optional" default="value"/>
		//  <attribute name="ref" type="ST_Ref" use="required"/>
		//  <attribute name="customList" type="ST_Xstring" use="optional"/>
		//  <attribute name="dxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="iconSet" type="ST_IconSetType" use="optional" default="3Arrows"/>
		//  <attribute name="iconId" type="xsd:unsignedInt" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "sortCondition";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			SortConditionElement.LocalName;

		private const string CustomListAttributeName = "customList";
		private const string DescendingAttributeName = "descending";
		private const string DxfIdAttributeName = "dxfId";
		private const string IconIdAttributeName = "iconId";
		private const string IconSetAttributeName = "iconSet";
		private const string RefAttributeName = "ref";
		private const string SortByAttributeName = "sortBy";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.sortCondition; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			string customList = default(string);
			bool descending = false;
			uint dxfId = default(uint);
			uint iconId = default(uint);
			ST_IconSetType iconSet = ST_IconSetType._3Arrows;
			string refValue = default(string);
			ST_SortBy sortBy = ST_SortBy.value;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case SortConditionElement.CustomListAttributeName:
						customList = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, customList);
						break;

					case SortConditionElement.DescendingAttributeName:
						descending = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, descending);
						break;

					case SortConditionElement.DxfIdAttributeName:
						dxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, dxfId);
						break;

					case SortConditionElement.IconIdAttributeName:
						iconId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, iconId);
						break;

					case SortConditionElement.IconSetAttributeName:
						iconSet = (ST_IconSetType)XmlElementBase.GetAttributeValue(attribute, DataType.ST_IconSetType, iconSet);
						break;

					case SortConditionElement.RefAttributeName:
						refValue = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Ref, refValue);
						break;

					case SortConditionElement.SortByAttributeName:
						sortBy = (ST_SortBy)XmlElementBase.GetAttributeValue(attribute, DataType.ST_SortBy, sortBy);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			if (String.IsNullOrEmpty(refValue))
			{
				Utilities.DebugFail("There was no ref attribute in the sortCondition element.");
				return;
			}

			int firstRowIndex;
			short firstColumnIndex;
			int lastRowIndex;
			short lastColumnIndex;
			// MD 4/9/12 - TFS101506
			//Utilities.ParseRegionAddress(refValue, manager.Workbook.CurrentFormat, CellReferenceMode.A1, null, -1,
			Utilities.ParseRegionAddress(refValue, manager.Workbook.CurrentFormat, CellReferenceMode.A1, CultureInfo.InvariantCulture, null, -1,
				out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

			TableContext tableContext = (TableContext)manager.ContextStack[typeof(TableContext)];
			if (tableContext == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTable on the context stack.");
				return;
			}
			else
			{
				Debug.Assert(firstColumnIndex == lastColumnIndex, "The sort condition for tables should apply one column only.");

				SortDirection sortDirection = descending ? SortDirection.Descending : SortDirection.Ascending;

				SortCondition sortCondition;
				switch (sortBy)
				{
					case ST_SortBy.cellColor:
						sortCondition = FillSortCondition.CreateFillSortCondition(manager, dxfId, sortDirection);
						break;

					case ST_SortBy.fontColor:
						sortCondition = FontColorSortCondition.CreateFontColorSortCondition(manager, dxfId, sortDirection);
						break;

					case ST_SortBy.icon:
						sortCondition = new IconSortCondition(iconSet, iconId, sortDirection);
						break;

					case ST_SortBy.value:
						if (customList == null)
							sortCondition = new OrderedSortCondition(sortDirection);
						else
							sortCondition = new CustomListSortCondition(sortDirection, customList.Split(','));
						break;

					default:
						Utilities.DebugFail("Unknown ST_SortBy value: " + sortBy);
						return;
				}

				if (sortCondition == null)
					return;

				tableContext.AddSortCondition(firstColumnIndex, sortCondition);
			}
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			EnumerableContext<KeyValuePair<WorksheetTableColumn, SortCondition>> sortConditions =
				(EnumerableContext<KeyValuePair<WorksheetTableColumn, SortCondition>>)manager.ContextStack[typeof(EnumerableContext<KeyValuePair<WorksheetTableColumn, SortCondition>>)];
			if (sortConditions == null)
			{
				Utilities.DebugFail("Cannot find the sortConditions on the context stack.");
				return;
			}

			KeyValuePair<WorksheetTableColumn, SortCondition> sortConditionPair = sortConditions.ConsumeCurrentItem();
			WorksheetTableColumn column = sortConditionPair.Key;
			SortCondition sortCondition = sortConditionPair.Value;

			string attributeValue = String.Empty;

			CustomListSortCondition customListSortCondition = sortCondition as CustomListSortCondition;
			if (customListSortCondition != null)
			{
				// Add the 'customList' attribute
				attributeValue = XmlElementBase.GetXmlString(customListSortCondition.GetListString(), DataType.ST_Xstring, default(string), false);
				XmlElementBase.AddAttribute(element, SortConditionElement.CustomListAttributeName, attributeValue);
			}

			// Add the 'descending' attribute
			attributeValue = XmlElementBase.GetXmlString(sortCondition.SortDirection == SortDirection.Descending, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, SortConditionElement.DescendingAttributeName, attributeValue);

			IColorSortCondition colorSortCondition = sortCondition as IColorSortCondition;
			if (colorSortCondition != null)
			{
				// Add the 'dxfId' attribute
				attributeValue = XmlElementBase.GetXmlString((uint)column.SortConditionDxfIdDuringSave, DataType.ST_DxfId);
				XmlElementBase.AddAttribute(element, SortConditionElement.DxfIdAttributeName, attributeValue);
			}

			IconSortCondition iconSortCondition = sortCondition as IconSortCondition;
			if (iconSortCondition != null)
			{
				// Add the 'iconId' attribute
				attributeValue = XmlElementBase.GetXmlString(iconSortCondition.IconIndex, DataType.UInt32, default(uint), false);
				XmlElementBase.AddAttribute(element, SortConditionElement.IconIdAttributeName, attributeValue);

				// Add the 'iconSet' attribute
				attributeValue = XmlElementBase.GetXmlString(iconSortCondition.IconSet, DataType.ST_IconSetType, ST_IconSetType._3Arrows, false);
				XmlElementBase.AddAttribute(element, SortConditionElement.IconSetAttributeName, attributeValue);
			}

			// Add the 'ref' attribute
			attributeValue = XmlElementBase.GetXmlString(column.SortRegion.ToString(CellReferenceMode.A1, false, true, true), DataType.ST_Ref, default(string), true);
			XmlElementBase.AddAttribute(element, SortConditionElement.RefAttributeName, attributeValue);

			// Add the 'sortBy' attribute
			attributeValue = XmlElementBase.GetXmlString(sortCondition.SortByValue, DataType.ST_SortBy, ST_SortBy.value, false);
			XmlElementBase.AddAttribute(element, SortConditionElement.SortByAttributeName, attributeValue);
		}

		#endregion // Save

		#endregion // Base Class Overrides
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