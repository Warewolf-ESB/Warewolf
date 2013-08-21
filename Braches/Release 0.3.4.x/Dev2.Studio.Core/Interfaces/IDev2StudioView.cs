using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core;

namespace Dev2.Studio.Core.Interfaces {
    public interface IDev2StudioView
    {
        void ShowDialog(IPropertyEditorWizard editor);
        void Close();
    }
}
