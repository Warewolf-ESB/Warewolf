#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

        public override string ToString() => _name;
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
