#region Using directives

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Resources;
using System.Globalization;
using System.Windows;
using System.Security;
using System.Runtime.InteropServices;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Themes.Internal;
using System.Windows.Markup;
using Infragistics;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyConfiguration(AssemblyRef.Configuration)]
[assembly: AssemblyDescription(AssemblyRef.AssemblyDescriptionBase + " - " + AssemblyRef.Configuration + " Version")]
[assembly: AssemblyTitle(AssemblyRef.AssemblyName + AssemblyRef.ProductTitleSuffix)]
[assembly: AssemblyProduct(AssemblyRef.AssemblyProduct + AssemblyRef.ProductTitleSuffix)]
[assembly: AssemblyCompany(AssemblyVersion.CompanyName)]
[assembly: AssemblyCopyright("Copyright � 2005-" + AssemblyVersion.EndCopyrightYear + " Infragistics, Inc., All Rights Reserved")]
[assembly: AssemblyTrademark("DataPresenter�")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: AllowPartiallyTrustedCallers()]

    [assembly: SecurityRules(SecurityRuleSet.Level1)]

[assembly: XmlnsPrefix("http://infragistics.com/Windows", "igWindows")]
// AS 6/20/08 BR34103
// DynamicResourceString (for which we have a datatemplate in the generic xaml) is
// within the Infragistics.Shared namespace (at least currently).
//
[assembly: XmlnsDefinition("http://infragistics.com/Windows", "Infragistics.Shared")]
// AS 6/18/08
// Added Infragistics.Windows so we can access the Resources class from xaml
[assembly: XmlnsDefinition("http://infragistics.com/Windows", "Infragistics.Windows")]
[assembly: XmlnsDefinition("http://infragistics.com/Windows", "Infragistics.Windows.Controls")]
[assembly: XmlnsDefinition("http://infragistics.com/Windows", "Infragistics.Windows.Controls.Markup")]
[assembly: XmlnsPrefix("http://infragistics.com/Themes", "igThemes")]
[assembly: XmlnsDefinition("http://infragistics.com/Themes", "Infragistics.Windows.Themes")]
[assembly: XmlnsPrefix("http://infragistics.com/Reporting", "igReporting")]
[assembly: XmlnsDefinition("http://infragistics.com/Reporting", "Infragistics.Windows.Reporting")]
[assembly: XmlnsPrefix("http://infragistics.com/Tiles", "igTiles")]
[assembly: XmlnsDefinition("http://infragistics.com/Tiles", "Infragistics.Windows.Tiles")]
[assembly: AssemblyResourceSetLoader(typeof(WindowsAssemblyResourceSetLoader))]

// AS 5/13/10 - SL sharing
[assembly: XmlnsPrefix("http://schemas.infragistics.com/xaml", "ig")]
[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml", "Infragistics.Controls")]
[assembly: XmlnsPrefix("http://schemas.infragistics.com/xaml/primitives", "igPrim")]
[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml/primitives", "Infragistics.Controls.Primitives")]

[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml", "Infragistics")]


// AS 11/6/07 ThemeGroupingName
[assembly: AssemblyThemeGroupingNameAttribute(PrimitivesGeneric.GroupingName)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.SourceAssembly, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]


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
[assembly: AssemblyFileVersion(AssemblyVersion.Version)] // AS 7/29/08

[assembly: System.Resources.NeutralResourcesLanguageAttribute("en-US")]

[assembly: System.Resources.SatelliteContractVersion(AssemblyVersion.SatelliteContractVersion)]

[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".OutlookBar" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Chart" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".DataPresenter" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".DataPresenter.ExcelExporter" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".DataPresenter.WordWriter" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".DockManager" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Editors" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Reporting" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Ribbon" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Tiles" + AssemblyRef.AssemblySuffixFull)]


[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".DataPresenter.CalculationAdapter" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Schedules" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.SchedulesDialogs" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.SchedulesExchangeConnector" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Editors.XamCalendar" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Editors.XamMaskedInput" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo( AssemblyVersion.AssemblyNamePrefix + ".Controls.Editors.XamDateTimeInput" + AssemblyRef.AssemblySuffixFull )]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Schedules.XamGantt" + AssemblyRef.AssemblySuffixFull)]

[assembly: InternalsVisibleTo( AssemblyVersion.AssemblyNamePrefix + ".Controls.Layouts.XamTileManager" + AssemblyRef.AssemblySuffixFull )]

// JM 5/18/11 - XamComboEditor Port from SL
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Editors.XamComboEditor" + AssemblyRef.AssemblySuffixFull)]

// AS 5/13/10 - SL sharing
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".DataManager" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Menus.XamDataTree" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Editors.XamColorPicker" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Interactions.XamDialogWindow" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Menus.XamMenu" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Editors.XamSlider" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Interactions.XamSpellChecker" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Menus.XamTagCloud" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Menus.XamTree" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Olap" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Grids.XamPivotGrid" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Charts.XamTreemap" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Timelines.XamTimeline" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Maps.XamMap" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Charts.XamGauge" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Charts.XamDataChart" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Barcodes" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".DataVisualization" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo( AssemblyVersion.AssemblyNamePrefix + ".Controls.Editors.XamMaskedInput" + AssemblyRef.AssemblySuffixFull_SL )]
[assembly: InternalsVisibleTo( AssemblyVersion.AssemblyNamePrefix + ".Controls.Editors.XamDateTimeInput" + AssemblyRef.AssemblySuffixFull_SL )]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Maps.XamOrgChart" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Maps.XamNetworkNode" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Grids.XamGrid" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Calculations.XamCalculationManager" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Persistence" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Interactions.XamFormulaEditor" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Undo" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".SyntaxParsing" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Documents.TextDocument" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Editors.XamSyntaxEditor" + AssemblyRef.AssemblySuffixFull)]

[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Grids.DateTimeColumn" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Grids.MultiColumnComboColumn" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Grids.SparklineColumn" + AssemblyRef.AssemblySuffixFull)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Charts.XamSparkline" + AssemblyRef.AssemblySuffixFull_SL)]
[assembly: InternalsVisibleTo(AssemblyVersion.AssemblyNamePrefix + ".Controls.Maps.XamGeographicMap" + AssemblyRef.AssemblySuffixFull_SL)]






class AssemblyRef
{
    public const string BaseResourceName = "Infragistics.Windows.strings";
	internal const string SampleDataXPath = @"/SampleData/users/user";


	// the fully qualified assembly name suffix
	internal const string AssemblySuffixFull = AssemblyVersion.AssemblyNameSuffix;

	// the fully qualified assembly name suffix using the SL strong name key file
	internal const string AssemblySuffixFull_SL = AssemblyVersion.AssemblyNameSuffix;




    internal const string AssemblyName = AssemblyVersion.AssemblyNamePrefix + "" + AssemblyVersion.AssemblyNameSuffix;






	internal const string AssemblyProduct = "Infragistics NetAdvantage for WPF";
	internal const string AssemblyDescriptionBase = "Infragistics Shared Components for WPF";


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