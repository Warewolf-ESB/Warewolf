// -----------------------------------------------------------------------
// <copyright file="IIntellisenseProvider.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Dev2.Studio.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Dev2.DataList.Contract;
    using Unlimited.Framework;
    

    /// <summary>
    /// The intellisense provider gives is used to handle intellisense result requests.
    /// </summary>
    public interface IIntellisenseProvider : IDisposable
    {
        bool HandlesResultInsertion { get; set; }
        bool Optional { get; set; }

        string PerformResultInsertion(string input, IntellisenseProviderContext context);
        IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context);
    }

    public sealed class IntellisenseProviderResult
    {
        private IIntellisenseProvider _provider;
        private string _name;
        private string _description;
        private string _dropdownDescription;
        private bool _isError;
        private bool _isPopup;
        private int _startIndex;
        private int _endIndex;
        private string[] _arguments;
        private string[] _argumentDescriptions;

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

            if (_arguments != null && _arguments.Length != 0)
            {
                List<string> args = new List<string>(_arguments);
                for (int i = args.Count - 1; i >= 0; i--)
                    if (String.IsNullOrEmpty(args[i]))
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
        private string _inputText;
        private string _textOnPopup;
        private int _caretPositionOnPopup;
        private int _caretPosition;
        private enIntellisensePartType _filterType;
        private IntellisenseDesiredResultSet _desiredResultSet;
        private object _state;
        private bool _isInCalculateMode;
        private object _textBox;
        public string InputText { get { return _inputText; } set { _inputText = value; }}
        public string TextOnPopup { get { return _textOnPopup; } set { _textOnPopup = value; } }
        public int CaretPosition { get { return _caretPosition; } set { _caretPosition = value; } }
        public int CaretPositionOnPopup { get { return _caretPositionOnPopup; } set { _caretPositionOnPopup = value; } }
        public enIntellisensePartType FilterType { get { return _filterType; } set { _filterType = value; } }
        public IntellisenseDesiredResultSet DesiredResultSet { get { return _desiredResultSet; } set { _desiredResultSet = value; } }
        public object State { get { return _state; } set { _state = value; } }
        public bool IsInCalculateMode { get { return _isInCalculateMode; } set { _isInCalculateMode = value; } }
        public object TextBox { get { return _textBox; } set { _textBox = value; } }
    }

    public enum IntellisenseDesiredResultSet
    {
        Default = 0,
        EntireSet = 1,
        ClosestMatch = 2
    }
}
