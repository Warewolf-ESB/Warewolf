using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{
	// MD 10/12/10 - TFS49853



	internal class ChartElement : XmlElementBase, IConsumedElementValueProvider
	{
		#region Constants

		/// <summary>graphicFrame</summary>
		public const string LocalName = "chart";

		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/chart/chart</summary>
		public const string QualifiedName =
			"http://schemas.openxmlformats.org/drawingml/2006/chart" +
			XmlElementBase.NamespaceSeparator +
			ChartElement.LocalName;

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return ChartElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			 if ( this.consumedValues != null )
                this.consumedValues.Clear();

			WorksheetChart chart = (WorksheetChart)manager.ContextStack[typeof(WorksheetChart)];

			if (chart == null)
			{
				Utilities.DebugFail("Could not find the chart on the context stack");
				return;
			}

			string relationshipId = null;

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					#region id

					case ChartElement.RelationshipIdAttributeName:
						{
							relationshipId = (string)XmlElementBase.GetAttributeValue(attribute, DataType.String, string.Empty);
							this.consumedValues.Add(ChartElement.RelationshipIdAttributeName, HandledAttributeIdentifier.ChartElement_Id);
						}
						break;
					#endregion id
				}
			}

			if (relationshipId == null)
			{
				Utilities.DebugFail("Could not get the chart data.");
				return;
			}

			byte[] data = manager.GetRelationshipDataFromActivePart(relationshipId) as byte[];

			if (data == null)
			{
				Utilities.DebugFail("Could not get the chart data.");
				return;
			}

			// MD 4/28/11 - TFS62775
			// Renamed for clarity.
			//chart.Data = data;
			chart.Excel2007RoundTripData = data;
		}

		#endregion Load

		#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			
		}

		#endregion Save

		#endregion Base class overrides

		#region IConsumedElementValueProvider implementation

		private Dictionary<string, HandledAttributeIdentifier> consumedValues = new Dictionary<string, HandledAttributeIdentifier>(2);

		/// <summary>
		/// off.x, off.y = Infragistics.Documents.Excel.WorksheetShape.GetBoundsInTwips
		/// </summary>
		Dictionary<string, HandledAttributeIdentifier> IConsumedElementValueProvider.ConsumedValues
		{
			get { return this.consumedValues; }
		}

		#endregion IConsumedElementValueProvider implementation
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