using Warewolf.Studio.Themes.Luna;

#region Using directives

using System.Reflection;
using System.Windows;
using Infragistics.Windows.Themes;
using System.Windows.Markup;
using System.Security;

#endregion


[assembly: AssemblyCompany("Warewolf")]
[assembly: AssemblyProduct("Warewolf ESB")]
[assembly: AssemblyCopyright("Copyright Warewolf 2013")]


[assembly: SecurityRules(SecurityRuleSet.Level1)]

//[assembly: AssemblyResourceSetLoader(typeof(ThemeAssemblyResourceSetLoader))]

//[assembly: XmlnsPrefix("http://infragistics.com/Themes/Luna", "igThemeLuna")]
//[assembly: XmlnsDefinition("http://infragistics.com/Themes/Luna", "Warewolf.Studio.Themes.Luna")] 


[assembly: ThemeInfo(
	ResourceDictionaryLocation.None, 
	ResourceDictionaryLocation.SourceAssembly 
)]

[assembly: AssemblyVersion("0.4.*")]


