﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Dev2.Runtime.Configuration.Properties {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Dev2.Runtime.Configuration.Properties.Resources", typeof(Resources).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid FilterMode enumeration value. The value must be one of the defined AutoCompleteFilterMode values to be accepted..
        /// </summary>
        internal static string AutoComplete_OnFilterModePropertyChanged_InvalidValue {
            get {
                return ResourceManager.GetString("AutoComplete_OnFilterModePropertyChanged_InvalidValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid maximum drop down height value &apos;{0}&apos;. The value must be greater than or equal to zero..
        /// </summary>
        internal static string AutoComplete_OnMaxDropDownHeightPropertyChanged_InvalidValue {
            get {
                return ResourceManager.GetString("AutoComplete_OnMaxDropDownHeightPropertyChanged_InvalidValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid MinimumPopulateDelay value &apos;{0}&apos;. The value must be greater than or equal to zero..
        /// </summary>
        internal static string AutoComplete_OnMinimumPopulateDelayPropertyChanged_InvalidValue {
            get {
                return ResourceManager.GetString("AutoComplete_OnMinimumPopulateDelayPropertyChanged_InvalidValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot set read-only property SearchText..
        /// </summary>
        internal static string AutoComplete_OnSearchTextPropertyChanged_InvalidWrite {
            get {
                return ResourceManager.GetString("AutoComplete_OnSearchTextPropertyChanged_InvalidWrite", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The RoutedPropertyChangingEvent cannot be canceled..
        /// </summary>
        internal static string RoutedPropertyChangingEventArgs_CancelSet_InvalidOperation {
            get {
                return ResourceManager.GetString("RoutedPropertyChangingEventArgs_CancelSet_InvalidOperation", resourceCulture);
            }
        }
    }
}
