using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class NAMERecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			byte[] data = new byte[ 0 ];
			int dataIndex = 0;
			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );

			bool hidden =									( optionFlags & 0x0001 ) == 0x0001;
			bool isFunction =								( optionFlags & 0x0002 ) == 0x0002;
			bool isVBMacro =								( optionFlags & 0x0004 ) == 0x0004;
			bool isMacroName =								( optionFlags & 0x0008 ) == 0x0008;
			//bool complexFormula =							( optionFlags & 0x0010 ) == 0x0010;
			bool isBuiltIn =								( optionFlags & 0x0020 ) == 0x0020;
			//FunctionGroup functionGroup =  (FunctionGroup)( ( optionFlags & 0x0FC0 ) >> 6 );
			bool isBinaryData =								( optionFlags & 0x1000 ) == 0x1000;

			// MD 1/13/09 - TFS6720
			// We need to do a few more things before bailing for VBMacro functions. Specifically, we have to make sure the current
			// workbook reference on the serialization manager knows about it, because if it is not added in the internal collection,
			// the index of other named references will be off.
			//if ( isVBMacro || isBinaryData )
			if ( isBinaryData )
			{
                Utilities.DebugFail("These named reference types are not implemented yet.");
				return;
			}

			manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex ); // keyboard shortcut

			byte lengthOfName = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );
			ushort sizeOfFormulaData = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex ); // Not used
			ushort indexToSheetThatContainsName = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			byte lengthOfCustomMenuText = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );
			byte lengthOfDescriptionText = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );
			byte lengthOfHelpTopicText = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );
			byte lengthOfStatusBarText = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );

			string name = manager.CurrentRecordStream.ReadFormattedStringFromBuffer( lengthOfName, ref data, ref dataIndex ).UnformattedString;

			// MD 10/9/07 - BR27172
			// The name may need to be coerced a little for names of add-in functions
			if ( isFunction && name.StartsWith( "_xlfn." ) )
				name = name.Substring( 6 );

			Formula formula = Formula.Load( manager.CurrentRecordStream, sizeOfFormulaData, FormulaType.NamedReferenceFormula, ref data, ref dataIndex );

			if ( lengthOfCustomMenuText > 0 )
			{
				manager.CurrentRecordStream.ReadFormattedStringFromBuffer( lengthOfCustomMenuText, ref data, ref dataIndex ); // custom menu text
			}

			if ( lengthOfDescriptionText > 0 )
			{
				manager.CurrentRecordStream.ReadFormattedStringFromBuffer( lengthOfDescriptionText, ref data, ref dataIndex ); // description text
			}

			if ( lengthOfHelpTopicText > 0 )
			{
				manager.CurrentRecordStream.ReadFormattedStringFromBuffer( lengthOfHelpTopicText, ref data, ref dataIndex ); // help topic text
			}

			if ( lengthOfStatusBarText > 0 )
			{
				manager.CurrentRecordStream.ReadFormattedStringFromBuffer( lengthOfStatusBarText, ref data, ref dataIndex ); // status bar text
			}

			object scope = manager.Workbook;

			if ( indexToSheetThatContainsName > 0 )
				scope = manager.Workbook.Worksheets[ indexToSheetThatContainsName - 1 ];

			if ( isBuiltIn )
				name = NamedReferenceBase.NameFromBuiltInName( (BuiltInName)name[ 0 ] );

			// MD 6/16/12 - CalcEngineRefactor
			// The CurrentWorkbookReference now created unconnected named references, so create a NamedReference manually and add it
			// to the workbook reference.
			//NamedReference reference = 
			//    // MD 10/9/07 - BR27172
			//    // The hidden bit should be passed in here
			//    //(NamedReference)manager.CurrentWorkbookReference.GetNamedReference( name, scope, true );
			//    (NamedReference)manager.Workbook.CurrentWorkbookReference.GetNamedReference( name, scope, hidden, true );
			NamedReference reference = new NamedReference(manager.Workbook.NamedReferences, scope, hidden);
			reference.SetNameInternal(name, false);
			manager.Workbook.CurrentWorkbookReference.AddNamedReference(reference);

			reference.FormulaInternal = formula;

			// MD 10/9/07 - BR27172
			// Store other named reference options on the NamedReference
			reference.IsFunction = isFunction;
			reference.IsMacroName = isMacroName;

			Debug.Assert( reference.IsBuiltIn == isBuiltIn );

			// MD 10/30/11 - TFS90733
			// We now support the loading of VBMacro references.
			//// MD 1/13/09 - TFS6720
			//// If the named reference is a VB Macro or the formula is null, just bail out instead of throwing an excetpion.
			//if ( isVBMacro || formula == null )
			//{
			//    Utilities.DebugFail("The VB marco named reference type is not implemented yet.");
			//    return;
			//}

			// MD 9/2/08 - Excel2007 format
			// Moved to WorkbookSerializationManager.AddNonExternalNamedReferenceDuringLoad
			//if ( hidden )
			//{
			//    // MD 10/9/07 - BR27172
			//    // If we fail extracting special data from the name, store the named reference as a hidden named reference
			//    //NAMERecord.TryExtractCustomViewInfo( manager, reference );
			//    if ( NAMERecord.TryExtractCustomViewInfo( manager, reference ) == false )
			//        manager.Workbook.HiddenNamedReferences.Add( reference );
			//}
			//else if ( formula.PostfixTokenList.Count > 0 )
			//    manager.Workbook.NamedReferences.Add( reference );
			manager.AddNonExternalNamedReferenceDuringLoad( reference, hidden );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			NamedReferenceBase namedReference = manager.ContextStack.Get<NamedReferenceBase>();

			if ( namedReference == null )
			{
                Utilities.DebugFail("There is no named reference in the context stack.");
				return;
			}

			string name = namedReference.Name;

			if ( namedReference.IsBuiltIn )
				name = ( (char)namedReference.BuiltInName ).ToString();

			ushort optionFlags = 0;

			if ( namedReference.Hidden )
				optionFlags |= 0x0001;

			// MD 10/9/07 - BR27172
			// The name of functions should be coerced
			if ( namedReference.IsFunction )
			{
				name = "_xlfn." + name;
				optionFlags |= 0x0002;
			}

			// MD 10/9/07 - BR27172
			if ( namedReference.IsMacroName )
				optionFlags |= 0x0008;

			if ( namedReference.IsBuiltIn )
				optionFlags |= 0x0020;

			manager.CurrentRecordStream.Write( optionFlags );
			manager.CurrentRecordStream.Write( (byte)0 );
			manager.CurrentRecordStream.Write( (byte)name.Length );
			manager.CurrentRecordStream.Write( (ushort)0 ); // size of formula data, come back and write it later
			manager.CurrentRecordStream.Write( (ushort)0 );

			if ( namedReference.Scope is Workbook )
				manager.CurrentRecordStream.Write( (ushort)0 );
			else
			{
				int index = manager.Workbook.Worksheets.IndexOf( (Worksheet)namedReference.Scope );
				Debug.Assert( index >= 0 );
				manager.CurrentRecordStream.Write( (ushort)( index + 1 ) );
			}

			manager.CurrentRecordStream.Write( (byte)0 );
			manager.CurrentRecordStream.Write( (byte)0 );
			manager.CurrentRecordStream.Write( (byte)0 );
			manager.CurrentRecordStream.Write( (byte)0 );

			manager.CurrentRecordStream.Write( name );

			// MD 10/30/11 - TFS90733
			// Macro references will not have a formula.
			//int formulaLength = namedReference.FormulaInternal.Save( manager, manager.CurrentRecordStream, false, false );
			int formulaLength = 0;
			if (namedReference.FormulaInternal != null)
				formulaLength = namedReference.FormulaInternal.Save(manager.CurrentRecordStream, false, false);
			else
				Debug.Assert(namedReference.IsMacroName, "Only macro refereneces shouldn't have a formula.");

			manager.CurrentRecordStream.Position = 4;
			manager.CurrentRecordStream.Write( (ushort)formulaLength );
			manager.CurrentRecordStream.Position = manager.CurrentRecordStream.Length;
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.NAME; }
		}

		// MD 9/2/08
		// Moved to WorkbookSerializationManager
		#region Moved

		
#region Infragistics Source Cleanup (Region)









































































































































































#endregion // Infragistics Source Cleanup (Region)


        #endregion Moved
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