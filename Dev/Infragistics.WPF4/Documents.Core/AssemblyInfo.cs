using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using Infragistics;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyConfiguration(AssemblyRef.Configuration)]
[assembly: AssemblyDescription(AssemblyRef.AssemblyDescriptionBase + " - " + AssemblyRef.Configuration + " Version")]
[assembly: AssemblyTitle(AssemblyRef.AssemblyName + AssemblyRef.ProductTitleSuffix)]
[assembly: AssemblyProduct(AssemblyRef.AssemblyProduct + AssemblyRef.ProductTitleSuffix)]
[assembly: AssemblyCompany( AssemblyVersion.CompanyName )]
[assembly: AssemblyCopyright( "Copyright(c) " + AssemblyVersion.EndCopyrightYear + " Infragistics, Inc." )]
[assembly: AssemblyCulture("")]		
[assembly: System.CLSCompliant(true)]		
[assembly: AllowPartiallyTrustedCallers()]

[assembly: SecurityRules(SecurityRuleSet.Level1)]


// MBS 8/14/09 - NA9.2 DataPresenterIOExporter

//[assembly: System.Windows.Markup.XmlnsPrefix("http://infragistics.com/IO", "igIO")]
//[assembly: System.Windows.Markup.XmlnsDefinition("http://infragistics.com/IO", "Infragistics.Documents.Core")]


// JJD 12/16/02 - FxCop
// Added ComVisible attribute to avoid fxcop violation
[assembly: System.Runtime.InteropServices.ComVisible(true)]

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion( AssemblyVersion.Version )]
[assembly: AssemblyFileVersion( AssemblyVersion.Version )] // AS 7/29/08

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as 
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
// MBS 8/9/07 
// We're now always delay-signing through the project settings
//
//#if DEBUG
//[assembly: AssemblyDelaySign(false)]
//#else
//    [assembly: AssemblyDelaySign(true)]
//#endif
//
//

// JJD 3/19/04 - DNF159 added attribute to prevent looking for satellite
// assemblies when the language is en-US 
[assembly: System.Resources.NeutralResourcesLanguageAttribute("en-US")]

[assembly: System.Resources.SatelliteContractVersion( AssemblyVersion.SatelliteContractVersion )]

class AssemblyRef
{
    // MRS 6/17/2008
	//public const string BaseResourceName = "Infragistics.Documents.Documents.Core.strings";
    public const string BaseResourceName = "Infragistics.Documents.Core.strings";

	internal const string AssemblyName = AssemblyVersion.AssemblyNamePrefix + ".Documents.Core" + AssemblyVersion.AssemblyNameSuffix;
	internal const string AssemblyProduct = AssemblyVersion.AssemblyNamePrefix + ".Documents.Core";
	internal const string AssemblyDescriptionBase = "Infragistics Documents Core class library";

	internal const string Configuration = AssemblyVersion.Configuration;


	internal const string ProductTitleSuffix = "";




    // MRS 6/18/2008 Localized grammatica resources
    //public const string GrammaticaResourceName = "Infragistics.Documents.Documents.Core.UltraCalcEngine.Compiler.Grammatica";
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