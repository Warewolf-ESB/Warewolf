using System;
using System.Security;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;
using Infragistics;


using Infragistics.Windows.Licensing;


#pragma warning disable 436

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyConfiguration(AssemblyRef.Configuration)]
[assembly: AssemblyDescription(AssemblyRef.AssemblyDescriptionBase + " - " + AssemblyRef.Configuration + " Version")]
[assembly: AssemblyTitle(AssemblyRef.AssemblyName + AssemblyRef.ProductTitleSuffix)]
[assembly: AssemblyProduct(AssemblyRef.AssemblyProduct + AssemblyRef.ProductTitleSuffix)]
[assembly: AssemblyCompany(AssemblyVersion.CompanyName)]
[assembly: AssemblyCopyright("Copyright © 2011-" + AssemblyVersion.EndCopyrightYear + " Infragistics, Inc., All Rights Reserved")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9E51EE35-DB44-404B-9BC7-1EDC84B12B0D")]

[assembly: StringResourceLocation(AssemblyRef.BaseResourceName)]



[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: AllowPartiallyTrustedCallers()]

[assembly: ThemeInfo(
	ResourceDictionaryLocation.SourceAssembly, //where theme specific resource dictionaries are located
	//(used if a resource is not found in the page, 
	// or application resource dictionaries)
	ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
	//(used if a resource is not found in the page, 
	// app, or any theme specific resource dictionaries)
)]


//[assembly: XmlnsPrefix("http://infragistics.com/Themes", "igThemes")]
//[assembly: XmlnsDefinition("http://infragistics.com/Themes", "Infragistics.Windows.Themes")]
//[assembly: AssemblyResourceSetLoader(typeof(ScheduleAssemblyResourceSetLoader))]
//[assembly: AssemblyThemeGroupingNameAttribute(ScheduleGeneric.GroupingName)]





[assembly: InfragisticsFeature(FeatureName = "XamCalculationManager", Version = "11.2")]



[assembly: XmlnsPrefix("http://schemas.infragistics.com/xaml", "ig")]
[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml", "Infragistics.Calculations")]
[assembly: XmlnsPrefix("http://schemas.infragistics.com/xaml/primitives", "igPrim")]
[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml/primitives", "Infragistics.Calculations.Primitives")]

//[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml", "Infragistics")]
//[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml", "Infragistics.Controls.Layouts")]
//[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml/primitives", "Infragistics.Controls.Primitives")]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion(AssemblyVersion.Version)]
[assembly: AssemblyFileVersion(AssemblyVersion.Version)]

[assembly: NeutralResourcesLanguageAttribute("en-US")]
[assembly: SatelliteContractVersion(AssemblyVersion.SatelliteContractVersion)]


[assembly: InternalsVisibleTo("InfragisticsWPF4.Controls.CalculationManager.UnitTests.v" + AssemblyVersion.MajorMinor)]
[assembly: InternalsVisibleTo("InfragisticsWPF4.Controls.Interactions.XamFormulaEditor.v" + AssemblyVersion.MajorMinor)]
[assembly: InternalsVisibleTo("InfragisticsWPF4.DataPresenter.CalculationAdapter.v" + AssemblyVersion.MajorMinor)]
[assembly: InternalsVisibleTo("InfragisticsWPF4.Calculations.XamCalculationManager.UnitTests.v" + AssemblyVersion.MajorMinor)]
[assembly: InternalsVisibleTo("Infragistics.WPF.UnitTests")]







class AssemblyRef
{
	public const string BaseResourceName = "Infragistics.Calculations.strings;Infragistics.Calculations.UltraCalcEngine.strings.CalcManager;Infragistics.Calculations.UltraCalcInterfaces.strings.CalcEngine";

	public const string GrammaticaResourceName = "Infragistics.Calculations.UltraCalcEngine.Compiler.Grammatica";



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

	internal const string AssemblyName = "InfragisticsWPF4.Calculations.XamCalculationManager.v" + AssemblyVersion.MajorMinor;
	internal const string AssemblyProduct = "Infragistics NetAdvantage for WPF";
	internal const string AssemblyDescriptionBase = "XamCalculationManager Component for WPF";

	internal const string Configuration = AssemblyVersion.Configuration;



	internal const string ProductTitleSuffix = "";



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