using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class TElement : XmlElementBase
	{
		#region Constants

		/// <summary>t</summary>
		public const string LocalName = "t";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/t</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			TElement.LocalName;


		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return TElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			// MD 11/8/11 - TFS85193
			// We now have new types to deal with formatted strings with paragraphs.
			#region Old Code

			////  Get the FormattedString with which this element is associated
			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////FormattedString fs = manager.ContextStack[typeof(FormattedString)] as FormattedString;
			//FormattedStringElement fs = manager.ContextStack[typeof(FormattedStringElement)] as FormattedStringElement;

			////  Append the value of this element to the UnformattedString property.
			////  Note that we need to do this without triggering a notification, since
			////  we use that notification to clear the XML element cache.
			//int lengthBeforeAppending = fs.UnformattedString.Length;
			//string us = fs.UnformattedString;

			//// MD 2/4/11
			//// Found while fixing TFS65015
			//// When fixing TFS49093 below, this line was rewritten incorrectly.
			////// MD 11/3/10 - TFS49093
			////// This method has been removed. Just set the property on the element. It will not fire off notifications.
			//////fs.SetUnformattedStringHelper( us + value, false );
			////fs.UnformattedString = us;
			//fs.UnformattedString = us + value;

			////  Get the FormattedStringRun with which this element is associated
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
			////    Utilities.DebugFail("Could not get a FormattedStringRun in TElement.Load.");
			////    return;
			////}
			//if (fs.HasFormatting == false)
			//{
			//    Utilities.DebugFail( "Could not get a FormattedStringRun in TElement.Load." );
			//    return;
			//}

			//FormattedStringRun run = fs.FormattingRuns[0];

			////  Set the FirstFormattedChar property to the length of the
			////  UnformattedString property before we appended this element's
			////  value, i.e., the index of the first char in this element's value.
			//run.FirstFormattedChar = lengthBeforeAppending;

			#endregion // Old Code
			FormattedTextRun shapeRun = (FormattedTextRun)manager.ContextStack[typeof(FormattedTextRun)];
			if (shapeRun == null)
			{
				Utilities.DebugFail("Could not get a ShapeFormattingRun in RPrElement.Load.");
				return;
			}

			FormattedTextParagraph paragraph = shapeRun.Paragraph;
			paragraph.UnformattedString += value;
			Debug.Assert(shapeRun.UnformattedString == value, "This run is not aligned correctly.");
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