using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.InterfaceImplementors
{
    public class BlankIntellisenseProvider : IIntellisenseProvider
    {
        #region Properties

        public bool HandlesResultInsertion { get; set; }

        public bool Optional { get; set; }

        #endregion Properties

        #region Methods

        public IntellisenseProviderType IntellisenseProviderType { get; private set; }

        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            return string.Empty;
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            return new List<IntellisenseProviderResult>();
        }

        public void Dispose()
        {
        }

        #endregion Methods
    }
}
