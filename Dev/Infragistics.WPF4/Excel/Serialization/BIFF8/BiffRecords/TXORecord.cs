using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class TXORecord : Biff8RecordBase
	{
		// http://msdn.microsoft.com/en-us/library/dd924063(v=office.12).aspx
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// MD 7/20/2007 - BR25039
			// Implemented the processing of the TXO record
            //Utilities.DebugFail( "These shouldn't be saved or loaded until the text box shape is supported." );
			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();

			// MD 4/16/12 - TFS109297
			int hAlignment = Utilities.GetBits(optionFlags, 1, 3);
			int vAlignment = Utilities.GetBits(optionFlags, 4, 6);

			ushort rotation = manager.CurrentRecordStream.ReadUInt16();

			manager.CurrentRecordStream.Seek( 6, SeekOrigin.Current ); //Reserved

			ushort stringLength = manager.CurrentRecordStream.ReadUInt16();
			ushort lengthOfFormattingRuns = manager.CurrentRecordStream.ReadUInt16();

			manager.CurrentRecordStream.Seek( 4, SeekOrigin.Current ); //Reserved

			// MD 11/8/11 - TFS85193
			// We need to load FormattedString and FormattedText instances separately.
			#region Old Code

			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////FormattedString text;
			//FormattedStringElement text;

			//if ( stringLength == 0 )
			//{
			//    // MD 11/3/10 - TFS49093
			//    // The formatted string data is now stored on the FormattedStringElement.
			//    //text = new FormattedString( string.Empty );
			//    text = new FormattedStringElement(manager.Workbook, string.Empty);
			//}
			//else
			//{
			//    text = manager.CurrentRecordStream.ReadFormattedString( stringLength );

			//    Debug.Assert( lengthOfFormattingRuns % 8 == 0 );
			//    int numberOfFormattingRuns = lengthOfFormattingRuns / 8;

			//    for ( int i = 0; i < numberOfFormattingRuns; i++ )
			//    {
			//        ushort firstFormattedChar = manager.CurrentRecordStream.ReadUInt16();
			//        ushort fontRecordIndex = manager.CurrentRecordStream.ReadUInt16();
			//        manager.CurrentRecordStream.Seek( 4, SeekOrigin.Current ); //Reserved

			//        // The last formatted run starts after the end of the string and would cause an argument out of range exception,
			//        // only add the runs that start before the end of the string
			//        if ( firstFormattedChar < stringLength )
			//        {
			//            FormattedStringRun formattingRun = new FormattedStringRun( text, firstFormattedChar, manager.Workbook );
			//            formattingRun.Font.SetFontFormatting( manager.Fonts[ fontRecordIndex ] );

			//            text.FormattingRuns.Add( formattingRun );
			//        }
			//    }
			//}

			//// MD 9/2/08 - Cell Comments
			//// All shapes with text now support TXO records. Also, changed the local variable name
			////WorksheetCellCommentShape comment = (WorksheetCellCommentShape)manager.ContextStack[ typeof( WorksheetCellCommentShape ) ];
			//WorksheetShapeWithText shape = (WorksheetShapeWithText)manager.ContextStack[ typeof( WorksheetShapeWithText ) ];

			//if ( shape == null )
			//{
			//    Utilities.DebugFail("There is no shape with text in the context stack.");
			//    return;
			//}

			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement so wrap the loaded value in a FormattedString.
			////shape.Text = text;
			//shape.Text = new FormattedString(text);

			#endregion // Old Code
			WorksheetShape shape = (WorksheetShape)manager.ContextStack[typeof(WorksheetShape)];
			WorksheetCellComment comment = shape as WorksheetCellComment;
			WorksheetShapeWithText shapeWithText = shape as WorksheetShapeWithText;

			FormattedStringElement text;
			if (comment != null)
			{
				if (stringLength == 0)
				{
					// MD 2/1/12 - TFS100573
					//text = new FormattedStringElement(manager.Workbook, string.Empty);
					text = new FormattedStringElement(StringElement.EmptyStringUTF8);
				}
				else
				{
					// MD 1/31/12 - TFS100573
					// FormattedStrings need a derived element type.
					//text = manager.CurrentRecordStream.ReadFormattedString(stringLength);
					StringElement element = manager.CurrentRecordStream.ReadFormattedString(stringLength);
					text = new FormattedStringElement(element.UnformattedString);

					Debug.Assert(lengthOfFormattingRuns % 8 == 0);
					int numberOfFormattingRuns = lengthOfFormattingRuns / 8;

					for (int i = 0; i < numberOfFormattingRuns; i++)
					{
						ushort firstFormattedChar = manager.CurrentRecordStream.ReadUInt16();
						ushort fontRecordIndex = manager.CurrentRecordStream.ReadUInt16();
						manager.CurrentRecordStream.Seek(4, SeekOrigin.Current); //Reserved

						// The last formatted run starts after the end of the string and would cause an argument out of range exception,
						// only add the runs that start before the end of the string
						if (firstFormattedChar < stringLength)
						{
							FormattedStringRun formattingRun = new FormattedStringRun(text, firstFormattedChar);
							formattingRun.GetFont(manager.Workbook).SetFontFormatting(manager.Fonts[fontRecordIndex]);

							text.FormattingRuns.Add(formattingRun);
						}
					}
				}

				comment.Text = new FormattedString(manager.Workbook, text, false, false);
			}
			else if (shapeWithText != null)
			{
				FormattedText formattedText;
				if (stringLength == 0)
				{
					formattedText = new FormattedText(string.Empty);
				}
				else
				{
					// MD 1/31/12 - TFS100573
					//text = manager.CurrentRecordStream.ReadFormattedString(stringLength);
					//formattedText = new FormattedText(text.UnformattedString);
					StringElement element = manager.CurrentRecordStream.ReadFormattedString(stringLength);
					formattedText = new FormattedText(element.UnformattedString);

					using (IEnumerator<FormattedTextParagraph> paragraphs = formattedText.Paragraphs.GetEnumerator())
					{
						bool result = paragraphs.MoveNext();
						if (result == false)
						{
							Utilities.DebugFail("There should be at least one paragraph.");
							return;
						}

						FormattedTextParagraph currentParagraph = paragraphs.Current;
						FormattedTextParagraph nextParagraph = paragraphs.MoveNext() ? paragraphs.Current : null;

						Debug.Assert(lengthOfFormattingRuns % 8 == 0);
						int numberOfFormattingRuns = lengthOfFormattingRuns / 8;

						FormattedTextRun lastValidRun = null;
						for (int i = 0; i < numberOfFormattingRuns; i++)
						{
							ushort firstFormattedChar = manager.CurrentRecordStream.ReadUInt16();
							ushort fontRecordIndex = manager.CurrentRecordStream.ReadUInt16();
							manager.CurrentRecordStream.Seek(4, SeekOrigin.Current); //Reserved

							if (nextParagraph != null && nextParagraph.StartIndex <= firstFormattedChar)
							{
								if (firstFormattedChar != nextParagraph.StartIndex)
								{
									Utilities.DebugFail("The formatting runs should be aligned to the paragraphs");
									if (lastValidRun != null)
									{
										FormattedTextRun nextRun = new FormattedTextRun(nextParagraph, 0);
										nextRun.InitializeFrom(lastValidRun, manager.Workbook);
										nextParagraph.AddFormattingRun(nextRun);
									}
								}

								currentParagraph = nextParagraph;
								nextParagraph = paragraphs.MoveNext() ? paragraphs.Current : null;
							}

							// The last formatted run starts after the end of the string and would cause an argument out of range exception,
							// only add the runs that start before the end of the string
							if (firstFormattedChar < stringLength)
							{
								FormattedTextRun formattingRun = new FormattedTextRun(currentParagraph,
									firstFormattedChar - currentParagraph.StartIndex);
								formattingRun.GetFont(manager.Workbook).SetFontFormatting(manager.Fonts[fontRecordIndex]);
								currentParagraph.AddFormattingRun(formattingRun);
								lastValidRun = formattingRun;
							}
						}
					}
				}

				// MD 4/16/12 - TFS109297
				#region Load the alignments

				VerticalTextAlignment verticalAlignment;
				switch (vAlignment)
				{
					case 1:
						verticalAlignment = VerticalTextAlignment.Top;
						break;

					case 2:
						verticalAlignment = VerticalTextAlignment.Center;
						break;

					case 3:
						verticalAlignment = VerticalTextAlignment.Bottom;
						break;

					case 4: // justify 
					case 7: // justify distributed
						verticalAlignment = VerticalTextAlignment.Center;
						break;

					default:
						Utilities.DebugFail("Unknown vAlignment value: " + vAlignment);
						verticalAlignment = VerticalTextAlignment.Center;
						break;
				}
				formattedText.VerticalAlignment = verticalAlignment;

				HorizontalTextAlignment horizontalAlignment;
				switch (hAlignment)
				{
					case 1:
						horizontalAlignment = HorizontalTextAlignment.Left;
						break;

					case 2:
						horizontalAlignment = HorizontalTextAlignment.Center;
						break;

					case 3:
						horizontalAlignment = HorizontalTextAlignment.Right;
						break;

					case 4:
						horizontalAlignment = HorizontalTextAlignment.Justified;
						break;

					case 7:
						horizontalAlignment = HorizontalTextAlignment.Distributed;
						break;

					default:
						Utilities.DebugFail("Unknown hAlignment value: " + hAlignment);
						horizontalAlignment = HorizontalTextAlignment.Center;
						break;
				}

				for (int i = 0; i < formattedText.Paragraphs.Count; i++)
					formattedText.Paragraphs[i].Alignment = horizontalAlignment;

				#endregion // Load the alignments

				shapeWithText.Text = formattedText;
			}
			else
			{
				Utilities.DebugFail("There is no shape that supports text in the context stack.");
			}

			shape.TxoOptionFlags = optionFlags;

			// MD 7/24/12 - TFS115693
			// The record stores the text rotation, not the shape rotation. We will just round-trip it for now.
			//shape.Rotation = rotation;
			shape.TxoRotation = rotation;
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 11/8/11 - TFS85193
			// We need to save FormattedString and FormattedText instances separately.
			#region Old Code

			//// MD 7/20/2007 - BR25039
			//// Implemented the processing of the TXO record
			////Utilities.DebugFail( "These shouldn't be saved or loaded until the text box shape is supported." );
			//// MD 4/18/11 - TFS62026
			//// This is not needed.
			////long pos = manager.WorkbookStream.Position - 4;

			//// MD 9/2/08 - Cell Comments
			//// All shapes with text now support TXO records. Also, changed the local variable name
			////WorksheetCellCommentShape comment = (WorksheetCellCommentShape)manager.ContextStack[ typeof( WorksheetCellCommentShape ) ];
			//WorksheetShapeWithText shape = (WorksheetShapeWithText)manager.ContextStack[ typeof( WorksheetShapeWithText ) ];

			//if ( shape == null )
			//{
			//    Utilities.DebugFail("There is no shape with text in the context stack.");
			//    return;
			//}

			//manager.CurrentRecordStream.Write( (ushort)shape.TxoOptionFlags );

			//// MD 7/15/11 - Shape support
			//// The rotation can be negative as well.
			////manager.CurrentRecordStream.Write( (ushort)shape.Rotation );
			//manager.CurrentRecordStream.Write((short)shape.Rotation);

			//manager.CurrentRecordStream.Write( new byte[ 6 ] ); // Reserved

			//// MD 9/2/08 - Cell Comments
			//// The Text cannot be null anymore
			////int stringLength = comment.Text == null ? 0 : comment.Text.UnformattedString.Length;
			//int stringLength = shape.Text.UnformattedString.Length;

			//manager.CurrentRecordStream.Write( (ushort)stringLength );

			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			//// MD 4/12/11 - TFS67084
			//// Removed the FormattedStringProxy class. The FormattedString holds the element directly now.
			////FormattedStringElement formattedStringElement = shape.Text.Proxy.Element;
			//FormattedStringElement formattedStringElement = shape.Text.Element;

			//// MD 7/18/11 - Shape support
			//// Shapes must have at least one formatting run.
			////int formattingRunsCount = formattedStringElement.HasFormatting ? formattedStringElement.FormattingRuns.Count : 0;
			//if (formattedStringElement.HasFormatting == false)
			//    formattedStringElement.FormattingRuns.Add(new FormattedStringRun(formattedStringElement, 0, manager.Workbook));

			//int formattingRunsCount = formattedStringElement.FormattingRuns.Count;

			//if ( stringLength == 0 )
			//    manager.CurrentRecordStream.Write( (ushort)0 );
			//else
			//{
			//    // MD 11/3/10 - TFS49093
			//    // Use the cached formatting runs count.
			//    //manager.CurrentRecordStream.Write( (ushort)( 8 * ( shape.Text.FormattingRuns.Count + 1 ) ) );
			//    manager.CurrentRecordStream.Write((ushort)(8 * (formattingRunsCount + 1)));
			//}

			//manager.CurrentRecordStream.Write( new byte[ 4 ] ); // Reserved

			//// If there is no text, no CONTINUE records should be written
			//if ( stringLength == 0 )
			//    return;

			//// The next piece of data must be in a CONTINUE record
			//manager.CurrentRecordStream.CapCurrentBlock();

			//// Write the comment text in the current CONTINUE block, but don't let the formatting runs 
			//// be serialized in this block
			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////manager.CurrentRecordStream.Write( shape.Text, false );
			//manager.CurrentRecordStream.Write(formattedStringElement, false);

			//// The next piece of data must be in a CONTINUE record
			//manager.CurrentRecordStream.CapCurrentBlock();

			//// MD 7/18/11 - Shape support
			//// This is no longer needed because we force the text to have at least one formatting run above.
			////Debug.Assert(
			////    String.IsNullOrEmpty( shape.Text.UnformattedString ) ||
			////    // MD 11/3/10 - TFS49093
			////    // Use the cached formatting runs count.
			////    //( shape.Text.FormattingRuns.Count >= 1 && shape.Text.FormattingRuns[ 0 ].FirstFormattedChar == 0 ),
			////    (formattingRunsCount >= 1 && formattedStringElement.FormattingRuns[0].FirstFormattedChar == 0),
			////    "There must be at least one formatting run and there must be one for the first char");

			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement. Also, only iterated the collection when we know we have items.
			////foreach ( FormattedStringRun run in shape.Text.FormattingRuns )
			//// MD 7/18/11 - Shape support
			//// This is no longer needed because we force the text to have at least one formatting run above.
			////if(formattingRunsCount > 0)
			////{
			//foreach (FormattedStringRun run in formattedStringElement.FormattingRuns)
			//{
			//    // MD 11/3/10 - TFS49093
			//    // Mkae sure we have an index in the font collection.
			//    Debug.Assert(run.Font.Element.IndexInFontCollection >= 0, "This Font was not placed in the fonts collection.");

			//    manager.CurrentRecordStream.Write( (ushort)run.FirstFormattedChar );
			//    manager.CurrentRecordStream.Write( (ushort)run.Font.Element.IndexInFontCollection );
			//    manager.CurrentRecordStream.Write( new byte[ 4 ] ); // Reserved
			//}
			////} 

			//// There should always be an extra formatting run with the formatted character being after 
			//// the end of the string
			//manager.CurrentRecordStream.Write( (ushort)stringLength );
			//manager.CurrentRecordStream.Write( (ushort)0 );
			//manager.CurrentRecordStream.Write( new byte[ 4 ] ); // Reserved

			#endregion // Old Code
			WorksheetShape shape = (WorksheetShape)manager.ContextStack[typeof(WorksheetShape)];
			if (shape == null)
			{
				Utilities.DebugFail("There is no shape that supports text in the context stack.");
				return;
			}

			WorksheetCellComment comment = shape as WorksheetCellComment;
			WorksheetShapeWithText shapeWithText = shape as WorksheetShapeWithText;

			// MD 4/16/12 - TFS109297
			// We now honor the alignment fields.
			//manager.CurrentRecordStream.Write((ushort)shape.TxoOptionFlags);
			ushort optionFlags = (ushort)shape.TxoOptionFlags;
			if (shapeWithText != null && shapeWithText.Text != null)
			{
				// Clear out the vAlignment and hAlignment values
				optionFlags &= 0xFF81;

				int vAlignment;
				switch (shapeWithText.Text.VerticalAlignment)
				{
					case VerticalTextAlignment.Top:
						vAlignment = 1;
						break;

					case VerticalTextAlignment.Center:
						vAlignment = 2;
						break;

					case VerticalTextAlignment.Bottom:
						vAlignment = 3;
						break;

					default:
						Utilities.DebugFail("Unknown VerticalTextAlignment: " + shapeWithText.Text.VerticalAlignment);
						vAlignment = 2;
						break;
				}

				Utilities.AddBits(ref optionFlags, vAlignment, 4, 6);

				int hAlignment = 2;
				if (shapeWithText.Text.Paragraphs.Count != 0)
				{
					switch (shapeWithText.Text.Paragraphs[0].Alignment)
					{
						case HorizontalTextAlignment.Left:
							hAlignment = 1;
							break;

						case HorizontalTextAlignment.Center:
							hAlignment = 2;
							break;

						case HorizontalTextAlignment.Right:
							hAlignment = 3;
							break;

						case HorizontalTextAlignment.Justified:
						case HorizontalTextAlignment.JustifiedLow:
							hAlignment = 4;
							break;

						case HorizontalTextAlignment.Distributed:
						case HorizontalTextAlignment.ThaiDistributed:
							hAlignment = 7;
							break;

						default:
							Utilities.DebugFail("Unknown HorizontalTextAlignment: " + shapeWithText.Text.Paragraphs[0].Alignment);
							vAlignment = 2;
							break;
					}
				}

				Utilities.AddBits(ref optionFlags, hAlignment, 1, 3);
			}

			manager.CurrentRecordStream.Write(optionFlags);

			// MD 7/24/12 - TFS115693
			// The record stores the text rotation, not the shape rotation. We will just round-trip it for now.
			//manager.CurrentRecordStream.Write((short)shape.Rotation);
			manager.CurrentRecordStream.Write(shape.TxoRotation);

			manager.CurrentRecordStream.Write(new byte[6]); // Reserved

			if (comment != null)
			{
				// MD 1/31/12 - TFS100573
				//FormattedStringElement formattedStringElement = comment.Text.Element;
				FormattedStringElement formattedStringElement = comment.Text.Element as FormattedStringElement;
				if (formattedStringElement == null)
					formattedStringElement = comment.Text.ConvertToFormattedStringElement();

				List<FormattingRunBase> formattingRuns = new List<FormattingRunBase>(formattedStringElement.FormattingRuns);

				if (formattingRuns.Count == 0)
					formattingRuns.Add(new FormattedStringRun(formattedStringElement, 0));

				// MD 1/18/12 - 12.1 - Cell Format Updates
				//TXORecord.SaveText(manager, FontResolverType.Normal, formattedStringElement.UnformattedString, formattingRuns);
				TXORecord.SaveText(manager, formattedStringElement.UnformattedString, formattingRuns);
			}
			else if (shapeWithText != null)
			{
				if (shapeWithText.Text == null)
				{
					manager.CurrentRecordStream.Write((ushort)0);
					return;
				}

				List<FormattingRunBase> formattingRuns = new List<FormattingRunBase>();
				for (int i = 0; i < shapeWithText.Text.Paragraphs.Count; i++)
				{
					FormattedTextParagraph paragraph = shapeWithText.Text.Paragraphs[i];
					formattingRuns.AddRange(paragraph.GetFormattingRuns(manager.Workbook));
				}

				if (formattingRuns.Count == 0)
				{
					FormattedTextParagraph paragraph;
					if (shapeWithText.Text.Paragraphs.Count == 0)
						paragraph = shapeWithText.Text.Paragraphs.Add(string.Empty);
					else
						paragraph = shapeWithText.Text.Paragraphs[0];

					formattingRuns.Add(new FormattedTextRun(paragraph, 0));
				}

				// MD 1/18/12 - 12.1 - Cell Format Updates
				//TXORecord.SaveText(manager, FontResolverType.ShapeWithText, shapeWithText.Text.ToString(), formattingRuns);
				TXORecord.SaveText(manager, shapeWithText.Text.ToString(), formattingRuns);
			}
			else
			{
				Utilities.DebugFail("There is no shape that supports text in the context stack.");
				return;
			}
		}

		// MD 1/18/12 - 12.1 - Cell Format Updates
		//private static void SaveText(BIFF8WorkbookSerializationManager manager, FontResolverType fontResolverType, string unformattedString, List<FormattingRunBase> formattingRuns)
		private static void SaveText(BIFF8WorkbookSerializationManager manager, string unformattedString, List<FormattingRunBase> formattingRuns)
		{
			manager.CurrentRecordStream.Write((ushort)unformattedString.Length);

			int formattingRunsCount = formattingRuns.Count;

			if (unformattedString.Length == 0)
				manager.CurrentRecordStream.Write((ushort)0);
			else
				manager.CurrentRecordStream.Write((ushort)(8 * (formattingRunsCount + 1)));

			manager.CurrentRecordStream.Write(new byte[4]); // Reserved

			// If there is no text, no CONTINUE records should be written
			if (unformattedString.Length == 0)
				return;

			// The next piece of data must be in a CONTINUE record
			manager.CurrentRecordStream.CapCurrentBlock();

			// Write the comment text in the current CONTINUE block, but don't let the formatting runs 
			// be serialized in this block
			manager.CurrentRecordStream.Write(unformattedString);

			// The next piece of data must be in a CONTINUE record
			manager.CurrentRecordStream.CapCurrentBlock();

			foreach (FormattingRunBase run in formattingRuns)
			{
				ushort indexInFontCollection;
				WorkbookFontProxy fontProxy = run.GetFontInternal(manager.Workbook);

				// MD 1/18/12 - 12.1 - Cell Format Updates
				//int tempIndexInFontCollection = fontProxy.Element.GetIndexInFontCollection(fontResolverType);
				int tempIndexInFontCollection = fontProxy.SaveIndex;

				if (tempIndexInFontCollection < 0)
				{
					Utilities.DebugFail("This Font was not placed in the fonts collection.");
					indexInFontCollection = 0;
				}
				else
				{
					indexInFontCollection = (ushort)tempIndexInFontCollection;
				}

				manager.CurrentRecordStream.Write((ushort)run.FirstFormattedCharAbsolute);
				manager.CurrentRecordStream.Write(indexInFontCollection);
				manager.CurrentRecordStream.Write(new byte[4]); // Reserved
			}

			// There should always be an extra formatting run with the formatted character being after 
			// the end of the string
			manager.CurrentRecordStream.Write((ushort)unformattedString.Length);
			manager.CurrentRecordStream.Write((ushort)0);
			manager.CurrentRecordStream.Write(new byte[4]); // Reserved
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.TXO; }
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