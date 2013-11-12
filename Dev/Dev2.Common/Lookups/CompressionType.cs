
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Common.Lookups
{
    public class CompressionType
    {
        public string DisplayName { get; private set; }
        public string CompressionRatio { get; private set; }
        public string Name { get; private set; }

        public CompressionType(string name, string compressionRatio)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "Cannot be null or empty");
            }

            if (string.IsNullOrEmpty(compressionRatio))
            {
                throw new ArgumentNullException("compressionRatio", "Cannot be null or empty");
            }

            CompressionRatio = compressionRatio;
            Name = name;
            DisplayName = string.Format("{0} ({1})", name, compressionRatio);
        }

        public static List<CompressionType> GetTypes()
        {
            return new List<CompressionType>
                {
                    new CompressionType("None", "No Compression"),
                    new CompressionType("Partial", "Best Speed"),
                    new CompressionType("Normal", "Default"),
                    new CompressionType("Max", "Best Compression")
                };
        }

        public static string GetName(string compressionRatio)
        {
            if (string.IsNullOrEmpty(compressionRatio))
            {
                return string.Empty;
            }

            CompressionType compressionType = GetTypes().SingleOrDefault(
                t => t.CompressionRatio.Replace(" ", "").ToLower()
                    .Equals(compressionRatio.Replace(" ", "").ToLower()));
            return compressionType == null ? string.Empty : compressionType.Name;
        }
    }
}
