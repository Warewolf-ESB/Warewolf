
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.InterfaceImplementors
{
    public class CompositeIntellisenseProvider : List<IIntellisenseProvider>, IIntellisenseProvider
    {
        #region Constructor
        public CompositeIntellisenseProvider()
        {
            Optional = false;
            IntellisenseProviderType = IntellisenseProviderType.NonDefault;
        }
        #endregion Constructor

        #region Override Methods

        public IntellisenseProviderType IntellisenseProviderType { get; private set; }

        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            throw new NotSupportedException();
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            var results = new List<IntellisenseProviderResult>();

            foreach(IIntellisenseProvider provider in this.OrderBy(a=>a.Optional))
            {
                if(provider.Optional)
                {
                    if(results.All(r => r.IsError) || context.DesiredResultSet == IntellisenseDesiredResultSet.EntireSet)
                    {
                        IList<IntellisenseProviderResult> subset = provider.GetIntellisenseResults(context);
                        results.AddRange(subset);
                    }
                }
                else
                {
                    var inputText = context.InputText;
                    var caretPosition = context.CaretPosition;

                    if((!string.IsNullOrEmpty(inputText) && (inputText.EndsWith("]") || inputText.EndsWith(")")))
                        && (provider.IntellisenseProviderType == IntellisenseProviderType.Default)
                        && caretPosition == inputText.Length)
                    {
                       return results;
                    }

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
