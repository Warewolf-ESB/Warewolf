﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Dev2.Studio.Core {
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
    public class StringResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal StringResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Dev2.Studio.Core.StringResources", typeof(StringResources).Assembly);
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
        ///   Looks up a localized string similar to Warewolf.
        /// </summary>
        public static string App_Data_Directory {
            get {
                return ResourceManager.GetString("App_Data_Directory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unsaved changes.
        /// </summary>
        public static string CloseHeader {
            get {
                return ResourceManager.GetString("CloseHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Closing Service Test Window from Taskbar handle exception. .
        /// </summary>
        public static string CloseTestViewHandledException {
            get {
                return ResourceManager.GetString("CloseTestViewHandledException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Critical Error.
        /// </summary>
        public static string CritErrorTitle {
            get {
                return ResourceManager.GetString("CritErrorTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Critical : Warewolf Must Restart. All open tabs will be saved. Please take the time to report this error to the community..
        /// </summary>
        public static string CriticalExceptionMessage {
            get {
                return ResourceManager.GetString("CriticalExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data you have entered is invalid. Please correct the data..
        /// </summary>
        public static string DataInput_Error {
            get {
                return ResourceManager.GetString("DataInput_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Did you know?.
        /// </summary>
        public static string DataInput_Error_Title {
            get {
                return ResourceManager.GetString("DataInput_Error_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please resolve all variable errors, before debugging..
        /// </summary>
        public static string Debugging_Error {
            get {
                return ResourceManager.GetString("Debugging_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error Debugging.
        /// </summary>
        public static string Debugging_Error_Title {
            get {
                return ResourceManager.GetString("Debugging_Error_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to localhost.
        /// </summary>
        public static string DefaultEnvironmentName {
            get {
                return ResourceManager.GetString("DefaultEnvironmentName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The resource {0} cannot be deleted because something depends on it..
        /// </summary>
        public static string Delete_Error {
            get {
                return ResourceManager.GetString("Delete_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error Deleting - {0}.
        /// </summary>
        public static string Delete_Error_Title {
            get {
                return ResourceManager.GetString("Delete_Error_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Destination server permission Deploy To not allowed..
        /// </summary>
        public static string DestinationPermission_Error {
            get {
                return ResourceManager.GetString("DestinationPermission_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you wish to delete {0} {1}?
        ///
        ///Version History will also be deleted..
        /// </summary>
        public static string DialogBody_ConfirmDelete {
            get {
                return ResourceManager.GetString("DialogBody_ConfirmDelete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you wish to delete {0} folder and all its contents?.
        /// </summary>
        public static string DialogBody_ConfirmFolderDelete {
            get {
                return ResourceManager.GetString("DialogBody_ConfirmFolderDelete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The workflow &apos;{0}&apos; that you are closing is not saved.
        ///The workflow is linked to a disconnected server. 
        ///To save the workflow, select Cancel and reconnect the server, or select Ok to discard your changes.
        ///-----------------------------------------------------------------
        ///Ok - Discard your changes.
        ///Cancel - Returns you to the workflow..
        /// </summary>
        public static string DialogBody_DisconnectedItemNotSaved {
            get {
                return ResourceManager.GetString("DialogBody_DisconnectedItemNotSaved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This folder cannot be deleted because it contains resources with dependents..
        /// </summary>
        public static string DialogBody_FolderContentsHaveDependencies {
            get {
                return ResourceManager.GetString("DialogBody_FolderContentsHaveDependencies", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} {1} cannot be deleted because something depends on it..
        /// </summary>
        public static string DialogBody_HasDependencies {
            get {
                return ResourceManager.GetString("DialogBody_HasDependencies", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} {1} cannot be renamed because a resource with that name already exists on the same server..
        /// </summary>
        public static string DialogBody_HasDuplicateName {
            get {
                return ResourceManager.GetString("DialogBody_HasDuplicateName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The workflow &apos;{0}&apos; that you are closing is not saved.
        ///Would you like to save the workflow?
        ///-----------------------------------------------------------------
        ///Yes - Save the workflow.
        ///No - Discard your changes.
        ///Cancel - Returns you to the workflow..
        /// </summary>
        public static string DialogBody_NotSaved {
            get {
                return ResourceManager.GetString("DialogBody_NotSaved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirm delete..
        /// </summary>
        public static string DialogTitle_ConfirmDelete {
            get {
                return ResourceManager.GetString("DialogTitle_ConfirmDelete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Folder has dependents.
        /// </summary>
        public static string DialogTitle_FolderHasDependencies {
            get {
                return ResourceManager.GetString("DialogTitle_FolderHasDependencies", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} has dependents.
        /// </summary>
        public static string DialogTitle_HasDependencies {
            get {
                return ResourceManager.GetString("DialogTitle_HasDependencies", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} exists.
        /// </summary>
        public static string DialogTitle_HasDuplicateName {
            get {
                return ResourceManager.GetString("DialogTitle_HasDuplicateName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Environments.
        /// </summary>
        public static string Environments_Directory {
            get {
                return ResourceManager.GetString("Environments_Directory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connection Failed.
        /// </summary>
        public static string Error_Connect_Failed {
            get {
                return ResourceManager.GetString("Error_Connect_Failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Name not provided.
        /// </summary>
        public static string Error_DSF_Name_Not_Provided {
            get {
                return ResourceManager.GetString("Error_DSF_Name_Not_Provided", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The recordset name is a duplicate of an existing recordset. Recordset names must be unique..
        /// </summary>
        public static string ErrorMessageDuplicateRecordset {
            get {
                return ResourceManager.GetString("ErrorMessageDuplicateRecordset", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Variable names must be unique..
        /// </summary>
        public static string ErrorMessageDuplicateValue {
            get {
                return ResourceManager.GetString("ErrorMessageDuplicateValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This variable name is a duplicate of an existing variable. Variable names must be unique..
        /// </summary>
        public static string ErrorMessageDuplicateVariable {
            get {
                return ResourceManager.GetString("ErrorMessageDuplicateVariable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Recordset must contain one or more field(s)..
        /// </summary>
        public static string ErrorMessageEmptyRecordSet {
            get {
                return ResourceManager.GetString("ErrorMessageEmptyRecordSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Variable name contains invalid character(s)..
        /// </summary>
        public static string ErrorMessageInvalidChar {
            get {
                return ResourceManager.GetString("ErrorMessageInvalidChar", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error : .
        /// </summary>
        public static string ErrorPrefix {
            get {
                return ResourceManager.GetString("ErrorPrefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        public static string ErrorTitle {
            get {
                return ResourceManager.GetString("ErrorTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are changes that have not been tested.
        ///Would you like to test the changes? 
        ///-----------------------------------------------------------------
        ///Yes - Test your changes.
        ///No - Discard your changes..
        /// </summary>
        public static string ItemSource_HasChanged_NotTested {
            get {
                return ResourceManager.GetString("ItemSource_HasChanged_NotTested", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your changes have not been saved.
        ///Would you like to save? 
        ///-----------------------------------------------------------------
        ///Yes - Save your changes.
        ///No - Discard your changes.
        ///Cancel - Returns you to the tab..
        /// </summary>
        public static string ItemSource_NotSaved {
            get {
                return ResourceManager.GetString("ItemSource_NotSaved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unsaved.
        /// </summary>
        public static string NewWorkflowBaseName {
            get {
                return ResourceManager.GetString("NewWorkflowBaseName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to pack://application:,,,/Warewolf Studio;component/Images/ExecuteDebugStart-32.png.
        /// </summary>
        public static string Pack_Uri_Debug_Image {
            get {
                return ResourceManager.GetString("Pack_Uri_Debug_Image", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to pack://application:,,,/Warewolf Studio;component/Images/ExecuteDebugStop-32.png.
        /// </summary>
        public static string Pack_Uri_Stop_Image {
            get {
                return ResourceManager.GetString("Pack_Uri_Stop_Image", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Debug.
        /// </summary>
        public static string Ribbon_Debug {
            get {
                return ResourceManager.GetString("Ribbon_Debug", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stop.
        /// </summary>
        public static string Ribbon_StopExecution {
            get {
                return ResourceManager.GetString("Ribbon_StopExecution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while saving.
        /// </summary>
        public static string SaveSettingErrorPrefix {
            get {
                return ResourceManager.GetString("SaveSettingErrorPrefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are duplicate permissions for a resource, 
        ///    i.e. one resource has permissions set twice with the same group. 
        ///    Please clear the duplicates before saving..
        /// </summary>
        public static string SaveSettingsDuplicateResourcePermissions {
            get {
                return ResourceManager.GetString("SaveSettingsDuplicateResourcePermissions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are duplicate server permissions, 
        ///    i.e. Server permissions have been setup up with the same group twice. 
        ///    Please clear the duplicates before saving..
        /// </summary>
        public static string SaveSettingsDuplicateServerPermissions {
            get {
                return ResourceManager.GetString("SaveSettingsDuplicateServerPermissions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while saving: Server unreachable..
        /// </summary>
        public static string SaveSettingsNotReachableErrorMsg {
            get {
                return ResourceManager.GetString("SaveSettingsNotReachableErrorMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while saving: You don&apos;t have permission to change settings on this server.
        ///You need Administrator permission..
        /// </summary>
        public static string SaveSettingsPermissionsErrorMsg {
            get {
                return ResourceManager.GetString("SaveSettingsPermissionsErrorMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please resolve the variable(s) errors below, before saving..
        /// </summary>
        public static string Saving_Error {
            get {
                return ResourceManager.GetString("Saving_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error Saving.
        /// </summary>
        public static string Saving_Error_Title {
            get {
                return ResourceManager.GetString("Saving_Error_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Source server permission Deploy From not allowed..
        /// </summary>
        public static string SourcePermission_Error {
            get {
                return ResourceManager.GetString("SourcePermission_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your changes have not been saved.
        ///Would you like to save? 
        ///-----------------------------------------------------------------
        ///Yes - Save your changes.
        ///No - Discard your changes.
        ///Cancel - Do not close the studio..
        /// </summary>
        public static string Unsaved_Changes {
            get {
                return ResourceManager.GetString("Unsaved_Changes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://warewolf.io/resources.php.
        /// </summary>
        public static string Uri_Community_HomePage {
            get {
                return ResourceManager.GetString("Uri_Community_HomePage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Help\PageNotAvailable.htm.
        /// </summary>
        public static string Uri_Studio_PageNotAvailable {
            get {
                return ResourceManager.GetString("Uri_Studio_PageNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UserInterfaceLayouts.
        /// </summary>
        public static string User_Interface_Layouts_Directory {
            get {
                return ResourceManager.GetString("User_Interface_Layouts_Directory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;xamDockManager version=&quot;12.1.20121.2107&quot;&gt;
        ///  &lt;contentPanes&gt;
        ///    &lt;contentPane name=&quot;OutputPane&quot; location=&quot;DockedRight&quot; lastFloatingSize=&quot;1000,200&quot; lastFloatingWindowRect=&quot;625,541,1016,234&quot; lastFloatingLocation=&quot;625,541&quot; lastActivatedTime=&quot;2013-06-10T07:30:08.998685Z&quot; /&gt;
        ///    &lt;contentPane name=&quot;Variables&quot; location=&quot;DockedRight&quot; lastFloatingSize=&quot;300,706&quot; lastFloatingWindowRect=&quot;1475,482,316,740&quot; lastFloatingLocation=&quot;1475,482&quot; lastActivatedTime=&quot;2013-06-10T07:30:02.88 [rest of string was truncated]&quot;;.
        /// </summary>
        public static string XmlOriginalLayout {
            get {
                return ResourceManager.GetString("XmlOriginalLayout", resourceCulture);
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
        ///			&lt;Output Name=&quot;vehicleVin&quot; [rest of string was truncated]&quot;;.
        /// </summary>
        public static string xmlServiceDefinition {
            get {
                return ResourceManager.GetString("xmlServiceDefinition", resourceCulture);
            }
        }
    }
}
