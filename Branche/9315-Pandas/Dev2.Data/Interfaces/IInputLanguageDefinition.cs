using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IInputLanguageDefinition {

        #region Properties
        string Name { get; }

        string MapsTo { get; }

        string StartTagSearch { get; }

        string EndTagSearch { get; }

        string StartTagReplace { get; }

        string EndTagReplace { get; }

        bool IsEvaluated { get; }
        #endregion
    }
}
