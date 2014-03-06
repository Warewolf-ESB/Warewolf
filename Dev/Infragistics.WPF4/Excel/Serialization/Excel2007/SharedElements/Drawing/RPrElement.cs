using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class RPrElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_TextCharacterProperties">
		// <sequence>
		// <element name="ln" type="CT_LineProperties" minOccurs="0" maxOccurs="1"/>
		// <group ref="EG_FillProperties" minOccurs="0" maxOccurs="1"/>
		// <group ref="EG_EffectProperties" minOccurs="0" maxOccurs="1"/>
		// <element name="highlight" type="CT_Color" minOccurs="0" maxOccurs="1"/>
		// <group ref="EG_TextUnderlineLine" minOccurs="0" maxOccurs="1"/>
		// <group ref="EG_TextUnderlineFill" minOccurs="0" maxOccurs="1"/>
		// <element name="latin" type="CT_TextFont" minOccurs="0" maxOccurs="1"/>
		// <element name="ea" type="CT_TextFont" minOccurs="0" maxOccurs="1"/>
		// <element name="cs" type="CT_TextFont" minOccurs="0" maxOccurs="1"/>
		// <element name="sym" type="CT_TextFont" minOccurs="0" maxOccurs="1"/>
		// <element name="hlinkClick" type="CT_Hyperlink" minOccurs="0" maxOccurs="1"/>
		// <element name="hlinkMouseOver" type="CT_Hyperlink" minOccurs="0" maxOccurs="1"/>
		// <element name="extLst" type="CT_OfficeArtExtensionList" minOccurs="0" maxOccurs="1"/>
		// </sequence>
		// <attribute name="kumimoji" type="xsd:boolean" use="optional"/>
		// <attribute name="lang" type="ST_TextLanguageID" use="optional"/>
		// <attribute name="altLang" type="ST_TextLanguageID" use="optional"/>
		// <attribute name="sz" type="ST_TextFontSize" use="optional"/>
		// <attribute name="b" type="xsd:boolean" use="optional"/>
		// <attribute name="i" type="xsd:boolean" use="optional"/>
		// <attribute name="u" type="ST_TextUnderlineType" use="optional"/>
		// <attribute name="strike" type="ST_TextStrikeType" use="optional"/>
		// <attribute name="kern" type="ST_TextNonNegativePoint" use="optional"/>
		// <attribute name="cap" type="ST_TextCapsType" use="optional"/>
		// <attribute name="spc" type="ST_TextPoint" use="optional"/>
		// <attribute name="normalizeH" type="xsd:boolean" use="optional"/>
		// <attribute name="baseline" type="ST_Percentage" use="optional"/>
		// <attribute name="noProof" type="xsd:boolean" use="optional"/>
		// <attribute name="dirty" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="err" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="smtClean" type="xsd:boolean" use="optional" default="true"/>
		// <attribute name="smtId" type="xsd:unsignedInt" use="optional" default="0"/>
		// <attribute name="bmk" type="xsd:string" use="optional"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>rPr</summary>
		public const string LocalName = "rPr";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/rPr</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			RPrElement.LocalName;

		internal const string SzAttributeName = "sz";
		internal const string BAttributeName = "b";
		internal const string BaselineAttributeName = "baseline";		// MD 11/8/11 - TFS85193
		internal const string IAttributeName = "i";
		internal const string UAttributeName = "u";
		internal const string StrikeAttributeName = "strike";

		// MD 11/8/11 - TFS85193
		internal const double SuperScriptBaseline = 0.30;
		internal const double SubscriptBaseline = -0.25;

		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return RPrElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			// MD 11/8/11 - TFS85193
			// We now have new types to deal with formatted strings with paragraphs.
			#region Old Code

			////  Get the FormattedStringRun with which this element is associated
			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////FormattedString fs = manager.ContextStack[typeof(FormattedString)] as FormattedString;
			//FormattedStringElement fs = manager.ContextStack[typeof(FormattedStringElement)] as FormattedStringElement;
			//
			//// MD 11/3/10 - TFS49093
			//// Get the internal property so we don't lazily create the collection.
			////List<FormattedStringRun> runs = fs.FormattingRuns;
			//// MD 4/12/11 - TFS67084
			//// Use the HasFormatting property instead of going to the internal property.
			////List<FormattedStringRun> runs = fs.FormattingRunsInternal;
			////
			////FormattedStringRun run = (runs != null && runs.Count > 0) ? runs[runs.Count - 1] : null;
			////if (run == null)
			////{
			////    Utilities.DebugFail("Could not get a FormattedStringRun in RPrElement.Load.");
			////    return;
			////}
			//if ( fs.HasFormatting == false )
			//{
			//    Utilities.DebugFail( "Could not get a FormattedStringRun in RPrElement.Load." );
			//    return;
			//}
			//
			//FormattedStringRun run = fs.FormattingRuns[0];
			//
			//WorkbookFontProxy font = run.Font;

			#endregion // Old Code
			FormattedTextRun shapeRun = (FormattedTextRun)manager.ContextStack[typeof(FormattedTextRun)];
			if (shapeRun == null)
			{
				Utilities.DebugFail("Could not get a ShapeFormattingRun in RPrElement.Load.");
				return;
			}

			WorkbookFontProxy font = shapeRun.GetFontInternal(manager.Workbook);
			
            object attributeValue = null;

            //  Push the run's font onto the context stack so the SrgbClrElement
            //  can access it easily.
            manager.ContextStack.Push( font );

			// MD 11/8/11 - TFS85193
			double baseline = 0;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{
					#region sz
					// Name = 'sz', Type = ST_TextFontSize, Default = 
					case RPrElement.SzAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Integer, 0 );
                        double fontSizePoints = Utilities.ToInteger(attributeValue);
                        fontSizePoints /= 100f;
                        int fontSizeTwips = (int)(Math.Ceiling(fontSizePoints * Utilities.TwipsPerPoint));

                        //  WorkbookFontProxy.Height
                        font.Height = fontSizeTwips;
					}
					break;
					#endregion sz

					#region b
					// Name = 'b', Type = Boolean, Default = 
					case RPrElement.BAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );

                        //  WorkbookFontProxy.Bold
                        bool bold = (bool)attributeValue;
                        font.Bold = bold ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
                    }
					break;
					#endregion b

					// MD 11/8/11 - TFS85193
					#region baseline
					// Name = 'baseline', Type = ST_Percentage, Default = 
					case RPrElement.BaselineAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Percentage, 100000);
						baseline = (double)attributeValue;
					}
					break;
					#endregion baseline

					#region i
					// Name = 'i', Type = Boolean, Default = 
					case RPrElement.IAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );

                        //  WorkbookFontProxy.Italic
                        bool italic = (bool)attributeValue;
                        font.Italic = italic ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
					}
					break;
					#endregion i

					#region u
					// Name = 'u', Type = ST_TextUnderlineType, Default = 
					case RPrElement.UAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.String, "none" );

                        //  WorkbookFontProxy.Underline
                        font.UnderlineStyle = Utilities.FromTextUnderlineType( attributeValue as string );
					}
					break;
					#endregion u

					#region strike
					// Name = 'strike', Type = ST_TextStrikeType, Default = 
					case RPrElement.StrikeAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.String, "noStrike" );

                        //  WorkbookFontProxy.Strikeout
                        font.Strikeout = Utilities.FromTextStrikeType( attributeValue as string );
					}
					break;
					#endregion strike

					// MD 7/23/12 - TFS117429
					default:
					shapeRun.RoundTrip2007Properties[attributeName] = attribute.Value;
					break;
				}
			}

			// MD 11/8/11 - TFS85193
			if (baseline >= RPrElement.SuperScriptBaseline)
				font.SuperscriptSubscriptStyle = FontSuperscriptSubscriptStyle.Superscript;
			else if (baseline <= RPrElement.SubscriptBaseline)
				font.SuperscriptSubscriptStyle = FontSuperscriptSubscriptStyle.Subscript;
			else
				font.SuperscriptSubscriptStyle = FontSuperscriptSubscriptStyle.None;
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
		}

			#endregion Save

		#endregion Base class overrides

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