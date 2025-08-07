using Dev2.Common;
using Dev2.Common.Common;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xaml;
using Unlimited.Applications.BusinessDesignStudio.Activities;

public static class XamlActivityHelper
{
    /// <summary>
    /// Gets Xaml ActivityBuilder from xamlDefinition with consistent assembly loading.
    /// </summary>
    /// <param name="xamlDefinition">The xaml definition.</param>
    /// <returns cref="ActivityBuilder">ActivityBuilder</returns>
    public static ActivityBuilder GetXamlActivityBuilderAsDataActivities(StringBuilder xamlDefinition)
    {
        if (xamlDefinition == null || xamlDefinition.Length == 0)
        {
            return null;
        }

        try
        {
            if (GlobalConstants.RuntimeNamespaceClean)
            {
                xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
            }
#if !(WINDOWS || NETFRAMEWORK)
            Dev2.DynamicServices.Objects.Dev2XamlLoader.RemoveWindowsElements(ref xamlDefinition);
#endif

            using (var xamlStream = xamlDefinition.EncodeForXmlDocument(tryUnicodeFirst: false))
            {
                // Get the assembly containing both activities
                var targetAssembly = typeof(DsfFlowDecisionActivity).Assembly;

                // Create custom schema context for consistent assembly resolution
                var schemaContext = new Dev2XamlSchemaContext(targetAssembly);

                var settings = new XamlXmlReaderSettings
                {
                    LocalAssembly = targetAssembly
                    // Note: Schema context is passed directly to XamlXmlReader constructor
                };

                using (var reader = new XamlXmlReader(xamlStream, schemaContext, settings))
                {
                    var xw = ActivityXamlServices.CreateBuilderReader(reader);
                    var load = XamlServices.Load(xw);
                    return load as ActivityBuilder;
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            System.Diagnostics.Debug.WriteLine($"Failed to load XAML ActivityBuilder: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Alternative approach using AppDomain assembly resolution
    /// </summary>
    /// <param name="xamlDefinition">The xaml definition.</param>
    /// <returns cref="ActivityBuilder">ActivityBuilder</returns>
    public static ActivityBuilder GetXamlActivityBuilderAsDataActivitiesWithAppDomainResolution(StringBuilder xamlDefinition)
    {
        if (xamlDefinition == null || xamlDefinition.Length == 0)
        {
            return null;
        }

        var targetAssembly = typeof(DsfFlowDecisionActivity).Assembly;
        ResolveEventHandler assemblyResolver = (sender, args) =>
        {
            var requestedAssemblyName = new AssemblyName(args.Name);
            var targetAssemblyName = new AssemblyName(targetAssembly.FullName);

            if (requestedAssemblyName.Name.Equals(targetAssemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                return targetAssembly;
            }
            return null;
        };

        try
        {
            // Subscribe to assembly resolution events
            AppDomain.CurrentDomain.AssemblyResolve += assemblyResolver;

            if (GlobalConstants.RuntimeNamespaceClean)
            {
                xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
            }
#if !(WINDOWS || NETFRAMEWORK)
            Dev2.DynamicServices.Objects.Dev2XamlLoader.RemoveWindowsElements(ref xamlDefinition);
#endif

			using (var xamlStream = xamlDefinition.EncodeForXmlDocument(tryUnicodeFirst: false))
            {
                var settings = new XamlXmlReaderSettings
                {
                    LocalAssembly = targetAssembly
                };

                using (var reader = new XamlXmlReader(xamlStream, settings))
                {
                    var xw = ActivityXamlServices.CreateBuilderReader(reader);
                    var load = XamlServices.Load(xw);
                    return load as ActivityBuilder;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load XAML ActivityBuilder: {ex.Message}");
            return null;
        }
        finally
        {
            // Always unsubscribe to prevent memory leaks
            AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolver;
        }
    }

    /// <summary>
    /// Alternative approach using type comparison instead of direct casting
    /// </summary>
    /// <param name="activity">The activity to check</param>
    /// <param name="cell">The cell to populate</param>
    /// <returns>True if the activity was processed as DsfDotNetMultiAssignActivity</returns>
    public static bool TryProcessX6JsonFromActivity(Activity activity, Dev2.Common.X6.Cell cell)
    {
        if (activity == null) return false;

        var activityType = activity.GetType();

        // Compare by type name and assembly name instead of direct casting
        //TODO: here we can just compare type namespace (i.e., Unlimited.Applications.BusinessDesignStudio.Activities) 
        if (activityType.Name == nameof(DsfDotNetMultiAssignActivity) &&
            activityType.Namespace == typeof(DsfDotNetMultiAssignActivity).Namespace)
        {
            try
            {
                // Use reflection to call ToX6Json method
                var toX6JsonMethod = activityType.GetMethod("ToX6Json");
                if (toX6JsonMethod != null)
                {
                    toX6JsonMethod.Invoke(activity, new[] { cell });
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to invoke ToX6Json: {ex.Message}");
            }
        }

        return false;
    }

    /// <summary>
    /// Generic method to safely cast activities loaded from different assembly contexts
    /// </summary>
    /// <typeparam name="T">The target activity type</typeparam>
    /// <param name="activity">The activity to cast</param>
    /// <param name="action">Action to perform if cast is successful</param>
    /// <returns>True if cast and action were successful</returns>
    public static bool TryCastAndExecute<T>(Activity activity, Action<T> action) where T : class
    {
        if (activity == null || action == null) return false;

        // First try direct cast
        if (activity is T directCast)
        {
            action?.Invoke(directCast);
            return true;
        }

        // If direct cast fails, try type comparison approach
        var activityType = activity.GetType();
        var targetType = typeof(T);

        if (activityType.Name == targetType.Name &&
            activityType.Namespace == targetType.Namespace)
        {
            try
            {
                // Create a wrapper that implements the interface through reflection
                var wrapper = CreateTypeWrapper<T>(activity);
                if (wrapper != null)
                {
                    action?.Invoke(wrapper);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create type wrapper: {ex.Message}");
            }
        }

        return false;
    }

    private static T CreateTypeWrapper<T>(object instance) where T : class
    {
        // This would require implementing a dynamic proxy or wrapper
        // For now, return null - this is a placeholder for more complex scenarios
        return null;
    }
}

/// <summary>
/// Custom XAML Schema Context to ensure consistent assembly loading using AppDomain loaded assemblies
/// </summary>
public class Dev2XamlSchemaContext : XamlSchemaContext
{
    private readonly Assembly _targetAssembly;
    private readonly Dictionary<string, Assembly> _assemblyCache;

    public Dev2XamlSchemaContext(Assembly targetAssembly) : base()
    {
        _targetAssembly = targetAssembly;
        _assemblyCache = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        // Pre-populate cache with all currently loaded assemblies in AppDomain
        PopulateAssemblyCache();
    }

    private void PopulateAssemblyCache()
    {
        try
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in loadedAssemblies)
            {
                try
                {
                    // Cache by full name
                    if (!string.IsNullOrEmpty(assembly.FullName))
                    {
                        _assemblyCache[assembly.FullName] = assembly;

                        // Also cache by simple name for easier lookup
                        var assemblyName = new AssemblyName(assembly.FullName);
                        if (!string.IsNullOrEmpty(assemblyName.Name))
                        {
                            _assemblyCache[assemblyName.Name] = assembly;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Some assemblies might not be accessible, skip them
                    System.Diagnostics.Debug.WriteLine($"Could not cache assembly {assembly}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error populating assembly cache: {ex.Message}");
        }
    }

    public override XamlType GetXamlType(Type type)
    {
        // Try to get type from cached assemblies first
        if (type != null && type.Assembly != null)
        {
            var assemblyName = type.Assembly.FullName;
            if (_assemblyCache.ContainsKey(assemblyName))
            {
                // Use the cached assembly version
                var cachedAssembly = _assemblyCache[assemblyName];
                if (cachedAssembly != type.Assembly)
                {
                    // Try to get the equivalent type from cached assembly
                    var cachedType = cachedAssembly.GetType(type.FullName);
                    if (cachedType != null)
                    {
                        return base.GetXamlType(cachedType);
                    }
                }
            }
        }

        return base.GetXamlType(type);
    }

    protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
    {
        try
        {
            // Try to resolve the type using cached assemblies first
            var resolvedType = ResolveTypeFromCache(xamlNamespace, name);
            if (resolvedType != null)
            {
                return GetXamlType(resolvedType);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error resolving XAML type '{name}' from namespace '{xamlNamespace}': {ex.Message}");
        }

        return base.GetXamlType(xamlNamespace, name, typeArguments);
    }

    private Type ResolveTypeFromCache(string xamlNamespace, string typeName)
    {
        if (string.IsNullOrEmpty(xamlNamespace) || string.IsNullOrEmpty(typeName))
            return null;

        // Extract assembly information from XAML namespace if present
        // Format: clr-namespace:Namespace;assembly=AssemblyName
        if (xamlNamespace.StartsWith("clr-namespace:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = xamlNamespace.Substring("clr-namespace:".Length).Split(';');
            var namespaceName = parts[0];
            string assemblyName = null;

            if (parts.Length > 1)
            {
                var assemblyPart = parts.FirstOrDefault(p => p.StartsWith("assembly=", StringComparison.OrdinalIgnoreCase));
                if (assemblyPart != null)
                {
                    assemblyName = assemblyPart.Substring("assembly=".Length);
                }
            }

            // Try to find type in cached assemblies
            foreach (var cachedAssembly in _assemblyCache.Values.Distinct())
            {
                try
                {
                    // If assembly name is specified, only check matching assembly
                    if (!string.IsNullOrEmpty(assemblyName))
                    {
                        var cachedAssemblyName = new AssemblyName(cachedAssembly.FullName);
                        if (!cachedAssemblyName.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    var fullTypeName = $"{namespaceName}.{typeName}";
                    var type = cachedAssembly.GetType(fullTypeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting type from assembly {cachedAssembly.FullName}: {ex.Message}");
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Refreshes the assembly cache with currently loaded assemblies
    /// Useful if assemblies are loaded dynamically after schema context creation
    /// </summary>
    public void RefreshAssemblyCache()
    {
        PopulateAssemblyCache();
    }

    /// <summary>
    /// Gets the count of cached assemblies for diagnostics
    /// </summary>
    public int CachedAssemblyCount => _assemblyCache.Count;

    /// <summary>
    /// Gets the names of all cached assemblies for diagnostics
    /// </summary>
    public IEnumerable<string> GetCachedAssemblyNames() => _assemblyCache.Keys.ToList();
}
