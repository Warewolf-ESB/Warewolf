using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GACManagerApi.Fusion;





namespace GACManagerApi
{
    /// <summary>
    /// An AssemblyDescription holds only the most basic assembly
    /// details that would be loaded from an application such as gacutil. 
    /// </summary>
    public class AssemblyDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyDescription"/> class.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        public AssemblyDescription(IAssemblyName assemblyName)
        {
            //  Get the qualified name.
            var stringBuilder = new StringBuilder(10000);
            var iLen = 10000;
            var hr = assemblyName.GetDisplayName(stringBuilder, ref iLen, ASM_DISPLAY_FLAGS.ASM_DISPLAYF_VERSION
                | ASM_DISPLAY_FLAGS.ASM_DISPLAYF_CULTURE
                | ASM_DISPLAY_FLAGS.ASM_DISPLAYF_PUBLIC_KEY_TOKEN
                | ASM_DISPLAY_FLAGS.ASM_DISPLAYF_PROCESSORARCHITECTURE);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
            var displayName = stringBuilder.ToString();

            //  Load properties from the display name.
            LoadPropertiesFromDisplayName(displayName);

            //  We have the assembly name, so we can use the optimised version to load the fusion properties.
            _lazyFusionProperties = new Lazy<AssemblyFusionProperties>(DoLoadFusionProperties);
            _lazyReflectionProperties = new Lazy<AssemblyReflectionProperties>(DoLoadReflectionProperties);
        }

        private AssemblyFusionProperties DoLoadFusionProperties()
        {
            //  Use the enumerator to get the assembly name.
            var enumerator = new AssemblyCacheEnumerator(DisplayName);
            var assemblyName = enumerator.GetNextAssembly();
            
            //  Return the properties.
            return DoLoadFusionProperties(assemblyName);
        }

        private AssemblyFusionProperties DoLoadFusionProperties(IAssemblyName assemblyName)
        {
            //  Create the fusion properties.
            var fusionProperties = new AssemblyFusionProperties();

            //  Load the properties.
            fusionProperties.Load(assemblyName);

            //  Return the properties.
            return fusionProperties;
        }
        
        private void LoadPropertiesFromDisplayName(string displayName)
        {
            DisplayName = displayName;

            var properties = displayName.Split(new[] {", "}, StringSplitOptions.None);

            //  Name should be first.
            try
            {
                Name = properties[0];
            }
            catch (Exception)
            {
                Name = "Unknown";
            }

            var versionString = (from p in properties where p.StartsWith("Version=") select p).FirstOrDefault();
            var cultureString = (from p in properties where p.StartsWith("Culture=") select p).FirstOrDefault();
            var publicKeyTokenString = (from p in properties where p.StartsWith("PublicKeyToken=") select p).FirstOrDefault();
            var processorArchitectureString = (from p in properties where p.StartsWith("processorArchitecture=") select p).FirstOrDefault();
            var customString = (from p in properties where p.StartsWith("Custom=") select p).FirstOrDefault();

            //  Then we should have version.
            if (!string.IsNullOrEmpty(versionString))
            {
                try
                {
                    Version = versionString.Substring(versionString.IndexOf('=') + 1);
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }

            //  Then culture.
            if (!string.IsNullOrEmpty(cultureString))
            {
                try
                {
                    cultureString = cultureString.Substring(cultureString.IndexOf('=') + 1);
                    Culture = cultureString;
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }

            //  Then public key token.
            if (!string.IsNullOrEmpty(publicKeyTokenString))
            {
                try
                {
                    publicKeyTokenString = publicKeyTokenString.Substring(publicKeyTokenString.IndexOf('=') + 1);
                    PublicKeyToken = HexToData(publicKeyTokenString);
                }
                catch (Exception)
                {
                    PublicKeyToken = null;
                }
            }

            //  Then processor architecture.
            if (!string.IsNullOrEmpty(processorArchitectureString))
            {
                try
                {
                    processorArchitectureString =
                        processorArchitectureString.Substring(processorArchitectureString.IndexOf('=') + 1);
                    ProcessorArchitecture = processorArchitectureString;
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }

            if (!string.IsNullOrEmpty(customString))
            {
                //  Then custom.
                try
                {
                    customString = customString.Substring(customString.IndexOf('=') + 1);
                    Custom = customString;
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }

            //  Finally, get the path.
            Path = AssemblyCache.QueryAssemblyInfo(DisplayName);
        }

        private static byte[] HexToData(string hexString)
        {
            if (hexString == null)
                return null;

            if (hexString.Length % 2 == 1)
                hexString = '0' + hexString; // Up to you whether to pad the first or last byte

            byte[] data = new byte[hexString.Length / 2];

            for (int i = 0; i < data.Length; i++)
                data[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            return data;
        }
        
        private AssemblyReflectionProperties DoLoadReflectionProperties()
        {
            //  Create reflection properties.
            var reflectionPropties = new AssemblyReflectionProperties();

            //  Load the reflection properties.
            reflectionPropties.Load(DisplayName);

            //  Return the properties.
            return reflectionPropties; 
        }
        
        /// <summary>
        /// Gets the short assembly name, such as mscorlib.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the qualified name of the assembly. This is useful for Install/Uninstall.
        /// v1.0/v1.1 assemblies: "name, Version=xx, Culture=xx, PublicKeyToken=xx".
        /// v2.0 assemblies: "name, Version=xx, Culture=xx, PublicKeyToken=xx, ProcessorArchitecture=xx".
        /// </summary>
        /// <value>
        /// The name of the qualified assembly.
        /// </value>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Gets the public key token.
        /// </summary>
        public byte[] PublicKeyToken { get; private set; }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        public string Culture { get; private set; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the processor architecture.
        /// </summary>
        public string ProcessorArchitecture { get; private set; }

        /// <summary>
        /// Gets the custom.
        /// </summary>
        public string Custom { get; private set; }

        /// <summary>
        /// The lazy fusion properties are fusion properties loaded only as required.
        /// </summary>
        private readonly Lazy<AssemblyFusionProperties> _lazyFusionProperties;

        /// <summary>
        /// The lazy reflection properties are properties loaded only as needed via reflection.
        /// </summary>
        private readonly Lazy<AssemblyReflectionProperties> _lazyReflectionProperties;

        /// <summary>
        /// Gets the fusion properties.
        /// </summary>
        public AssemblyFusionProperties FusionProperties
        {
            get { return _lazyFusionProperties.Value; }
        }

        /// <summary>
        /// Gets the reflection properties.
        /// </summary>
        public AssemblyReflectionProperties ReflectionProperties
        {
            get { return _lazyReflectionProperties.Value; }
        }
    }
}
