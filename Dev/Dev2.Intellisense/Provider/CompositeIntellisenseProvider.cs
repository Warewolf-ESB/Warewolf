using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces;


namespace Dev2.Studio.InterfaceImplementors
{
    public class CompositeIntellisenseProvider : List<IIntellisenseProvider>, IIntellisenseProvider
    {
        #region Constructor
        public CompositeIntellisenseProvider()
        {
            Optional = false;
        }
        #endregion Constructor

        #region Override Methods
        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            throw new NotSupportedException();
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            List<IntellisenseProviderResult> results = new List<IntellisenseProviderResult>();

            foreach(IIntellisenseProvider provider in this)
            {
                if(provider.Optional)
                {
                    if(results.Where(r => r.IsError == false).Count() == 0 || context.DesiredResultSet == IntellisenseDesiredResultSet.EntireSet)
                    {
                        IList<IntellisenseProviderResult> subset = provider.GetIntellisenseResults(context);
                        results.AddRange(subset);
                    }
                }
                else
                {
                    IList<IntellisenseProviderResult> subset = provider.GetIntellisenseResults(context);
                    results.AddRange(subset);
                }
            }

            return results;
        }

        public void Dispose()
        {
            for(int i = 0; i < Count; i++) this[i].Dispose();
            Clear();
        }

        #endregion Override Methods

        #region Properties

        public bool Optional { get; set; }
        public bool HandlesResultInsertion { get; set; }

        #endregion Properties
    }
}
