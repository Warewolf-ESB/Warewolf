﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Dev2.Core.Tests {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class StringResourcesTest {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal StringResourcesTest() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Dev2.Core.Tests.StringResourcesTest", typeof(StringResourcesTest).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;DataList&gt;&lt;scalar1 Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;Input&quot; /&gt;&lt;scalar2 Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;Input&quot; /&gt;&lt;Recset Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;None&quot; &gt;&lt;Field1 Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;None&quot; /&gt;&lt;Field2 Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;None&quot; /&gt;&lt;/Recset&gt;&lt;/DataList&gt;.
        /// </summary>
        public static string DebugInputWindow_DataList {
            get {
                return ResourceManager.GetString("DebugInputWindow_DataList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;DataList&gt;&lt;scalar1 Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;Output&quot; /&gt;&lt;scalar2 Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;Output&quot; /&gt;&lt;Recset Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;None&quot; &gt;&lt;Field1 Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;None&quot; /&gt;&lt;Field2 Description=&quot;&quot; IsEditable=&quot;True&quot; ColumnIODirection=&quot;None&quot; /&gt;&lt;/Recset&gt;&lt;/DataList&gt;.
        /// </summary>
        public static string DebugInputWindow_NoInputs_XMLData {
            get {
                return ResourceManager.GetString("DebugInputWindow_NoInputs_XMLData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;Activity mc:Ignorable=&quot;sap&quot; x:Class=&quot;TestWorkflow&quot; xmlns=&quot;http://schemas.microsoft.com/netfx/2009/xaml/activities&quot; xmlns:av=&quot;http://schemas.microsoft.com/winfx/2006/xaml/presentation&quot; xmlns:dsca=&quot;clr-namespace:Dev2.Studio.Core.Activities;assembly=Dev2.Studio.Core.Activities&quot; xmlns:mc=&quot;http://schemas.openxmlformats.org/markup-compatibility/2006&quot; xmlns:mva=&quot;clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities&quot; xmlns:s=&quot;clr-namespace:System;assembly=mscorlib&quot; xmlns:sap=&quot;http://schemas.mic [rest of string was truncated]&quot;;.
        /// </summary>
        public static string DebugInputWindow_WorkflowXaml {
            get {
                return ResourceManager.GetString("DebugInputWindow_WorkflowXaml", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;DataList&gt;
        ///  &lt;scalar1&gt;ScalarData1&lt;/scalar1&gt;
        ///  &lt;scalar2&gt;ScalarData2&lt;/scalar2&gt;
        ///  &lt;Recset&gt;
        ///    &lt;Field1&gt;Field1Data1&lt;/Field1&gt;
        ///    &lt;Field2&gt;Field2Data1&lt;/Field2&gt;
        ///  &lt;/Recset&gt;
        ///  &lt;Recset&gt;
        ///    &lt;Field1&gt;Field1Data2&lt;/Field1&gt;
        ///    &lt;Field2&gt;Field2Data2&lt;/Field2&gt;
        ///  &lt;/Recset&gt;
        ///  &lt;Recset&gt;
        ///    &lt;Field1&gt;Field1Data3&lt;/Field1&gt;
        ///    &lt;Field2&gt;Field2Data3&lt;/Field2&gt;
        ///  &lt;/Recset&gt;
        ///  &lt;Recset&gt;
        ///    &lt;Field1&gt;Field1Data4&lt;/Field1&gt;
        ///    &lt;Field2&gt;Field2Data4&lt;/Field2&gt;
        ///  &lt;/Recset&gt;
        ///&lt;/DataList&gt;.
        /// </summary>
        public static string DebugInputWindow_XMLData {
            get {
                return ResourceManager.GetString("DebugInputWindow_XMLData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;DataList&gt;
        ///	&lt;NUD2347 ColumnIODirection=&quot;Both&quot; Description=&quot;&quot;/&gt;
        ///	&lt;number ColumnIODirection=&quot;Both&quot; Description=&quot;&quot;/&gt;
        ///	&lt;vehicleColor ColumnIODirection=&quot;Both&quot; Description=&quot;&quot;/&gt;
        ///	&lt;Fines ColumnIODirection=&quot;Both&quot; Description=&quot;&quot;&gt;
        ///		&lt;Speed Description=&quot;&quot;/&gt;
        ///		&lt;Date Description=&quot;&quot;/&gt;
        ///		&lt;Location Description=&quot;&quot;/&gt;
        ///	&lt;/Fines&gt;
        ///	&lt;Registrations ColumnIODirection=&quot;Both&quot; Description=&quot;&quot;&gt;
        ///		&lt;Speed Description=&quot;&quot;/&gt;
        ///		&lt;Date Description=&quot;&quot;/&gt;
        ///		&lt;Location Description=&quot;&quot;/&gt;
        ///	&lt;/Registrations&gt;
        ///&lt;/DataList&gt;.
        /// </summary>
        public static string xmlDataList {
            get {
                return ResourceManager.GetString("xmlDataList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;Service Name=&quot;GetCars&quot;&gt;
        ///  &lt;Actions&gt;
        ///	&lt;Action Name=&quot;GetCarsByReg&quot; Type=&quot;InvokeStoredProc&quot; SourceName=&quot;CarsDatabase&quot; SourceMethod=&quot;proc_GetCarsByReg&quot;&gt;
        ///		&lt;Inputs&gt;
        ///			&lt;Input Name=&quot;reg&quot; Source=&quot;&quot; DefaultValue=&quot;NUD2347&quot;&gt;
        ///				&lt;Validator Type=&quot;Required&quot; /&gt;				
        ///			&lt;/Input&gt;
        ///			&lt;Input Name=&quot;asdfsad&quot; Source=&quot;registration223&quot; DefaultValue=&quot;w3rt24324&quot;&gt;
        ///				&lt;Validator Type=&quot;Required&quot; /&gt;				
        ///			&lt;/Input&gt;			
        ///			&lt;Input Name=&quot;number&quot; Source=&quot;&quot; DefaultValue=&quot;&quot;/&gt;
        ///		&lt;/Inputs&gt;
        ///		&lt;Outputs&gt;
        ///			&lt;Output Name=&quot;vehicleVin&quot; MapsTo=&quot;VIN&quot; [rest of string was truncated]&quot;;.
        /// </summary>
        public static string xmlServiceDefinition {
            get {
                return ResourceManager.GetString("xmlServiceDefinition", resourceCulture);
            }
        }
    }
}
