using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal abstract class Biff8RecordBase : BiffRecordBase<BIFF8RecordType, BIFF8WorkbookSerializationManager>
	{
		// MD 4/18/11 - TFS62026
		// We will cache this delegate so it is not recreated each time we need it.
		[ThreadStatic]
		private static CreateBiffRecordDelegate createBiffRecordCallback;

		public static CreateBiffRecordDelegate CreateBiffRecordCallback
		{
			get
			{
				if (Biff8RecordBase.createBiffRecordCallback == null)
					Biff8RecordBase.createBiffRecordCallback = new CreateBiffRecordDelegate(Biff8RecordBase.CreateBiffRecord);

				return Biff8RecordBase.createBiffRecordCallback;
			}
		}

		public static BIFF8RecordType GetContinuationBlockType( BIFF8RecordType type )
		{
			switch ( type )
			{
				// The OBJ record should never be continued
				case BIFF8RecordType.OBJ:
				case BIFF8RecordType.Default:
					return BIFF8RecordType.Default;

				case BIFF8RecordType.MERGEDCELLS:
					return BIFF8RecordType.MERGEDCELLS;

				// MD 11/29/11 - TFS96205
				case BIFF8RecordType.THEME:
				case BIFF8RecordType.AUTOFILTER12:
				case BIFF8RecordType.SORTDATA12:
					return BIFF8RecordType.CONTINUEFRT12;
			}

			return BIFF8RecordType.CONTINUE;
		}

		public static BiffRecordBase<BIFF8RecordType, BIFF8WorkbookSerializationManager> CreateBiffRecord( BIFF8RecordType type )
		{
		    switch ( type )
		    {
		        case (BIFF8RecordType)0xDC:
		            // PARAMQRY
		            // SXEXT
                    Utilities.DebugFail("Figure out how to handle this record.");
					return null;

				case BIFF8RecordType.Record1904:			return new Record1904();
				case BIFF8RecordType.ADDIN:					break;
				case BIFF8RecordType.ADDMENU:				break;
				case BIFF8RecordType.ARRAY:					return new ARRAYRecord();
				case BIFF8RecordType.AUTOFILTER:			break;
				case BIFF8RecordType.AUTOFILTER12:			return new AUTOFILTER12Record();				// MD 2/20/12 - 12.1 - Table Support
				case BIFF8RecordType.AUTOFILTERINFO:		break;
				case BIFF8RecordType.BACKUP:				return new BACKUPRecord();
				case BIFF8RecordType.BLANK:					return new BLANKRecord();
				case BIFF8RecordType.BITMAP:				return new BITMAPRecord();
		        case BIFF8RecordType.BOF:					return new BOFRecord();
				case BIFF8RecordType.BOOKBOOL:				return new BOOKBOOLRecord();
				case BIFF8RecordType.BOOKEXT:				return new BOOKEXTRecord();
				case BIFF8RecordType.BOOLERR:				return new BOOLERRRecord();
				case BIFF8RecordType.BOTTOMMARGIN:			return new BOTTOMMARGINRecord();
				case BIFF8RecordType.BOUNDSHEET:			return new BOUNDSHEETRecord();
				case BIFF8RecordType.CALCCOUNT:				return new CALCCOUNTRecord();
				case BIFF8RecordType.CALCMODE:				return new CALCMODERecord();
				case BIFF8RecordType.CELLWATCH:				break;
				case BIFF8RecordType.CF:					break;
				case BIFF8RecordType.CODEPAGE:				return new CODEPAGERecord();
				case BIFF8RecordType.COLINFO:				return new COLINFORecord();
				case BIFF8RecordType.CONDFMT:				break;
				case BIFF8RecordType.CONTINUE:				break;
				case BIFF8RecordType.CONTINUEFRT:			break;
				case BIFF8RecordType.CONTINUEFRT11:			break;
				case BIFF8RecordType.CONTINUEFRT12:			break;											// MD 11/29/11 - TFS96205
				case BIFF8RecordType.COORDLIST:				break;
				case BIFF8RecordType.COUNTRY:				return new COUNTRYRecord();
				case BIFF8RecordType.CRASHRECERR:			break;
				case BIFF8RecordType.CRN:					return new CRNRecord();
				case BIFF8RecordType.CRTCOOPT:				break;
				case BIFF8RecordType.DATALABEXT:			break;
				case BIFF8RecordType.DATALABEXTCONTENTS:	break;
				case BIFF8RecordType.DBCELL:				return new DBCELLRecord();
				case BIFF8RecordType.DBQUERYEXT:			break;
				case BIFF8RecordType.DCON:					break;
				case BIFF8RecordType.DCONBIN:				break;
				case BIFF8RecordType.DCONNAME:				break;
				case BIFF8RecordType.DCONREF:				break;
				case BIFF8RecordType.DEFAULTROWHEIGHT:		return new DEFAULTROWHEIGHTRecord();
				case BIFF8RecordType.DEFCOLWIDTH:			return new DEFCOLWIDTHRecord();
				case BIFF8RecordType.DELMENU:				break;
				case BIFF8RecordType.DELTA:					return new DELTARecord();
				case BIFF8RecordType.DIMENSIONS:			return new DIMENSIONSRecord();
				case BIFF8RecordType.DOCROUTE:				break;
				case BIFF8RecordType.DROPDOWNOBJIDS:		break;
				case BIFF8RecordType.DSF:					return new DSFRecord();
				case BIFF8RecordType.DV:					return new DVRecord();							// MD 9/12/08 - TFS6887
				case BIFF8RecordType.DVAL:					return new DVALRecord();						// MD 9/12/08 - TFS6887
				case BIFF8RecordType.DXF:					return new DXFRecord();							// MD 2/21/12 - 12.1 - Table Support
				case BIFF8RecordType.EDG:					break;
				case BIFF8RecordType.EFONT:					break;
				case BIFF8RecordType.EOF:					return new EOFRecord();
				case BIFF8RecordType.EXCEL9FILE:			return new EXCEL9FILERecord();
				case BIFF8RecordType.EXTERNCOUNT:			break;
				case BIFF8RecordType.EXTERNNAME:			return new EXTERNNAMERecord();
				case BIFF8RecordType.EXTERNSHEET:			return new EXTERNSHEETRecord();
				case BIFF8RecordType.EXTSST:				return new EXTSSTRecord();
				case BIFF8RecordType.EXTSTRING:				break;
				case BIFF8RecordType.FEAT11:				return new FEAT11Record();						// MD 2/18/12 - 12.1 - Table Support
				case BIFF8RecordType.FEAT12:				return new FEAT12Record();						// MD 2/20/12 - 12.1 - Table Support
				case BIFF8RecordType.FEATHEADR:				return new FEATHEADRRecord();					// MD 1/26/12 - 12.1 - Cell Format Updates
				case BIFF8RecordType.FEATHEADR11:			return new FEATHEADR11Record();					// MD 2/18/12 - 12.1 - Table Support
				case BIFF8RecordType.FILEPASS:				break;
				case BIFF8RecordType.FILESHARING:			break;
				case BIFF8RecordType.FILESHARING2:			break;
				case BIFF8RecordType.FILTERMODE:			break;
				case BIFF8RecordType.FMQRY:					break;
				case BIFF8RecordType.FMSQRY:				break;
				case BIFF8RecordType.FNGROUPCOUNT:			return new FNGROUPCOUNTRecord();
				case BIFF8RecordType.FNGROUPNAME:			break;
				case BIFF8RecordType.FONT:					return new FONTRecord();
				case BIFF8RecordType.FOOTER:				return new FOOTERRecord();
				case BIFF8RecordType.FORMAT:				return new FORMATRecord();
				case BIFF8RecordType.FORMULA:				return new FORMULARecord();
				case BIFF8RecordType.GCW:					break;
				case BIFF8RecordType.GRIDSET:				return new GRIDSETRecord();
				case BIFF8RecordType.GUTS:					return new GUTSRecord();
				case BIFF8RecordType.HCENTER:				return new HCENTERRecord();
				case BIFF8RecordType.HEADER:				return new HEADERRecord();
				case BIFF8RecordType.HFPicture:				return new HFPICTURERecord();					// MD 10/30/11 - TFS90733
				case BIFF8RecordType.HIDEOBJ:				return new HIDEOBJRecord();
				case BIFF8RecordType.HLINK:					break;
				case BIFF8RecordType.HLINKTOOLTIP:			break;
				case BIFF8RecordType.HORIZONTALPAGEBREAKS:	return new HORIZONTALPAGEBREAKSRecord();		// MD 2/1/11 - Data Validation support
				case BIFF8RecordType.IMDATA:				break;
				case BIFF8RecordType.INDEX:					return new INDEXRecord();
				case BIFF8RecordType.INTERFACEEND:			return new INTERFACEENDRecord();
				case BIFF8RecordType.INTERFACEHDR:			return new INTERFACEHDRRecord();
				case BIFF8RecordType.ITERATION:				return new ITERATIONRecord();
				case BIFF8RecordType.LABEL:					return new LABELRecord();						// MD 1/9/08 - Found while fixing BR29299
				case BIFF8RecordType.LABELRANGES:			break;
				case BIFF8RecordType.LABELSST:				return new LABELSSTRecord();
				case BIFF8RecordType.LEFTMARGIN:			return new LEFTMARGINRecord();
				case BIFF8RecordType.LHNGRAPH:				break;
				case BIFF8RecordType.LHRECORD:				break;
				case BIFF8RecordType.LIST12:				return new LIST12Record();						// MD 2/19/12 - 12.1 - Table Support
				case BIFF8RecordType.LISTCF:				break;
				case BIFF8RecordType.LISTCONDFMT:			break;
				case BIFF8RecordType.LISTDV:				break;
				case BIFF8RecordType.LISTFIELD:				break;
				case BIFF8RecordType.LISTOBJ:				break;
				case BIFF8RecordType.LNEXT:					break;
				case BIFF8RecordType.LPR:					break;
				case BIFF8RecordType.MERGEDCELLS:			return new MERGEDCELLSRecord();
				case BIFF8RecordType.MKREXT:				break;
				case BIFF8RecordType.MMS:					return new MMSRecord();
				case BIFF8RecordType.MSODRAWING:			return new MSODRAWINGRecord();
				case BIFF8RecordType.MSODRAWINGGROUP:		return new MSODRAWINGGROUPRecord();
				case BIFF8RecordType.MSODRAWINGSELECTION:	return new MSODRAWINGSELECTIONRecord();			// MD 7/20/2007 - BR25039
				case BIFF8RecordType.MULBLANK:				return new MULBLANKRecord();
				case BIFF8RecordType.MULRK:					return new MULRKRecord();
				case BIFF8RecordType.NAME:					return new NAMERecord();
				case BIFF8RecordType.NAMEEXT:				return new NAMEEXTRecord();
				case BIFF8RecordType.NOTE:					return new NOTERecord();						// MD 7/20/2007 - BR25039
				case BIFF8RecordType.NUMBER:				return new NUMBERRecord();
				case BIFF8RecordType.OBJ:					return new OBJRecord();
				case BIFF8RecordType.OBJPROTECT:			break;
				case BIFF8RecordType.OBPROJ:				return new OBPROJRecord();						// MD 10/1/08 - TFS8453
				case BIFF8RecordType.OLEDBCONN:				break;
				case BIFF8RecordType.OLESIZE:				break;
				case BIFF8RecordType.PAGELAYOUTINFO:		return new PAGELAYOUTINFORecord();
				case BIFF8RecordType.PALETTE:				return new PALETTERecord();
				case BIFF8RecordType.PANE:					return new PANERecord();
				case BIFF8RecordType.PASSWORD:				return new PASSWORDRecord();
				case BIFF8RecordType.PLS:					return new PLSRecord();
				case BIFF8RecordType.PLV:					break;
				case BIFF8RecordType.PRECISION:				return new PRECISIONRecord();
				case BIFF8RecordType.PRINTGRIDLINES:		return new PRINTGRIDLINESRecord();
				case BIFF8RecordType.PRINTHEADERS:			return new PRINTHEADERSRecord();
				case BIFF8RecordType.PROTECT:				return new PROTECTRecord();
				case BIFF8RecordType.PROT4REV:				return new PROT4REVRecord();
				case BIFF8RecordType.PROT4REVPASS:			return new PROT4REVPASSRecord();
				case BIFF8RecordType.PUB:					break;
				case BIFF8RecordType.QSI:					break;
				case BIFF8RecordType.QSIF:					break;
				case BIFF8RecordType.QSIR:					break;
				case BIFF8RecordType.QSISXTAG:				break;
				case BIFF8RecordType.REALTIMEDATA:			break;
				case BIFF8RecordType.RECALCID:				return new RECALCIDRecord();
				case BIFF8RecordType.RECIPNAME:				break;
				case BIFF8RecordType.REFMODE:				return new REFMODERecord();
				case BIFF8RecordType.REFRESHALL:			return new REFRESHALLRecord();
				case BIFF8RecordType.RIGHTMARGIN:			return new RIGHTMARGINRecord();
				case BIFF8RecordType.RK:					return new RKRecord();
				case BIFF8RecordType.ROW:					return new ROWRecord();
				case BIFF8RecordType.SAVERECALC:			return new SAVERECALCRecord();
				case BIFF8RecordType.SCENARIO:				break;
				case BIFF8RecordType.SCENMAN:				break;
				case BIFF8RecordType.SCENPROTECT:			break;
				case BIFF8RecordType.SCL:					return new SCLRecord();
				case BIFF8RecordType.SELECTION:				return new SELECTIONRecord();
				case BIFF8RecordType.SETUP:					return new SETUPRecord();
				case BIFF8RecordType.SHEETEXT:				return new SHEETEXTRecord();
				case BIFF8RecordType.SHRFMLA:				return new SHRFMLARecord();
				case BIFF8RecordType.SORT:					break;
				case BIFF8RecordType.SORTDATA12:			return new SORTDATA12Record();
				case BIFF8RecordType.SOUND:					break;
				case BIFF8RecordType.SST:					return new SSTRecord();
				case BIFF8RecordType.STANDARDWIDTH:			return new STANDARDWIDTHRecord();
				case BIFF8RecordType.STRING:				return new STRINGRecord();
				case BIFF8RecordType.STYLE:					return new STYLERecord();
				case BIFF8RecordType.STYLEEXT:				return new STYLEEXTRecord();					// MD 1/26/12 - 12.1 - Cell Format Updates
				case BIFF8RecordType.SUB:					break;
				case BIFF8RecordType.SUPBOOK:				return new SUPBOOKRecord();
				case BIFF8RecordType.SXADDL:				break;
				case BIFF8RecordType.SXDB:					break;
				case BIFF8RecordType.SXDBEX:				break;
				case BIFF8RecordType.SXDI:					break;
				case BIFF8RecordType.SXDXF:					break;
				case BIFF8RecordType.SXEX:					break;
				case BIFF8RecordType.SXFDBTYPE:				break;
				case BIFF8RecordType.SXFILT:				break;
				case BIFF8RecordType.SXFMLA:				break;
				case BIFF8RecordType.SXFORMAT:				break;
				case BIFF8RecordType.SXFORMULA:				break;
				case BIFF8RecordType.SXIDSTM:				break;
				case BIFF8RecordType.SXITM:					break;
				case BIFF8RecordType.SXIVD:					break;
				case BIFF8RecordType.SXLI:					break;
				case BIFF8RecordType.SXNAME:				break;
				case BIFF8RecordType.SXPAIR:				break;
				case BIFF8RecordType.SXPI:					break;
				case BIFF8RecordType.SXPIEX:				break;
				case BIFF8RecordType.SXRULE:				break;
				case BIFF8RecordType.SXSELECT:				break;
				case BIFF8RecordType.SXSTRING:				break;
				case BIFF8RecordType.SXTBL:					break;
				case BIFF8RecordType.SXTBPG:				break;
				case BIFF8RecordType.SXTBRGIITM:			break;
				case BIFF8RecordType.SXTH:					break;
				case BIFF8RecordType.SXVD:					break;
				case BIFF8RecordType.SXVDEX:				break;
				case BIFF8RecordType.SXVDTEX:				break;
				case BIFF8RecordType.SXVI:					break;
				case BIFF8RecordType.SXVIEW:				break;
				case BIFF8RecordType.SXVIEWEX:				break;
				case BIFF8RecordType.SXVIEWEX9:				break;
				case BIFF8RecordType.SXVS:					break;
				case BIFF8RecordType.TABID:					return new TABIDRecord();
				case BIFF8RecordType.TABIDCONF:				break;
				case BIFF8RecordType.TABLE:					return new TABLERecord();
				case BIFF8RecordType.TABLESTYLE:			return new TABLESTYLERecord();					// MD 2/22/12 - 12.1 - Table Support
				case BIFF8RecordType.TABLESTYLEELEMENT:		return new TABLESTYLEELEMENTRecord();			// MD 2/22/12 - 12.1 - Table Support
				case BIFF8RecordType.TABLESTYLES:			return new TABLESTYLESRecord();					// MD 2/19/12 - 12.1 - Table Support
				case BIFF8RecordType.TEMPLATE:				return new TEMPLATERecord();					// MD 5/7/10 - 10.2 - Excel Templates
				case BIFF8RecordType.THEME:					return new THEMERecord();						// MD 11/29/11 - TFS96205
				case BIFF8RecordType.TOPMARGIN:				return new TOPMARGINRecord();
				case BIFF8RecordType.TXO:					return new TXORecord();
				case BIFF8RecordType.TXTQUERY:				break;
				case BIFF8RecordType.UDDESC:				break;
				case BIFF8RecordType.UNCALCED:				break;
				case BIFF8RecordType.USERBVIEW:				return new USERBVIEWRecord();
				case BIFF8RecordType.USERSVIEWBEGIN:		return new USERSVIEWBEGINRecord();
				case BIFF8RecordType.USERSVIEWEND:			return new USERSVIEWENDRecord();
				case BIFF8RecordType.USESELFS:				return new USESELFSRecord();
				case BIFF8RecordType.VBAOBJECTNAME:			return new VBAOBJECTNAMERecord();				// MD 10/1/08 - TFS8453
				case BIFF8RecordType.VCENTER:				return new VCENTERRecord();
				case BIFF8RecordType.VERTICALPAGEBREAKS:	return new VERTICALPAGEBREAKSRecord();			// MD 2/1/11 - Data Validation support
				case BIFF8RecordType.WEBPUB:				break;
				case BIFF8RecordType.WINDOW1:				return new WINDOW1Record();
				case BIFF8RecordType.WINDOW2:				return new WINDOW2Record();
				case BIFF8RecordType.WINDOWPROTECT:			return new WINDOWPROTECTRecord();
				case BIFF8RecordType.WOPT:					break;
				case BIFF8RecordType.WRITEACCESS:			return new WRITEACCESSRecord();
				case BIFF8RecordType.WRITEPROT:				break;
				case BIFF8RecordType.WSBOOL:				return new WSBOOLRecord();
				case BIFF8RecordType.XCT:					return new XCTRecord();
				case BIFF8RecordType.XF:					return new XFRecord();
				case BIFF8RecordType.XFCRC:					return new XFCRCRecord();						// MD 11/29/11 - TFS96205
				case BIFF8RecordType.XFEXT:					return new XFEXTRecord();						// MD 11/29/11 - TFS96205
				case BIFF8RecordType.XL5MODIFY:				break;

				//// Obsolete record types in BIFF8
				case BIFF8RecordType.RSTRING:				break;
		    }

			Debug.Assert( Enum.IsDefined( typeof( BIFF8RecordType ), type ) == false, "Unhandled record type: " + type );
			return null;
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