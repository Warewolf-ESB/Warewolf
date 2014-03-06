using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;


using System.Security;
using System.Windows;


using System.Runtime.InteropServices;
using System.Windows.Markup;
using Infragistics;


#pragma warning disable 436

[assembly: AssemblyTitle(AssemblyRef.AssemblyName + AssemblyRef.ProductTitleSuffix)]
[assembly: AssemblyDescription(AssemblyRef.AssemblyDescriptionBase + " - " + AssemblyRef.Configuration + " Version")]
[assembly: AssemblyProduct(AssemblyRef.AssemblyProduct + AssemblyRef.ProductTitleSuffix)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Infragistics, Inc.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("ad3b3a28-cbb3-4121-964c-1e1decd13cda")]
[assembly: CLSCompliant(true)]


[assembly: AllowPartiallyTrustedCallers()]
[assembly: ThemeInfo(
    ResourceDictionaryLocation.SourceAssembly, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page,
    // or application resource dictionaries)
    ResourceDictionaryLocation.None //where the generic resource dictionary is located
    //(used if a resource is not found in the page,
    // app, or any theme specific resource dictionaries)
)]


[assembly: NeutralResourcesLanguage("en")]

[assembly: StringResourceLocation("Infragistics.Controls.Strings")]




[assembly: AssemblyCopyright("Copyright © Infragistics, Inc. 2009 - " + AssemblyVersion.EndCopyrightYear)]
[assembly: AssemblyVersion(AssemblyVersion.Version)]
[assembly: AssemblyFileVersion(AssemblyVersion.Version)]
[assembly: SatelliteContractVersion(AssemblyVersion.SatelliteContractVersion)]

[assembly: SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Infragistics is a word.")]



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


[assembly: XmlnsPrefix("http://schemas.infragistics.com/xaml", "ig")]
[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml", "Infragistics.Controls")]
[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml", "Infragistics.Controls.Maps")]
[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml", "Infragistics")]
[assembly: XmlnsDefinition("http://schemas.infragistics.com/xaml", "Infragistics.Collections")]






[assembly: InternalsVisibleTo(AssemblyRef.XamDataChartAssemblyName)]
[assembly: InternalsVisibleTo(AssemblyRef.XamNetworkNodeAssemblyName)]
[assembly: InternalsVisibleTo(AssemblyRef.XamOrgChartAssemblyName)]







[assembly: InternalsVisibleTo("Infragistics.WPF4.DV.UnitTests")]


internal class AssemblyRef
{



    private const string Preview = "";











    private const string XamDataChartAssemblyPrefix = "InfragisticsWPF4.";

    internal const string XamDataChartAssemblyName = AssemblyRef.XamDataChartAssemblyPrefix + "Controls.Charts.XamDataChart." + AssemblyRef.Preview + "v" + AssemblyVersion.MajorMinor;
    internal const string XamNetworkNodeAssemblyName = AssemblyRef.XamDataChartAssemblyPrefix + "Controls.Maps.XamNetworkNode." + AssemblyRef.Preview + "v" + AssemblyVersion.MajorMinor;
    internal const string XamOrgChartAssemblyName = AssemblyRef.XamDataChartAssemblyPrefix + "Controls.Maps.XamOrgChart." + AssemblyRef.Preview + "v" + AssemblyVersion.MajorMinor;




    internal const string AssemblyName = AssemblyRef.AssemblyNamePrefix + ".DataVisualization" + AssemblyRef.AssemblyNameSuffix;









    internal const string AssemblyProduct = "Infragistics NetAdvantage for WPF";
    internal const string AssemblyDescriptionBase = "Infragistics Compression for WPF";


    internal const string Configuration = AssemblyVersion.Configuration;


    internal const string ProductTitleSuffix = "";











	internal const string AssemblyNamePrefix = "InfragisticsWPF4";




    internal const string AssemblyNameSuffix = ".v" + AssemblyVersion.MajorMinor;
}

#pragma warning restore 436
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