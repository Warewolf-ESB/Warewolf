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
using Dev2.Common.Interfaces;

namespace Dev2.Studio.Interfaces
{
    public interface IIntellisenseProvider : IDisposable
    {
        bool HandlesResultInsertion { get; }
        bool Optional { get; }
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
        readonly IIntellisenseProvider _provider;
        readonly string _name;
        readonly string _description;
        readonly string _dropdownDescription;
        readonly bool _isError;
        readonly bool _isPopup;
        readonly int _startIndex;
        readonly int _endIndex;
        readonly string[] _arguments;
        readonly string[] _argumentDescriptions;

        public IIntellisenseProvider Provider => _provider;
        public string Name => _name;
        public string Description => _description;
        public string DropdownDescription => String.Empty;
        public string[] Arguments => _arguments;
        public string[] ArgumentDescriptions => _argumentDescriptions;
        public int StartIndex => _startIndex;
        public int EndIndex => _endIndex;
        public bool IsError => _isError;
        public bool IsPopup => _isPopup;

        public IntellisenseProviderResult(IIntellisenseProvider provider, string name, string dropdownDescription, string description, string[] arguments, string[] argumentDescriptions)
        {
            _provider = provider;
            _name = name;
            _dropdownDescription = dropdownDescription;
            _description = description;
            _arguments = arguments;
            _argumentDescriptions = argumentDescriptions;

            if (_arguments != null && _arguments.Length != 0)
            {
                var args = new List<string>(_arguments);
                for (int i = args.Count - 1; i >= 0; i--)
                {
                    if (String.IsNullOrEmpty(args[i]))
                    {
                        args.RemoveAt(i);
                    }
                }

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
            _isPopup = false;
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
        public int CaretPosition { get; set; }
        public int CaretPositionOnPopup { get; set; }
        public enIntellisensePartType FilterType { get; set; }
        public bool IsInCalculateMode { get; set; }
        public IntellisenseDesiredResultSet DesiredResultSet { get; set; }
    }

    public enum IntellisenseDesiredResultSet
    {
        Default = 0,
        EntireSet = 1,
        ClosestMatch = 2
    }
}
