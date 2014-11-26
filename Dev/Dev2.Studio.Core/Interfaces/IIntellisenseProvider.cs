
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// -----------------------------------------------------------------------
// <copyright file="IIntellisenseProvider.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable once CheckNamespace

using System;
using System.Collections.Generic;
using Dev2.DataList.Contract;

namespace Dev2.Studio.Core.Interfaces
{
    /// <summary>
    /// The intellisense provider gives is used to handle intellisense result requests.
    /// </summary>
    public interface IIntellisenseProvider : IDisposable
    {
        bool HandlesResultInsertion { get; set; }
        bool Optional { get; set; }
        IntellisenseProviderType IntellisenseProviderType { get; }
        string PerformResultInsertion(string input, IntellisenseProviderContext context);
        IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context);
    }

    public enum IntellisenseProviderType
    {
        NonDefault,
        Default
        
    }

    public sealed class IntellisenseProviderResult
    {
        private readonly IIntellisenseProvider _provider;
        private readonly string _name;
        private readonly string _description;
        private readonly string _dropdownDescription;
        private readonly bool _isError;
        private readonly bool _isPopup;
        private readonly int _startIndex;
        private readonly int _endIndex;
        private readonly string[] _arguments;
        private readonly string[] _argumentDescriptions;

        public IIntellisenseProvider Provider { get { return _provider; } }
        public string Name { get { return _name; } }
        public string Description { get { return _description; } }
        public string DropdownDescription { get { return _dropdownDescription; } }
        public string[] Arguments { get { return _arguments; } }
        public string[] ArgumentDescriptions { get { return _argumentDescriptions; } }
        public int StartIndex { get { return _startIndex; } }
        public int EndIndex { get { return _endIndex; } }
        public bool IsError { get { return _isError; } }
        public bool IsPopup { get { return _isPopup; } }

        public IntellisenseProviderResult(IIntellisenseProvider provider, string name, string dropdownDescription, string description, string[] arguments, string[] argumentDescriptions)
        {
            _provider = provider;
            _name = name;
            _dropdownDescription = dropdownDescription;
            _description = description;
            _arguments = arguments;
            _argumentDescriptions = argumentDescriptions;

            if(_arguments != null && _arguments.Length != 0)
            {
                List<string> args = new List<string>(_arguments);
                for(int i = args.Count - 1; i >= 0; i--)
                    if(String.IsNullOrEmpty(args[i]))
                        args.RemoveAt(i);

                _arguments = args.ToArray();
            }
        }

        public IntellisenseProviderResult(IIntellisenseProvider provider, string name, string dropdownDescription, string description, bool isError)
        {
            _provider = provider;
            _name = name;
            _dropdownDescription = dropdownDescription;
            _description = description;
            _isError = isError;
        }

        public IntellisenseProviderResult(IIntellisenseProvider provider, string name, string description)
        {
            _provider = provider;
            _name = name;
            _description = description;
            _isPopup = true;
        }

        public IntellisenseProviderResult(IIntellisenseProvider provider, string name, string dropdownDescription, string description, bool isError, int startIndex, int endIndex)
        {
            _provider = provider;
            _name = name;
            _dropdownDescription = dropdownDescription;
            _description = description;
            _isError = isError;
            _startIndex = startIndex;
            _endIndex = endIndex;
        }

        public override string ToString()
        {
            return _name;
        }
    }

    public sealed class IntellisenseProviderContext
    {
        public string InputText { get; set; }
        public string TextOnPopup { get; set; }
        public int CaretPosition { get; set; }
        public int CaretPositionOnPopup { get; set; }
        public enIntellisensePartType FilterType { get; set; }
        public IntellisenseDesiredResultSet DesiredResultSet { get; set; }
        public object State { get; set; }
        public bool IsInCalculateMode { get; set; }
        public object TextBox { get; set; }
    }

    public enum IntellisenseDesiredResultSet
    {
        Default = 0,
        EntireSet = 1,
        ClosestMatch = 2
    }
}
