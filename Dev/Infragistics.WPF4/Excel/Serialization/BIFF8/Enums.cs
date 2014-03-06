using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.BIFF8
{
	internal enum BIFF8RecordType : ushort
	{
		Default = 0x0000,

		Record1904 = 0x0022,
		ADDIN = 0x0087,
		ADDMENU = 0x00C2,
		AREAFORMAT = 0x100A,
		ARRAY = 0x0221,
		AUTOFILTER = 0x009E,
		AUTOFILTER12 = 0x087E,				// MD 12/7/11 - 12.1 - Table Support
		AUTOFILTERINFO = 0x009D,
		AXCEXT = 0x1062,
		AXESUSED = 0x1046,
		AXIS = 0x101D,
		AXISLINE = 0x1021,
		AXISPARENT = 0x1041,
		BACKUP = 0x0040,
		BEGIN = 0x1033,
		BITMAP = 0x00E9,
		BLANK = 0x0201,
		BOF = 0x0809,
		BOOKBOOL = 0x00DA,
		BOOKEXT = 0x0863,
		BOOLERR = 0x0205,
		BOTTOMMARGIN = 0x0029,
		BOUNDSHEET = 0x0085,
		BRAI = 0x1051,
		CALCCOUNT = 0x000C,
		CALCMODE = 0x000D,
		CATLAB = 0x0856,
		CATSERRANGE = 0x1020,
		CELLWATCH = 0x086C,
		CF = 0x01B1,
		CHART = 0x1002,
		CHART3DBarSHAPE = 0x105F,
		CHARTFORMAT = 0x1014,
		CHARTFRTINFO = 0x0850,
		CODEPAGE = 0x0042,
		COLINFO = 0x007D,
		CONDFMT = 0x01B0,
		CONTINUE = 0x003C,
		CONTINUEFRT = 0x0812,
		CONTINUEFRT11 = 0x0875,
		CONTINUEFRT12 = 0x087F,				// MD 11/29/11 - TFS96205
		COORDLIST = 0x00A9,
		COUNTRY = 0x008C,
		CRASHRECERR = 0x0865,
		CRN = 0x005A,
		CRTCOOPT = 0x08CB,
		CRTLINK = 0x1022,
		DATAFORMAT = 0x1006,
		DATALABEXT = 0x086A,
		DATALABEXTCONTENTS = 0x086B,
		DBCELL = 0x00D7,
		DBQUERYEXT = 0x0803,
		DCON = 0x0050,
		DCONBIN = 0x01B5,
		DCONNAME = 0x0052,
		DCONREF = 0x0051,
		DEFAULTROWHEIGHT = 0x0225,
		DEFAULTTEXT = 0x1024,
		DEFCOLWIDTH = 0x0055,
		DELMENU = 0x00C3,
		DELTA = 0x0010,
		DIMENSIONS = 0x0200,
		DOCROUTE = 0x00B8,
		DROPDOWNOBJIDS = 0x0874,
		DSF = 0x161,
		DV = 0x01BE,
		DVAL = 0x01B2,
		DXF = 0x088D,
		EDG = 0x0088,
		EFONT = 0x0045,
		END = 0x1034,
		ENDBLOCK = 0x0853,
		EOF = 0x000A,
		EXCEL9FILE = 0x01C0,
		EXTERNCOUNT = 0x0016,
		EXTERNNAME = 0x0023,
		EXTERNSHEET = 0x0017,
		EXTSST = 0x00FF,
		EXTSTRING = 0x0804,
		//FEAT = 0x0868,
		FEAT11 = 0x0872,					// MD 12/7/11 - 12.1 - Table Support
		FEAT12 = 0x0878,					// MD 2/20/12 - 12.1 - Table Support
		FEATHEADR = 0x0867,					// MD 1/26/12 - 12.1 - Cell Format Updates
		FEATHEADR11 = 0x0871,				// MD 12/7/11 - 12.1 - Table Support
		//FEATINFO = 0x086D,
		//FEATINFO11 = 0x0873,
		FILEPASS = 0x002F,
		FILESHARING = 0x005B,
		FILESHARING2 = 0x0125,
		FILTERMODE = 0x009B,
		FMQRY = 0x08C6,
		FMSQRY = 0x08C7,
		FNGROUPCOUNT = 0x009C,
		FNGROUPNAME = 0x009A,
		FONT = 0x0031,
		FONTX = 0x1026,
		FOOTER = 0x0015,
		FORMAT = 0x041E,
		FORMULA = 0x0006,
		FRAME = 0x1032,
		GCW = 0x00AB,
		GRIDSET = 0x0082,
		GUTS = 0x0080,
		HCENTER = 0x0083,
		HEADER = 0x0014,
		HFPicture = 0x0866,
		HIDEOBJ = 0x008D,
		HLINK = 0x01B8,
		HLINKTOOLTIP = 0x0800,
		HORIZONTALPAGEBREAKS = 0x001B,
		IMDATA = 0x007F,
		INDEX = 0x020B,
		INTERFACEEND = 0x00E2,
		INTERFACEHDR = 0x00E1,
		ITERATION = 0x0011,
		LABEL = 0x0204,
		LABELRANGES = 0x015F,
		LABELSST = 0x00FD,
		LEFTMARGIN = 0x0026,
		LEGEND = 0x1015,
		LHNGRAPH = 0x0095,
		LHRECORD = 0x0094,
		LINE = 0x1018,
		LINEFORMAT = 0x1007,
		LIST12 = 0x0877,					// MD 12/7/11 - 12.1 - Table Support
		LISTCF = 0x08C5,
		LISTCONDFMT = 0x08C4,
		LISTDV = 0x08C3,
		LISTFIELD = 0x08C2,
		LISTOBJ = 0x08C1,
		LNEXT = 0x08C9,
		LPR = 0x0098,
		MARKERFORMAT = 0x1009,
		MERGEDCELLS = 0x00E5,
		MKREXT = 0x08CA,
		MMS = 0x00C1,
		MSODRAWING = 0x00EC,
		MSODRAWINGGROUP = 0x00EB,
		MSODRAWINGSELECTION = 0x00ED,
		MULBLANK = 0x00BE,
		MULRK = 0x00BD,
		NAME = 0x0018,
		NAMEEXT = 0x0894,
		NOTE = 0x001C,
		NUMBER = 0x0203,
		OBJ = 0x005D,
		OBJECTLINK = 0x1027,
		OBJPROTECT = 0x0063,
		OBPROJ = 0x00D3,
		OLEDBCONN = 0x080A,
		OLESIZE = 0x00DE,
		PAGELAYOUTINFO = 0x088B,
		PALETTE = 0x0092,
		PANE = 0x0041,
		PARAMQRY = 0x00DC,
		PASSWORD = 0x0013,
		PIEFORMAT = 0x100B,
		PLOTAREA = 0x1035,
		PLOTGROWTH = 0x1064,
		PLS = 0x004D,
		PLV = 0x08C8,
		POS = 0x104F,
		PRECISION = 0x000E,
		PRINTGRIDLINES = 0x002B,
		PRINTHEADERS = 0x002A,
		PROTECT = 0x0012,
		PROT4REV = 0x01AF,
		PROT4REVPASS = 0x01BC,
		PUB = 0x089,
		QSI = 0x01AD,
		QSIF = 0x0807,
		QSIR = 0x0806,
		QSISXTAG = 0x0802,
		REALTIMEDATA = 0x0813,
		RECALCID = 0x01C1,
		RECIPNAME = 0x00B9,
		REFMODE = 0x000F,
		REFRESHALL = 0x01B7,
		RIGHTMARGIN = 0x0027,
		RK = 0x027E,
		ROW = 0x0208,
		RSTRING = 0x00D6,
		SAVERECALC = 0x005F,
		SCENARIO = 0x00AF,
		SCENMAN = 0x00AE,
		SCENPROTECT = 0x00DD,
		SCL = 0x00A0,
		SELECTION = 0x001D,
		SERIES = 0x1003,
		SERIESTEXT = 0x100D,
		SERTOCRT = 0x1045,
		SETUP = 0x00A1,
		SHEETEXT = 0x0862,
		SHRFMLA = 0x04BC,
		SHTPROPS = 0x1044,
		SORT = 0x0090,
		SORTDATA12 = 0x0895,					// MD 12/7/11 - 12.1 - Table Support
		SOUND = 0x0096,
		SST = 0x00FC,
		STANDARDWIDTH = 0x0099,
		STARTBLOCK = 0x0852,
		STRING = 0x0207,
		STYLE = 0x0293,
		STYLEEXT = 0x0892,					// MD 1/26/12 - 12.1 - Cell Format Updates
		SUB = 0x0091,
		SUPBOOK = 0x01AE,
		SXADDL = 0x0864,
		SXDB = 0x00C6,
		SXDBEX = 0x0122,
		SXDI = 0x00C5,
		SXDXF = 0x00F4,
		SXEX = 0x00F1,
		SXEXT = 0x00DC,
		SXFDBTYPE = 0x01BB,
		SXFILT = 0x00F2,
		SXFMLA = 0x00F9,
		SXFORMAT = 0x00FB,
		SXFORMULA = 0x0103,
		SXIDSTM = 0x00D5,
		SXITM = 0x00F5,
		SXIVD = 0x00B4,
		SXLI = 0x00B5,
		SXNAME = 0x00F6,
		SXPAIR = 0x00F8,
		SXPI = 0x00B6,
		SXPIEX = 0x080E,
		SXRULE = 0x00F0,
		SXSELECT = 0x00F7,
		SXSTRING = 0x00CD,
		SXTBL = 0x00D0,
		SXTBPG = 0x00D2,
		SXTBRGIITM = 0x00D1,
		SXTH = 0x080D,
		SXVD = 0x00B1,
		SXVDEX = 0x0100,
		SXVDTEX = 0x080F,
		SXVI = 0x00B2,
		SXVIEW = 0x00B0,
		SXVIEWEX = 0x080C,
		SXVIEWEX9 = 0x0810,
		SXVS = 0x00E3,
		TABID = 0x013D,
		TABIDCONF = 0x00EA,
		TABLE = 0x0236,
		TABLESTYLE = 0x088F,				// MD 2/22/12 - 12.1 - Table Support
		TABLESTYLEELEMENT = 0x0890,			// MD 2/22/12 - 12.1 - Table Support
		TABLESTYLES = 0x088E,				// MD 2/19/12 - 12.1 - Table Support
		TEMPLATE = 0x0060,
		TEXT = 0x1025,
		THEME = 0x0896,						// MD 11/29/11 - TFS96205
		TICK = 0x101E,
		TOPMARGIN = 0x0028,
		TXO = 0x01B6,
		TXTQUERY = 0x0805,
		UDDESC = 0x00DF,
		UNCALCED = 0x005E,
		UNITS = 0x1001,
		USERBVIEW = 0x01A9,
		USERSVIEWBEGIN = 0x01AA,
		USERSVIEWEND = 0x01AB,
		USESELFS = 0x0160,
		VALUERANGE = 0x101F,
		VBAOBJECTNAME = 0x01BA,				// MD 10/1/08 - TFS8453 (This isn't the real record name, I just made it up)
		VCENTER = 0x0084,
		VERTICALPAGEBREAKS = 0x001A,
		WEBPUB = 0x0801,
		WINDOW1 = 0x003D,
		WINDOW2 = 0x023E,
		WINDOWPROTECT = 0x0019,
		WOPT = 0x080B,
		WRITEACCESS = 0x005C,
		WRITEPROT = 0x0086,
		WSBOOL = 0x0081,
		XCT = 0x0059,
		XF = 0x00E0,
		XFCRC = 0x087C,						// MD 11/29/11 - TFS96205
		XFEXT = 0x087D,						// MD 11/29/11 - TFS96205
		XL5MODIFY = 0x0162,
	}

	// MD 1/16/12 - 12.1 - Cell Format Updates
	// This is no longer needed.
	//// MD 11/29/11 - TFS96205
	//// http://msdn.microsoft.com/en-us/library/dd925394(v=office.12).aspx
	//internal enum ColorTheme : uint
	//{
	//    Dark1 = 0x00000000,
	//    Light1 = 0x00000001,
	//    Dark2 = 0x00000002,
	//    Light2 = 0x00000003,
	//    Accent1 = 0x00000004,
	//    Accent2 = 0x00000005,
	//    Accent3 = 0x00000006,
	//    Accent4 = 0x00000007,
	//    Accent5 = 0x00000008,
	//    Accent6 = 0x00000009,
	//    Hyperlink = 0x0000000A,
	//    FollowedHyperlink = 0x0000000B,
	//}

	// MD 11/29/11 - TFS96205
	// http://msdn.microsoft.com/en-us/library/dd952422(v=office.12).aspx
	internal enum FontScheme : byte
	{
		None = 0x00,
		Major = 0x01,
		Minor = 0x02,
		Nil = 0xFF,
	}

	// MD 2/18/12 - 12.1 - Table Support
	// http://msdn.microsoft.com/en-us/library/dd906804(v=office.12).aspx
	internal enum SharedFeatureType : ushort
	{
		Protection = 0x0002,
		Fec2 = 0x0003,
		Factoid = 0x0004,
		List = 0x0005,
	}

	// MD 2/18/12 - 12.1 - Table Support
	// http://msdn.microsoft.com/en-us/library/dd951980(v=office.12).aspx
	internal enum SourceType : uint
	{
		Range = 0x00000000,
		Sharepoint = 0x00000001,
		XML = 0x00000002,
		ExternalData = 0x00000003,
	}

	// MD 11/29/11 - TFS96205
	// http://msdn.microsoft.com/en-us/library/dd947047(v=office.12).aspx
	internal enum XColorType : ushort
	{
		Auto = 0x0000,
		Indexed = 0x0001,
		RGB = 0x0002,
		Themed = 0x0003,
		NotSet = 0x0004,
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