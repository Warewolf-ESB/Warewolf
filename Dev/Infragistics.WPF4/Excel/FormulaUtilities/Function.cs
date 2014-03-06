using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities
{



	internal class Function : 
		// MD 10/8/07 - BR27172
		// Implemented IComparable so we can sort the functions
		IComparable<Function>
	{
		#region Constants

		private const int AddInFunctionID = 255;

		// MD 8/29/11 - TFS85072
		// This id will actually map to the AddInFunctionID, but it should not be written out like a normal add-in function when it is 
		// saved in Excel 2007.
		private const int Excel2007OnlyID = -1;

		// MD 10/9/07 - BR27172
		// Defined constants to prevent literals from being used multiple times
		private const int COUNTFunctionID = 0;
		private const int IFFunctionID = 1;
		private const int ISNAFunctionID = 2;
		private const int ISERRORFunctionID = 3;
		private const int SUMFunctionID = 4;
		private const int AVERAGEFunctionID = 5;
		private const int MINFunctionID = 6;
		private const int MAXFunctionID = 7;
		private const int ROWFunctionID = 8;
		private const int COLUMNFunctionID = 9;
		private const int NAFunctionID = 10;
		private const int NPVFunctionID = 11;
		private const int STDEVFunctionID = 12;
		private const int DOLLARFunctionID = 13;
		private const int FIXEDFunctionID = 14;
		private const int SINFunctionID = 15;
		private const int COSFunctionID = 16;
		private const int TANFunctionID = 17;
		private const int ATANFunctionID = 18;
		private const int PIFunctionID = 19;
		private const int SQRTFunctionID = 20;
		private const int EXPFunctionID = 21;
		private const int LNFunctionID = 22;
		private const int LOG10FunctionID = 23;
		private const int ABSFunctionID = 24;
		private const int INTFunctionID = 25;
		private const int SIGNFunctionID = 26;
		private const int ROUNDFunctionID = 27;
		private const int LOOKUPFunctionID = 28;
		private const int INDEXFunctionID = 29;
		private const int REPTFunctionID = 30;
		private const int MIDFunctionID = 31;
		private const int LENFunctionID = 32;
		private const int VALUEFunctionID = 33;
		private const int TRUEFunctionID = 34;
		private const int FALSEFunctionID = 35;
		private const int ANDFunctionID = 36;
		private const int ORFunctionID = 37;
		private const int NOTFunctionID = 38;
		private const int MODFunctionID = 39;
		private const int DCOUNTFunctionID = 40;
		private const int DSUMFunctionID = 41;
		private const int DAVERAGEFunctionID = 42;
		private const int DMINFunctionID = 43;
		private const int DMAXFunctionID = 44;
		private const int DSTDEVFunctionID = 45;
		private const int VARFunctionID = 46;
		private const int DVARFunctionID = 47;
		private const int TEXTFunctionID = 48;
		private const int LINESTFunctionID = 49;
		private const int TRENDFunctionID = 50;
		private const int LOGESTFunctionID = 51;
		private const int GROWTHFunctionID = 52;
		private const int PVFunctionID = 56;
		private const int FVFunctionID = 57;
		private const int NPERFunctionID = 58;
		private const int PMTFunctionID = 59;
		private const int RATEFunctionID = 60;
		private const int MIRRFunctionID = 61;
		private const int IRRFunctionID = 62;
		private const int RANDFunctionID = 63;
		private const int MATCHFunctionID = 64;
		private const int DATEFunctionID = 65;
		private const int TIMEFunctionID = 66;
		private const int DAYFunctionID = 67;
		private const int MONTHFunctionID = 68;
		private const int YEARFunctionID = 69;
		private const int WEEKDAYFunctionID = 70;
		private const int HOURFunctionID = 71;
		private const int MINUTEFunctionID = 72;
		private const int SECONDFunctionID = 73;
		private const int NOWFunctionID = 74;
		private const int AREASFunctionID = 75;
		private const int ROWSFunctionID = 76;
		private const int COLUMNSFunctionID = 77;
		private const int OFFSETFunctionID = 78;
		private const int SEARCHFunctionID = 82;
		private const int TRANSPOSEFunctionID = 83;
		private const int TYPEFunctionID = 86;
		private const int ATAN2FunctionID = 97;
		private const int ASINFunctionID = 98;
		private const int ACOSFunctionID = 99;
		private const int CHOOSEFunctionID = 100;
		private const int HLOOKUPFunctionID = 101;
		private const int VLOOKUPFunctionID = 102;
		private const int ISREFFunctionID = 105;
		private const int LOGFunctionID = 109;
		private const int CHARFunctionID = 111;
		private const int LOWERFunctionID = 112;
		private const int UPPERFunctionID = 113;
		private const int PROPERFunctionID = 114;
		private const int LEFTFunctionID = 115;
		private const int RIGHTFunctionID = 116;
		private const int EXACTFunctionID = 117;
		private const int TRIMFunctionID = 118;
		private const int REPLACEFunctionID = 119;
		private const int SUBSTITUTEFunctionID = 120;
		private const int CODEFunctionID = 121;
		private const int FINDFunctionID = 124;
		private const int CELLFunctionID = 125;
		private const int ISERRFunctionID = 126;
		private const int ISTEXTFunctionID = 127;
		private const int ISNUMBERFunctionID = 128;
		private const int ISBLANKFunctionID = 129;
		private const int TFunctionID = 130;
		private const int NFunctionID = 131;
		private const int DATEVALUEFunctionID = 140;
		private const int TIMEVALUEFunctionID = 141;
		private const int SLNFunctionID = 142;
		private const int SYDFunctionID = 143;
		private const int DDBFunctionID = 144;
		private const int INDIRECTFunctionID = 148;
		private const int CLEANFunctionID = 162;
		private const int MDETERMFunctionID = 163;
		private const int MINVERSEFunctionID = 164;
		private const int MMULTFunctionID = 165;
		private const int IPMTFunctionID = 167;
		private const int PPMTFunctionID = 168;
		private const int COUNTAFunctionID = 169;
		private const int PRODUCTFunctionID = 183;
		private const int FACTFunctionID = 184;
		private const int DPRODUCTFunctionID = 189;
		private const int ISNONTEXTFunctionID = 190;
		private const int STDEVPFunctionID = 193;
		private const int VARPFunctionID = 194;
		private const int DSTDEVPFunctionID = 195;
		private const int DVARPFunctionID = 196;
		private const int TRUNCFunctionID = 197;
		private const int ISLOGICALFunctionID = 198;
		private const int DCOUNTAFunctionID = 199;
		private const int USDOLLARFunctionID = 204;
		private const int FINDBFunctionID = 205;
		private const int SEARCHBFunctionID = 206;
		private const int REPLACEBFunctionID = 207;
		private const int LEFTBFunctionID = 208;
		private const int RIGHTBFunctionID = 209;
		private const int MIDBFunctionID = 210;
		private const int LENBFunctionID = 211;
		private const int ROUNDUPFunctionID = 212;
		private const int ROUNDDOWNFunctionID = 213;
		private const int ASCFunctionID = 214;
		private const int DBSCFunctionID = 215;
		private const int RANKFunctionID = 216;
		private const int ADDRESSFunctionID = 219;
		private const int DAYS360FunctionID = 220;
		private const int TODAYFunctionID = 221;
		private const int VDBFunctionID = 222;
		private const int MEDIANFunctionID = 227;
		private const int SUMPRODUCTFunctionID = 228;
		private const int SINHFunctionID = 229;
		private const int COSHFunctionID = 230;
		private const int TANHFunctionID = 231;
		private const int ASINHFunctionID = 232;
		private const int ACOSHFunctionID = 233;
		private const int ATANHFunctionID = 234;
		private const int DGETFunctionID = 235;
		private const int INFOFunctionID = 244;
		private const int DBFunctionID = 247;
		private const int FREQUENCYFunctionID = 252;
		private const int ERROR_TYPEFunctionID = 261;
		private const int AVEDEVFunctionID = 269;
		private const int BETADISTFunctionID = 270;
		private const int GAMMALNFunctionID = 271;
		private const int BETAINVFunctionID = 272;
		private const int BINOMDISTFunctionID = 273;
		private const int CHIDISTFunctionID = 274;
		private const int CHIINVFunctionID = 275;
		private const int COMBINFunctionID = 276;
		private const int CONFIDENCEFunctionID = 277;
		private const int CRITBINOMFunctionID = 278;
		private const int EVENFunctionID = 279;
		private const int EXPONDISTFunctionID = 280;
		private const int FDISTFunctionID = 281;
		private const int FINVFunctionID = 282;
		private const int FISHERFunctionID = 283;
		private const int FISHERINVFunctionID = 284;
		private const int FLOORFunctionID = 285;
		private const int GAMMADISTFunctionID = 286;
		private const int GAMMAINVFunctionID = 287;
		private const int CEILINGFunctionID = 288;
		private const int HYPGEOMVERTFunctionID = 289;
		private const int LOGNORMDISTFunctionID = 290;
		private const int LOGINVFunctionID = 291;
		private const int NEGBINOMDISTFunctionID = 292;
		private const int NORMDISTFunctionID = 293;
		private const int NORMSDISTFunctionID = 294;
		private const int NORMINVFunctionID = 295;
		private const int MNORMSINVFunctionID = 296;
		private const int STANDARDIZEFunctionID = 297;
		private const int ODDFunctionID = 298;
		private const int PERMUTFunctionID = 299;
		private const int POISSONFunctionID = 300;
		private const int TDISTFunctionID = 301;
		private const int WEIBULLFunctionID = 302;
		private const int SUMXMY2FunctionID = 303;
		private const int SUMX2MY2FunctionID = 304;
		private const int SUMX2PY2FunctionID = 305;
		private const int CHITESTFunctionID = 306;
		private const int CORRELFunctionID = 307;
		private const int COVARFunctionID = 308;
		private const int FORECASTFunctionID = 309;
		private const int FTESTFunctionID = 310;
		private const int INTERCEPTFunctionID = 311;
		private const int PEARSONFunctionID = 312;
		private const int RSQFunctionID = 313;
		private const int STEYXFunctionID = 314;
		private const int SLOPEFunctionID = 315;
		private const int TTESTFunctionID = 316;
		private const int PROBFunctionID = 317;
		private const int DEVSQFunctionID = 318;
		private const int GEOMEANFunctionID = 319;
		private const int HARMEANFunctionID = 320;
		private const int SUMSQFunctionID = 321;
		private const int KURTFunctionID = 322;
		private const int SKEWFunctionID = 323;
		private const int ZTESTFunctionID = 324;
		private const int LARGEFunctionID = 325;
		private const int SMALLFunctionID = 326;
		private const int QUARTILEFunctionID = 327;
		private const int PERCENTILEFunctionID = 328;
		private const int PERCENTRANKFunctionID = 329;
		private const int MODEFunctionID = 330;
		private const int TRIMMEANFunctionID = 331;
		private const int TINVFunctionID = 332;
		private const int CONCATENATEFunctionID = 336;
		private const int POWERFunctionID = 337;
		private const int RADIANSFunctionID = 342;
		private const int DEGREESFunctionID = 343;
		private const int SUBTOTALFunctionID = 344;
		private const int SUMIFFunctionID = 345;
		private const int COUNTIFFunctionID = 346;
		private const int COUNTBLANKFunctionID = 347;
		private const int ISPMTFunctionID = 350;
		private const int DATEDIFFunctionID = 351;
		private const int DATESTRINGFunctionID = 352;
		private const int NUMBERSTRINGFunctionID = 353;
		private const int ROMANFunctionID = 354;
		private const int GETPIVOTDATAFunctionID = 358;
		private const int HYPERLINKFunctionID = 359;
		private const int PHONETICFunctionID = 360;
		private const int AVERAGEAFunctionID = 361;
		private const int MAXAFunctionID = 362;
		private const int MINAFunctionID = 363;
		private const int STDEVPAFunctionID = 364;
		private const int VARPAFunctionID = 365;
		private const int STDEVAFunctionID = 366;
		private const int VARAFunctionID = 367;

		private const string COUNTFunctionName = "COUNT";
		private const string IFFunctionName = "IF";
		private const string ISNAFunctionName = "ISNA";
		private const string ISERRORFunctionName = "ISERROR";
		private const string SUMFunctionName = "SUM";
		private const string AVERAGEFunctionName = "AVERAGE";
		private const string MINFunctionName = "MIN";
		private const string MAXFunctionName = "MAX";
		private const string ROWFunctionName = "ROW";
		private const string COLUMNFunctionName = "COLUMN";
		private const string NAFunctionName = "NA";
		private const string NPVFunctionName = "NPV";
		private const string STDEVFunctionName = "STDEV";
		private const string DOLLARFunctionName = "DOLLAR";
		private const string FIXEDFunctionName = "FIXED";
		private const string SINFunctionName = "SIN";
		private const string COSFunctionName = "COS";
		private const string TANFunctionName = "TAN";
		private const string ATANFunctionName = "ATAN";
		private const string PIFunctionName = "PI";
		private const string SQRTFunctionName = "SQRT";
		private const string EXPFunctionName = "EXP";
		private const string LNFunctionName = "LN";
		private const string LOG10FunctionName = "LOG10";
		private const string ABSFunctionName = "ABS";
		private const string INTFunctionName = "INT";
		private const string SIGNFunctionName = "SIGN";
		private const string ROUNDFunctionName = "ROUND";
		private const string LOOKUPFunctionName = "LOOKUP";
		private const string INDEXFunctionName = "INDEX";
		private const string REPTFunctionName = "REPT";
		private const string MIDFunctionName = "MID";
		private const string LENFunctionName = "LEN";
		private const string VALUEFunctionName = "VALUE";
		private const string TRUEFunctionName = "TRUE";
		private const string FALSEFunctionName = "FALSE";
		private const string ANDFunctionName = "AND";
		private const string ORFunctionName = "OR";
		private const string NOTFunctionName = "NOT";
		private const string MODFunctionName = "MOD";
		private const string DCOUNTFunctionName = "DCOUNT";
		private const string DSUMFunctionName = "DSUM";
		private const string DAVERAGEFunctionName = "DAVERAGE";
		private const string DMINFunctionName = "DMIN";
		private const string DMAXFunctionName = "DMAX";
		private const string DSTDEVFunctionName = "DSTDEV";
		private const string VARFunctionName = "VAR";
		private const string DVARFunctionName = "DVAR";
		private const string TEXTFunctionName = "TEXT";
		private const string LINESTFunctionName = "LINEST";
		private const string TRENDFunctionName = "TREND";
		private const string LOGESTFunctionName = "LOGEST";
		private const string GROWTHFunctionName = "GROWTH";
		private const string PVFunctionName = "PV";
		private const string FVFunctionName = "FV";
		private const string NPERFunctionName = "NPER";
		private const string PMTFunctionName = "PMT";
		private const string RATEFunctionName = "RATE";
		private const string MIRRFunctionName = "MIRR";
		private const string IRRFunctionName = "IRR";
		private const string RANDFunctionName = "RAND";
		private const string MATCHFunctionName = "MATCH";
		private const string DATEFunctionName = "DATE";
		private const string TIMEFunctionName = "TIME";
		private const string DAYFunctionName = "DAY";
		private const string MONTHFunctionName = "MONTH";
		private const string YEARFunctionName = "YEAR";
		private const string WEEKDAYFunctionName = "WEEKDAY";
		private const string HOURFunctionName = "HOUR";
		private const string MINUTEFunctionName = "MINUTE";
		private const string SECONDFunctionName = "SECOND";
		private const string NOWFunctionName = "NOW";
		private const string AREASFunctionName = "AREAS";
		private const string ROWSFunctionName = "ROWS";
		private const string COLUMNSFunctionName = "COLUMNS";
		private const string OFFSETFunctionName = "OFFSET";
		private const string SEARCHFunctionName = "SEARCH";
		private const string TRANSPOSEFunctionName = "TRANSPOSE";
		private const string TYPEFunctionName = "TYPE";
		private const string ATAN2FunctionName = "ATAN2";
		private const string ASINFunctionName = "ASIN";
		private const string ACOSFunctionName = "ACOS";
		private const string CHOOSEFunctionName = "CHOOSE";
		private const string HLOOKUPFunctionName = "HLOOKUP";
		private const string VLOOKUPFunctionName = "VLOOKUP";
		private const string ISREFFunctionName = "ISREF";
		private const string LOGFunctionName = "LOG";
		private const string CHARFunctionName = "CHAR";
		private const string LOWERFunctionName = "LOWER";
		private const string UPPERFunctionName = "UPPER";
		private const string PROPERFunctionName = "PROPER";
		private const string LEFTFunctionName = "LEFT";
		private const string RIGHTFunctionName = "RIGHT";
		private const string EXACTFunctionName = "EXACT";
		private const string TRIMFunctionName = "TRIM";
		private const string REPLACEFunctionName = "REPLACE";
		private const string SUBSTITUTEFunctionName = "SUBSTITUTE";
		private const string CODEFunctionName = "CODE";
		private const string FINDFunctionName = "FIND";
		private const string CELLFunctionName = "CELL";
		private const string ISERRFunctionName = "ISERR";
		private const string ISTEXTFunctionName = "ISTEXT";
		private const string ISNUMBERFunctionName = "ISNUMBER";
		private const string ISBLANKFunctionName = "ISBLANK";
		private const string TFunctionName = "T";
		private const string NFunctionName = "N";
		private const string DATEVALUEFunctionName = "DATEVALUE";
		private const string TIMEVALUEFunctionName = "TIMEVALUE";
		private const string SLNFunctionName = "SLN";
		private const string SYDFunctionName = "SYD";
		private const string DDBFunctionName = "DDB";
		private const string INDIRECTFunctionName = "INDIRECT";
		private const string CLEANFunctionName = "CLEAN";
		private const string MDETERMFunctionName = "MDETERM";
		private const string MINVERSEFunctionName = "MINVERSE";
		private const string MMULTFunctionName = "MMULT";
		private const string IPMTFunctionName = "IPMT";
		private const string PPMTFunctionName = "PPMT";
		private const string COUNTAFunctionName = "COUNTA";
		private const string PRODUCTFunctionName = "PRODUCT";
		private const string FACTFunctionName = "FACT";
		private const string DPRODUCTFunctionName = "DPRODUCT";
		private const string ISNONTEXTFunctionName = "ISNONTEXT";
		private const string STDEVPFunctionName = "STDEVP";
		private const string VARPFunctionName = "VARP";
		private const string DSTDEVPFunctionName = "DSTDEVP";
		private const string DVARPFunctionName = "DVARP";
		private const string TRUNCFunctionName = "TRUNC";
		private const string ISLOGICALFunctionName = "ISLOGICAL";
		private const string DCOUNTAFunctionName = "DCOUNTA";
		private const string USDOLLARFunctionName = "USDOLLAR";
		private const string FINDBFunctionName = "FINDB";
		private const string SEARCHBFunctionName = "SEARCHB";
		private const string REPLACEBFunctionName = "REPLACEB";
		private const string LEFTBFunctionName = "LEFTB";
		private const string RIGHTBFunctionName = "RIGHTB";
		private const string MIDBFunctionName = "MIDB";
		private const string LENBFunctionName = "LENB";
		private const string ROUNDUPFunctionName = "ROUNDUP";
		private const string ROUNDDOWNFunctionName = "ROUNDDOWN";
		private const string ASCFunctionName = "ASC";
		private const string DBSCFunctionName = "DBSC";
		private const string RANKFunctionName = "RANK";
		private const string ADDRESSFunctionName = "ADDRESS";
		private const string DAYS360FunctionName = "DAYS360";
		private const string TODAYFunctionName = "TODAY";
		private const string VDBFunctionName = "VDB";
		private const string MEDIANFunctionName = "MEDIAN";
		private const string SUMPRODUCTFunctionName = "SUMPRODUCT";
		private const string SINHFunctionName = "SINH";
		private const string COSHFunctionName = "COSH";
		private const string TANHFunctionName = "TANH";
		private const string ASINHFunctionName = "ASINH";
		private const string ACOSHFunctionName = "ACOSH";
		private const string ATANHFunctionName = "ATANH";
		private const string DGETFunctionName = "DGET";
		private const string INFOFunctionName = "INFO";
		private const string DBFunctionName = "DB";
		private const string FREQUENCYFunctionName = "FREQUENCY";
		private const string ERROR_TYPEFunctionName = "ERROR.TYPE";
		private const string AVEDEVFunctionName = "AVEDEV";
		private const string BETADISTFunctionName = "BETADIST";
		private const string GAMMALNFunctionName = "GAMMALN";
		private const string BETAINVFunctionName = "BETAINV";
		private const string BINOMDISTFunctionName = "BINOMDIST";
		private const string CHIDISTFunctionName = "CHIDIST";
		private const string CHIINVFunctionName = "CHIINV";
		private const string COMBINFunctionName = "COMBIN";
		private const string CONFIDENCEFunctionName = "CONFIDENCE";
		private const string CRITBINOMFunctionName = "CRITBINOM";
		private const string EVENFunctionName = "EVEN";
		private const string EXPONDISTFunctionName = "EXPONDIST";
		private const string FDISTFunctionName = "FDIST";
		private const string FINVFunctionName = "FINV";
		private const string FISHERFunctionName = "FISHER";
		private const string FISHERINVFunctionName = "FISHERINV";
		private const string FLOORFunctionName = "FLOOR";
		private const string GAMMADISTFunctionName = "GAMMADIST";
		private const string GAMMAINVFunctionName = "GAMMAINV";
		private const string CEILINGFunctionName = "CEILING";
		private const string HYPGEOMVERTFunctionName = "HYPGEOMVERT";
		private const string LOGNORMDISTFunctionName = "LOGNORMDIST";
		private const string LOGINVFunctionName = "LOGINV";
		private const string NEGBINOMDISTFunctionName = "NEGBINOMDIST";
		private const string NORMDISTFunctionName = "NORMDIST";
		private const string NORMSDISTFunctionName = "NORMSDIST";
		private const string NORMINVFunctionName = "NORMINV";
		private const string MNORMSINVFunctionName = "MNORMSINV";
		private const string STANDARDIZEFunctionName = "STANDARDIZE";
		private const string ODDFunctionName = "ODD";
		private const string PERMUTFunctionName = "PERMUT";
		private const string POISSONFunctionName = "POISSON";
		private const string TDISTFunctionName = "TDIST";
		private const string WEIBULLFunctionName = "WEIBULL";
		private const string SUMXMY2FunctionName = "SUMXMY2";
		private const string SUMX2MY2FunctionName = "SUMX2MY2";
		private const string SUMX2PY2FunctionName = "SUMX2PY2";
		private const string CHITESTFunctionName = "CHITEST";
		private const string CORRELFunctionName = "CORREL";
		private const string COVARFunctionName = "COVAR";
		private const string FORECASTFunctionName = "FORECAST";
		private const string FTESTFunctionName = "FTEST";
		private const string INTERCEPTFunctionName = "INTERCEPT";
		private const string PEARSONFunctionName = "PEARSON";
		private const string RSQFunctionName = "RSQ";
		private const string STEYXFunctionName = "STEYX";
		private const string SLOPEFunctionName = "SLOPE";
		private const string TTESTFunctionName = "TTEST";
		private const string PROBFunctionName = "PROB";
		private const string DEVSQFunctionName = "DEVSQ";
		private const string GEOMEANFunctionName = "GEOMEAN";
		private const string HARMEANFunctionName = "HARMEAN";
		private const string SUMSQFunctionName = "SUMSQ";
		private const string KURTFunctionName = "KURT";
		private const string SKEWFunctionName = "SKEW";
		private const string ZTESTFunctionName = "ZTEST";
		private const string LARGEFunctionName = "LARGE";
		private const string SMALLFunctionName = "SMALL";
		private const string QUARTILEFunctionName = "QUARTILE";
		private const string PERCENTILEFunctionName = "PERCENTILE";
		private const string PERCENTRANKFunctionName = "PERCENTRANK";
		private const string MODEFunctionName = "MODE";
		private const string TRIMMEANFunctionName = "TRIMMEAN";
		private const string TINVFunctionName = "TINV";
		private const string CONCATENATEFunctionName = "CONCATENATE";
		private const string POWERFunctionName = "POWER";
		private const string RADIANSFunctionName = "RADIANS";
		private const string DEGREESFunctionName = "DEGREES";
		private const string SUBTOTALFunctionName = "SUBTOTAL";
		private const string SUMIFFunctionName = "SUMIF";
		private const string COUNTIFFunctionName = "COUNTIF";
		private const string COUNTBLANKFunctionName = "COUNTBLANK";
		private const string ISPMTFunctionName = "ISPMT";
		private const string DATEDIFFunctionName = "DATEDIF";
		private const string DATESTRINGFunctionName = "DATESTRING";
		private const string NUMBERSTRINGFunctionName = "NUMBERSTRING";
		private const string ROMANFunctionName = "ROMAN";
		private const string GETPIVOTDATAFunctionName = "GETPIVOTDATA";
		private const string HYPERLINKFunctionName = "HYPERLINK";
		private const string PHONETICFunctionName = "PHONETIC";
		private const string AVERAGEAFunctionName = "AVERAGEA";
		private const string MAXAFunctionName = "MAXA";
		private const string MINAFunctionName = "MINA";
		private const string STDEVPAFunctionName = "STDEVPA";
		private const string VARPAFunctionName = "VARPA";
		private const string STDEVAFunctionName = "STDEVA";
		private const string VARAFunctionName = "VARA";

		private const string ACCRINTFunctionName = "ACCRINT";
		private const string ACCRINTMFunctionName = "ACCRINTM";
		private const string AMORDEGRCFunctionName = "AMORDEGRC";
		private const string AMORLINCFunctionName = "AMORLINC";
		private const string AVERAGEIFFunctionName = "AVERAGEIF";
		private const string AVERAGEIFSFunctionName = "AVERAGEIFS";
		private const string BAHTTEXTFunctionName = "BAHTTEXT";
		private const string BESSELIFunctionName = "BESSELI";
		private const string BESSELJFunctionName = "BESSELJ";
		private const string BESSELKFunctionName = "BESSELK";
		private const string BESSELYFunctionName = "BESSELY";
		private const string BIN2DECFunctionName = "BIN2DEC";
		private const string BIN2HEXFunctionName = "BIN2HEX";
		private const string BIN2OCTFunctionName = "BIN2OCT";
		private const string COMPLEXFunctionName = "COMPLEX";
		private const string CONVERTFunctionName = "CONVERT";
		private const string COUPDAYBSFunctionName = "COUPDAYBS";
		private const string COUPDAYSFunctionName = "COUPDAYS";
		private const string COUPDAYSNCFunctionName = "COUPDAYSNC";
		private const string COUPNCDFunctionName = "COUPNCD";
		private const string COUPNUMFunctionName = "COUPNUM";
		private const string COUPPCDFunctionName = "COUPPCD";
		private const string CUBEKPIMEMBERFunctionName = "CUBEKPIMEMBER";
		private const string CUBEMEMBERFunctionName = "CUBEMEMBER";
		private const string CUBEMEMBERPROPERTYFunctionName = "CUBEMEMBERPROPERTY";
		private const string CUBERANKEDMEMBERFunctionName = "CUBERANKEDMEMBER";
		private const string CUBESETFunctionName = "CUBESET";
		private const string CUBESETCOUNTFunctionName = "CUBESETCOUNT";
		private const string CUBEVALUEFunctionName = "CUBEVALUE";
		private const string CUMIPMTFunctionName = "CUMIPMT";
		private const string CUMPRINCFunctionName = "CUMPRINC";
		private const string DEC2BINFunctionName = "DEC2BIN";
		private const string DEC2HEXFunctionName = "DEC2HEX";
		private const string DEC2OCTFunctionName = "DEC2OCT";
		private const string DELTAFunctionName = "DELTA";
		private const string DISCFunctionName = "DISC";
		private const string DOLLARDEFunctionName = "DOLLARDE";
		private const string DOLLARFRFunctionName = "DOLLARFR";
		private const string DURATIONFunctionName = "DURATION";
		private const string EDATEFunctionName = "EDATE";
		private const string EFFECTFunctionName = "EFFECT";
		private const string EOMONTHFunctionName = "EOMONTH";
		private const string ERFFunctionName = "ERF";
		private const string ERFCFunctionName = "ERFC";
		private const string FACTDOUBLEFunctionName = "FACTDOUBLE";
		private const string FVSCHEDULEFunctionName = "FVSCHEDULE";
		private const string GCDFunctionName = "GCD";
		private const string GESTEPFunctionName = "GESTEP";
		private const string HEX2BINFunctionName = "HEX2BIN";
		private const string HEX2DECFunctionName = "HEX2DEC";
		private const string HEX2OCTFunctionName = "HEX2OCT";
		private const string HYPGEOMDISTFunctionName = "HYPGEOMDIST";
		private const string IFERRORFunctionName = "IFERROR";							// MD 8/29/11 - TFS85072
		private const string IMABSFunctionName = "IMABS";
		private const string IMAGINARYFunctionName = "IMAGINARY";
		private const string IMARGUMENTFunctionName = "IMARGUMENT";
		private const string IMCONJUGATEFunctionName = "IMCONJUGATE";
		private const string IMCOSFunctionName = "IMCOS";
		private const string IMDIVFunctionName = "IMDIV";
		private const string IMEXPFunctionName = "IMEXP";
		private const string IMLNFunctionName = "IMLN";
		private const string IMLOG10FunctionName = "IMLOG10";
		private const string IMLOG2FunctionName = "IMLOG2";
		private const string IMPOWERFunctionName = "IMPOWER";
		private const string IMPRODUCTFunctionName = "IMPRODUCT";
		private const string IMREALFunctionName = "IMREAL";
		private const string IMSINFunctionName = "IMSIN";
		private const string IMSQRTFunctionName = "IMSQRT";
		private const string IMSUBFunctionName = "IMSUB";
		private const string IMSUMFunctionName = "IMSUM";
		private const string INTRATEFunctionName = "INTRATE";
		private const string ISEVENFunctionName = "ISEVEN";
		private const string ISODDFunctionName = "ISODD";
		private const string LCMFunctionName = "LCM";
		private const string MDURATIONFunctionName = "MDURATION";
		private const string MROUNDFunctionName = "MROUND";
		private const string MULTINOMIALFunctionName = "MULTINOMIAL";
		private const string NETWORKDAYSFunctionName = "NETWORKDAYS";
		private const string NOMINALFunctionName = "NOMINAL";
		private const string NORMSINVFunctionName = "NORMSINV";
		private const string OCT2BINFunctionName = "OCT2BIN";
		private const string OCT2DECFunctionName = "OCT2DEC";
		private const string OCT2HEXFunctionName = "OCT2HEX";
		private const string ODDFPRICEFunctionName = "ODDFPRICE";
		private const string ODDFYIELDFunctionName = "ODDFYIELD";
		private const string ODDLPRICEFunctionName = "ODDLPRICE";
		private const string ODDLYIELDFunctionName = "ODDLYIELD";
		private const string PRICEFunctionName = "PRICE";
		private const string PRICEDISCFunctionName = "PRICEDISC";
		private const string PRICEMATFunctionName = "PRICEMAT";
		private const string QUOTIENTFunctionName = "QUOTIENT";
		private const string RANDBETWEENFunctionName = "RANDBETWEEN";
		private const string RECEIVEDFunctionName = "RECEIVED";
		private const string RTDFunctionName = "RTD";
		private const string SERIESSUMFunctionName = "SERIESSUM";
		private const string SQRTPIFunctionName = "SQRTPI";
		private const string SUMIFSFunctionName = "SUMIFS";
		private const string TBILLEQFunctionName = "TBILLEQ";
		private const string TBILLPRICEFunctionName = "TBILLPRICE";
		private const string TBILLYIELDFunctionName = "TBILLYIELD";
		private const string WEEKNUMFunctionName = "WEEKNUM";
		private const string WORKDAYFunctionName = "WORKDAY";
		private const string XIRRFunctionName = "XIRR";
		private const string XNPVFunctionName = "XNPV";
		private const string YEARFRACFunctionName = "YEARFRAC";
		private const string YIELDFunctionName = "YIELD";
		private const string YIELDDISCFunctionName = "YIELDDISC";
		private const string YIELDMATFunctionName = "YIELDMAT";

        // MBS 7/10/08 - Excel 2007 Format
        internal const byte MAXPARAMS = 30;
		internal const byte MAXPARAMS2007 = 255;

		#endregion Constants
		

		#region Member Variables

		private string name;
		private int functionID;
		private byte minParams;

        // MBS 7/10/08 - Excel 2007 Format
        // Changed to an int because we need to allow -1
        //private byte maxParams;
        private int maxParams;

		// 8/4/08 - Excel formula solving
		private bool isUnknownAddInFunction;

		private bool isVolatile;

		// MD 10/9/07 - BR27172
		private int tuplesStart;
		private int tuplesDegree;
		private int[] forcedReferenceIndices;

		private TokenClass returnClass;
		private TokenClass[] paramClasses;

		// MD 8/29/11 - TFS85072
		private bool isExcel2007OnlyFunction;

		// MD 4/6/12 - TFS102169
		// Added support for external functions.
		private string workbookPath;

		#endregion Member Variables

		#region Constructor

		private Function(
			string name,
			int functionID,
			byte minParameters,
            // MBS 7/10/08 - Excel 2007 Format
            // Changed to an int because we need to allow -1
			//byte maxParameters,
            int maxParameters,
			bool isVolatile,

			// MD 10/9/07 - BR27172
			int tuplesStart,
			int tuplesDegree,
			int[] forcedReferenceIndices,

			TokenClass returnClass,
			params TokenClass[] paramClasses )
		{
			// MD 10/8/07 - BR27172
			// The condition here does not apply for AddIn functions
			//Debug.Assert( minParameters <= paramClasses.Length && paramClasses.Length <= maxParameters );
			//Debug.Assert( functionID == Function.AddInFunctionID || ( minParameters <= paramClasses.Length && paramClasses.Length <= maxParameters ) );

			this.name = name;

			// MD 8/29/11 - TFS85072
			// If the Excel2007OnlyID is specified, it is still an add-in function (in Excel 2003), but we also want to set the flag that it is defined
			// in Excel 2007 only.
			//this.functionID = functionID;
			if (functionID == Function.Excel2007OnlyID)
			{
				this.functionID = Function.AddInFunctionID;
				this.isExcel2007OnlyFunction = true;

				// MD 5/10/12 - TFS111368
				// The Excel 2007 only add in functions need the add in workbook name.
				this.workbookPath = AddInFunctionsWorkbookReference.AddInFunctionsWorkbookName;
			}
			else
			{
				this.functionID = functionID;
			}

			this.minParams = minParameters;
			this.maxParams = maxParameters;
			this.isVolatile = isVolatile;

			// MD 10/9/07 - BR27172
			this.tuplesStart = tuplesStart;
			this.tuplesDegree = tuplesDegree;
			this.forcedReferenceIndices = forcedReferenceIndices;

			this.returnClass = returnClass;
			this.paramClasses = paramClasses;
		}

		#endregion Constructor

		#region Interfaces

		// MD 10/8/07 - BR27172
		// Implemented IComparable so we can sort the functions
		#region IComparable<Function> Members

		// MD 1/24/08
		// Made changes to allow for VS2008 style unit test accessors
		//int IComparable<Function>.CompareTo( Function other )
		public int CompareTo( Function other )
		{
			int result = this.functionID - other.functionID;

			if ( result != 0 )
				return result;

			// MD 4/6/12 - TFS102169
			// We also need to distinguish between functions which are named the same from different workbooks.
			//return string.Compare( this.name, other.name, StringComparison.InvariantCultureIgnoreCase );
			result = string.Compare(this.name, other.name, StringComparison.InvariantCultureIgnoreCase);
			if (result != 0)
				return result;

			return string.Compare(this.workbookPath, other.workbookPath, StringComparison.InvariantCultureIgnoreCase);
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region GetExpectedParameterClass

		public virtual TokenClass GetExpectedParameterClass( int index )
		{
            // MBS 7/10/08 - Excel 2007 Format
            // We can't assert for this anymore since we don't know the current format and
            // how many parameters we can take.  This should be handled before we get 
            // here by the function parsing and the GetMaxParams method
			//Debug.Assert( index < maxParams );            

			if ( index < this.paramClasses.Length )
				return this.paramClasses[ index ];

			return this.paramClasses[ this.paramClasses.Length - 1 ];
		}

		#endregion GetExpectedParameterClass

        // MBS 7/10/08 - Excel 2007 Format
        #region GetMaxParams

        public byte GetMaxParams(WorkbookFormat format)
        {
			// MD 2/4/11
			// Done while fixing TFS65015
			// Use the new Utilities.Is2003Format method so we don't need to switch on the format all over the place.
			//byte maxFormatParams = 0;
			//switch (format)
			//{
			//    case WorkbookFormat.Excel97To2003:
			//    // MD 5/7/10 - 10.2 - Excel Templates
			//    case WorkbookFormat.Excel97To2003Template:
			//        maxFormatParams = Function.MAXPARAMS;
			//        break;
			//
			//    case WorkbookFormat.Excel2007:
			//    // MD 10/1/08 - TFS8471
			//    case WorkbookFormat.Excel2007MacroEnabled:
			//    // MD 5/7/10 - 10.2 - Excel Templates
			//    case WorkbookFormat.Excel2007MacroEnabledTemplate:
			//    case WorkbookFormat.Excel2007Template:
			//        maxFormatParams = Function.MAXPARAMS2007;
			//        break;
			//
			//    default:
			//        Utilities.DebugFail("Unkown workbook format: " + format);
			//        goto case WorkbookFormat.Excel97To2003;
			//}
			byte maxFormatParams = Utilities.Is2003Format(format)
				? Function.MAXPARAMS
				: Function.MAXPARAMS2007;

            if (this.maxParams == -1)
            {
                int maxValues = maxFormatParams - this.TuplesStart;
                while ((maxValues % this.TuplesDegree) != 0)
                    maxValues--;

                return (byte)maxValues;
            }

            return (byte)Math.Min(maxFormatParams, this.maxParams);
        }
        #endregion //GetMaxParams

        #endregion Methods

        #region Properties

        #region ID

        public int ID
		{
			get { return this.functionID; }
		}

		#endregion ID

		// MD 10/8/07 - BR27172
		#region IsAddIn






		public bool IsAddIn
		{
			get { return this.functionID == Function.AddInFunctionID; }
		}

		#endregion IsAddIn

		// MD 8/29/11 - TFS85072
		#region IsExcel2007OnlyFunction






		public bool IsExcel2007OnlyFunction
		{
			get { return this.isExcel2007OnlyFunction; }
		}

		#endregion  // IsExcel2007OnlyFunction

		// MD 10/9/07 - BR27172
		#region IsFuncV






		public bool IsFuncV
		{
			get
			{
				return
					this.functionID == Function.AddInFunctionID ||
					this.minParams != this.maxParams;
			}
		}

		#endregion IsFuncV

		// MD 10/9/07 - BR27172
		#region IsInternalAddInFunction







		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public bool IsInternalAddInFunction
		{
			get
			{
				Debug.Assert( this.IsAddIn, "IsInternalAddInFunction should not be called for non add-in functions." );

				return
					this == Function.AVERAGEIF ||
					this == Function.AVERAGEIFS ||
					this == Function.BAHTTEXT ||
					this == Function.CUBEKPIMEMBER ||
					this == Function.CUBEMEMBER ||
					this == Function.CUBEMEMBERPROPERTY ||
					this == Function.CUBERANKEDMEMBER ||
					this == Function.CUBESET ||
					this == Function.CUBESETCOUNT ||
					this == Function.CUBEVALUE ||
					this == Function.IFERROR ||					// MD 8/29/11 - TFS85072
					this == Function.RTD ||
					this == Function.SUMIFS;
			}
		}

		#endregion IsInternalAddInFunction

		// 8/4/08 - Excel formula solving
		#region IsUnknownAddInFunction






		internal bool IsUnknownAddInFunction
		{
			get { return this.isUnknownAddInFunction; }
		} 

		#endregion IsUnknownAddInFunction

		#region IsVolatile

		public bool IsVolatile
		{
			get { return this.isVolatile; }
		}

		#endregion IsVolatile

		// MD 10/9/07 - BR27172
		#region ForcedReferenceIndices






		public int[] ForcedReferenceIndices
		{
			get { return this.forcedReferenceIndices; }
		}

		#endregion ForcedReferenceIndices

        // MBS 7/10/08 - Excel 2007 Format      
        // Removed in favor of the new GetMaxParams method.  
		#region MaxParams - Removed

        //public byte MaxParams
        //{
        //    get { return this.maxParams; }
        //}

		#endregion MaxParams

		#region MinParams

		public byte MinParams
		{
			get { return this.minParams; }
		}

		#endregion MinParams

		#region Name

		public string Name
		{
			get { return this.name; }
		}

		#endregion Name

		#region ReturnClass

		public TokenClass ReturnClass
		{
			get { return this.returnClass; }
		}

		#endregion ReturnClass

		// MD 10/9/07 - BR27172
		#region TuplesDegree






		public int TuplesDegree
		{
			get { return this.tuplesDegree; }
		}

		#endregion TuplesDegree

		// MD 10/9/07 - BR27172
		#region TuplesStart






		public int TuplesStart
		{
			get { return this.tuplesStart; }
		}

		#endregion TuplesStart

		// MD 4/6/12 - TFS102169
		// Added support for external functions.
		#region WorkbookPath

		public string WorkbookPath
		{
			get { return this.workbookPath; }
		}

		#endregion // WorkbookPath

		#endregion Properties

		#region Static Members

		// MD 10/8/07 - BR27172
		// A sorted list will not allow duplicate keys, and all add-in functions have the same function id, 
		// so we need to duplicate keys.
		//private static SortedList<int, Function> functions = new SortedList<int, Function>();
		// MD 4/18/08 - BR32154
		// This variable may be accessed by multiple threads. Since it is just used for caching, instead of using locks, we can just use
		// a different collection for each thread by putting the ThreadStatic attribute on it. However, if we do this, we cannot create
		// the collection at the definition, because the static constructor is only called on the first thread that accesses the class.
		// Additional threads would have a null collection. Therefore, we need to lazily create the collection in a property. All references
		// to the static field have been replaced with references to the static property.
		//private static List<Function> functions = new List<Function>();
		[ThreadStatic]
		private static List<Function> functions;
		private static List<Function> Functions
		{
			get
			{
				if ( Function.functions == null )
					Function.functions = new List<Function>();

				return Function.functions;
			}
		}

		// MD 4/6/12 - TFS102169
		// Added support for external functions.
		#region GetExternalFunction

		public static Function GetExternalFunction(string filePath, string name)
		{
			Function function = Function.GetFunction(name, Function.AddInFunctionID, 0, -1, false, 0, 1, null, TokenClass.Reference, TokenClass.Reference);
			function.isUnknownAddInFunction = true;
			function.workbookPath = filePath;
			return function;
		}

		#endregion // GetExternalFunction

		#region GetFunction

		public static Function GetFunction( int functionID )
		{
			switch ( functionID )
			{
				// MD 10/9/07 - BR27172
				// Changed all items which existed in this switch statement because they were using literals.
				// Moved them to constants
				case Function.COUNTFunctionID: return Function.COUNT;
				case Function.IFFunctionID: return Function.IF;
				case Function.ISNAFunctionID: return Function.ISNA;
				case Function.ISERRORFunctionID: return Function.ISERROR;
				case Function.SUMFunctionID: return Function.SUM;
				case Function.AVERAGEFunctionID: return Function.AVERAGE;
				case Function.MINFunctionID: return Function.MIN;
				case Function.MAXFunctionID: return Function.MAX;
				case Function.ROWFunctionID: return Function.ROW;
				case Function.COLUMNFunctionID: return Function.COLUMN;
				case Function.NAFunctionID: return Function.NA;
				case Function.NPVFunctionID: return Function.NPV;
				case Function.STDEVFunctionID: return Function.STDEV;
				case Function.DOLLARFunctionID: return Function.DOLLAR;
				case Function.FIXEDFunctionID: return Function.FIXED;
				case Function.SINFunctionID: return Function.SIN;
				case Function.COSFunctionID: return Function.COS;
				case Function.TANFunctionID: return Function.TAN;
				case Function.ATANFunctionID: return Function.ATAN;
				case Function.PIFunctionID: return Function.PI;
				case Function.SQRTFunctionID: return Function.SQRT;
				case Function.EXPFunctionID: return Function.EXP;
				case Function.LNFunctionID: return Function.LN;
				case Function.LOG10FunctionID: return Function.LOG10;
				case Function.ABSFunctionID: return Function.ABS;
				case Function.INTFunctionID: return Function.INT;
				case Function.SIGNFunctionID: return Function.SIGN;
				case Function.ROUNDFunctionID: return Function.ROUND;
				case Function.LOOKUPFunctionID: return Function.LOOKUP;
				case Function.INDEXFunctionID: return Function.INDEX;
				case Function.REPTFunctionID: return Function.REPT;
				case Function.MIDFunctionID: return Function.MID;
				case Function.LENFunctionID: return Function.LEN;
				case Function.VALUEFunctionID: return Function.VALUE;
				case Function.TRUEFunctionID: return Function.TRUE;
				case Function.FALSEFunctionID: return Function.FALSE;
				case Function.ANDFunctionID: return Function.AND;
				case Function.ORFunctionID: return Function.OR;
				case Function.NOTFunctionID: return Function.NOT;
				case Function.MODFunctionID: return Function.MOD;
				case Function.DCOUNTFunctionID: return Function.DCOUNT;
				case Function.DSUMFunctionID: return Function.DSUM;
				case Function.DAVERAGEFunctionID: return Function.DAVERAGE;
				case Function.DMINFunctionID: return Function.DMIN;
				case Function.DMAXFunctionID: return Function.DMAX;
				case Function.DSTDEVFunctionID: return Function.DSTDEV;
				case Function.VARFunctionID: return Function.VAR;
				case Function.DVARFunctionID: return Function.DVAR;
				case Function.TEXTFunctionID: return Function.TEXT;
				case Function.LINESTFunctionID: return Function.LINEST;
				case Function.TRENDFunctionID: return Function.TREND;
				case Function.LOGESTFunctionID: return Function.LOGEST;
				case Function.GROWTHFunctionID: return Function.GROWTH;
				case Function.PVFunctionID: return Function.PV;
				case Function.FVFunctionID: return Function.FV;
				case Function.NPERFunctionID: return Function.NPER;
				case Function.PMTFunctionID: return Function.PMT;
				case Function.RATEFunctionID: return Function.RATE;
				case Function.MIRRFunctionID: return Function.MIRR;
				case Function.IRRFunctionID: return Function.IRR;
				case Function.RANDFunctionID: return Function.RAND;
				case Function.MATCHFunctionID: return Function.MATCH;
				case Function.DATEFunctionID: return Function.DATE;
				case Function.TIMEFunctionID: return Function.TIME;
				case Function.DAYFunctionID: return Function.DAY;
				case Function.MONTHFunctionID: return Function.MONTH;
				case Function.YEARFunctionID: return Function.YEAR;
				case Function.WEEKDAYFunctionID: return Function.WEEKDAY;
				case Function.HOURFunctionID: return Function.HOUR;
				case Function.MINUTEFunctionID: return Function.MINUTE;
				case Function.SECONDFunctionID: return Function.SECOND;
				case Function.NOWFunctionID: return Function.NOW;
				case Function.AREASFunctionID: return Function.AREAS;
				case Function.ROWSFunctionID: return Function.ROWS;
				case Function.COLUMNSFunctionID: return Function.COLUMNS;
				case Function.OFFSETFunctionID: return Function.OFFSET;
				case Function.SEARCHFunctionID: return Function.SEARCH;
				case Function.TRANSPOSEFunctionID: return Function.TRANSPOSE;
				case Function.TYPEFunctionID: return Function.TYPE;
				case Function.ATAN2FunctionID: return Function.ATAN2;
				case Function.ASINFunctionID: return Function.ASIN;
				case Function.ACOSFunctionID: return Function.ACOS;
				case Function.CHOOSEFunctionID: return Function.CHOOSE;
				case Function.HLOOKUPFunctionID: return Function.HLOOKUP;
				case Function.VLOOKUPFunctionID: return Function.VLOOKUP;
				case Function.ISREFFunctionID: return Function.ISREF;
				case Function.LOGFunctionID: return Function.LOG;
				case Function.CHARFunctionID: return Function.CHAR;
				case Function.LOWERFunctionID: return Function.LOWER;
				case Function.UPPERFunctionID: return Function.UPPER;
				case Function.PROPERFunctionID: return Function.PROPER;
				case Function.LEFTFunctionID: return Function.LEFT;
				case Function.RIGHTFunctionID: return Function.RIGHT;
				case Function.EXACTFunctionID: return Function.EXACT;
				case Function.TRIMFunctionID: return Function.TRIM;
				case Function.REPLACEFunctionID: return Function.REPLACE;
				case Function.SUBSTITUTEFunctionID: return Function.SUBSTITUTE;
				case Function.CODEFunctionID: return Function.CODE;
				case Function.FINDFunctionID: return Function.FIND;
				case Function.CELLFunctionID: return Function.CELL;
				case Function.ISERRFunctionID: return Function.ISERR;
				case Function.ISTEXTFunctionID: return Function.ISTEXT;
				case Function.ISNUMBERFunctionID: return Function.ISNUMBER;
				case Function.ISBLANKFunctionID: return Function.ISBLANK;
				case Function.TFunctionID: return Function.T;
				case Function.NFunctionID: return Function.N;
				case Function.DATEVALUEFunctionID: return Function.DATEVALUE;
				case Function.TIMEVALUEFunctionID: return Function.TIMEVALUE;
				case Function.SLNFunctionID: return Function.SLN;
				case Function.SYDFunctionID: return Function.SYD;
				case Function.DDBFunctionID: return Function.DDB;
				case Function.INDIRECTFunctionID: return Function.INDIRECT;
				case Function.CLEANFunctionID: return Function.CLEAN;
				case Function.MDETERMFunctionID: return Function.MDETERM;
				case Function.MINVERSEFunctionID: return Function.MINVERSE;
				case Function.MMULTFunctionID: return Function.MMULT;
				case Function.IPMTFunctionID: return Function.IPMT;
				case Function.PPMTFunctionID: return Function.PPMT;
				case Function.COUNTAFunctionID: return Function.COUNTA;
				case Function.PRODUCTFunctionID: return Function.PRODUCT;
				case Function.FACTFunctionID: return Function.FACT;
				case Function.DPRODUCTFunctionID: return Function.DPRODUCT;
				case Function.ISNONTEXTFunctionID: return Function.ISNONTEXT;
				case Function.STDEVPFunctionID: return Function.STDEVP;
				case Function.VARPFunctionID: return Function.VARP;
				case Function.DSTDEVPFunctionID: return Function.DSTDEVP;
				case Function.DVARPFunctionID: return Function.DVARP;
				case Function.TRUNCFunctionID: return Function.TRUNC;
				case Function.ISLOGICALFunctionID: return Function.ISLOGICAL;
				case Function.DCOUNTAFunctionID: return Function.DCOUNTA;
				case Function.USDOLLARFunctionID: return Function.USDOLLAR;
				case Function.FINDBFunctionID: return Function.FINDB;
				case Function.SEARCHBFunctionID: return Function.SEARCHB;
				case Function.REPLACEBFunctionID: return Function.REPLACEB;
				case Function.LEFTBFunctionID: return Function.LEFTB;
				case Function.RIGHTBFunctionID: return Function.RIGHTB;
				case Function.MIDBFunctionID: return Function.MIDB;
				case Function.LENBFunctionID: return Function.LENB;
				case Function.ROUNDUPFunctionID: return Function.ROUNDUP;
				case Function.ROUNDDOWNFunctionID: return Function.ROUNDDOWN;
				case Function.ASCFunctionID: return Function.ASC;
				case Function.DBSCFunctionID: return Function.DBSC;
				case Function.RANKFunctionID: return Function.RANK;
				case Function.ADDRESSFunctionID: return Function.ADDRESS;
				case Function.DAYS360FunctionID: return Function.DAYS360;
				case Function.TODAYFunctionID: return Function.TODAY;
				case Function.VDBFunctionID: return Function.VDB;
				case Function.MEDIANFunctionID: return Function.MEDIAN;
				case Function.SUMPRODUCTFunctionID: return Function.SUMPRODUCT;
				case Function.SINHFunctionID: return Function.SINH;
				case Function.COSHFunctionID: return Function.COSH;
				case Function.TANHFunctionID: return Function.TANH;
				case Function.ASINHFunctionID: return Function.ASINH;
				case Function.ACOSHFunctionID: return Function.ACOSH;
				case Function.ATANHFunctionID: return Function.ATANH;
				case Function.DGETFunctionID: return Function.DGET;
				case Function.INFOFunctionID: return Function.INFO;
				case Function.DBFunctionID: return Function.DB;
				case Function.FREQUENCYFunctionID: return Function.FREQUENCY;
				case Function.ERROR_TYPEFunctionID: return Function.ERROR_TYPE;
				case Function.AVEDEVFunctionID: return Function.AVEDEV;
				case Function.BETADISTFunctionID: return Function.BETADIST;
				case Function.GAMMALNFunctionID: return Function.GAMMALN;
				case Function.BETAINVFunctionID: return Function.BETAINV;
				case Function.BINOMDISTFunctionID: return Function.BINOMDIST;
				case Function.CHIDISTFunctionID: return Function.CHIDIST;
				case Function.CHIINVFunctionID: return Function.CHIINV;
				case Function.COMBINFunctionID: return Function.COMBIN;
				case Function.CONFIDENCEFunctionID: return Function.CONFIDENCE;
				case Function.CRITBINOMFunctionID: return Function.CRITBINOM;
				case Function.EVENFunctionID: return Function.EVEN;
				case Function.EXPONDISTFunctionID: return Function.EXPONDIST;
				case Function.FDISTFunctionID: return Function.FDIST;
				case Function.FINVFunctionID: return Function.FINV;
				case Function.FISHERFunctionID: return Function.FISHER;
				case Function.FISHERINVFunctionID: return Function.FISHERINV;
				case Function.FLOORFunctionID: return Function.FLOOR;
				case Function.GAMMADISTFunctionID: return Function.GAMMADIST;
				case Function.GAMMAINVFunctionID: return Function.GAMMAINV;
				case Function.CEILINGFunctionID: return Function.CEILING;
				case Function.HYPGEOMVERTFunctionID: return Function.HYPGEOMVERT;
				case Function.LOGNORMDISTFunctionID: return Function.LOGNORMDIST;
				case Function.LOGINVFunctionID: return Function.LOGINV;
				case Function.NEGBINOMDISTFunctionID: return Function.NEGBINOMDIST;
				case Function.NORMDISTFunctionID: return Function.NORMDIST;
				case Function.NORMSDISTFunctionID: return Function.NORMSDIST;
				case Function.NORMINVFunctionID: return Function.NORMINV;
				case Function.MNORMSINVFunctionID: return Function.MNORMSINV;
				case Function.STANDARDIZEFunctionID: return Function.STANDARDIZE;
				case Function.ODDFunctionID: return Function.ODD;
				case Function.PERMUTFunctionID: return Function.PERMUT;
				case Function.POISSONFunctionID: return Function.POISSON;
				case Function.TDISTFunctionID: return Function.TDIST;
				case Function.WEIBULLFunctionID: return Function.WEIBULL;
				case Function.SUMXMY2FunctionID: return Function.SUMXMY2;
				case Function.SUMX2MY2FunctionID: return Function.SUMX2MY2;
				case Function.SUMX2PY2FunctionID: return Function.SUMX2PY2;
				case Function.CHITESTFunctionID: return Function.CHITEST;
				case Function.CORRELFunctionID: return Function.CORREL;
				case Function.COVARFunctionID: return Function.COVAR;
				case Function.FORECASTFunctionID: return Function.FORECAST;
				case Function.FTESTFunctionID: return Function.FTEST;
				case Function.INTERCEPTFunctionID: return Function.INTERCEPT;
				case Function.PEARSONFunctionID: return Function.PEARSON;
				case Function.RSQFunctionID: return Function.RSQ;
				case Function.STEYXFunctionID: return Function.STEYX;
				case Function.SLOPEFunctionID: return Function.SLOPE;
				case Function.TTESTFunctionID: return Function.TTEST;
				case Function.PROBFunctionID: return Function.PROB;
				case Function.DEVSQFunctionID: return Function.DEVSQ;
				case Function.GEOMEANFunctionID: return Function.GEOMEAN;
				case Function.HARMEANFunctionID: return Function.HARMEAN;
				case Function.SUMSQFunctionID: return Function.SUMSQ;
				case Function.KURTFunctionID: return Function.KURT;
				case Function.SKEWFunctionID: return Function.SKEW;
				case Function.ZTESTFunctionID: return Function.ZTEST;
				case Function.LARGEFunctionID: return Function.LARGE;
				case Function.SMALLFunctionID: return Function.SMALL;
				case Function.QUARTILEFunctionID: return Function.QUARTILE;
				case Function.PERCENTILEFunctionID: return Function.PERCENTILE;
				case Function.PERCENTRANKFunctionID: return Function.PERCENTRANK;
				case Function.MODEFunctionID: return Function.MODE;
				case Function.TRIMMEANFunctionID: return Function.TRIMMEAN;
				case Function.TINVFunctionID: return Function.TINV;
				case Function.CONCATENATEFunctionID: return Function.CONCATENATE;
				case Function.POWERFunctionID: return Function.POWER;
				case Function.RADIANSFunctionID: return Function.RADIANS;
				case Function.DEGREESFunctionID: return Function.DEGREES;
				case Function.SUBTOTALFunctionID: return Function.SUBTOTAL;
				case Function.SUMIFFunctionID: return Function.SUMIF;
				case Function.COUNTIFFunctionID: return Function.COUNTIF;
				case Function.COUNTBLANKFunctionID: return Function.COUNTBLANK;
				case Function.ISPMTFunctionID: return Function.ISPMT;
				case Function.DATEDIFFunctionID: return Function.DATEDIF;
				case Function.DATESTRINGFunctionID: return Function.DATESTRING;
				case Function.NUMBERSTRINGFunctionID: return Function.NUMBERSTRING;
				case Function.ROMANFunctionID: return Function.ROMAN;
				case Function.GETPIVOTDATAFunctionID: return Function.GETPIVOTDATA;
				case Function.HYPERLINKFunctionID: return Function.HYPERLINK;
				case Function.PHONETICFunctionID: return Function.PHONETIC;
				case Function.AVERAGEAFunctionID: return Function.AVERAGEA;
				case Function.MAXAFunctionID: return Function.MAXA;
				case Function.MINAFunctionID: return Function.MINA;
				case Function.STDEVPAFunctionID: return Function.STDEVPA;
				case Function.VARPAFunctionID: return Function.VARPA;
				case Function.STDEVAFunctionID: return Function.STDEVA;
				case Function.VARAFunctionID: return Function.VARA;

				// MD 10/8/07 - BR27172
				// Return the generic add in function for all functions with code 255
				case Function.AddInFunctionID: return Function.AddInFunction;

				default: 
					Utilities.DebugFail( "Unknown function code: " + functionID );
					return null;
			}
		}

		#endregion GetFunction

		#region GetFunction

		public static Function GetFunction( string name )
		{
			// MD 4/6/12 - TFS101506
			//switch ( name.ToUpper( CultureInfo.CurrentCulture ) )
			switch (name.ToUpper(CultureInfo.InvariantCulture))
			{
				// MD 10/9/07 - BR27172
				// Changed all items which existed in this switch statement because they were using literals.
				// Moved them to constants
				case Function.COUNTFunctionName: return Function.COUNT;
				case Function.IFFunctionName: return Function.IF;
				case Function.ISNAFunctionName: return Function.ISNA;
				case Function.ISERRORFunctionName: return Function.ISERROR;
				case Function.SUMFunctionName: return Function.SUM;
				case Function.AVERAGEFunctionName: return Function.AVERAGE;
				case Function.MINFunctionName: return Function.MIN;
				case Function.MAXFunctionName: return Function.MAX;
				case Function.ROWFunctionName: return Function.ROW;
				case Function.COLUMNFunctionName: return Function.COLUMN;
				case Function.NAFunctionName: return Function.NA;
				case Function.NPVFunctionName: return Function.NPV;
				case Function.STDEVFunctionName: return Function.STDEV;
				case Function.DOLLARFunctionName: return Function.DOLLAR;
				case Function.FIXEDFunctionName: return Function.FIXED;
				case Function.SINFunctionName: return Function.SIN;
				case Function.COSFunctionName: return Function.COS;
				case Function.TANFunctionName: return Function.TAN;
				case Function.ATANFunctionName: return Function.ATAN;
				case Function.PIFunctionName: return Function.PI;
				case Function.SQRTFunctionName: return Function.SQRT;
				case Function.EXPFunctionName: return Function.EXP;
				case Function.LNFunctionName: return Function.LN;
				case Function.LOG10FunctionName: return Function.LOG10;
				case Function.ABSFunctionName: return Function.ABS;
				case Function.INTFunctionName: return Function.INT;
				case Function.SIGNFunctionName: return Function.SIGN;
				case Function.ROUNDFunctionName: return Function.ROUND;
				case Function.LOOKUPFunctionName: return Function.LOOKUP;
				case Function.INDEXFunctionName: return Function.INDEX;
				case Function.REPTFunctionName: return Function.REPT;
				case Function.MIDFunctionName: return Function.MID;
				case Function.LENFunctionName: return Function.LEN;
				case Function.VALUEFunctionName: return Function.VALUE;
				case Function.TRUEFunctionName: return Function.TRUE;
				case Function.FALSEFunctionName: return Function.FALSE;
				case Function.ANDFunctionName: return Function.AND;
				case Function.ORFunctionName: return Function.OR;
				case Function.NOTFunctionName: return Function.NOT;
				case Function.MODFunctionName: return Function.MOD;
				case Function.DCOUNTFunctionName: return Function.DCOUNT;
				case Function.DSUMFunctionName: return Function.DSUM;
				case Function.DAVERAGEFunctionName: return Function.DAVERAGE;
				case Function.DMINFunctionName: return Function.DMIN;
				case Function.DMAXFunctionName: return Function.DMAX;
				case Function.DSTDEVFunctionName: return Function.DSTDEV;
				case Function.VARFunctionName: return Function.VAR;
				case Function.DVARFunctionName: return Function.DVAR;
				case Function.TEXTFunctionName: return Function.TEXT;
				case Function.LINESTFunctionName: return Function.LINEST;
				case Function.TRENDFunctionName: return Function.TREND;
				case Function.LOGESTFunctionName: return Function.LOGEST;
				case Function.GROWTHFunctionName: return Function.GROWTH;
				case Function.PVFunctionName: return Function.PV;
				case Function.FVFunctionName: return Function.FV;
				case Function.NPERFunctionName: return Function.NPER;
				case Function.PMTFunctionName: return Function.PMT;
				case Function.RATEFunctionName: return Function.RATE;
				case Function.MIRRFunctionName: return Function.MIRR;
				case Function.IRRFunctionName: return Function.IRR;
				case Function.RANDFunctionName: return Function.RAND;
				case Function.MATCHFunctionName: return Function.MATCH;
				case Function.DATEFunctionName: return Function.DATE;
				case Function.TIMEFunctionName: return Function.TIME;
				case Function.DAYFunctionName: return Function.DAY;
				case Function.MONTHFunctionName: return Function.MONTH;
				case Function.YEARFunctionName: return Function.YEAR;
				case Function.WEEKDAYFunctionName: return Function.WEEKDAY;
				case Function.HOURFunctionName: return Function.HOUR;
				case Function.MINUTEFunctionName: return Function.MINUTE;
				case Function.SECONDFunctionName: return Function.SECOND;
				case Function.NOWFunctionName: return Function.NOW;
				case Function.AREASFunctionName: return Function.AREAS;
				case Function.ROWSFunctionName: return Function.ROWS;
				case Function.COLUMNSFunctionName: return Function.COLUMNS;
				case Function.OFFSETFunctionName: return Function.OFFSET;
				case Function.SEARCHFunctionName: return Function.SEARCH;
				case Function.TRANSPOSEFunctionName: return Function.TRANSPOSE;
				case Function.TYPEFunctionName: return Function.TYPE;
				case Function.ATAN2FunctionName: return Function.ATAN2;
				case Function.ASINFunctionName: return Function.ASIN;
				case Function.ACOSFunctionName: return Function.ACOS;
				case Function.CHOOSEFunctionName: return Function.CHOOSE;
				case Function.HLOOKUPFunctionName: return Function.HLOOKUP;
				case Function.VLOOKUPFunctionName: return Function.VLOOKUP;
				case Function.ISREFFunctionName: return Function.ISREF;
				case Function.LOGFunctionName: return Function.LOG;
				case Function.CHARFunctionName: return Function.CHAR;
				case Function.LOWERFunctionName: return Function.LOWER;
				case Function.UPPERFunctionName: return Function.UPPER;
				case Function.PROPERFunctionName: return Function.PROPER;
				case Function.LEFTFunctionName: return Function.LEFT;
				case Function.RIGHTFunctionName: return Function.RIGHT;
				case Function.EXACTFunctionName: return Function.EXACT;
				case Function.TRIMFunctionName: return Function.TRIM;
				case Function.REPLACEFunctionName: return Function.REPLACE;
				case Function.SUBSTITUTEFunctionName: return Function.SUBSTITUTE;
				case Function.CODEFunctionName: return Function.CODE;
				case Function.FINDFunctionName: return Function.FIND;
				case Function.CELLFunctionName: return Function.CELL;
				case Function.ISERRFunctionName: return Function.ISERR;
				case Function.ISTEXTFunctionName: return Function.ISTEXT;
				case Function.ISNUMBERFunctionName: return Function.ISNUMBER;
				case Function.ISBLANKFunctionName: return Function.ISBLANK;
				case Function.TFunctionName: return Function.T;
				case Function.NFunctionName: return Function.N;
				case Function.DATEVALUEFunctionName: return Function.DATEVALUE;
				case Function.TIMEVALUEFunctionName: return Function.TIMEVALUE;
				case Function.SLNFunctionName: return Function.SLN;
				case Function.SYDFunctionName: return Function.SYD;
				case Function.DDBFunctionName: return Function.DDB;
				case Function.INDIRECTFunctionName: return Function.INDIRECT;
				case Function.CLEANFunctionName: return Function.CLEAN;
				case Function.MDETERMFunctionName: return Function.MDETERM;
				case Function.MINVERSEFunctionName: return Function.MINVERSE;
				case Function.MMULTFunctionName: return Function.MMULT;
				case Function.IPMTFunctionName: return Function.IPMT;
				case Function.PPMTFunctionName: return Function.PPMT;
				case Function.COUNTAFunctionName: return Function.COUNTA;
				case Function.PRODUCTFunctionName: return Function.PRODUCT;
				case Function.FACTFunctionName: return Function.FACT;
				case Function.DPRODUCTFunctionName: return Function.DPRODUCT;
				case Function.ISNONTEXTFunctionName: return Function.ISNONTEXT;
				case Function.STDEVPFunctionName: return Function.STDEVP;
				case Function.VARPFunctionName: return Function.VARP;
				case Function.DSTDEVPFunctionName: return Function.DSTDEVP;
				case Function.DVARPFunctionName: return Function.DVARP;
				case Function.TRUNCFunctionName: return Function.TRUNC;
				case Function.ISLOGICALFunctionName: return Function.ISLOGICAL;
				case Function.DCOUNTAFunctionName: return Function.DCOUNTA;
				case Function.USDOLLARFunctionName: return Function.USDOLLAR;
				case Function.FINDBFunctionName: return Function.FINDB;
				case Function.SEARCHBFunctionName: return Function.SEARCHB;
				case Function.REPLACEBFunctionName: return Function.REPLACEB;
				case Function.LEFTBFunctionName: return Function.LEFTB;
				case Function.RIGHTBFunctionName: return Function.RIGHTB;
				case Function.MIDBFunctionName: return Function.MIDB;
				case Function.LENBFunctionName: return Function.LENB;
				case Function.ROUNDUPFunctionName: return Function.ROUNDUP;
				case Function.ROUNDDOWNFunctionName: return Function.ROUNDDOWN;
				case Function.ASCFunctionName: return Function.ASC;
				case Function.DBSCFunctionName: return Function.DBSC;
				case Function.RANKFunctionName: return Function.RANK;
				case Function.ADDRESSFunctionName: return Function.ADDRESS;
				case Function.DAYS360FunctionName: return Function.DAYS360;
				case Function.TODAYFunctionName: return Function.TODAY;
				case Function.VDBFunctionName: return Function.VDB;
				case Function.MEDIANFunctionName: return Function.MEDIAN;
				case Function.SUMPRODUCTFunctionName: return Function.SUMPRODUCT;
				case Function.SINHFunctionName: return Function.SINH;
				case Function.COSHFunctionName: return Function.COSH;
				case Function.TANHFunctionName: return Function.TANH;
				case Function.ASINHFunctionName: return Function.ASINH;
				case Function.ACOSHFunctionName: return Function.ACOSH;
				case Function.ATANHFunctionName: return Function.ATANH;
				case Function.DGETFunctionName: return Function.DGET;
				case Function.INFOFunctionName: return Function.INFO;
				case Function.DBFunctionName: return Function.DB;
				case Function.FREQUENCYFunctionName: return Function.FREQUENCY;
				case Function.ERROR_TYPEFunctionName: return Function.ERROR_TYPE;
				case Function.AVEDEVFunctionName: return Function.AVEDEV;
				case Function.BETADISTFunctionName: return Function.BETADIST;
				case Function.GAMMALNFunctionName: return Function.GAMMALN;
				case Function.BETAINVFunctionName: return Function.BETAINV;
				case Function.BINOMDISTFunctionName: return Function.BINOMDIST;
				case Function.CHIDISTFunctionName: return Function.CHIDIST;
				case Function.CHIINVFunctionName: return Function.CHIINV;
				case Function.COMBINFunctionName: return Function.COMBIN;
				case Function.CONFIDENCEFunctionName: return Function.CONFIDENCE;
				case Function.CRITBINOMFunctionName: return Function.CRITBINOM;
				case Function.EVENFunctionName: return Function.EVEN;
				case Function.EXPONDISTFunctionName: return Function.EXPONDIST;
				case Function.FDISTFunctionName: return Function.FDIST;
				case Function.FINVFunctionName: return Function.FINV;
				case Function.FISHERFunctionName: return Function.FISHER;
				case Function.FISHERINVFunctionName: return Function.FISHERINV;
				case Function.FLOORFunctionName: return Function.FLOOR;
				case Function.GAMMADISTFunctionName: return Function.GAMMADIST;
				case Function.GAMMAINVFunctionName: return Function.GAMMAINV;
				case Function.CEILINGFunctionName: return Function.CEILING;
				case Function.HYPGEOMVERTFunctionName: return Function.HYPGEOMVERT;
				case Function.LOGNORMDISTFunctionName: return Function.LOGNORMDIST;
				case Function.LOGINVFunctionName: return Function.LOGINV;
				case Function.NEGBINOMDISTFunctionName: return Function.NEGBINOMDIST;
				case Function.NORMDISTFunctionName: return Function.NORMDIST;
				case Function.NORMSDISTFunctionName: return Function.NORMSDIST;
				case Function.NORMINVFunctionName: return Function.NORMINV;
				case Function.MNORMSINVFunctionName: return Function.MNORMSINV;
				case Function.STANDARDIZEFunctionName: return Function.STANDARDIZE;
				case Function.ODDFunctionName: return Function.ODD;
				case Function.PERMUTFunctionName: return Function.PERMUT;
				case Function.POISSONFunctionName: return Function.POISSON;
				case Function.TDISTFunctionName: return Function.TDIST;
				case Function.WEIBULLFunctionName: return Function.WEIBULL;
				case Function.SUMXMY2FunctionName: return Function.SUMXMY2;
				case Function.SUMX2MY2FunctionName: return Function.SUMX2MY2;
				case Function.SUMX2PY2FunctionName: return Function.SUMX2PY2;
				case Function.CHITESTFunctionName: return Function.CHITEST;
				case Function.CORRELFunctionName: return Function.CORREL;
				case Function.COVARFunctionName: return Function.COVAR;
				case Function.FORECASTFunctionName: return Function.FORECAST;
				case Function.FTESTFunctionName: return Function.FTEST;
				case Function.INTERCEPTFunctionName: return Function.INTERCEPT;
				case Function.PEARSONFunctionName: return Function.PEARSON;
				case Function.RSQFunctionName: return Function.RSQ;
				case Function.STEYXFunctionName: return Function.STEYX;
				case Function.SLOPEFunctionName: return Function.SLOPE;
				case Function.TTESTFunctionName: return Function.TTEST;
				case Function.PROBFunctionName: return Function.PROB;
				case Function.DEVSQFunctionName: return Function.DEVSQ;
				case Function.GEOMEANFunctionName: return Function.GEOMEAN;
				case Function.HARMEANFunctionName: return Function.HARMEAN;
				case Function.SUMSQFunctionName: return Function.SUMSQ;
				case Function.KURTFunctionName: return Function.KURT;
				case Function.SKEWFunctionName: return Function.SKEW;
				case Function.ZTESTFunctionName: return Function.ZTEST;
				case Function.LARGEFunctionName: return Function.LARGE;
				case Function.SMALLFunctionName: return Function.SMALL;
				case Function.QUARTILEFunctionName: return Function.QUARTILE;
				case Function.PERCENTILEFunctionName: return Function.PERCENTILE;
				case Function.PERCENTRANKFunctionName: return Function.PERCENTRANK;
				case Function.MODEFunctionName: return Function.MODE;
				case Function.TRIMMEANFunctionName: return Function.TRIMMEAN;
				case Function.TINVFunctionName: return Function.TINV;
				case Function.CONCATENATEFunctionName: return Function.CONCATENATE;
				case Function.POWERFunctionName: return Function.POWER;
				case Function.RADIANSFunctionName: return Function.RADIANS;
				case Function.DEGREESFunctionName: return Function.DEGREES;
				case Function.SUBTOTALFunctionName: return Function.SUBTOTAL;
				case Function.SUMIFFunctionName: return Function.SUMIF;
				case Function.COUNTIFFunctionName: return Function.COUNTIF;
				case Function.COUNTBLANKFunctionName: return Function.COUNTBLANK;
				case Function.ISPMTFunctionName: return Function.ISPMT;
				case Function.DATEDIFFunctionName: return Function.DATEDIF;
				case Function.DATESTRINGFunctionName: return Function.DATESTRING;
				case Function.NUMBERSTRINGFunctionName: return Function.NUMBERSTRING;
				case Function.ROMANFunctionName: return Function.ROMAN;
				case Function.HYPERLINKFunctionName: return Function.HYPERLINK;
				case Function.PHONETICFunctionName: return Function.PHONETIC;
				case Function.AVERAGEAFunctionName: return Function.AVERAGEA;
				case Function.MAXAFunctionName: return Function.MAXA;
				case Function.MINAFunctionName: return Function.MINA;
				case Function.STDEVPAFunctionName: return Function.STDEVPA;
				case Function.VARPAFunctionName: return Function.VARPA;
				case Function.STDEVAFunctionName: return Function.STDEVA;
				case Function.VARAFunctionName: return Function.VARA;

				// MD 10/8/07 - BR27172
				// Added some standard add-in functions
				case Function.ACCRINTFunctionName: return Function.ACCRINT;
				case Function.ACCRINTMFunctionName: return Function.ACCRINTM;
				case Function.AMORDEGRCFunctionName: return Function.AMORDEGRC;
				case Function.AMORLINCFunctionName: return Function.AMORLINC;
				case Function.AVERAGEIFFunctionName: return Function.AVERAGEIF;
				case Function.AVERAGEIFSFunctionName: return Function.AVERAGEIFS;
				case Function.BAHTTEXTFunctionName: return Function.BAHTTEXT;
				case Function.BESSELIFunctionName: return Function.BESSELI;
				case Function.BESSELJFunctionName: return Function.BESSELJ;
				case Function.BESSELKFunctionName: return Function.BESSELK;
				case Function.BESSELYFunctionName: return Function.BESSELY;
				case Function.BIN2DECFunctionName: return Function.BIN2DEC;
				case Function.BIN2HEXFunctionName: return Function.BIN2HEX;
				case Function.BIN2OCTFunctionName: return Function.BIN2OCT;
				case Function.COMPLEXFunctionName: return Function.COMPLEX;
				case Function.CONVERTFunctionName: return Function.CONVERT;
				case Function.COUPDAYBSFunctionName: return Function.COUPDAYBS;
				case Function.COUPDAYSFunctionName: return Function.COUPDAYS;
				case Function.COUPDAYSNCFunctionName: return Function.COUPDAYSNC;
				case Function.COUPNCDFunctionName: return Function.COUPNCD;
				case Function.COUPNUMFunctionName: return Function.COUPNUM;
				case Function.COUPPCDFunctionName: return Function.COUPPCD;
				case Function.CUBEKPIMEMBERFunctionName: return Function.CUBEKPIMEMBER;
				case Function.CUBEMEMBERFunctionName: return Function.CUBEMEMBER;
				case Function.CUBEMEMBERPROPERTYFunctionName: return Function.CUBEMEMBERPROPERTY;
				case Function.CUBERANKEDMEMBERFunctionName: return Function.CUBERANKEDMEMBER;
				case Function.CUBESETFunctionName: return Function.CUBESET;
				case Function.CUBESETCOUNTFunctionName: return Function.CUBESETCOUNT;
				case Function.CUBEVALUEFunctionName: return Function.CUBEVALUE;
				case Function.CUMIPMTFunctionName: return Function.CUMIPMT;
				case Function.CUMPRINCFunctionName: return Function.CUMPRINC;
				case Function.DEC2BINFunctionName: return Function.DEC2BIN;
				case Function.DEC2HEXFunctionName: return Function.DEC2HEX;
				case Function.DEC2OCTFunctionName: return Function.DEC2OCT;
				case Function.DELTAFunctionName: return Function.DELTA;
				case Function.DISCFunctionName: return Function.DISC;
				case Function.DOLLARDEFunctionName: return Function.DOLLARDE;
				case Function.DOLLARFRFunctionName: return Function.DOLLARFR;
				case Function.DURATIONFunctionName: return Function.DURATION;
				case Function.EDATEFunctionName: return Function.EDATE;
				case Function.EFFECTFunctionName: return Function.EFFECT;
				case Function.EOMONTHFunctionName: return Function.EOMONTH;
				case Function.ERFFunctionName: return Function.ERF;
				case Function.ERFCFunctionName: return Function.ERFC;
				case Function.FACTDOUBLEFunctionName: return Function.FACTDOUBLE;
				case Function.FVSCHEDULEFunctionName: return Function.FVSCHEDULE;
				case Function.GCDFunctionName: return Function.GCD;
				case Function.GESTEPFunctionName: return Function.GESTEP;
				case Function.GETPIVOTDATAFunctionName: return Function.GETPIVOTDATA;
				case Function.HEX2BINFunctionName: return Function.HEX2BIN;
				case Function.HEX2DECFunctionName: return Function.HEX2DEC;
				case Function.HEX2OCTFunctionName: return Function.HEX2OCT;
				case Function.HYPGEOMDISTFunctionName: return Function.HYPGEOMDIST;
				case Function.IFERRORFunctionName: return Function.IFERROR;								// MD 8/29/11 - TFS85072
				case Function.IMABSFunctionName: return Function.IMABS;
				case Function.IMAGINARYFunctionName: return Function.IMAGINARY;
				case Function.IMARGUMENTFunctionName: return Function.IMARGUMENT;
				case Function.IMCONJUGATEFunctionName: return Function.IMCONJUGATE;
				case Function.IMCOSFunctionName: return Function.IMCOS;
				case Function.IMDIVFunctionName: return Function.IMDIV;
				case Function.IMEXPFunctionName: return Function.IMEXP;
				case Function.IMLNFunctionName: return Function.IMLN;
				case Function.IMLOG10FunctionName: return Function.IMLOG10;
				case Function.IMLOG2FunctionName: return Function.IMLOG2;
				case Function.IMPOWERFunctionName: return Function.IMPOWER;
				case Function.IMPRODUCTFunctionName: return Function.IMPRODUCT;
				case Function.IMREALFunctionName: return Function.IMREAL;
				case Function.IMSINFunctionName: return Function.IMSIN;
				case Function.IMSQRTFunctionName: return Function.IMSQRT;
				case Function.IMSUBFunctionName: return Function.IMSUB;
				case Function.IMSUMFunctionName: return Function.IMSUM;
				case Function.INTRATEFunctionName: return Function.INTRATE;
				case Function.ISEVENFunctionName: return Function.ISEVEN;
				case Function.ISODDFunctionName: return Function.ISODD;
				case Function.LCMFunctionName: return Function.LCM;
				case Function.MDURATIONFunctionName: return Function.MDURATION;
				case Function.MROUNDFunctionName: return Function.MROUND;
				case Function.MULTINOMIALFunctionName: return Function.MULTINOMIAL;
				case Function.NETWORKDAYSFunctionName: return Function.NETWORKDAYS;
				case Function.NOMINALFunctionName: return Function.NOMINAL;
				case Function.NORMSINVFunctionName: return Function.NORMSINV;
				case Function.OCT2BINFunctionName: return Function.OCT2BIN;
				case Function.OCT2DECFunctionName: return Function.OCT2DEC;
				case Function.OCT2HEXFunctionName: return Function.OCT2HEX;
				case Function.ODDFPRICEFunctionName: return Function.ODDFPRICE;
				case Function.ODDFYIELDFunctionName: return Function.ODDFYIELD;
				case Function.ODDLPRICEFunctionName: return Function.ODDLPRICE;
				case Function.ODDLYIELDFunctionName: return Function.ODDLYIELD;
				case Function.PRICEFunctionName: return Function.PRICE;
				case Function.PRICEDISCFunctionName: return Function.PRICEDISC;
				case Function.PRICEMATFunctionName: return Function.PRICEMAT;
				case Function.QUOTIENTFunctionName: return Function.QUOTIENT;
				case Function.RANDBETWEENFunctionName: return Function.RANDBETWEEN;
				case Function.RECEIVEDFunctionName: return Function.RECEIVED;
				case Function.RTDFunctionName: return Function.RTD;
				case Function.SERIESSUMFunctionName: return Function.SERIESSUM;
				case Function.SQRTPIFunctionName: return Function.SQRTPI;
				case Function.SUMIFSFunctionName: return Function.SUMIFS;
				case Function.TBILLEQFunctionName: return Function.TBILLEQ;
				case Function.TBILLPRICEFunctionName: return Function.TBILLPRICE;
				case Function.TBILLYIELDFunctionName: return Function.TBILLYIELD;
				case Function.WEEKNUMFunctionName: return Function.WEEKNUM;
				case Function.WORKDAYFunctionName: return Function.WORKDAY;
				case Function.XIRRFunctionName: return Function.XIRR;
				case Function.XNPVFunctionName: return Function.XNPV;
				case Function.YEARFRACFunctionName: return Function.YEARFRAC;
				case Function.YIELDFunctionName: return Function.YIELD;
				case Function.YIELDDISCFunctionName: return Function.YIELDDISC;
				case Function.YIELDMATFunctionName: return Function.YIELDMAT;

				default:
					// MD 2/4/11 - TFS65015
					// In the 2007 format, function names beginning with "_xll." are add-in functions.
					if (name.StartsWith("_xll."))
						return Function.GetUnknownAddInFunction(name.Substring(5));

					return null;
			}
		}

		#endregion GetFunction

		#region GetFunction

		private static Function GetFunction(
			string name,
			int functionID,
			byte minParameters,
            // MBS 7/10/08 - Excel 2007 Format
            // Changed to an int because we need to allow -1
			//byte maxParameters,
            int maxParameters,
			bool isVolatile,
			int tuplesStart,
			int tuplesDegree,
			int[] forcedReferenceIndices,
			TokenClass returnClass,
			params TokenClass[] paramClasses )
		{
			// MD 10/8/07 - BR27172
			// Refactored the way to get the desired function
			#region Refactored

			//Function function;
			//
			//if ( functions.ContainsKey( functionID ) )
			//{
			//    function = functions[ functionID ];
			//    Debug.Assert( function.name == name );
			//}
			//else
			//{
			//    function = new Function( name, functionID, minParameters, maxParameters, isVolatile, returnClass, paramClasses );
			//    functions.Add( functionID, function );
			//}

			#endregion Refactored

			// Create a new function instance for binary search purposes (it will also be used if the function does not exist)
			//Function function = new Function( name, functionID, minParameters, maxParameters, isVolatile, returnClass, paramClasses );
			Function function = new Function( 
				name, 
				functionID, 
				minParameters, 
				maxParameters, 
				isVolatile, 
				tuplesStart, 
				tuplesDegree, 
				forcedReferenceIndices, 
				returnClass, 
				paramClasses );

			// MD 4/18/08 - BR32154
			// Use property instead of field. See notes on field.
			//int index = functions.BinarySearch( function );
			int index = Function.Functions.BinarySearch( function );

			// If the function exists, return the existing one
			if ( index >= 0 )
			{
				// MD 4/18/08 - BR32154
				// Use property instead of field. See notes on field.
				//return functions[ index ];
				return Function.Functions[ index ];
			}

			// Otherwise, insert the created function and return it
			// MD 4/18/08 - BR32154
			// Use property instead of field. See notes on field.
			//functions.Insert( ~index, function );
			Function.Functions.Insert( ~index, function );

			return function;
		}

		#endregion GetFunction

		// 8/4/08 - Excel formula solving
		#region GetUnknownAddInFunction

		public static Function GetUnknownAddInFunction( string name )
		{
			Function function = Function.GetFunction( name, Function.AddInFunctionID, 0, -1, false, 0, 1, null, TokenClass.Reference, TokenClass.Reference );
			function.isUnknownAddInFunction = true;

			// MD 4/6/12 - TFS102169
			// The function now stores the workbook path.
			function.workbookPath = AddInFunctionsWorkbookReference.AddInFunctionsWorkbookName;

			return function;
		} 

		#endregion GetUnknownAddInFunction

		#region Functions

		// MD 10/9/07 - BR27172
		// These definitions were all changed for the following 2 reasons:
		// 1. Literals were replace with constants (function name and function id)
		// 2. Extra parameters were added to the GetFunction overload (tuplesStart, tuplesDegree, and forcedReferenceIndices)
        //
        // MBS 7/10/08 - Excel 2007 Format
        // Replaced all MaxParameter arguments of 29/30 with -1, since we need to know that the function can take
        // the maximum number of arguments allowed by the version of Excel
		internal static Function COUNT 				{ get { return Function.GetFunction( Function.COUNTFunctionName, 				Function.COUNTFunctionID, 			0,	-1,		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function IF 				{ get { return Function.GetFunction( Function.IFFunctionName, 					Function.IFFunctionID, 				2, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Value, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ISNA 				{ get { return Function.GetFunction( Function.ISNAFunctionName, 				Function.ISNAFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ISERROR 			{ get { return Function.GetFunction( Function.ISERRORFunctionName, 				Function.ISERRORFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SUM 				{ get { return Function.GetFunction( Function.SUMFunctionName, 					Function.SUMFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function AVERAGE 			{ get { return Function.GetFunction( Function.AVERAGEFunctionName, 				Function.AVERAGEFunctionID, 		1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function MIN 				{ get { return Function.GetFunction( Function.MINFunctionName, 					Function.MINFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function MAX 				{ get { return Function.GetFunction( Function.MAXFunctionName, 					Function.MAXFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function ROW 				{ get { return Function.GetFunction( Function.ROWFunctionName, 					Function.ROWFunctionID, 			0, 	1, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function COLUMN 			{ get { return Function.GetFunction( Function.COLUMNFunctionName, 				Function.COLUMNFunctionID, 			0, 	1, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function NA 				{ get { return Function.GetFunction( Function.NAFunctionName, 					Function.NAFunctionID, 				0, 	0, 		false, 	0, 1, null, TokenClass.Value ); } }
		internal static Function NPV 				{ get { return Function.GetFunction( Function.NPVFunctionName, 					Function.NPVFunctionID, 			2, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function STDEV 				{ get { return Function.GetFunction( Function.STDEVFunctionName, 				Function.STDEVFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function DOLLAR 			{ get { return Function.GetFunction( Function.DOLLARFunctionName, 				Function.DOLLARFunctionID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		// 8/5/08 - Excel formula solving
		// Min params was incorrect.
		//internal static Function FIXED 			{ get { return Function.GetFunction( Function.FIXEDFunctionName, 				Function.FIXEDFunctionID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FIXED 				{ get { return Function.GetFunction( Function.FIXEDFunctionName, 				Function.FIXEDFunctionID, 			1, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SIN 				{ get { return Function.GetFunction( Function.SINFunctionName, 					Function.SINFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function COS 				{ get { return Function.GetFunction( Function.COSFunctionName, 					Function.COSFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TAN 				{ get { return Function.GetFunction( Function.TANFunctionName, 					Function.TANFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ATAN 				{ get { return Function.GetFunction( Function.ATANFunctionName, 				Function.ATANFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function PI 				{ get { return Function.GetFunction( Function.PIFunctionName, 					Function.PIFunctionID, 				0, 	0, 		false, 	0, 1, null, TokenClass.Value ); } }
		internal static Function SQRT 				{ get { return Function.GetFunction( Function.SQRTFunctionName, 				Function.SQRTFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function EXP 				{ get { return Function.GetFunction( Function.EXPFunctionName, 					Function.EXPFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LN 				{ get { return Function.GetFunction( Function.LNFunctionName, 					Function.LNFunctionID, 				1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LOG10 				{ get { return Function.GetFunction( Function.LOG10FunctionName, 				Function.LOG10FunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ABS 				{ get { return Function.GetFunction( Function.ABSFunctionName, 					Function.ABSFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function INT 				{ get { return Function.GetFunction( Function.INTFunctionName, 					Function.INTFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SIGN 				{ get { return Function.GetFunction( Function.SIGNFunctionName, 				Function.SIGNFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ROUND 				{ get { return Function.GetFunction( Function.ROUNDFunctionName, 				Function.ROUNDFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LOOKUP 			{ get { return Function.GetFunction( Function.LOOKUPFunctionName, 				Function.LOOKUPFunctionID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function INDEX 				{ get { return Function.GetFunction( Function.INDEXFunctionName, 				Function.INDEXFunctionID, 			2, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function REPT 				{ get { return Function.GetFunction( Function.REPTFunctionName, 				Function.REPTFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function MID 				{ get { return Function.GetFunction( Function.MIDFunctionName, 					Function.MIDFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LEN 				{ get { return Function.GetFunction( Function.LENFunctionName, 					Function.LENFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function VALUE 				{ get { return Function.GetFunction( Function.VALUEFunctionName, 				Function.VALUEFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TRUE 				{ get { return Function.GetFunction( Function.TRUEFunctionName, 				Function.TRUEFunctionID, 			0, 	0, 		false, 	0, 1, null, TokenClass.Value ); } }
		internal static Function FALSE 				{ get { return Function.GetFunction( Function.FALSEFunctionName, 				Function.FALSEFunctionID, 			0, 	0, 		false, 	0, 1, null, TokenClass.Value ); } }
		internal static Function AND 				{ get { return Function.GetFunction( Function.ANDFunctionName, 					Function.ANDFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function OR 				{ get { return Function.GetFunction( Function.ORFunctionName, 					Function.ORFunctionID, 				1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function NOT 				{ get { return Function.GetFunction( Function.NOTFunctionName, 					Function.NOTFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function MOD 				{ get { return Function.GetFunction( Function.MODFunctionName, 					Function.MODFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DCOUNT 			{ get { return Function.GetFunction( Function.DCOUNTFunctionName, 				Function.DCOUNTFunctionID, 			3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DSUM 				{ get { return Function.GetFunction( Function.DSUMFunctionName, 				Function.DSUMFunctionID, 			3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DAVERAGE 			{ get { return Function.GetFunction( Function.DAVERAGEFunctionName, 			Function.DAVERAGEFunctionID, 		3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DMIN 				{ get { return Function.GetFunction( Function.DMINFunctionName, 				Function.DMINFunctionID, 			3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DMAX 				{ get { return Function.GetFunction( Function.DMAXFunctionName, 				Function.DMAXFunctionID, 			3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DSTDEV 			{ get { return Function.GetFunction( Function.DSTDEVFunctionName, 				Function.DSTDEVFunctionID, 			3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function VAR 				{ get { return Function.GetFunction( Function.VARFunctionName, 					Function.VARFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function DVAR 				{ get { return Function.GetFunction( Function.DVARFunctionName, 				Function.DVARFunctionID, 			3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function TEXT 				{ get { return Function.GetFunction( Function.TEXTFunctionName, 				Function.TEXTFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LINEST 			{ get { return Function.GetFunction( Function.LINESTFunctionName, 				Function.LINESTFunctionID, 			1, 	4, 		false, 	0, 1, null, TokenClass.Array, TokenClass.Reference, TokenClass.Reference, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TREND 				{ get { return Function.GetFunction( Function.TRENDFunctionName, 				Function.TRENDFunctionID, 			1, 	4, 		false, 	0, 1, null, TokenClass.Array, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function LOGEST 			{ get { return Function.GetFunction( Function.LOGESTFunctionName, 				Function.LOGESTFunctionID, 			1, 	4, 		false, 	0, 1, null, TokenClass.Array, TokenClass.Reference, TokenClass.Reference, TokenClass.Value, TokenClass.Value ); } }
		internal static Function GROWTH 			{ get { return Function.GetFunction( Function.GROWTHFunctionName, 				Function.GROWTHFunctionID, 			1, 	4, 		false, 	0, 1, null, TokenClass.Array, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function PV 				{ get { return Function.GetFunction( Function.PVFunctionName, 					Function.PVFunctionID, 				3, 	5, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FV 				{ get { return Function.GetFunction( Function.FVFunctionName, 					Function.FVFunctionID, 				3, 	5, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function NPER 				{ get { return Function.GetFunction( Function.NPERFunctionName, 				Function.NPERFunctionID, 			3, 	5, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function PMT 				{ get { return Function.GetFunction( Function.PMTFunctionName, 					Function.PMTFunctionID, 			3, 	5, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function RATE 				{ get { return Function.GetFunction( Function.RATEFunctionName, 				Function.RATEFunctionID, 			3, 	6, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function MIRR 				{ get { return Function.GetFunction( Function.MIRRFunctionName, 				Function.MIRRFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IRR 				{ get { return Function.GetFunction( Function.IRRFunctionName, 					Function.IRRFunctionID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function RAND 				{ get { return Function.GetFunction( Function.RANDFunctionName, 				Function.RANDFunctionID, 			0, 	0, 		true, 	0, 1, null, TokenClass.Value ); } }
		internal static Function MATCH 				{ get { return Function.GetFunction( Function.MATCHFunctionName, 				Function.MATCHFunctionID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DATE 				{ get { return Function.GetFunction( Function.DATEFunctionName, 				Function.DATEFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TIME 				{ get { return Function.GetFunction( Function.TIMEFunctionName, 				Function.TIMEFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DAY 				{ get { return Function.GetFunction( Function.DAYFunctionName, 					Function.DAYFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function MONTH 				{ get { return Function.GetFunction( Function.MONTHFunctionName, 				Function.MONTHFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function YEAR 				{ get { return Function.GetFunction( Function.YEARFunctionName, 				Function.YEARFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function WEEKDAY 			{ get { return Function.GetFunction( Function.WEEKDAYFunctionName, 				Function.WEEKDAYFunctionID, 		1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function HOUR 				{ get { return Function.GetFunction( Function.HOURFunctionName, 				Function.HOURFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function MINUTE 			{ get { return Function.GetFunction( Function.MINUTEFunctionName, 				Function.MINUTEFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SECOND 			{ get { return Function.GetFunction( Function.SECONDFunctionName, 				Function.SECONDFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function NOW 				{ get { return Function.GetFunction( Function.NOWFunctionName, 					Function.NOWFunctionID, 			0, 	0, 		true, 	0, 1, null, TokenClass.Value ); } }
		internal static Function AREAS 				{ get { return Function.GetFunction( Function.AREASFunctionName, 				Function.AREASFunctionID, 			1, 	1, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function ROWS 				{ get { return Function.GetFunction( Function.ROWSFunctionName, 				Function.ROWSFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function COLUMNS 			{ get { return Function.GetFunction( Function.COLUMNSFunctionName, 				Function.COLUMNSFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function OFFSET 			{ get { return Function.GetFunction( Function.OFFSETFunctionName, 				Function.OFFSETFunctionID, 			3, 	5, 		true, 	0, 1, new int[] { 0 }, TokenClass.Reference, TokenClass.Reference, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SEARCH 			{ get { return Function.GetFunction( Function.SEARCHFunctionName, 				Function.SEARCHFunctionID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TRANSPOSE 			{ get { return Function.GetFunction( Function.TRANSPOSEFunctionName, 			Function.TRANSPOSEFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Array, TokenClass.Array ); } }
		internal static Function TYPE 				{ get { return Function.GetFunction( Function.TYPEFunctionName, 				Function.TYPEFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ATAN2 				{ get { return Function.GetFunction( Function.ATAN2FunctionName, 				Function.ATAN2FunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ASIN 				{ get { return Function.GetFunction( Function.ASINFunctionName, 				Function.ASINFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ACOS 				{ get { return Function.GetFunction( Function.ACOSFunctionName, 				Function.ACOSFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CHOOSE 			{ get { return Function.GetFunction( Function.CHOOSEFunctionName, 				Function.CHOOSEFunctionID, 			2, 	-1, 	false, 	0, 1, null, TokenClass.Reference, TokenClass.Value, TokenClass.Reference ); } }

		// MD 1/10/12 - TFS99077
		// Changed the first parameter classes of these functions to Value so single cell width/height ranges can be converted to single cells to get the value.
		//internal static Function HLOOKUP 			{ get { return Function.GetFunction( Function.HLOOKUPFunctionName, 				Function.HLOOKUPFunctionID, 		3, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Value ); } }
		//internal static Function VLOOKUP 			{ get { return Function.GetFunction( Function.VLOOKUPFunctionName, 				Function.VLOOKUPFunctionID, 		3, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function HLOOKUP 			{ get { return Function.GetFunction( Function.HLOOKUPFunctionName, 				Function.HLOOKUPFunctionID, 		3, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function VLOOKUP 			{ get { return Function.GetFunction( Function.VLOOKUPFunctionName, 				Function.VLOOKUPFunctionID, 		3, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }

		internal static Function ISREF 				{ get { return Function.GetFunction( Function.ISREFFunctionName, 				Function.ISREFFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function LOG 				{ get { return Function.GetFunction( Function.LOGFunctionName, 					Function.LOGFunctionID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CHAR 				{ get { return Function.GetFunction( Function.CHARFunctionName, 				Function.CHARFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LOWER 				{ get { return Function.GetFunction( Function.LOWERFunctionName, 				Function.LOWERFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function UPPER 				{ get { return Function.GetFunction( Function.UPPERFunctionName, 				Function.UPPERFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function PROPER 			{ get { return Function.GetFunction( Function.PROPERFunctionName, 				Function.PROPERFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LEFT 				{ get { return Function.GetFunction( Function.LEFTFunctionName, 				Function.LEFTFunctionID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function RIGHT 				{ get { return Function.GetFunction( Function.RIGHTFunctionName, 				Function.RIGHTFunctionID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function EXACT 				{ get { return Function.GetFunction( Function.EXACTFunctionName, 				Function.EXACTFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TRIM 				{ get { return Function.GetFunction( Function.TRIMFunctionName, 				Function.TRIMFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function REPLACE 			{ get { return Function.GetFunction( Function.REPLACEFunctionName, 				Function.REPLACEFunctionID, 		4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SUBSTITUTE 		{ get { return Function.GetFunction( Function.SUBSTITUTEFunctionName, 			Function.SUBSTITUTEFunctionID, 		3, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CODE 				{ get { return Function.GetFunction( Function.CODEFunctionName, 				Function.CODEFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FIND 				{ get { return Function.GetFunction( Function.FINDFunctionName, 				Function.FINDFunctionID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CELL 				{ get { return Function.GetFunction( Function.CELLFunctionName, 				Function.CELLFunctionID, 			1, 	2, 		true, 	0, 1, new int[] { 1 }, TokenClass.Value, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function ISERR 				{ get { return Function.GetFunction( Function.ISERRFunctionName, 				Function.ISERRFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ISTEXT 			{ get { return Function.GetFunction( Function.ISTEXTFunctionName, 				Function.ISTEXTFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ISNUMBER 			{ get { return Function.GetFunction( Function.ISNUMBERFunctionName, 			Function.ISNUMBERFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ISBLANK 			{ get { return Function.GetFunction( Function.ISBLANKFunctionName, 				Function.ISBLANKFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function T 					{ get { return Function.GetFunction( Function.TFunctionName, 					Function.TFunctionID, 				1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function N 					{ get { return Function.GetFunction( Function.NFunctionName, 					Function.NFunctionID, 				1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DATEVALUE 			{ get { return Function.GetFunction( Function.DATEVALUEFunctionName, 			Function.DATEVALUEFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TIMEVALUE 			{ get { return Function.GetFunction( Function.TIMEVALUEFunctionName, 			Function.TIMEVALUEFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SLN 				{ get { return Function.GetFunction( Function.SLNFunctionName, 					Function.SLNFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SYD 				{ get { return Function.GetFunction( Function.SYDFunctionName, 					Function.SYDFunctionID, 			4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DDB 				{ get { return Function.GetFunction( Function.DDBFunctionName, 					Function.DDBFunctionID, 			4, 	5, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function INDIRECT 			{ get { return Function.GetFunction( Function.INDIRECTFunctionName, 			Function.INDIRECTFunctionID, 		1, 	2, 		true, 	0, 1, null, TokenClass.Reference, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CLEAN 				{ get { return Function.GetFunction( Function.CLEANFunctionName, 				Function.CLEANFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function MDETERM 			{ get { return Function.GetFunction( Function.MDETERMFunctionName, 				Function.MDETERMFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array ); } }
		internal static Function MINVERSE 			{ get { return Function.GetFunction( Function.MINVERSEFunctionName, 			Function.MINVERSEFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Array, TokenClass.Array ); } }
		internal static Function MMULT 				{ get { return Function.GetFunction( Function.MMULTFunctionName, 				Function.MMULTFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Array, TokenClass.Array, TokenClass.Array ); } }
		internal static Function IPMT 				{ get { return Function.GetFunction( Function.IPMTFunctionName, 				Function.IPMTFunctionID, 			4, 	6, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function PPMT 				{ get { return Function.GetFunction( Function.PPMTFunctionName, 				Function.PPMTFunctionID, 			4, 	6, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function COUNTA 			{ get { return Function.GetFunction( Function.COUNTAFunctionName, 				Function.COUNTAFunctionID, 			0, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function PRODUCT 			{ get { return Function.GetFunction( Function.PRODUCTFunctionName, 				Function.PRODUCTFunctionID, 		0, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function FACT 				{ get { return Function.GetFunction( Function.FACTFunctionName, 				Function.FACTFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DPRODUCT 			{ get { return Function.GetFunction( Function.DPRODUCTFunctionName, 			Function.DPRODUCTFunctionID, 		3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ISNONTEXT 			{ get { return Function.GetFunction( Function.ISNONTEXTFunctionName, 			Function.ISNONTEXTFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function STDEVP 			{ get { return Function.GetFunction( Function.STDEVPFunctionName, 				Function.STDEVPFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function VARP 				{ get { return Function.GetFunction( Function.VARPFunctionName, 				Function.VARPFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function DSTDEVP 			{ get { return Function.GetFunction( Function.DSTDEVPFunctionName, 				Function.DSTDEVPFunctionID, 		3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DVARP 				{ get { return Function.GetFunction( Function.DVARPFunctionName, 				Function.DVARPFunctionID, 			3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function TRUNC 				{ get { return Function.GetFunction( Function.TRUNCFunctionName, 				Function.TRUNCFunctionID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ISLOGICAL 			{ get { return Function.GetFunction( Function.ISLOGICALFunctionName, 			Function.ISLOGICALFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DCOUNTA 			{ get { return Function.GetFunction( Function.DCOUNTAFunctionName, 				Function.DCOUNTAFunctionID, 		3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function USDOLLAR 			{ get { return Function.GetFunction( Function.USDOLLARFunctionName, 			Function.USDOLLARFunctionID, 		1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FINDB 				{ get { return Function.GetFunction( Function.FINDBFunctionName, 				Function.FINDBFunctionID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SEARCHB 			{ get { return Function.GetFunction( Function.SEARCHBFunctionName, 				Function.SEARCHBFunctionID, 		2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function REPLACEB 			{ get { return Function.GetFunction( Function.REPLACEBFunctionName, 			Function.REPLACEBFunctionID, 		4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LEFTB 				{ get { return Function.GetFunction( Function.LEFTBFunctionName, 				Function.LEFTBFunctionID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function RIGHTB 			{ get { return Function.GetFunction( Function.RIGHTBFunctionName, 				Function.RIGHTBFunctionID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function MIDB 				{ get { return Function.GetFunction( Function.MIDBFunctionName, 				Function.MIDBFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LENB 				{ get { return Function.GetFunction( Function.LENBFunctionName, 				Function.LENBFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ROUNDUP 			{ get { return Function.GetFunction( Function.ROUNDUPFunctionName, 				Function.ROUNDUPFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ROUNDDOWN 			{ get { return Function.GetFunction( Function.ROUNDDOWNFunctionName, 			Function.ROUNDDOWNFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ASC 				{ get { return Function.GetFunction( Function.ASCFunctionName, 					Function.ASCFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DBSC 				{ get { return Function.GetFunction( Function.DBSCFunctionName, 				Function.DBSCFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function RANK 				{ get { return Function.GetFunction( Function.RANKFunctionName, 				Function.RANKFunctionID, 			2, 	3, 		false, 	0, 1, new int[] { 1 }, TokenClass.Value, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function ADDRESS 			{ get { return Function.GetFunction( Function.ADDRESSFunctionName, 				Function.ADDRESSFunctionID, 		2, 	5, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DAYS360 			{ get { return Function.GetFunction( Function.DAYS360FunctionName, 				Function.DAYS360FunctionID, 		2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TODAY 				{ get { return Function.GetFunction( Function.TODAYFunctionName, 				Function.TODAYFunctionID, 			0, 	0, 		true, 	0, 1, null, TokenClass.Value ); } }
		internal static Function VDB 				{ get { return Function.GetFunction( Function.VDBFunctionName, 					Function.VDBFunctionID, 			5, 	7, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function MEDIAN 			{ get { return Function.GetFunction( Function.MEDIANFunctionName, 				Function.MEDIANFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function SUMPRODUCT 		{ get { return Function.GetFunction( Function.SUMPRODUCTFunctionName, 			Function.SUMPRODUCTFunctionID, 		1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Array ); } }
		internal static Function SINH 				{ get { return Function.GetFunction( Function.SINHFunctionName, 				Function.SINHFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function COSH 				{ get { return Function.GetFunction( Function.COSHFunctionName, 				Function.COSHFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TANH 				{ get { return Function.GetFunction( Function.TANHFunctionName, 				Function.TANHFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ASINH 				{ get { return Function.GetFunction( Function.ASINHFunctionName, 				Function.ASINHFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ACOSH 				{ get { return Function.GetFunction( Function.ACOSHFunctionName, 				Function.ACOSHFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ATANH 				{ get { return Function.GetFunction( Function.ATANHFunctionName, 				Function.ATANHFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DGET 				{ get { return Function.GetFunction( Function.DGETFunctionName, 				Function.DGETFunctionID, 			3, 	3, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function INFO 				{ get { return Function.GetFunction( Function.INFOFunctionName, 				Function.INFOFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DB 				{ get { return Function.GetFunction( Function.DBFunctionName, 					Function.DBFunctionID, 				4, 	5, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FREQUENCY 			{ get { return Function.GetFunction( Function.FREQUENCYFunctionName, 			Function.FREQUENCYFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Array, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ERROR_TYPE 		{ get { return Function.GetFunction( Function.ERROR_TYPEFunctionName, 			Function.ERROR_TYPEFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function AVEDEV 			{ get { return Function.GetFunction( Function.AVEDEVFunctionName, 				Function.AVEDEVFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function BETADIST 			{ get { return Function.GetFunction( Function.BETADISTFunctionName, 			Function.BETADISTFunctionID, 		3, 	5, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function GAMMALN 			{ get { return Function.GetFunction( Function.GAMMALNFunctionName, 				Function.GAMMALNFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function BETAINV 			{ get { return Function.GetFunction( Function.BETAINVFunctionName, 				Function.BETAINVFunctionID, 		3, 	5, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function BINOMDIST 			{ get { return Function.GetFunction( Function.BINOMDISTFunctionName, 			Function.BINOMDISTFunctionID, 		4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CHIDIST 			{ get { return Function.GetFunction( Function.CHIDISTFunctionName, 				Function.CHIDISTFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CHIINV 			{ get { return Function.GetFunction( Function.CHIINVFunctionName, 				Function.CHIINVFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function COMBIN 			{ get { return Function.GetFunction( Function.COMBINFunctionName, 				Function.COMBINFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CONFIDENCE 		{ get { return Function.GetFunction( Function.CONFIDENCEFunctionName, 			Function.CONFIDENCEFunctionID, 		3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CRITBINOM 			{ get { return Function.GetFunction( Function.CRITBINOMFunctionName, 			Function.CRITBINOMFunctionID, 		3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function EVEN 				{ get { return Function.GetFunction( Function.EVENFunctionName, 				Function.EVENFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function EXPONDIST 			{ get { return Function.GetFunction( Function.EXPONDISTFunctionName, 			Function.EXPONDISTFunctionID, 		3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FDIST 				{ get { return Function.GetFunction( Function.FDISTFunctionName, 				Function.FDISTFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FINV 				{ get { return Function.GetFunction( Function.FINVFunctionName, 				Function.FINVFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FISHER 			{ get { return Function.GetFunction( Function.FISHERFunctionName, 				Function.FISHERFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FISHERINV 			{ get { return Function.GetFunction( Function.FISHERINVFunctionName, 			Function.FISHERINVFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FLOOR 				{ get { return Function.GetFunction( Function.FLOORFunctionName, 				Function.FLOORFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function GAMMADIST 			{ get { return Function.GetFunction( Function.GAMMADISTFunctionName, 			Function.GAMMADISTFunctionID, 		4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function GAMMAINV 			{ get { return Function.GetFunction( Function.GAMMAINVFunctionName, 			Function.GAMMAINVFunctionID, 		3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CEILING 			{ get { return Function.GetFunction( Function.CEILINGFunctionName, 				Function.CEILINGFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function HYPGEOMVERT 		{ get { return Function.GetFunction( Function.HYPGEOMVERTFunctionName, 			Function.HYPGEOMVERTFunctionID, 	4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LOGNORMDIST 		{ get { return Function.GetFunction( Function.LOGNORMDISTFunctionName, 			Function.LOGNORMDISTFunctionID, 	3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function LOGINV 			{ get { return Function.GetFunction( Function.LOGINVFunctionName, 				Function.LOGINVFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function NEGBINOMDIST 		{ get { return Function.GetFunction( Function.NEGBINOMDISTFunctionName, 		Function.NEGBINOMDISTFunctionID, 	3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function NORMDIST 			{ get { return Function.GetFunction( Function.NORMDISTFunctionName, 			Function.NORMDISTFunctionID, 		4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function NORMSDIST 			{ get { return Function.GetFunction( Function.NORMSDISTFunctionName, 			Function.NORMSDISTFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function NORMINV 			{ get { return Function.GetFunction( Function.NORMINVFunctionName, 				Function.NORMINVFunctionID, 		3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function MNORMSINV 			{ get { return Function.GetFunction( Function.MNORMSINVFunctionName, 			Function.MNORMSINVFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function STANDARDIZE 		{ get { return Function.GetFunction( Function.STANDARDIZEFunctionName, 			Function.STANDARDIZEFunctionID, 	3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ODD 				{ get { return Function.GetFunction( Function.ODDFunctionName, 					Function.ODDFunctionID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function PERMUT 			{ get { return Function.GetFunction( Function.PERMUTFunctionName, 				Function.PERMUTFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function POISSON 			{ get { return Function.GetFunction( Function.POISSONFunctionName, 				Function.POISSONFunctionID, 		3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function TDIST 				{ get { return Function.GetFunction( Function.TDISTFunctionName, 				Function.TDISTFunctionID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function WEIBULL 			{ get { return Function.GetFunction( Function.WEIBULLFunctionName, 				Function.WEIBULLFunctionID, 		4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SUMXMY2 			{ get { return Function.GetFunction( Function.SUMXMY2FunctionName, 				Function.SUMXMY2FunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function SUMX2MY2 			{ get { return Function.GetFunction( Function.SUMX2MY2FunctionName, 			Function.SUMX2MY2FunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function SUMX2PY2 			{ get { return Function.GetFunction( Function.SUMX2PY2FunctionName, 			Function.SUMX2PY2FunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function CHITEST 			{ get { return Function.GetFunction( Function.CHITESTFunctionName, 				Function.CHITESTFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function CORREL 			{ get { return Function.GetFunction( Function.CORRELFunctionName, 				Function.CORRELFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function COVAR 				{ get { return Function.GetFunction( Function.COVARFunctionName, 				Function.COVARFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function FORECAST 			{ get { return Function.GetFunction( Function.FORECASTFunctionName, 			Function.FORECASTFunctionID, 		3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function FTEST 				{ get { return Function.GetFunction( Function.FTESTFunctionName, 				Function.FTESTFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function INTERCEPT 			{ get { return Function.GetFunction( Function.INTERCEPTFunctionName, 			Function.INTERCEPTFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function PEARSON 			{ get { return Function.GetFunction( Function.PEARSONFunctionName, 				Function.PEARSONFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function RSQ 				{ get { return Function.GetFunction( Function.RSQFunctionName, 					Function.RSQFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function STEYX 				{ get { return Function.GetFunction( Function.STEYXFunctionName, 				Function.STEYXFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function SLOPE 				{ get { return Function.GetFunction( Function.SLOPEFunctionName, 				Function.SLOPEFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array ); } }
		internal static Function TTEST 				{ get { return Function.GetFunction( Function.TTESTFunctionName, 				Function.TTESTFunctionID, 			4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array, TokenClass.Value, TokenClass.Value ); } }
		internal static Function PROB 				{ get { return Function.GetFunction( Function.PROBFunctionName, 				Function.PROBFunctionID, 			3, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Array, TokenClass.Array, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DEVSQ 				{ get { return Function.GetFunction( Function.DEVSQFunctionName, 				Function.DEVSQFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function GEOMEAN 			{ get { return Function.GetFunction( Function.GEOMEANFunctionName, 				Function.GEOMEANFunctionID, 		1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function HARMEAN 			{ get { return Function.GetFunction( Function.HARMEANFunctionName, 				Function.HARMEANFunctionID, 		1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function SUMSQ 				{ get { return Function.GetFunction( Function.SUMSQFunctionName, 				Function.SUMSQFunctionID, 			0, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function KURT 				{ get { return Function.GetFunction( Function.KURTFunctionName, 				Function.KURTFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function SKEW 				{ get { return Function.GetFunction( Function.SKEWFunctionName, 				Function.SKEWFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function ZTEST 				{ get { return Function.GetFunction( Function.ZTESTFunctionName, 				Function.ZTESTFunctionID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function LARGE 				{ get { return Function.GetFunction( Function.LARGEFunctionName, 				Function.LARGEFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function SMALL 				{ get { return Function.GetFunction( Function.SMALLFunctionName, 				Function.SMALLFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function QUARTILE 			{ get { return Function.GetFunction( Function.QUARTILEFunctionName, 			Function.QUARTILEFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function PERCENTILE 		{ get { return Function.GetFunction( Function.PERCENTILEFunctionName, 			Function.PERCENTILEFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function PERCENTRANK 		{ get { return Function.GetFunction( Function.PERCENTRANKFunctionName, 			Function.PERCENTRANKFunctionID, 	2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function MODE 				{ get { return Function.GetFunction( Function.MODEFunctionName, 				Function.MODEFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Array ); } }
		internal static Function TRIMMEAN 			{ get { return Function.GetFunction( Function.TRIMMEANFunctionName, 			Function.TRIMMEANFunctionID, 		2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function TINV 				{ get { return Function.GetFunction( Function.TINVFunctionName, 				Function.TINVFunctionID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function CONCATENATE 		{ get { return Function.GetFunction( Function.CONCATENATEFunctionName, 			Function.CONCATENATEFunctionID, 	0, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function POWER 				{ get { return Function.GetFunction( Function.POWERFunctionName, 				Function.POWERFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function RADIANS 			{ get { return Function.GetFunction( Function.RADIANSFunctionName, 				Function.RADIANSFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DEGREES 			{ get { return Function.GetFunction( Function.DEGREESFunctionName, 				Function.DEGREESFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
        internal static Function SUBTOTAL           { get { return Function.GetFunction( Function.SUBTOTALFunctionName,             Function.SUBTOTALFunctionID,        2, 	-1, false, 0, 1, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254 }, TokenClass.Value, TokenClass.Value, TokenClass.Reference); } }
		internal static Function SUMIF 				{ get { return Function.GetFunction( Function.SUMIFFunctionName, 				Function.SUMIFFunctionID, 			2, 	3, 		false, 	0, 1, new int[] { 0, 2 }, TokenClass.Value, TokenClass.Reference, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function COUNTIF 			{ get { return Function.GetFunction( Function.COUNTIFFunctionName, 				Function.COUNTIFFunctionID, 		2, 	2, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function COUNTBLANK 		{ get { return Function.GetFunction( Function.COUNTBLANKFunctionName, 			Function.COUNTBLANKFunctionID, 		1, 	1, 		false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function ISPMT 				{ get { return Function.GetFunction( Function.ISPMTFunctionName, 				Function.ISPMTFunctionID, 			4, 	4, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DATEDIF 			{ get { return Function.GetFunction( Function.DATEDIFFunctionName, 				Function.DATEDIFFunctionID, 		3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DATESTRING 		{ get { return Function.GetFunction( Function.DATESTRINGFunctionName, 			Function.DATESTRINGFunctionID, 		1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function NUMBERSTRING 		{ get { return Function.GetFunction( Function.NUMBERSTRINGFunctionName, 		Function.NUMBERSTRINGFunctionID, 	2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ROMAN 				{ get { return Function.GetFunction( Function.ROMANFunctionName, 				Function.ROMANFunctionID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function GETPIVOTDATA 		{ get { return Function.GetFunction( Function.GETPIVOTDATAFunctionName, 		Function.GETPIVOTDATAFunctionID,	2, 	-1, 	false, 	2, 2, null, TokenClass.Reference, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function HYPERLINK 			{ get { return Function.GetFunction( Function.HYPERLINKFunctionName, 			Function.HYPERLINKFunctionID, 		1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function PHONETIC 			{ get { return Function.GetFunction( Function.PHONETICFunctionName, 			Function.PHONETICFunctionID, 		1, 	-1, 	false, 	0, 1, new int[] { 0 }, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function AVERAGEA 			{ get { return Function.GetFunction( Function.AVERAGEAFunctionName, 			Function.AVERAGEAFunctionID, 		1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function MAXA 				{ get { return Function.GetFunction( Function.MAXAFunctionName, 				Function.MAXAFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function MINA 				{ get { return Function.GetFunction( Function.MINAFunctionName, 				Function.MINAFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function STDEVPA 			{ get { return Function.GetFunction( Function.STDEVPAFunctionName, 				Function.STDEVPAFunctionID, 		1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function VARPA 				{ get { return Function.GetFunction( Function.VARPAFunctionName, 				Function.VARPAFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function STDEVA 			{ get { return Function.GetFunction( Function.STDEVAFunctionName, 				Function.STDEVAFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function VARA 				{ get { return Function.GetFunction( Function.VARAFunctionName, 				Function.VARAFunctionID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		
		// MD 10/8/07 - BR27172
		// Added some standard add-in functions and a generic add-in function to represent them all when 
		// loading a workbook with add-in functions used
		internal static Function AddInFunction		{ get { return Function.GetFunction( "<AddIn>",									Function.AddInFunctionID,			0,	-1,		false,	0, 1, null,	TokenClass.Reference,		TokenClass.Reference ); } }
		internal static Function ACCRINT			{ get { return Function.GetFunction( Function.ACCRINTFunctionName, 				Function.Excel2007OnlyID, 			6, 	8, 		false,	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ACCRINTM			{ get { return Function.GetFunction( Function.ACCRINTMFunctionName, 			Function.Excel2007OnlyID, 			4, 	5, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function AMORDEGRC			{ get { return Function.GetFunction( Function.AMORDEGRCFunctionName, 			Function.Excel2007OnlyID, 			6, 	7, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function AMORLINC			{ get { return Function.GetFunction( Function.AMORLINCFunctionName, 			Function.Excel2007OnlyID, 			6, 	7, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function AVERAGEIF			{ get { return Function.GetFunction( Function.AVERAGEIFFunctionName, 			Function.Excel2007OnlyID, 			2, 	3, 		false, 	0, 1, new int[] { 0, 2 }, TokenClass.Value, TokenClass.Reference, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function AVERAGEIFS			{ get { return Function.GetFunction( Function.AVERAGEIFSFunctionName, 			Function.Excel2007OnlyID, 			3, 	-1, 	false, 	3, 2, new int[] { 0, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37, 39, 41, 43, 45, 47, 49, 51, 53, 55, 57, 59, 61, 63, 65, 67, 69, 71, 73, 75, 77, 79, 81, 83, 85, 87, 89, 91, 93, 95, 97, 99, 101, 103, 105, 107, 109, 111, 113, 115, 117, 119, 121, 123, 125, 127, 129, 131, 133, 135, 137, 139, 141, 143, 145, 147, 149, 151, 153, 155, 157, 159, 161, 163, 165, 167, 169, 171, 173, 175, 177, 179, 181, 183, 185, 187, 189, 191, 193, 195, 197, 199, 201, 203, 205, 207, 209, 211, 213, 215, 217, 219, 221, 223, 225, 227, 229, 231, 233, 235, 237, 239, 241, 243, 245, 247, 249, 251 }, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function BAHTTEXT			{ get { return Function.GetFunction( Function.BAHTTEXTFunctionName, 			Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function BESSELI			{ get { return Function.GetFunction( Function.BESSELIFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function BESSELJ			{ get { return Function.GetFunction( Function.BESSELJFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function BESSELK			{ get { return Function.GetFunction( Function.BESSELKFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function BESSELY			{ get { return Function.GetFunction( Function.BESSELYFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function BIN2DEC			{ get { return Function.GetFunction( Function.BIN2DECFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function BIN2HEX			{ get { return Function.GetFunction( Function.BIN2HEXFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function BIN2OCT			{ get { return Function.GetFunction( Function.BIN2OCTFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function COMPLEX			{ get { return Function.GetFunction( Function.COMPLEXFunctionName, 				Function.Excel2007OnlyID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CONVERT			{ get { return Function.GetFunction( Function.CONVERTFunctionName, 				Function.Excel2007OnlyID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function COUPDAYBS			{ get { return Function.GetFunction( Function.COUPDAYBSFunctionName, 			Function.Excel2007OnlyID, 			3, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function COUPDAYS			{ get { return Function.GetFunction( Function.COUPDAYSFunctionName, 			Function.Excel2007OnlyID, 			3, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function COUPDAYSNC			{ get { return Function.GetFunction( Function.COUPDAYSNCFunctionName, 			Function.Excel2007OnlyID, 			3, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function COUPNCD			{ get { return Function.GetFunction( Function.COUPNCDFunctionName, 				Function.Excel2007OnlyID, 			3, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function COUPNUM			{ get { return Function.GetFunction( Function.COUPNUMFunctionName, 				Function.Excel2007OnlyID, 			3, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function COUPPCD			{ get { return Function.GetFunction( Function.COUPPCDFunctionName, 				Function.Excel2007OnlyID, 			3, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CUBEKPIMEMBER		{ get { return Function.GetFunction( Function.CUBEKPIMEMBERFunctionName, 		Function.Excel2007OnlyID, 			3, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CUBEMEMBER			{ get { return Function.GetFunction( Function.CUBEMEMBERFunctionName, 			Function.Excel2007OnlyID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CUBEMEMBERPROPERTY	{ get { return Function.GetFunction( Function.CUBEMEMBERPROPERTYFunctionName,	Function.Excel2007OnlyID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CUBERANKEDMEMBER	{ get { return Function.GetFunction( Function.CUBERANKEDMEMBERFunctionName, 	Function.Excel2007OnlyID, 			3, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CUBESET			{ get { return Function.GetFunction( Function.CUBESETFunctionName, 				Function.Excel2007OnlyID, 			2, 	5, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CUBESETCOUNT		{ get { return Function.GetFunction( Function.CUBESETCOUNTFunctionName, 		Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CUBEVALUE			{ get { return Function.GetFunction( Function.CUBEVALUEFunctionName, 			Function.Excel2007OnlyID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CUMIPMT			{ get { return Function.GetFunction( Function.CUMIPMTFunctionName, 				Function.Excel2007OnlyID, 			6, 	6, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function CUMPRINC			{ get { return Function.GetFunction( Function.CUMPRINCFunctionName, 			Function.Excel2007OnlyID, 			6, 	6, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DEC2BIN			{ get { return Function.GetFunction( Function.DEC2BINFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DEC2HEX			{ get { return Function.GetFunction( Function.DEC2HEXFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DEC2OCT			{ get { return Function.GetFunction( Function.DEC2OCTFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DELTA				{ get { return Function.GetFunction( Function.DELTAFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function DISC				{ get { return Function.GetFunction( Function.DISCFunctionName, 				Function.Excel2007OnlyID, 			4, 	5, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DOLLARDE			{ get { return Function.GetFunction( Function.DOLLARDEFunctionName, 			Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DOLLARFR			{ get { return Function.GetFunction( Function.DOLLARFRFunctionName, 			Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function DURATION			{ get { return Function.GetFunction( Function.DURATIONFunctionName, 			Function.Excel2007OnlyID, 			5, 	6, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function EDATE				{ get { return Function.GetFunction( Function.EDATEFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function EFFECT				{ get { return Function.GetFunction( Function.EFFECTFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function EOMONTH			{ get { return Function.GetFunction( Function.EOMONTHFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ERF				{ get { return Function.GetFunction( Function.ERFFunctionName, 					Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ERFC				{ get { return Function.GetFunction( Function.ERFCFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function FACTDOUBLE			{ get { return Function.GetFunction( Function.FACTDOUBLEFunctionName, 			Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function FVSCHEDULE			{ get { return Function.GetFunction( Function.FVSCHEDULEFunctionName, 			Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function GCD				{ get { return Function.GetFunction( Function.GCDFunctionName, 					Function.Excel2007OnlyID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function GESTEP 			{ get { return Function.GetFunction( Function.GESTEPFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function HEX2BIN 			{ get { return Function.GetFunction( Function.HEX2BINFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function HEX2DEC 			{ get { return Function.GetFunction( Function.HEX2DECFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function HEX2OCT 			{ get { return Function.GetFunction( Function.HEX2OCTFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function HYPGEOMDIST 		{ get { return Function.GetFunction( Function.HYPGEOMDISTFunctionName, 			Function.Excel2007OnlyID, 			4, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }

		// MD 8/29/11 - TFS85072
		internal static Function IFERROR 			{ get { return Function.GetFunction( Function.IFERRORFunctionName, 				Function.Excel2007OnlyID,			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Value, TokenClass.Reference ); } }

		internal static Function IMABS 				{ get { return Function.GetFunction( Function.IMABSFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMAGINARY 			{ get { return Function.GetFunction( Function.IMAGINARYFunctionName, 			Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMARGUMENT 		{ get { return Function.GetFunction( Function.IMARGUMENTFunctionName, 			Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMCONJUGATE 		{ get { return Function.GetFunction( Function.IMCONJUGATEFunctionName, 			Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMCOS 				{ get { return Function.GetFunction( Function.IMCOSFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMDIV 				{ get { return Function.GetFunction( Function.IMDIVFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMEXP 				{ get { return Function.GetFunction( Function.IMEXPFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMLN 				{ get { return Function.GetFunction( Function.IMLNFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMLOG10 			{ get { return Function.GetFunction( Function.IMLOG10FunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMLOG2 			{ get { return Function.GetFunction( Function.IMLOG2FunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMPOWER 			{ get { return Function.GetFunction( Function.IMPOWERFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMPRODUCT 			{ get { return Function.GetFunction( Function.IMPRODUCTFunctionName, 			Function.Excel2007OnlyID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function IMREAL 			{ get { return Function.GetFunction( Function.IMREALFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMSIN 				{ get { return Function.GetFunction( Function.IMSINFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMSQRT 			{ get { return Function.GetFunction( Function.IMSQRTFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMSUB 				{ get { return Function.GetFunction( Function.IMSUBFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function IMSUM 				{ get { return Function.GetFunction( Function.IMSUMFunctionName, 				Function.Excel2007OnlyID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function INTRATE 			{ get { return Function.GetFunction( Function.INTRATEFunctionName, 				Function.Excel2007OnlyID, 			4, 	5, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ISEVEN 			{ get { return Function.GetFunction( Function.ISEVENFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ISODD 				{ get { return Function.GetFunction( Function.ISODDFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function LCM 				{ get { return Function.GetFunction( Function.LCMFunctionName, 					Function.Excel2007OnlyID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function MDURATION 			{ get { return Function.GetFunction( Function.MDURATIONFunctionName, 			Function.Excel2007OnlyID, 			5, 	6, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function MROUND 			{ get { return Function.GetFunction( Function.MROUNDFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Value ); } }
		internal static Function MULTINOMIAL 		{ get { return Function.GetFunction( Function.MULTINOMIALFunctionName, 			Function.Excel2007OnlyID, 			1, 	-1, 	false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function NETWORKDAYS 		{ get { return Function.GetFunction( Function.NETWORKDAYSFunctionName, 			Function.Excel2007OnlyID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function NOMINAL 			{ get { return Function.GetFunction( Function.NOMINALFunctionName, 				Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function NORMSINV 			{ get { return Function.GetFunction( Function.NORMSINVFunctionName, 			Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function OCT2BIN 			{ get { return Function.GetFunction( Function.OCT2BINFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function OCT2DEC 			{ get { return Function.GetFunction( Function.OCT2DECFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function OCT2HEX 			{ get { return Function.GetFunction( Function.OCT2HEXFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function ODDFPRICE 			{ get { return Function.GetFunction( Function.ODDFPRICEFunctionName, 			Function.Excel2007OnlyID, 			8, 	9, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ODDFYIELD 			{ get { return Function.GetFunction( Function.ODDFYIELDFunctionName, 			Function.Excel2007OnlyID, 			8, 	9, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ODDLPRICE 			{ get { return Function.GetFunction( Function.ODDLPRICEFunctionName, 			Function.Excel2007OnlyID, 			7, 	8, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function ODDLYIELD 			{ get { return Function.GetFunction( Function.ODDLYIELDFunctionName, 			Function.Excel2007OnlyID, 			7, 	8, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function PRICE 				{ get { return Function.GetFunction( Function.PRICEFunctionName, 				Function.Excel2007OnlyID, 			6, 	7, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function PRICEDISC 			{ get { return Function.GetFunction( Function.PRICEDISCFunctionName, 			Function.Excel2007OnlyID, 			4, 	5, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function PRICEMAT 			{ get { return Function.GetFunction( Function.PRICEMATFunctionName, 			Function.Excel2007OnlyID, 			5, 	6, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function QUOTIENT 			{ get { return Function.GetFunction( Function.QUOTIENTFunctionName, 			Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function RANDBETWEEN 		{ get { return Function.GetFunction( Function.RANDBETWEENFunctionName, 			Function.Excel2007OnlyID, 			2, 	2, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function RECEIVED 			{ get { return Function.GetFunction( Function.RECEIVEDFunctionName, 			Function.Excel2007OnlyID, 			4, 	5, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function RTD 				{ get { return Function.GetFunction( Function.RTDFunctionName, 					Function.Excel2007OnlyID, 			3, 	-1, 	false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function SERIESSUM 			{ get { return Function.GetFunction( Function.SERIESSUMFunctionName, 			Function.Excel2007OnlyID, 			4, 	4, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Value, TokenClass.Value, TokenClass.Value, TokenClass.Reference ); } }
		internal static Function SQRTPI 			{ get { return Function.GetFunction( Function.SQRTPIFunctionName, 				Function.Excel2007OnlyID, 			1, 	1, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function SUMIFS 			{ get { return Function.GetFunction( Function.SUMIFSFunctionName, 				Function.Excel2007OnlyID, 			3, 	-1, 	false, 	3, 2, new int[] { 0, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37, 39, 41, 43, 45, 47, 49, 51, 53, 55, 57, 59, 61, 63, 65, 67, 69, 71, 73, 75, 77, 79, 81, 83, 85, 87, 89, 91, 93, 95, 97, 99, 101, 103, 105, 107, 109, 111, 113, 115, 117, 119, 121, 123, 125, 127, 129, 131, 133, 135, 137, 139, 141, 143, 145, 147, 149, 151, 153, 155, 157, 159, 161, 163, 165, 167, 169, 171, 173, 175, 177, 179, 181, 183, 185, 187, 189, 191, 193, 195, 197, 199, 201, 203, 205, 207, 209, 211, 213, 215, 217, 219, 221, 223, 225, 227, 229, 231, 233, 235, 237, 239, 241, 243, 245, 247, 249, 251 }, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function TBILLEQ 			{ get { return Function.GetFunction( Function.TBILLEQFunctionName, 				Function.Excel2007OnlyID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function TBILLPRICE 		{ get { return Function.GetFunction( Function.TBILLPRICEFunctionName, 			Function.Excel2007OnlyID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function TBILLYIELD 		{ get { return Function.GetFunction( Function.TBILLYIELDFunctionName, 			Function.Excel2007OnlyID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function WEEKNUM 			{ get { return Function.GetFunction( Function.WEEKNUMFunctionName, 				Function.Excel2007OnlyID, 			1, 	2, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function WORKDAY 			{ get { return Function.GetFunction( Function.WORKDAYFunctionName, 				Function.Excel2007OnlyID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Value, TokenClass.Value ); } }
		internal static Function XIRR 				{ get { return Function.GetFunction( Function.XIRRFunctionName, 				Function.Excel2007OnlyID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function XNPV 				{ get { return Function.GetFunction( Function.XNPVFunctionName, 				Function.Excel2007OnlyID, 			3, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function YEARFRAC 			{ get { return Function.GetFunction( Function.YEARFRACFunctionName, 			Function.Excel2007OnlyID, 			2, 	3, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function YIELD 				{ get { return Function.GetFunction( Function.YIELDFunctionName, 				Function.Excel2007OnlyID, 			6, 	7, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function YIELDDISC 			{ get { return Function.GetFunction( Function.YIELDDISCFunctionName, 			Function.Excel2007OnlyID, 			4, 	5, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		internal static Function YIELDMAT 			{ get { return Function.GetFunction( Function.YIELDMATFunctionName, 			Function.Excel2007OnlyID, 			5, 	6, 		false, 	0, 1, null, TokenClass.Reference, TokenClass.Reference ); } }
		
		#endregion Functions

		#endregion Static Members
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