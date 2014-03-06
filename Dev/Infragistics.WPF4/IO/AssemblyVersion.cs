using System;




	namespace Infragistics



{
	/// <summary>
	/// Contains version information for the Infragistics assemblies.
	/// </summary>

	internal static class AssemblyVersion



	{
		/// <summary>
		/// Major.Minor number portion of the assembly version
		/// </summary>
		public const string MajorMinor = "12.1";



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		/// <summary>
		/// Build number portion of the assembly version
		/// </summary>
		public const string Build = "20121";

		// this needs to be bumped after we release anything to the public
		/// <summary>
		/// Revision number portion of the assembly version
		/// </summary>
		public const string Revision = "2107";


		/// <summary>
		/// Assembly version number
		/// </summary>
		public const string Version = MajorMinor + "." + Build + "." + Revision;

// AS 5/13/10
// We don't need this as images are part of the design assembly.
//
//
//        // MRS 11/11/05
//        // In order to support the new true color bitmaps for toolbox icons in Whidbey, 
//        // we need to point to a different file location depending on which framework we
//        // are using. 
//        // This means a constant to determine the location and a change to the ToolboxBitmap 
//        // attribute on every control. 
//        //
//        /// <summary>
//        /// Location of the resources for toolbox bitmaps. 
//        /// </summary>
//#if WPF
//        public const string ToolBoxBitmapFolder = "ToolboxBitmaps.CLR3.";
//#elif !CLR2
//        public const string ToolBoxBitmapFolder = "ToolboxBitmaps.CLR1.";
//#else
//        public const string ToolBoxBitmapFolder = "ToolboxBitmaps.CLR2.";
//#endif

		/// <summary>
		/// Assembly company name
		/// </summary>
		public const string CompanyName = "Infragistics Inc.";

		/// <summary>
		/// End year of assembly copyright
		/// </summary>
		public const string EndCopyrightYear = "2011";

		/// <summary>
		/// The assembly version number for the satellite assemblies.
		/// </summary>
		// AS 7/29/08
		//public const string SatelliteContractVersion = "3.0.5000.0";
		public const string SatelliteContractVersion = MajorMinor + ".0.0";

		/// <summary>
		/// The current build configuration for the assembly.
		/// </summary>



		public const string Configuration = "Release";






		internal const string PublicKeyToken_WPF = "7dd5c3163f2cd0cb";
		internal const string PublicKey_WPF = "002400000480000094000000060200000024000052534131000400000100010001afa6285b0af5cdd03aa2b6fdaf33fc4759cf9cd9bcf8b778ae60b9fcf71fc8126b78dbf930519614013b7999297907dd9c00bcc487a14f4c6733fe9adb96c053f005d7148f1666fcb882a0f9ba4307c85694b3322889dab357ad5cefd72ccc45e1b6973bdd2f15b2a300077b8d9de30739200887c5407c8a68c90345cbc4f1";

		internal const string PublicKeyToken = "7dd5c3163f2cd0cb";
		internal const string PublicKey = "002400000480000094000000060200000024000052534131000400000100010001afa6285b0af5cdd03aa2b6fdaf33fc4759cf9cd9bcf8b778ae60b9fcf71fc8126b78dbf930519614013b7999297907dd9c00bcc487a14f4c6733fe9adb96c053f005d7148f1666fcb882a0f9ba4307c85694b3322889dab357ad5cefd72ccc45e1b6973bdd2f15b2a300077b8d9de30739200887c5407c8a68c90345cbc4f1";



		internal const string AssemblyNamePrefix = "InfragisticsWPF4";




		// what is after the prefix and control name portion of an assembly name





		internal const string AssemblyNameSuffix = ".v" + AssemblyVersion.MajorMinor;


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