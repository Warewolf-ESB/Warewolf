using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Framework;

namespace Dev2.Studio.InterfaceImplementors
{
    public class BlankIntellisenseProvider : IIntellisenseProvider
    {

        #region Class Members

        List<IIntellisenseResult> _intellisenseResults;

        #endregion Class Members

        #region Ctor

        public BlankIntellisenseProvider()
        {
            _intellisenseResults = new List<IIntellisenseResult>();
        }

        #endregion Ctor

        #region Properties

        public bool HandlesResultInsertion { get; set; }

        public bool Optional { get; set; }

        #endregion Properties

        #region Methods

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
            _intellisenseResults = null;
        }

        #endregion Methods
    }
}
