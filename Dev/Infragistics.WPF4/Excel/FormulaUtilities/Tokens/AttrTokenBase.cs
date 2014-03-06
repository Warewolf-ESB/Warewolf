using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Abstract base class for special attribute tokens.
	//    /// </summary> 
	//#endif
	//    internal abstract class AttrTokenBase : FormulaToken
	//    {
	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public AttrTokenBase( Formula formula )
	//        //    : base( formula, TokenClass.Control ) { }
	//        public AttrTokenBase()
	//            : base(TokenClass.Control) { }

	//        // MD 9/23/09 - TFS19150
	//        // Every read operation is relatively slow, so the buffer is now cached and passed into this method so we can get values from it.
	//        //public static AttrTokenBase CreateOperator( Formula formula, BiffRecordStream reader )
	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public static AttrTokenBase CreateOperator( Formula formula, BiffRecordStream reader, ref byte[] data, ref int dataIndex )
	//        public static AttrTokenBase CreateOperator(BiffRecordStream reader, ref byte[] data, ref int dataIndex)
	//        {
	//            AttrType attributeType = (AttrType)reader.ReadByteFromBuffer( ref data, ref dataIndex );

	//            switch ( attributeType )
	//            {
	//                // MD 10/22/10 - TFS36696
	//                // We don't need to store the formula on the token anymore.
	//                //case AttrType.Volatile:			return new AttrVolitileToken( formula );
	//                //case AttrType.If:				return new AttrIfToken( formula );
	//                //case AttrType.Choose:			return new AttrChooseToken( formula );
	//                //case AttrType.Skip:				return new AttrSkipToken( formula );
	//                //case AttrType.Sum:				return new AttrSumToken( formula );
	//                //case AttrType.Assign:			break;
	//                //case AttrType.Space:			return new AttrSpaceToken( formula );
	//                //case AttrType.SpaceVolatile:	return new AttrSpaceVolitileToken( formula );
	//                case AttrType.Volatile:			return new AttrVolitileToken();
	//                case AttrType.If:				return new AttrIfToken();
	//                case AttrType.Choose:			return new AttrChooseToken();
	//                case AttrType.Skip:				return new AttrSkipToken();
	//                case AttrType.Sum:				return new AttrSumToken();
	//                case AttrType.Assign:			break;
	//                case AttrType.Space:			return new AttrSpaceToken();
	//                case AttrType.SpaceVolatile:	return new AttrSpaceVolitileToken();

	//                default:
	//                    break;
	//            }

	//            Utilities.DebugFail( "Unknown attribute token" );
	//            return null;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            stream.Write( (byte)this.Type );

	//            // MD 10/22/10 - TFS36696
	//            // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//            //this.SaveAttr( stream, isForExternalNamedReference );
	//            this.SaveAttr(stream, isForExternalNamedReference, tokenPositionsInRecordStream);
	//        }

	//        // MD 7/24/07
	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture )
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//        {
	//            return this.GetType().Name;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //protected abstract void SaveAttr( BiffRecordStream stream, bool isForExternalNamedReference );
	//        protected abstract void SaveAttr(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream);

	//        public override Token Token
	//        {
	//            get { return Token.Attr; }
	//        }

	//        public abstract AttrType Type { get;}

	//        public virtual bool Volatile
	//        {
	//            get { return false; }
	//        }

	//        public enum AttrType : byte
	//        {
	//            Volatile = 0x01,
	//            If = 0x02,
	//            Choose = 0x04,
	//            Skip = 0x08,
	//            Sum = 0x10,
	//            Assign = 0x020,
	//            Space = 0x40,
	//            SpaceVolatile = 0x41
	//        }
	//    }

	#endregion // Old Code





	internal abstract class AttrTokenBase : FormulaToken
	{
		#region Constructor

		public AttrTokenBase()
			: base(TokenClass.Control) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write((byte)this.Type);
			this.SaveAttr(stream, isForExternalNamedReference, tokenPositionsInRecordStream);
		}

		#endregion // Save

		#region Token

		public override Token Token
		{
			get { return Token.Attr; }
		}

		#endregion // Token

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return this.GetType().Name;
		}

		#endregion // ToString

		#endregion // Base Class Overrides

		#region CreateOperator

		public static AttrTokenBase CreateOperator(BiffRecordStream reader, ref byte[] data, ref int dataIndex)
		{
			AttrType attributeType = (AttrType)reader.ReadByteFromBuffer(ref data, ref dataIndex);

			switch (attributeType)
			{
				case AttrType.Volatile: return new AttrVolitileToken();
				case AttrType.If: return new AttrIfToken();
				case AttrType.Choose: return new AttrChooseToken();
				case AttrType.Skip: return new AttrSkipToken();
				case AttrType.Sum: return new AttrSumToken();
				case AttrType.Assign: break;
				case AttrType.Space: return new AttrSpaceToken();
				case AttrType.SpaceVolatile: return new AttrSpaceVolitileToken();

				default:
					break;
			}

			Utilities.DebugFail("Unknown attribute token");
			return null;
		}

		#endregion // CreateOperator

		#region SaveAttr

		protected abstract void SaveAttr(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream);

		#endregion // SaveAttr

		#region Type

		public abstract AttrType Type { get; }

		#endregion // Type

		#region Volatile

		public virtual bool Volatile
		{
			get { return false; }
		}

		#endregion // Volatile


		#region AttrType enum

		public enum AttrType : byte
		{
			Volatile = 0x01,
			If = 0x02,
			Choose = 0x04,
			Skip = 0x08,
			Sum = 0x10,
			Assign = 0x020,
			Space = 0x40,
			SpaceVolatile = 0x41
		}

		#endregion // AttrType enum
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