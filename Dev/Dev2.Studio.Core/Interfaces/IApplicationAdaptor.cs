using System;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Markup;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IApplicationAdaptor
    {
        //
        // Summary:
        //     Gets or sets the System.Reflection.Assembly that provides the pack uniform resource
        //     identifiers (URIs) for resources in a WPF application.
        //
        // Returns:
        //     A reference to the System.Reflection.Assembly that provides the pack uniform
        //     resource identifiers (URIs) for resources in a WPF application.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     A WPF application has an entry assembly, or System.Windows.Application.ResourceAssembly
        //     has already been set.
         Assembly ResourceAssembly { get; set; }
        //
        // Summary:
        //     Gets the System.Windows.Application object for the current System.AppDomain.
        //
        // Returns:
        //     The System.Windows.Application object for the current System.AppDomain.
         Application Current { get; }
        //
        // Summary:
        //     Gets the instantiated windows in an application.
        //
        // Returns:
        //     A System.Windows.WindowCollection that contains references to all window objects
        //     in the current System.AppDomain.
         WindowCollection Windows { get; }
        //
        // Summary:
        //     Gets or sets the main window of the application.
        //
        // Returns:
        //     A System.Windows.Window that is designated as the main application window.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     System.Windows.Application.MainWindow is set from an application that's hosted
        //     in a browser, such as an XAML browser applications (XBAPs).
         Window MainWindow { get; set; }
        //
        // Summary:
        //     Gets or sets the condition that causes the System.Windows.Application.Shutdown
        //     method to be called.
        //
        // Returns:
        //     A System.Windows.ShutdownMode enumeration value. The default value is System.Windows.ShutdownMode.OnLastWindowClose.
         ShutdownMode ShutdownMode { get; set; }
        //
        // Summary:
        //     Gets or sets a collection of application-scope resources, such as styles and
        //     brushes.
        //
        // Returns:
        //     A System.Windows.ResourceDictionary object that contains zero or more application-scope
        //     resources.
        [Ambient]
         ResourceDictionary Resources { get; set; }
        //
        // Summary:
        //     Gets or sets a UI that is automatically shown when an application starts.
        //
        // Returns:
        //     A System.Uri that refers to the UI that automatically opens when an application
        //     starts.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     System.Windows.Application.StartupUri is set with a value of null.
         Uri StartupUri { get; set; }
        //
        // Summary:
        //     Gets a collection of application-scope properties.
        //
        // Returns:
        //     An System.Collections.IDictionary that contains the application-scope properties.
         IDictionary Properties { get; }
        
        // Summary:
        //     Searches for a user interface (UI) resource, such as a System.Windows.Style or
        //     System.Windows.Media.Brush, with the specified key, and throws an exception if
        //     the requested resource is not found (see XAML Resources).
        //
        // Parameters:
        //   resourceKey:
        //     The name of the resource to find.
        //
        // Returns:
        //     The requested resource object. If the requested resource is not found, a System.Windows.ResourceReferenceKeyNotFoundException
        //     is thrown.
        //
        // Exceptions:
        //   T:System.Windows.ResourceReferenceKeyNotFoundException:
        //     The resource cannot be found.
         object FindResource(object resourceKey);
        //
        // Summary:
        //     Starts a Windows Presentation Foundation (WPF) application and opens the specified
        //     window.
        //
        // Parameters:
        //   window:
        //     A System.Windows.Window that opens automatically when an application starts.
        //
        // Returns:
        //     The System.Int32 application exit code that is returned to the operating system
        //     when the application shuts down. By default, the exit code value is 0.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     System.Windows.Application.Run is called from a browser-hosted application (for
        //     example, an XAML browser application (XBAP)).
        [SecurityCritical]
        int Run(Window window);
        //
        // Summary:
        //     Starts a Windows Presentation Foundation (WPF) application.
        //
        // Returns:
        //     The System.Int32 application exit code that is returned to the operating system
        //     when the application shuts down. By default, the exit code value is 0.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     System.Windows.Application.Run is called from a browser-hosted application (for
        //     example, an XAML browser application (XBAP)).
        int Run();
        //
        // Summary:
        //     Shuts down an application.
         void Shutdown();
        //
        // Summary:
        //     Shuts down an application that returns the specified exit code to the operating
        //     system.
        //
        // Parameters:
        //   exitCode:
        //     An integer exit code for an application. The default exit code is 0.
        [SecurityCritical]
         void Shutdown(int exitCode);
        //
        // Summary:
        //     Searches for the specified resource.
        //
        // Parameters:
        //   resourceKey:
        //     The name of the resource to find.
        //
        // Returns:
        //     The requested resource object. If the requested resource is not found, a null
        //     reference is returned.
      object TryFindResource(object resourceKey);
        
      
    }
}
