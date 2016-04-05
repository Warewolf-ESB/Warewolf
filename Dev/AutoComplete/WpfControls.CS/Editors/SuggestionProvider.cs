using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;

namespace WpfControls.Editors
{


    public class SuggestionProvider : ISuggestionProvider
    {


        #region Private Fields

        private Func<string,int,bool, IEnumerable<string>> _method;

        #endregion Private Fields

        #region Public Constructors

        public SuggestionProvider(Func<string,int,bool, IEnumerable<string>> method,enIntellisensePartType type)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            _method = method;
        }

        #endregion Public Constructors

        #region Public Methods

        public IEnumerable<string> GetSuggestions(string filter, int a , bool b)
        {
            return _method(filter,a,b);
        }

        #endregion Public Methods

        #region Implementation of ISuggestionProvider

        public IEnumerable<string> GetSuggestions(string filter, int caretPosition, bool tokenise, enIntellisensePartType type)
        {
            yield break;
        }

        public ObservableCollection<string> VariableList { get; set; }

        #endregion
    }
}
