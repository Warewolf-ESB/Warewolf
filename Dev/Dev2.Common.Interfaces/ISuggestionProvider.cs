using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces
{
    public interface ISuggestionProvider
    {
        #region Public Methods

        IEnumerable<string> GetSuggestions(string filter, int caretPosition, bool tokenise, enIntellisensePartType type);

        #endregion Public Methods

        ObservableCollection<string> VariableList { get; set; }
    }
}