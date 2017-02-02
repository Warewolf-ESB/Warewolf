/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dev2.Intellisense;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;


// ReSharper disable CheckNamespace
namespace Dev2.Studio.InterfaceImplementors
// ReSharper restore CheckNamespace
{

    public class DefaultIntellisenseProvider : DependencyObject, IIntellisenseProvider
    {
        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        public DefaultIntellisenseProvider()
        {
            HandlesResultInsertion = true;
            IntellisenseProviderType = IntellisenseProviderType.Default;
            EventPublishers.Aggregator.Subscribe(this);
        }
        #endregion

        #region Implementation of IIntellisenseProvider

        public bool HandlesResultInsertion { get; set; }
        public bool Optional { get; set; }
        public IntellisenseProviderType IntellisenseProviderType { get; private set; }

        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            VerifyArgument.IsNotNull("context", context);
            if (input == String.Empty)
            {
                return context.InputText;
            }
            var resbuilder = new IntellisenseStringResultBuilder();
            var editorText = context.InputText; 
            var originalText = context.InputText;
            var originalCaret = context.CaretPosition;
            if (originalText == String.Empty || input.StartsWith(originalText))
            {
                var result = resbuilder.Build(input, originalCaret, originalText, editorText);
                context.CaretPosition = result.CaretPosition;
                return result.Result;
            }
            // ReSharper disable once RedundantIfElseBlock
            else
            {


                var result = resbuilder.Build(input, originalCaret, originalText, editorText);
                context.CaretPosition = result.CaretPosition;
                return result.Result;
            }
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            if (context == null) return new List<IntellisenseProviderResult>();
                return DataListSingleton.ActiveDataList.Provider.GetSuggestions(context.InputText, context.CaretPosition, true, context.FilterType).Select(a => new IntellisenseProviderResult(this, a, String.Empty)).ToList();
            
        }

        #endregion

    }
}
